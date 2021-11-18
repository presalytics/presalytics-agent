
namespace cli
{
    using Serilog;
    using Serilog.Sinks.File;
    using presalytics.Models;
    using Serilog.Events;
    using System;
    using System.IO;

    public static class FileLogger
    {
        public static ILogger GetLogger()
        {
            string fldr = SerializationExtensions.GetOrCreateUserDataFolder();
            string fname = Path.Join(fldr, "presalytics-cli-log.txt");
            return new LoggerConfiguration()
                .MinimumLevel.Override("presalytics", LogEventLevel.Debug)
                .WriteTo.File(fname, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}