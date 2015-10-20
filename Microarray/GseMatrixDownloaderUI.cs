using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;

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
