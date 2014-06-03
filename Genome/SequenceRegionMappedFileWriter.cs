using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome
{
  public class SequenceRegionMappedFileWriter : IFileWriter<List<SequenceRegionMapped>>
  {
    public SequenceRegionMappedFileWriter()
    { }

    public void WriteToFile(string fileName, List<SequenceRegionMapped> mapped)
    {
      var groups = mapped.GroupBy(m => m.Region.Name).OrderBy(m => m.Key).ToList();

      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Subject\tLocation\tEstimateCount\tQueryCount\tOtherLocations");

        foreach (var g in groups)
        {
          var queryCount = g.Sum(m => m.AlignedLocations.Sum(n => n.Parent.QueryCount));
          var estimateCount = g.Sum(m => m.EstimatedCount);

          var otherlocation = (from vv in g.Skip(1)
                               select vv.Region.GetLocation()).Merge(";");

          sw.WriteLine("{0}\t{1}\t{2}\t{3:0.##}\t{4}",
            g.Key,
            g.First().Region.GetLocation(),
            estimateCount,
            queryCount,
            otherlocation);
        }
      }
    }
  }
}
