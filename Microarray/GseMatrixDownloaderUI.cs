using RCPA.Gui;
using RCPA.Gui.Command;

namespace CQS.Microarray
{
  public partial class GseMatrixDownloaderUI : AbstractProcessorUI
  {
    public static string Title = "GEO Matrix File Downloader";
    public static string Version = "1.0.0";

    public GseMatrixDownloaderUI()
    {
      InitializeComponent();

      this.Text = Constants.GetSqhVanderbiltTitle(Title, Version);
    }

    protected override RCPA.IProcessor GetProcessor()
    {
      return new GseMatrixDownloader(new GseMatrixDownloaderOptions() { InputDirectory = rootDirectory.FullName });
    }
  }
}
