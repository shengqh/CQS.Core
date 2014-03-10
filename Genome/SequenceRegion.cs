using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome
{
  public class SequenceRegion : ISequenceRegion, IComparable<SequenceRegion>
  {
    public SequenceRegion()
    {
      this.Seqname = string.Empty;
      this.Start = 0;
      this.End = 0;
      this.Name = string.Empty;
      this.Strand = '+';
      this.Sequence = string.Empty;
    }

    public string Seqname { get; set; }

    public long Start { get; set; }

    public long End { get; set; }

    public string Name { get; set; }

    public char Strand { get; set; }

    public virtual long Length
    {
      get
      {
        return this.End - this.Start + 1;
      }
    }

    public int CompareTo(SequenceRegion other)
    {
      var result = this.Seqname.CompareTo(other.Seqname);
      if (0 == result)
      {
        result = this.Start.CompareTo(other.Start);
      }
      return result;
    }

    public bool Contains(long position)
    {
      return position >= this.Start && position <= this.End;
    }

    public string Sequence { get; set; }
  }
}
