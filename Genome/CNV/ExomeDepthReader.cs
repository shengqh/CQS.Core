using RCPA;
using System;
using System.Collections.Generic;

namespace CQS.Genome.CNV
{
  public class ExomeDepthReader : AbstractHeaderFile<CNVItem>
  {
    public ExomeDepthReader() : base() { }

    public ExomeDepthReader(string filename) : base(filename) { }

    private static Action<string, CNVItem> FuncState = (m, n) =>
    {
      n.ItemType = EnumUtils.StringToEnum<CNVType>(m.ToUpper(), CNVType.UNKNOWN);
    };

    protected override Dictionary<string, Action<string, CNVItem>> GetHeaderActionMap()
    {
      var result = new Dictionary<string, Action<string, CNVItem>>();

      result["chromosome"] = CNVItemUtils.FuncChrom;
      result["start"] = CNVItemUtils.FuncChromStart;
      result["end"] = CNVItemUtils.FuncChromEnd;
      result["type"] = FuncState;

      return result;
    }
  }
}
