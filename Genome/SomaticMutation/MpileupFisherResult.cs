using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupFisherResult
  {
    public MpileupFisherResult()
    {
      this.CandidateFile = string.Empty;
      this.FailedReason = string.Empty;
    }

    public string CandidateFile { get; set; }
    public PileupItem Item { get; set; }
    public FisherExactTestResult Group { get; set; }
    public string FailedReason { get; set; }
  }
}
