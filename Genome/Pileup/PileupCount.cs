using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Sam;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class PileupCount : Dictionary<char, int>
  {
    public string Chromosome { get; set; }

    public long Position { get; set; }

    public char Reference { get; set; }

    public override string ToString()
    {
      return string.Format("{0}:{1}, {2}, {3}",
        this.Chromosome,
        this.Position,
        this.Reference,
        (from r in this orderby r.Key select string.Format("{0}:{1}", r.Key, r.Value)).Merge("; "));
    }

    public FisherExactTestResult FisherExactTest()
    {
      FisherExactTestResult result = new FisherExactTestResult();
      if (this.Count == 0)
      {
        return result;
      }

      if (this.Count == 1 && this.ContainsKey(this.Reference))
      {
        return result;
      }

      var counts = this.ToList().OrderByDescending(m => m.Value).ToList();
      result.Sample1.Name = this.Reference.ToString();
      result.Sample1.Succeed = counts.Sum(m => m.Value);
      result.Sample1.Failed = 0;

      if (counts[0].Key == this.Reference)
      {
        result.Sample2.Name = counts[1].Key.ToString();
        result.Sample2.Succeed = counts[0].Value;
        result.Sample2.Failed = counts[1].Value;
      }
      else
      {
        result.Sample2.Name = counts[0].Key.ToString();
        result.Sample2.Failed = counts[0].Value;
        if (this.ContainsKey(this.Reference))
        {
          result.Sample2.Succeed = this[this.Reference];
        }
        else
        {
          result.Sample2.Succeed = 0;
        }
      }

      result.CalculateTwoTailPValue();
      return result;
    }
  }
}
