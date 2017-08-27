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
using registry_browser.Helpers;

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

            EnsureRegistryIsReachable();

            services.Configure<Helpers.RegistryOptions>(Configuration.GetSection("Registry"));
        }

        private void EnsureRegistryIsReachable()
        {
             var url = Configuration.GetSection("Registry")["Url"];
            var user = Configuration.GetSection("Registry")["User"];
            var password = Configuration.GetSection("Registry")["Password"];

            _logger.LogInformation($"Using Registry: {url} User: {user} Password: {new string('*', password.Length)}");

            var options =  new RegistryOptions(){User = user, Password = password, Url = url};
            options.Validate(this._logger);

            _logger.LogInformation("Trying to connect to the registry");

            try
            {
                RegistryConnectionTest.EnsureRegistryIsReachable(options);
            }
            catch (System.Exception ex)
            {   
                _logger.LogError(9999, ex, "Could not reach the registry. Please check the connection settings");
                throw new ArgumentException("Registry not reachable, Aborting");
            }

            _logger.LogInformation("Registry is reachable");
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
