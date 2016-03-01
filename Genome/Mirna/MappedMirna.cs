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

    public double GetEstimatedCount()
    {
      return MappedRegions.Sum(m => m.Mapped.Sum(n => n.Value.GetEstimatedCount()));
    }

    public double GetEstimatedCount(int offset, string nta)
    {
      List<SAMAlignedLocation> locs;

      if (MirnaConsts.NO_OFFSET == offset)
      {
        locs = (from mr in MappedRegions
                from srm in mr.Mapped.Values
                from loc in srm.AlignedLocations
                select loc).ToList();
      }
      else
      {
        locs = (from mr in MappedRegions
                where mr.Mapped.ContainsKey(offset)
                let srm = mr.Mapped[offset]
                from loc in srm.AlignedLocations
                select loc).ToList();
      }

      if (MirnaConsts.NO_NTA != nta)
      {
        locs.RemoveAll(m => !m.Parent.ClippedNTA.Equals(nta));
      }

      return locs.Sum(m => m.Parent.GetEstimatedCount());
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

    ///// <summary>
    ///// Get mutated string, the mutated position will be marked by uppercase letter.
    ///// </summary>
    ///// <returns></returns>
    //public string GetMutationString()
    //{
    //  var result = new StringBuilder(this.Sequence.ToLower());
    //  foreach (var region in this.MappedRegions)
    //  {
    //  }
    //}
  }
}
