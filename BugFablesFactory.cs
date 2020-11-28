using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;

namespace LiveSplit.BugFables
{
  class BugFablesFactory : IComponentFactory
  {
    public const string AutosplitterName = "Bug Fables: The Everlasting Sapling Autosplitter";
    public string ComponentName => AutosplitterName + " v" + Version.ToString(3);
    public string Description => ComponentName;
    public ComponentCategory Category => ComponentCategory.Control;
    public string UpdateName => ComponentName;
    public string XMLURL => UpdateURL + "Components/Updates.xml";
    public string UpdateURL => "https://raw.githubusercontent.com/aldelaro5/LiveSplit.BugFables/main/";
    public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
    public IComponent Create(LiveSplitState state) => new BugFablesComponent(state);
  }
}
