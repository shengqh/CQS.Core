using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.QC
{
  public class RNASeQCItem
  {
    public string Sample { get; set; }

    public long AlternativeAlignments { get; set; }

    public long MappedUnique { get; set; }

    public long MappedPairs { get; set; }

    public double BaseMismatchRate { get; set; }
    
    public int FragmentLengthMean { get; set; }
    
    public int FragmentLengthStdDev { get; set; }
    
    public long ChimericPairs { get; set; }

    public double IntragenicRate { get; set; }
    
    public double IntronicRate { get; set; }

    public double ExonicRate { get; set; }

    public int ReadLength { get; set; }
    
    public int TranscriptsDetected { get; set; }
    
    public int GenesDetected { get; set; }
    
    public double MeanPerBaseCoverage { get; set; }

    public double ExpressionProfilingEfficiency { get; set; }

    public double IntergenicRate { get { return 1 - this.IntragenicRate; } }
  }
}
