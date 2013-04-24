namespace CQS.TCGA.Microarray
{
  partial class MicroarrayDataSummaryBuilderUI
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
      this.btnDatabase = new System.Windows.Forms.Button();
      this.txtDatabase = new System.Windows.Forms.TextBox();
      this.pnlFile.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlFile
      // 
      this.pnlFile.Location = new System.Drawing.Point(24, 12);
      this.pnlFile.Size = new System.Drawing.Size(1097, 22);
      // 
      // txtOriginalFile
      // 
      this.txtOriginalFile.Location = new System.Drawing.Point(341, 0);
      this.txtOriginalFile.Size = new System.Drawing.Size(756, 21);
      // 
      // btnOriginalFile
      // 
      this.btnOriginalFile.Size = new System.Drawing.Size(341, 22);
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 118);
      this.lblProgress.Size = new System.Drawing.Size(1147, 21);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 139);
      this.progressBar.Size = new System.Drawing.Size(1147, 21);
      // 
      // btnClose
      // 
      this.btnClose.Location = new System.Drawing.Point(621, 7);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(536, 7);
      // 
      // btnGo
      // 
      this.btnGo.Location = new System.Drawing.Point(451, 7);
      // 
      // btnDatabase
      // 
      this.btnDatabase.Location = new System.Drawing.Point(24, 51);
      this.btnDatabase.Name = "btnDatabase";
      this.btnDatabase.Size = new System.Drawing.Size(341, 23);
      this.btnDatabase.TabIndex = 9;
      this.btnDatabase.Text = "button1";
      this.btnDatabase.UseVisualStyleBackColor = true;
      // 
      // txtDatabase
      // 
      this.txtDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtDatabase.Location = new System.Drawing.Point(365, 53);
      this.txtDatabase.Name = "txtDatabase";
      this.txtDatabase.Size = new System.Drawing.Size(756, 21);
      this.txtDatabase.TabIndex = 10;
      // 
      // CombineQuantificationResultProcessorUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1147, 196);
      this.Controls.Add(this.txtDatabase);
      this.Controls.Add(this.btnDatabase);
      this.MaximumSize = new System.Drawing.Size(10000, 234);
      this.MinimumSize = new System.Drawing.Size(800, 234);
      this.Name = "CombineQuantificationResultProcessorUI";
      this.TabText = "DatabaseSnpValidatorUI";
      this.Text = "DatabaseSnpValidatorUI";
      this.Controls.SetChildIndex(this.pnlFile, 0);
      this.Controls.SetChildIndex(this.btnDatabase, 0);
      this.Controls.SetChildIndex(this.txtDatabase, 0);
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.pnlFile.ResumeLayout(false);
      this.pnlFile.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnDatabase;
    private System.Windows.Forms.TextBox txtDatabase;
  }
}