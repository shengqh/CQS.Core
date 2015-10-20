using RCPA.Commandline;
using RCPA.Gui.Command;

namespace CQS.Microarray
{
  public class GseMatrixDownloaderCommand : AbstractCommandLineCommand<GseMatrixDownloaderOptions>, IToolCommand
  {
    public override string Name
    {
      get { return "gse_matrix"; }
    }

    public override string Description
    {
      get { return "Download GSE Matrix Files"; }
    }

    public override RCPA.IProcessor GetProcessor(GseMatrixDownloaderOptions options)
    {
      return new GseMatrixDownloader(options);
    }

    #region IToolCommand Members

    public string GetClassification()
    {
      return "Sample";
    }

    public string GetCaption()
    {
      return GseMatrixDownloaderUI.Title;
    }

    public string GetVersion()
    {
      return GseMatrixDownloaderUI.Version;
    }

    public void Run()
    {
      new GseMatrixDownloaderUI().MyShow();
    }

    #endregion
  }
}
