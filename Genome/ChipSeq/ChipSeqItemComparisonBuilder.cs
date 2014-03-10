using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;

namespace CQS.Genome.ChipSeq
{
  public class ChipSeqItemComparisonBuilder : AbstractThreadFileProcessor
  {
    private List<string> sourceFiles;

    private Dictionary<string, string> filenameMap;

    public ChipSeqItemComparisonBuilder(IEnumerable<string> sourceFiles, Dictionary<string,string> filenameMap)
    {
      this.sourceFiles = sourceFiles.ToList();
      this.filenameMap = filenameMap;
    }

    public override IEnumerable<string> Process(string fileName)
    {
      var curResult = ChipSeqItemUtils.ReadItems(sourceFiles);

      foreach (var g in curResult.Values)
      {
        g.ForEach(m =>
        {
          m.CalculateFields();
          m.InitializeDetails(filenameMap);
        });
        g.Sort((m1, m2) => m2.Ratio.CompareTo(m1.Ratio));
      }

      var ocsList = (from g in curResult.Values
                     orderby g.Max(m => m.Ratio) descending
                     from o in g
                     select o).ToList();

      new OverlappedChipSeqItemFormat().WriteToFile(fileName, ocsList);

      return new string[] { fileName };
    }
  }
}
