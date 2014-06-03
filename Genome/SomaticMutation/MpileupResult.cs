using CQS.Genome.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupResult
  {
    public MpileupResult(string name, string candidateDir) :
      this(name, candidateDir,
       string.Format("{0}/chromosome_{1}.candidates", candidateDir, name),
       string.Format("{0}/chromosome_{1}.summary", candidateDir, name))
    { }

    public MpileupResult(string name, string candidateDir, string candidateFile, string candidateSummary)
    {
      this.Name = name;
      this.CandidateFile = candidateFile;
      this.CandidateSummary = candidateSummary;

      this.Results = new List<MpileupFisherResult>();

      this.TotalCount = 0;
      this.CandidateCount = 0;
      this.GroupFisherFailed = 0;
      this.MinimumReadDepthFailed = 0;
      this.OneEventFailed = 0;
      this.MinorAlleleDecreasedFailed = 0;
      this.LimitationOfMinorAlleleFailed = 0;
    }

    public string Name { get; private set; }
    public string CandidateFile { get; private set; }
    public string CandidateSummary { get; private set; }

    public List<MpileupFisherResult> Results { get; private set; }

    public int TotalCount { get; set; }
    public int CandidateCount { get; set; }
    public int GroupFisherFailed { get; set; }
    public int MinimumReadDepthFailed { get; set; }
    public int OneEventFailed { get; set; }
    public int MinorAlleleDecreasedFailed { get; set; }
    public int LimitationOfMinorAlleleFailed { get; set; }

    public void MergeWith(MpileupResult res)
    {
      this.TotalCount += res.TotalCount;
      this.CandidateCount += res.CandidateCount;
      this.GroupFisherFailed += res.GroupFisherFailed;
      this.MinimumReadDepthFailed += res.MinimumReadDepthFailed;
      this.OneEventFailed += res.OneEventFailed;
      this.MinorAlleleDecreasedFailed += res.MinorAlleleDecreasedFailed;
      this.LimitationOfMinorAlleleFailed += res.LimitationOfMinorAlleleFailed;
      this.Results.AddRange(res.Results);
    }
  }
}
