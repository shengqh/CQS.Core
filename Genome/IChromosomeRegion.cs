using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome
{
  public interface IChromosomeRegion
  {
    string Chrom { get; set; }

    long ChromStart { get; set; }

    long ChromEnd { get; set; }

    long Length { get; }
  }

  public class ChromosomeRegion : IChromosomeRegion
  {
    public string Chrom { get; set; }

    public long ChromStart { get; set; }

    public long ChromEnd { get; set; }

    public virtual long Length
    {
      get
      {
        return this.ChromEnd - this.ChromStart + 1;
      }
    }
  }

  public static class IChromosomeRegionExtension
  {
    public static bool Overlap(this IChromosomeRegion one, IChromosomeRegion two, double minPercentage)
    {
      if (!one.Chrom.Equals(two.Chrom))
      {
        return false;
      }

      IChromosomeRegion first, second;
      if (one.ChromStart < two.ChromStart || (one.ChromStart == two.ChromStart && one.ChromEnd > two.ChromEnd))
      {
        first = one;
        second = two;
      }
      else
      {
        first = two;
        second = one;
      }

      //no-overlap
      if (first.ChromEnd < second.ChromStart)
      {
        return false;
      }

      //contain
      if (first.ChromEnd >= second.ChromEnd)
      {
        return true;
      }

      //overlap
      var overlap = first.ChromEnd - second.ChromStart + 1;
      return (overlap >= first.Length * minPercentage || overlap >= second.Length * minPercentage);
    }
  }
}
