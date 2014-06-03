using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Gtf;
using Bio.IO.SAM;
using CQS.Genome.Bed;
using CQS.Genome.Fastq;
using System.Collections.Concurrent;
using System.Threading;
using RCPA.Seq;

namespace CQS.Genome.Mapping
{
  public abstract class AbstractCountProcessor<T> : AbstractThreadFileProcessor where T : CountProcessorOptions
  {
    private static string MIRNA = "ATGCUN";

    protected T options;

    protected AbstractCountProcessor(T options)
    {
      this.options = options;
    }

    protected List<SAMAlignedItem> ParseCandidates(string fileName, string outputFile, out int totalReadCount, out int mappedReadCount)
    {
      Progress.SetMessage("processing file: " + fileName);

      HashSet<string> totalQueryNames;
      var cm = options.GetCountMap();
      var candiateBuilder = GetCandidateBuilder();

      var result = candiateBuilder.Build<SAMAlignedItem>(fileName, out totalQueryNames);

      result.ForEach(m =>
      {
        m.QueryCount = cm.GetCount(m.Qname);
        m.SortLocations();
      });

      if (options.MatchedFile)
      {
        Progress.SetMessage("output matched query details...");
        new SAMAlignedItemFileFormat().WriteToFile(outputFile + ".matched", result);
      }

      totalReadCount = totalQueryNames.Sum(m => cm.GetCount(m));
      totalQueryNames.Clear();
      Console.WriteLine("total reads = {0}", totalReadCount);

      mappedReadCount = result.Sum(m => m.QueryCount);
      Console.WriteLine("mapped reads = {0}", mappedReadCount);

      return result;
    }

    protected List<GtfItem> GetSequenceRegions(string key)
    {
      //Read sequence regions
      var srItems = SequenceRegionUtils.GetSequenceRegions(options.CoordinateFile, key, options.BedAsGtf);
      srItems.ForEach(m =>
      {
        m.Seqname = m.Seqname.StringAfter("chr");
      });

      //Fill sequence information
      var sr = srItems.FirstOrDefault(m => m.Name.Contains(":"));
      if (sr != null)
      {
        var sequence = sr.Name.StringAfter(":");
        if (sequence.All(m => MIRNA.Contains(m)))
        {
          srItems.ForEach(m => m.Sequence = m.Name.StringAfter(":"));
          srItems.ForEach(m => m.Name = m.Name.StringBefore(":"));
        }
      }

      if (!string.IsNullOrEmpty(options.FastaFile))
      {
        Progress.SetMessage("Filling sequence ...");
        var seqs = SequenceUtils.Read(new FastaFormat(), options.FastaFile).ToDictionary(m => m.Name);
        srItems.ForEach(m =>
        {
          if (seqs.ContainsKey(m.Name))
          {
            m.Sequence = seqs[m.Name].SeqString;
          }
          else
          {
            Console.WriteLine("Missing sequence: " + m.Name);
          }
        });
        seqs.Clear();
      }
      return srItems;
    }

    protected string GetResultFilename(string fileName)
    {
      return string.IsNullOrEmpty(options.OutputFile) ? Path.GetFullPath(fileName) + ".count" : Path.GetFullPath(options.OutputFile);
    }

    protected ICandidateBuilder GetCandidateBuilder()
    {
      var candiateBuilder = options.GetCandidateBuilder();
      candiateBuilder.Progress = this.Progress;
      return candiateBuilder;
    }
  }
}
