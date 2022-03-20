using System.Collections.Generic;
using System.Linq;

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

    public static void FilterMappedRegion(MappedItem item)
    {
      //deal with the item with multiple regions but one of them contains others
      if (item.MappedRegions.Count > 1)
      {
        var removed = new List<SequenceRegionMapped>();
        for (int i = 0; i < item.MappedRegions.Count; i++)
        {
          var regi = item.MappedRegions[i];
          for (int j = i + 1; j < item.MappedRegions.Count; j++)
          {
            var regj = item.MappedRegions[j];
            if (removed.Contains(regj))
            {
              continue;
            }

            var con = regi.Region.Contains(regj.Region);
            if (con == 1)
            {
              //if i contains j and all mapped reads mapped to j, remove i
              if (regi.AlignedLocations.All(m => regj.AlignedLocations.Contains(m)))
              {
                removed.Add(regi);
                break;
              }

              //if i contains j and all mapped reads from j were contained in i, remove j 
              if (regj.AlignedLocations.All(m => regi.AlignedLocations.Contains(m)))
              {
                removed.Add(regj);
                continue;
              }
            }
            else if (con == -1)
            {
              //if j contains i and all mapped reads mapped to i, remove j
              if (regj.AlignedLocations.All(m => regi.AlignedLocations.Contains(m)))
              {
                removed.Add(regj);
                continue;
              }

              //if j contains i and all mapped reads from i were contained in j, remove i 
              if (regi.AlignedLocations.All(m => regj.AlignedLocations.Contains(m)))
              {
                removed.Add(regi);
                break;
              }
            }
          }
        }

        removed.ForEach(m => m.AlignedLocations.ForEach(l => l.Features.Remove(m.Region)));

        item.MappedRegions.RemoveAll(m => removed.Contains(m));
      }
    }
  }
}
