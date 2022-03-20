using RCPA.Commandline;

namespace CQS.Genome.SomaticMutation
{
  public class ExtractProcessorCommand : AbstractCommandLineCommand<ExtractProcessorOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "extract"; }
    }

    public override string Description
    {
      get { return "extract allele frequencies of sites in vcf/bed file."; }
    }

    public override RCPA.IProcessor GetProcessor(ExtractProcessorOptions options)
    {
      return new ExtractProcessor(options);
    }
    #endregion ICommandLineTool
  }
}
