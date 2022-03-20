using RCPA.Commandline;

namespace CQS.Genome.Vcf
{
  public class VcfFilterProcessorCommand : AbstractCommandLineCommand<VcfFilterProcessorOptions>
  {
    #region ICommandLineTool

    public override string Name
    {
      get { return "vcf_filter"; }
    }

    public override string Description
    {
      get { return "Filter VCF by minimum median depth"; }
    }

    public override RCPA.IProcessor GetProcessor(VcfFilterProcessorOptions options)
    {
      return new VcfFilterProcessor(options);
    }

    #endregion ICommandLineTool
  }
}
