using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Statistics;
using CQS.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItemPositionTest : IPileupItemTest<FisherExactTestResult>
  {
    public PileupItemPositionTest()
    { }

    public FisherExactTestResult Test(PileupItem item)
    {
      var result = new FisherExactTestResult();

      var bases = (from sample in item.Samples
                   from b in sample
                   select b).ToList();
      var allevents = bases.GetEventCountList();

      if (allevents.Count < 2)
      {
        return result;
      }

      result.Name1 = PositionType.MIDDLE.ToString();
      result.Name2 = "TERMINAL";
      result.SucceedName = allevents[0].Event;
      result.FailedName = allevents[1].Event;

      foreach (var b in bases)
      {
        if (b.Position.Equals(PositionType.MIDDLE))
        {
          if (b.Event.Equals(result.SucceedName))
          {
            result.SucceedCount1++;
          }
          else
          {
            result.FailedCount1++;
          }
        }
        else
        {
          if (b.Event.Equals(result.SucceedName))
          {
            result.SucceedCount2++;
          }
          else
          {
            result.FailedCount2++;
          }
        }
      }

      result.PValue = MyFisherExactTest.TwoTailPValue(result.SucceedCount1, result.FailedCount1,
        result.SucceedCount2, result.FailedCount2);

      return result;
    }
  }
}
