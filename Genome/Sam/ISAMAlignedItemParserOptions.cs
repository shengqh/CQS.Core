using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Sam
{
  public interface ISAMAlignedItemParserOptions
  {
    string Samtools { get; set; }

    int MinimumReadLength { get; set; }

    int MaximumReadLength { get; set; }

    int MaximumMismatchCount { get; set; }

    int EngineType { get; set; }

    ISAMFormat GetSAMFormat();

  }
}
