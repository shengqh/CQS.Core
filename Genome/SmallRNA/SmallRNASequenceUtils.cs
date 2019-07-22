using CQS.Genome.Mapping;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public static class SmallRNASequenceUtils
  {
    public static Dictionary<string, List<SmallRNASequence>> ConvertFrom(List<ChromosomeCountSlimItem> items)
    {
      var result = new Dictionary<string, Dictionary<string, SmallRNASequence>>();
      foreach (var c in items)
      {
        foreach (var q in c.Queries)
        {
          Dictionary<string, SmallRNASequence> seqList;
          if (!result.TryGetValue(q.Sample, out seqList))
          {
            seqList = new Dictionary<string, SmallRNASequence>();
            result[q.Sample] = seqList;
          }

          if (!seqList.ContainsKey(q.Sequence))
          {
            seqList[q.Sequence] = new SmallRNASequence()
            {
              Sample = q.Sample,
              Sequence = q.Sequence,
              Count = q.QueryCount
            };
          }
        }
      }

      return result.ToDictionary(l => l.Key, l => l.Value.Values.ToList());
    }

    public static List<SmallRNASequenceContig> GetTopContig(List<SmallRNASequence> sequences, double minimumOverlapRate, int maximumExtensionBase)
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

      return GetTopContig(result, minimumOverlapRate, maximumExtensionBase);
    }

    public static List<SmallRNASequenceContig> GetTopContig(List<SmallRNASequenceContig> result, double minimumOverlapRate, int maximumExtensionBase)
    {
      result.Sort((m1, m2) => m2.ContigCount.CompareTo(m1.ContigCount));

      //merge all contig contained in another contig
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

          var concatSequence = maximumExtensionBase > 0 ?
            StringUtils.ConcatOverlapByExtensionNumber(contig.ContigSequence, nextContig.ContigSequence, maximumExtensionBase) :
            StringUtils.ConcatOverlapByPercentage(contig.ContigSequence, nextContig.ContigSequence, minimumOverlapRate);
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

    public static List<SmallRNASequenceContig> BuildMiniContig(List<SmallRNASequenceContig> contigs, int topNumber)
    {
      var result = new List<SmallRNASequenceContig>();
      for (int i = 0; i < contigs.Count && i < topNumber; i++)
      {
        var contig = contigs[i];
        var seqCount = (from seq in contig.Sequences.GroupBy(l => l.Sequence)
                        select new { Seq = seq.Key, SeqCount = seq.Sum(l => l.Count), SeqIndex = contig.ContigSequence.IndexOf(seq.Key) }).OrderByDescending(l => l.SeqCount).ToList();
        //seqCount.ForEach(l => Console.WriteLine("{0}\t{1}\t{2}", l.Seq, l.SeqCount, l.SeqIndex));

        var fq = seqCount[0];
        var list = seqCount.Where(l => Math.Abs(l.SeqIndex - fq.SeqIndex) < 3 && l.SeqCount >= fq.SeqCount * 0.1).ToList();
        var start = list.Min(l => l.SeqIndex);
        var end = list.Max(l => l.SeqIndex + l.Seq.Length);
        var contigSeq = contig.ContigSequence.Substring(start, end - start);
        var contigSequences = new HashSet<string>(list.ConvertAll(l => l.Seq));

        var miniContig = new SmallRNASequenceContig();
        miniContig.ContigSequence = contigSeq;
        miniContig.ContigCount = list.Sum(l => l.SeqCount);
        miniContig.Sequences.AddRange(contig.Sequences.Where(l => contigSequences.Contains(l.Sequence)));
        result.Add(miniContig);
      }

      return result;
    }

    public static List<SmallRNASequenceContig> BuildContigByIdenticalSimilarity(Dictionary<string, List<SmallRNASequence>> counts, double minOverlapRate, int maxExtensionBase, int topNumber = int.MaxValue, IProgressCallback progress = null)
    {
      Func<List<SmallRNASequence>, List<SmallRNASequenceContig>> getTopConfigFunc;
      if (topNumber == int.MaxValue)
      {
        getTopConfigFunc = (m) => GetTopContig(m, minOverlapRate, maxExtensionBase);
      }
      else
      {
        getTopConfigFunc = (m) => GetTopContig(m.Take(Math.Min(m.Count, topNumber)).ToList(), minOverlapRate, maxExtensionBase);
      }
      List<SmallRNASequenceContig> sequences = (from lst in counts.Values
                                                let smap = getTopConfigFunc(lst)
                                                from seq in smap
                                                select seq).ToList();

      if (progress == null)
      {
        Console.WriteLine("Total sequence = {0}", sequences.Count);
      }
      else
      {
        progress.SetMessage("Total sequence = {0}", sequences.Count);
      }

      var result = GetTopContig(sequences, minOverlapRate, maxExtensionBase);

      //normalize the contig count by sample size
      Dictionary<string, int> totalCounts = GetSampleCountMap(counts);

      CalculateNormalizedContigCount(result, totalCounts);

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

      CalculateNormalizedContigCount(result, totalCounts);

      result.Sort((m1, m2) => m2.ContigCount.CompareTo(m1.ContigCount));

      return result;
    }

    private static Dictionary<string, int> GetSampleCountMap(Dictionary<string, List<SmallRNASequence>> counts)
    {
      return (from sample in counts.Keys
              let lst = counts[sample]
              select new { Sample = sample, Count = lst.Sum(l => l.Count) }).ToDictionary(l => l.Sample, l => l.Count);
    }

    private static void CalculateNormalizedContigCount(List<SmallRNASequenceContig> result, Dictionary<string, int> totalCounts)
    {
      result.ForEach(l => l.ContigCount = l.Sequences.Sum(s => s.Count * 1.0 / totalCounts[s.Sample]));
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
          SmallRNASequenceContig contig;
          if (resultMap.TryGetValue(count.Sequence, out contig))
          {
            contig.Sequences.Add(count);
          }
        }
      }

      //Initialize config count
      Dictionary<string, int> totalCounts = GetSampleCountMap(counts);
      CalculateNormalizedContigCount(resultMap.Values.ToList(), totalCounts);

      return (from v in resultMap.Values
              orderby v.ContigCount descending
              select v).ToList();
    }
  }
}
