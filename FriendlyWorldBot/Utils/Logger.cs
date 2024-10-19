using System;

namespace FriendlyWorldBot.Utils;

public class Logger {
    public static readonly Logger Instance = new();

    public void Debug(string message) {
        Console.WriteLine(message);
    }
    
    public void Info(string message) {
        Console.WriteLine(message);
    }
}