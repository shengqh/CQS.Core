using RCPA.Commandline;
using RCPA.Gui.Command;

namespace CQS.Genome.Annotation
{
  public class ParalyzerClusterAnnotatorCommand : AbstractCommandLineCommand<ParalyzerClusterAnnotatorOptions>
  {
    #region ICommandLineTool

    public override string Name
    {
      get { return "paralyzer_annotation"; }
    }

    public override string Description
    {
      get { return "Annotate Paralyzer result by miRNA database"; }
    }

    public override RCPA.IProcessor GetProcessor(ParalyzerClusterAnnotatorOptions options)
    {
      return new ParalyzerClusterAnnotator(options);
    }

    #endregion ICommandLineTool
  }
}
