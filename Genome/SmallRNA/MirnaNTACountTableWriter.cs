using CQS.Genome.Feature;
using CQS.Genome.Mirna;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class MirnaNTACountTableWriter : SmallRNACountTableWriter
  {
    private static string GetIsomiRKey(long offset)
    {
      return string.Format("_+_{0}", offset);
    }

    private static string GetNTAKey(string nta)
    {
      return string.Format("_NTA_{0}", nta);
    }

    private static string GetNTAIsomiRKey(string nta, long offset)
    {
      return GetIsomiRKey(offset) + GetNTAKey(nta);
    }

    private static string GetSampleKey(string sample, string prefix)
    {
      return prefix + sample;
    }

    public override IEnumerable<string> WriteToFile(string outputFile, List<FeatureItemGroup> features, List<string> samples, string removeNamePrefix)
    {
      var header = "Feature\tLocation\tSequence\t" + samples.Merge("\t");

      var dicList = new List<Dictionary<string, double>>();
      var ntaList = new List<string[]>();
      var isomiRList = new List<string[]>();
      var ntaIsomiRList = new List<string[]>();
      foreach (var featureGroup in features)
      {
        var dic = new Dictionary<string, double>();
        var ntas = new HashSet<string>();
        var isomiRs = new HashSet<string>();
        var ntaIsomiRs = new HashSet<string>();
        foreach (var feature in featureGroup)
        {
          foreach (var featureLoc in feature.Locations)
          {
            foreach (var samLoc in featureLoc.SamLocations)
            {
              var ntaKey = GetNTAKey(samLoc.SamLocation.Parent.ClippedNTA);
              var isomiRkey = GetIsomiRKey(samLoc.Offset);
              var ntaIsomiRKey = GetNTAIsomiRKey(samLoc.SamLocation.Parent.ClippedNTA, samLoc.Offset);

              ntas.Add(ntaKey);
              isomiRs.Add(isomiRkey);
              ntaIsomiRs.Add(ntaIsomiRKey);

              var samCount = samLoc.SamLocation.Parent.GetEstimatedCount();

              string sampleKey = samLoc.SamLocation.Parent.Sample;
              AddCount(dic, sampleKey, samCount);

              var sampleNTAKey = GetSampleKey(samLoc.SamLocation.Parent.Sample, ntaKey);
              AddCount(dic, sampleNTAKey, samCount);

              string sampleIsomiRKey = GetSampleKey(samLoc.SamLocation.Parent.Sample, isomiRkey);
              AddCount(dic, sampleIsomiRKey, samCount);

              var sampleNTAIsomiRKey = GetSampleKey(samLoc.SamLocation.Parent.Sample, ntaIsomiRKey);
              AddCount(dic, sampleNTAIsomiRKey, samCount);
            }
          }
        }

        dicList.Add(dic);
        ntaList.Add(ntas.OrderBy(m => m).ToArray());
        isomiRList.Add(isomiRs.OrderBy(m => m).ToArray());
        ntaIsomiRList.Add(ntaIsomiRs.OrderBy(m => m).ToArray());
      }

      var ntaFile = Path.ChangeExtension(outputFile, ".NTA.count");
      var isomiRFile = Path.ChangeExtension(outputFile, ".isomiR.count");
      var ntaIsomiRFile = Path.ChangeExtension(outputFile, ".isomiR_NTA.count");
      using (var sw = new StreamWriter(outputFile))
      using (var swNTA = new StreamWriter(ntaFile))
      using (var swIsomiR = new StreamWriter(isomiRFile))
      using (var swNTAIsomiR = new StreamWriter(ntaIsomiRFile))
      {
        sw.WriteLine(header);
        swNTA.WriteLine(header);
        swIsomiR.WriteLine(header);
        swNTAIsomiR.WriteLine(header);

        for (int i = 0; i < features.Count; i++)
        {
          var feature = features[i];
          var dic = dicList[i];
          var ntas = ntaList[i];
          var isomiRs = isomiRList[i];
          var ntaIsomiRs = ntaIsomiRList[i];

          var featureName = (from f in feature select f.Name.StringAfter(removeNamePrefix)).Merge(";");
          var featureSequences = (from l in feature select l.Sequence).ToArray();
          var sequence = featureSequences.Distinct().Count() == 1 ? featureSequences.First() : featureSequences.Merge(";");
          var featureLocations = feature.DisplayLocations;

          WriteCounts(samples, sw, dic, new[] { string.Empty }, featureName, sequence, featureLocations);
          WriteCounts(samples, swNTA, dic, ntas, featureName, sequence, featureLocations);
          WriteCounts(samples, swIsomiR, dic, isomiRs, featureName, sequence, featureLocations);
          WriteCounts(samples, swNTAIsomiR, dic, ntaIsomiRs, featureName, sequence, featureLocations);
        }
      }
      return new[] { outputFile, ntaFile, isomiRFile, ntaIsomiRFile };
    }

    private static void WriteCounts(List<string> samples, StreamWriter swNTA, Dictionary<string, double> dic, string[] ntas, string featureName, string sequence, string featureLocations)
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

    private static void WriteCount(StreamWriter sw, Dictionary<string, double> dic, string key)
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

    private static void AddCount(Dictionary<string, double> dic, string key, double samCount)
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
