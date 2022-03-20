using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItemPositionTest : IPileupItemTest<FisherExactTestResult>
  {
    public PileupItemPositionTest()
    { }

    public FisherExactTestResult Test(PileupItem item, PairedEvent paired)
    {
      var result = new FisherExactTestResult();

      result.Sample1.Name = PositionType.MIDDLE.ToString();
      result.Sample2.Name = "TERMINAL";
      result.SucceedName = paired.MajorEvent;
      result.FailedName = paired.MinorEvent;

      foreach (var s in item.Samples)
      {
        foreach (var b in s)
        {
          var sample = b.Position == PositionType.MIDDLE ? result.Sample1 : result.Sample2;

          if (b.Event.Equals(result.SucceedName))
          {
            sample.Succeed++;
          }
          else
          {
            sample.Failed++;
          }
        }
      }

      result.CalculateTwoTailPValue();

      return result;
    }
  }
}
