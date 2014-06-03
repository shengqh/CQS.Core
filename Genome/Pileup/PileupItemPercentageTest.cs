using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Statistics;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupItemNormalTest
  {
    private double maxPrecentage;
    public PileupItemNormalTest(double maxPrecentage)
    {
      this.maxPrecentage = maxPrecentage;
    }

    public bool Accept(FisherExactTestResult result)
    {
      return result.Sample1.FailedPercentage <= this.maxPrecentage;
    }
  }

  public class PileupItemTumorTest
  {
    private int _minReads;
    private double _minPrecentage;
   
    public PileupItemTumorTest(int minReads, double minPrecentage)
    {
      this._minReads = minReads;
      this._minPrecentage = minPrecentage;
    }

    public bool Accept(FisherExactTestResult result)
    {
      return result.Sample2.Failed >= _minReads && result.Sample2.FailedPercentage >= _minPrecentage;
    }
  }
}
