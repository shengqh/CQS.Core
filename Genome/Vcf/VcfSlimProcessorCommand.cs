using RCPA.Commandline;
using RCPA.Gui.Command;

namespace CQS.Genome.Vcf
{
  public class VcfSlimProcessorCommand : AbstractCommandLineCommand<VcfSlimProcessorOptions>
  {
    #region ICommandLineTool

    public override string Name
    {
      get { return "slim_vcf"; }
    }

    public override string Description
    {
      get { return "Slim VCF, clear information and following columns"; }
    }

    public override RCPA.IProcessor GetProcessor(VcfSlimProcessorOptions options)
    {
      return new VcfSlimProcessor(options);
    }

    #endregion ICommandLineTool
  }
}
