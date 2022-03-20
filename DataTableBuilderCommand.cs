using RCPA.Commandline;

namespace CQS
{
  public class DataTableBuilderCommand : AbstractCommandLineCommand<DataTableBuilderOptions>
  {
    public override string Name
    {
      get { return "data_table"; }
    }

    public override string Description
    {
      get { return "Build data table from files located in subdirectories of root directory"; }
    }

    public override RCPA.IProcessor GetProcessor(DataTableBuilderOptions options)
    {
      return new DataTableBuilder(options);
    }
  }
}
