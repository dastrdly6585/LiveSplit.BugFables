using System;
using System.Windows.Forms;

namespace LiveSplit.BugFables.UI
{
  public partial class SplitSetting : UserControl
  {
    public bool SplitEnabled
    {
      get
      {
        return chkSplitEnabled.Checked;
      }
      set
      {
        chkSplitEnabled.Checked = value;
      }
    }
    
    public string SplitGroup { get; set; }
    public string SplitName { get; set; }

    private SettingsUserControl parent;

    public SplitSetting(string splitName, bool splitEnabled, string group, SettingsUserControl parent)
    {
      InitializeComponent();
      this.parent = parent;
      this.SplitName = splitName;
      chkSplitEnabled.Text = splitName;
      chkSplitEnabled.Checked = splitEnabled;
      SplitGroup = group;
    }

    private void chkSplitEnabled_CheckedChanged(object sender, EventArgs e)
    {
      parent.settingsHasChanged = true;
    }
  }
}
