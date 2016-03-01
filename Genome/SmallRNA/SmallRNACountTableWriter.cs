using CQS.Genome.Feature;
using CQS.Genome.Mirna;
using RCPA.Gui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountTableWriter : ProgressClass
  {
    public virtual IEnumerable<string> WriteToFile(string outputFile, List<FeatureItemGroup> features, List<string> samples, string removeNamePrefix)
    {
      List<string> result = new List<string>();
      using (StreamWriter sw = new StreamWriter(outputFile))
      {
        //sw.NewLine = Environment.NewLine;
        sw.WriteLine("Feature\tLocation\tSequence\t{0}", samples.Merge("\t"));
        foreach (var feature in features)
        {
          OutputCount(sw, feature, samples, MirnaConsts.NO_OFFSET, false, "", removeNamePrefix);
        }
      }
      result.Add(outputFile);
      return result;
    }

    protected void OutputCount(StreamWriter sw, FeatureItemGroup feature, List<string> samples, int offset, bool hasNTA, string indexSuffix, string removeNamePrefix)
    {
      Func<FeatureSamLocation, bool> acceptOffset = m => MirnaConsts.NO_OFFSET == offset || m.Offset == offset;

      if (!feature.Any(l => l.Locations.Any(k => k.SamLocations.Any(g => acceptOffset(g)))))
      {
        return;
      }

      var featureName = (from f in feature select f.Name.StringAfter(removeNamePrefix)).Merge(";");

      var featureSequences = (from l in feature
                              select l.Sequence).ToArray();

      var sequence = featureSequences.Distinct().Count() == 1 ? featureSequences.First() : featureSequences.Merge(";");

      if (!hasNTA)
      {
        sw.Write("{0}{1}\t{2}\t{3}", featureName, indexSuffix, feature.DisplayLocations, sequence);
        foreach (var sample in samples)
        {
          var count = feature.GetEstimatedCount(m => m.SamLocation.Parent.Sample.Equals(sample) && acceptOffset(m));
          sw.Write("\t{0:0.#}", count);
        }
        sw.WriteLine();
      }
      else
      {
        var ntas = (from m in feature
                    from mr in m.Locations
                    from l in mr.SamLocations
                    where acceptOffset(l)
                    select l.SamLocation.Parent.ClippedNTA).Distinct().OrderBy(m => m).ToList();

        foreach (var nta in ntas)
        {
          sw.Write("{0}{1}_NTA_{2}\t{3}\t{4}", featureName, indexSuffix, nta, feature.DisplayLocations, sequence);
          foreach (var sample in samples)
          {
            var count = feature.GetEstimatedCount(m => m.SamLocation.Parent.Sample.Equals(sample) && acceptOffset(m) && m.SamLocation.Parent.ClippedNTA.Equals(nta));
            sw.Write("\t{0:0.#}", count);
          }
          sw.WriteLine();
        }
      }
    }
  }
}
