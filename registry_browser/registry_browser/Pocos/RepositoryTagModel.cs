using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace registry_browser.Pocos
{
    public class RepositoryTagModel
    {
        public Uri BaseUri { get; set; }

        public string ManifestAddress { get; set; }
        public string RepositoryName { get; set; }

        public RepositoryTags RepositoryTags { get; set; }
    }
}
