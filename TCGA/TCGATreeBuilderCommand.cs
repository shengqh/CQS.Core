using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.TCGA
{
  public class TCGATreeBuilderCommand : AbstractCommandLineCommand<TCGATreeBuilderOptions>, IToolCommand
  {
    #region IToolCommand Members

    public string GetClassification()
    {
      return "TCGA";
    }

    public string GetCaption()
    {
      return TCGATreeBuilderUI.title;
    }

    public string GetVersion()
    {
      return TCGATreeBuilderUI.version;
    }

    public void Run()
    {
      new TCGATreeBuilderUI().MyShow();
    }

    #endregion

    public override string Name
    {
      get { return "tcga_tree"; }
    }

    public override string Description
    {
      get { return "Build TCGA public data structure tree"; }
    }

    public override RCPA.IProcessor GetProcessor(TCGATreeBuilderOptions options)
    {
      return new TCGATreeBuilder(options);
    }
  }
}
