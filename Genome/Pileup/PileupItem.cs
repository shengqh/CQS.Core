using System;
using System.Collections.Generic;
using System.Linq;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItem
  {
    /// <summary>
    ///   Bases at that position from aligned reads
    /// </summary>
    private readonly List<PileupBaseList> _samples = new List<PileupBaseList>();

    private char _nucleotide;

    /// <summary>
    ///   Sequence identifier
    /// </summary>
    public string SequenceIdentifier { get; set; }

    /// <summary>
    ///   Position in sequence (starting from 1)
    /// </summary>
    public long Position { get; set; }

    public string UpperNucleotide { get; private set; }

    /// <summary>
    ///   Nucleotide at that position
    /// </summary>
    public char Nucleotide
    {
      get { return _nucleotide; }
      set
      {
        _nucleotide = value;
        UpperNucleotide = value.ToString().ToUpper();
      }
    }

    public List<PileupBaseList> Samples
    {
      get { return _samples; }
    }

    /// <summary>
    ///   How many event type in current entry
    /// </summary>
    /// <returns>number of event type</returns>
    public int GetEventNumber(int minimumMappingQuality)
    {
      return (from s in Samples
              from gi in s
              where gi.PassScoreFilter(minimumMappingQuality)
              select gi.Event).Distinct().Count();
    }

    public PileupItem CloneByFilter(Func<PileupBase, bool> accept)
    {
      var result = new PileupItem();
      foreach (var sample in Samples)
      {
        var cloneSample = new PileupBaseList();
        cloneSample.SampleName = sample.SampleName;
        cloneSample.AddRange(from s in sample where accept(s) select s);
        result.Samples.Add(cloneSample);
      }
      return result;
    }

    public bool OnlyOneEvent()
    {
      string lastEvent = null;
      foreach (var s in Samples)
      {
        foreach (var b in s)
        {
          if (!b.Event.Equals(lastEvent))
          {
            if (lastEvent == null)
            {
              lastEvent = b.Event;
            }
            else
            {
              return false;
            }
          }
        }
      }

      return true;
    }

    public FisherExactTestResult InitializeTable()
    {
      return InitializeTable(GetPairedEvent());
    }

    public FisherExactTestResult InitializeTable(PairedEvent events)
    {
      var result = new FisherExactTestResult
      {
        Sample1 = { Name = Samples[0].SampleName },
        Sample2 = { Name = Samples[1].SampleName },
        SucceedName = events.MajorEvent,
        FailedName = events.MinorEvent
      };

      PrepareCount(result.Sample1, events, _samples[0]);
      PrepareCount(result.Sample2, events, _samples[1]);

      return result;
    }

    public static void PrepareCount(FisherExactTestResult.Sample sample, PairedEvent events, PileupBaseList bases)
    {
      foreach (var b in bases)
      {
        if (b.Event.Equals(events.MajorEvent))
        {
          sample.Succeed++;
        }
        else if (b.Event.Equals(events.MinorEvent))
        {
          sample.Failed++;
        }
      }
    }

    public PairedEvent GetPairedEvent()
    {
      if (_samples.Count == 0)
      {
        throw new Exception("No sample defined in PileupItem!");
      }

      _samples.ForEach(m =>
      {
        if (m.EventCountList == null)
        {
          m.InitEventCountList();
        }
      });

      string majorEvent = string.Empty;
      string minorEvent = string.Empty;
      if (_samples.Count == 1)
      {
        //both major and minor are defined by the only sample
        var sampleEvents = _samples[0].EventCountList;
        if (sampleEvents.Count > 1)
        {
          majorEvent = sampleEvents[0].Event;
          minorEvent = sampleEvents[1].Event;
        }
        else if (sampleEvents.Count > 0)
        {
          majorEvent = sampleEvents[0].Event;
        }
      }
      else
      {
        //major are defined by normal sample
        var sampleEvents = _samples[0].EventCountList;
        if (sampleEvents.Count > 0)
        {
          majorEvent = sampleEvents[0].Event;
        }

        //minor are defined by tumor sample
        var tumorEvents = _samples[1].EventCountList;
        foreach (var e in tumorEvents)
        {
          if (!e.Event.Equals(majorEvent))
          {
            if (majorEvent.Equals(string.Empty))
            {
              majorEvent = e.Event;
            }
            else
            {
              minorEvent = e.Event;
              break;
            }
          }
        }
      }

      return new PairedEvent(majorEvent, minorEvent);
    }
  }
}