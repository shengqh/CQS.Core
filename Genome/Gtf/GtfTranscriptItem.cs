using System.Collections.Generic;

namespace CQS.Genome.Gtf
{
  public class GtfTranscriptItem : List<GtfItem>
  {
    public string Seqname
    {
      get
      {
        if (this.Count == 0)
        {
          return string.Empty;
        }
        else
        {
          return this[0].Seqname;
        }
      }
    }

    public string TranscriptId
    {
      get
      {
        if (this.Count == 0)
        {
          return string.Empty;
        }
        else
        {
          return this[0].TranscriptId;
        }
      }
    }

    public char Strand
    {
      get
      {
        if (this.Count == 0)
        {
          return '.';
        }
        else
        {
          return this[0].Strand;
        }
      }
    }

    public long Start
    {
      get
      {
        if (this.Count == 0)
        {
          return -1;
        }

        if (this.Strand == '+')
        {
          return this[0].Start;
        }
        else if (this.Strand == '-')
        {
          return this[0].End;
        }
        else
        {
          return -1;
        }
      }
    }

    public void SortByLocation()
    {
      if (this.Count == 0)
      {
        return;
      }

      if (this.Strand == '+')
      {
        this.Sort((m1, m2) => m1.Start.CompareTo(m2.Start));
      }
      else if (this.Strand == '-')
      {
        this.Sort((m1, m2) => m2.End.CompareTo(m1.End));
      }
    }

    public bool IsSameTranscript(GtfItem item)
    {
      if (this.Count == 0)
      {
        return true;
      }
      else
      {
        return this[0].IsSameTranscript(item);
      }
    }

    public int FindItemIndex(long start, long end)
    {
      if (this.Count == 0)
      {
        return -1;
      }

      if (this[0].Start > end)
      {
        return -1;
      }

      if (this[this.Count - 1].End < start)
      {
        return -1;
      }

      return this.FindIndex(m => m.InRange(start) || m.InRange(end));
    }
  }
}
