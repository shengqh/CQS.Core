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
      this.Sequence = string.Empty;
    }

    public string Qname { get; set; }
    public string Sequence { get; set; }
    public int QueryCount { get; set; }
    public List<string> Chromosomes { get; set; }
    public string Sample { get; set; }
    public double GetEstimatedCount() { return QueryCount * 1.0 / Chromosomes.Count; }
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

    public double EstimatedCount { get; private set; }

    public double CalculateEstimatedCount()
    {
      this.EstimatedCount = this.Queries.Sum(l => l.GetEstimatedCount()) * Names.Count;
      return this.EstimatedCount;
    }

    public double GetQueryCount()
    {
      return Queries.Sum(l => l.QueryCount);
    }
  }

  public static class ChromosomeCountSlimItemExtension
  {
    private static int AllSubsetCount = 0;
    private static int FirstSame = 0;

    public static SAMChromosomeItem[] GetQueries(this List<ChromosomeCountSlimItem> chroms)
    {
      return (from item in chroms
              from query in item.Queries
              select query).Distinct().OrderBy(l => l.Qname).ToArray();
    }

    public static void MergeIdenticalItems(this List<ChromosomeCountSlimItem> chroms)
    {
      Console.WriteLine("All groups = {0}", chroms.Count);
      var uniqueQueryGroups = (from g in chroms.GroupBy(l => l.Queries.Count)
                               select g.ToArray()).ToArray();

      Console.WriteLine("Groups with same unique queries = {0}", uniqueQueryGroups.Length);

      chroms.Clear();
      foreach (var uqg in uniqueQueryGroups)
      {
        var queryCountGroups = (from g in uqg.GroupBy(l => l.GetQueryCount())
                                select g.ToArray()).ToArray();
        foreach (var qcg in queryCountGroups)
        {
          var qnameGroups = (from g in qcg.GroupBy(g => g.Queries[0].Qname)
                             select g.ToList()).ToArray();
          foreach (var qg in qnameGroups)
          {
            if (qg.Count > 1)
            {
              DoMergeIdentical(qg);
            }
            chroms.AddRange(qg);
          }
          qnameGroups = null;
        }
        queryCountGroups = null;
      }
      uniqueQueryGroups = null;
    }

    private static void MergeItems_old(this List<ChromosomeCountSlimItem> chroms)
    {
      MergeIdenticalItems(chroms);

      AllSubsetCount = 0;
      FirstSame = 0;

      MergeSubset(chroms);

      Console.WriteLine("Allsubsetcount = {0}, First same = {1}", AllSubsetCount, FirstSame);
    }

    public static void MergeSubset(this List<ChromosomeCountSlimItem> chroms)
    {
      var index = 0;
      while (index < chroms.Count)
      {
        Console.WriteLine("Merging {0} / {1} ...", index + 1, chroms.Count);
        DoMergeSubset(chroms, index);
        index++;
      }
    }

    private static void DoMergeIdentical(List<ChromosomeCountSlimItem> chroms)
    {
      //int oldCount = chroms.Count;
      var index = 0;
      while (index < chroms.Count)
      {
        DoMergeIdentical(chroms, index);
        index++;
      }
      //Console.WriteLine("{0} => {1}", oldCount, chroms.Count);
    }

    private static void DoMergeSubset(List<ChromosomeCountSlimItem> chroms, int index)
    {
      var vi = chroms[index];
      var viQueries = new HashSet<SAMChromosomeItem>(vi.Queries);
      for (int j = chroms.Count - 1; j > index; j--)
      {
        var vj = chroms[j];
        if (vj.Queries.All(l => viQueries.Contains(l)))
        {
          if (vi.Queries.Count == vj.Queries.Count)
          {
            Console.WriteLine("Impossible!!!");
            vi.Names.AddRange(vj.Names);
          }
          else
          {
            AllSubsetCount++;
            if (vi.Queries.First().Qname.Equals(vj.Queries.First().Qname))
            {
              FirstSame++;
            }

            //remove all mapped information from SAMAlignedItem
            foreach (var q in vj.Queries)
            {
              q.Chromosomes.RemoveAll(l => vj.Names.Contains(l));
            }
          }

          chroms.RemoveAt(j);
        }
      }
    }

    /// <summary>
    /// Merge chromosomes with identical queries
    /// </summary>
    /// <param name="chroms">Chroms whose queries are ordered by query name.</param>
    /// <param name="index"></param>
    private static void DoMergeIdentical(List<ChromosomeCountSlimItem> chroms, int index)
    {
      var vi = chroms[index];
      for (int j = chroms.Count - 1; j > index; j--)
      {
        var vj = chroms[j];
        bool bSame = true;
        for (int i = 0; i < vi.Queries.Count; i++)
        {
          if (vi.Queries[i] != vj.Queries[i])
          {
            bSame = false;
            break;
          }
        }

        if (bSame)
        {
          vi.Names.AddRange(vj.Names);
          chroms.RemoveAt(j);
        }
      }
    }

    public static void MergeCalculateSortByEstimatedCount(this List<ChromosomeCountSlimItem> counts)
    {
      Console.WriteLine("Merging...");
      counts.MergeItems();

      Console.WriteLine("Estimating...");
      counts.ForEach(m => { m.CalculateEstimatedCount(); });

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

    public static void MergeItems(this List<ChromosomeCountSlimItem> chroms)
    {
      Console.WriteLine("Before merge = {0}", chroms.Count);
      MergeIdenticalItems(chroms);

      Console.WriteLine("After merge identical = {0}", chroms.Count);

      chroms.Sort((m1, m2) => m2.Queries.Count.CompareTo(m1.Queries.Count));

      var dic = chroms.ToDictionary(m => m.Names.First());
      for (int i = chroms.Count - 1; i > 0; i--)
      {
        var smallOne = chroms[i];

        if (i % 1000 == 0)
        {
          Console.WriteLine("Processing {0} ... ", i);
        }

        var vi = new HashSet<SAMChromosomeItem>(smallOne.Queries);

        List<ChromosomeCountSlimItem> candidates = new List<ChromosomeCountSlimItem>();
        ChromosomeCountSlimItem item;
        foreach (var chr in smallOne.Queries[0].Chromosomes)
        {
          if (dic.TryGetValue(chr, out item) && item != smallOne && item.Queries.Count > smallOne.Queries.Count)
          {
            candidates.Add(item);
          }
        }
        candidates.Sort((m1, m2) => m2.Queries.Count.CompareTo(m1.Queries.Count));

        foreach (var cand in candidates)
        {
          if (vi.IsSubsetOf(cand.Queries))
          {
            foreach (var q in chroms[i].Queries)
            {
              q.Chromosomes.RemoveAll(l => chroms[i].Names.Contains(l));
            }

            chroms.RemoveAt(i);
            break;
          }
        }
      }

      Console.WriteLine("After remove subset = {0}", chroms.Count);
    }
  }
}
