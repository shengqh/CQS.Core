using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Sam
{
  public static class EngineType
  {
    public const int bowtie1 = 1;
    public const int bowtie2 = 2;
    public const int bwa = 3;
    public const int gsnap = 4;
    public const int star = 5;
  };

  public interface ISAMAlignedItemParserOptions
  {
    int MinimumReadLength { get; set; }

    int MaximumReadLength { get; set; }

    int MaximumMismatch { get; set; }

    int MaximumNoPenaltyMutationCount { get; set; }

    int EngineType { get; set; }

    ISAMFormat GetSAMFormat();

    bool IgnoreScore { get; set; }
  }
}
