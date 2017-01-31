using CQS.Genome.Feature;
using CQS.Genome.Mirna;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RCPA;

namespace CQS.Genome.SmallRNA
{
  public static class NTACountTableUtils
  {
    public static string GetIsomiRKey(long offset)
    {
      return string.Format("_+_{0}", offset);
    }

    public static string GetNTAKey(string nta)
    {
      return string.Format("_NTA_{0}", nta);
    }

    public static string GetNTAIsomiRKey(string nta, long offset)
    {
      return GetIsomiRKey(offset) + GetNTAKey(nta);
    }

    public static string GetSampleKey(string sample, string prefix)
    {
      return prefix + sample;
    }

    public static void WriteCounts(List<string> samples, StreamWriter swNTA, Dictionary<string, double> dic, string[] ntas, string featureName, string sequence, string featureLocations)
    {
      //output nta result
      foreach (var nta in ntas)
      {
        swNTA.Write("{0}{1}\t{2}\t{3}", featureName, nta, featureLocations, sequence);
        foreach (var sample in samples)
        {
          WriteCount(swNTA, dic, GetSampleKey(sample, nta));
        }
        swNTA.WriteLine();
      }
    }

    public static void WriteCount(StreamWriter sw, Dictionary<string, double> dic, string key)
    {
      double count;
      if (dic.TryGetValue(key, out count))
      {
        sw.Write("\t{0:0.#}", count);
      }
      else
      {
        sw.Write("\t0");
      }
    }

    public static void AddCount(Dictionary<string, double> dic, string key, double samCount)
    {
      double count;
      if (dic.TryGetValue(key, out count))
      {
        dic[key] = count + samCount;
      }
      else
      {
        dic[key] = samCount;
      }
    }
  }
}
