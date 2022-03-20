using System;
using System.Collections.Generic;

namespace CQS.Genome.CNV
{
  public class ExomeCopyReader : AbstractHeaderFile<CNVItem>
  {
    public ExomeCopyReader() : base() { }

    public ExomeCopyReader(string filename) : base(filename) { }

    private static Action<string, CNVItem> FuncState = (m, n) =>
    {
      var copynumber = int.Parse(m);
      n.ItemType = copynumber > 2 ? CNVType.DUPLICATION : CNVType.DELETION;
    };

    protected override Dictionary<string, Action<string, CNVItem>> GetHeaderActionMap()
    {
      var result = new Dictionary<string, Action<string, CNVItem>>();

      result["sample.name"] = CNVItemUtils.FuncFileName;
      result["space"] = CNVItemUtils.FuncChrom;
      result["start"] = CNVItemUtils.FuncChromStart;
      result["end"] = CNVItemUtils.FuncChromEnd;
      result["copy.count"] = FuncState;

      return result;
    }
  }
}
