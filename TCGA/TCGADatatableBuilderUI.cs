﻿using RCPA;
using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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
      options.Platforms = GetSelectedPlatforms();
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

      if (GetSelectedPlatforms().Count == 0)
      {
        throw new ArgumentException("Select platform first!");
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

    private List<string> GetSelectedPlatforms()
    {
      List<string> result = new List<string>();
      foreach (string obj in lbPlatforms.SelectedItems)
      {
        result.Add(obj);
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

    private void lbTumors_SelectedIndexChanged(object sender, EventArgs e)
    {
      FillPlatform();
    }

    private void FillPlatform()
    {
      lbPlatforms.BeginUpdate();
      try
      {
        var oldplatforms = GetSelectedPlatforms();

        lbPlatforms.Items.Clear();

        if (lbDataTypes.SelectedItem == null || !Directory.Exists(rootDir.FullName) || lbTumors.SelectedItem == null)
        {
          return;
        }

        var technology = TCGATechnology.Parse(lbDataTypes.SelectedItem as string);
        var tumors = GetSelectedTumors();

        var platforms = (from tumor in tumors
                         let dir = rootDir.FullName + "/" + tumor
                         let plats = Directory.GetDirectories(technology.GetTechnologyDirectory(dir))
                         from plat in plats
                         select Path.GetFileName(plat)).Distinct().OrderBy(m => m).ToList();

        platforms.ForEach(m => lbPlatforms.Items.Add(m));
        if (platforms.Count == 1)
        {
          lbPlatforms.SelectedIndex = 0;
        }
        else
        {
          if (oldplatforms.Count > 0)
          {
            for (int i = 0; i < lbPlatforms.Items.Count; i++)
            {
              if (oldplatforms.Contains(lbPlatforms.Items[i]))
              {
                lbPlatforms.SetSelected(i, true);
              }
            }
          }
        }
      }
      finally
      {
        lbPlatforms.EndUpdate();
      }
    }

  }
}
