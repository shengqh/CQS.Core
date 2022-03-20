using RCPA.Commandline;

namespace CQS.Genome.SomaticMutation
{
  public class SomaticMutationTableBuilderCommand : AbstractCommandLineCommand<SomaticMutationTableBuilderOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "table"; }
    }

    public override string Description
    {
      get { return "Summarize and build somatic mutation table"; }
    }

    public override RCPA.IProcessor GetProcessor(SomaticMutationTableBuilderOptions options)
    {
      return new SomaticMutationTableBuilder(options);
    }
    #endregion ICommandLineTool
  }
}
