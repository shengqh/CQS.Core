using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItem
  {
    /// <summary>
    /// Sequence identifier
    /// </summary>
    public string SequenceIdentifier { get; set; }

    /// <summary>
    /// Position in sequence (starting from 1)
    /// </summary>
    public long Position { get; set; }

    private string _upperNucleotide;
    public string UpperNucleotide
    {
      get
      {
        return _upperNucleotide;
      }
    }

    private char _nucleotide;
    /// <summary>
    /// Nucleotide at that position
    /// </summary>
    public char Nucleotide
    {
      get
      {
        return _nucleotide;
      }
      set
      {
        _nucleotide = value;
        _upperNucleotide = value.ToString().ToUpper();
      }
    }

    /// <summary>
    /// Bases at that position from aligned reads
    /// </summary>
    private List<PileupBaseList> samples = new List<PileupBaseList>();
    public List<PileupBaseList> Samples
    {
      get
      {
        return samples;
      }
    }

    /// <summary>
    /// How many event type in current entry
    /// </summary>
    /// <returns>number of event type</returns>
    public int GetEventNumber(int minimumMappingQuality)
    {
      return (from s in this.Samples
              from gi in s
              where gi.PassScoreFilter(minimumMappingQuality)
              select gi.Event).Distinct().Count();
    }

    public PileupItem CloneByFilter(Func<PileupBase, bool> accept)
    {
      PileupItem result = new PileupItem();
      foreach (var sample in this.Samples)
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
      foreach (var s in this.Samples)
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

    private class Item
    {
      public string SampleName { get; set; }
      public string EventName { get; set; }
      public int Count { get; set; }
    }

    private int GetCount(Dictionary<string, Dictionary<string, Item>> map, string sample, string eventName)
    {
      Dictionary<string, Item> dic;
      if (map.TryGetValue(sample, out dic))
      {
        Item item;
        if (dic.TryGetValue(eventName, out item))
        {
          return item.Count;
        }
      }

      return 0;
    }

    public FisherExactTestResult InitializeTable()
    {
      FisherExactTestResult result = new FisherExactTestResult();

      Dictionary<string, Dictionary<string, Item>> map = new Dictionary<string, Dictionary<string, Item>>();
      List<Item> items = new List<Item>();

      foreach (var s in this.Samples)
      {
        var dic = new Dictionary<string, Item>();
        map[s.SampleName] = dic;
        foreach (var b in s)
        {
          Item value;
          if (!dic.TryGetValue(b.Event, out value))
          {
            value = new Item()
            {
              SampleName = s.SampleName,
              EventName = b.Event,
              Count = 1
            };

            dic[b.Event] = value;
            items.Add(value);
          }
          else
          {
            value.Count++;
          }
        }
      }

      //major and minor alleles are defined by normal sample
      var allevents = this.samples[0].GetEventCountList();

      result.Name1 = this.Samples[0].SampleName;
      result.Name2 = this.Samples[1].SampleName;
      result.SucceedName = allevents[0].Event;
      if (allevents.Count > 1)
      {
        result.FailedName = allevents[1].Event;
      }
      else
      {
        allevents = this.samples[1].GetEventCountList();
        foreach (var e in allevents)
        {
          if (!e.Event.Equals(result.SucceedName))
          {
            result.FailedName = e.Event;
            break;
          }
        }
      }

      result.SucceedCount1 = GetCount(map, result.Name1, result.SucceedName);
      result.FailedCount1 = GetCount(map, result.Name1, result.FailedName);
      result.SucceedCount2 = GetCount(map, result.Name2, result.SucceedName);
      result.FailedCount2 = GetCount(map, result.Name2, result.FailedName);

      return result;
    }
  }
}
