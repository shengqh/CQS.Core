using RCPA;

namespace CQS.Genome.Sam
{
  public class AlignmentResultCleanerCommand : AbstractCommandLineCommand<AlignmentResultCleanerOptions>
  {
    public override string Name
    {
      get { return "align_clean"; }
    }

    public override string Description
    {
      get { return "If *sorted.bam exists, all other bam and corresponding sam/bai will be deleted. If *.bam exists, all *.sai will be deleted."; }
    }

    public override IProcessor GetProcessor(AlignmentResultCleanerOptions options)
    {
      return new AlignmentResultCleaner(options);
    }
  }
}