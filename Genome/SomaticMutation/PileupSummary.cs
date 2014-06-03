using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.SomaticMutation
{
  class PileupSummary
  {
    public int MinReadDepthFailed { get; set; }
    public int OneEventFailed { get; set; }
    public int MinorAlleleDecreasedFailed { get; set; }
    public int MinimumPercentageFailed { get; set; }
    public int GroupFisherFailed { get; set; }
    public int CandidateCount { get; set; }
    public int TotalCount { get; set; }
  }
}
