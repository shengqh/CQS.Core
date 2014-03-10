namespace CQS.Sample
{
  partial class SampleInfoBuilderUI
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
      this.columnFiles = new RCPA.Gui.FileField();
      this.pnlFile.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlFile
      // 
      this.pnlFile.Location = new System.Drawing.Point(3, 13);
      this.pnlFile.Size = new System.Drawing.Size(947, 24);
      // 
      // txtOriginalFile
      // 
      this.txtOriginalFile.Size = new System.Drawing.Size(701, 20);
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 79);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 102);
      // 
      // columnFiles
      // 
      this.columnFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.columnFiles.FullName = "";
      this.columnFiles.Key = "FileField";
      this.columnFiles.Location = new System.Drawing.Point(3, 43);
      this.columnFiles.Name = "columnFiles";
      this.columnFiles.OpenButtonText = "Browse All File ...";
      this.columnFiles.WidthOpenButton = 246;
      this.columnFiles.PreCondition = null;
      this.columnFiles.Size = new System.Drawing.Size(947, 23);
      this.columnFiles.TabIndex = 10;
      // 
      // SampleInfoBuilderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 164);
      this.Controls.Add(this.columnFiles);
      this.Name = "SampleInfoBuilderUI";
      this.Text = "BreastCancerSampleInformationBuilderUI";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.pnlFile, 0);
      this.Controls.SetChildIndex(this.columnFiles, 0);
      this.pnlFile.ResumeLayout(false);
      this.pnlFile.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.FileField columnFiles;
  }
}