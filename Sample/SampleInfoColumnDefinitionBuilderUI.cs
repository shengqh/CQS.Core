using RCPA.Gui;
using RCPA.Gui.Command;
using System;
using System.IO;
using System.Windows.Forms;

namespace CQS.Sample
{
  public partial class SampleInfoColumnDefinitionBuilderUI : AbstractUI
  {
    public static string Title = "GEO Sample Info Column Definition Builder";

    public static string Version = "1.0.0";

    public SampleInfoColumnDefinitionBuilderUI()
    {
      InitializeComponent();
      this.Text = Constants.GetSqhVanderbiltTitle(Title, Version);
    }

    protected override void DoRealGo()
    {
      if (dlgOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        txtColumns.Text = File.ReadAllLines(dlgOpen.FileName).Merge(Environment.NewLine);
        dlgSave.FileName = dlgOpen.FileName;
      }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      if (dlgSave.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        File.WriteAllText(dlgSave.FileName, txtColumns.Text);
        MessageBox.Show(this, "Columns saved to : " + dlgSave.FileName);
      }
    }
  }
}
