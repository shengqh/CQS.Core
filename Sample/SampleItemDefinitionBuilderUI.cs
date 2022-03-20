using CQS.Microarray.Affymatrix;
using CQS.Ncbi.Geo;
using RCPA.Format;
using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace CQS.Sample
{
  public partial class SampleItemDefinitionBuilderUI : ComponentUI
  {
    private List<FileDefinitionItem> headers;

    private TextFileDefinition items = new TextFileDefinition();

    private BindingList<DefaultValue> defaultValues = new BindingList<DefaultValue>();

    private List<string> properties;

    [RcpaOption("LastDirectory", RcpaOptionType.String)]
    public string LastDirectory = String.Empty;

    public static string Title = "GEO Sample Info Definition Builder";
    public static string Version = "1.0.0";

    private FolderBrowser dlgOpenDirectory = new FolderBrowser();

    public SampleItemDefinitionBuilderUI()
    {
      InitializeComponent();

      columnFiles.FileArgument = new OpenFileArgument("Column Definition", ".columns");
      columnFiles.AfterBrowseFileEvent = btnColumnLoad_Click;

      this.Text = Constants.GetSqhVanderbiltTitle(Title, Version);
    }

    public override void LoadOption()
    {
      base.LoadOption();
      if (!File.Exists(columnFiles.FullName))
      {
        MessageBox.Show(this, "Column file is not exist : " + columnFiles.FullName);
        return;
      }
      DoLoadColumns();
    }

    private void btnColumnLoad_Click(object sender, EventArgs e)
    {
      if (!File.Exists(columnFiles.FullName))
      {
        MessageBox.Show(this, "Column file is not exist : " + columnFiles.FullName);
        return;
      }

      DoLoadColumns();
    }

    private void DoLoadColumns()
    {
      properties = SampleUtils.ReadPropertiesFromFile(columnFiles.FullName);

      defaultValues = new BindingList<DefaultValue>(
                        (from pn in properties
                         select new DefaultValue()
                         {
                           PropertyName = pn,
                           Value = string.Empty
                         }).ToList());

      properties.Insert(0, string.Empty);

      colPropertyName.Items.Clear();
      colPropertyName.Items.AddRange(properties.ToArray());

      headers = (from name in properties
                 select new FileDefinitionItem()
                 {
                   PropertyName = name
                 }).ToList();
    }


    private void btnInit_Click(object sender, EventArgs e)
    {
      if (!Directory.Exists(dlgOpenDirectory.SelectedPath))
      {
        dlgOpenDirectory.SelectedPath = LastDirectory;
      }

      if (dlgOpenDirectory.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
      {
        NewFromData(dlgOpenDirectory.SelectedPath);
      }
    }

    public void NewFromData(string subdir)
    {
      try
      {
        var siformat = Directory.GetFiles(subdir, "*.siformat");

        TextFileDefinition prefile = new TextFileDefinition();
        if (siformat.Length > 0)
        {
          prefile.ReadFromFile(siformat[0]);
        }

        var map = new RawSampleInfoReader().ReadDescriptionFromDirectory(subdir);

        LastDirectory = subdir;
        lastFile = String.Empty;

        var files = new HashSet<string>(from f in CelFile.GetCelFiles(subdir, false)
                                        select GeoUtils.GetGsmName(f));

        Dictionary<string, HashSet<string>> headers = new Dictionary<string, HashSet<string>>();
        foreach (var m in map)
        {
          var gsm = m.Key.ToUpper();

          if (!files.Contains(gsm))
          {
            continue;
          }

          var curmap = m.Value;

          foreach (var entry in curmap)
          {
            if (!headers.ContainsKey(entry.Key))
            {
              headers[entry.Key] = new HashSet<string>();
            }
            headers[entry.Key].UnionWith(entry.Value);
          }
        }

        ClearDataSource();

        items.Clear();
        foreach (var part in headers)
        {
          items.Add(new FileDefinitionItem()
          {
            AnnotationName = part.Key,
            Example = (from v in part.Value
                       orderby v
                       select v).Merge(";")
          });
        }

        foreach (var olditem in prefile)
        {
          if (!string.IsNullOrEmpty(olditem.PropertyName))
          {
            var newitem = items.Find(m => m.AnnotationName.Equals(olditem.AnnotationName));
            if (newitem != null)
            {
              newitem.PropertyName = olditem.PropertyName;
            }
          }
        }

        items.DefaultValues.Clear();

        items.Sort((m1, m2) => m1.AnnotationName.CompareTo(m2.AnnotationName));

        UpdateDataSource();

        label1.Text = "Annotation/property mapping - " + Path.GetFileName(subdir);

        dlgOpenDirectory.SelectedPath = subdir;
        dlgSaveFormatFile.FileName = Path.Combine(subdir, Path.GetFileName(subdir) + ".siformat");
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
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
        return Title;
      }

      public string GetVersion()
      {
        return "1.0.0";
      }

      public void Run()
      {
        new SampleItemDefinitionBuilderUI().MyShow();
      }

      #endregion
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      SaveOption();
      Close();
    }

    private string lastFile;
    private void btnLoad_Click(object sender, EventArgs e)
    {
      dlgOpenFormatFile.InitialDirectory = LastDirectory;

      if (dlgOpenFormatFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        var fileName = dlgOpenFormatFile.FileName;

        ReadFormatFile(fileName);
      }
    }

    public void ReadFormatFile(string fileName)
    {
      ClearDataSource();

      items.ReadFromFile(fileName);

      items.RemoveAll(m => !properties.Contains(m.PropertyName));
      items.DefaultValues.RemoveAll(m => !properties.Contains(m.PropertyName));

      UpdateDataSource();

      lastFile = fileName;

      label1.Text = "Annotation/property mapping - " + Path.GetFileNameWithoutExtension(fileName);

      dlgOpenDirectory.SelectedPath = Path.GetDirectoryName(fileName);
      dlgSaveFormatFile.FileName = fileName;

    }

    private void ClearDataSource()
    {
      gvItems.DataSource = null;
      dvGrids.DataSource = null;
    }

    private void UpdateDataSource()
    {
      gvItems.DataSource = items;

      foreach (var df in defaultValues)
      {
        var dv = items.DefaultValues.Find(m => m.PropertyName.Equals(df.PropertyName));
        if (dv != null)
        {
          df.Value = dv.Value;
        }
        else
        {
          df.Value = string.Empty;
        }
      }

      dvGrids.DataSource = defaultValues;
      btnTest.Enabled = true;
      btnSave.Enabled = true;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
      if (dlgSaveFormatFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        DoSaveToFile(dlgSaveFormatFile.FileName);
      }
    }

    private void DoSaveToFile(string filename)
    {
      FormToDefinition();

      var map = headers.ToDictionary(m => m.PropertyName);
      items.ForEach(m =>
      {
        if (!string.IsNullOrEmpty(m.PropertyName))
        {
          var item = map[m.PropertyName];
          m.ValueType = item.ValueType;
          m.Required = item.Required;
          m.Format = item.Format;
        }
      });

      items.WriteToFile(filename);
    }

    private void FormToDefinition()
    {
      items.DefaultValues.Clear();
      foreach (var df in defaultValues)
      {
        if (!string.IsNullOrEmpty(df.Value))
        {
          items.DefaultValues.Add(new DefaultValue()
          {
            PropertyName = df.PropertyName,
            Value = df.Value
          });
        }
      }
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
      if (dlgOpenDirectory.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        FormToDefinition();

        var parser = new SampleItemParser(items);
        var map = parser.ParseDataset(dlgOpenDirectory.SelectedPath);
        var lst = (from v in map.Values
                   orderby v.Sample
                   select v).ToList();

        var form = new SampleInformationForm();
        form.SetColumns(properties);
        form.SetDataset(Path.GetFileName(dlgOpenDirectory.SelectedPath));
        form.SetDataSource(lst);
        form.ShowDialog();
      }
    }

    private void btnSaveAndFirst_Click(object sender, EventArgs e)
    {
      if (TrySave())
      {
        var dirs = Directory.GetDirectories(Path.GetDirectoryName(LastDirectory));
        if (dirs.Length > 0)
        {
          NewFromData(dirs.First());
        }
        else
        {
          MessageBox.Show("No directory found!");
        }
      }
    }

    private void btnSaveAndPrev_Click(object sender, EventArgs e)
    {
      if (TrySave())
      {
        var dirs = Directory.GetDirectories(Path.GetDirectoryName(LastDirectory));
        var index = Array.IndexOf(dirs, LastDirectory);
        if (index > 0)
        {
          NewFromData(dirs[index - 1]);
        }
        else
        {
          MessageBox.Show("Current directory is the first one!");
        }
      }
    }

    private bool TrySave()
    {
      if (gvItems.DataSource == null)
      {
        return true;
      }

      if (string.IsNullOrEmpty(dlgSaveFormatFile.FileName))
      {
        if (dlgSaveFormatFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
          return false;
        }
      }

      DoSaveToFile(dlgSaveFormatFile.FileName);

      if (string.IsNullOrEmpty(LastDirectory))
      {
        btnInit.PerformClick();
        return false;
      }

      return true;
    }

    private void btnSaveAndNext_Click(object sender, EventArgs e)
    {
      if (TrySave())
      {
        var dirs = Directory.GetDirectories(Path.GetDirectoryName(LastDirectory));
        var index = Array.IndexOf(dirs, LastDirectory);
        if (index < dirs.Length - 1)
        {
          NewFromData(dirs[index + 1]);
        }
        else
        {
          MessageBox.Show("Current directory is the last one!");
        }
      }
    }

    private void btnSaveAndLast_Click(object sender, EventArgs e)
    {
      if (TrySave())
      {
        var dirs = Directory.GetDirectories(Path.GetDirectoryName(LastDirectory));
        if (dirs.Length > 0)
        {
          NewFromData(dirs.Last());
        }
        else
        {
          MessageBox.Show("No directory found!");
        }
      }
    }
  }
}
