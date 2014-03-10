using CommandLine;
using CQS.Commandline;
using CQS.Genome.Sam;

namespace CQS.Genome.Mapping
{
  public class SAMAlignedItemParserOptions : ISAMAlignedItemParserOptions
  {
    public int MinimumReadLength { get; set; }

    public int MaximumMismatchCount { get; set; }

    public int EngineType { get; set; }

    public string Samtools { get; set; }

    public ISAMFormat GetSAMFormat()
    {
      switch (this.EngineType)
      {
        case 2: return SAMFormat.Bowtie2;
        case 3: return SAMFormat.Bwa;
        default: return SAMFormat.Bowtie1;
      }
    }
  }
}
