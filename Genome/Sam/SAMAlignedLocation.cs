using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Bio.IO.SAM;

namespace CQS.Genome.Sam
{
  public class SAMAlignedLocation : SequenceRegion
  {
    public SAMAlignedLocation(SAMAlignedItem parent)
    {
      this.Features = new List<ISequenceRegion>();
      if (parent != null)
      {
        parent.AddLocation(this);
      }
    }

    public SAMAlignedItem Parent { get; set; }

    public SAMFlags Flag { get; set; }

    public int MapQ { get; set; }

    public string Cigar { get; set; }

    public string Qual { get; set; }

    /// <summary>
    /// Alignment score, parsed from option values
    /// </summary>
    public int AlignmentScore { get; set; }

    /// <summary>
    /// Number of mismatch, parsed from option values
    /// </summary>
    public int NumberOfMismatch { get; set; }

    /// <summary>
    /// Mismatch positions, parsed from option values
    /// </summary>
    public string MismatchPositions { get; set; }

    /// <summary>
    /// The features (such as gene or miRNA) that query sequence mapped to.
    /// </summary>
    public List<ISequenceRegion> Features { get; private set; }

    public virtual void ParseEnd(string sequence)
    {
      this.End = this.Start + sequence.Length - 1;
    }

    public static string GetKey(string qname, string loc)
    {
      return string.Format("{0}-{1}", qname, loc);
    }

    public string GetKey()
    {
      return GetKey(Parent.Qname, this.GetLocation());
    }
  }

  public static class SAMAlignedLocationExtension
  {
    public static Dictionary<string, SAMAlignedLocation> ToSAMAlignedLocationMap(this XElement root)
    {
      var items = root.ToSAMAlignedItems();

      //(from item in items
      // from loc in item.Locations
      // select loc).GroupBy(m => m.GetKey()).Where(m => m.Count() > 1).ToList().ForEach(m => Console.WriteLine(m.First().GetKey()));

      return (from item in items
              from loc in item.Locations
              select loc).ToDictionary(m => m.GetKey());
    }
  }
}
