using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CQS.Genome.Sam;

namespace CQS.Genome.Mirna
{
  public class MappedMirna
  {
    public MappedMirna()
    {
      this.Name = string.Empty;
      this.Sequence = string.Empty;
      this.MappedRegions = new List<MappedMirnaRegion>();
    }

    public string Name { get; set; }

    public string Sequence { get; set; }

    public List<MappedMirnaRegion> MappedRegions { get; private set; }

    public string NameSequence
    {
      get
      {
        if (!string.IsNullOrEmpty(this.Sequence))
        {
          return this.Name + ":" + this.Sequence;
        }
        else
        {
          return this.Name;
        }
      }
    }

    private Regex reg = new Regex("(.+?):(.+?)-(.+?):([+-])");

    public MappedMirnaRegion FindOrCreateRegion(string loc)
    {
      foreach (var region in MappedRegions)
      {
        if (region.Region.GetLocation().Equals(loc))
        {
          return region;
        }
      }

      var result = new MappedMirnaRegion();
      result.Region = SequenceRegionUtils.ParseLocation<SequenceRegion>(loc);
      this.MappedRegions.Add(result);

      return result;
    }

    public double EstimateCount
    {
      get { return MappedRegions.Sum(m => m.Mapped.Sum(n => n.Value.EsminatedCount)); }
    }

    public double GetEstimatedCount(int index)
    {
      return MappedRegions.Sum(m =>
      {
        if (m.Mapped.ContainsKey(index))
        {
          return m.Mapped[index].AlignedLocations.Sum(n => n.Parent.EsminatedCount);
        }
        else
        {
          return 0;
        }
      });
    }

    public long Length
    {
      get
      {
        if (MappedRegions.Count == 0 || MappedRegions[0].Region == null)
        {
          return 0;
        }
        else
        {
          return MappedRegions[0].Region.Length;
        }
      }
    }

    public override string ToString()
    {
      return this.Name;
    }

    public string DisplayLocation
    {
      get
      {
        return (from loc in MappedRegions
                select loc.Region.GetLocation()).Merge(",");
      }
    }
  }
}
