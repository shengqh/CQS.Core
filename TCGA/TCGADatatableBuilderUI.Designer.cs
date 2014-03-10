namespace CQS.TCGA
{
  partial class TCGADatatableBuilderUI
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
      this.btnLoad = new System.Windows.Forms.Button();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.lbDataTypes = new System.Windows.Forms.ListBox();
      this.label1 = new System.Windows.Forms.Label();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.lbTumors = new System.Windows.Forms.ListBox();
      this.label2 = new System.Windows.Forms.Label();
      this.lbSampleTypes = new System.Windows.Forms.ListBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cbCount = new RCPA.Gui.RcpaCheckField();
      this.rootDir = new RCPA.Gui.DirectoryField();
      this.targetFile = new RCPA.Gui.FileField();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 568);
      this.lblProgress.Size = new System.Drawing.Size(1166, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 591);
      this.progressBar.Size = new System.Drawing.Size(1166, 23);
      // 
      // btnClose
      // 
      this.btnClose.Location = new System.Drawing.Point(631, 8);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(546, 8);
      // 
      // btnGo
      // 
      this.btnGo.Location = new System.Drawing.Point(461, 8);
      // 
      // btnLoad
      // 
      this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLoad.Location = new System.Drawing.Point(1043, 53);
      this.btnLoad.Name = "btnLoad";
      this.btnLoad.Size = new System.Drawing.Size(97, 23);
      this.btnLoad.TabIndex = 12;
      this.btnLoad.Text = "Load Tumors";
      this.btnLoad.UseVisualStyleBackColor = true;
      this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.splitContainer1.Location = new System.Drawing.Point(24, 97);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.lbDataTypes);
      this.splitContainer1.Panel1.Controls.Add(this.label1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
      this.splitContainer1.Size = new System.Drawing.Size(1116, 410);
      this.splitContainer1.SplitterDistance = 372;
      this.splitContainer1.TabIndex = 13;
      // 
      // lbDataTypes
      // 
      this.lbDataTypes.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lbDataTypes.FormattingEnabled = true;
      this.lbDataTypes.Location = new System.Drawing.Point(0, 13);
      this.lbDataTypes.Name = "lbDataTypes";
      this.lbDataTypes.Size = new System.Drawing.Size(372, 397);
      this.lbDataTypes.TabIndex = 1;
      this.lbDataTypes.SelectedIndexChanged += new System.EventHandler(this.lbDataTypes_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Top;
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(372, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Select data type (only one data type allowed)";
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.lbTumors);
      this.splitContainer2.Panel1.Controls.Add(this.label2);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.lbSampleTypes);
      this.splitContainer2.Panel2.Controls.Add(this.label3);
      this.splitContainer2.Size = new System.Drawing.Size(740, 410);
      this.splitContainer2.SplitterDistance = 383;
      this.splitContainer2.TabIndex = 0;
      // 
      // lbTumors
      // 
      this.lbTumors.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lbTumors.FormattingEnabled = true;
      this.lbTumors.Location = new System.Drawing.Point(0, 13);
      this.lbTumors.Name = "lbTumors";
      this.lbTumors.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.lbTumors.Size = new System.Drawing.Size(383, 397);
      this.lbTumors.Sorted = true;
      this.lbTumors.TabIndex = 4;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Top;
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(383, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Select tumor types (ctrl+click for multiple selection)";
      // 
      // lbSampleTypes
      // 
      this.lbSampleTypes.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lbSampleTypes.FormattingEnabled = true;
      this.lbSampleTypes.Location = new System.Drawing.Point(0, 13);
      this.lbSampleTypes.Name = "lbSampleTypes";
      this.lbSampleTypes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.lbSampleTypes.Size = new System.Drawing.Size(353, 397);
      this.lbSampleTypes.TabIndex = 6;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Top;
      this.label3.Location = new System.Drawing.Point(0, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(353, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "Select sample types (ctrl+click for multiple selection)";
      // 
      // cbCount
      // 
      this.cbCount.AutoSize = true;
      this.cbCount.Key = "cbCount";
      this.cbCount.Location = new System.Drawing.Point(24, 522);
      this.cbCount.Name = "cbCount";
      this.cbCount.PreCondition = null;
      this.cbCount.Size = new System.Drawing.Size(94, 17);
      this.cbCount.TabIndex = 14;
      this.cbCount.Text = "Is count data?";
      this.cbCount.UseVisualStyleBackColor = true;
      // 
      // rootDir
      // 
      this.rootDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.rootDir.FullName = "";
      this.rootDir.Key = "RootDir";
      this.rootDir.Location = new System.Drawing.Point(24, 53);
      this.rootDir.Name = "rootDir";
      this.rootDir.OpenButtonText = "Browse  Directory ...";
      this.rootDir.OpenButtonWidth = 341;
      this.rootDir.PreCondition = null;
      this.rootDir.Size = new System.Drawing.Size(1002, 23);
      this.rootDir.TabIndex = 17;
      // 
      // targetFile
      // 
      this.targetFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.targetFile.FullName = "";
      this.targetFile.Key = "TargetFile";
      this.targetFile.Location = new System.Drawing.Point(24, 24);
      this.targetFile.Name = "targetFile";
      this.targetFile.OpenButtonText = "Browse All File ...";
      this.targetFile.WidthOpenButton = 341;
      this.targetFile.PreCondition = null;
      this.targetFile.Size = new System.Drawing.Size(1116, 23);
      this.targetFile.TabIndex = 18;
      // 
      // TCGADatatableBuilderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1166, 653);
      this.Controls.Add(this.targetFile);
      this.Controls.Add(this.rootDir);
      this.Controls.Add(this.cbCount);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.btnLoad);
      this.MinimumSize = new System.Drawing.Size(800, 250);
      this.Name = "TCGADatatableBuilderUI";
      this.TabText = "TCGA Downloader";
      this.Text = "TCGA Downloader";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.btnLoad, 0);
      this.Controls.SetChildIndex(this.splitContainer1, 0);
      this.Controls.SetChildIndex(this.cbCount, 0);
      this.Controls.SetChildIndex(this.rootDir, 0);
      this.Controls.SetChildIndex(this.targetFile, 0);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnLoad;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.ListBox lbDataTypes;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.ListBox lbTumors;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ListBox lbSampleTypes;
    private System.Windows.Forms.Label label3;
    private RCPA.Gui.RcpaCheckField cbCount;
    private RCPA.Gui.DirectoryField rootDir;
    private RCPA.Gui.FileField targetFile;

  }
}