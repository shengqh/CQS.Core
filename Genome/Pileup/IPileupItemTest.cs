using CQS.Statistics;

namespace CQS.Genome.Pileup
{
  public interface IPileupItemTest<T> where T : ITestResult
  {
    T Test(PileupItem item, PairedEvent paired);
  }
}
