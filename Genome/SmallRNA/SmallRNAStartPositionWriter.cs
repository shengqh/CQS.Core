using CQS.Genome.Feature;
using RCPA;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAStartPositionWriter : IFileWriter<List<FeatureItemGroup>>
  {
    private Func<FeatureItemGroup, string> getName;
    private int minimumReadCount;

    public SmallRNAStartPositionWriter(Func<FeatureItemGroup, string> getName, int minimumReadCount = 5)
    {
      this.getName = getName;
      this.minimumReadCount = minimumReadCount;
    }

    public void WriteToFile(string fileName, List<FeatureItemGroup> features)
    {
      var groups = minimumReadCount > 0 ? features.Where(l => l.GetEstimatedCount() >= minimumReadCount).ToList() : features;

      //Console.WriteLine("{0} groups for output startpos", groups.Count);

      var samples = (from f in features from s in f from ss in s.Locations from sss in ss.SamLocations select sss.SamLocation.Parent.Sample).Distinct().OrderBy(l => l).ToList();

      //Console.WriteLine("{0} samples for output startpos", samples.Count);

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
                foreach (var loc in region.SamLocations)
                {
                  if (!loc.SamLocation.Parent.Sample.Equals(sample))
                  {
                    continue;
                  }

                  var estimatedCount = loc.SamLocation.Parent.GetEstimatedCount();
                  //Console.WriteLine("{0}: {1}", loc.SamLocation.Parent.Qname, estimatedCount);

                  var offset = region.Strand == '+' ? loc.SamLocation.Start - region.Start : region.End - loc.SamLocation.End;

                  double v;
                  if (!positionCount.TryGetValue(offset, out v))
                  {
                    positionCount[offset] = estimatedCount;
                  }
                  else
                  {
                    positionCount[offset] = v + estimatedCount;
                  }
                }
              }
            }

            var groupName = getName(group);
            //Console.WriteLine("{0} => {1}", group.Name, groupName);

            var allcount = group.GetEstimatedCount(l => l.SamLocation.Parent.Sample.Equals(sample));
            var keys = positionCount.Keys.ToList();
            keys.Sort();
            foreach (var key in keys)
            {
              var perc = positionCount[key] / allcount;
              //Console.WriteLine("{0} => {1}, {2} => {3}", group.Name, groupName, key, perc);

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
