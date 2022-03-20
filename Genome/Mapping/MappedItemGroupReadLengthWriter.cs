using RCPA;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class MappedItemGroupReadLengthWriter : IFileWriter<List<MappedItemGroup>>
  {
    public MappedItemGroupReadLengthWriter()
    { }

    public void WriteToFile(string fileName, List<MappedItemGroup> groups)
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
