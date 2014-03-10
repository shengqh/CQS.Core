using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  /// <summary>
  /// Position information from multiple samples
  /// </summary>
  public class AlignedPositionMapSamples
  {
    /// <summary>
    /// Chromosome
    /// </summary>
    public string Chromosome
    {
      get
      {
        if (this.Samples.Count > 0)
        {
          return this.Samples[0].Chromosome;
        }
        return string.Empty;
      }
    }

    /// <summary>
    /// Position in sequence (starting from 1)
    /// </summary>
    public long Position
    {
      get
      {
        if (this.Samples.Count > 0)
        {
          return this.Samples[0].Position;
        }
        return 0;
      }
    }

    public char UpperReferenceAllele
    {
      get
      {
        if (this.Samples.Count > 0)
        {
          return Char.ToUpper(this.Samples[0].ReferenceAllele);
        }
        return ' ';
      }
    }

    /// <summary>
    /// Alleles at that position from aligned reads
    /// </summary>
    public List<AlignedPositionMap> Samples { get; private set; }

    /// <summary>
    /// How many event type in current entry
    /// </summary>
    /// <returns>number of event type</returns>
    public int GetEventNumber()
    {
      return (from s in this.Samples
              from allele in s.Keys
              select allele).Distinct().Count();
    }

    /// <summary>
    /// How many event type in current entry based on quality score
    /// </summary>
    /// <param name="minimumMappingQuality">minimum base mapping quality</param>
    /// <returns>number of event type</returns>
    public int GetEventNumber(char minimumMappingQuality)
    {
      return (from s in this.Samples
              from allele in s
              where allele.Value.Any(n => n.Score >= minimumMappingQuality)
              select allele.Key).Distinct().Count();
    }

    private string GetFirstEvent()
    {
      foreach (var s in this.Samples)
      {
        foreach (var b in s)
        {
          return b.Key;
        }
      }
      return null;
    }

    /// <summary>
    /// Check if only one event in this position
    /// </summary>
    /// <returns></returns>
    public bool OnlyOneEvent()
    {
      string firstEvent = GetFirstEvent();
      if (firstEvent == null)
      {
        return true;
      }

      foreach (var s in this.Samples)
      {
        foreach (var b in s)
        {
          if (!b.Key.Equals(firstEvent))
          {
            return false;
          }
        }
      }

      return true;
    }

    private string GetFirstEvent(char minimumMappingQuality)
    {
      foreach (var s in this.Samples)
      {
        foreach (var b in s)
        {
          if (b.Value.Any(m => m.Score >= minimumMappingQuality))
          {
            return b.Key;
          }
        }
      }
      return null;
    }

    /// <summary>
    /// Check if only one event in this position with base quality limitation
    /// </summary>
    /// <returns></returns>
    public bool OnlyOneEvent(char minimumMappingQuality)
    {
      string firstEvent = GetFirstEvent(minimumMappingQuality);
      if (firstEvent == null)
      {
        return true;
      }

      foreach (var s in this.Samples)
      {
        foreach (var b in s)
        {
          if (b.Value.Any(m => m.Score >= minimumMappingQuality) && !b.Key.Equals(firstEvent))
          {
            return false;
          }
        }
      }

      return true;
    }

    //public FisherExactTestResult InitializeTable()
    //{
    //  return InitializeTable(this.GetPairedEvent());
    //}

    //public FisherExactTestResult InitializeTable(PairedEvent events)
    //{
    //  FisherExactTestResult result = new FisherExactTestResult();
    //  result.Sample1.Name = this.Samples[0].SampleName;
    //  result.Sample2.Name = this.Samples[1].SampleName;

    //  result.SucceedName = events.MajorEvent;
    //  result.FailedName = events.MinorEvent;

    //  PrepareCount(result.Sample1, events, this.samples[0]);
    //  PrepareCount(result.Sample2, events, this.samples[1]);

    //  return result;
    //}

    //public static void PrepareCount(FisherExactTestResult.Sample sample, PairedEvent events, PileupBaseList bases)
    //{
    //  foreach (var b in bases)
    //  {
    //    if (b.Event.Equals(events.MajorEvent))
    //    {
    //      sample.Succeed++;
    //    }
    //    else if (b.Event.Equals(events.MinorEvent))
    //    {
    //      sample.Failed++;
    //    }
    //  }
    //}

    //public PairedEvent GetPairedEvent()
    //{
    //  if (this.samples.Count == 0)
    //  {
    //    throw new Exception("No sample defined in PileupItem!");
    //  }

    //  if (this.samples.Count == 1)
    //  {
    //    //both major and minor are defined by the only sample
    //    var sampleEvents = this.samples[0].GetEventCountList();
    //    if (sampleEvents.Count > 1)
    //    {
    //      return new PairedEvent(sampleEvents[0].Event, sampleEvents[1].Event);
    //    }
    //    else
    //    {
    //      return new PairedEvent(sampleEvents[0].Event, string.Empty);
    //    }
    //  }
    //  else
    //  {
    //    //major are defined by normal sample
    //    var sampleEvents = this.samples[0].GetEventCountList();
    //    var majorEvent = sampleEvents[0].Event;

    //    //minor are defined by tumor sample
    //    var tumorEvents = this.samples[1].GetEventCountList();
    //    foreach (var e in tumorEvents)
    //    {
    //      if (!e.Event.Equals(majorEvent))
    //      {
    //        return new PairedEvent(majorEvent, e.Event);
    //      }
    //    }
    //    return new PairedEvent(majorEvent, string.Empty);
    //  }
    //}
  }
}
