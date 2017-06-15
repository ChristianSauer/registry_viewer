using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace registry_browser
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            AddCommandLineArgsToConfig(builder);

            AddUserSecretsToConfig(env, builder);

            Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .WriteTo.ColoredConsole()
              .CreateLogger();

            Configuration = builder.Build();
        }

        private static void AddUserSecretsToConfig(IHostingEnvironment env, IConfigurationBuilder builder)
        {
            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see 
                // http://go.microsoft.com/fwlink/?LinkID=532709
                // note that sections are indicated by ":" not by "__" (like envs) or "_"
                builder.AddUserSecrets<Startup>();
            }
        }

        private void AddCommandLineArgsToConfig(IConfigurationBuilder builder)
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            var commandLineArgsWithoutFirstArg = commandLineArgs.Reverse().Take(commandLineArgs.Length - 1).ToArray();

            if (commandLineArgsWithoutFirstArg.Length > 0)
            {
                builder.AddCommandLine(commandLineArgsWithoutFirstArg);
            }
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Configure using a sub-section of the appsettings.json file.
            services.Configure<Helpers.RegistryOptions>(Configuration.GetSection("Registry"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            loggerFactory.AddSerilog();
            // Ensure any buffered events are sent at shutdown
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
