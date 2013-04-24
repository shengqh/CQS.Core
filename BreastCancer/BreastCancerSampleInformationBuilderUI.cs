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

namespace CQS.BreastCancer
{
  public partial class BreastCancerSampleInformationBuilderUI : AbstractFileProcessorUI
  {
    public static string title = "Breast Cancer Sample Info Builder";
    public static string version = "1.0.0";

    public BreastCancerSampleInformationBuilderUI()
    {
      InitializeComponent();

      SetDirectoryArgument("DatasetRootDirectory", "Dataset Root");

      this.Text = Constants.GetSqhVanderbiltTitle(title, version);
    }

    protected override RCPA.IFileProcessor GetFileProcessor()
    {
      return new BreastCancerSampleInformationBuilder();
    }

    public class Command : IToolCommand
    {
      #region IToolCommand Members

      public string GetClassification()
      {
        return "BreastCancer";
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
        new BreastCancerSampleInformationBuilderUI().MyShow();
      }

      #endregion
    }
  }
}
