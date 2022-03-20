using CommandLine;
using CQS.Genome.Sam;
using RCPA.Commandline;

namespace CQS.Genome.Pileup
{
  public class AlignedPositionMapBuilderOptions : AbstractOptions
  {
    private const int DEFAULT_MinimumReadQuality = 20;
    private const int DEFAULT_MinimumBaseQuality = 20;
    private const int DEFAULT_EngineType = 1;

    public AlignedPositionMapBuilderOptions()
    {
      this.MinimumReadQuality = DEFAULT_MinimumReadQuality;
      this.MinimumBaseQuality = DEFAULT_MinimumBaseQuality;
      this.EngineType = DEFAULT_EngineType;
    }

    [Option('r', "read_quality", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadQuality, HelpText = "Minimum mapQ of read for pileup")]
    public int MinimumReadQuality { get; set; }

    [Option('b', "base_quality", MetaValue = "INT", DefaultValue = DEFAULT_MinimumBaseQuality, HelpText = "Minimum base quality for mpileup result filter")]
    public int MinimumBaseQuality { get; set; }

    [Option('e', "engineType", DefaultValue = DEFAULT_EngineType, MetaValue = "INT", HelpText = "Engine type (1:bowtie1, 2:bowtie2, 3:bwa)")]
    public int EngineType { get; set; }

    public ISAMFormat GetSAMFormat()
    {
      switch (this.EngineType)
      {
        case 1: return SAMFormat.Bowtie1;
        case 3: return SAMFormat.Bwa;
        default: return SAMFormat.Bowtie2;
      }
    }

    public override bool PrepareOptions()
    {
      return true;
    }
  }
}
