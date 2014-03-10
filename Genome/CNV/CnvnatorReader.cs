using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS.Genome.CNV
{
  public class CnvnatorReader : AbstractTableFile<CnvnatorItem>
  {
    private static Regex chrReg = new Regex(@"(?:chr){0,1}(.+):(\d+)-(\d+)");

    protected override Dictionary<int, Action<string, CnvnatorItem>> GetIndexActionMap()
    {
      var result = new Dictionary<int, Action<string, CnvnatorItem>>();
      result[0] = (m, n) =>
      {
        if (m.Equals("duplication"))
        {
          n.ItemType = CNVType.DUPLICATION;
        }
        else if (m.Equals("deletion"))
        {
          n.ItemType = CNVType.DELETION;
        }
        else
        {
          throw new ArgumentException(string.Format("Unknown CNV type : {0}", m));
        }
      };

      result[1] = (m, n) =>
      {
        var match = chrReg.Match(m);
        if (!match.Success)
        {
          throw new ArgumentException(string.Format("Unknown chromosome definition : {0}", m));
        }
        n.Seqname = match.Groups[1].Value;
        n.Start = long.Parse(match.Groups[2].Value);
        n.End = long.Parse(match.Groups[3].Value);
      };

      result[2] = CNVItemUtils.FuncNothing;
      result[3] = CNVItemUtils.FuncNothing;
      result[4] = (m, n) => n.PValue1 = double.Parse(m);
      result[5] = (m, n) => n.PValue2 = double.Parse(m);
      result[6] = (m, n) => n.PValue3 = double.Parse(m);
      result[7] = (m, n) => n.PValue4 = double.Parse(m);
      result[8] = (m, n) => n.Q0 = double.Parse(m);

      return result;
    }
  }
}
