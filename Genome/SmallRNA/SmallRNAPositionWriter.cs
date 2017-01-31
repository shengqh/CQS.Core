using CQS.Genome.Feature;
using RCPA;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAPositionWriter : IFileWriter<List<FeatureItemGroup>>
  {
    private Func<FeatureItemGroup, string> getName;
    private bool positionByPercentage;
    private int minimumReadCount;

    public SmallRNAPositionWriter(Func<FeatureItemGroup, string> getName, bool positionByPercentage = false, int minimumReadCount = 5)
    {
      this.getName = getName;
      this.minimumReadCount = minimumReadCount;
      this.positionByPercentage = positionByPercentage;
    }

    public SmallRNAPositionWriter(bool positionByPercentage = false, int minimumReadCount = 5)
    {
      this.getName = m => m[0].Name.StringAfter(":").StringBefore("|");
      this.minimumReadCount = minimumReadCount;
      this.positionByPercentage = positionByPercentage;
    }

    public void WriteToFile(string fileName, List<FeatureItemGroup> features)
    {
      var groups = minimumReadCount > 0 ? features.Where(l => l.GetEstimatedCount() >= minimumReadCount).ToList() : features;

      var samples = (from f in features from s in f from ss in s.Locations from sss in ss.SamLocations select sss.SamLocation.Parent.Sample).Distinct().OrderBy(l => l).ToList();

      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine("File\tFeature\tStrand\tTotalCount\tPositionCount\tPosition\tPercentage");
        foreach (var sample in samples)
        {
          groups.Sort((m1, m2) => m2.GetEstimatedCount(l => l.SamLocation.Parent.Sample.Equals(sample)).CompareTo(m1.GetEstimatedCount(l => l.SamLocation.Parent.Sample.Equals(sample))));
          foreach (var group in groups)
          {
            Dictionary<long, double> positionCount = new Dictionary<long, double>();
            foreach (var item in group)
            {
              foreach (var region in item.Locations)
              {
                var regionLength = region.Length;
                foreach (var loc in region.SamLocations)
                {
                  if (!loc.SamLocation.Parent.Sample.Equals(sample))
                  {
                    continue;
                  }

                  Dictionary<long, double> curCount = new Dictionary<long, double>();
                  var estimatedCount = loc.SamLocation.Parent.GetEstimatedCount();
                  //Console.WriteLine("{0}: {1}", loc.SamLocation.Parent.Qname, estimatedCount);

                  var offsetStart = region.Strand == '+' ? loc.SamLocation.Start - region.Start : region.End - loc.SamLocation.End;
                  var offsetEnd = region.Strand == '+' ? loc.SamLocation.End - region.Start : region.End - loc.SamLocation.Start;
                  if (positionByPercentage)
                  {
                    offsetStart = offsetStart * 100 / regionLength;
                    offsetEnd = offsetEnd * 100 / regionLength;
                  }

                  double v;
                  for (var offset = offsetStart; offset <= offsetEnd; offset++)
                  {
                    if (!curCount.TryGetValue(offset, out v) || v < estimatedCount)
                    {
                      curCount[offset] = estimatedCount;
                    }
                  }

                  foreach (var cc in curCount)
                  {
                    double vv;
                    if (!positionCount.TryGetValue(cc.Key, out vv))
                    {
                      positionCount[cc.Key] = cc.Value;
                    }
                    else
                    {
                      positionCount[cc.Key] = vv + cc.Value;
                    }
                  }
                }
              }
            }

            var groupName = getName(group);
            var allcount = group.GetEstimatedCount(l => l.SamLocation.Parent.Sample.Equals(sample));
            var keys = positionCount.Keys.ToList();
            keys.Sort();
            foreach (var key in keys)
            {
              var perc = positionCount[key] / allcount;

              if (perc > 0.005)
              {
                sw.WriteLine("{0}\t{1}\t{2}\t{3:0.##}\t{4:0.##}\t{5}\t{6:0.00}",
                  sample,
                  groupName,
                  group[0].Locations[0].Strand,
                  allcount,
                  positionCount[key],
                  key,
                  perc);
              }
            }
          }
        }
      }
    }
  }
}
