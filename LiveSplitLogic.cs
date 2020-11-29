using LiveSplit.BugFables.UI;
using System;

namespace LiveSplit.BugFables
{
  public class LiveSplitLogic
  {
		private enum EndTimeState
    {
      NotArrivedYet,
      ArrivedInRoom,
      SongLevelUpStarting,
      SongLevelIsPlaying,
      SongIsFading
    }

		private EndTimeState currentEndTimeState = EndTimeState.NotArrivedYet;

		private GameMemory gameMemory = new GameMemory();

    private bool oldListeningToTitleSong = false;

    private Split[] splits;
    private SettingsUserControl settings;

    public LiveSplitLogic(GameMemory gameMemory, SettingsUserControl settings)
    {
      this.gameMemory = gameMemory;
      this.settings = settings;
      InitSplits();
    }

    private void InitSplits()
    {
      splits = settings.GetSplitsGlitchless();
    }

    public bool ShouldStart()
    {
      bool shouldStart = false;
      bool newListeningToTitleSong = gameMemory.ReadFirstMusicId() == (int)GameEnums.Song.Title;
      
      if (oldListeningToTitleSong && !newListeningToTitleSong)
      {
        byte[] flags = gameMemory.ReadFlags();
        shouldStart = BitConverter.ToBoolean(flags, (int)GameEnums.Flag.NewGameStarted);
      }

      oldListeningToTitleSong = newListeningToTitleSong;
      return shouldStart;
    }

    public bool ShouldSplit(int currentSplitIndex, int currentRunSplitsCount)
    {
      if (settings.Mode != SettingsUserControl.AutoSplitterMode.StartEndOnly && 
          currentSplitIndex != currentRunSplitsCount - 1)
      {
        return ShouldMidSplit(currentSplitIndex);
      }
      else
      {
        return ShouldEnd();
      }
    }

    private bool ShouldMidSplit(int currentSplitIndex)
    {
      int currentRoomId = gameMemory.ReadCurrentRoomId();
      byte[] flags = gameMemory.ReadFlags();

      if (splits.Length - 1 < currentSplitIndex)
        return false;

      Split split = splits[currentSplitIndex];
      
      if (split.requiredRoom != GameEnums.Room.UNASSUGNED &&
          currentRoomId != (int)split.requiredRoom)
        return false;

      if (split.requiredFlags != null)
      {
        if (split.requiredFlags.Length != 0)
        {
          bool allFlagsTrue = true;
          foreach (var requiredFlag in split.requiredFlags)
          {
            if (!BitConverter.ToBoolean(flags, (int)requiredFlag))
            {
              allFlagsTrue = false;
              break;
            }
          }

          if (!allFlagsTrue)
            return false;
        }
      }
      return true;
    }

    private bool ShouldEnd()
    {
      if (currentEndTimeState == EndTimeState.NotArrivedYet)
      {
        if (gameMemory.ReadCurrentRoomId() == (int)GameEnums.Room.BugariaEndThrone)
          currentEndTimeState = EndTimeState.ArrivedInRoom;
      }
      else
      {
        bool newMusicCouroutine = gameMemory.ReadMusicCoroutineInProgress();
        int currentSong = gameMemory.ReadFirstMusicId();

        if (currentEndTimeState == EndTimeState.ArrivedInRoom && currentSong == (int)GameEnums.Song.LevelUp)
        {
          currentEndTimeState = EndTimeState.SongLevelUpStarting;
        }
        else if (currentEndTimeState == EndTimeState.SongLevelUpStarting && !newMusicCouroutine)
        {
          currentEndTimeState = EndTimeState.SongLevelIsPlaying;
        }
        else if (currentEndTimeState == EndTimeState.SongLevelIsPlaying && newMusicCouroutine)
        {
          currentEndTimeState = EndTimeState.SongIsFading;
        }
        else if (currentEndTimeState == EndTimeState.SongIsFading && !newMusicCouroutine)
        {
          currentEndTimeState = EndTimeState.NotArrivedYet;
          return true;
        }
      }

      return false;
    }

		public void ResetLogic()
		{
      oldListeningToTitleSong = false;

      InitSplits();
    }
  }
}
