namespace CQS.Genome.Mapping
{
  partial class MappedCountProcessorUI
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
      this.fileFastq = new RCPA.Gui.FileField();
      this.cbEngine = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.countFile = new RCPA.Gui.FileField();
      this.cbBedAsGtf = new RCPA.Gui.RcpaCheckField();
      this.bamFile = new RCPA.Gui.FileField();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 192);
      this.lblProgress.Size = new System.Drawing.Size(955, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 215);
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
      // fileFastq
      // 
      this.fileFastq.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fileFastq.FileArgument = null;
      this.fileFastq.FullName = "";
      this.fileFastq.Key = "FastqFile";
      this.fileFastq.Location = new System.Drawing.Point(12, 73);
      this.fileFastq.Name = "fileFastq";
      this.fileFastq.OpenButtonText = "Browse  ...";
      this.fileFastq.PreCondition = null;
      this.fileFastq.Required = false;
      this.fileFastq.Size = new System.Drawing.Size(919, 23);
      this.fileFastq.TabIndex = 12;
      this.fileFastq.WidthOpenButton = 246;
      // 
      // cbEngine
      // 
      this.cbEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbEngine.FormattingEnabled = true;
      this.cbEngine.Location = new System.Drawing.Point(258, 145);
      this.cbEngine.Name = "cbEngine";
      this.cbEngine.Size = new System.Drawing.Size(121, 21);
      this.cbEngine.TabIndex = 13;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(169, 148);
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
      this.cbBedAsGtf.Location = new System.Drawing.Point(418, 146);
      this.cbBedAsGtf.Name = "cbBedAsGtf";
      this.cbBedAsGtf.PreCondition = null;
      this.cbBedAsGtf.Size = new System.Drawing.Size(74, 17);
      this.cbBedAsGtf.TabIndex = 16;
      this.cbBedAsGtf.Text = "Bed as gtf";
      this.cbBedAsGtf.UseVisualStyleBackColor = true;
      // 
      // bamFile
      // 
      this.bamFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.bamFile.FileArgument = null;
      this.bamFile.FullName = "";
      this.bamFile.Key = "BamFile";
      this.bamFile.Location = new System.Drawing.Point(12, 12);
      this.bamFile.Name = "bamFile";
      this.bamFile.OpenButtonText = "Browse  ...";
      this.bamFile.PreCondition = null;
      this.bamFile.Size = new System.Drawing.Size(919, 23);
      this.bamFile.TabIndex = 19;
      this.bamFile.WidthOpenButton = 246;
      // 
      // MappedCountProcessorUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 277);
      this.Controls.Add(this.bamFile);
      this.Controls.Add(this.cbBedAsGtf);
      this.Controls.Add(this.countFile);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.cbEngine);
      this.Controls.Add(this.fileFastq);
      this.Controls.Add(this.gffFile);
      this.Name = "MappedCountProcessorUI";
      this.Text = "Mirna Count Processor";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.gffFile, 0);
      this.Controls.SetChildIndex(this.fileFastq, 0);
      this.Controls.SetChildIndex(this.cbEngine, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.countFile, 0);
      this.Controls.SetChildIndex(this.cbBedAsGtf, 0);
      this.Controls.SetChildIndex(this.bamFile, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private RCPA.Gui.FileField gffFile;
    private RCPA.Gui.FileField fileFastq;
    private System.Windows.Forms.ComboBox cbEngine;
    private System.Windows.Forms.Label label1;
    private RCPA.Gui.FileField countFile;
    private RCPA.Gui.RcpaCheckField cbBedAsGtf;
    private RCPA.Gui.FileField bamFile;

  }
}