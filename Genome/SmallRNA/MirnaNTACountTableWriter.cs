using CQS.Genome.Feature;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class MirnaNTACountTableWriter : SmallRNACountTableWriter
  {
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
              var ntaKey = NTACountTableUtils.GetNTAKey(samLoc.SamLocation.Parent.ClippedNTA);
              var isomiRkey = NTACountTableUtils.GetIsomiRKey(samLoc.Offset);
              var ntaIsomiRKey = NTACountTableUtils.GetNTAIsomiRKey(samLoc.SamLocation.Parent.ClippedNTA, samLoc.Offset);

              ntas.Add(ntaKey);
              isomiRs.Add(isomiRkey);
              ntaIsomiRs.Add(ntaIsomiRKey);

              var samCount = samLoc.SamLocation.Parent.GetEstimatedCount();

              string sampleKey = samLoc.SamLocation.Parent.Sample;
              NTACountTableUtils.AddCount(dic, sampleKey, samCount);

              var sampleNTAKey = NTACountTableUtils.GetSampleKey(samLoc.SamLocation.Parent.Sample, ntaKey);
              NTACountTableUtils.AddCount(dic, sampleNTAKey, samCount);

              string sampleIsomiRKey = NTACountTableUtils.GetSampleKey(samLoc.SamLocation.Parent.Sample, isomiRkey);
              NTACountTableUtils.AddCount(dic, sampleIsomiRKey, samCount);

              var sampleNTAIsomiRKey = NTACountTableUtils.GetSampleKey(samLoc.SamLocation.Parent.Sample, ntaIsomiRKey);
              NTACountTableUtils.AddCount(dic, sampleNTAIsomiRKey, samCount);
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

          NTACountTableUtils.WriteCounts(samples, sw, dic, new[] { string.Empty }, featureName, sequence, featureLocations);
          NTACountTableUtils.WriteCounts(samples, swNTA, dic, ntas, featureName, sequence, featureLocations);
          NTACountTableUtils.WriteCounts(samples, swIsomiR, dic, isomiRs, featureName, sequence, featureLocations);
          NTACountTableUtils.WriteCounts(samples, swNTAIsomiR, dic, ntaIsomiRs, featureName, sequence, featureLocations);
        }
      }

      string readFile = WriteReadCountTable(outputFile, features, samples);

      return new[] { outputFile, ntaFile, isomiRFile, ntaIsomiRFile, readFile };
    }
  }
}
