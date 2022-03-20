using RCPA.Commandline;

namespace CQS.Genome.Mirna
{
  public class MirnaNTACountTableBuilderCommand : AbstractCommandLineCommand<MirnaCountTableBuilderOptions>
  {
    public override string Name
    {
      get { return "mirna_nta_table"; }
    }

    public override string Description
    {
      get { return "Build miRNA NTA data table from files located in subdirectories of root directory"; }
    }

    public override RCPA.IProcessor GetProcessor(MirnaCountTableBuilderOptions options)
    {
      return new MirnaNTACountTableBuilder(options);
    }
  }
}
