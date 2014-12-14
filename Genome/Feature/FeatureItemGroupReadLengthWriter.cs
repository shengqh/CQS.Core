using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.Feature
{
  public class FeatureItemGroupReadLengthWriter : IFileWriter<List<FeatureItemGroup>>
  {
    public FeatureItemGroupReadLengthWriter()
    { }

    public void WriteToFile(string fileName, List<FeatureItemGroup> groups)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Subject\tReadLength\tReadCount");

        foreach (var g in groups)
        {
          var reads = (from l in g.GetAlignedLocations()
                       select l.Parent).Distinct().GroupBy(m => m.Sequence.Length).OrderBy(m => m.Key).ToList();
          foreach (var read in reads)
          {
            sw.WriteLine("{0}\t{1}\t{2}",
              g.DisplayName,
              read.Key,
              read.Sum(m => m.QueryCount));
          }
        }
      }
    }
  }
}
