using RCPA.Commandline;

namespace CQS.Genome.Quantification
{
  public class HTSeqCountToFPKMCalculatorCommand : AbstractCommandLineCommand<HTSeqCountToFPKMCalculatorOptions>
  {
    public override string Name
    {
      get { return "count2fpkm"; }
    }

    public override string Description
    {
      get { return "Convert HTSeq count to FPKM"; }
    }

    public override RCPA.IProcessor GetProcessor(HTSeqCountToFPKMCalculatorOptions options)
    {
      return new HTSeqCountToFPKMCalculator(options);
    }
  }
}
