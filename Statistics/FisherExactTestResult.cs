using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Statistics;

namespace CQS.Genome.Statistics
{
  public class FisherExactTestResult : ITestResult
  {
    public FisherExactTestResult()
    {
      this.Name1 = "Normal";
      this.Name2 = "Tumor";
      this.SucceedName = "Succeed";
      this.FailedName = "Failed";
      this.SucceedCount1 = 0;
      this.FailedCount1 = 0;
      this.SucceedCount2 = 0;
      this.FailedCount2 = 0;
      this.PValue = 1;
    }

    public string Name1 { get; set; }

    public string Name2 { get; set; }

    public string SucceedName { get; set; }

    public string FailedName { get; set; }
    
    public int SucceedCount1 { get; set; }
    
    public int FailedCount1 { get; set; }
    
    public int SucceedCount2 { get; set; }
    
    public int FailedCount2 { get; set; }
    
    public double PValue { get; set; }
  }
}
