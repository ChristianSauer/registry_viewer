using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Optional;

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

        public Optional.Option<Uri, Exception> Uri
        {
            get
            {
                try
                {
                    return Option.Some<Uri, Exception>(new Uri(this.Url));
                }
                catch (ArgumentNullException ex)
                { 
                    return Option.None<Uri, Exception>(ex);
                }
                catch (UriFormatException ex)
                {
                    return Option.None<Uri, Exception>(ex);
                }
            }
        }

        public string Url { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public void Validate()
        {
        }
    }
}
