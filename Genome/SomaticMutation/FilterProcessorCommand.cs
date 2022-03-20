using RCPA.Commandline;

namespace CQS.Genome.SomaticMutation
{
  public class FilterProcessorCommand : AbstractCommandLineCommand<FilterProcessorOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "filter"; }
    }

    public override string Description
    {
      get { return "Filter candidates by logistic regression model."; }
    }

    public override RCPA.IProcessor GetProcessor(FilterProcessorOptions options)
    {
      return new FilterProcessor(options);
    }
    #endregion ICommandLineTool
  }
}
