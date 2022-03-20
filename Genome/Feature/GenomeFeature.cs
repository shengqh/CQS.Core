using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Feature
{
  public class GenomeFeature
  {
    public GenomeFeature()
    {
      this.Blocks = new List<ISequenceRegion>();
    }

    public GenomeFeature(string name, IEnumerable<ISequenceRegion> blocks)
    {
      this.Name = name;
      this.Blocks = new List<ISequenceRegion>(blocks);
    }

    public string Name { get; set; }

    public List<ISequenceRegion> Blocks { get; private set; }

    public List<long> Positions { get; private set; }

    /// <summary>
    /// Here, the locus in Blocks are bed format (0-based)
    /// </summary>
    public void InitializePositions()
    {
      Positions = new List<long>();
      foreach (var block in Blocks)
      {
        for (long i = block.Start; i < block.End; i++)
        {
          Positions.Add(i);
        }
      }
      if (!IsForward)
      {
        Positions.Sort((m1, m2) => m2.CompareTo(m1));
      }
    }

    public bool IsForward
    {
      get
      {
        return Blocks[0].Strand == '+';
      }
    }

    /// <summary>
    /// Get 0-based genome locus from 0-based internal locus
    /// </summary>
    /// <param name="start">0-based start position</param>
    /// <param name="end">0-based start position (not included)</param>
    /// <returns>location in bed format</returns>
    public Location GetGenomeLocusFromInternalLocus(int start, int end)
    {
      if (Positions.Count < end)
      {
        return null;
      }

      var list = new List<long>();
      for (int i = start; i < end; i++)
      {
        list.Add(Positions[i]);
      }

      if (!IsForward)
      {
        list.Reverse();
      }

      for (int i = 1; i < list.Count; i++)
      {
        if (list[i] != list[i - 1] + 1)
        {
          return null;
        }
      }

      return new Location(list.First(), list.Last() + 1);
    }
  }
}
