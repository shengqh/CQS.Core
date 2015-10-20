namespace CQS.Microarray
{
  partial class GseMatrixDownloaderUI
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
      this.rootDirectory = new RCPA.Gui.DirectoryField();
      this.pnlButton.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 50);
      this.lblProgress.Size = new System.Drawing.Size(955, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 73);
      this.progressBar.Size = new System.Drawing.Size(955, 23);
      // 
      // pnlButton
      // 
      this.pnlButton.Location = new System.Drawing.Point(0, 96);
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
      // rootDirectory
      // 
      this.rootDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.rootDirectory.FullName = "";
      this.rootDirectory.Key = "RootDirectory";
      this.rootDirectory.Location = new System.Drawing.Point(3, 12);
      this.rootDirectory.Name = "rootDirectory";
      this.rootDirectory.OpenButtonText = "Browse  GSE Root Directory ...";
      this.rootDirectory.OpenButtonWidth = 226;
      this.rootDirectory.PreCondition = null;
      this.rootDirectory.Size = new System.Drawing.Size(947, 23);
      this.rootDirectory.TabIndex = 9;
      // 
      // GseMatrixDownloaderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 135);
      this.Controls.Add(this.rootDirectory);
      this.Name = "GseMatrixDownloaderUI";
      this.Text = "BreastCancerSampleInformationBuilderUI";
      this.Controls.SetChildIndex(this.pnlButton, 0);
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.rootDirectory, 0);
      this.pnlButton.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.DirectoryField rootDirectory;

  }
}