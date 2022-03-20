using RCPA.Commandline;

namespace CQS.Genome.Mapping
{
  public class MappedReadBuilderCommand : AbstractCommandLineCommand<MappedReadBuilderOptions>
  {
    public override string Name
    {
      get { return "mapped_reads"; }
    }

    public override string Description
    {
      get { return "Extract perfect mapped reads with count and sequence"; }
    }

    public override RCPA.IProcessor GetProcessor(MappedReadBuilderOptions options)
    {
      return new MappedReadBuilder(options);
    }
  }
}
