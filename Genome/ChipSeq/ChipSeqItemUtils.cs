using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.ChipSeq
{
  public static class ChipSeqItemUtils
  {
    public static List<ChipSeqItem> ReadInGeneItem(string fileName)
    {
      return (from item in new ChipSeqItemFormat().ReadFromFile(fileName)
              where item.InGene
              select item).ToList();
    }

    public static Dictionary<string, List<OverlappedChipSeqItem>> ReadItems(List<string> sourceFiles)
    {
      if (sourceFiles.Count <= 2)
      {
        throw new ArgumentException("Input at least two data files first!");
      }

      var firstFile = ReadInGeneItem(sourceFiles[0]);

      var curResult = OverlappedChipSeqItem.Build(firstFile);

      for (int i = 1; i < sourceFiles.Count; i++)
      {
        var curFile = ReadInGeneItem(sourceFiles[i]);

        foreach (var item in curFile)
        {
          if (!curResult.ContainsKey(item.GeneSymbol))
          {
            var oi = new OverlappedChipSeqItem();
            oi.Add(item);
            var oil = new List<OverlappedChipSeqItem>();
            oil.Add(oi);
            curResult[item.GeneSymbol] = oil;
            continue;
          }

          var curGroup = curResult[item.GeneSymbol];
          OverlappedChipSeqItem.AddToList(curGroup, item);
        }
      }
      return curResult;
    }

  }
}
