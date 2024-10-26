namespace FriendlyWorldBot.Utils;

public interface IMemoryConstants {
    public const string CreepIsBuilding = "isBuilding";
    public const string CreepTarget = "target";
    public const string CreepTempTarget = "tempTarget";
    public const string CreepKeepSaying = "keepSaying";
    public const string CreepLog = "log";
    
    public const string RoomBrokenStructures = "brokenStructures";
    public const string RoomMainSource = "mainSource";
    public const string RoomAdditionalExtensions = "additionalExtensions";
    public const string RoomName = "name";
    public const string RoomNameShort = "nameShort";
    public const string RoomCreatedRoadsForLevel = "createdRoadsForLevel";
    
    public const string GameRepairStructuresAtPercent = "repairStructuresAtPercent";
    public const double GameRepairStructuresAtPercentDefault = 0.80;
    public const string GameRepairWallsAtPercent = "repairWallsAtPercent";
    public const double GameRepairWallsAtPercentDefault = 0.00001; // should be around 3000
    
    public const string SpawnMain = "main";
}