using CQS.Genome.Feature;
using CQS.Genome.Mirna;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RCPA;

namespace CQS.Genome.SmallRNA
{
  public class TrnaNTACountTableWriter : SmallRNACountTableWriter
  {
    public override IEnumerable<string> WriteToFile(string outputFile, List<FeatureItemGroup> features, List<string> samples, string removeNamePrefix)
    {
      var header = "Feature\tLocation\tSequence\t" + samples.Merge("\t");

      var dicList = new List<Dictionary<string, double>>();
      var ntaList = new List<string[]>();
      foreach (var featureGroup in features)
      {
        var dic = new Dictionary<string, double>();
        var ntas = new HashSet<string>();
        foreach (var feature in featureGroup)
        {
          foreach (var featureLoc in feature.Locations)
          {
            foreach (var samLoc in featureLoc.SamLocations)
            {
              var ntaKey = NTACountTableUtils.GetNTAKey(samLoc.SamLocation.Parent.ClippedNTA);
              ntas.Add(ntaKey);

              var samCount = samLoc.SamLocation.Parent.GetEstimatedCount();

              string sampleKey = samLoc.SamLocation.Parent.Sample;
              NTACountTableUtils.AddCount(dic, sampleKey, samCount);

              var sampleNTAKey = NTACountTableUtils.GetSampleKey(samLoc.SamLocation.Parent.Sample, ntaKey);
              NTACountTableUtils.AddCount(dic, sampleNTAKey, samCount);
            }
          }
        }

        dicList.Add(dic);
        ntaList.Add(ntas.OrderBy(m => m).ToArray());
      }

      var ntaFile = Path.ChangeExtension(outputFile, ".NTA.count");
      using (var sw = new StreamWriter(outputFile))
      using (var swNTA = new StreamWriter(ntaFile))
      {
        sw.WriteLine(header);
        swNTA.WriteLine(header);

        for (int i = 0; i < features.Count; i++)
        {
          var feature = features[i];
          var dic = dicList[i];
          var ntas = ntaList[i];

          var featureName = (from f in feature select f.Name.StringAfter(removeNamePrefix)).Merge(";");
          var featureSequences = (from l in feature select l.Sequence).ToArray();
          var sequence = featureSequences.Distinct().Count() == 1 ? featureSequences.First() : featureSequences.Merge(";");
          var featureLocations = feature.DisplayLocations;

          NTACountTableUtils.WriteCounts(samples, sw, dic, new[] { string.Empty }, featureName, sequence, featureLocations);
          NTACountTableUtils.WriteCounts(samples, swNTA, dic, ntas, featureName, sequence, featureLocations);
        }
      }

      string readFile = WriteReadCountTable(outputFile, features, samples);

      return new[] { outputFile, ntaFile, readFile };
    }
  }
}
