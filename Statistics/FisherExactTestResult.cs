using CQS.Statistics;

namespace CQS.Genome.Statistics
{
  public class FisherExactTestResult : ITestResult
  {
    public FisherExactTestResult()
    {
      SucceedName = "Succeed";
      FailedName = "Failed";
      Sample1 = new Sample("Normal");
      Sample2 = new Sample("Tumor");
      PValue = 1;
    }

    public string SucceedName { get; set; }

    public string FailedName { get; set; }

    public Sample Sample1 { get; private set; }

    public Sample Sample2 { get; private set; }

    public double PValue { get; set; }

    public double CalculateTwoTailPValue()
    {
      //When sample size < 20
      PValue = MyFisherExactTest.TwoTailPValue(Sample1.Succeed, Sample1.Failed, Sample2.Succeed, Sample2.Failed);
      return PValue;
    }

    public override string ToString()
    {
      return string.Format("{0} ~ {1} = {2:0.##E+0}", Sample1, Sample2, PValue);
    }

    public class Sample
    {
      public Sample(string name = "")
      {
        Name = name;
        Succeed = 0;
        Failed = 0;
      }

      public string Name { get; set; }
      public int Succeed { get; set; }
      public int Failed { get; set; }

      public int TotalCount
      {
        get { return Succeed + Failed; }
      }

      public double FailedPercentage
      {
        get { return Failed * 1.0 / TotalCount; }
      }

      public override string ToString()
      {
        return string.Format("{0}:{1}:{2}", Name, Succeed, Failed);
      }
    }
  }
}