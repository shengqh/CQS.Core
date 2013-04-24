using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Seq;

namespace CQS.Genome
{
  public class MatchExon : List<Location>, IComparable<MatchExon>
  {
    public int TranscriptCount { get; set; }
    public string TranscriptId { get; set; }
    public bool RetainedIntron { get; set; }
    public long IntronSize { get; set; }
    public string Sequence { get; set; }
    public string TranscriptType { get; set; }

    public bool ContainLocation(Location other)
    {
      foreach (var loc in this)
      {
        if (loc.Start <= other.Start && loc.End >= other.End)
        {
          return true;
        }
      }

      return false;
    }

    public bool ContainLocations(IEnumerable<Location> other)
    {
      if (this.Count < other.Count())
      {
        return false;
      }

      foreach (var loci in other)
      {
        if (!ContainLocation(loci))
        {
          return false;
        }
      }

      return true;
    }

    public bool EqualLocations(IEnumerable<Location> other)
    {
      return this.SequenceEqual(other);
    }

    public void FillSequence(Sequence seq, char strand)
    {
      StringBuilder sb = new StringBuilder();
      foreach (var loc in this)
      {
        sb.Append(seq.SeqString.Substring((int)loc.Start, (int)loc.Length));
      }
      this.Sequence = sb.ToString().ToUpper();

      if (strand == '-')
      {
        this.Sequence = SequenceUtils.ToAnotherStrand(this.Sequence);
      }
    }

    public int CompareTo(MatchExon other)
    {
      throw new NotImplementedException();
    }

    public MatchExon()
    {
      this.TranscriptCount = 0;
      this.TranscriptId = string.Empty;
      this.RetainedIntron = false;
      this.IntronSize = 0;
      this.Sequence = string.Empty;
      this.TranscriptType = string.Empty;
    }
  }
}
