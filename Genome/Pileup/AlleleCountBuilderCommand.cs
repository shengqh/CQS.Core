using RCPA.Commandline;

namespace CQS.Genome.Pileup
{
  public class AlleleCountBuilderCommand : AbstractCommandLineCommand<AlleleCountBuilderOptions>
  {
    public override string Name
    {
      get { return "cqs_allelecount"; }
    }

    public override string Description
    {
      get { return "Get count of major/minor allele from bam/sam file "; }
    }

    public override RCPA.IProcessor GetProcessor(AlleleCountBuilderOptions options)
    {
      return new AlleleCountBuilder(options);
    }
  }
}
