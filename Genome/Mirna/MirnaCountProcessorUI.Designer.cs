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
      this.fileFastq = new RCPA.Gui.FileField();
      this.cbEngine = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.countFile = new RCPA.Gui.FileField();
      this.cbBedAsGtf = new RCPA.Gui.RcpaCheckField();
      this.fileFasta = new RCPA.Gui.FileField();
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
      this.lblProgress.Location = new System.Drawing.Point(0, 241);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 264);
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
      this.cbBedAsGtf.Location = new System.Drawing.Point(418, 180);
      this.cbBedAsGtf.Name = "cbBedAsGtf";
      this.cbBedAsGtf.PreCondition = null;
      this.cbBedAsGtf.Size = new System.Drawing.Size(74, 17);
      this.cbBedAsGtf.TabIndex = 16;
      this.cbBedAsGtf.Text = "Bed as gtf";
      this.cbBedAsGtf.UseVisualStyleBackColor = true;
      // 
      // fileFasta
      // 
      this.fileFasta.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fileFasta.FileArgument = null;
      this.fileFasta.FullName = "";
      this.fileFasta.Key = "FastaFile";
      this.fileFasta.Location = new System.Drawing.Point(12, 131);
      this.fileFasta.Name = "fileFasta";
      this.fileFasta.OpenButtonText = "Browse  ...";
      this.fileFasta.WidthOpenButton = 246;
      this.fileFasta.PreCondition = null;
      this.fileFasta.Required = false;
      this.fileFasta.Size = new System.Drawing.Size(919, 23);
      this.fileFasta.TabIndex = 17;
      // 
      // MirnaCountProcessorUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 326);
      this.Controls.Add(this.fileFasta);
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
      this.Controls.SetChildIndex(this.fileFasta, 0);
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
    private RCPA.Gui.FileField fileFasta;

  }
}