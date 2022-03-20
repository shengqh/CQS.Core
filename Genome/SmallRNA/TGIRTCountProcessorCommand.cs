using RCPA.Commandline;

namespace CQS.Genome.SmallRNA
{
  public class TGIRTCountProcessorCommand : AbstractCommandLineCommand<TGIRTCountProcessorOptions>
  {
    public override string Name
    {
      get { return "tgirt_count"; }
    }

    public override string Description
    {
      get { return "TGIRT - smallRNA count"; }
    }

    public override RCPA.IProcessor GetProcessor(TGIRTCountProcessorOptions options)
    {
      return new TGIRTCountProcessor(options);
    }
  }
}
