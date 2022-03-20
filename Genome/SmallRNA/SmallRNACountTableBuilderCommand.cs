using RCPA.Commandline;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountTableBuilderCommand : AbstractCommandLineCommand<SmallRNACountTableBuilderOptions>
  {
    public override string Name
    {
      get { return "smallrna_table"; }
    }

    public override string Description
    {
      get { return "Build smallRNA data table from files located in subdirectories of root directory"; }
    }

    public override RCPA.IProcessor GetProcessor(SmallRNACountTableBuilderOptions options)
    {
      return new SmallRNACountTableBuilder(options);
    }
  }
}
