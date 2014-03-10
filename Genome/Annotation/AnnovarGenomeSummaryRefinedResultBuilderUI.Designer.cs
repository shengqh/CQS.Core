namespace CQS.Genome.Annotation
{
  partial class AnnovarGenomeSummaryRefinedResultBuilderUI
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
      this.affyFile = new RCPA.Gui.FileField();
      this.annFile = new RCPA.Gui.FileField();
      this.SuspendLayout();
      // 
      // lblProgress
      // 
      this.lblProgress.Location = new System.Drawing.Point(0, 92);
      this.lblProgress.Size = new System.Drawing.Size(977, 23);
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 115);
      this.progressBar.Size = new System.Drawing.Size(977, 23);
      // 
      // btnClose
      // 
      this.btnClose.Location = new System.Drawing.Point(536, 8);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(451, 8);
      // 
      // btnGo
      // 
      this.btnGo.Location = new System.Drawing.Point(366, 8);
      // 
      // affyFile
      // 
      this.affyFile.FullName = "";
      this.affyFile.Key = "AffyFile";
      this.affyFile.Location = new System.Drawing.Point(12, 54);
      this.affyFile.Name = "affyFile";
      this.affyFile.OpenButtonText = "Browse All File ...";
      this.affyFile.WidthOpenButton = 266;
      this.affyFile.PreCondition = null;
      this.affyFile.Size = new System.Drawing.Size(954, 23);
      this.affyFile.TabIndex = 10;
      // 
      // annFile
      // 
      this.annFile.FullName = "";
      this.annFile.Key = "AnnFile";
      this.annFile.Location = new System.Drawing.Point(12, 25);
      this.annFile.Name = "annFile";
      this.annFile.OpenButtonText = "Browse All File ...";
      this.annFile.WidthOpenButton = 266;
      this.annFile.PreCondition = null;
      this.annFile.Size = new System.Drawing.Size(954, 23);
      this.annFile.TabIndex = 11;
      // 
      // AnnovarGenomeSummaryRefinedResultBuilderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(977, 177);
      this.Controls.Add(this.annFile);
      this.Controls.Add(this.affyFile);
      this.Name = "AnnovarGenomeSummaryRefinedResultBuilderUI";
      this.TabText = "MiRNAToDNAConverterUI";
      this.Text = "MiRNAToDNAConverterUI";
      this.Controls.SetChildIndex(this.progressBar, 0);
      this.Controls.SetChildIndex(this.lblProgress, 0);
      this.Controls.SetChildIndex(this.affyFile, 0);
      this.Controls.SetChildIndex(this.annFile, 0);
      this.ResumeLayout(false);

    }

    #endregion

    private RCPA.Gui.FileField affyFile;
    private RCPA.Gui.FileField annFile;
  }
}