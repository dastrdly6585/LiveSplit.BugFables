
namespace LiveSplit.BugFables.UI
{
  partial class SplitSetting
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.chkSplitEnabled = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // chkSplitEnabled
      // 
      this.chkSplitEnabled.AutoSize = true;
      this.chkSplitEnabled.Location = new System.Drawing.Point(29, 3);
      this.chkSplitEnabled.Name = "chkSplitEnabled";
      this.chkSplitEnabled.Size = new System.Drawing.Size(113, 24);
      this.chkSplitEnabled.TabIndex = 0;
      this.chkSplitEnabled.Text = "checkBox1";
      this.chkSplitEnabled.UseMnemonic = false;
      this.chkSplitEnabled.UseVisualStyleBackColor = true;
      this.chkSplitEnabled.CheckedChanged += new System.EventHandler(this.chkSplitEnabled_CheckedChanged);
      // 
      // SplitSetting
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.chkSplitEnabled);
      this.Name = "SplitSetting";
      this.Size = new System.Drawing.Size(400, 30);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.CheckBox chkSplitEnabled;
  }
}
