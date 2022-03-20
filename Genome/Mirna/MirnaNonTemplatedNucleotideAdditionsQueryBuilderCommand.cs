using RCPA.Commandline;

namespace CQS.Genome.Mirna
{
  public class MirnaNonTemplatedNucleotideAdditionsQueryBuilderCommand : AbstractCommandLineCommand<MirnaNonTemplatedNucleotideAdditionsQueryBuilderOptions>
  {
    public override string Name
    {
      get { return "fastq_mirna"; }
    }

    public override string Description
    {
      get { return "Clip fastq file for miRNA NTA detection"; }
    }

    public override RCPA.IProcessor GetProcessor(MirnaNonTemplatedNucleotideAdditionsQueryBuilderOptions options)
    {
      return new MirnaNonTemplatedNucleotideAdditionsQueryBuilder(options);
    }
  }
}
