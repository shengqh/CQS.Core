using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountMap :CountMap
  {
    public SmallRNACountMap() {}

    public SmallRNACountMap(string countFile):base(countFile){}

    protected override void ReadCountFile(string countFile)
    {
      base.ReadCountFile(countFile);
      var list = Counts.ToList();
      foreach (var l in list)
      {
        Counts[l.Key.StringBefore(SmallRNAConsts.NTA_TAG)] = l.Value;
      }
    }

    public override int GetTotalCount()
    {
      return (from c in Counts
              where !c.Key.Contains(SmallRNAConsts.NTA_TAG)
              select c.Value).Sum();
    }
  }
}
