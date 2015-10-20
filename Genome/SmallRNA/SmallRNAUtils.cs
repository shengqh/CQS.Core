using CQS.Genome.Sam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.Genome.SmallRNA
{
  public static class SmallRNAUtils
  {
    public static void AssignOriginalName(this IEnumerable<SAMAlignedItem> items)
    {
      foreach (var item in items)
      {
        item.OriginalQname = item.Qname.StringBefore(SmallRNAConsts.NTA_TAG);
      }
    }
  }
}
