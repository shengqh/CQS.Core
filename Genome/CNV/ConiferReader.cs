using System;
using System.Collections.Generic;

namespace CQS.Genome.CNV
{
  public class ConiferReader : AbstractHeaderFile<CNVItem>
  {
    public ConiferReader() : base() { }

    public ConiferReader(string filename) : base(filename) { }

    private static Action<string, CNVItem> FuncState = (m, n) =>
    {
      if (m.Equals("dup"))
      {
        n.ItemType = CNVType.DUPLICATION;
      }
      else if (m.Equals("del"))
      {
        n.ItemType = CNVType.DELETION;
      }
      else
      {
        throw new ArgumentException(string.Format("Unknown CNV type : {0}", m));
      }
    };

    protected override Dictionary<string, Action<string, CNVItem>> GetHeaderActionMap()
    {
      var result = new Dictionary<string, Action<string, CNVItem>>();

      result["sampleID"] = CNVItemUtils.FuncFileName;
      result["chromosome"] = CNVItemUtils.FuncChrom;
      result["start"] = CNVItemUtils.FuncChromStart;
      result["stop"] = CNVItemUtils.FuncChromEnd;
      result["state"] = FuncState;

      return result;
    }
  }
}
