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
using CQS.Genome.SmallRNA;

namespace CQS.Genome.Mapping
{
  public abstract class AbstractCountProcessor<T> : AbstractThreadProcessor where T : ICountProcessorOptions
  {
    protected T options;

    private CountMap _counts;
    public CountMap Counts
    {
      get
      {
        if (_counts == null)
        {
          Progress.SetMessage("Reading count map ...");
          _counts = options.GetCountMap();
          Progress.SetMessage("Reading count map done ...");
        }
        return _counts;
      }
    }

    protected AbstractCountProcessor(T options)
    {
      this.options = options;
    }

    protected virtual List<SAMAlignedItem> ParseCandidates(string fileName, string outputFile, out HashSet<string> queryNames)
    {
      return ParseCandidates(new string[] { fileName }.ToList(), outputFile, out queryNames);
    }

    protected virtual List<SAMAlignedItem> ParseCandidates(IList<string> fileNames, string outputFile, out HashSet<string> queryNames)
    {
      var candiateBuilder = GetCandidateBuilder();

      queryNames = new HashSet<string>();

      var result = new List<SAMAlignedItem>();
      foreach (var fileName in fileNames)
      {
        Progress.SetMessage("processing file: " + fileName);

        HashSet<string> curQueryNames;
        var curResult = candiateBuilder.Build<SAMAlignedItem>(fileName, out curQueryNames);

        queryNames.UnionWith(curQueryNames);
        result.AddRange(curResult);
      }

      FilterAlignedItems(result);

      FillReadCount(result);

      result.ForEach(m => m.SortLocations());

      return result;
    }

    protected virtual void FillReadCount(List<SAMAlignedItem> result)
    {
      result.ForEach(m =>
      {
        m.QueryCount = Counts.GetCount(m.Qname, m.OriginalQname);
      });
    }

    protected virtual void FilterAlignedItems(List<SAMAlignedItem> result)
    {
      if (result.Any(l => l.Qname.Contains(SmallRNAConsts.NTA_TAG)))
      {
        result.ForEach(l => l.OriginalQname = l.Qname.StringBefore(SmallRNAConsts.NTA_TAG));
      }
    }

    protected virtual ICandidateBuilder GetCandidateBuilder()
    {
      var candiateBuilder = options.GetCandidateBuilder();
      candiateBuilder.Progress = this.Progress;
      return candiateBuilder;
    }
  }
}
