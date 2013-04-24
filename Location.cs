using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS
{
  public class Location : IEquatable<Location>
  {
    public Location()
    {
      this.Start = -1;
      this.End = -1;
    }

    public Location(long start, long end)
    {
      this.Start = start;
      this.End = end;

      CheckStartEnd();
    }

    private void CheckStartEnd()
    {
      if (this.Start > this.End)
      {
        throw new ArgumentException(string.Format("start should be less/equal to end: start = {0}, end = {1}", this.Start, this.End));
      }
    }

    public Location(string loc)
    {
      var parts = loc.Split('-');

      if (parts.Length == 1)
      {
        int start;
        if (int.TryParse(parts[0], out start))
        {
          this.Start = start;
          this.End = start;
          CheckStartEnd();
          return;
        }
      }
      else if (parts.Length == 2)
      {
        int start, end;
        if (int.TryParse(parts[0], out start) && int.TryParse(parts[1], out end))
        {
          this.Start = start;
          this.End = end;
          CheckStartEnd();
          return;
        }
      }

      throw new ArgumentException(string.Format("Wrong location string {0}", loc));
    }

    public long Start { get; set; }

    public long End { get; set; }

    public long Length
    {
      get
      {
        if (this.Start == -1)
        {
          return 0;
        }

        return this.End - this.Start + 1;
      }
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Location))
      {
        return false;
      }

      return this.Equals(obj as Location);
    }

    public bool Equals(Location other)
    {
      if (null == other)
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return this.Start == other.Start && this.End == other.End;
    }

    public override int GetHashCode()
    {
      return this.Start.GetHashCode() ^ this.End.GetHashCode();
    }

    public override string ToString()
    {
      return string.Format("{0}-{1}", this.Start, this.End);
    }
  }
}
