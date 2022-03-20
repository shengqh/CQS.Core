using CQS.Converter;
using CQS.Sample;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace CQS.BreastCancer
{
  public partial class BreastCancerSampleInformationForm : Form
  {
    public BreastCancerSampleInformationForm()
    {
      InitializeComponent();

      var headers = ConverterUtils.GetItems<BreastCancerSampleItem, SampleInfoAttribute>();
      foreach (var header in headers)
      {
        var column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = header.PropertyName;
        column.HeaderText = header.PropertyName;
        column.Resizable = System.Windows.Forms.DataGridViewTriState.True;
        column.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
        siGrids.Columns.Add(column);
      }
    }

    private IRawSampleInfoReader reader;

    private List<BreastCancerSampleItem> samples;

    public void SetDataSource(List<BreastCancerSampleItem> samples)
    {
      siGrids.DataSource = null;
      this.samples = samples;
      siGrids.DataSource = samples;

    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      this.Close();
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

    public void SetRawInfoReader(IRawSampleInfoReader reader, string dataset)
    {
      this.reader = reader;

      colSampleLink.Visible = DataLinkUtils.IsDataLinkSupported(dataset);
      colSampleText.Visible = !colSampleLink.Visible;
    }
  }
}
