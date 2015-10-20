using RCPA.Commandline;
using RCPA.Gui.Command;

namespace CQS.Genome.Vcf
{
  public class GvcfValidationProcessorCommand : AbstractCommandLineCommand<GvcfValidationProcessorOptions>
  {
    #region ICommandLineTool

    public override string Name
    {
      get { return "gvcf_validate"; }
    }

    public override string Description
    {
      get { return "Validate GVCF file"; }
    }

    public override RCPA.IProcessor GetProcessor(GvcfValidationProcessorOptions options)
    {
      return new GvcfValidationProcessor(options);
    }

    #endregion ICommandLineTool
  }
}
