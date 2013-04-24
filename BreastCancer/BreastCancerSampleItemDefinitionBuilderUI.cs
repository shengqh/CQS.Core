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

namespace CQS.BreastCancer
{
  public partial class BreastCancerSampleItemDefinitionBuilderUI : Form
  {
    private List<FileDefinitionItem> headers;

    private TextFileDefinition items = new TextFileDefinition();

    private BindingList<DefaultValue> defaultValues = new BindingList<DefaultValue>();

    private List<string> propertyNames;

    private string lastDirectory = String.Empty;

    public static string title = "Breast Cancer Sample Info Definition Builder";

    private FolderBrowser dlgOpenDirectory = new FolderBrowser();

    public BreastCancerSampleItemDefinitionBuilderUI()
    {
      InitializeComponent();

      headers = ConverterUtils.GetItems<BreastCancerSampleItem, SampleInfo>();

      propertyNames = (from h in headers
                       orderby h.PropertyName
                       select h.PropertyName).ToList();

      defaultValues = new BindingList<DefaultValue>(
                        (from pn in propertyNames
                         select new DefaultValue()
                         {
                           PropertyName = pn,
                           Value = string.Empty
                         }).ToList());

      propertyNames.Insert(0, string.Empty);

      colPropertyName.Items.AddRange(propertyNames.ToArray());

      if (Directory.Exists(@"d:\projects\breastcancer\dataset"))
      {
        lastDirectory = @"d:\projects\breastcancer\dataset";
      }
      else
      {
        lastDirectory = FileUtils.GetTemplateDir().FullName;
      }

      this.Text = title;
    }

    private void btnInit_Click(object sender, EventArgs e)
    {
      if (!Directory.Exists(dlgOpenDirectory.SelectedPath))
      {
        dlgOpenDirectory.SelectedPath = lastDirectory;
      }

      if (dlgOpenDirectory.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        NewFromData(dlgOpenDirectory.SelectedPath);
      }
    }

    public void NewFromData(string subdir)
    {
      var siformat = Directory.GetFiles(subdir, "*.siformat");

      TextFileDefinition prefile = new TextFileDefinition();
      if (siformat.Length > 0)
      {
        prefile.ReadFromFile(siformat[0]);

        bool bFound = false;
        prefile.ForEach(m =>
        {
          if (m.PropertyName.Equals("TumorStage"))
          {
            m.PropertyName = "TumorStatus";
            bFound = true;
          }

          if (m.PropertyName.Equals("Metastasis"))
          {
            m.PropertyName = "MetastasisStatus";
            bFound = true;
          }
        });

        if (bFound)
        {
          prefile.WriteToFile(siformat[0]);
        }
      }

      var map = RawSampleInfoReaderFactory.GetReader(subdir).ReadDescriptionFromDirectory(subdir);

      lastDirectory = subdir;
      lastFile = String.Empty;

      var files = new HashSet<string>(from f in Directory.GetFiles(subdir, "*.CEL")
                                      select GeoUtils.GetGsmName(f));

      Dictionary<string, HashSet<string>> headers = new Dictionary<string, HashSet<string>>();
      foreach (var m in map)
      {
        var gsm = m.Key.ToLower();

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
      foreach (var olddv in prefile.DefaultValues)
      {
        if (propertyNames.Contains(olddv.PropertyName))
        {
          items.DefaultValues.Add(new DefaultValue()
          {
            PropertyName = olddv.PropertyName,
            Value = olddv.Value
          });
        }
      }

      items.Sort((m1, m2) => m1.AnnotationName.CompareTo(m2.AnnotationName));

      UpdateDataSource();

      this.Text = title + " - " + Path.GetFileName(subdir);
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
        return "1.0.0";
      }

      public void Run()
      {
        new BreastCancerSampleItemDefinitionBuilderUI().Show();
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
      ClearDataSource();

      items.ReadFromFile(fileName);

      items.RemoveAll(m => !propertyNames.Contains(m.PropertyName));
      items.DefaultValues.RemoveAll(m => !propertyNames.Contains(m.PropertyName));

      UpdateDataSource();

      lastFile = fileName;
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
      FormToDefinition();

      if (File.Exists(lastFile) && !File.Exists(dlgSaveFormatFile.FileName))
      {
        dlgSaveFormatFile.FileName = lastFile;
      }
      else
      {
        dlgSaveFormatFile.FileName = Path.Combine(lastDirectory, Path.GetFileName(lastDirectory) + ".siformat");
        dlgSaveFormatFile.InitialDirectory = lastDirectory;
      }

      if (dlgSaveFormatFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
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

        items.WriteToFile(dlgSaveFormatFile.FileName);
      }
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

        var parser = new PropertyMappingParser(items);
        var map = new Dictionary<string, BreastCancerSampleItem>();
        parser.ParseDataset(dlgOpenDirectory.SelectedPath, map);
        var lst = (from v in map.Values
                   orderby v.Sample
                   select v).ToList();

        var form = new BreastCancerSampleInformationForm();

        var reader = RawSampleInfoReaderFactory.GetReader(dlgOpenDirectory.SelectedPath);
        form.SetRawInfoReader(reader, Path.GetFileNameWithoutExtension(dlgOpenDirectory.SelectedPath));
        form.SetDataSource(lst);
        form.ShowDialog();
      }
    }
  }
}
