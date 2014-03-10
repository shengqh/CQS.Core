using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Gtf;
using RCPA.Seq;

namespace CQS.Genome.Bed
{
  public class MatchedBedItem : BedItem
  {
    public long ExpectStart { get; set; }

    public long ExpectEnd { get; set; }

    private List<MatchExon> exons = new List<MatchExon>();
    public List<MatchExon> Exons
    {
      get
      {
        return exons;
      }
    }

    public string DirectExpectSequence { get; set; }

    public void SetExpectLength(int length)
    {
      long before = (length - this.Length) / 2;

      if (this.Start < this.End)
      {
        this.ExpectStart = this.Start - before;
      }
      else
      {
        this.ExpectStart = this.End - before;
      }
      this.ExpectEnd = this.ExpectStart + length - 1;
    }

    public bool IsInExon()
    {
      if (this.Exons.Count == 0)
      {
        return false;
      }

      foreach (var exon in this.exons)
      {
        if (exon.RetainedIntron)
        {
          return false;
        }
      }

      return true;
    }

    public void MergeExon()
    {
      bool hasRetainedIntron = false;
      bool hasFullExon = false;
      foreach (var exon in this.exons)
      {
        if (exon.RetainedIntron)
        {
          hasRetainedIntron = true;
        }
        else
        {
          hasFullExon = true;
        }
      }

      if (hasFullExon && hasRetainedIntron)
      {
        this.exons.RemoveAll(m => m.RetainedIntron);
      }

      for (int j = this.exons.Count - 1; j > 0; j--)
      {
        var exonj = this.exons[j];
        for (int k = j - 1; k >= 0; k--)
        {
          var exonk = this.exons[k];
          if (exonk.EqualLocations(exonj))
          {
            exonk.TranscriptId += ";" + exonj.TranscriptId;
            exonk.TranscriptCount += exonj.TranscriptCount;
            this.exons.RemoveAt(j);
            break;
          }

          if (exonk.ContainLocations(exonj))
          {
            this.exons.RemoveAt(j);
            break;
          }

          if (exonj.ContainLocations(exonk))
          {
            this.exons[k] = exonj;
            this.exons.RemoveAt(j);
            break;
          }
        }
      }

      this.exons.Sort((m1, m2) => m2.TranscriptCount.CompareTo(m1.TranscriptCount));
    }

    public void MatchGtfTranscriptItem(GtfTranscriptItem gtItem)
    {
      int index = gtItem.FindItemIndex(this.Start, this.End - 1);
      if (-1 == index)
      {
        return;
      }

      GtfItem gitem = gtItem[index];
      MatchExon exon = new MatchExon();
      this.exons.Add(exon);
      exon.TranscriptId = gitem.TranscriptId;
      exon.TranscriptCount = 1;
      exon.TranscriptType = gitem.Source;

      Location ml = new Location();
      exon.Add(ml);
      if (gitem.Start > this.Start)
      {
        //if the peak range is out of the exon range, that peak range may be potential exon which
        //is detected by RNASeq data, so just extend the peak range to expect length rather than
        //extend by prior exon range
        ml.Start = this.ExpectStart;
        exon.IntronSize = gitem.Start - this.Start;
        exon.RetainedIntron = true;
      }
      else if (gitem.Start <= this.ExpectStart)
      {
        //the exon has enough base pair
        ml.Start = this.ExpectStart;
      }
      else
      {
        //the exon has no enough base pair
        ml.Start = gitem.Start;
        long expectLength = ml.Start - this.ExpectStart;
        int curindex = index - 1;
        while (expectLength > 0 && curindex >= 0)
        {
          Location lp = new Location();
          exon.Insert(0, lp);

          GtfItem prior = gtItem[curindex];
          lp.End = prior.End;
          if (prior.Length >= expectLength)
          {
            lp.Start = prior.End - expectLength + 1;
            break;
          }
          else
          {
            lp.Start = prior.Start;
            expectLength = expectLength - prior.Length;
            curindex--;
          }
        }
      }

      if (gitem.End < this.End - 1)
      {
        ml.End = this.ExpectEnd;
        exon.IntronSize = this.End - 1 - gitem.End;
        exon.RetainedIntron = true;
      }
      else if (gitem.End >= this.ExpectEnd)
      {
        ml.End = this.ExpectEnd;
      }
      else
      {
        ml.End = gitem.End;
        long expectLength = this.ExpectEnd - ml.End;
        int curindex = index + 1;
        while (expectLength > 0 && curindex < gtItem.Count)
        {
          Location lp = new Location();
          exon.Add(lp);

          GtfItem next = gtItem[curindex];
          lp.Start = next.Start;
          if (next.Length >= expectLength)
          {
            lp.End = next.Start + expectLength - 1;
            break;
          }
          else
          {
            lp.End = next.End;
            expectLength = expectLength - next.Length;
            curindex++;
          }
        }
      }
    }

    public void FillSequence(Sequence seq)
    {
      //matched exon fill sequence
      exons.ForEach(m => m.FillSequence(seq, this.Strand));

      //fill direct sequence
      this.DirectExpectSequence = seq.SeqString.Substring((int)this.ExpectStart, (int)(this.ExpectEnd - this.ExpectStart + 1)).ToUpper();
      if (this.Strand == '-')
      {
        this.DirectExpectSequence = SequenceUtils.GetReverseComplementedSequence(this.DirectExpectSequence);
      }
    }
  }
}
