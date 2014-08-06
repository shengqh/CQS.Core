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
using RCPA.Utils;
using System.IO;

namespace CQS.TCGA
{
  public partial class TCGADatatableBuilderUI : AbstractProcessorUI
  {
    public static readonly string title = " TCGA Data Table Builder";
    public static readonly string version = "1.0.1";

    private List<object> tumors = new List<object>();

    public TCGADatatableBuilderUI()
    {
      InitializeComponent();

      rootDir.SetDirectoryArgument("TCGARoot", "TCGA Data Root");

      targetFile.FileArgument = new SaveFileArgument("Target Data", "tsv");

      lbDataTypes.Items.AddRange(TCGATechnology.GetTechnologyNames().ToArray());

      lbSampleTypes.Items.AddRange(TCGASampleCode.GetSampleCodes().OrderBy(m => m.Code).ToList().ConvertAll(m => string.Format("{0}, {1}", m.ShortLetterCode, m.Definition)).ToArray());

      this.Text = Constants.GetSQHTitle(title, version);
    }

    protected override IProcessor GetProcessor()
    {
      TCGADatatableBuilderOptions options = new TCGADatatableBuilderOptions();

      options.DataType = lbDataTypes.SelectedItem as string;
      options.TCGADirectory = rootDir.FullName;
      options.TumorTypes = GetSelectedTumors();
      options.OutputFile = targetFile.FullName;
      options.IsCount = cbCount.Checked;
      options.TCGASampleCodeStrings = GetSampleCodes();
      options.WithClinicalInformationOnly = cbClinicalOnly.Checked;

      return new TCGADatatableBuilder(options);
    }

    private IList<string> GetSampleCodes()
    {
      List<string> result = new List<string>();
      foreach (string sc in lbSampleTypes.SelectedItems)
      {
        result.Add(sc.StringBefore(","));
      }
      return result;
    }

    protected override void DoBeforeValidate()
    {
      base.DoBeforeValidate();
      if (lbDataTypes.SelectedIndex == -1)
      {
        throw new ArgumentException("Select data type first!");
      }

      if (GetSelectedTumors().Count == 0)
      {
        throw new ArgumentException("Select tumor type first!");
      }

      if (GetSampleCodes().Count == 0)
      {
        throw new ArgumentException("Select sample type first!");
      }
    }

    private List<string> GetSelectedTumors()
    {
      List<string> result = new List<string>();
      foreach (string obj in lbTumors.SelectedItems)
      {
        result.Add(obj.StringBefore(","));
      }
      return result;
    }

    private void btnLoad_Click(object sender, EventArgs e)
    {
      if (!Directory.Exists(rootDir.FullName))
      {
        MessageBox.Show(this, string.Format("TCGA data root directory {0} not exists", rootDir.FullName));
        return;
      }

      tumors = (from dir in Directory.GetDirectories(rootDir.FullName)
                where Directory.Exists(dir + "/data")
                select Path.GetFileName(dir) as object).ToList();

      FillTumor();
    }

    private void FillTumor()
    {
      var map = TCGAUtils.GetTumorDescriptionMap();

      object[] curitems;
      if (lbDataTypes.SelectedItem != null && Directory.Exists(rootDir.FullName))
      {
        var tecname = lbDataTypes.SelectedItem as string;
        var selected = TCGATechnology.Parse(tecname);
        curitems = (from tumor in this.tumors
                    let dir = rootDir.FullName + "/" + tumor
                    where Directory.Exists(selected.GetTechnologyDirectory(dir))
                    select tumor).ToArray();
      }
      else
      {
        curitems = this.tumors.ToArray();
      }

      lbTumors.BeginUpdate();
      try
      {
        var selected = new HashSet<string>(GetSelectedTumors().ConvertAll(m => m as string));
        lbTumors.Items.Clear();
        foreach (string item in curitems)
        {
          var name = map.ContainsKey(item.ToUpper()) ? item + ", " + map[item.ToUpper()] : item;
          lbTumors.Items.Add(name);
        }
        if (selected.Count > 0)
        {
          for (int i = 0; i < lbTumors.Items.Count; i++)
          {
            var name = curitems[i] as string;
            if (selected.Contains(name))
            {
              lbTumors.SetSelected(i, true);
            }
          }
        }
      }
      finally
      {
        lbTumors.EndUpdate();
      }
    }

    protected override void MyAfterLoadOption(object sender, EventArgs e)
    {
      base.MyAfterLoadOption(sender, e);

      if (Directory.Exists(rootDir.FullName))
      {
        btnLoad_Click(null, null);
      }
    }

    private void lbDataTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
      FillTumor();
    }
  }
}
