using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mapping
{
  public static class MappedItemUtils
  {
    public static List<MappedItem> GroupByName(IEnumerable<SequenceRegionMapped> regions)
    {
      var result = new List<MappedItem>();

      foreach (var curregions in regions.GroupBy(m => m.Region.Name).ToList())
      {
        var mi = new MappedItem();
        mi.Name = curregions.Key;
        mi.MappedRegions.AddRange(curregions);
        result.Add(mi);
      }

      return result;
    }
  }
}
