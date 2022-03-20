using RCPA.Commandline;

namespace CQS.Genome.Bacteria
{
  public class GffToBedConverterCommand : AbstractCommandLineCommand<GffToBedConverterOptions>
  {
    public override string Name
    {
      get { return "bacteria_gff2bed"; }
    }

    public override string Description
    {
      get { return "Convert bacteria gff to bed"; }
    }

    public override RCPA.IProcessor GetProcessor(GffToBedConverterOptions options)
    {
      return new GffToBedConverter(options);
    }
  }
}
