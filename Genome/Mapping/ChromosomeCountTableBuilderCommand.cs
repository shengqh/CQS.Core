
namespace CQS.Genome.Mapping
{
  public class ChromosomeCountTableBuilderCommand : AbstractCommandLineCommand<SimpleDataTableBuilderOptions>
  {
    public override string Name
    {
      get { return "chromosome_table"; }
    }

    public override string Description
    {
      get { return "Build chromosome data table from files located in subdirectories of root directory"; }
    }

    public override RCPA.IProcessor GetProcessor(SimpleDataTableBuilderOptions options)
    {
      return new ChromosomeCountTableBuilder(options);
    }
  }
}
