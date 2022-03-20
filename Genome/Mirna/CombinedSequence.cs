using System;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mirna
{
  public class CombinedSequence
  {
    public string Sequence1 { get; set; }

    public int Position1 { get; set; }

    public string Sequence2 { get; set; }

    public int Position2 { get; set; }

    public int[] MismatchPositions { get; set; }

    public string GetAnnotatedSequence1()
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < Position2; i++)
      {
        sb.Append(".");
      }
      for (int i = 0; i < Sequence1.Length; i++)
      {
        if (MismatchPositions.Contains(Position2 + i))
        {
          sb.Append(Char.ToLower(Sequence1[i]));
        }
        else
        {
          sb.Append(Sequence1[i]);
        }
      }
      for (int i = Position2 + Sequence1.Length; i < Position1 + Sequence2.Length; i++)
      {
        sb.Append('.');
      }
      return sb.ToString();
    }

    public string GetAnnotatedSequence2()
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < Position1; i++)
      {
        sb.Append(".");
      }
      for (int i = 0; i < Sequence2.Length; i++)
      {
        if (MismatchPositions.Contains(Position1 + i))
        {
          sb.Append(Char.ToLower(Sequence2[i]));
        }
        else
        {
          sb.Append(Sequence2[i]);
        }
      }
      for (int i = Position1 + Sequence2.Length; i < Position2 + Sequence1.Length; i++)
      {
        sb.Append('.');
      }
      return sb.ToString();
    }
  }
}
