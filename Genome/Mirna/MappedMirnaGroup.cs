using CQS.Genome.Sam;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mirna
{
  /// <summary>
  /// A list of mirna with some identical features, such as sequence or same set of mapped reads.
  /// </summary>
  public class MappedMirnaGroup : List<MappedMirna>
  {
    public MappedMirnaGroup()
    { }

    public double EstimateCount
    {
      get { return this.Sum(m => m.EstimatedCount); }
    }

    public double GetEstimatedCount(int offset = MirnaConsts.NO_OFFSET, string nta = MirnaConsts.NO_NTA)
    {
      return this.Sum(m => m.GetEstimatedCount(offset, nta));
    }

    public string DisplayName
    {
      get
      {
        return (from mirna in this select mirna.Name).Merge(";");
      }
    }

    public string DisplayLocation
    {
      get
      {
        return (from mirna in this select mirna.DisplayLocation).Merge(";");
      }
    }

    public List<SamAlignedLocation> GetAlignedLocations()
    {
      return (from mirna in this
              from pos in mirna.MappedRegions
              from mapped in pos.Mapped
              from q in mapped.Value.AlignedLocations
              select q).Distinct().OrderBy(m => m.Parent.Qname).ToList();
    }
  }

  public static class MappedMiRNAGroupExtension
  {
    public static HashSet<string> GetQueryNames(this List<MappedMirnaGroup> groups)
    {
      return new HashSet<string>(from mirna in groups
                                 from item in mirna.GetAlignedLocations()
                                 select item.Parent.Qname);
    }
  }
}
