using RCPA.Commandline;

namespace CQS.Genome.SomaticMutation
{
  public class PipelineProcessorCommand : AbstractCommandLineCommand<PipelineProcessorOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "call"; }
    }

    public override string Description
    {
      get { return "Call somatic mutation, filter candidates by brglm model and finally annotate the result"; }
    }

    public override RCPA.IProcessor GetProcessor(PipelineProcessorOptions options)
    {
      return new PipelineProcessor(options);
    }
    #endregion ICommandLineTool
  }
}
