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
      this.propertyFile = new RCPA.Gui.FileField();
      this.rootDirectory = new RCPA.Gui.DirectoryField();
      this.pnlButton.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 77);
      this.lblProgress.Size = new System.Drawing.Size(955, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 100);
      this.progressBar.Size = new System.Drawing.Size(955, 23);
      // 
      // pnlButton
      // 
      this.pnlButton.Location = new System.Drawing.Point(0, 123);
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
      // propertyFile
      // 
      this.propertyFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.propertyFile.FullName = "";
      this.propertyFile.Key = "PropertyFile";
      this.propertyFile.Location = new System.Drawing.Point(3, 41);
      this.propertyFile.Name = "propertyFile";
      this.propertyFile.OpenButtonText = "Browse All File ...";
      this.propertyFile.PreCondition = null;
      this.propertyFile.Size = new System.Drawing.Size(947, 23);
      this.propertyFile.TabIndex = 10;
      this.propertyFile.WidthOpenButton = 250;
      // 
      // rootDirectory
      // 
      this.rootDirectory.FullName = "";
      this.rootDirectory.Key = "RootDirectory";
      this.rootDirectory.Location = new System.Drawing.Point(3, 12);
      this.rootDirectory.Name = "rootDirectory";
      this.rootDirectory.OpenButtonText = "Browse  Directory ...";
      this.rootDirectory.OpenButtonWidth = 250;
      this.rootDirectory.PreCondition = null;
      this.rootDirectory.Size = new System.Drawing.Size(947, 23);
      this.rootDirectory.TabIndex = 11;
      // 
      // SampleInfoBuilderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 162);
      this.Controls.Add(this.rootDirectory);
      this.Controls.Add(this.propertyFile);
      this.Name = "SampleInfoBuilderUI";
      this.Text = "BreastCancerSampleInformationBuilderUI";
      this.Controls.SetChildIndex(this.pnlButton, 0);
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.propertyFile, 0);
      this.Controls.SetChildIndex(this.rootDirectory, 0);
      this.pnlButton.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.FileField propertyFile;
    private RCPA.Gui.DirectoryField rootDirectory;
  }
}