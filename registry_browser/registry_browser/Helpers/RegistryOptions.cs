using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Optional;
using Optional.Linq;
using Optional.Unsafe;

namespace registry_browser.Helpers
{
    public class RegistryOptions
    {
        public RegistryOptions()
        {
            this.Url = string.Empty;
            this.User = string.Empty;
            this.Password = string.Empty;
        }

        public Uri GetUrlAsrUri()
        {
            return new Uri(this.Url);
        }

        public string Url { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public void Validate(ILogger logger)
        {
            var userIsEmpty = string.IsNullOrWhiteSpace(this.User);
            var passWordIsEmptry = string.IsNullOrWhiteSpace(this.Password);

            if ((userIsEmpty && !passWordIsEmptry) | (!userIsEmpty) && passWordIsEmptry)
            {
                throw new ArgumentException("If Registry__User is set, Registry__Password must be set and cannot be an emptry string. Also, the reverse must be true, too.");
            }

            try
            {
                var _ = this.GetUrlAsrUri();
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is UriFormatException)
            {
                this.HandleInvalidUri(ex, logger);
            }
        }

        public HttpClient AddBasicAuthToClient(HttpClient client)
        {
            if (!this.HasBasicAuth())
            {
                return client;
            }

            var credentials = Encoding.ASCII.GetBytes($"{this.User}:{this.Password}");

            var base64String = Convert.ToBase64String(credentials);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
            return client;
        }

        private bool HasBasicAuth()
        {
            return !string.IsNullOrWhiteSpace(this.User) & !string.IsNullOrWhiteSpace(this.Password);
        }


        private Uri HandleInvalidUri(Exception ex, ILogger logger)
        {
            if (ex is ArgumentNullException)
            {
                logger.LogError(1, ex, "The Option Registry__Url cannot be empty! Please set the environment variable");
            }
            else if (ex is UriFormatException)
            {
                logger.LogError(2, ex,
                    $"The Option Registry__Url is malformed, please provide a valid Uri, e.g. http://example.com. The value is:'{this.Url}'");
            }
            else
            {
                logger.LogError(3, ex, $"Unknown error when trying to format Registry__Url. The value is: {this.Url}");
            }

            throw ex;
        }
    }
}
