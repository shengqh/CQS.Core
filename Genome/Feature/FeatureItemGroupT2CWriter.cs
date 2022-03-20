using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Feature
{
  public class FeatureItemGroupT2CWriter : IFileWriter<List<FeatureItemGroup>>
  {
    private double expectRate;
    public FeatureItemGroupT2CWriter(double expectRate)
    {
      this.expectRate = expectRate;
    }

    public void WriteToFile(string fileName, List<FeatureItemGroup> groups)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Class\tName\tLocation\tTotal_count\tT2C_count\tpvalue\tT2C_rate\t%T2C\tFoldChange");
        foreach (var g in groups)
        {
          var rate = g[0].Locations[0].QueryCount * 1.0 / g[0].Locations[0].QueryCountBeforeFilter;
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5:0.##e-0}\t{6:0.####}\t{7:0.##}\t{8:0.##}",
            g[0].Name.StringBefore(":"),
            (from l in g select l.Name).Merge("/"),
            (from l in g select l.DisplayLocations).Merge("/"),
            g[0].Locations[0].QueryCountBeforeFilter,
            g[0].Locations[0].QueryCount,
            g[0].Locations[0].PValue,
            rate,
            rate * 100,
            rate / expectRate);
        }
      }
    }
  }
}
