using System;

namespace LiveSplit.BugFables
{
  public class Split
  {
    public GameEnums.Room requiredRoom = GameEnums.Room.UNASSUGNED;
    public GameEnums.Event requiredBattleEvent = GameEnums.Event.UNASSIGNED;
    public int requiredNbrBattleInEvent = -1;
    public GameEnums.Flag[] requiredFlags;

    public string group = "";
    public string name = "";
    public bool isEnbaled = true;
  }
}
