using RCPA.Commandline;

namespace CQS.Genome.Plink
{
  public class PlinkStrandFlipProcessorCommand : AbstractCommandLineCommand<PlinkStrandFlipProcessorOptions>
  {
    public override string Name
    {
      get { return "plink_flip"; }
    }

    public override string Description
    {
      get { return "Flip alleles based on dbsnp and 1000 genome"; }
    }

    public override RCPA.IProcessor GetProcessor(PlinkStrandFlipProcessorOptions options)
    {
      return new PlinkStrandFlipProcessor(options);
    }
  }
}
