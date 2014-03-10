using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.ChipSeq
{
  public class OverlappedChipSeqComparisonItem
  {
    private Dictionary<string, OverlappedChipSeqItem> _itemMap = new Dictionary<string, OverlappedChipSeqItem>();

    public Dictionary<string, OverlappedChipSeqItem> ItemMap
    {
      get
      {
        return _itemMap;
      }
    }

    public string GeneSymbol
    {
      get
      {
        if (this.ItemMap.Count == 0)
        {
          return string.Empty;
        }

        return this.ItemMap.First().Value.GeneSymbol;
      }
    }

    public string Chromosome
    {
      get
      {
        if (this.ItemMap.Count == 0)
        {
          return string.Empty;
        }

        return this.ItemMap.First().Value.Chromosome;
      }
    }

    public void AddItem(string key, ChipSeqItem item)
    {
      if (!_itemMap.ContainsKey(key))
      {
        _itemMap[key] = new OverlappedChipSeqItem();
      }

      _itemMap[key].Add(item);
    }

    public static List<string> GetKeys(List<OverlappedChipSeqComparisonItem> items)
    {
      return (from item in items
              from key in item.ItemMap.Keys
              select key).Distinct().OrderBy(m => m).ToList();
    }

    public static OverlappedChipSeqComparisonItem Build(OverlappedChipSeqItem item, Dictionary<string, string> fileNameGroup)
    {
      var result = new OverlappedChipSeqComparisonItem();

      foreach (var csi in item)
      {
        var key = fileNameGroup[csi.Filename];
        result.AddItem(key, csi);
      }

      return result;
    }
  }
}
