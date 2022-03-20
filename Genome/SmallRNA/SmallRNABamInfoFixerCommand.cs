using RCPA.Commandline;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNABamInfoFixerCommand : AbstractCommandLineCommand<SmallRNABamInfoFixerOptions>
  {
    public override string Name
    {
      get { return "smallrna_baminfo_fix"; }
    }

    public override string Description
    {
      get { return "Fix wrong total reads in count result info file"; }
    }

    public override RCPA.IProcessor GetProcessor(SmallRNABamInfoFixerOptions options)
    {
      return new SmallRNABamInfoFixer(options);
    }
  }
}
