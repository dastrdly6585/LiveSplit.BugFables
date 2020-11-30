using LiveSplit.UI;
using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.BugFables.UI
{
  public partial class SettingsUserControl : UserControl
  {
    public enum AutoSplitterMode
    {
      StartEndOnly,
      Glitchless
    }

    public AutoSplitterMode Mode { get; set; }
    public bool settingsHasChanged { get; set; }

    private Split[] glitchlessAllSplits;

    private LiveSplitState liveSplitState;

    public SettingsUserControl(LiveSplitState state)
    {
      InitializeComponent();
      lblVersion.Text = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
      settingsHasChanged = false;
      liveSplitState = state;
      InitSplits();
      rdbStartEndOnly.Checked = true;
      grpSplits.Visible = false;
      btnSetCurrentSplits.Visible = false;
      string currentGroup = "";
      foreach (var split in glitchlessAllSplits)
      {
        flowSplits.SuspendLayout();

        if (currentGroup == "" || split.group != currentGroup)
        {
          Label lbl = new Label();
          lbl.Text = split.group;
          lbl.Width = 400;
          lbl.Height = 15;
          flowSplits.Controls.Add(lbl);
        }
        flowSplits.Controls.Add(new SplitSetting(split.name, split.isEnabled, split.group, this));

        flowSplits.ResumeLayout(true);

        currentGroup = split.group;
      }
    }
    public Split[] GetSplitsGlitchless()
    {
      return glitchlessAllSplits.Where(x => x.isEnabled).ToArray();
    }

    public void LoadSettings(XmlNode node)
    {
      Mode = SettingsHelper.ParseEnum(node["Mode"], AutoSplitterMode.StartEndOnly);

      switch (Mode)
      {
        case AutoSplitterMode.StartEndOnly:
          rdbStartEndOnly.Checked = true;
          grpSplits.Visible = false;
          btnSetCurrentSplits.Visible = false;
          break;
        case AutoSplitterMode.Glitchless:
          rdbGlitchless.Checked = true;
          grpSplits.Visible = true;
          btnSetCurrentSplits.Visible = true;
          break;
      }

      XmlReadSplits(node.SelectNodes(".//Splits/Split"));
    }

    private void XmlReadSplits(XmlNodeList splitNodes)
    {
      if (splitNodes == null) 
        return;

      SplitSetting[] splitSettings = GetSplitsSettings();

      for (int i = 0; i < splitNodes.Count; i++)
      {
        bool isSplitEnabled = SettingsHelper.ParseBool(splitNodes[i]["IsEnabled"]);
        splitSettings[i].SplitEnabled = isSplitEnabled;
        glitchlessAllSplits[i].isEnabled = isSplitEnabled;
      }
    }

    public XmlNode SaveSettings(XmlDocument doc)
    {
      XmlElement xmlSettings = doc.CreateElement("Settings");

      SettingsHelper.CreateSetting(doc, xmlSettings, "Version",
          Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
      SettingsHelper.CreateSetting(doc, xmlSettings, "Mode", Mode);

      XmlElement xmlSplits = doc.CreateElement("Splits");
      xmlSettings.AppendChild(xmlSplits);
      SplitSetting[] splitSettings = GetSplitsSettings();

      int currentIndex = 0;
      foreach (var split in splitSettings)
      {
        XmlElement xmlSplit = doc.CreateElement("Split");
        xmlSplits.AppendChild(xmlSplit);

        SettingsHelper.CreateSetting(doc, xmlSplit, "Name", split.SplitName);
        SettingsHelper.CreateSetting(doc, xmlSplit, "Group", split.SplitGroup);
        SettingsHelper.CreateSetting(doc, xmlSplit, "IsEnabled", split.SplitEnabled);

        glitchlessAllSplits[currentIndex].isEnabled = split.SplitEnabled;

        currentIndex++;
      }

      return xmlSettings;
    }

    private SplitSetting[] GetSplitsSettings()
    {
      List<SplitSetting> splitSettings = new List<SplitSetting>();
      foreach (var control in flowSplits.Controls)
      {
        if (control.GetType() == typeof(SplitSetting))
          splitSettings.Add((SplitSetting)control);
      }

      return splitSettings.ToArray();
    }

    private void InitSplits()
    {
      glitchlessAllSplits = new Split[]
      {
        new Split { group = "Chapter 1", name = "Leif Rescue",
                    requiredBattleEvent = GameEnums.Event.ApproachingSpuderWeb, requiredNbrBattleInEvent = 2 },
        new Split { group = "Chapter 1", name = "Leif Rescue (Post Cutscene)",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.RescuedLeif } },
        new Split { group = "Chapter 1", name = "Enter Treasure Room",
                    requiredRoom = GameEnums.Room.SnakemouthTreasureRoom },
        new Split { group = "Chapter 1", name = "Spider",
                    requiredBattleEvent = GameEnums.Event.ApproachingAncientMask, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 1", name = "End of Chapter 1",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.EndedCh1 } },

        new Split { group = "Chapter 2", name = "Enter Golden Settlement",
                    requiredRoom = GameEnums.Room.GoldenSettlementEntrance },
        new Split { group = "Chapter 2", name = "Aria",
                    requiredBattleEvent = GameEnums.Event.StartingGSCeremony, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 2", name = "Ceremony",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.CeremonyEnded } },
        new Split { group = "Chapter 2", name = "Golden Hills after Big Crank",
                    requiredRoom = GameEnums.Room.GoldenHillsDungeonEntrance,
                    requiredFlags = new GameEnums.Flag[2] { GameEnums.Flag.GotBigCrankTopHalf, GameEnums.Flag.GotBigCrankBottomHalf } },
        new Split { group = "Chapter 2", name = "Zasp & Mothiva",
                    requiredBattleEvent = GameEnums.Event.PlacingBigCrank, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 2", name = "Zasp & Mothiva (Post Cutscene)",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.BeatZaspAndMothivaGoldenHills } },
        new Split { group = "Chapter 2", name = "Enter Venus's Garden",
                    requiredRoom = GameEnums.Room.GoldenHillsDungeonBoss },
        new Split { group = "Chapter 2", name = "Venus Guardian",
                    requiredBattleEvent = GameEnums.Event.ApproachingVenus, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 2", name = "End of Chapter 2",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.EndedCh2 } },

        new Split { group = "Chapter 3", name = "Monsieur Scarlet",
                    requiredBattleEvent = GameEnums.Event.TalkingAboutRequestingAssistance, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 3", name = "Requesting Assistance",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.CompletedRequestingAssistance } },
        new Split { group = "Chapter 3", name = "Bandit Ambush Fight",
                    requiredBattleEvent = GameEnums.Event.ApproachingBanditWaspAmbush, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 3", name = "Merchant Rescue",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.RescuedMerchants } },
        new Split { group = "Chapter 3", name = "Enter Honey Factory",
                    requiredRoom = GameEnums.Room.HoneyFactoryEntrance },
        new Split { group = "Chapter 3", name = "Gen & Eri Rescue",
                    requiredBattleEvent = GameEnums.Event.ApproachingGenEriBeeBoops, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 3", name = "Gen & Eri Rescue (Post Cutscene)",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.RescuedGenEri } },
        new Split { group = "Chapter 3", name = "Malbee Rescue",
                    requiredBattleEvent = GameEnums.Event.ApproachingMalbeeAbomihoneys, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 3", name = "Malbee Rescue (Post Cutscene)",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.RescuedMalbee } },
        new Split { group = "Chapter 3", name = "Ahoneynation",
                    requiredBattleEvent = GameEnums.Event.ApproachingAhoneynation, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 3", name = "Zasp & Mothiva Rescue",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.RescuedZaspMothiva } },
        new Split { group = "Chapter 3", name = "Factory Core after Overseer Rescue",
                    requiredRoom = GameEnums.Room.HoneyFactoryCore,
                    requiredFlags = new GameEnums.Flag[1] { GameEnums.Flag.RescuedOverseer } },
        new Split { group = "Chapter 3", name = "Heavy Drone B-33",
                    requiredBattleEvent = GameEnums.Event.TalkingToOverseerInCore, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 3", name = "End of Chapter 3",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.EndedCh3 } },

        new Split { group = "Chapter 4", name = "Enter Bandit Hideout",
                    requiredRoom = GameEnums.Room.HideoutEntrance },
        new Split { group = "Chapter 4", name = "Astotheles",
                    requiredBattleEvent = GameEnums.Event.AncientCastleKeysEvent, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 4", name = "Earth Key",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.GotEarthKey } },
        new Split { group = "Chapter 4", name = "Dune Scorpion",
                    requiredBattleEvent = GameEnums.Event.ApproachingDuneScorpion, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 4", name = "Enter Ancient Castle",
                    requiredRoom = GameEnums.Room.SandCastleEntrance },
        new Split { group = "Chapter 4", name = "Enter Treasure Room",
                    requiredRoom = GameEnums.Room.SandCastleBossRoom },
        new Split { group = "Chapter 4", name = "The Watcher",
                    requiredBattleEvent = GameEnums.Event.ApproachingWatcher, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 4", name = "End of Chapter 4",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.EndedCh4 } },

        new Split { group = "Chapter 5", name = "Enter Far Grasslands",
                    requiredRoom = GameEnums.Room.FarGrasslands1 },
        new Split { group = "Chapter 5", name = "Enter The Beast's Room",
                    requiredRoom = GameEnums.Room.SwamplandsBoss },
        new Split { group = "Chapter 5", name = "The Beast",
                    requiredBattleEvent = GameEnums.Event.ApproachingTheBeast, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 5", name = "The Beast (Post Cutscene)",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.BeatTheBeast } },
        new Split { group = "Chapter 5", name = "General Ultimax",
                    requiredBattleEvent = GameEnums.Event.ApproachingWaspKingdomHiveThroneRoom, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 5", name = "Vanessa II Rescue",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.RescuedVanessaII } },

        new Split { group = "Chapter 6", name = "Enter Forsaken Lands",
                    requiredRoom = GameEnums.Room.BarrenLandsEntrance },
        new Split { group = "Chapter 6", name = "Primal Weevil",
                    requiredBattleEvent = GameEnums.Event.ApproachingPrimalWeevil, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 6", name = "Primal Weevil (Post Cutscene)",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.BeatPrimalWeevil } },
        new Split { group = "Chapter 6", name = "Zasp & Mothiva 2",
                    requiredBattleEvent = GameEnums.Event.EnteringColosseum, requiredNbrBattleInEvent = 3 },
        new Split { group = "Chapter 6", name = "Colosseum",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.BeatColosseum } },
        new Split { group = "Chapter 6", name = "Rubber Prison Giant's Lair Bridge",
                    requiredRoom = GameEnums.Room.RubberPrisonGiantLairBridge },
        new Split { group = "Chapter 6", name = "ULTIMAX Tank",
                    requiredBattleEvent = GameEnums.Event.ApproachingUltimaxTank, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 6", name = "ULTIMAX Tank (Post Cutscene)",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.BeatUltimaxTank } },

        new Split { group = "Chapter 7", name = "Deadlanders Trio",
                    requiredBattleEvent = GameEnums.Event.ApproachingFridgeEntrance, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 7", name = "Deadlanders Trio (Post Cutscene)",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.BeatDeadLandersTrio } },
        new Split { group = "Chapter 7", name = "Exit Fridge",
                    requiredRoom = GameEnums.Room.GiantLairRoachVillage },
        new Split { group = "Chapter 7", name = "Final Boss Room",
                    requiredRoom = GameEnums.Room.GiantLairSaplingPlains },
        new Split { group = "Chapter 7", name = "The Wasp King",
                    requiredBattleEvent = GameEnums.Event.ApproachingWaspKing, requiredNbrBattleInEvent = 1 },
        new Split { group = "Chapter 7", name = "The Everlasting Sapling",
                    requiredFlags = new GameEnums.Flag[] { GameEnums.Flag.PostGame } },
        new Split { group = "Chapter 7", name = "The Everlasting King",
                    requiredBattleEvent = GameEnums.Event.ApproachingWaspKing, requiredNbrBattleInEvent = 2 },
        new Split { group = "Chapter 7", name = "Ant City Plaza",
                    requiredRoom = GameEnums.Room.BugariaEndPlaza }
      };
    }

    private void rdbGlitchless_CheckedChanged(object sender, EventArgs e)
    {
      if (rdbGlitchless.Checked)
      {
        Mode = AutoSplitterMode.Glitchless;
        settingsHasChanged = true;
        grpSplits.Visible = true;
        btnSetCurrentSplits.Visible = true;
      }
    }

    private void rdbStartEndOnly_CheckedChanged(object sender, EventArgs e)
    {
      if (rdbStartEndOnly.Checked)
      {
        Mode = AutoSplitterMode.StartEndOnly;
        settingsHasChanged = true;
        grpSplits.Visible = false;
        btnSetCurrentSplits.Visible = false;
      }
    }

    private void btnSetCurrentSplits_Click(object sender, EventArgs e)
    {
      var answer = MessageBox.Show("This action will clear your current splits which will delete all splits and " +
                      "their associated times before adding new splits corresponding to the enabled ones in this window. " +
                      "\n\nAre you sure you want to proceed?", "Clearing current splits", 
                      MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
      if (answer == DialogResult.Yes)
      {
        liveSplitState.Run.Clear();
        SplitSetting[] splitSettings = GetSplitsSettings();
        string currentGroup = "";
        foreach (var split in splitSettings)
        {
          if (!split.SplitEnabled)
            continue;

          if (currentGroup == "")
            currentGroup = split.SplitGroup;

          if (currentGroup != split.SplitGroup)
          {
            ISegment lastSplit = liveSplitState.Run.Last();
            lastSplit.Name = "{" + currentGroup + "}" + lastSplit.Name.Remove(0,1);
            currentGroup = split.SplitGroup;
          }

          liveSplitState.Run.AddSegment("-" + split.SplitName);
        }

        liveSplitState.Run.AddSegment("{" + currentGroup + "}" + "End");

        MessageBox.Show("The operation was performed successfully. You may need to remove and add your splits " +
                        "in your layout for the changes to take effect in LiveSplit.", 
                        "Operation successfull", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }
  }
}
