using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Bio.IO.SAM;
using System.Text.RegularExpressions;
using RCPA.Seq;

namespace CQS.Genome.Sam
{
  public class SamAlignedLocation : SequenceRegion
  {
    public SamAlignedLocation(SAMAlignedItem parent)
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

    public long Offset(ISequenceRegion region)
    {
      if (region.Strand == '+')
      {
        return this.Start - region.Start;
      }
      else
      {
        return this.End - region.End;
      }
    }

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

    private static Regex mismatch = new Regex(@"(\d+)(\S)");

    public SingleNucleotidePolymorphism GetMutation(string querySequence)
    {
      if (this.NumberOfMismatch == 0)
      {
        return null;
      }

      var isPositiveStrand = this.Strand == '+';
      var m = mismatch.Match(this.MismatchPositions);
      if (!m.Success)
      {
        return null;
      }

      var seq = isPositiveStrand ? querySequence : SequenceUtils.GetReversedSequence(querySequence);
      var pos = int.Parse(m.Groups[1].Value);
      var detectedChr = seq[pos];

      var chr = m.Groups[2].Value.First();
      chr = isPositiveStrand ? chr : SequenceUtils.GetComplementAllele(chr);

      return new SingleNucleotidePolymorphism(pos, chr, detectedChr);
    }
  }

  public static class SAMAlignedLocationExtension
  {
    public static Dictionary<string, SamAlignedLocation> ToSAMAlignedLocationMap(this XElement root)
    {
      var items = root.ToSAMAlignedItems();

      return (from item in items
              from loc in item.Locations
              select loc).ToDictionary(m => m.GetKey());
    }

    public static Dictionary<string, SAMAlignedItem> ToSAMAlignedItemMap(this XElement root)
    {
      return root.ToSAMAlignedItems().ToDictionary(m => m.Qname);
    }
  }
}
