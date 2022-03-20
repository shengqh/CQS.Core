using RCPA;

namespace CQS.Genome.Pileup
{
  public interface IPileupItemParser : IParser<string, PileupItem>
  {
    PileupItem GetSequenceIdentifierAndPosition(string line);
  }
}
