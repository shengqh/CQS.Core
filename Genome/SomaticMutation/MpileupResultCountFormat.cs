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
    public void WriteToFile(string fileName, MpileupResult t)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Reason\tRemoved candidate\tRetained candidate");

        var _totalCount = t.TotalCount;
        sw.WriteLine("total site\t\t{0}", _totalCount);
        _totalCount -= t.MinimumReadDepthFailed;
        sw.WriteLine("minimum read depth failed\t{0}\t{1}", t.MinimumReadDepthFailed, _totalCount);
        _totalCount -= t.OneEventFailed;
        sw.WriteLine("all same allele\t{0}\t{1}", t.OneEventFailed, _totalCount);
        _totalCount -= t.MinorAlleleDecreasedFailed;
        sw.WriteLine("minor allele decreased\t{0}\t{1}", t.MinorAlleleDecreasedFailed, _totalCount);
        _totalCount -= t.LimitationOfMinorAlleleFailed;
        sw.WriteLine("limitation of minor allele failed\t{0}\t{1}", t.LimitationOfMinorAlleleFailed, _totalCount);
        _totalCount -= t.GroupFisherFailed;
        sw.WriteLine("fisher exact test is not significant\t{0}\t{1}", t.GroupFisherFailed, _totalCount);
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
        result.MinimumReadDepthFailed = ParseCount(sr.ReadLine(), 1);
        result.OneEventFailed = ParseCount(sr.ReadLine(), 1);
        result.MinorAlleleDecreasedFailed = ParseCount(sr.ReadLine(), 1);
        result.LimitationOfMinorAlleleFailed = ParseCount(sr.ReadLine(), 1);
        result.GroupFisherFailed = ParseCount(sr.ReadLine(), 1);
      }

      return result;
    }
  }
}
