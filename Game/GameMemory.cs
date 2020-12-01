﻿using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LiveSplit.BugFables
{
  public class GameMemory
  {
    private const string UnityPlayerModuleName = "UnityPlayer.dll";
    private const string ProcessName = "Bug Fables";

    private enum BfVersion
    {
      UNASSIGNED,
      v110
    }

    private BfVersion currentBfVersion = BfVersion.UNASSIGNED;

    // Version specific information
    const int baseAddrMainManagerStaticPath110 = 0x014EC558;
    readonly List<int> offsetPathPrefixMainManagerStatic110 = new List<int> { 0x58, 0x98, 0x0, 0x18 };
    const int numFlags110 = 750;

    // Version agnostic essentials
    private int baseAddrMainManagerPath = 0;
    private List<int> offsetPathPrefixMainManagerStatic = new List<int>();
    private int numFlags = -1;

    public const int nbrBytesEnemyEncounter = 256 * 2 * sizeof(int);

    // General purpose offsets
    const int offsetArrayFirstElement = 0x20;

    // MainManager static offsets
    const int offsetMainManagerInstance = 0x10;
    const int offsetMainManagerMap = 0x20;
    const int offsetMainManagerBattle = 0x40;

    // MainManager offsets
    const int offsetMainManagerMusicIdArray = 0x160;
    const int offsetMainManagerFlagsArray = 0x160;
    const int offsetMainManagerMusicCoroutine = 0x58;
    const int offsetMainManagerEnemyEncounter = 0x190;

    // Unity specific offsets
    const int offsetUnityCachedPtr = 0x10;
    readonly List<int> offsetPathUnityGameObjectName = new List<int> { offsetUnityCachedPtr, 0x30, 0x60, 0x0 };

    private Process BfGameProcess = null;

    private DeepPointer DPMainManagerMusicCoroutine = null;
    private DeepPointer DPMainManagerCurrentRoomName = null;
    private DeepPointer DPMainManagerFlags = null;
    private DeepPointer DPMainManagerFirstMusicId = null;
    private DeepPointer DPMainManagerBattle = null;
    private DeepPointer DPMainManagerBattlePtr = null;
    private DeepPointer DPMainManagerEnemyEncounter = null;

    public GameMemory()
    {
      ProcessHook();
    }

    public bool ProcessHook()
    {
      Process proc = Process.GetProcessesByName(ProcessName).FirstOrDefault();

      // Already hooked
      if (BfGameProcess != null && proc != null)
        return true;
      // Already unhooked
      if (BfGameProcess == null && proc == null)
        return false;

      // New hook
      if (BfGameProcess == null && proc != null)
      {
        currentBfVersion = DetermineCurrentGameVersion();
        InitVersionSpecificVariables();
        InitDeepPointers();
        BfGameProcess = proc;
        return true;
      }

      // New Unhook
      if (BfGameProcess != null && proc == null)
        ResetEverything();

      return false;
    }

    public bool ReadEnemyEncounter(out byte[] enemyEncounter)
    {
      enemyEncounter = new byte[nbrBytesEnemyEncounter];
      if (!DPMainManagerEnemyEncounter.DerefBytes(BfGameProcess, nbrBytesEnemyEncounter, out enemyEncounter))
      {
        enemyEncounter = null;
        return false;
      }
      return true;
    }
    public int GetNbrDefeatedForEnemyId(byte[] enemyEncounter, GameEnums.Enemy enemy)
    {
      return BitConverter.ToInt32(enemyEncounter, (int)enemy * sizeof(int) * 2 + 4);
    }

    public bool ReadFlags(out byte[] flags)
    {
      flags = new byte[numFlags];
      if (!DPMainManagerFlags.DerefBytes(BfGameProcess, numFlags, out flags))
      {
        flags = null;
        return false;
      }
      return true;
    }

    public bool ReadFirstMusicId(out int songId)
    {
      if (!DPMainManagerFirstMusicId.Deref<int>(BfGameProcess, out songId))
      {
        songId = -1;
        return false;
      }
      return true;
    }

    public bool ReadMusicCoroutineInProgress(out long musicCoroutine)
    {
      if (!DPMainManagerMusicCoroutine.Deref<long>(BfGameProcess, out musicCoroutine))
      {
        musicCoroutine = -1;
        return false;
      }
      return true;
    }

    public bool ReadBattlePtr(out long battlePtr)
    {
      if (!DPMainManagerBattlePtr.Deref<long>(BfGameProcess, out battlePtr))
      {
        if (!DPMainManagerBattle.Deref<long>(BfGameProcess, out battlePtr))
        {
          battlePtr = -1;
          return false;
        }
        battlePtr = 0;
        return true;
      }
      return true;
    }

    public bool ReadCurrentRoomId(out int roomId)
    {
      StringBuilder sb = new StringBuilder();
      if (DPMainManagerCurrentRoomName.DerefString(BfGameProcess, ReadStringType.ASCII, sb))
      {
        string roomName = sb.ToString();
        if (!int.TryParse(roomName, out roomId))
        {
          roomId = -1;
          return false;
        }
        return true;
      }

      roomId = -1;
      return false;
    }

    private void InitDeepPointers()
    {
      DPMainManagerMusicCoroutine = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerMusicCoroutine }));

      DPMainManagerCurrentRoomName = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerMap }, offsetPathUnityGameObjectName));

      DPMainManagerFlags = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerInstance, offsetMainManagerFlagsArray,
                                                   offsetArrayFirstElement }));

      DPMainManagerFirstMusicId = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerMusicIdArray, offsetArrayFirstElement }));

      DPMainManagerBattlePtr = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
      GetFullOffsetPathFromParts(new List<int> { offsetMainManagerBattle, offsetUnityCachedPtr }));

      DPMainManagerBattle = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
      GetFullOffsetPathFromParts(new List<int> { offsetMainManagerBattle }));

      DPMainManagerEnemyEncounter = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
      GetFullOffsetPathFromParts(new List<int> { offsetMainManagerInstance, offsetMainManagerEnemyEncounter, offsetArrayFirstElement }));
    }

    private int[] GetFullOffsetPathFromParts(List<int> mainParts, List<int> unitySuffixPath = null, int unityArrayOffset = -1)
    {
      List<int> pathList = new List<int>();
      pathList.AddRange(offsetPathPrefixMainManagerStatic);
      pathList.AddRange(mainParts);
      if (unitySuffixPath != null)
      {
        pathList.AddRange(unitySuffixPath);
        if (unityArrayOffset != -1)
          pathList.Add(unityArrayOffset);
      }
      return pathList.ToArray();
    }

    private void InitVersionSpecificVariables()
    {
      switch (currentBfVersion)
      {
        case BfVersion.v110:
          baseAddrMainManagerPath = baseAddrMainManagerStaticPath110;
          offsetPathPrefixMainManagerStatic.AddRange(offsetPathPrefixMainManagerStatic110);
          numFlags = numFlags110;
          break;
        case BfVersion.UNASSIGNED:
          Console.WriteLine("Got an unassigned version!");
          break;
      }
    }

    private void ResetEverything()
    {
      BfGameProcess = null;
      currentBfVersion = BfVersion.UNASSIGNED;
      baseAddrMainManagerPath = 0;
      offsetPathPrefixMainManagerStatic.Clear();

      DPMainManagerMusicCoroutine = null;
      DPMainManagerCurrentRoomName = null;
      DPMainManagerFlags = null;
      DPMainManagerFirstMusicId = null;
      DPMainManagerBattle = null;
      DPMainManagerBattlePtr = null;
      DPMainManagerEnemyEncounter = null;
      numFlags = -1;
    }

    private BfVersion DetermineCurrentGameVersion()
    {
      return BfVersion.v110;
    }
  }
}
