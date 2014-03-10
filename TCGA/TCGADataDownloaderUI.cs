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
  public partial class TCGADataDownloaderUI : AbstractProcessorUI
  {
    public static readonly string Title = " TCGA Data Downloader";
    public static readonly string Version = "1.0.0";

    private string _lastXml = null;

    public TCGADataDownloaderUI()
    {
      InitializeComponent();

      targetDir.SetDirectoryArgument("DataDirectory", "TCGA Data Target");
      xmlFile.FileArgument = new OpenFileArgument("TCGA Tree Xml Format", "xml");

      if (!SystemUtils.IsLinux)
      {
        zip7File.Visible = true;
        zip7File.Required = true;
        zip7File.FileArgument = new OpenFileArgument("7zip execute", "exe");
      }
      else
      {
        zip7File.Visible = false;
        zip7File.Required = false;
      }

      lbDataTypes.Items.AddRange(TCGATechnology.GetTechnologyNames().ToArray());

      this.Text = Constants.GetSQHTitle(Title, Version);
    }

    protected override IProcessor GetProcessor()
    {
      var options = new TCGADataDownloaderOptions
      {
        Technologies = GetSelectedTechnologies(),
        TumorTypes = GetSelectedTumors(),
        XmlFile = xmlFile.FullName,
        Zip7 = zip7File.FullName,
        OutputDirectory = targetDir.FullName
      };

      return new TCGADataDownloader(options);
    }

    protected override void DoBeforeValidate()
    {
      base.DoBeforeValidate();
      var selectedTecs = GetSelectedTechnologies();
      if (selectedTecs.Count == 0)
      {
        throw new ArgumentException("Select data type first!");
      }

      if (GetSelectedTumors().Count == 0)
      {
        throw new ArgumentException("Select tumor type first!");
      }
    }

    private List<string> GetSelectedTumors()
    {
      var result = new List<string>();
      foreach (string obj in lbTumors.SelectedItems)
      {
        result.Add(obj);
      }
      return result;
    }

    private List<ITCGATechnology> GetSelectedTechnologies()
    {
      var result = new List<ITCGATechnology>();
      foreach (string obj in lbDataTypes.SelectedItems)
      {
        result.Add(TCGATechnology.Parse(obj));
      }
      return result;
    }

    private SpiderTreeNode _rootNode;
    private List<object> _tumors = new List<object>();
    private void btnLoad_Click(object sender, EventArgs e)
    {
      if (!File.Exists(xmlFile.FullName))
      {
        MessageBox.Show(this, string.Format("Xml file {0} not exists", xmlFile.FullName));
        return;
      }

      _lastXml = xmlFile.FullName;
      _rootNode = new SpiderTreeNodeXmlFormat().ReadFromFile(xmlFile.FullName);
      _tumors = (from node in _rootNode.Nodes
                select node.Name as object).Distinct().ToList();
      FillTumor();
    }

    private void FillTumor()
    {
      if (!File.Exists(xmlFile.FullName))
      {
        return;
      }

      if (!xmlFile.FullName.Equals(_lastXml))
      {
        btnLoad_Click(null, null);
      }

      object[] curitems;
      if (lbDataTypes.SelectedItems.Count == 0)
      {
        curitems = this._tumors.ToArray();
      }
      else
      {
        List<ITCGATechnology> selected = GetSelectedTechnologies();
        curitems = (from subnode in _rootNode.Nodes
                    let nodes = subnode.FindDeepestNode(m => selected.Any(n => n.IsData(m)))
                    where nodes.Count > 0
                    select subnode.Name as object).Distinct().ToArray();
      }

      lbTumors.BeginUpdate();
      try
      {
        var selected = new HashSet<string>(GetSelectedTumors().ConvertAll(m => m as string));
        lbTumors.Items.Clear();
        lbTumors.Items.AddRange(curitems.ToArray());
        if (selected.Count > 0)
        {
          for (int i = 0; i < lbTumors.Items.Count; i++)
          {
            var name = lbTumors.Items[i] as string;
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

      if (File.Exists(xmlFile.FullName))
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
