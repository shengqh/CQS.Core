using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Utils;
using RCPA.Gui.Command;

namespace CQS.TCGA
{
  public class TCGADataDownloaderCommand : AbstractCommandLineCommand<TCGADataDownloaderOptions>, IToolCommand
  {
    #region IToolCommand Members

    public string GetClassification()
    {
      return "TCGA";
    }

    public string GetCaption()
    {
      return TCGADataDownloaderUI.Title;
    }

    public string GetVersion()
    {
      return TCGADataDownloaderUI.Version;
    }

    public void Run()
    {
      new TCGADataDownloaderUI().MyShow();
    }

    #endregion

    public override string Name
    {
      get { return "tcga_data"; }
    }

    public override string Description
    {
      get { return "Download TCGA public data"; }
    }

    public override RCPA.IProcessor GetProcessor(TCGADataDownloaderOptions options)
    {
      return new TCGADataDownloader(options);
    }
  }
}
