using RCPA;
using System;

namespace CQS.Genome.CNV
{
  public class CNVItem : SequenceRegion
  {
    public CNVItem()
    {
      ItemType = CNVType.UNKNOWN;
      Seqname = string.Empty;
      ItemName = string.Empty;
      FileName = string.Empty;
      Annotation = string.Empty;
      Score = 1.0;
    }

    public string ItemName { get; set; }

    public string FileName { get; set; }

    public CNVType ItemType { get; set; }

    public virtual double Score { get; set; }

    public string Annotation { get; set; }
  }

  public static class CNVItemUtils
  {
    public static Action<string, CNVItem> FuncChrom = (m, n) => n.Seqname = m;
    public static Action<string, CNVItem> FuncChromStart = (m, n) => n.Start = long.Parse(m);
    public static Action<string, CNVItem> FuncChromEnd = (m, n) => n.End = long.Parse(m);
    public static Action<string, CNVItem> FuncItemName = (m, n) => n.ItemName = m;
    public static Action<string, CNVItem> FuncFileName = (m, n) => n.FileName = m;
    public static Action<string, CNVItem> FuncItemType = (m, n) =>
    {
      var enums = EnumUtils.EnumToStringArray<CNVType>();
      var um = m.ToUpper();
      foreach (var e in enums)
      {
        if (e.StartsWith(um))
        {
          n.ItemType = EnumUtils.StringToEnum(e, CNVType.UNKNOWN);
          return;
        }
      }

      throw new Exception("Unknow CNV type " + m);
    };
    public static Action<string, CNVItem> FuncPValue = (m, n) => n.Score = double.Parse(m);
    public static Action<string, CNVItem> FuncAnnotation = (m, n) => n.Annotation = m;
    public static Action<string, CNVItem> FuncNothing = (m, n) => { };
  }
}
