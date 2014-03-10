using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Sam;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupCountList
  {
    public PileupCountList()
    {
      this.Count = new List<PileupCount>();
    }

    public void Clear()
    {
      this.Count = new List<PileupCount>();
    }

    public string Chromosome
    {
      get
      {
        if (this.Count.Count > 0)
        {
          return this.Count[0].Chromosome;
        }
        else
        {
          return string.Empty;
        }
      }
    }

    public long Position
    {
      get
      {
        if (this.Count.Count > 0)
        {
          return this.Count[0].Position;
        }
        else
        {
          return -1;
        }
      }
    }

    public List<PileupCount> Count { get; private set; }

    public List<PileupCount> Add(SAMAlignedItem item, int count)
    {
      List<PileupCount> result = null;

      if (!item.Locations[0].Seqname.Equals(this.Chromosome))
      {
        result = Count;
        Count = new List<PileupCount>();
      }
      else if (this.Position != -1)
      {
        if (item.Pos > this.Count.Last().Position)
        {
          result = Count;
          Count = new List<PileupCount>();
        }
        else
        {
          int finishedCount = (int)(item.Pos - this.Position);
          if (finishedCount > 0)
          {
            result = new List<PileupCount>();
            result.AddRange(Count.Take(finishedCount));
            Count.RemoveRange(0, finishedCount);
          }
        }
      }

      string align, refer;
      item.GetSequences(out align, out refer);
      for (int i = Count.Count; i < align.Length; i++)
      {
        Count.Add(new PileupCount()
        {
          Chromosome = item.Locations[0].Seqname,
          Position = item.Locations[0].Start + i,
          Reference = refer[i]
        });
      }

      for (int i = 0; i < align.Length; i++)
      {
        var c = align[i];
        var dic = Count[i];
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
