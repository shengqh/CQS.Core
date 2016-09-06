using CQS.Genome.Mapping;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public static class SmallRNASequenceUtils
  {
    public const double MINIMUM_OVERLAP_RATE = 0.9;

    public static Dictionary<string, List<SmallRNASequence>> ConvertFrom(List<ChromosomeCountSlimItem> items)
    {
      var result = new Dictionary<string, List<SmallRNASequence>>();
      foreach (var c in items)
      {
        foreach (var q in c.Queries)
        {
          List<SmallRNASequence> seqList;
          if (!result.TryGetValue(q.Sample, out seqList))
          {
            seqList = new List<SmallRNASequence>();
            result[q.Sample] = seqList;
          }

          seqList.Add(new SmallRNASequence()
          {
            Sample = q.Sample,
            Sequence = q.Sequence,
            Count = q.QueryCount
          });
        }
      }

      return result;
    }

    public static List<SmallRNASequenceContig> GetTopContig(List<SmallRNASequence> sequences, double minimumOverlapRate = MINIMUM_OVERLAP_RATE)
    {
      List<SmallRNASequenceContig> result = new List<SmallRNASequenceContig>();
      foreach (var seq in sequences)
      {
        var contig = new SmallRNASequenceContig();
        contig.ContigSequence = seq.Sequence;
        contig.ContigCount = seq.Count;
        contig.Sequences.Add(seq);
        result.Add(contig);
      }

      return GetTopContig(result, minimumOverlapRate);
    }

    public static List<SmallRNASequenceContig> GetTopContig(List<SmallRNASequenceContig> result, double minimumOverlapRate = MINIMUM_OVERLAP_RATE)
    {
      result.Sort((m1, m2) => m2.ContigCount.CompareTo(m1.ContigCount));

      //merge all contig contains in another contig
      int index = 0;
      while (index < result.Count)
      {
        var contig = result[index];
        var removed = new List<int>();
        for (var next = index + 1; next < result.Count; next++)
        {
          var nextContig = result[next];
          if (contig.ContigSequence.Contains(nextContig.ContigSequence))
          {
            contig.ContigCount += nextContig.ContigCount;
            contig.Sequences.AddRange(nextContig.Sequences);
            removed.Add(next);
            continue;
          }

          if (nextContig.ContigSequence.Contains(contig.ContigSequence))
          {
            contig.ContigSequence = nextContig.ContigSequence;
            contig.ContigCount += nextContig.ContigCount;
            contig.Sequences.AddRange(nextContig.Sequences);
            removed.Add(next);
            continue;
          }
        }
        for (int i = removed.Count - 1; i >= 0; i--)
        {
          result.RemoveAt(removed[i]);
        }
        index++;
      }

      //merge all overlap contig
      index = 0;
      while (index < result.Count)
      {
        var contig = result[index];
        var removed = new List<int>();
        for (var next = index + 1; next < result.Count; next++)
        {
          var nextContig = result[next];

          var concatSequence = StringUtils.ConcatOverlap(contig.ContigSequence, nextContig.ContigSequence, minimumOverlapRate);
          if (concatSequence != null)
          {
            contig.ContigSequence = concatSequence;
            contig.ContigCount += nextContig.ContigCount;
            contig.Sequences.AddRange(nextContig.Sequences);
            removed.Add(next);
          }
        }
        for (int i = removed.Count - 1; i >= 0; i--)
        {
          result.RemoveAt(removed[i]);
        }

        index++;
      }

      return result;
    }

    public static List<SmallRNASequenceContig> BuildContigByIdenticalSimilarity(Dictionary<string, List<SmallRNASequence>> counts, int topNumber = int.MaxValue, double minOverlapRate = MINIMUM_OVERLAP_RATE, IProgressCallback progress = null)
    {
      List<SmallRNASequenceContig> sequences;
      if (topNumber == int.MaxValue)
      {
        sequences = (from lst in counts.Values
                     let smap = GetTopContig(lst, minOverlapRate)
                     from seq in smap
                     select seq).ToList();
      }
      else {
        sequences = (from lst in counts.Values
                     let smap = GetTopContig(lst.Take(Math.Min(lst.Count, topNumber * 100)).ToList(), minOverlapRate)
                     from seq in smap
                     select seq).ToList();
      }

      if (progress == null)
      {
        Console.WriteLine("Total sequence = {0}", sequences.Count);
      }
      else
      {
        progress.SetMessage("Total sequence = {0}", sequences.Count);
      }

      var result = GetTopContig(sequences, minOverlapRate);
      result.Sort((m1, m2) => m2.ContigCount.CompareTo(m1.ContigCount));
      result = result.Take(topNumber).ToList();
      result.ForEach(m => m.Sequences.Clear());

      //get all contained sequences
      foreach (var map in counts.Values)
      {
        foreach (var count in map)
        {
          foreach (var seq in result)
          {
            if (seq.ContigSequence.Contains(count.Sequence))
            {
              seq.Sequences.Add(count);
              break;
            }
          }
        }
      }

      foreach (var seq in result)
      {
        seq.ContigCount = seq.Sequences.Sum(l => l.Count);
      }
      result.Sort((m1, m2) => m2.ContigCount.CompareTo(m1.ContigCount));

      return result;
    }

    public static List<SmallRNASequenceContig> BuildContigByIdenticalSequence(Dictionary<string, List<SmallRNASequence>> counts, int topNumber = int.MaxValue)
    {
      //Get unique sequences
      List<IGrouping<string, SmallRNASequence>> sequences;
      if (topNumber == int.MaxValue)
      {
        sequences = (from map in counts.Values
                     let smap = map
                     from seq in smap
                     select seq).GroupBy(m => m.Sequence).
                         OrderByDescending(m => m.Sum(l => l.Count)).ToList();
      }
      else
      {
        sequences = (from map in counts.Values
                     let smap = map.Take(Math.Min(map.Count, topNumber)).ToList()
                     from seq in smap
                     select seq).GroupBy(m => m.Sequence).
                         OrderByDescending(m => m.Sum(l => l.Count)).ToList();
      }

      //Initialize Sequence~Contig map
      var resultMap = new Dictionary<string, SmallRNASequenceContig>();
      foreach (var seq in sequences)
      {
        var contig = new SmallRNASequenceContig();
        contig.ContigSequence = seq.Key;
        resultMap[seq.Key] = contig;
      }

      //Add smallRNAsequence into Sequence~Contig map
      foreach (var map in counts.Values)
      {
        foreach (var count in map)
        {
          resultMap[count.Sequence].Sequences.Add(count);
        }
      }

      //Initialize config count
      foreach (var seq in resultMap.Values)
      {
        seq.ContigCount = seq.Sequences.Sum(l => l.Count);
      }

      return (from v in resultMap.Values
              orderby v.ContigCount descending
              select v).ToList();
    }
  }
}
