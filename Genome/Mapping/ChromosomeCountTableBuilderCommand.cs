
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
      get { return "Build chromosome data table from files located in subdirectories of root directory"; }
    }

    public override RCPA.IProcessor GetProcessor(ChromosomeCountTableBuilderOptions options)
    {
      return new ChromosomeCountSlimTableBuilder(options);
    }
  }
}
