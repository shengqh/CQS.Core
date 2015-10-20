using CQS.Genome.Sam;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountItem
  {
    public ChromosomeCountItem()
    {
      this.Names = new List<string>();
      this.Queries = new List<SAMAlignedItem>();
    }

    public List<string> Names { get; set; }

    public List<SAMAlignedItem> Queries { get; set; }

    public int QueryCount
    {
      get
      {
        return (from q in Queries
                select q.QueryCount).Sum();
      }
    }

    public void UnionQueryWith(IEnumerable<SAMAlignedItem> items)
    {
      this.Queries = this.Queries.Union(items).Distinct().ToList();
    }

    public void UnionNameWith(IEnumerable<string> items)
    {
      this.Names = this.Names.Union(items).Distinct().ToList();
    }

    public double EstimatedCount { get; private set; }

    public double CalculateEstimatedCount()
    {
      this.EstimatedCount = (from q in Queries
                             select q.QueryCount * Names.Count * 1.0 / q.DistinctSeqnameCount).Sum();
      return this.EstimatedCount;
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
      var index = 0;
      while (index < chroms.Count)
      {
        Console.WriteLine("Merging {0} / {1} ...", index, chroms.Count);
        var vi = chroms[index];
        var vseti = new HashSet<SAMAlignedItem>(vi.Queries);
        for (int j = chroms.Count - 1; j > index; j--)
        {
          var vj = chroms[j];
          if (vj.Queries.All(l => vseti.Contains(l)))
          {
            if (vi.Queries.Count == vj.Queries.Count)
            {
              vi.UnionNameWith(vj.Names);
            }
            else
            {
              //remove all mapped information from SAMAlignedItem
              foreach (var q in vj.Queries)
              {
                q.RemoveLocation(l => vj.Names.Contains(l.Parent.Qname));
              }
            }

            chroms.RemoveAt(j);
          }
        }

        index++;
      }
    }

    //public static void MergeItems2(this List<ChromosomeCountItem> chroms)
    //{
    //  for (int i = chroms.Count - 1; i >= 0; i--)
    //  {
    //    var vi = chroms[i];
    //    var vseti = new HashSet<SAMAlignedItem>(vi.Queries);
    //    for (int j = i - 1; j >= 0; j--)
    //    {
    //      var vj = chroms[j];
    //      if (vseti.IsSubsetOf(vj.Queries))
    //      {
    //        if (vi.Queries.Count == vj.Queries.Count)
    //        {
    //          vj.UnionNameWith(vi.Names);
    //        }
    //        else
    //        {
    //          //remove all mapped information from SAMAlignedItem
    //          foreach (var q in vi.Queries)
    //          {
    //            q.RemoveLocation(l => vi.Names.Contains(l.Parent.Qname));
    //          }
    //        }

    //        chroms.RemoveAt(i);
    //        break;
    //      }
    //    }
    //  }
    //}

    public static void CalculateAndSortByEstimatedCount(this List<ChromosomeCountItem> counts)
    {
      counts.Sort((m1, m2) => m2.QueryCount.CompareTo(m1.QueryCount));

      counts.MergeItems();

      counts.ForEach(m =>
      {
        foreach (var q in m.Queries)
        {
          q.InitializeDistinctSeqnameCount();
        }
        m.CalculateEstimatedCount();
      });

      counts.Sort((m1, m2) => m2.EstimatedCount.CompareTo(m1.EstimatedCount));
    }
  }
}
