using RCPA.Commandline;

namespace CQS.Genome.Sam
{
  public class BamSummaryBuilderCommand : AbstractCommandLineCommand<BamSummaryBuilderOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "bam_stat"; }
    }

    public override string Description
    {
      get { return "Summarize samtools flagstat result"; }
    }

    public override RCPA.IProcessor GetProcessor(BamSummaryBuilderOptions options)
    {
      return new BamSummaryBuilder(options);
    }
    #endregion ICommandLineTool
  }
}
