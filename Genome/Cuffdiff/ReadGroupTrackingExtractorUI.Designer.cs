namespace CQS.Genome.Cuffdiff
{
  partial class ReadGroupTrackingExtractorUI
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
      this.significantFile = new RCPA.Gui.FileField();
      this.mapFile = new RCPA.Gui.FileField();
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
      this.lblProgress.Location = new System.Drawing.Point(0, 111);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 134);
      // 
      // significantFile
      // 
      this.significantFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.significantFile.FileArgument = null;
      this.significantFile.FullName = "";
      this.significantFile.Key = "SignificantFile";
      this.significantFile.Location = new System.Drawing.Point(12, 43);
      this.significantFile.Name = "significantFile";
      this.significantFile.OpenButtonText = "Browse  ...";
      this.significantFile.WidthOpenButton = 246;
      this.significantFile.PreCondition = null;
      this.significantFile.Required = false;
      this.significantFile.Size = new System.Drawing.Size(919, 23);
      this.significantFile.TabIndex = 10;
      // 
      // mapFile
      // 
      this.mapFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mapFile.FileArgument = null;
      this.mapFile.FullName = "";
      this.mapFile.Key = "MapFile";
      this.mapFile.Location = new System.Drawing.Point(12, 73);
      this.mapFile.Name = "mapFile";
      this.mapFile.OpenButtonText = "Browse  ...";
      this.mapFile.WidthOpenButton = 246;
      this.mapFile.PreCondition = null;
      this.mapFile.Required = false;
      this.mapFile.Size = new System.Drawing.Size(919, 23);
      this.mapFile.TabIndex = 11;
      // 
      // ReadGroupTrackingExtractorUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 196);
      this.Controls.Add(this.mapFile);
      this.Controls.Add(this.significantFile);
      this.Name = "ReadGroupTrackingExtractorUI";
      this.Text = "ReadGroupTrackingExtractorUI";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.pnlFile, 0);
      this.Controls.SetChildIndex(this.significantFile, 0);
      this.Controls.SetChildIndex(this.mapFile, 0);
      this.pnlFile.ResumeLayout(false);
      this.pnlFile.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.FileField significantFile;
    private RCPA.Gui.FileField mapFile;
  }
}