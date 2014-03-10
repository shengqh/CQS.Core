namespace CQS.TCGA
{
  partial class TCGATreeBuilderUI
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
      this.targetDir = new RCPA.Gui.DirectoryField();
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
      // targetDir
      // 
      this.targetDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.targetDir.FullName = "";
      this.targetDir.Key = "TargetDir";
      this.targetDir.Location = new System.Drawing.Point(12, 27);
      this.targetDir.Name = "targetDir";
      this.targetDir.OpenButtonText = "Browse  Directory ...";
      this.targetDir.OpenButtonWidth = 226;
      this.targetDir.PreCondition = null;
      this.targetDir.Size = new System.Drawing.Size(1172, 23);
      this.targetDir.TabIndex = 9;
      // 
      // TCGATreeBuilderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1196, 162);
      this.Controls.Add(this.targetDir);
      this.MaximumSize = new System.Drawing.Size(10000, 200);
      this.MinimumSize = new System.Drawing.Size(800, 200);
      this.Name = "TCGATreeBuilderUI";
      this.TabText = "";
      this.Text = "TCGATreeBuilderUI";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.targetDir, 0);
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.DirectoryField targetDir;

  }
}