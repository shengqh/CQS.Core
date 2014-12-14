using CQS.Genome.Sam;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Feature
{
  public class FeatureItem
  {
    public FeatureItem()
    {
      Mapped = new List<FeatureLocation>();
      Name = string.Empty;
    }

    public List<FeatureLocation> Mapped { get; private set; }

    public string Name { get; set; }

    public double EstimateCount
    {
      get { return GetEstimateCount(m => true); }
    }

    public double GetEstimateCount(Func<SamAlignedLocation, bool> func)
    {
      return Mapped.Sum(m => m.GetEstimatedCount(func));
    }

    public int QueryCount
    {
      get
      {
        return (from region in Mapped
                from loc in region.SamLocations
                select loc.SamLocation.Parent).Distinct().Sum(m => m.QueryCount);
      }
    }

    public string Locations
    {
      get
      {
        return (from loc in Mapped
                select loc.GetLocation()).Merge(",");
      }
    }

    public string Sequence
    {
      get
      {
        return (from loc in Mapped
                select loc.Sequence).Merge(",");
      }
    }

    public override string ToString()
    {
      return Name;
    }

    public void FilterLocations(HashSet<long> allowedOffsets)
    {
      foreach (var region in Mapped)
      {
        region.SamLocations.RemoveAll(loc =>
        {
          return !allowedOffsets.Contains(loc.Offset);
        });
      }

      Mapped.RemoveAll(region => region.SamLocations.Count == 0);
    }

    public void CombineLocations()
    {
      //deal with the item with multiple regions but one of them contains others
      if (this.Mapped.Count > 1)
      {
        var removed = new List<FeatureLocation>();
        for (int i = 0; i < this.Mapped.Count; i++)
        {
          var regi = this.Mapped[i];
          for (int j = i + 1; j < this.Mapped.Count; j++)
          {
            var regj = this.Mapped[j];
            if (removed.Contains(regj))
            {
              continue;
            }

            var con = regi.Contains(regj);
            if (con == 1)
            {
              //if i contains j and all mapped reads mapped to j, remove i. Small range is perfered.
              if (regj.ContainLocations(regi))
              {
                removed.Add(regi);
                break;
              }

              //if i contains j and all mapped reads from j were contained in i, remove j 
              if (regj.ContainLocations(regi))
              {
                removed.Add(regj);
                continue;
              }
            }
            else if (con == -1)
            {
              //if j contains i and all mapped reads mapped to i, remove j. Small range is perfered.
              if (regi.ContainLocations(regj))
              {
                removed.Add(regj);
                continue;
              }

              //if j contains i and all mapped reads from i were contained in j, remove i 
              if (regj.ContainLocations(regi))
              {
                removed.Add(regi);
                break;
              }
            }
          }
        }

        //remove the feature from SAMAlignedItem feature list.
        removed.ForEach(m => m.SamLocations.ForEach(l => l.SamLocation.Features.Remove(m)));

        this.Mapped.RemoveAll(m => removed.Contains(m));
      }
    }
  }

  public static class FeatureItemExtension
  {
    public static List<FeatureItemGroup> GroupByIdenticalQuery(this List<FeatureItem> items)
    {
      var dic = items.GroupBy(m => (from r in m.Mapped
                                    from l in r.SamLocations
                                    select l.SamLocation.Parent.Qname).Distinct().OrderBy(l => l).Merge(";")).ToList();
      var result = new List<FeatureItemGroup>();
      foreach (var curItems in dic)
      {
        var group = new FeatureItemGroup();
        group.AddRange(from item in curItems orderby item.Name select item);
        result.Add(group);
      }

      return result;
    }
  }
}