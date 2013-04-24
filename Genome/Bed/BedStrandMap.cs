using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Bed
{
  public class BedStrandMap<T> : Dictionary<char, List<T>> where T : BedItem, new()
  {
    public void AddItem(T item)
    {
      var items = FindBedItems(item.Strand);
      if (items == null)
      {
        items = new List<T>();
        this[item.Strand] = items;
      }
      items.Add(item);
    }

    public List<T> FindBedItems(char strand)
    {
      if (this.ContainsKey(strand))
      {
        return this[strand];
      }

      return null;
    }
  }
}
