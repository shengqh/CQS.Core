using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mirna
{
  public class MirnaT2CMutationBuilder
  {
    public List<MappedMirnaGroup> Build(string countXmlFile)
    {
      var result = new MappedMirnaGroupXmlFileFormat().ReadFromFile(countXmlFile);

      foreach (var group in result)
      {
        foreach (var mirna in group)
        {
          foreach (var region in mirna.MappedRegions)
          {
            var positions = region.Mapped.Keys.ToList();
            foreach (var position in positions)
            {
              var mapped = region.Mapped[position];
              mapped.AlignedLocations.RemoveAll(q =>
              {
                var snp = q.GetNotGsnapMismatch(q.Parent.Sequence);
                if (null == snp)
                {
                  return true;
                }

                return !snp.IsMutation('T', 'C');
              });

              if (mapped.AlignedLocations.Count == 0)
              {
                region.Mapped.Remove(position);
              }
            }
          }

          mirna.MappedRegions.RemoveAll(l => l.Mapped.Count == 0);
        }

        group.RemoveAll(n => n.MappedRegions.Count == 0);
      }

      result.RemoveAll(m => m.Count == 0);

      return result;
    }
  }
}
