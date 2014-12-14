using CQS.Genome.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mapping
{
  public class MappedCountItem
  {
    public string ItemName { get; set; }

    public Dictionary<string, FeatureItemGroup> NameGroupMap { get; set; }

    public HashSet<string> DisplayNames { get; set; }

    public void InitializeNames()
    {
      this.DisplayNames = new HashSet<string>(from d in NameGroupMap.Values
                                              select d.DisplayName);
    }

    public Dictionary<string, FeatureItemGroup> DisplayNameGroupMap { get; set; }

    public void InitializeDisplayNameMap()
    {
      this.DisplayNameGroupMap = NameGroupMap.Values.ToDictionary(m => m.DisplayName);
    }
  }

  public class MappedCountItemList : List<MappedCountItem>
  {
    /// <summary>
    /// Parse data based on options.
    /// </summary>
    /// <param name="options">options</param>
    /// <param name="sort">sort function used to sort features</param>
    /// <returns>Sorted feature names</returns>
    public List<string> Parse(SimpleDataTableBuilderOptions options, Action<List<string>> sort)
    {
      Clear();

      var format = new FeatureItemGroupXmlFormat();

      this.AddRange(from file in options.GetCountFiles()
                    let xmlfile = file.File.EndsWith(".xml") ? file.File : file.File + ".mapped.xml"
                    select new MappedCountItem()
                    {
                      ItemName = file.Name,
                      NameGroupMap = (from m in format.ReadFromFile(xmlfile)
                                      from n in m
                                      select new FeatureItemGroup(n)).ToDictionary(m => m.Name)
                    });

      var names = (from countItem in this
                   from featureName in countItem.NameGroupMap.Keys
                   select featureName).Distinct().ToList();

      sort(names);

      Console.WriteLine("merging subjects with identical queries...");
      for (int i = names.Count - 1; i >= 0; i--)
      {
        for (int j = i - 1; j >= 0; j--)
        {
          if (this.All(m => HasSameQuery(m.NameGroupMap, names[j], names[i])))
          {
            this.ForEach(m => MergeGroup(m.NameGroupMap, names[j], names[i]));
            Console.WriteLine("Merge " + names[i] + " into " + names[j]);
          }
        }
      }
      Console.WriteLine("merging subjects with identical queries done.");

      this.ForEach(m => m.InitializeDisplayNameMap());

      var result = (from m in this
                    from n in m.DisplayNameGroupMap.Keys
                    select n).Distinct().ToList();

      sort(result);

      return result;
    }

    private void MergeGroup(Dictionary<string, FeatureItemGroup> m, string namej, string namei)
    {
      var gi = m[namei];
      var gj = m[namej];
      gj.AddRange(gi);
      m.Remove(namei);
    }

    private bool HasSameQuery(Dictionary<string, FeatureItemGroup> m, string namej, string namei)
    {
      if (m.ContainsKey(namei) && m.ContainsKey(namej))
      {
        var gi = m[namei];
        var gj = m[namej];

        if (gi.QueryNames.Equals(gj.QueryNames))
        {
          return true;
        }
      }

      return false;
    }
  }
}
