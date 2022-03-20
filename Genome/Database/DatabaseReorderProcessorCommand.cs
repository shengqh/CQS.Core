using RCPA.Commandline;

namespace CQS.Genome.Database
{
  public class DatabaseReorderProcessorCommand : AbstractCommandLineCommand<DatabaseReorderProcessorOptions>
  {
    public override string Name
    {
      get { return "database_reorder"; }
    }

    public override string Description
    {
      get { return "Reorder chromosomes of database"; }
    }

    public override RCPA.IProcessor GetProcessor(DatabaseReorderProcessorOptions options)
    {
      return new DatabaseReorderProcessor(options);
    }
  }
}
