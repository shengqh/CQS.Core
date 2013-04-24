using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS.Genome.CNV
{
  public class FreecReader : AbstractTableFile<CNVItem>
  {
    protected override Dictionary<int, Action<string, CNVItem>> GetIndexActionMap()
    {
      Dictionary<int, Action<string, CNVItem>> result = new Dictionary<int, Action<string, CNVItem>>();

      result[0] = CNVItemUtils.FuncChrom;
      result[1] = CNVItemUtils.FuncChromStart;
      result[2] = CNVItemUtils.FuncChromEnd;
      result[3] = CNVItemUtils.FuncPValue;
      result[4] = (m, n) =>
      {
        if (m.Equals("gain"))
        {
          n.ItemType = CNVType.DUPLICATION;
        }
        else
        {
          n.ItemType = CNVType.DELETION;
        }
      };

      return result;
    }
  }
}
