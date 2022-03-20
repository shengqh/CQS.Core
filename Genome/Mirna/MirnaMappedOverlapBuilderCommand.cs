using RCPA.Commandline;
using RCPA.Gui.Command;

namespace CQS.Genome.Mirna
{
  public class MirnaMappedOverlapBuilderCommand : AbstractCommandLineCommand<MirnaMappedOverlapBuilderOptions>, IToolCommand
  {
    #region ICommandLineCommand Members

    public override string Name
    {
      get { return "mirna_overlap"; }
    }

    public override string Description
    {
      get { return "miRNA overlap comparison between two mapped files"; }
    }

    public override RCPA.IProcessor GetProcessor(MirnaMappedOverlapBuilderOptions options)
    {
      return new MirnaMappedOverlapBuilder(options);
    }

    #endregion

    #region IToolCommand Members

    public string GetClassification()
    {
      return "miRNA";
    }

    public string GetCaption()
    {
      return MirnaMappedOverlapBuilderUI.title;
    }

    public string GetVersion()
    {
      return MirnaMappedOverlapBuilderUI.version;
    }

    public void Run()
    {
      new MirnaMappedOverlapBuilderUI().MyShow();
    }

    #endregion
  }
}
