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

namespace CQS.TCGA
{
  public partial class TCGATreeBuilderUI : AbstractProcessorUI
  {
    public static readonly string title = " TCGA Data Tree Builder";
    public static readonly string version = "1.0.0";

    public TCGATreeBuilderUI()
    {
      InitializeComponent();

      targetDir.SetDirectoryArgument("DataDirectory", "TCGA Data Target");

      this.Text = Constants.GetSQHTitle(title, version);
    }

    protected override IProcessor GetProcessor()
    {
      var options = new TCGATreeBuilderOptions()
      {
        OutputDirectory = targetDir.FullName
      }; 
      return new TCGATreeBuilder(options);
    }
  }
}
