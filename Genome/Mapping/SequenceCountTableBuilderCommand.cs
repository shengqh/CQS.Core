
using RCPA.Commandline;
namespace CQS.Genome.Mapping
{
  public class SequenceCountTableBuilderCommand : AbstractCommandLineCommand<SequenceCountTableBuilderOptions>
  {
    public override string Name
    {
      get { return "bam_sequence_count_table"; }
    }

    public override string Description
    {
      get { return "Build sequence count table from bam files"; }
    }

    public override RCPA.IProcessor GetProcessor(SequenceCountTableBuilderOptions options)
    {
      return new SequenceCountTableBuilder(options);
    }
  }
}
