using RCPA.Commandline;

namespace CQS.Genome.Fastq
{
  public class FastqDemultiplexProcessorCommand : AbstractCommandLineCommand<FastqDemultiplexProcessorOptions>
  {
    public override string Name
    {
      get { return "fastq_demultiplex"; }
    }

    public override string Description
    {
      get { return "Demultiplex fastq file"; }
    }

    public override RCPA.IProcessor GetProcessor(FastqDemultiplexProcessorOptions options)
    {
      return new FastqDemultiplexProcessor(options);
    }
  }
}
