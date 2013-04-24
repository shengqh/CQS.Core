using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Statistics;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItemReadDepthFilter : IFilter<PileupItem>
  {
    private int minReadDepth;

    private int minBaseMappingQuality;

    public PileupItemReadDepthFilter(int minReadDepth, int minBaseMappingQuality)
    {
      this.minReadDepth = minReadDepth;
      this.minBaseMappingQuality = minBaseMappingQuality - 1;
    }

    public bool Accept(PileupItem t)
    {
      foreach (var s in t.Samples)
      {
        var count = s.Count(m => m.Score > this.minBaseMappingQuality);
        if (count < this.minReadDepth)
        {
          return false;
        }
      }

      return true;
    }
  }
}
