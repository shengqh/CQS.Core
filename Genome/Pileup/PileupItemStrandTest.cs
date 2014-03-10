using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Statistics;
using CQS.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItemStrandTest : IPileupItemTest<FisherExactTestResult>
  {
    public PileupItemStrandTest()
    { }

    public FisherExactTestResult Test(PileupItem item, PairedEvent paired)
    {
      var result = new FisherExactTestResult();

      result.Sample1.Name = StrandType.FORWARD.ToString();
      result.Sample2.Name = StrandType.REVERSE.ToString();
      result.SucceedName = paired.MajorEvent;
      result.FailedName = paired.MinorEvent;

      foreach (var s in item.Samples)
      {
        foreach (var b in s)
        {
          var sample = b.Strand == StrandType.FORWARD ? result.Sample1 : result.Sample2;
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
