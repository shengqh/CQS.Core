using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.CNV
{
  public class CnvnatorItem : CNVItem
  {
    public double PValue1 { get; set; }

    public double PValue2 { get; set; }

    public double PValue3 { get; set; }

    public double PValue4 { get; set; }

    public double Q0 { get; set; }

    public override double Score
    {
      get
      {
        return Math.Max(this.PValue1, Math.Max(this.PValue2, Math.Max(this.PValue3, this.PValue4)));
      }
      set
      {
        base.Score = value;
      }
    }
  }

  public static class CnvnatorItemExtension
  {
    public static void FilterByPvalue(this List<CnvnatorItem> items, double pvalue)
    {
      items.RemoveAll(m => m.Score > pvalue);
    }

    public static void FilterByQ0(this List<CnvnatorItem> items, double q0value)
    {
      items.RemoveAll(m => m.Q0 > q0value);
    }

    public static void FilterByLength(this List<CnvnatorItem> items, int len)
    {
      items.RemoveAll(m => m.Length < len);
    }
  }
}
