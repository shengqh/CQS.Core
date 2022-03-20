using RCPA.Commandline;

namespace CQS.Genome.Gtf
{
  public class Gtf2BedGeneIdBuilderCommand : AbstractCommandLineCommand<Gtf2BedGeneIdBuilderOptions>
  {
    public override string Name
    {
      get { return "gtf2bed"; }
    }

    public override string Description
    {
      get { return "Convert gtf to bed based on gene_id"; }
    }

    public override RCPA.IProcessor GetProcessor(Gtf2BedGeneIdBuilderOptions options)
    {
      return new Gtf2BedGeneIdBuilder(options);
    }
  }
}
