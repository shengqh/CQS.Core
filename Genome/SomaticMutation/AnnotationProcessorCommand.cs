using RCPA.Commandline;

namespace CQS.Genome.SomaticMutation
{
  public class AnnotationProcessorCommand : AbstractCommandLineCommand<AnnotationProcessorOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "annotation"; }
    }

    public override string Description
    {
      get { return "Annotate mutation using varies tools."; }
    }

    public override RCPA.IProcessor GetProcessor(AnnotationProcessorOptions options)
    {
      return new AnnotationProcessor(options);
    }
    #endregion ICommandLineTool
  }
}
