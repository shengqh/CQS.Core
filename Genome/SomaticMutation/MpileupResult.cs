using System.Collections.Generic;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupResult
  {
    private static string GetPadChromosome(string name)
    {
      int value;
      if (int.TryParse(name, out value))
      {
        return name.PadLeft(2, '0');
      }
      else
      {
        return name;
      }
    }

    public MpileupResult(string name, string candidateDir) :
      this(name, candidateDir,
       string.Format("{0}/chromosome_{1}.candidates", candidateDir, GetPadChromosome(name)),
       string.Format("{0}/chromosome_{1}.summary", candidateDir, GetPadChromosome(name)))
    { }

    public MpileupResult(string name, string candidateDir, string candidateFile, string candidateSummary)
    {
      this.Name = name;
      this.CandidateFile = candidateFile;
      this.CandidateSummary = candidateSummary;

      this.Results = new List<MpileupFisherResult>();

      this.TotalCount = 0;
      this.Ignored = 0;
      this.NotCovered = 0;
      this.CandidateCount = 0;
      this.GroupFisherFailed = 0;
      this.MinimumReadDepthFailed = 0;
      this.OneEventFailed = 0;
      this.MinorAlleleDecreasedFailed = 0;
      this.MinorAlleleFailedInTumorSample = 0;
      this.MinorAlleleFailedInNormalSample = 0;
    }

    public string Name { get; private set; }
    public string CandidateFile { get; private set; }
    public string CandidateSummary { get; private set; }

    public List<MpileupFisherResult> Results { get; private set; }

    public long TotalCount { get; set; }
    public long Ignored { get; set; }
    public long NotCovered { get; set; }
    public long CandidateCount { get; set; }
    public long GroupFisherFailed { get; set; }
    public long MinimumReadDepthFailed { get; set; }
    public long OneEventFailed { get; set; }
    public long MinorAlleleDecreasedFailed { get; set; }
    public long MinorAlleleFailedInTumorSample { get; set; }
    public long MinorAlleleFailedInNormalSample { get; set; }

    public void MergeWith(MpileupResult res)
    {
      this.TotalCount += res.TotalCount;
      this.Ignored += res.Ignored;
      this.NotCovered += res.NotCovered;
      this.CandidateCount += res.CandidateCount;
      this.GroupFisherFailed += res.GroupFisherFailed;
      this.MinimumReadDepthFailed += res.MinimumReadDepthFailed;
      this.OneEventFailed += res.OneEventFailed;
      this.MinorAlleleDecreasedFailed += res.MinorAlleleDecreasedFailed;
      this.MinorAlleleFailedInTumorSample += res.MinorAlleleFailedInTumorSample;
      this.MinorAlleleFailedInNormalSample += res.MinorAlleleFailedInNormalSample;
      this.Results.AddRange(res.Results);
    }
  }
}
