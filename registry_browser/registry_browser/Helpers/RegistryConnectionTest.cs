using System.Net.Http;

namespace registry_browser.Helpers
{
    public static class RegistryConnectionTest
    {
        public static void EnsureRegistryIsReachable(Helpers.RegistryOptions registryOptions)
        {
            using (var client = new HttpClient())
            {
                registryOptions.AddBasicAuthToClient(client);
                client.BaseAddress = registryOptions.GetUrlAsrUri();

                var response = client.GetAsync("/v2/").Result;
                response.EnsureSuccessStatusCode();
            }
        }
    }
}