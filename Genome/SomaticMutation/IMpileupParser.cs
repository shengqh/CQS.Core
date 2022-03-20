namespace CQS.Genome.SomaticMutation
{
  public interface IMpileupParser
  {
    MpileupFisherResult Parse(string line, bool writeCandidateFile);
  }
}
