using RCPA.Commandline;

namespace CQS.Genome.SmallRNA
{
  public class TrnaNonTemplatedNucleotideAdditionsQueryBuilderCommand : AbstractCommandLineCommand<TrnaNonTemplatedNucleotideAdditionsQueryBuilderOptions>
  {
    public override string Name
    {
      get { return "tgirt_nta"; }
    }

    public override string Description
    {
      get { return "TGIRT - clip fastq file for TRNA CCA/CCAA NTA"; }
    }

    public override RCPA.IProcessor GetProcessor(TrnaNonTemplatedNucleotideAdditionsQueryBuilderOptions options)
    {
      return new TrnaNonTemplatedNucleotideAdditionsQueryBuilder(options);
    }
  }
}
