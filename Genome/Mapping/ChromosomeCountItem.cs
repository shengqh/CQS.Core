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
      this.Names = new HashSet<string>();
      this.Queries = new HashSet<SAMAlignedItem>();
    }

    public HashSet<string> Names { get; set; }

    public HashSet<SAMAlignedItem> Queries { get; set; }

    public int QueryCount { get; private set; }
    public double EstimatedCount { get; private set; }

    public void CalculateCount()
    {
      this.QueryCount = this.Queries.Sum(l => l.QueryCount);
      this.EstimatedCount = this.Queries.Sum(l => l.QueryCount * 1.0 / l.DistinctSeqnameCount) * Names.Count;
    }
  }

  public static class ChromosomeMappedCountItemExtension
  {
    public static List<SAMAlignedItem> GetQueries(this List<ChromosomeCountItem> items)
    {
      return (from item in items
              from query in item.Queries
              select query).Distinct().OrderBy(m => m.Qname).ToList();
    }


    public static void MergeItems(this List<ChromosomeCountItem> chroms)
    {
      var index = 0;
      while (index < chroms.Count)
      {
        Console.WriteLine("Merging {0} / {1} ...", index + 1, chroms.Count);
        var vi = chroms[index];
        for (int j = chroms.Count - 1; j > index; j--)
        {
          var vj = chroms[j];
          if (vj.Queries.All(l => vi.Queries.Contains(l)))
          {
            if (vi.Queries.Count == vj.Queries.Count)
            {
              vi.Names.UnionWith(vj.Names);
            }
            else
            {
              //remove all mapped information from SAMAlignedItem
              foreach (var q in vj.Queries)
              {
                q.RemoveLocation(l => vj.Names.Contains(l.Seqname));
              }
            }

            chroms.RemoveAt(j);
          }
        }

        index++;
      }
    }

    public static void MergeCalculateSortByEstimatedCount(this List<ChromosomeCountItem> counts)
    {
      counts.Sort((m1, m2) => m2.Queries.Count.CompareTo(m1.Queries.Count));

      counts.MergeItems();

      counts.ForEach(m =>
      {
        foreach (var q in m.Queries)
        {
          q.InitializeDistinctSeqnameCount();
        }
        m.CalculateCount();
      });

      counts.Sort((m1, m2) =>
      {
        var result = m2.EstimatedCount.CompareTo(m1.EstimatedCount);
        if (result == 0)
        {
          result = m2.QueryCount.CompareTo(m1.QueryCount);
        }
        if (result == 0)
        {
          result = m1.Names.OrderBy(m => m).First().CompareTo(m2.Names.OrderBy(m => m).First());
        }
        return result;
      });
    }
  }
}
