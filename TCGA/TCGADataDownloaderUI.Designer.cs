namespace CQS.TCGA
{
  partial class TCGADataDownloaderUI
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
      this.xmlFile = new RCPA.Gui.FileField();
      this.zip7File = new RCPA.Gui.FileField();
      this.btnLoad = new System.Windows.Forms.Button();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.lbDataTypes = new System.Windows.Forms.ListBox();
      this.label1 = new System.Windows.Forms.Label();
      this.lbTumors = new System.Windows.Forms.ListBox();
      this.label2 = new System.Windows.Forms.Label();
      this.targetDir = new RCPA.Gui.DirectoryField();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 602);
      this.lblProgress.Size = new System.Drawing.Size(1166, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 625);
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
      // xmlFile
      // 
      this.xmlFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.xmlFile.FullName = "";
      this.xmlFile.Key = "XmlFile";
      this.xmlFile.Location = new System.Drawing.Point(24, 83);
      this.xmlFile.Name = "xmlFile";
      this.xmlFile.OpenButtonText = "Browse All File ...";
      this.xmlFile.WidthOpenButton = 341;
      this.xmlFile.PreCondition = null;
      this.xmlFile.Size = new System.Drawing.Size(1013, 23);
      this.xmlFile.TabIndex = 10;
      // 
      // zip7File
      // 
      this.zip7File.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.zip7File.FullName = "";
      this.zip7File.Key = "Zip7File";
      this.zip7File.Location = new System.Drawing.Point(24, 54);
      this.zip7File.Name = "zip7File";
      this.zip7File.OpenButtonText = "Browse All File ...";
      this.zip7File.WidthOpenButton = 341;
      this.zip7File.PreCondition = null;
      this.zip7File.Size = new System.Drawing.Size(1116, 23);
      this.zip7File.TabIndex = 11;
      // 
      // btnLoad
      // 
      this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLoad.Location = new System.Drawing.Point(1043, 83);
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
      this.splitContainer1.Location = new System.Drawing.Point(30, 127);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.lbDataTypes);
      this.splitContainer1.Panel1.Controls.Add(this.label1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.lbTumors);
      this.splitContainer1.Panel2.Controls.Add(this.label2);
      this.splitContainer1.Size = new System.Drawing.Size(1110, 450);
      this.splitContainer1.SplitterDistance = 370;
      this.splitContainer1.TabIndex = 13;
      // 
      // lbDataTypes
      // 
      this.lbDataTypes.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lbDataTypes.FormattingEnabled = true;
      this.lbDataTypes.Location = new System.Drawing.Point(0, 13);
      this.lbDataTypes.Name = "lbDataTypes";
      this.lbDataTypes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.lbDataTypes.Size = new System.Drawing.Size(370, 437);
      this.lbDataTypes.TabIndex = 1;
      this.lbDataTypes.SelectedIndexChanged += new System.EventHandler(this.lbDataTypes_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Top;
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(370, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Select data types (ctrl+click for multiple selection)";
      // 
      // lbTumors
      // 
      this.lbTumors.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lbTumors.FormattingEnabled = true;
      this.lbTumors.Location = new System.Drawing.Point(0, 13);
      this.lbTumors.Name = "lbTumors";
      this.lbTumors.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.lbTumors.Size = new System.Drawing.Size(736, 437);
      this.lbTumors.Sorted = true;
      this.lbTumors.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Top;
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(736, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Select tumor types (ctrl+click for multiple selection)";
      // 
      // targetDir
      // 
      this.targetDir.FullName = "";
      this.targetDir.Key = "TargetDir";
      this.targetDir.Location = new System.Drawing.Point(24, 25);
      this.targetDir.Name = "targetDir";
      this.targetDir.OpenButtonText = "Browse  Directory ...";
      this.targetDir.OpenButtonWidth = 341;
      this.targetDir.PreCondition = null;
      this.targetDir.Size = new System.Drawing.Size(1116, 23);
      this.targetDir.TabIndex = 14;
      // 
      // TCGADataDownloaderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1166, 687);
      this.Controls.Add(this.targetDir);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.btnLoad);
      this.Controls.Add(this.zip7File);
      this.Controls.Add(this.xmlFile);
      this.MinimumSize = new System.Drawing.Size(800, 250);
      this.Name = "TCGADataDownloaderUI";
      this.TabText = "TCGA Downloader";
      this.Text = "TCGA Downloader";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.xmlFile, 0);
      this.Controls.SetChildIndex(this.zip7File, 0);
      this.Controls.SetChildIndex(this.btnLoad, 0);
      this.Controls.SetChildIndex(this.splitContainer1, 0);
      this.Controls.SetChildIndex(this.targetDir, 0);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.FileField xmlFile;
    private RCPA.Gui.FileField zip7File;
    private System.Windows.Forms.Button btnLoad;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.ListBox lbDataTypes;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ListBox lbTumors;
    private System.Windows.Forms.Label label2;
    private RCPA.Gui.DirectoryField targetDir;

  }
}