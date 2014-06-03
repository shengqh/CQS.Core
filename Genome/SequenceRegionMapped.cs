using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Sam;

namespace CQS.Genome
{
  public class SequenceRegionMapped
  {
    public SequenceRegionMapped()
    {
      this.AlignedLocations = new List<SAMAlignedLocation>();
      this.Region = new SequenceRegion();
    }

    public ISequenceRegion Region { get; set; }

    public int Offset { get; set; }

    public double EstimatedCount
    {
      get
      {
        return this.AlignedLocations.Sum(m => m.Parent.EstimatedCount);
      }
    }

    public List<SAMAlignedLocation> AlignedLocations { get; private set; }

    public double GetEstimatedCount(Func<SAMAlignedLocation, bool> func)
    {
      return (from loc in this.AlignedLocations
              where func(loc)
              select loc.Parent.EstimatedCount).Sum();
    }
  }
}
