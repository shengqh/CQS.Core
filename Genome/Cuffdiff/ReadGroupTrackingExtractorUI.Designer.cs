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
      this.trackingFile = new RCPA.Gui.FileField();
      this.pnlButton.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 111);
      this.lblProgress.Size = new System.Drawing.Size(955, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 134);
      this.progressBar.Size = new System.Drawing.Size(955, 23);
      // 
      // pnlButton
      // 
      this.pnlButton.Location = new System.Drawing.Point(0, 157);
      this.pnlButton.Size = new System.Drawing.Size(955, 39);
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
      // significantFile
      // 
      this.significantFile.AfterBrowseFileEvent = null;
      this.significantFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.significantFile.FileArgument = null;
      this.significantFile.FullName = "";
      this.significantFile.Key = "SignificantFile";
      this.significantFile.Location = new System.Drawing.Point(12, 43);
      this.significantFile.Name = "significantFile";
      this.significantFile.OpenButtonText = "Browse  ...";
      this.significantFile.PreCondition = null;
      this.significantFile.Required = false;
      this.significantFile.Size = new System.Drawing.Size(919, 23);
      this.significantFile.TabIndex = 10;
      this.significantFile.WidthOpenButton = 246;
      // 
      // mapFile
      // 
      this.mapFile.AfterBrowseFileEvent = null;
      this.mapFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mapFile.FileArgument = null;
      this.mapFile.FullName = "";
      this.mapFile.Key = "MapFile";
      this.mapFile.Location = new System.Drawing.Point(12, 73);
      this.mapFile.Name = "mapFile";
      this.mapFile.OpenButtonText = "Browse  ...";
      this.mapFile.PreCondition = null;
      this.mapFile.Required = false;
      this.mapFile.Size = new System.Drawing.Size(919, 23);
      this.mapFile.TabIndex = 11;
      this.mapFile.WidthOpenButton = 246;
      // 
      // trackingFile
      // 
      this.trackingFile.AfterBrowseFileEvent = null;
      this.trackingFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.trackingFile.FileArgument = null;
      this.trackingFile.FullName = "";
      this.trackingFile.Key = "SignificantFile";
      this.trackingFile.Location = new System.Drawing.Point(12, 12);
      this.trackingFile.Name = "trackingFile";
      this.trackingFile.OpenButtonText = "Browse  ...";
      this.trackingFile.PreCondition = null;
      this.trackingFile.Required = false;
      this.trackingFile.Size = new System.Drawing.Size(919, 23);
      this.trackingFile.TabIndex = 10;
      this.trackingFile.WidthOpenButton = 246;
      // 
      // ReadGroupTrackingExtractorUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 196);
      this.Controls.Add(this.mapFile);
      this.Controls.Add(this.trackingFile);
      this.Controls.Add(this.significantFile);
      this.Name = "ReadGroupTrackingExtractorUI";
      this.Text = "ReadGroupTrackingExtractorUI";
      this.Controls.SetChildIndex(this.pnlButton, 0);
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.significantFile, 0);
      this.Controls.SetChildIndex(this.trackingFile, 0);
      this.Controls.SetChildIndex(this.mapFile, 0);
      this.pnlButton.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.FileField significantFile;
    private RCPA.Gui.FileField mapFile;
    private RCPA.Gui.FileField trackingFile;
  }
}