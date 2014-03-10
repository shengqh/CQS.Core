namespace CQS.Sample
{
  partial class SampleInformationForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.siGrids = new System.Windows.Forms.DataGridView();
      this.breastCancerSampleItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnClose = new System.Windows.Forms.Button();
      this.colDataset = new System.Windows.Forms.DataGridViewLinkColumn();
      this.colSampleText = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colSampleLink = new System.Windows.Forms.DataGridViewLinkColumn();
      ((System.ComponentModel.ISupportInitialize)(this.siGrids)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.breastCancerSampleItemBindingSource)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // siGrids
      // 
      this.siGrids.AutoGenerateColumns = false;
      this.siGrids.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.siGrids.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDataset,
            this.colSampleText,
            this.colSampleLink});
      this.siGrids.DataSource = this.breastCancerSampleItemBindingSource;
      this.siGrids.Dock = System.Windows.Forms.DockStyle.Fill;
      this.siGrids.Location = new System.Drawing.Point(0, 0);
      this.siGrids.Name = "siGrids";
      this.siGrids.Size = new System.Drawing.Size(599, 282);
      this.siGrids.TabIndex = 1;
      this.siGrids.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.siGrids_CellContentClick);
      // 
      // breastCancerSampleItemBindingSource
      // 
      this.breastCancerSampleItemBindingSource.DataSource = typeof(CQS.BreastCancer.BreastCancerSampleItem);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnClose);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 282);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(599, 35);
      this.panel1.TabIndex = 2;
      // 
      // btnClose
      // 
      this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
      this.btnClose.Location = new System.Drawing.Point(511, 0);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(88, 35);
      this.btnClose.TabIndex = 0;
      this.btnClose.Text = "&Close";
      this.btnClose.UseVisualStyleBackColor = true;
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // colDataset
      // 
      this.colDataset.DataPropertyName = "Dataset";
      this.colDataset.HeaderText = "Dataset";
      this.colDataset.Name = "colDataset";
      this.colDataset.ReadOnly = true;
      this.colDataset.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.colDataset.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      // 
      // colSampleText
      // 
      this.colSampleText.DataPropertyName = "Sample";
      this.colSampleText.HeaderText = "Sample";
      this.colSampleText.Name = "colSampleText";
      // 
      // colSampleLink
      // 
      this.colSampleLink.DataPropertyName = "Sample";
      this.colSampleLink.HeaderText = "Sample";
      this.colSampleLink.Name = "colSampleLink";
      this.colSampleLink.ReadOnly = true;
      this.colSampleLink.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.colSampleLink.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      // 
      // SampleInformationForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(599, 317);
      this.Controls.Add(this.siGrids);
      this.Controls.Add(this.panel1);
      this.Name = "SampleInformationForm";
      this.Text = "BreastCancerSampleInformationForm";
      ((System.ComponentModel.ISupportInitialize)(this.siGrids)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.breastCancerSampleItemBindingSource)).EndInit();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.BindingSource breastCancerSampleItemBindingSource;
    private System.Windows.Forms.DataGridView siGrids;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.DataGridViewLinkColumn colDataset;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSampleText;
    private System.Windows.Forms.DataGridViewLinkColumn colSampleLink;
  }
}