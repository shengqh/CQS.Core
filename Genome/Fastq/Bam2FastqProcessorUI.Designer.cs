namespace CQS.Genome.Fastq
{
  partial class Bam2FastqProcessorUI
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
      this.bamFile = new RCPA.Gui.FileField();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 77);
      this.lblProgress.Size = new System.Drawing.Size(1196, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 100);
      this.progressBar.Size = new System.Drawing.Size(1196, 23);
      // 
      // btnClose
      // 
      this.btnClose.Location = new System.Drawing.Point(646, 8);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(561, 8);
      // 
      // btnGo
      // 
      this.btnGo.Location = new System.Drawing.Point(476, 8);
      // 
      // bamFile
      // 
      this.bamFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.bamFile.FullName = "";
      this.bamFile.Key = "BamFile";
      this.bamFile.Location = new System.Drawing.Point(12, 23);
      this.bamFile.Name = "bamFile";
      this.bamFile.OpenButtonText = "Browse All File ...";
      this.bamFile.WidthOpenButton = 226;
      this.bamFile.PreCondition = null;
      this.bamFile.Size = new System.Drawing.Size(1172, 23);
      this.bamFile.TabIndex = 9;
      // 
      // Bam2FastqProcessorUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1196, 162);
      this.Controls.Add(this.bamFile);
      this.MaximumSize = new System.Drawing.Size(10000, 200);
      this.MinimumSize = new System.Drawing.Size(800, 200);
      this.Name = "Bam2FastqProcessorUI";
      this.TabText = "";
      this.Text = "TCGATreeBuilderUI";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.bamFile, 0);
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.FileField bamFile;


  }
}