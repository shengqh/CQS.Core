using RCPA.Commandline;

namespace CQS.Genome.Pileup
{
  public class PileupCountBuilderCommand : AbstractCommandLineCommand<PileupCountBuilderOptions>
  {
    public override string Name
    {
      get { return "cqs_pileup"; }
    }

    public override string Description
    {
      get { return "Pileup from bam/sam file"; }
    }

    public override RCPA.IProcessor GetProcessor(PileupCountBuilderOptions options)
    {
      return new PileupCountBuilder(options);
    }
  }
}
