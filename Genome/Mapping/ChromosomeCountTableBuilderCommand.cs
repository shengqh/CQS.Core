
using RCPA.Commandline;
namespace CQS.Genome.Mapping
{
  public class ChromosomeCountTableBuilderCommand : AbstractCommandLineCommand<ChromosomeCountTableBuilderOptions>
  {
    public override string Name
    {
      get { return "chromosome_table"; }
    }

    public override string Description
    {
      get { return "Build chromosome count table from count xml file"; }
    }

    public override RCPA.IProcessor GetProcessor(ChromosomeCountTableBuilderOptions options)
    {
      return new ChromosomeCountTableBuilder(options);
    }
  }
}
