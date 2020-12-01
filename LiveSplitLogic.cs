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
      int currentSong;
      byte[] flags;

      try
      {
        if (!gameMemory.ReadFirstMusicId(out currentSong))
          return false;

        if (!gameMemory.ReadFlags(out flags))
          return false;
      }
      catch (Exception)
      {
        return false;
      }

      bool shouldStart = false;
      bool newListeningToTitleSong = (currentSong == (int)GameEnums.Song.Title);
      if (oldListeningToTitleSong && !newListeningToTitleSong)
      {
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
      if (splits.Length - 1 < currentSplitIndex)
        return false;

      int currentRoomId;
      byte[] flags;
      try
      {
        if (!gameMemory.ReadCurrentRoomId(out currentRoomId))
          return false;

        if (!gameMemory.ReadFlags(out flags))
          return false;
      }
      catch (Exception)
      {
        return false;
      }

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
      int currentSong;
      long musicCoroutine;
      int currentRoomId;

      try
      {
        if (gameMemory.ReadFirstMusicId(out currentSong))
          return false;

        if (gameMemory.ReadMusicCoroutineInProgress(out musicCoroutine))
          return false;

        if (gameMemory.ReadCurrentRoomId(out currentRoomId))
          return false;
      }
      catch (Exception)
      {
        return false;
      }

      if (currentEndTimeState == EndTimeState.NotArrivedYet)
      {
        if (currentRoomId == (int)GameEnums.Room.BugariaEndThrone)
          currentEndTimeState = EndTimeState.ArrivedInRoom;
      }
      else
      {

        bool newMusicCouroutineInProgress = (musicCoroutine != 0);

        if (currentEndTimeState == EndTimeState.ArrivedInRoom && currentSong == (int)GameEnums.Song.LevelUp)
        {
          currentEndTimeState = EndTimeState.SongLevelUpStarting;
        }
        else if (currentEndTimeState == EndTimeState.SongLevelUpStarting && !newMusicCouroutineInProgress)
        {
          currentEndTimeState = EndTimeState.SongLevelIsPlaying;
        }
        else if (currentEndTimeState == EndTimeState.SongLevelIsPlaying && newMusicCouroutineInProgress)
        {
          currentEndTimeState = EndTimeState.SongIsFading;
        }
        else if (currentEndTimeState == EndTimeState.SongIsFading && !newMusicCouroutineInProgress)
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
