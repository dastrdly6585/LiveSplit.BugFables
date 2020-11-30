using LiveSplit.ComponentUtil;
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

    private const string MainManagerName = "Main Camera";

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

    // Version agnostics essentials
    private int baseAddrMainManagerPath = 0;
    private List<int> offsetPathPrefixMainManagerStatic = new List<int>();
    private int numFlags = -1;

    // General purpose offsets
    const int offsetArrayLength = 0x18;
    const int offsetArrayFirstElement = 0x20;

    // MainManager static offsets
    const int offsetMainManagerInstance = 0x10;
    const int offsetMainManagerMap = 0x20;
    const int offsetMainManagerBattle = 0x40;
    const int offsetMainManagerLastEvent = 0x3b0;

    // MainManager offsets
    const int offsetMainMangerMusicIdArray = 0x160;
    const int offsetMainManagerFlagsArray = 0x160;
    const int offsetMainManagerMusicCoroutine = 0x58;
    const int offsetMainManagerInBattle = 0x259;

    // BattleControl offsets
    const int offsetBattleControlEnemyData = 0x18;

    // Unity specific offsets
    const int offsetUnityCachedPtr = 0x10;
    readonly List<int> offsetPathUnityGameObjectName = new List<int> { offsetUnityCachedPtr, 0x30, 0x60, 0x0 };

    private Process BfGameProcess = null;

    private DeepPointer DPMainManagerMusicCoroutine = null;
    private DeepPointer DPMainManagerCurrentRoomName = null;
    private DeepPointer DPMainManagerLastEvent = null;
    private DeepPointer DPMainManagerInBattle = null;
    private DeepPointer DPMainManagerBattleEnemyDataLength = null;
    private DeepPointer DPMainManagerBattleName = null;
    private DeepPointer DPMainManagerFlags = null;
    private DeepPointer DPMainManagerFirstMusicId = null;

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

    public byte[] ReadFlags()
    {
      byte[] flags = new byte[750];
      DPMainManagerFlags.DerefBytes(BfGameProcess, numFlags, out flags);
      return flags;
    }

    public int ReadFirstMusicId()
    {
      int songId = -1;
      DPMainManagerFirstMusicId.Deref<int>(BfGameProcess, out songId);
      return songId;
    }

    public bool ReadMusicCoroutineInProgress()
    {
      long musicCoroutine = 0;
      DPMainManagerMusicCoroutine.Deref<long>(BfGameProcess, out musicCoroutine);
      return musicCoroutine != 0;
    }

    public int ReadCurrentRoomId()
    {
      StringBuilder sb = new StringBuilder();
      if (DPMainManagerCurrentRoomName.DerefString(BfGameProcess, ReadStringType.ASCII, sb))
      {
        string roomName = sb.ToString();
        int roomId = -1;
        if (int.TryParse(roomName, out roomId))
          return roomId;
        else
          return -1;
      }
      else
      {
        return -1;
      }
    }

    public int ReadLastEventId()
    {
        int lastEventId = -1;
        DPMainManagerLastEvent.Deref<int>(BfGameProcess, out lastEventId);
        return lastEventId;
    }

    public bool ReadBattleInProgress()
    {
        bool inBattle = false;
        DPMainManagerInBattle.Deref<bool>(BfGameProcess, out inBattle);
        return inBattle;
    }

    public int ReadBattleEnemyDataLength()
    {
        int enemyDataLength = -1;
        DPMainManagerBattleEnemyDataLength.Deref<int>(BfGameProcess, out enemyDataLength);
        return enemyDataLength;
    }

    public bool ReadBattleHasMainManagerName()
    {
        StringBuilder sb = new StringBuilder();
        if (DPMainManagerBattleName.DerefString(BfGameProcess, ReadStringType.ASCII, sb))
        {
            string battleName = sb.ToString();
            return (battleName == MainManagerName);
        }
        else
        {
            return false;
        }
    }

    private void InitDeepPointers()
    {
      DPMainManagerMusicCoroutine = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerMusicCoroutine }));

      DPMainManagerCurrentRoomName = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerMap }, offsetPathUnityGameObjectName));

      DPMainManagerLastEvent = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerLastEvent }));

      DPMainManagerInBattle = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerInstance, offsetMainManagerInBattle }));

      DPMainManagerBattleEnemyDataLength = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerBattle, offsetBattleControlEnemyData, offsetArrayLength }));

      DPMainManagerFlags = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerInstance, offsetMainManagerFlagsArray,
                                                   offsetArrayFirstElement }));
      DPMainManagerFirstMusicId = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainMangerMusicIdArray, offsetArrayFirstElement }));

      DPMainManagerBattleName = new DeepPointer(UnityPlayerModuleName, baseAddrMainManagerPath,
        GetFullOffsetPathFromParts(new List<int> { offsetMainManagerBattle }, offsetPathUnityGameObjectName));
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
      DPMainManagerLastEvent = null;
      DPMainManagerBattleEnemyDataLength = null;
      DPMainManagerBattleName = null;
      DPMainManagerFlags = null;
      DPMainManagerFirstMusicId = null;
      numFlags = -1;
    }

    private BfVersion DetermineCurrentGameVersion()
    {
      return BfVersion.v110;
    }
  }
}
