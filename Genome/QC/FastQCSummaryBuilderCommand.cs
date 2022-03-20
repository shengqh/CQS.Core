using RCPA.Commandline;

namespace CQS.Genome.QC
{
  public class FastQCSummaryBuilderCommand : AbstractCommandLineCommand<FastQCSummaryBuilderOptions>
  {
    public override string Name
    {
      get { return "fastqc_summary"; }
    }

    public override string Description
    {
      get { return "Build summary of FastQC result"; }
    }

    public override RCPA.IProcessor GetProcessor(FastQCSummaryBuilderOptions options)
    {
      return new FastQCSummaryBuilder(options);
    }
  }
}
