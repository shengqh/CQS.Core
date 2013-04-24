namespace CQS.FileTemplate
{
  partial class HeaderDefinitionBuilderUI
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
      this.dlgOpenFormatFile = new System.Windows.Forms.OpenFileDialog();
      this.dlgSaveFormatFile = new System.Windows.Forms.SaveFileDialog();
      this.btnClose = new System.Windows.Forms.Button();
      this.btnSave = new System.Windows.Forms.Button();
      this.btnLoad = new System.Windows.Forms.Button();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.txtProperties = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.txtName = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.btnInit = new System.Windows.Forms.Button();
      this.dlgOpenDataFile = new System.Windows.Forms.OpenFileDialog();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // dlgOpenFormatFile
      // 
      this.dlgOpenFormatFile.DefaultExt = "header";
      this.dlgOpenFormatFile.Filter = "Header definition file|*.header|All files|*.*";
      this.dlgOpenFormatFile.Title = "Open header definition file";
      // 
      // dlgSaveFormatFile
      // 
      this.dlgSaveFormatFile.DefaultExt = "header";
      this.dlgSaveFormatFile.Filter = "Header definition file|*.header|All files|*.*";
      this.dlgSaveFormatFile.Title = "Save header definition file";
      // 
      // btnClose
      // 
      this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
      this.btnClose.Location = new System.Drawing.Point(459, 0);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(100, 30);
      this.btnClose.TabIndex = 0;
      this.btnClose.Text = "&Close";
      this.btnClose.UseVisualStyleBackColor = true;
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // btnSave
      // 
      this.btnSave.Dock = System.Windows.Forms.DockStyle.Right;
      this.btnSave.Location = new System.Drawing.Point(359, 0);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(100, 30);
      this.btnSave.TabIndex = 0;
      this.btnSave.Text = "&Save definition...";
      this.btnSave.UseVisualStyleBackColor = true;
      this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // btnLoad
      // 
      this.btnLoad.Dock = System.Windows.Forms.DockStyle.Right;
      this.btnLoad.Location = new System.Drawing.Point(259, 0);
      this.btnLoad.Name = "btnLoad";
      this.btnLoad.Size = new System.Drawing.Size(100, 30);
      this.btnLoad.TabIndex = 0;
      this.btnLoad.Text = "&Load definition...";
      this.btnLoad.UseVisualStyleBackColor = true;
      this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.txtProperties);
      this.splitContainer1.Panel1.Controls.Add(this.label2);
      this.splitContainer1.Panel1.Controls.Add(this.txtName);
      this.splitContainer1.Panel1.Controls.Add(this.label1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.AutoScroll = true;
      this.splitContainer1.Panel2.Controls.Add(this.btnInit);
      this.splitContainer1.Panel2.Controls.Add(this.btnLoad);
      this.splitContainer1.Panel2.Controls.Add(this.btnSave);
      this.splitContainer1.Panel2.Controls.Add(this.btnClose);
      this.splitContainer1.Size = new System.Drawing.Size(559, 455);
      this.splitContainer1.SplitterDistance = 421;
      this.splitContainer1.TabIndex = 0;
      // 
      // txtProperties
      // 
      this.txtProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtProperties.Location = new System.Drawing.Point(10, 70);
      this.txtProperties.Multiline = true;
      this.txtProperties.Name = "txtProperties";
      this.txtProperties.Size = new System.Drawing.Size(546, 348);
      this.txtProperties.TabIndex = 5;
      this.txtProperties.TextChanged += new System.EventHandler(this.txtName_TextChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(10, 54);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(147, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Headers (one header per line)";
      // 
      // txtName
      // 
      this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtName.Location = new System.Drawing.Point(10, 31);
      this.txtName.Name = "txtName";
      this.txtName.Size = new System.Drawing.Size(546, 20);
      this.txtName.TabIndex = 3;
      this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(83, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Definition name:";
      // 
      // btnInit
      // 
      this.btnInit.Dock = System.Windows.Forms.DockStyle.Right;
      this.btnInit.Location = new System.Drawing.Point(159, 0);
      this.btnInit.Name = "btnInit";
      this.btnInit.Size = new System.Drawing.Size(100, 30);
      this.btnInit.TabIndex = 1;
      this.btnInit.Text = "&Init from data...";
      this.btnInit.UseVisualStyleBackColor = true;
      this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
      // 
      // dlgOpenDataFile
      // 
      this.dlgOpenDataFile.Title = "Open data file ...";
      // 
      // HeaderDefinitionBuilderUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(559, 455);
      this.Controls.Add(this.splitContainer1);
      this.Name = "HeaderDefinitionBuilderUI";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Breast Cancer Sample Info Definition Builder";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel1.PerformLayout();
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog dlgOpenFormatFile;
    private System.Windows.Forms.SaveFileDialog dlgSaveFormatFile;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnLoad;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.TextBox txtProperties;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnInit;
    private System.Windows.Forms.OpenFileDialog dlgOpenDataFile;
  }
}