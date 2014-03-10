using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Statistics;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItemNormalPercentageTest
  {
    private double maxPrecentage;
    public PileupItemNormalPercentageTest(double maxPrecentage)
    {
      this.maxPrecentage = maxPrecentage;
    }

    public bool Accept(FisherExactTestResult result)
    {
      return result.Sample1.FailedPercentage <= this.maxPrecentage;
    }
  }

  public class PileupItemTumorPercentageTest
  {
    private double minPrecentage;
    public PileupItemTumorPercentageTest(double minPrecentage)
    {
      this.minPrecentage = minPrecentage;
    }

    public bool Accept(FisherExactTestResult result)
    {
      return result.Sample2.FailedPercentage >= this.minPrecentage;
    }
  }
}
