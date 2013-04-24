using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;

namespace CQS.ChipSeq
{
  public class OverlappedChipSeqComparisonItemBuilder : AbstractThreadFileProcessor
  {
    private List<string> sourceFiles;
    private Dictionary<string, string> filenameMap;
    private Dictionary<string, string> filenameGroup;

    public OverlappedChipSeqComparisonItemBuilder(IEnumerable<string> sourceFiles, Dictionary<string, string> filenameMap, Dictionary<string, string> filenameGroup)
    {
      this.sourceFiles = sourceFiles.ToList();
      this.filenameMap = filenameMap;
      this.filenameGroup = filenameGroup;
    }

    public override IEnumerable<string> Process(string fileName)
    {
      var curResult = ChipSeqItemUtils.ReadItems(sourceFiles);
      var curComp = new Dictionary<string, List<OverlappedChipSeqComparisonItem>>();

      foreach (var res in curResult)
      {
        curComp[res.Key] = (from r in res.Value
                            select OverlappedChipSeqComparisonItem.Build(r, filenameGroup)).ToList();
      }

      foreach (var g in curComp.Values)
      {
        foreach (var r in g)
        {
          foreach (var item in r.ItemMap.Values)
          {
            item.CalculateFields();
            item.InitializeDetails(filenameMap);
          }
        }
      }

      var olist = (from g in curComp.Values
                   from r in g
                   select r).ToList();

      new OverlappedChipSeqComparisonItemFormat().WriteToFile(fileName, olist);

      return new string[] { fileName };
    }
  }
}
