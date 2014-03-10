using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CQS.Genome
{
  public interface ISequenceRegion
  {
    string Seqname { get; set; }

    long Start { get; set; }

    long End { get; set; }

    string Name { get; set; }

    char Strand { get; set; }

    long Length { get; }

    bool Contains(long position);

    string Sequence { get; set; }
  }

  public static class SequenceRegionExtension
  {
    public static string GetDisplayName(this ISequenceRegion sr)
    {
      if (!string.IsNullOrEmpty(sr.Sequence))
      {
        return sr.Name + ":" + sr.Sequence;
      }
      else
      {
        return sr.Name;
      }
    }

    public static void ParseLocation(this ISequenceRegion loc, XElement locEle)
    {
      loc.Seqname = locEle.Attribute("seqname").Value;
      loc.Start = long.Parse(locEle.Attribute("start").Value);
      loc.End = long.Parse(locEle.Attribute("end").Value);
      loc.Strand = locEle.Attribute("strand").Value[0];
    }

    public static void UnionWith(this ISequenceRegion loc, ISequenceRegion item)
    {
      loc.Start = Math.Min(loc.Start, item.Start);
      loc.End = Math.Max(loc.End, item.End);
    }
  }
}
