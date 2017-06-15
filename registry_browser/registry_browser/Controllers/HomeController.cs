﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using registry_browser.Helpers;
using registry_browser.Pocos;

namespace registry_browser.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly Uri baseAdress;
        private RegistryOptions registryOptions;

        public HomeController(ILogger<HomeController> logger, IOptions<RegistryOptions> registryOptionsAcessor)
        {
            this.registryOptions = registryOptionsAcessor.Value;
            this.logger = logger;
            this.registryOptions.Validate(logger);

            this.baseAdress = registryOptions.GetUrlAsrUri();
            this.EnsureRegistryIsReachable(registryOptions);
        }

        private void EnsureRegistryIsReachable(Helpers.RegistryOptions registryOptions)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = this.baseAdress;
                logger.LogInformation("Using the registry: {registry}", this.baseAdress);

                // todo move this in startup if possible
                var response = client.GetAsync("/v2/").Result;
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                registryOptions.AddBasicAuthToClient(client);
                client.BaseAddress = this.baseAdress;
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = this.baseAdress;
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

                var manifestAddress = $"{this.baseAdress}/v2/{repository}/manifests";
                var model = new RepositoryTagModel()
                {
                    ManifestAddress = manifestAddress,
                    BaseUri = this.baseAdress,
                    Repository = repository,
                    RepositoryTags = repositoryTags,
                    RepositoryAddress = address
                    
                };


                return View(model);
            }
        }

        private string RemoveProtocolFromDockerAddress(string repository)
        {
            var address = $"{this.baseAdress.ToString().ToLower()}/{repository}";
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

        public async Task<IActionResult> DeleteTag(string repository, string tag)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = this.baseAdress;

                // get manifest first
                var manifestResponse = await client.GetAsync($"/v2/{repository}/manifests/{tag}");
                manifestResponse.EnsureSuccessStatusCode();

                IEnumerable<string> digestHeaders;
                if (!manifestResponse.Headers.TryGetValues("Docker-Content-Digest", out digestHeaders))
                {
                    throw new ArgumentException("The response did not contain a Docker-Content-Digest Header, aborting delete");
                }
                var digestHeader = digestHeaders.First();

                this.logger.LogInformation("Trying to delete the digest: {digest}", digestHeader);

                client.DefaultRequestHeaders.Add("Accept", "application/vnd.docker.distribution.manifest.v2+json");
                var deleteResponse = await client.DeleteAsync($"/v2/{repository}/manifests/{digestHeader}");
                if (deleteResponse.StatusCode == HttpStatusCode.MethodNotAllowed)
                {
                    this.logger.LogWarning("Could not delete {repository}:{tag} because the registry does not allow deletes.",
                        repository, tag);

                    return View("CannotDeleteTag", new TagDeletedModel {Repository = repository, Tag = tag});

                }

                deleteResponse.EnsureSuccessStatusCode();

                return View("TagDeleted", new TagDeletedModel { Repository=repository, Tag=tag} );
            }
        }
    }
}