using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RCPA.Gui;
using RCPA.Gui.FileArgument;
using RCPA.Gui.Command;
using RCPA;

namespace CQS.Genome.Mirna
{
  public partial class MirnaToDNAConverterUI : AbstractFileProcessorUI
  {
    private static string title = "miRNA to DNA database converter";
    private static string version = "1.0.0";

    public MirnaToDNAConverterUI()
    {
      InitializeComponent();

      this.SetFileArgument("miRNAFile", new OpenFileArgument("miRNA Database", new string[] {"fa","fasta" }));

      this.Text = Constants.GetSQHTitle(title, version);
    }

    protected override IFileProcessor GetFileProcessor()
    {
      return new MirnaToDNAConverter();
    }

    public class Command : IToolCommand
    {
      #region IToolCommand Members

      public string GetClassification()
      {
        return MenuCommandType.Database;
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
        new MirnaToDNAConverterUI().MyShow();
      }

      #endregion
    }
  }
}
