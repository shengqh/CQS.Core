using RCPA.Commandline;

namespace CQS.Genome.Parclip
{
  public class ParclipSmallRNAT2CBuilderCommand : AbstractCommandLineCommand<ParclipSmallRNAT2CBuilderOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "parclip_t2c"; }
    }

    public override string Description
    {
      get { return "Find smallRNA T2C mutation sites"; }
    }

    public override RCPA.IProcessor GetProcessor(ParclipSmallRNAT2CBuilderOptions options)
    {
      return new ParclipSmallRNAT2CBuilder(options);
    }
    #endregion ICommandLineTool
  }
}
