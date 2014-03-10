using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public enum StrandType { UNKNOWN, FORWARD, REVERSE };
  public enum AlignedEventType { UNKNOWN, MATCH, MISMATCH, INSERTION, DELETION };
  public enum PositionType { UNKNOWN, START, MIDDLE, END };

  public class PileupBase
  {
    public PileupBase()
    {
      this.Strand = StrandType.UNKNOWN;
      this.EventType = AlignedEventType.UNKNOWN;
      this.Position = PositionType.UNKNOWN;
      this.Event = string.Empty;
      this.Score = 0;
      this.ReadMappingQuality = 0;
    }

    /// <summary>
    /// StrandType { UNKNOWN, FORWARD, REVERSE }
    /// </summary>
    public StrandType Strand { get; set; }

    /// <summary>
    /// EventType { UNKNOWN, MATCH, MISMATCH, INSERTION, DELETION }
    /// </summary>
    public AlignedEventType EventType { get; set; }

    /// <summary>
    /// PositionType { UNKNOWN, START, MIDDLE, END }
    /// </summary>
    public PositionType Position { get; set; }

    public string Event { get; set; }

    public int Score { get; set; }

    public int ReadMappingQuality { get; set; }

    public bool PassScoreFilter(int minimumMappingQuality)
    {
      return this.Score >= minimumMappingQuality ||
        this.EventType == Pileup.AlignedEventType.INSERTION ||
        this.EventType == Pileup.AlignedEventType.DELETION;
    }
  }

  public class PileupBaseList : List<PileupBase>
  {
    public string SampleName { get; set; }

    public List<EventCount> GetEventCountList(int minimumMappingQuality)
    {
      var result = new List<EventCount>();
      var map = new Dictionary<string, EventCount>();
      foreach (var e in this)
      {
        if (!e.PassScoreFilter(minimumMappingQuality))
        {
          continue;
        }

        EventCount ec;
        if (map.TryGetValue(e.Event, out ec))
        {
          ec.Count++;
        }
        else
        {
          ec = new EventCount(e.Event, 1);
          map[e.Event] = ec;
        }
      }

      result = (from ec in map.Values
                orderby ec.Count descending
                select ec).ToList();

      return result;
    }
  }

  public static class PileupBaseExtension
  {
    public static List<EventCount> GetEventCountList(this IEnumerable<PileupBase> bases)
    {
      var map = new Dictionary<string, EventCount>();
      foreach (var e in bases)
      {
        EventCount ec;
        if (map.TryGetValue(e.Event, out ec))
        {
          ec.Count++;
        }
        else
        {
          ec = new EventCount(e.Event, 1);
          map[e.Event] = ec;
        }
      }

      return (from ec in map.Values
              orderby ec.Count descending
              select ec).ToList();
    }

    public static List<EventCount> GetEventCountList(this IEnumerable<PileupBase> bases, int minimumMappingQuality)
    {
      IEnumerable<PileupBase> filtered;
      if (minimumMappingQuality == 0)
      {
        filtered = bases;
      }
      else
      {
        filtered = from b in bases
                   where b.PassScoreFilter(minimumMappingQuality)
                   select b;
      }

      return GetEventCountList(filtered);
    }

  }
}
