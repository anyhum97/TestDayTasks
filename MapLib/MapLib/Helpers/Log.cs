using Microsoft.Extensions.Logging;
using Serilog;

public static class Log
{
    public static ILoggerFactory LoggerFactory { get; }

    static Log()
    {
        // Явное обращение к Serilog.Log
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/maplib.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(dispose: true);
        });
    }

    public static ILogger<T> For<T>() => LoggerFactory.CreateLogger<T>();
}
