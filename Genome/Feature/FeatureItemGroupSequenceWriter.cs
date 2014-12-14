using System.Collections.Generic;
using System.IO;
using System.Linq;
using CQS.Genome.Sam;
using RCPA;
using RCPA.Seq;

namespace CQS.Genome.Feature
{
  public class FeatureItemGroupSequenceWriter : IFileWriter<List<FeatureItemGroup>>
  {
    public void WriteToFile(string fileName, List<FeatureItemGroup> groups)
    {
      groups.SortTRna();

      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Index\tSubject\tSubjectLocation\tSequence\tSequenceLocation\tQueryCount");

        var index = 0;
        foreach (var g in groups)
        {
          index++;

          var gstrand = g.GetAlignedLocations().First().Strand;

          var reads = (from l in g.GetAlignedLocations()
                       select l.Parent).Distinct().ToList();

          var list = reads.GroupBy(m => GetSequence(gstrand, m)).ToList().ConvertAll(m =>
          {
            var loc = (from item in g
                       from mr in item.Mapped
                       from l in mr.SamLocations
                       where GetSequence(gstrand, l.SamLocation.Parent).Equals(m.Key)
                       select l.SamLocation).First();
            return new { Item = m, Location = loc };
          }).OrderBy(m => m.Location.Start).ToList();

          foreach (var read in list)
          {
            sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
              index,
              g.DisplayName,
              g.DisplayLocation,
              read.Item.Key,
              read.Location.GetLocation(),
              read.Item.Sum(m => m.QueryCount));
          }
        }
      }
    }

    private static string GetSequence(char gstrand, SAMAlignedItem m)
    {
      return gstrand == '+' ? m.Sequence : SequenceUtils.GetReverseComplementedSequence(m.Sequence);
    }
  }
}