using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Statistics;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItemGroupTest : IPileupItemTest<FisherExactTestResult>
  {

    public PileupItemGroupTest()
    { }

    /// <summary>
    /// The item put into test must have exact two samples and have at least two events, otherwise null reference exception will be thrown
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public FisherExactTestResult Test(PileupItem item)
    {
      FisherExactTestResult result = item.InitializeTable();

      return Test(result);
    }

    public FisherExactTestResult Test(FisherExactTestResult result)
    {
      result.PValue = MyFisherExactTest.TwoTailPValue(result.SucceedCount1, result.FailedCount1,
        result.SucceedCount2, result.FailedCount2);

      return result;
    }
  }
}
