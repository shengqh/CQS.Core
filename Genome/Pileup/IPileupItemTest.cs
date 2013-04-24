using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Statistics;

namespace CQS.Genome.Pileup
{
  public interface IPileupItemTest<T> where T : ITestResult
  {
    T Test(PileupItem item);
  }
}
