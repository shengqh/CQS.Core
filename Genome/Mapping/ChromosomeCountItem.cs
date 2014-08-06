using CQS.Genome.Sam;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountItem
  {
    public ChromosomeCountItem()
    {
      this.Names = new HashSet<string>();
      this.Queries = new HashSet<SAMAlignedItem>();
    }

    public HashSet<string> Names { get; set; }

    public HashSet<SAMAlignedItem> Queries { get; set; }

    public int QueryCount
    {
      get
      {
        return (from q in Queries
                select q.QueryCount).Sum();
      }
    }
  }

  public static class ChromosomeMappedCountItemExtension
  {
    public static List<SAMAlignedItem> GetQueries(this List<ChromosomeCountItem> items)
    {
      return (from item in items
              from loc in item.Queries
              select loc).Distinct().OrderBy(m => m.Qname).ToList();
    }

    public static void MergeItems(this List<ChromosomeCountItem> chroms)
    {
      for (int i = chroms.Count - 1; i >= 0; i--)
      {
        var vi = chroms[i];
        for (int j = i - 1; j >= 0; j--)
        {
          var vj = chroms[j];
          if (vj.Queries.IsSupersetOf(vi.Queries))
          {
            if (vi.Queries.Count == vj.Queries.Count)
            {
              vj.Names.UnionWith(vi.Names);
            }
            chroms.RemoveAt(i);
            break;
          }
        }
      }
    }

  }
}
