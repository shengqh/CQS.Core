using RCPA.Gui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace CQS.Sample
{
  public partial class SampleInformationForm : Form
  {
    public SampleInformationForm()
    {
      InitializeComponent();
    }

    //private IRawSampleInfoReader reader;

    private List<SampleItem> samples;

    public void SetDataSource(List<SampleItem> samples)
    {
      siGrids.DataSource = null;
      this.samples = samples;
      siGrids.DataSource = samples;

    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    public void SetColumns(IEnumerable<string> properties)
    {
      foreach (var propertyName in properties)
      {
        if (string.IsNullOrEmpty(propertyName))
        {
          continue;
        }

        var column = new AnnotationColumn();
        column.DataPropertyName = "Annotations";
        column.HeaderText = propertyName;
        column.Key = propertyName;
        column.Resizable = System.Windows.Forms.DataGridViewTriState.True;
        column.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
        siGrids.Columns.Add(column);
      }
    }

    private void siGrids_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
      if (e.ColumnIndex == colDataset.Index)
      {
        var link = DataLinkUtils.GetDatasetLink(siGrids.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string);
        Process.Start(link);
      }
      else if (e.ColumnIndex == colSampleLink.Index)
      {
        var link = DataLinkUtils.GetDataLink(siGrids.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string);
        Process.Start(link);
      }
    }

    public void SetDataset(string dataset)
    {
      colSampleLink.Visible = DataLinkUtils.IsDataLinkSupported(dataset);
      colSampleText.Visible = !colSampleLink.Visible;
    }
  }
}
