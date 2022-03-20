﻿using RCPA.Commandline;
using RCPA.Gui.Command;

namespace CQS.TCGA
{
  public class TCGADatatableBuilderCommand : AbstractCommandLineCommand<TCGADatatableBuilderOptions>, IToolCommand
  {
    #region IToolCommand Members

    public string GetClassification()
    {
      return "TCGA";
    }

    public string GetCaption()
    {
      return TCGADatatableBuilderUI.title;
    }

    public string GetVersion()
    {
      return TCGADatatableBuilderUI.version;
    }

    public void Run()
    {
      new TCGADatatableBuilderUI().MyShow();
    }

    #endregion

    public override string Name
    {
      get { return "tcga_table"; }
    }

    public override string Description
    {
      get { return "TCGA Data Table Builder"; }
    }

    public override RCPA.IProcessor GetProcessor(TCGADatatableBuilderOptions options)
    {
      return new TCGADatatableBuilder(options);
    }
  }
}
