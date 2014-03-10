namespace CQS.Genome.Mirna
{
  partial class MirnaMappedOverlapBuilderUI
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
      this.refFile = new RCPA.Gui.FileField();
      this.samFile = new RCPA.Gui.FileField();
      this.fileTarget = new RCPA.Gui.FileField();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 116);
      this.lblProgress.Size = new System.Drawing.Size(955, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 139);
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
      // refFile
      // 
      this.refFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.refFile.FileArgument = null;
      this.refFile.FullName = "";
      this.refFile.Key = "RefFile";
      this.refFile.Location = new System.Drawing.Point(12, 43);
      this.refFile.Name = "refFile";
      this.refFile.OpenButtonText = "Browse  ...";
      this.refFile.WidthOpenButton = 246;
      this.refFile.PreCondition = null;
      this.refFile.Size = new System.Drawing.Size(919, 23);
      this.refFile.TabIndex = 11;
      // 
      // samFile
      // 
      this.samFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.samFile.FileArgument = null;
      this.samFile.FullName = "";
      this.samFile.Key = "SampleFile";
      this.samFile.Location = new System.Drawing.Point(12, 73);
      this.samFile.Name = "samFile";
      this.samFile.OpenButtonText = "Browse  ...";
      this.samFile.WidthOpenButton = 246;
      this.samFile.PreCondition = null;
      this.samFile.Required = false;
      this.samFile.Size = new System.Drawing.Size(919, 23);
      this.samFile.TabIndex = 12;
      // 
      // fileTarget
      // 
      this.fileTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fileTarget.FileArgument = null;
      this.fileTarget.FullName = "";
      this.fileTarget.Key = "TargetFile";
      this.fileTarget.Location = new System.Drawing.Point(12, 14);
      this.fileTarget.Name = "fileTarget";
      this.fileTarget.OpenButtonText = "Browse  ...";
      this.fileTarget.WidthOpenButton = 246;
      this.fileTarget.PreCondition = null;
      this.fileTarget.Size = new System.Drawing.Size(919, 23);
      this.fileTarget.TabIndex = 13;
      // 
      // MirnaMappedOverlapBuilderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(955, 201);
      this.Controls.Add(this.fileTarget);
      this.Controls.Add(this.samFile);
      this.Controls.Add(this.refFile);
      this.Name = "MirnaMappedOverlapBuilderUI";
      this.Text = "ReadGroupTrackingExtractorUI";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.refFile, 0);
      this.Controls.SetChildIndex(this.samFile, 0);
      this.Controls.SetChildIndex(this.fileTarget, 0);
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.FileField refFile;
    private RCPA.Gui.FileField samFile;
    private RCPA.Gui.FileField fileTarget;

  }
}