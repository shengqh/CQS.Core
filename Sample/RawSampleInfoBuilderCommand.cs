using RCPA.Commandline;

namespace CQS.Sample
{
  public class RawSampleInfoBuilderCommand : AbstractCommandLineCommand<RawSampleInfoBuilderOptions>
  {
    public override string Name
    {
      get { return "sample_rawinfo"; }
    }

    public override string Description
    {
      get { return "Build sample information table from gse matrix file or sdrf file"; }
    }

    public override RCPA.IProcessor GetProcessor(RawSampleInfoBuilderOptions options)
    {
      return new RawSampleInfoBuilder(options);
    }
  }
}
