using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RCPA.Gui;
using RCPA.Gui.FileArgument;
using RCPA.Gui.Command;
using RCPA;

namespace CQS.TCGA.Microarray
{
  public partial class MicroarrayDataSummaryBuilderUI : AbstractFileProcessorUI
  {
    private static readonly string title = " Microarray Data Summary Builder";
    private static readonly string version = "1.0.1";

    private RcpaFileField itraqFile;
    private SaveFileArgument targetFile;

    public MicroarrayDataSummaryBuilderUI()
    {
      InitializeComponent();

      base.SetDirectoryArgument("DataDirectory", "Microarray Level3 Data Root");

      this.itraqFile = new RcpaFileField(btnDatabase, txtDatabase, "SdrfFile", new OpenFileArgument("Metadata Sdrf", "sdrf.txt"), true);
      AddComponent(this.itraqFile);

      this.targetFile = new SaveFileArgument("Result", "tsv");

      this.Text = Constants.GetSQHTitle(title, version);
    }

    protected override string GetOriginFile()
    {
      var dlg = targetFile.GetFileDialog();
      if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        return dlg.FileName;
      }
      return null;
    }

    protected override IFileProcessor GetFileProcessor()
    {
      return new MicroarrayDataSummaryBuilder(base.GetOriginFile(), itraqFile.FullName);
    }

    public class Command : IToolCommand
    {
      #region IToolCommand Members

      public string GetClassification()
      {
        return "TCGA";
      }

      public string GetCaption()
      {
        return title;
      }

      public string GetVersion()
      {
        return version;
      }

      public void Run()
      {
        new MicroarrayDataSummaryBuilderUI().MyShow();
      }

      #endregion
    }
  }
}
