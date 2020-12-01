namespace LiveSplit.BugFables
{
  public class Split
  {
    public GameEnums.Room requiredRoom = GameEnums.Room.UNASSIGNED;
    public GameEnums.Flag[] requiredFlags;
    public GameEnums.Enemy[] requiredEnemiesDefeated;

    public string group = "";
    public string name = "";
    public bool isEnabled = true;
  }
}
