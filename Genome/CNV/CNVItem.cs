using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;

namespace CQS.Genome.CNV
{
  public class CNVItem : IChromosomeRegion
  {
    public CNVItem()
    {
      ItemType = CNVType.UNKNOWN;
      Chrom = string.Empty;
      ItemName = string.Empty;
      FileName = string.Empty;
      Annotation = string.Empty;
      Score = 1.0;
    }

    public string Chrom { get; set; }

    public long ChromStart { get; set; }

    public long ChromEnd { get; set; }

    public string ItemName { get; set; }

    public string FileName { get; set; }

    public CNVType ItemType { get; set; }

    public virtual double Score { get; set; }

    public string Annotation { get; set; }

    public long Length
    {
      get
      {
        return this.ChromEnd - this.ChromStart + 1;
      }
    }
  }

  public static class CNVItemUtils
  {
    public static Action<string, CNVItem> FuncChrom = (m, n) => n.Chrom = m;
    public static Action<string, CNVItem> FuncChromStart = (m, n) => n.ChromStart = long.Parse(m);
    public static Action<string, CNVItem> FuncChromEnd = (m, n) => n.ChromEnd = long.Parse(m);
    public static Action<string, CNVItem> FuncItemName = (m, n) => n.ItemName = m;
    public static Action<string, CNVItem> FuncFileName = (m, n) => n.FileName = m;
    public static Action<string, CNVItem> FuncItemType = (m, n) => n.ItemType = EnumUtils.StringToEnum(m, CNVType.DUPLICATION);
    public static Action<string, CNVItem> FuncPValue = (m, n) => n.Score = double.Parse(m);
    public static Action<string, CNVItem> FuncAnnotation = (m, n) => n.Annotation = m;
    public static Action<string, CNVItem> FuncNothing = (m, n) => { };
  }
}
