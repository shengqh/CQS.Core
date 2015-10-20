using RCPA.Commandline;
using RCPA.Gui.Command;

namespace CQS.Genome.Bed
{
  public class BedSorterOptionsCommand : AbstractCommandLineCommand<BedSorterOptions>
  {
    #region ICommandLineTool

    public override string Name
    {
      get { return "sort_bed"; }
    }

    public override string Description
    {
      get { return "Sort bed based on genome"; }
    }

    public override RCPA.IProcessor GetProcessor(BedSorterOptions options)
    {
      return new BedSorter(options);
    }

    #endregion ICommandLineTool
  }
}
