using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using registry_browser.Helpers;

namespace registry_browser.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly string baseAdress;

        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
            this.baseAdress = "https://registry.comma-soft.net";
        }

        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.baseAdress);
                logger.LogInformation("Using the registry: {registry}", this.baseAdress);

                // todo move this in startup if possible
                var response = await client.GetAsync("/v2/");
                response.EnsureSuccessStatusCode();

                // todo handle url not reachable
                // todo handle ssl / no ssl?
                // todo make registry path configurable, exit in case of error

                var catalogResponse = await client.GetAsync("/v2/_catalog");
                catalogResponse.EnsureSuccessStatusCode();

                var catalog = JsonConvert.DeserializeObject<Pocos.Repositories>(await catalogResponse.Content.ReadAsStringAsync());

                return View(catalog);
            }
        }

        public async Task<IActionResult> GetRepositoryTags(string repository)
        {
            var manifestAddress = $"{this.baseAdress}/v2/{repository}/manifests";
            ViewData["manifestAddress"] = manifestAddress;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.baseAdress);
                // todo sanitize and check this?
                var repositoryTagResponse = await client.GetAsync($"/v2/{repository}/tags/list");
                repositoryTagResponse.EnsureSuccessStatusCode();

                var repositoryTags =
                    JsonConvert.DeserializeObject<Pocos.RepositoryTags>(
                        await repositoryTagResponse.Content.ReadAsStringAsync());

                // todo natural sort + latest first?

                repositoryTags.tags =
                    repositoryTags.tags.OrderByDescending(f => f, new CustomComparer<string>(NaturalSorting.CompareNatural))
                        .ToList();

                var address = RemoveProtocolFromDockerAddress(repository);
                ViewData["repositoryAddress"] = address;

                return View(repositoryTags);

            }
        }

        private string RemoveProtocolFromDockerAddress(string repository)
        {
            var address = $"{this.baseAdress.ToLower()}/{repository}";
            address = address.Replace("http://", string.Empty);
            address = address.Replace("https://", string.Empty);
            return address;
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
