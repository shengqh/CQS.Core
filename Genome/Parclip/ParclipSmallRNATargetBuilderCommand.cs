using RCPA.Commandline;

namespace CQS.Genome.Parclip
{
  public class ParclipSmallRNATargetBuilderCommand : AbstractCommandLineCommand<ParclipSmallRNATargetBuilderOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "parclip_mirna_target"; }
    }

    public override string Description
    {
      get { return "Find miRNA target 3'UTR sites"; }
    }

    public override RCPA.IProcessor GetProcessor(ParclipSmallRNATargetBuilderOptions options)
    {
      return new ParclipSmallRNATargetBuilder(options);
    }
    #endregion ICommandLineTool
  }
}
