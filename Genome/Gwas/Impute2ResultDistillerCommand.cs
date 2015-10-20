using RCPA.Commandline;

namespace CQS.Genome.Gwas
{
  public class Impute2ResultDistillerCommand : AbstractCommandLineCommand<Impute2ResultDistillerOptions>
  {
    #region ICommandLineCommand Members

    public override string Name
    {
      get { return "impute2_distiller"; }
    }

    public override string Description
    {
      get { return "Impute2 result distiller"; }
    }

    public override RCPA.IProcessor GetProcessor(Impute2ResultDistillerOptions options)
    {
      return new Impute2ResultDistiller(options);
    }

    #endregion
  }
}
