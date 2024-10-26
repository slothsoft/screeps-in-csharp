using System;
using ScreepsDotNet;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Utils;

public class Logger
{
    public static readonly Logger Instance = new(Program.Game);

    private readonly IGame _game;
    
    private Logger(IGame game)
    {
        _game = game;
    }

    public LoggerSeverity Severity { get; set; } = LoggerSeverity.Info;
    
    public void Error(string message) => Log(LoggerSeverity.Error, message);

    public void Warning(string message) => Log(LoggerSeverity.Warning, message);

    public void Debug(string message) => Log(LoggerSeverity.Debug, message);

    public void Info(string message) => Log(LoggerSeverity.Info, message);

    internal void Log(LoggerSeverity severity, string message)
    {
        if (severity >= Severity)
        {
            Console.WriteLine(message);
        }
    }
}