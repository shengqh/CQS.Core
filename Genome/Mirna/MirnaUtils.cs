using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mirna
{
  public static class MirnaUtils
  {
    public static List<int> GetMismatchIndecies(string refseq, string samseq)
    {
      var maxlength = Math.Max(refseq.Length, samseq.Length);
      var minlength = Math.Min(refseq.Length, samseq.Length);

      var result = new List<int>();
      for (int i = 0; i < minlength; i++)
      {
        if (refseq[i] != samseq[i])
        {
          result.Add(i);
        }
      }

      for (int i = minlength; i < maxlength; i++)
      {
        result.Add(i);
      }

      return result;
    }

    public static CombinedSequence GetCombinedSequence(string seq1, string seq2)
    {
      List<CombinedSequence> scores = new List<CombinedSequence>();
      for (int i = 0; i < 3; i++)
      {
        List<int> vs = new List<int>();
        for (int j = 0; j < i; j++)
        {
          vs.Add(j);
        }

        var subseq1 = seq1.Substring(i);
        scores.Add(new CombinedSequence()
        {
          Sequence1 = seq1,
          Sequence2 = seq2,
          Position1 = i,
          Position2 = 0,
          MismatchPositions = vs.Union(GetMismatchIndecies(subseq1, seq2).ConvertAll(m => m + i)).ToArray()
        });

        var subseq2 = seq2.Substring(i);
        scores.Add(new CombinedSequence()
        {
          Sequence1 = seq1,
          Sequence2 = seq2,
          Position1 = 0,
          Position2 = i,
          MismatchPositions = vs.Union(GetMismatchIndecies(seq1, subseq2).ConvertAll(m => m + i)).ToArray()
        });
      }

      var min = scores.Min(m => m.MismatchPositions.Length);
      return scores.Find(m => m.MismatchPositions.Length == min);
    }
  }
}
