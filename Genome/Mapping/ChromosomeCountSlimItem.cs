using CQS.Genome.Sam;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class SAMChromosomeItem
  {
    public SAMChromosomeItem()
    {
      this.Chromosomes = new List<string>();
    }

    public string Qname { get; set; }
    public int QueryCount { get; set; }
    public List<string> Chromosomes { get; set; }

    public string Sample { get; set; }
  }

  public class ChromosomeCountSlimItem
  {
    public ChromosomeCountSlimItem()
    {
      this.Names = new List<string>();
      this.Queries = new List<SAMChromosomeItem>();
    }

    public List<string> Names { get; set; }

    public List<SAMChromosomeItem> Queries { get; set; }

    public int QueryCount
    {
      get
      {
        return (from q in Queries
                select q.QueryCount).Sum();
      }
    }

    public void UnionQueryWith(IEnumerable<SAMChromosomeItem> items)
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
                             select q.QueryCount * Names.Count * 1.0 / q.Chromosomes.Count).Sum();
      return this.EstimatedCount;
    }
  }

  public static class ChromosomeMappedCountSlimItemExtension
  {
    public static List<SAMChromosomeItem> GetQueries(this List<ChromosomeCountSlimItem> items)
    {
      return (from item in items
              from loc in item.Queries
              select loc).Distinct().OrderBy(m => m.Qname).ToList();
    }

    public static void MergeItems(this List<ChromosomeCountSlimItem> chroms)
    {
      var index = 0;
      while (index < chroms.Count)
      {
        Console.WriteLine("Merging {0} / {1} ...", index, chroms.Count);
        var vi = chroms[index];
        var vseti = new HashSet<SAMChromosomeItem>(vi.Queries);
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
                q.Chromosomes.RemoveAll(l => vj.Names.Contains(l));
              }
            }

            chroms.RemoveAt(j);
          }
        }

        index++;
      }
    }

    public static void CalculateAndSortByEstimatedCount(this List<ChromosomeCountSlimItem> counts)
    {
      Console.WriteLine("Sorting by number of query...");
      counts.Sort((m1, m2) => m2.Queries.Count.CompareTo(m1.Queries.Count));

      Console.WriteLine("Merging...");
      counts.MergeItems();

      Console.WriteLine("Estimating...");
      counts.ForEach(m =>
      {
        m.CalculateEstimatedCount();
      });

      Console.WriteLine("Sorting by estimated count...");
      counts.Sort((m1, m2) =>
      {
        var res = m2.EstimatedCount.CompareTo(m1.EstimatedCount);
        if (res == 0)
        {
          res = m1.Names.First().CompareTo(m2.Names.First());
        }
        return res;
      });
    }
  }
}
