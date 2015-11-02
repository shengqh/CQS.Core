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

    //4_JH584292_random_13694_T_T_G_49_0_37_6_8.5E-03

    private static Regex GetFileRegex(char separator)
    {
      return new Regex(string.Format(@"(.+){0}(\d+){0}([a-zA-Z]){0}([a-zA-Z]){0}([a-zA-Z]){0}(\d+){0}(\d+){0}(\d+){0}(\d+)([^{0}]+)(.*)", separator));
    }

    private static Dictionary<char, Regex> fileRegMap =  new Dictionary<char,Regex>();

    public static MpileupFisherResult ParseString(string line, char separator = '_')
    {
      Regex reg;
      if (!fileRegMap.TryGetValue(separator, out reg))
      {
        reg = GetFileRegex(separator);
        fileRegMap[separator] = reg;
      }

      var result = new MpileupFisherResult();
      try
      {
        var m = reg.Match(line);
        if (!m.Success)
        {
          throw new Exception(string.Format("Cannot parse fisher result from {0}", line));
        }
        result.Item = new PileupItem()
        {
          SequenceIdentifier = m.Groups[1].Value,
          Position = long.Parse(m.Groups[2].Value),
          Nucleotide = m.Groups[3].Value[0]
        };
        result.Group = new FisherExactTestResult()
        {
          SucceedName = m.Groups[4].Value,
          FailedName = m.Groups[5].Value,
        };
        result.Group.Sample1.Succeed = int.Parse(m.Groups[6].Value);
        result.Group.Sample1.Failed = int.Parse(m.Groups[7].Value);
        result.Group.Sample2.Succeed = int.Parse(m.Groups[8].Value);
        result.Group.Sample2.Failed = int.Parse(m.Groups[9].Value);
        result.Group.PValue = double.Parse(m.Groups[10].Value);
        result.FailedReason = m.Groups[11].Value;
        if (!string.IsNullOrWhiteSpace(result.FailedReason))
        {
          result.FailedReason = result.FailedReason.Substring(1);
          //Console.WriteLine("Failed reason = " + result.FailedReason);
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
