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
      this.AlignedLocations = new List<SamAlignedLocation>();
      this.Region = new SequenceRegion();
    }

    public ISequenceRegion Region { get; set; }

    public int Offset { get; set; }

    public int _queryCountBeforeFilter;

    public int QueryCountBeforeFilter
    {
      get
      {
        if (_queryCountBeforeFilter == 0)
        {
          return QueryCount;
        }
        return _queryCountBeforeFilter;
      }
      set
      {
        _queryCountBeforeFilter = value;
      }
    }

    public double EstimatedCount
    {
      get
      {
        return this.AlignedLocations.Sum(m => m.Parent.EstimatedCount);
      }
    }

    public int QueryCount
    {
      get
      {
        return this.AlignedLocations.Sum(m => m.Parent.QueryCount);
      }
    }

    public List<SamAlignedLocation> AlignedLocations { get; private set; }

    public double GetEstimatedCount(Func<SamAlignedLocation, bool> func)
    {
      return (from loc in this.AlignedLocations
              where func(loc)
              select loc.Parent.EstimatedCount).Sum();
    }

    public double PValue { get; set; }
  }
}
