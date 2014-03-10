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

namespace CQS.Sample
{
  public partial class SampleInfoBuilderUI : AbstractFileProcessorUI
  {
    public static string title = "GEO Sample Info Builder";
    public static string version = "1.0.0";

    public SampleInfoBuilderUI()
    {
      InitializeComponent();

      SetDirectoryArgument("DatasetRootDirectory", "Dataset Root");
      columnFiles.FileArgument = new OpenFileArgument("Column Definition", ".columns");

      this.Text = Constants.GetSqhVanderbiltTitle(title, version);
    }

    protected override RCPA.IFileProcessor GetFileProcessor()
    {
      return new SampleInfoBuilder(columnFiles.FullName);
    }

    public class Command : IToolCommand
    {
      #region IToolCommand Members

      public string GetClassification()
      {
        return "Sample";
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
        new SampleInfoBuilderUI().MyShow();
      }

      #endregion
    }
  }
}
