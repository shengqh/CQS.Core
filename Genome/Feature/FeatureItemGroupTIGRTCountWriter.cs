using System.Collections.Generic;
using System.IO;
using System.Linq;
using RCPA;

namespace CQS.Genome.Feature
{
  public class FeatureItemGroupTIGRTCountWriter : IFileWriter<List<FeatureItemGroup>>
  {
    public void WriteToFile(string fileName, List<FeatureItemGroup> groups)
    {
      if (groups.Any(m => !string.IsNullOrEmpty(m.First().Sequence)))
      {
        using (var sw = new StreamWriter(fileName))
        {
          sw.WriteLine("Object\tLocation\tSequence\tEstimateCount\tQueryCount\tNTARate\tCCRate");

          foreach (var g in groups)
          {
            var queryCount = g.QueryCount;
            var estimateCount = g.Sum(m => m.GetEstimatedCount());
            var ntaCount = GetNTACount(g);
            var ccCount = GetCCCount(g);

            sw.WriteLine("{0}\t{1}\t{2}\t{3:0.##}\t{4}\t{5:0.###}\t{6:0.###}",
              g.DisplayName,
              g.DisplayLocations,
              g.DisplaySequence,
              estimateCount,
              queryCount,
              ntaCount / estimateCount,
              ccCount / estimateCount);
          }
        }
      }
      else
      {
        using (var sw = new StreamWriter(fileName))
        {
          sw.WriteLine("Object\tLocation\tEstimateCount\tQueryCount\tNTARate\tCCRate");

          foreach (var g in groups)
          {
            var queryCount = g.QueryCount;
            var estimateCount = g.Sum(m => m.GetEstimatedCount());
            var ntaCount = GetNTACount(g);
            var ccCount = GetCCCount(g);

            sw.WriteLine("{0}\t{1}\t{2:0.##}\t{3}\t{4:0.###}\t{5:0.###}",
              g.DisplayName,
              g.DisplayLocations,
              estimateCount,
              queryCount,
              ntaCount / estimateCount,
              ccCount / estimateCount);
          }
        }
      }
    }

    private static double GetCCCount(FeatureItemGroup g)
    {
      return g.Sum(m => m.GetEstimatedCount(l => l.SamLocation.Parent.Qname.EndsWith("TRNA_") && l.SamLocation.Parent.Sequence.EndsWith("CC")));
    }

    private static double GetNTACount(FeatureItemGroup g)
    {
      var ntaCount = g.Sum(m => m.GetEstimatedCount(l => l.SamLocation.Parent.Qname.EndsWith("TRNA_CCA") || l.SamLocation.Parent.Qname.EndsWith("TRNA_CCAA")));
      return ntaCount;
    }
  }
}