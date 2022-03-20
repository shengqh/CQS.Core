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

    public string RejectReason
    {
      get { return string.Format("Normal MAF > {0}", this.maxPrecentage); }
    }
  }

  public class PileupItemTumorTest
  {
    private int minReads;
    private double minPrecentage;

    public PileupItemTumorTest(int minReads, double minPrecentage)
    {
      this.minReads = minReads;
      this.minPrecentage = minPrecentage;
    }

    public bool Accept(FisherExactTestResult result)
    {
      return result.Sample2.Failed >= minReads && result.Sample2.FailedPercentage >= minPrecentage;
    }

    public string RejectReason
    {
      get { return string.Format("Tumor minor allele < {0} or Tumor MAF < {1}", this.minReads, this.minPrecentage); }
    }
  }
}
