using RCPA.Commandline;

namespace CQS.Genome.QC
{
  public class BamSummaryShortBuilderCommand : AbstractCommandLineCommand<BamSummaryShortBuilderOptions>
  {
    public override string Name
    {
      get { return "bam_stat"; }
    }

    public override string Description
    {
      get { return "Build summary of bam statistic result"; }
    }

    public override RCPA.IProcessor GetProcessor(BamSummaryShortBuilderOptions options)
    {
      return new BamSummaryShortBuilder(options);
    }
  }
}
