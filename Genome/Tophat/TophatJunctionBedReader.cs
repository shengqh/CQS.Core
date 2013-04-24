using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Bed;

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
                 Chr = bed.Chrom,
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
