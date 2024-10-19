using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Utils;

public static class LoggerExtensions {
    public static void Debug(this ICreep creep, string message) {
        creep.Say("⚠");
        Logger.Instance.Debug($"[{creep.Id}] {message}");
    }

    public static void LogInfo(this ICreep creep, string message) {
        creep.Say("⚠");
        Logger.Instance.Info($"[{creep.Id}] {message}");
    }
}