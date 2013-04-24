using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Statistics;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItemPercentageTest
  {
    private double minPrecentage;
    public PileupItemPercentageTest(double minPrecentage)
    {
      this.minPrecentage = minPrecentage;
    }

    public bool Accept(FisherExactTestResult result)
    {
      return (result.FailedCount2 * 1.0 / (result.FailedCount2 + result.SucceedCount2) >= this.minPrecentage);
    }
  }
}
