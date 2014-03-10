namespace CQS.Sample
{
  partial class SampleInfoColumnDefinitionBuilderUI
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
      this.label1 = new System.Windows.Forms.Label();
      this.txtColumns = new System.Windows.Forms.TextBox();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.dlgSave = new System.Windows.Forms.SaveFileDialog();
      this.SuspendLayout();
      // 
      // btnClose
      // 
      this.btnClose.Location = new System.Drawing.Point(279, 8);
      // 
      // btnCancel
      // 
      this.btnCancel.Enabled = true;
      this.btnCancel.Location = new System.Drawing.Point(194, 8);
      this.btnCancel.Text = "&Save";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // btnGo
      // 
      this.btnGo.Location = new System.Drawing.Point(109, 8);
      this.btnGo.Text = "&Load";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Dock = System.Windows.Forms.DockStyle.Top;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(303, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Input column names you want, one name per line : ";
      // 
      // txtColumns
      // 
      this.txtColumns.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtColumns.Location = new System.Drawing.Point(0, 16);
      this.txtColumns.Multiline = true;
      this.txtColumns.Name = "txtColumns";
      this.txtColumns.Size = new System.Drawing.Size(462, 450);
      this.txtColumns.TabIndex = 1;
      // 
      // dlgOpen
      // 
      this.dlgOpen.DefaultExt = "columns";
      this.dlgOpen.Filter = "Column Definition File|*.columns|All Files|*.*";
      // 
      // dlgSave
      // 
      this.dlgSave.DefaultExt = "columns";
      this.dlgSave.Filter = "Column Definition File|*.columns|All Files|*.*";
      // 
      // SampleInfoColumnDefinitionBuilderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(462, 505);
      this.Controls.Add(this.txtColumns);
      this.Controls.Add(this.label1);
      this.Name = "SampleInfoColumnDefinitionBuilderUI";
      this.Text = "SampleInfoColumnDefinitionBuilderUI";
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.txtColumns, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txtColumns;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.SaveFileDialog dlgSave;
  }
}