using RCPA;
using RCPA.Commandline;
using RCPA.Gui.Command;

namespace CQS.Genome.Mapping
{
  public class MappedCountProcessorCommand : AbstractCommandLineCommand<MappedCountProcessorOptions>, IToolCommand
  {
    #region IToolCommand Members

    public string GetClassification()
    {
      return "Mapping";
    }

    public string GetCaption()
    {
      return MappedCountProcessorUI.title;
    }

    public string GetVersion()
    {
      return MappedCountProcessorUI.version;
    }

    public void Run()
    {
      new MappedCountProcessorUI().MyShow();
    }

    #endregion

    public override string Name
    {
      get { return "mapped_count"; }
    }

    public override string Description
    {
      get { return "Parsing mapping count from bam/sam file"; }
    }

    public override IProcessor GetProcessor(MappedCountProcessorOptions options)
    {
      return new MappedCountProcessor(options);
    }
  }
}
