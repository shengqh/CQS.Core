using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupFisherResultFileFormat : IFileFormat<List<MpileupFisherResult>>
  {
    public List<MpileupFisherResult> ReadFromFile(string fileName)
    {
      var result = new List<MpileupFisherResult>();

      foreach (var line in File.ReadAllLines(fileName).Skip(1))
      {
        if (!string.IsNullOrWhiteSpace(line))
        {
          var item = ParseString(line, '\t');
          result.Add(item);
        }
      }

      return result;
    }

    public static MpileupFisherResult ParseString(string line, char separator = '_')
    {
      var parts = line.Split(separator);
      var result = new MpileupFisherResult();
      try
      {
        double pvalue;
        bool hasReason = !double.TryParse(parts[parts.Length - 1], out pvalue) && double.TryParse(parts[parts.Length - 2], out pvalue);

        var partStart = parts.Length - (hasReason ? 10 : 9);

        result.Item = new PileupItem()
        {
          SequenceIdentifier = parts.Take(partStart).Merge(separator),
          Position = long.Parse(parts[partStart]),
          Nucleotide = parts[partStart + 1][0]
        };
        result.Group = new FisherExactTestResult()
        {
          SucceedName = parts[partStart + 2],
          FailedName = parts[partStart + 3]
        };
        result.Group.Sample1.Succeed = int.Parse(parts[partStart + 4]);
        result.Group.Sample1.Failed = int.Parse(parts[partStart + 5]);
        result.Group.Sample2.Succeed = int.Parse(parts[partStart + 6]);
        result.Group.Sample2.Failed = int.Parse(parts[partStart + 7]);
        result.Group.PValue = double.Parse(parts[partStart + 8]);
        if (hasReason)
        {
          result.FailedReason = parts[partStart + 9];
        }
        else
        {
          result.FailedReason = string.Empty;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("ParseString error: " + line);
        throw ex;
      }
      return result;
    }

    public static string GetString(MpileupFisherResult mfr, char separator = '_', bool includeFailedReason = true)
    {
      var result = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10:0.0E00}",
        separator,
        mfr.Item.SequenceIdentifier,
        mfr.Item.Position,
        mfr.Item.Nucleotide,
        mfr.Group.SucceedName,
        mfr.Group.FailedName,
        mfr.Group.Sample1.Succeed,
        mfr.Group.Sample1.Failed,
        mfr.Group.Sample2.Succeed,
        mfr.Group.Sample2.Failed,
        mfr.Group.PValue);
      if (includeFailedReason)
      {
        result = result + separator + mfr.FailedReason;
      }
      return result;
    }

    public void WriteToFile(string fileName, List<MpileupFisherResult> items)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("chr\tloc\tref\tmajor_allele\tminor_allele\tnormal_major_count\tnormal_minor_count\ttumor_major_count\ttumor_minor_count\tfisher_group\tfilter");

        foreach (var res in items)
        {
          sw.WriteLine(GetString(res, '\t'));
        }
      }
    }
  }
}
