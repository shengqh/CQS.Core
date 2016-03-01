using RCPA;
using RCPA.Commandline;

namespace CQS.Genome.Mapping
{
  public class SamExtractorCommand : AbstractCommandLineCommand<SamExtractorOptions>
  {
    public override string Name
    {
      get { return "sam_extract"; }
    }

    public override string Description
    {
      get { return "Extract reads mapped to smallRNA from bam file"; }
    }

    public override IProcessor GetProcessor(SamExtractorOptions options)
    {
      return new SamExtractor(options);
    }
  }
}