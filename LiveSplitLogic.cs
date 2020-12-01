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
    private byte[] oldEnemyEncounter = null;

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
      byte[] enemyEncounter;
      long battlePtr;
      try
      {
        if (!gameMemory.ReadCurrentRoomId(out currentRoomId))
          return false;
        if (!gameMemory.ReadFlags(out flags))
          return false;
        if (!gameMemory.ReadEnemyEncounter(out enemyEncounter))
          return false;
        if (!gameMemory.ReadBattlePtr(out battlePtr))
          return false;
      }
      catch (Exception)
      {
        return false;
      }

      if (oldEnemyEncounter == null)
      {
        oldEnemyEncounter = new byte[GameMemory.nbrBytesEnemyEncounter];
        enemyEncounter.CopyTo(oldEnemyEncounter, 0);
      }

      Split split = splits[currentSplitIndex];

      if (!MidSplitRoomCheck(split, currentRoomId))
        return false;
      if (!MidSplitFlagsCheck(split, flags))
        return false;
      if (!MidSplitEnemyDefeatedCheck(split, enemyEncounter, battlePtr))
        return false;

      enemyEncounter.CopyTo(oldEnemyEncounter, 0);

      return true;
    }

    private bool MidSplitRoomCheck(Split split, int currentRoomId)
    {
      return (split.requiredRoom == GameEnums.Room.UNASSIGNED ||
              currentRoomId == (int)split.requiredRoom);
    }

    private bool MidSplitFlagsCheck(Split split, byte[] flags)
    {
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

          return allFlagsTrue;
        }
      }

      return true;
    }

    private bool MidSplitEnemyDefeatedCheck(Split split, byte[] enemyEncounter, long battlePtr)
    {
      if (split.requiredEnemiesDefeated != null)
      {
        if (split.requiredEnemiesDefeated.Length != 0)
        {
          if (battlePtr != 0)
            return false;

          bool allEnemiesDefeated = true;
          foreach (var requiredEnemy in split.requiredEnemiesDefeated)
          {
            int newNbrDefeated = gameMemory.GetNbrDefeatedForEnemyId(enemyEncounter, requiredEnemy);
            int oldNbrDefeated = gameMemory.GetNbrDefeatedForEnemyId(oldEnemyEncounter, requiredEnemy);
            if (newNbrDefeated <= oldNbrDefeated)
            {
              allEnemiesDefeated = false;
              break;
            }
          }

          return allEnemiesDefeated;
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
      oldEnemyEncounter = null;
      currentEndTimeState = EndTimeState.NotArrivedYet;

      InitSplits();
    }
  }
}
