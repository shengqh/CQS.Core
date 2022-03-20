using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Feature
{
  public static class FeatureItemUtils
  {
    public static void RemoveByLocation(this List<FeatureItemGroup> groups, Func<FeatureLocation, bool> removeFilter)
    {
      foreach (var group in groups)
      {
        foreach (var item in group)
        {
          item.Locations.RemoveAll(l => removeFilter(l));
        }
        group.RemoveAll(l => l.Locations.Count == 0);
      }
      groups.RemoveAll(l => l.Count == 0);
    }

    public static void CombineLocationByMappedReads(this FeatureItem item)
    {
      //deal with the item with multiple regions but one of them contains others
      if (item.Locations.Count > 1)
      {
        var removed = new List<FeatureLocation>();
        for (int i = 0; i < item.Locations.Count; i++)
        {
          var regi = item.Locations[i];
          for (int j = i + 1; j < item.Locations.Count; j++)
          {
            var regj = item.Locations[j];
            if (removed.Contains(regj))
            {
              continue;
            }

            var con = regi.Contains(regj);

            //Keep the small one if two ranges mapped by same reads
            if (con == 1)
            {
              //if i contains j and all mapped reads mapped to j, remove i
              if (regi.SamLocations.All(m => regj.SamLocations.Any(l => l.SamLocation == m.SamLocation)))
              {
                removed.Add(regi);
                break;
              }

              //if i contains j and all mapped reads from j were contained in i, remove j 
              if (regj.SamLocations.All(m => regi.SamLocations.Any(l => l.SamLocation == m.SamLocation)))
              {
                removed.Add(regj);
                continue;
              }
            }
            else if (con == -1)
            {
              //if j contains i and all mapped reads mapped to i, remove j
              if (regj.SamLocations.All(m => regi.SamLocations.Any(l => l.SamLocation == m.SamLocation)))
              {
                removed.Add(regj);
                continue;
              }

              //if j contains i and all mapped reads from i were contained in j, remove i 
              if (regi.SamLocations.All(m => regj.SamLocations.Any(l => l.SamLocation == m.SamLocation)))
              {
                removed.Add(regi);
                break;
              }
            }
          }
        }

        foreach (var floc in removed)
        {
          foreach (var sloc in floc.SamLocations)
          {
            sloc.SamLocation.Parent.RemoveLocation(sloc.SamLocation);
          }
        }

        item.Locations.RemoveAll(m => removed.Contains(m));
      }
    }
  }
}
