namespace CQS.Genome.Mirna
{
  partial class MirnaCountProcessorUI
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
      this.gffFile = new RCPA.Gui.FileField();
      this.fastqFile = new RCPA.Gui.FileField();
      this.cbEngine = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.countFile = new RCPA.Gui.FileField();
      this.cbBedAsGtf = new RCPA.Gui.RcpaCheckField();
      this.fastaFile = new RCPA.Gui.FileField();
      this.bamFile = new RCPA.Gui.FileField();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 241);
      this.lblProgress.Size = new System.Drawing.Size(955, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 264);
      this.progressBar.Size = new System.Drawing.Size(955, 23);
      // 
      // btnClose
      // 
      this.btnClose.Location = new System.Drawing.Point(525, 8);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(440, 8);
      // 
      // btnGo
      // 
      this.btnGo.Location = new System.Drawing.Point(355, 8);
      // 
      // gffFile
      // 
      this.gffFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.gffFile.FileArgument = null;
      this.gffFile.FullName = "";
      this.gffFile.Key = "GffFile";
      this.gffFile.Location = new System.Drawing.Point(12, 43);
      this.gffFile.Name = "gffFile";
      this.gffFile.OpenButtonText = "Browse  ...";
      this.gffFile.PreCondition = null;
      this.gffFile.Size = new System.Drawing.Size(919, 23);
      this.gffFile.TabIndex = 11;
      this.gffFile.WidthOpenButton = 246;
      // 
      // fastqFile
      // 
      this.fastqFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fastqFile.FileArgument = null;
      this.fastqFile.FullName = "";
      this.fastqFile.Key = "FastqFile";
      this.fastqFile.Location = new System.Drawing.Point(12, 73);
      this.fastqFile.Name = "fastqFile";
      this.fastqFile.OpenButtonText = "Browse  ...";
      this.fastqFile.PreCondition = null;
      this.fastqFile.Required = false;
      this.fastqFile.Size = new System.Drawing.Size(919, 23);
      this.fastqFile.TabIndex = 12;
      this.fastqFile.WidthOpenButton = 246;
      // 
      // cbEngine
      // 
      this.cbEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbEngine.FormattingEnabled = true;
      this.cbEngine.Location = new System.Drawing.Point(258, 179);
      this.cbEngine.Name = "cbEngine";
      this.cbEngine.Size = new System.Drawing.Size(121, 21);
      this.cbEngine.TabIndex = 13;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(169, 182);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(83, 13);
      this.label1.TabIndex = 14;
      this.label1.Text = "Mapping engine";
      // 
      // countFile
      // 
      this.countFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.countFile.FileArgument = null;
      this.countFile.FullName = "";
      this.countFile.Key = "CountFile";
      this.countFile.Location = new System.Drawing.Point(12, 102);
      this.countFile.Name = "countFile";
      this.countFile.OpenButtonText = "Browse  ...";
      this.countFile.PreCondition = null;
      this.countFile.Required = false;
      this.countFile.Size = new System.Drawing.Size(919, 23);
      this.countFile.TabIndex = 15;
      this.countFile.WidthOpenButton = 246;
      // 
      // cbBedAsGtf
      // 
      this.cbBedAsGtf.AutoSize = true;
      this.cbBedAsGtf.Checked = true;
      this.cbBedAsGtf.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbBedAsGtf.Key = "BedAsGtf";
      this.cbBedAsGtf.Location = new System.Drawing.Point(418, 180);
      this.cbBedAsGtf.Name = "cbBedAsGtf";
      this.cbBedAsGtf.PreCondition = null;
      this.cbBedAsGtf.Size = new System.Drawing.Size(74, 17);
      this.cbBedAsGtf.TabIndex = 16;
      this.cbBedAsGtf.Text = "Bed as gtf";
      this.cbBedAsGtf.UseVisualStyleBackColor = true;
      // 
      // fastaFile
      // 
      this.fastaFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fastaFile.FileArgument = null;
      this.fastaFile.FullName = "";
      this.fastaFile.Key = "FastaFile";
      this.fastaFile.Location = new System.Drawing.Point(12, 131);
      this.fastaFile.Name = "fastaFile";
      this.fastaFile.OpenButtonText = "Browse  ...";
      this.fastaFile.PreCondition = null;
      this.fastaFile.Required = false;
      this.fastaFile.Size = new System.Drawing.Size(919, 23);
      this.fastaFile.TabIndex = 17;
      this.fastaFile.WidthOpenButton = 246;
      // 
      // bamFile
      // 
      this.bamFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.bamFile.FileArgument = null;
      this.bamFile.FullName = "";
      this.bamFile.Key = "BamFile";
      this.bamFile.Location = new System.Drawing.Point(12, 14);
      this.bamFile.Name = "bamFile";
      this.bamFile.OpenButtonText = "Browse  ...";
      this.bamFile.PreCondition = null;
      this.bamFile.Size = new System.Drawing.Size(919, 23);
      this.bamFile.TabIndex = 18;
      this.bamFile.WidthOpenButton = 246;
      // 
      // MirnaCountProcessorUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 326);
      this.Controls.Add(this.bamFile);
      this.Controls.Add(this.fastaFile);
      this.Controls.Add(this.cbBedAsGtf);
      this.Controls.Add(this.countFile);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.cbEngine);
      this.Controls.Add(this.fastqFile);
      this.Controls.Add(this.gffFile);
      this.Name = "MirnaCountProcessorUI";
      this.Text = "Mirna Count Processor";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.gffFile, 0);
      this.Controls.SetChildIndex(this.fastqFile, 0);
      this.Controls.SetChildIndex(this.cbEngine, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.countFile, 0);
      this.Controls.SetChildIndex(this.cbBedAsGtf, 0);
      this.Controls.SetChildIndex(this.fastaFile, 0);
      this.Controls.SetChildIndex(this.bamFile, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private RCPA.Gui.FileField gffFile;
    private RCPA.Gui.FileField fastqFile;
    private System.Windows.Forms.ComboBox cbEngine;
    private System.Windows.Forms.Label label1;
    private RCPA.Gui.FileField countFile;
    private RCPA.Gui.RcpaCheckField cbBedAsGtf;
    private RCPA.Gui.FileField fastaFile;
    private RCPA.Gui.FileField bamFile;

  }
}