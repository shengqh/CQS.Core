using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.Mirna
{
  public class MirnaCountFileWriter : IFileWriter<List<MappedMirnaGroup>>
  {
    private List<int> offsets;

    public MirnaCountFileWriter(List<int> offsets)
    {
      this.offsets = offsets;
    }

    public void WriteToFile(string fileName, List<MappedMirnaGroup> mirnas)
    {
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine("miRNA\tLocation\tSequence\tTotalCount\t" + (from p in this.offsets select "Count" + p.ToString()).Merge("\t"));

        foreach (var mirna in mirnas)
        {
          var counts = (from p in this.offsets
                        select mirna.GetEstimatedCount(p)).ToList();

          sw.WriteLine("{0}\t{1}\t{2}\t{3:0.##}\t{4}",
            mirna.DisplayName,
            mirna.DisplayLocation,
            mirna[0].Sequence,
            counts.Sum(),
            counts.ConvertAll(m => string.Format("{0:0.##}", m)).Merge("\t"));
        }
      }
    }
  }
}
