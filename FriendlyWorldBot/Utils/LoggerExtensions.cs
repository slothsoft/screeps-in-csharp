using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Utils;

public static class LoggerExtensions {

    public static string GetIcon(this LoggerSeverity severity)
    {
        return severity switch
        {
            LoggerSeverity.Error => "🛑",
            LoggerSeverity.Warning => "⚠️",
            LoggerSeverity.Debug => "🐞",
            LoggerSeverity.Info => "ℹ️",
            _ => " ",
        };
    }
    
    public static void LogError(this ICreep creep, string message) => creep.Log(LoggerSeverity.Error, message);

    public static void LogWarning(this ICreep creep, string message) => creep.Log(LoggerSeverity.Warning, message);
    
    public static void LogDebug(this ICreep creep, string message) => creep.Log(LoggerSeverity.Debug, message);

    public static void LogInfo(this ICreep creep, string message) => creep.Log(LoggerSeverity.Info, message);
    
    private static void Log(this ICreep creep, LoggerSeverity severity, string message) {
        if (severity >= Logger.Instance.Severity && creep.Exists)
        {
            creep.Say(severity.GetIcon());
            creep.Memory.SetValue(CreepKeepSaying, 15);
            creep.Memory.SetValue(CreepLog, message);
            Logger.Instance.Log(new LogEntry(severity, message, creep.Room, creep));
        }
    }
    
    public static void LogError(this IRoom room, string message) => room.Log(LoggerSeverity.Error, message);

    public static void LogWarning(this IRoom room, string message) => room.Log(LoggerSeverity.Warning, message);
    
    public static void LogDebug(this IRoom room, string message) => room.Log(LoggerSeverity.Debug, message);

    public static void LogInfo(this IRoom room, string message) => room.Log(LoggerSeverity.Info, message);
    
    private static void Log(this IRoom room, LoggerSeverity severity, string message) {
        if (severity >= Logger.Instance.Severity && room.Exists)
        {
            Logger.Instance.Log(new LogEntry(severity, message, room));
        }
    }
}