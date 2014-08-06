
namespace CQS.Genome.Mapping
{
  public class ChromosomeCountProcessorCommand : AbstractCommandLineCommand<ChromosomeCountProcessorOptions>
  {
    public override string Name
    {
      get { return "chromosome_count"; }
    }

    public override string Description
    {
      get { return "Parsing chromosome count from bam/sam file"; }
    }

    public override RCPA.IProcessor GetProcessor(ChromosomeCountProcessorOptions options)
    {
      return new ChromosomeCountProcessor(options);
    }
  }
}
