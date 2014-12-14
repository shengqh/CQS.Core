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
using CQS.Genome.Mirna;

namespace CQS.Genome.Mapping
{
  public abstract class AbstractCountProcessor<T> : AbstractThreadProcessor where T : CountProcessorOptions
  {
    protected T options;

    protected AbstractCountProcessor(T options)
    {
      this.options = options;
    }

    protected virtual List<SAMAlignedItem> ParseCandidates(string fileName, string outputFile, out int totalReadCount, out int mappedReadCount)
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

      if (totalQueryNames.Count > 0 && totalQueryNames.First().Contains(MirnaConsts.NTA_TAG))
      {
        var qnames = (from q in totalQueryNames
                      select q.StringBefore(MirnaConsts.NTA_TAG) + MirnaConsts.NTA_TAG).Distinct();
        totalReadCount = qnames.Sum(m => cm.GetCount(m));
        var rnames = (from r in result
                      select r.Qname.StringBefore(MirnaConsts.NTA_TAG) + MirnaConsts.NTA_TAG).Distinct();

        mappedReadCount = rnames.Sum(m => cm.GetCount(m));
      }
      else
      {
        totalReadCount = totalQueryNames.Sum(m => cm.GetCount(m));
        totalQueryNames.Clear();
        mappedReadCount = result.Sum(m => m.QueryCount);
      }
      Console.WriteLine("total reads = {0}", totalReadCount);
      Console.WriteLine("mapped reads = {0}", mappedReadCount);

      return result;
    }

    protected string GetResultFilename(string fileName)
    {
      return string.IsNullOrEmpty(options.OutputFile) ? Path.GetFullPath(fileName) + ".count" : Path.GetFullPath(options.OutputFile);
    }

    protected virtual ICandidateBuilder GetCandidateBuilder()
    {
      var candiateBuilder = options.GetCandidateBuilder();
      candiateBuilder.Progress = this.Progress;
      return candiateBuilder;
    }
  }
}
