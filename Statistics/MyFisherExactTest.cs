using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.Distributions;
using Meta.Numerics.Statistics;
using CQS.Genome.Statistics;

namespace CQS.Statistics
{
  public static class MyFisherExactTest
  {
    public static double TwoTailPValue(int successOfSampleA, int failOfSampleA, int successOfSampleB, int failOfSampleB)
    {
      int[,] mat;

      int minValue = Math.Min(Math.Min(successOfSampleA, successOfSampleB), Math.Min(failOfSampleA, failOfSampleB));
      if (successOfSampleA == minValue)
      {
        mat = new[,] { { successOfSampleA, failOfSampleA }, { successOfSampleB, failOfSampleB } };
      }
      else if (failOfSampleA == minValue)
      {
        mat = new[,] { { failOfSampleA, successOfSampleA }, { failOfSampleB, successOfSampleB } };
      }
      else if (successOfSampleB == minValue)
      {
        mat = new[,] { { successOfSampleB, failOfSampleB }, { successOfSampleA, failOfSampleA } };
      }
      else
      {
        mat = new[,] { { failOfSampleB, successOfSampleB }, { failOfSampleA, successOfSampleA } };
      }

      return new BinaryContingencyTable(mat).FisherExactTest().Statistic;
    }
  }
}
