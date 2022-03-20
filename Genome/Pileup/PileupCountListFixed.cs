using CQS.Genome.Sam;
using System.Collections.Generic;

namespace CQS.Genome.Pileup
{
  public class PileupCountListFixed
  {
    public PileupCountListFixed(int length)
    {
      this.Count = new List<PileupCount>(length);
      for (int i = 0; i <= length; i++)
      {
        this.Count.Add(new PileupCount());
      }
    }

    public void Clear()
    {
      this.Count = new List<PileupCount>();
    }

    public List<PileupCount> Count { get; private set; }

    public void Add(SAMAlignedItem item, int count)
    {
      string align, refer;
      item.GetSequences(out align, out refer);

      for (int i = 0; i < align.Length; i++)
      {
        var c = align[i];
        var dic = Count[(int)(item.Pos) + i];
        int curcount = 0;
        if (dic.TryGetValue(c, out curcount))
        {
          dic[c] = curcount + count;
        }
        else
        {
          dic[c] = count;
        }
      }
    }

    public int AddByScore(SAMAlignedItem item, int count, int minScore)
    {
      int result = 0;
      string align, score;
      item.GetSequenceScore(out align, out score);

      for (int i = 0; i < align.Length; i++)
      {
        if (minScore > 0)
        {
          var bq = (int)(score[i]) - 33;
          if (bq < minScore)
          {
            result++;
            continue;
          }
        }

        var c = align[i];
        var dic = Count[(int)(item.Pos) + i];
        int curcount = 0;
        if (dic.TryGetValue(c, out curcount))
        {
          dic[c] = curcount + count;
        }
        else
        {
          dic[c] = count;
        }
      }

      return result;
    }
  }
}
