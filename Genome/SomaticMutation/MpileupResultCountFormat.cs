using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupResultCountFormat : IFileFormat<MpileupResult>
  {
    private PileupProcessorOptions options;
    private bool outputNotCovered;
    public MpileupResultCountFormat(PileupProcessorOptions options, bool outputNotCovered)
    {
      this.options = options;
      this.outputNotCovered = outputNotCovered;
    }

    public void WriteToFile(string fileName, MpileupResult t)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Reason\tRemoved candidate\tRetained candidate");

        var _totalCount = t.TotalCount;
        sw.WriteLine("total site\t\t{0}", _totalCount);
        if (outputNotCovered) // for validation only
        {
          _totalCount -= t.NotCovered;
          sw.WriteLine("no read covered\t{0}\t{1}", t.NotCovered, _totalCount);
        }
        _totalCount -= t.MinimumReadDepthFailed;
        sw.WriteLine("read depth < {0}\t{1}\t{2}", options.MinimumReadDepth, t.MinimumReadDepthFailed, _totalCount);
        _totalCount -= t.OneEventFailed;
        sw.WriteLine("no alternative allele\t{0}\t{1}", t.OneEventFailed, _totalCount);
        _totalCount -= t.MinorAlleleDecreasedFailed;
        sw.WriteLine("minor allele percentage decreased in tumor sample\t{0}\t{1}", t.MinorAlleleDecreasedFailed, _totalCount);
        _totalCount -= t.MinorAlleleFailedInTumorSample;
        sw.WriteLine("limitation of minor allele failed in tumor sample\t{0}\t{1}", t.MinorAlleleFailedInTumorSample, _totalCount);
        _totalCount -= t.MinorAlleleFailedInNormalSample;
        sw.WriteLine("limitation of minor allele failed in normal sample\t{0}\t{1}", t.MinorAlleleFailedInNormalSample, _totalCount);
        _totalCount -= t.GroupFisherFailed;
        sw.WriteLine("fisher exact test pvalue > {0:0.####}\t{1}\t{2}", options.FisherPvalue, t.GroupFisherFailed, _totalCount);
      }
    }

    private static int ParseCount(string line, int pos)
    {
      var parts = line.Split('\t');
      return int.Parse(parts[pos]);
    }

    public MpileupResult ReadFromFile(string fileName)
    {
      var result = new MpileupResult(string.Empty, string.Empty);

      using (var sr = new StreamReader(fileName))
      {
        var line = sr.ReadLine();
        result.TotalCount = ParseCount(sr.ReadLine(), 2);
        line = sr.ReadLine();
        if (line.StartsWith("no read covered"))
        {
          result.NotCovered = ParseCount(line, 1);
          result.MinimumReadDepthFailed = ParseCount(sr.ReadLine(), 1);
        }
        else
        {
          result.MinimumReadDepthFailed = ParseCount(line, 1);
        }
        result.OneEventFailed = ParseCount(sr.ReadLine(), 1);
        result.MinorAlleleDecreasedFailed = ParseCount(sr.ReadLine(), 1);
        result.MinorAlleleFailedInTumorSample = ParseCount(sr.ReadLine(), 1);
        result.MinorAlleleFailedInNormalSample = ParseCount(sr.ReadLine(), 1);
        result.GroupFisherFailed = ParseCount(sr.ReadLine(), 1);
      }

      return result;
    }
  }
}
