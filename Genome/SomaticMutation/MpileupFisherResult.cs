using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupFisherResult
  {
    public string CandidateFile { get; set; }
    public PileupItem Item { get; set; }
    public FisherExactTestResult Group { get; set; }

    public string GetString(char separator = '_')
    {
      return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10:0.0E00}",
        separator,
            Item.SequenceIdentifier,
            Item.Position,
            Item.Nucleotide,
            Group.SucceedName,
            Group.FailedName,
            Group.Sample1.Succeed,
            Group.Sample1.Failed,
            Group.Sample2.Succeed,
            Group.Sample2.Failed,
            Group.PValue);
    }

    public void ParseString(string line, char separator = '_')
    {
      try
      {
        var parts = line.Split(separator);
        this.Item = new PileupItem()
        {
          SequenceIdentifier = parts[0],
          Position = long.Parse(parts[1]),
          Nucleotide = parts[2][0]
        };
        this.Group = new FisherExactTestResult()
        {
          SucceedName = parts[3],
          FailedName = parts[4],
        };
        this.Group.Sample1.Succeed = int.Parse(parts[5]);
        this.Group.Sample1.Failed = int.Parse(parts[6]);
        this.Group.Sample2.Succeed = int.Parse(parts[7]);
        this.Group.Sample2.Failed = int.Parse(parts[8]);
        this.Group.PValue = double.Parse(parts[9]);
      }
      catch (Exception ex)
      {
        Console.WriteLine("ParseString error: " + line);
        throw ex;
      }
    }
  }
}
