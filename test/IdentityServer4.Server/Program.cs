using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace IdentityServer4.Server
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            using var host = CreateWebHostBuilder(args).Build();

            return host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] _)
        {
            return WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .UseSerilog(ConfigureLogger);
        }

        private static void ConfigureLogger(WebHostBuilderContext hostContext, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(AppContext.BaseDirectory, "Logs",
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.log"))
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                    theme: AnsiConsoleTheme.Literate);
        }
    }
}