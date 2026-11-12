using System;

namespace KaizenLang.Core.Logging;

public enum LogLevel { Debug = 1, Info = 2, Warning = 3, Error = 4, None = 99 }

public static class Logger
{
    private static LogLevel _level = LogLevel.Info;

    static Logger()
    {
        try
        {
            var env = Environment.GetEnvironmentVariable("KAIZEN_LOG_LEVEL");
            if (!string.IsNullOrEmpty(env) && Enum.TryParse<LogLevel>(env, true, out var lvl))
                _level = lvl;
        }
        catch
        {
        }
    }

    public static LogLevel Level { get => _level; set => _level = value; }

    public static void Log(LogLevel level, string message)
    {
        if (level < _level || _level == LogLevel.None)
            return;
        var prefix = level switch
        {
            LogLevel.Debug => "[DBG]",
            LogLevel.Info => "[INF]",
            LogLevel.Warning => "[WRN]",
            LogLevel.Error => "[ERR]",
            _ => "[UNK]"
        };
        Console.WriteLine($"{prefix} {message}");
    }

    public static void Debug(string msg) => Log(LogLevel.Debug, msg);
    public static void Info(string msg) => Log(LogLevel.Info, msg);
    public static void Warn(string msg) => Log(LogLevel.Warning, msg);
    public static void Error(string msg) => Log(LogLevel.Error, msg);
}
