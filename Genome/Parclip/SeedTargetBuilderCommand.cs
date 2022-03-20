using RCPA.Commandline;

namespace CQS.Genome.Parclip
{
  public class SeedTargetBuilderCommand : AbstractCommandLineCommand<SeedTargetBuilderOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "parclip_seed_target"; }
    }

    public override string Description
    {
      get { return "Find seed target 3'UTR sites"; }
    }

    public override RCPA.IProcessor GetProcessor(SeedTargetBuilderOptions options)
    {
      return new SeedTargetBuilder(options);
    }
    #endregion ICommandLineTool
  }
}
