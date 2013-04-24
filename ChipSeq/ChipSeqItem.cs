using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.ChipSeq
{
  public class ChipSeqItem
  {
    public string Filename { get; set; }

    public string Chromosome { get; set; }
    
    public int Start { get; set; }

    public int End { get; set; }

    public int Length
    {
      get
      {
        return End - Start + 1;
      }
    }

    public double ReadDensity { get; set; }

    public double TreatmentCount { get; set; }

    public double ControlCount { get; set; }

    public double EnrichmentFactor { get; set; }

    public string GeneSymbol { get; set; }

    public string LongestTranscript { get; set; }

    public string OverlapType { get; set; }

    public bool InGene
    {
      get
      {
        return !string.IsNullOrEmpty(OverlapType) && OverlapType.ToLower().Contains("cds-exon");
      }
    }

    public int DistanceToTSS { get; set; }

    public string CounAndFactor
    {
      get
      {
        return string.Format("{0:0},{1:0},{2:0.00}", TreatmentCount, ControlCount, EnrichmentFactor);
      }
    }

    public double GetOverlapPercentage(ChipSeqItem another)
    {
      if (!this.GeneSymbol.Equals(another.GeneSymbol))
      {
        return 0;
      }

      if (this.End < another.Start)
      {
        return 0;
      }

      if (this.Start > another.End)
      {
        return 0;
      }

      var oStart = Math.Max(this.Start, another.Start);
      var oEnd = Math.Min(this.End, another.End);
      var oLength = oEnd - oStart + 1;
      var minLen = Math.Min(this.Length, another.Length);
      return oLength * 1.0 / minLen;
    }
  }
}
