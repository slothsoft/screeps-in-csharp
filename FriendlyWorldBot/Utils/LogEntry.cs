using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Utils;

public record LogEntry(LoggerSeverity Severity, string Message, IRoom? Room = null, ICreep? Creep = null);