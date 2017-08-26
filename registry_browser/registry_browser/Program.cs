using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;

namespace registry_browser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                    // todo should be possible to configure this via appsettings.json, but net core 2.0 has some issues apparently.
                    Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.ColoredConsole().CreateLogger();
                    logging.AddSerilog(dispose: true);
                }).Build();

                // todo add registry configuration back in
    }
}
