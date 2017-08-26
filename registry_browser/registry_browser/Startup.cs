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

namespace registry_browser
{
    public class Startup
    {
        private ILogger _logger;
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            _logger = loggerFactory.CreateLogger<Startup>();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var url = Configuration.GetSection("Registry")["Url"];
            var user = Configuration.GetSection("Registry")["User"];
            var password = Configuration.GetSection("Registry")["Password"];

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("Registry.Url is null. Please provide a valid url using environment variables, commandline or appsettings.json. See readme.md for details");
            }

            _logger.LogInformation($"Using Registry: {url} User: {user} Password: {new string('*', password.Length)}");

            services.Configure<Helpers.RegistryOptions>(Configuration.GetSection("Registry"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
