using RCPA;

namespace CQS.Genome.Mapping
{
  public class DistinctMappedReadProcessorCommand : AbstractCommandLineCommand<DistinctMappedReadProcessorOptions>
  {
    public override string Name
    {
      get { return "mapped_distinct"; }
    }

    public override string Description
    {
      get { return "Build distinct mapped data table from two count xml files based on same fastq file"; }
    }

    public override IProcessor GetProcessor(DistinctMappedReadProcessorOptions options)
    {
      return new DistinctMappedReadProcessor(options);
    }
  }
}