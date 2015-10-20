using RCPA.Commandline;
using RCPA.Gui.Command;

namespace CQS.Genome.Vcf
{
  public class VcfGenotypeTableBuilderCommand : AbstractCommandLineCommand<VcfGenotypeTableBuilderOptions>
  {
    #region ICommandLineTool

    public override string Name
    {
      get { return "vcf_table"; }
    }

    public override string Description
    {
      get { return "Generate genotype table from VCF file"; }
    }

    public override RCPA.IProcessor GetProcessor(VcfGenotypeTableBuilderOptions options)
    {
      return new VcfGenotypeTableBuilder(options);
    }

    #endregion ICommandLineTool
  }
}
