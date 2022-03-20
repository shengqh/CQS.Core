using RCPA.Commandline;

namespace CQS.Genome.Sam
{
  public class BamCleanerCommand : AbstractCommandLineCommand<BamCleanerOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "bam_clean"; }
    }

    public override string Description
    {
      get { return "Keep the bam file with longest file name and delete any other bam files (for failed refinement)"; }
    }

    public override RCPA.IProcessor GetProcessor(BamCleanerOptions options)
    {
      return new BamCleaner(options);
    }
    #endregion ICommandLineTool
  }
}
