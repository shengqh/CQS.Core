using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.Genome.GroSeq
{
  public class PearsonCorrelationCalculator
  {
    public double Calculate(string fileName)
    {
      double sumx = 0.0, sumy = 0.0, sumxy = 0.0, sumxx = 0.0, sumyy = 0.0, n = 0;
      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
          if (parts.Length < 2)
          {
            continue;
          }

          n++;

          var x = double.Parse(parts[0]);
          var y = double.Parse(parts[1]);

          sumx += x;
          sumy += y;
          sumxy += x * y;
          sumxx += x * x;
          sumyy += y * y;

          if ((n % 100000) == 0)
          {
            Console.WriteLine("{0}", n);
          }
        }
      }

      return (n * sumxy - sumx * sumy) / (Math.Sqrt(n * sumxx - sumx * sumx) * Math.Sqrt(n * sumyy - sumy * sumy));
    }
  }
}
