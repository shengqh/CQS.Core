using CQS.Genome.Bed;
using RCPA;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Tophat
{
  public class TophatJunctionBedReader : IFileReader<List<JunctionItem>>
  {
    public List<JunctionItem> ReadFromFile(string fileName)
    {
      var beds = new BedItemFile<BedItem>().ReadFromFile(fileName);

      List<JunctionItem> result = (from bed in beds
                                   select new JunctionItem()
                                   {
                                     Chr = bed.Seqname,
                                     Start1 = bed.Blocks[0].ChromStart,
                                     End1 = bed.Blocks[0].ChromEnd,
                                     Start2 = bed.Blocks[1].ChromStart,
                                     End2 = bed.Blocks[1].ChromEnd,
                                     Name = bed.Name
                                   }).ToList();

      return result;
    }
  }
}
