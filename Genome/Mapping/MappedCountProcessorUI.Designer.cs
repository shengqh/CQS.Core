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
      this.pnlFile.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlFile
      // 
      this.pnlFile.Location = new System.Drawing.Point(12, 13);
      this.pnlFile.Size = new System.Drawing.Size(919, 24);
      // 
      // txtOriginalFile
      // 
      this.txtOriginalFile.Size = new System.Drawing.Size(673, 20);
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 192);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 215);
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
      this.gffFile.WidthOpenButton = 246;
      this.gffFile.PreCondition = null;
      this.gffFile.Size = new System.Drawing.Size(919, 23);
      this.gffFile.TabIndex = 11;
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
      this.fileFastq.WidthOpenButton = 246;
      this.fileFastq.PreCondition = null;
      this.fileFastq.Required = false;
      this.fileFastq.Size = new System.Drawing.Size(919, 23);
      this.fileFastq.TabIndex = 12;
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
      this.countFile.WidthOpenButton = 246;
      this.countFile.PreCondition = null;
      this.countFile.Required = false;
      this.countFile.Size = new System.Drawing.Size(919, 23);
      this.countFile.TabIndex = 15;
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
      // MirnaCountProcessorUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 277);
      this.Controls.Add(this.cbBedAsGtf);
      this.Controls.Add(this.countFile);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.cbEngine);
      this.Controls.Add(this.fileFastq);
      this.Controls.Add(this.gffFile);
      this.Name = "MirnaCountProcessorUI";
      this.Text = "Mirna Count Processor";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.pnlFile, 0);
      this.Controls.SetChildIndex(this.gffFile, 0);
      this.Controls.SetChildIndex(this.fileFastq, 0);
      this.Controls.SetChildIndex(this.cbEngine, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.countFile, 0);
      this.Controls.SetChildIndex(this.cbBedAsGtf, 0);
      this.pnlFile.ResumeLayout(false);
      this.pnlFile.PerformLayout();
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

  }
}