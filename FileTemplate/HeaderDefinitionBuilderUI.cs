using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RCPA.Format;
using System.IO;
using RCPA.Gui.Command;
using RCPA.Utils;
using CQS.Converter;
using RCPA;
using CQS.Ncbi.Geo;
using CQS.Sample;
using CQS.BreastCancer.parser;
using RCPA.Gui;

namespace CQS.FileTemplate
{
  public partial class HeaderDefinitionBuilderUI : Form
  {
    private HeaderDefinition headers = new HeaderDefinition();

    private string lastDirectory = String.Empty;

    public static string title = "Header Definition Builder";

    public HeaderDefinitionBuilderUI()
    {
      InitializeComponent();

      lastDirectory = FileUtils.GetTemplateDir().FullName;

      this.Text = title;
    }

    public class Command : IToolCommand
    {
      #region IToolCommand Members

      public string GetClassification()
      {
        return "Template";
      }

      public string GetCaption()
      {
        return title;
      }

      public string GetVersion()
      {
        return "1.0.0";
      }

      public void Run()
      {
        new HeaderDefinitionBuilderUI().Show();
      }

      #endregion
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      Close();
    }

    private string lastFile;
    private void btnLoad_Click(object sender, EventArgs e)
    {
      dlgOpenFormatFile.InitialDirectory = lastDirectory;

      if (dlgOpenFormatFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        var fileName = dlgOpenFormatFile.FileName;
        ReadFormatFile(fileName);
      }
    }

    public void ReadFormatFile(string fileName)
    {
      headers = HeaderDefinition.LoadFromFile(fileName);
      txtName.Text = headers.Name;
      txtProperties.Lines = headers.Properties.ToArray();

      lastFile = fileName;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
      FormToDefinition();

      if (File.Exists(lastFile) && !File.Exists(dlgSaveFormatFile.FileName))
      {
        dlgSaveFormatFile.FileName = lastFile;
      }
      else
      {
        dlgSaveFormatFile.FileName = Path.Combine(lastDirectory, txtName.Text + "." + dlgSaveFormatFile.DefaultExt );
        dlgSaveFormatFile.InitialDirectory = lastDirectory;
      }

      if (dlgSaveFormatFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        headers.Save(dlgSaveFormatFile.FileName);
        lastFile = dlgSaveFormatFile.FileName;
      }
    }

    private void FormToDefinition()
    {
      headers = new HeaderDefinition();
      headers.Name = txtName.Text;
      headers.Properties = (from line in txtProperties.Lines
                            let lt = line.Trim()
                            select lt).ToList();
    }

    private void txtName_TextChanged(object sender, EventArgs e)
    {
      CheckButtonState();
    }

    private void CheckButtonState()
    {
      btnSave.Enabled = txtName.Text.Trim().Length > 0 && txtProperties.Lines.Length > 0;
    }

    private void btnInit_Click(object sender, EventArgs e)
    {
      if (dlgOpenDataFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        using (StreamReader sr = new StreamReader(dlgOpenDataFile.FileName))
        {
          var line = sr.ReadLine();
          var parts = line.Split('\t');
          if (parts.Length == 1)
          {
            parts = line.Split(',');
          }
          this.txtName.Text = Path.GetFileNameWithoutExtension(dlgOpenDataFile.FileName);
          this.txtProperties.Lines = parts;
        }
      }
    }
  }
}
