using RCPA.Commandline;

namespace CQS.Genome.Fastq
{
  public class FastqLengthDistributionBuilderCommand : AbstractCommandLineCommand<FastqLengthDistributionBuilderOptions>
  {
    public override string Name
    {
      get { return "fastq_len"; }
    }

    public override string Description
    {
      get { return "Build fastq read length distribution"; }
    }

    public override RCPA.IProcessor GetProcessor(FastqLengthDistributionBuilderOptions options)
    {
      return new FastqLengthDistributionBuilder(options);
    }
  }
}
