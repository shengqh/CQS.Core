using System.Collections.Generic;
using System.IO;
using System.Linq;
using RCPA;

namespace CQS.Genome.Feature
{
  public class FeatureItemGroupCountWriter : IFileWriter<List<FeatureItemGroup>>
  {
    public void WriteToFile(string fileName, List<FeatureItemGroup> groups)
    {
      if (groups.Any(m => !string.IsNullOrEmpty(m.First().Sequence)))
      {
        using (var sw = new StreamWriter(fileName))
        {
          sw.WriteLine("Object\tLocation\tSequence\tEstimateCount\tQueryCount");

          foreach (var g in groups)
          {
            var queryCount = g.QueryCount;
            var estimateCount = g.Sum(m => m.GetEstimatedCount());

            sw.WriteLine("{0}\t{1}\t{2}\t{3:0.##}\t{4}",
              g.DisplayName,
              g.DisplayLocations,
              g.DisplaySequence,
              estimateCount,
              queryCount);
          }
        }
      }
      else
      {
        using (var sw = new StreamWriter(fileName))
        {
          sw.WriteLine("Object\tLocation\tEstimateCount\tQueryCount");

          foreach (var g in groups)
          {
            var queryCount = g.QueryCount;
            var estimateCount = g.Sum(m => m.GetEstimatedCount());

            sw.WriteLine("{0}\t{1}\t{2:0.##}\t{3}",
              g.DisplayName,
              g.DisplayLocations,
              estimateCount,
              queryCount);
          }
        }
      }
    }
  }
}