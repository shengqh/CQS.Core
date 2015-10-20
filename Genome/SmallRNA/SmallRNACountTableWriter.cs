using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Gtf;
using Bio.IO.SAM;
using CQS.Genome.Bed;
using CQS.Genome.Fastq;
using System.Collections.Concurrent;
using System.Threading;
using RCPA.Commandline;
using CommandLine;
using System.Text.RegularExpressions;
using CQS.Genome.Mapping;
using CQS.Genome.Feature;
using CQS.Genome.Mirna;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountTableWriter
  {
    public IEnumerable<string> WriteToFile(string outputFile, Dictionary<string, Dictionary<string, FeatureItemGroup>> dic, string removeNamePrefix)
    {
      //sample names
      var samples = GetSamples(dic);

      var orderedFeatures = GetOrderedFeatures(dic);

      List<string> result = DoWriteToFile(outputFile, dic, samples, orderedFeatures, removeNamePrefix);

      return result;
    }

    protected virtual List<string> DoWriteToFile(string outputFile, Dictionary<string, Dictionary<string, FeatureItemGroup>> dic, List<string> samples, List<string> orderedFeatures, string removeNamePrefix)
    {
      List<string> result = new List<string>();
      using (StreamWriter sw = new StreamWriter(outputFile))
      {
        sw.WriteLine("Feature\tLocation\tSequence\t{0}", samples.Merge("\t"));
        foreach (var feature in orderedFeatures)
        {
          OutputCount(sw, dic, feature, samples, MirnaConsts.NO_OFFSET, false, "", removeNamePrefix);
        }
      }
      result.Add(outputFile);
      return result;
    }

    protected List<string> GetSamples(Dictionary<string, Dictionary<string, FeatureItemGroup>> dic)
    {
      return dic.Keys.OrderBy(m => m).ToList();
    }

    protected List<string> GetOrderedFeatures(Dictionary<string, Dictionary<string, FeatureItemGroup>> dic)
    {
      //miRNA names
      var features = (from c in dic.Values
                      from k in c.Keys
                      select k).Distinct().OrderBy(m => m).ToList();

      var orderedFeatures = (from f in features
                             let count = (from v in dic.Values
                                          where v.ContainsKey(f)
                                          select v[f].EstimateCount).Sum()
                             orderby count descending
                             select f).ToList();
      return orderedFeatures;
    }

    protected void OutputCount(StreamWriter sw, Dictionary<string, Dictionary<string, FeatureItemGroup>> dic, string feature, List<string> names, int offset, bool hasNTA, string indexSuffix, string removeNamePrefix)
    {
      Func<FeatureSamLocation, bool> acceptOffset = m => MirnaConsts.NO_OFFSET == offset || m.Offset == offset;

      var mmg = (from v in dic.Values
                 where v.ContainsKey(feature)
                 let g = v[feature]
                 from m in g
                 from mr in m.Locations
                 where mr.SamLocations.Any(l => acceptOffset(l))
                 select g).FirstOrDefault();

      if (mmg == null)
      {
        return;
      }

      var featureName = (from f in feature.Split(';')
                         select f.StringAfter(removeNamePrefix)).Merge(";");
      if (!hasNTA)
      {
        sw.Write("{0}{1}\t{2}\t{3}", featureName, indexSuffix, mmg.DisplayLocations, mmg[0].Sequence);
        foreach (var name in names)
        {
          var map = dic[name];
          FeatureItemGroup group;
          if (map.TryGetValue(feature, out group))
          {
            sw.Write("\t{0:0.#}", group.GetEstimateCount(acceptOffset));
          }
          else
          {
            sw.Write("\t0");
          }
        }
        sw.WriteLine();
      }
      else
      {
        var ntas = (from v in dic.Values
                    where v.ContainsKey(feature)
                    let g = v[feature]
                    from m in g
                    from mr in m.Locations
                    from l in mr.SamLocations
                    where acceptOffset(l)
                    select l.SamLocation.Parent.ClippedNTA).Distinct().OrderBy(m => m).ToList();

        foreach (var nta in ntas)
        {
          sw.Write("{0}{1}_NTA_{2}\t{3}\t{4}", featureName, indexSuffix, nta, mmg.DisplayLocations, mmg[0].Sequence);
          foreach (var name in names)
          {
            var map = dic[name];
            FeatureItemGroup group;
            if (map.TryGetValue(feature, out group))
            {
              sw.Write("\t{0:0.###}", group.GetEstimateCount(m => m.SamLocation.Parent.ClippedNTA.Equals(nta) && acceptOffset(m)));
            }
            else
            {
              sw.Write("\t0");
            }
          }
          sw.WriteLine();
        }
      }
    }

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
          var count = feature.GetEstimateCount(m => m.SamLocation.Parent.Sample.Equals(sample) && acceptOffset(m));
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
            var count = feature.GetEstimateCount(m => m.SamLocation.Parent.Sample.Equals(sample) && acceptOffset(m) && m.SamLocation.Parent.ClippedNTA.Equals(nta));
            sw.Write("\t{0:0.#}", count);
          }
          sw.WriteLine();
        }
      }
    }
  }
}
