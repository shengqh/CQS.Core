using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.Sam
{
  public class SAMAlignedItemFileFormat : IFileFormat<List<SAMAlignedItem>>
  {
    public List<SAMAlignedItem> ReadFromFile(string fileName)
    {
      throw new NotImplementedException();
    }

    public void WriteToFile(string fileName, List<SAMAlignedItem> reads)
    {
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Query\tSequence\tLength\tScore\tQueryCount\tMatchedCount\tMatches");

        foreach (var read in reads)
        {
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
            read.Qname,
            read.Sequence,
            read.Sequence.Length,
            read.AlignmentScore,
            read.QueryCount,
            read.Locations.Count,
            (from loc in read.Locations select loc.GetLocation()).Merge(','));
        }
      }
    }
  }
}
