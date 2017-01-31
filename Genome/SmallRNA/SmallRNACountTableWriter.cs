using CQS.Genome.Feature;
using CQS.Genome.Mirna;
using RCPA.Gui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RCPA;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountTableWriter : ProgressClass
  {
    public virtual IEnumerable<string> WriteToFile(string outputFile, List<FeatureItemGroup> features, List<string> samples, string removeNamePrefix)
    {
      using (StreamWriter sw = new StreamWriter(outputFile))
      {
        //sw.NewLine = Environment.NewLine;
        sw.WriteLine("Feature\tLocation\tSequence\t{0}", samples.Merge("\t"));
        foreach (var feature in features)
        {
          OutputCount(sw, feature, samples, MirnaConsts.NO_OFFSET, false, "", removeNamePrefix);
        }
      }

      Progress.SetMessage("Writing read count file ...");
      string readFile = WriteReadCountTable(outputFile, features, samples);

      return new[] { outputFile, readFile };
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
        var counts = (from sample in samples
                      select feature.GetEstimatedCount(m => m.SamLocation.Parent.Sample.Equals(sample) && acceptOffset(m))).ToArray();
        if (counts.Any(l => l >= 0.05))
        {
          sw.WriteLine("{0}{1}\t{2}\t{3}\t{4}", featureName, indexSuffix, feature.DisplayLocations, sequence,
            (from count in counts select string.Format("{0:0.#}", count)).Merge("\t"));
        }
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
          var counts = (from sample in samples
                        select feature.GetEstimatedCount(m => m.SamLocation.Parent.Sample.Equals(sample) && acceptOffset(m) && m.SamLocation.Parent.ClippedNTA.Equals(nta))).ToArray();
          if (counts.Any(l => l >= 0.05))
          {
            sw.Write("{0}{1}_NTA_{2}\t{3}\t{4}\t{5}", featureName, indexSuffix, nta, feature.DisplayLocations, sequence,
              (from count in counts select string.Format("{0:0.#}", count)).Merge("\t"));
          }
        }
      }
    }

    protected static string WriteReadCountTable(string outputFile, List<FeatureItemGroup> features, List<string> samples)
    {
      var readFile = Path.ChangeExtension(outputFile, ".read.count");
      using (var sw = new StreamWriter(readFile))
      {
        var reads = (from feature in features
                     from a in feature.GetAlignedLocations()
                     select a.Parent).Distinct().ToGroupDictionary(l => l.Sequence);

        var sequences = (from read in reads
                         orderby read.Value.Sum(l => l.QueryCount) descending
                         select read.Key).ToArray();

        sw.WriteLine("Sequence\t" + samples.Merge("\t"));
        foreach (var seq in sequences)
        {
          sw.Write(seq);
          var dic = reads[seq];
          foreach (var sample in samples)
          {
            var sampleSam = dic.Where(l => l.Sample.Equals(sample)).ToList();
            if (sampleSam.Count == 0)
            {
              sw.Write("\t0");
            }
            else
            {
              sw.Write("\t" + sampleSam.Sum(l => l.QueryCount).ToString());
            }
          }
          sw.WriteLine();
        }
      }

      return readFile;
    }
  }
}
