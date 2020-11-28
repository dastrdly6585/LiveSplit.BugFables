﻿using System;

namespace LiveSplit.BugFables.GameEnums
{
	public enum Room
	{
		UNASSUGNED = -1,
		TestRoom = 0,
		SnakemouthTreasureRoom = 21,
		GoldenSettlementEntrance = 39,
		GoldenHillsDungeonEntrance = 45,
		GoldenHillsDungeonBoss = 53,
		HoneyFactoryEntrance = 72,
		HoneyFactoryCore = 75,
		HideoutEntrance = 99,
		SandCastleBossRoom = 128,
		FarGrasslands1 = 137,
		BarrenLandsEntrance = 149,
		SwamplandsBoss = 154,
		RubberPrisonGiantLairBridge = 231,
		GiantLairRoachVillage = 237,
		GiantLairSaplingPlains = 238,
		BugariaEndThrone = 242
	}

	public enum Event
	{
		UNASSIGNED = -1,
		ApproachingSpuderWeb = 6,
		ApproachingAncientMask = 26,
		StartingGSCeremony = 58,
		PlacingBigCrank = 67,
		ApproachingVenus = 73,
		TalkingAboutRequestingAssistance = 77,
		ApproachingBanditWaspAmbush = 93,
		ApproachingGenEriBeeBoops = 97,
		ApproachingMalbeeAbomihoneys = 98,
		TalkingToOverseerInCore = 99,
		ApproachingAhoneynation = 101,
		AncientCastleKeysEvent = 109,
		ApproachingDuneScorpion = 111,
		ApproachingWatcher = 117,
		ApproachingTheBeast = 137,
		ApproachingWaspKingdomHiveThroneRoom = 140,
		ApproachingPrimalWeevil = 151,
		EnteringColosseum = 163,
		ApproachingUltimaxTank = 193,
		ApproachingFridgeEntrance = 198,
		ApproachingWaspKing = 200
	}

	public enum Flag
	{
		UNASSIGNED = -1,
		GotBigCrankTopHalf = 113,
		GotBigCrankBottomHalf = 117,
		RescuedOverseer = 218,
		NewGameStarted = 691
	}

	public enum Song
	{
		UNASSIGNED = -1,
		Title = 12,
		LevelUp = 6
  }
}
