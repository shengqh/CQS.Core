using RCPA.Commandline;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNASequenceCountTableBuilderCommand : AbstractCommandLineCommand<SmallRNASequenceCountTableBuilderOptions>
  {
    public override string Name
    {
      get { return "smallrna_sequence_count_table"; }
    }

    public override string Description
    {
      get { return "Build smallRNA identical sequence count table"; }
    }

    public override RCPA.IProcessor GetProcessor(SmallRNASequenceCountTableBuilderOptions options)
    {
      return new SmallRNASequenceCountTableBuilder(options);
    }
  }
}
