using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wikimedia
{
    public class WikimediaQueryResponse
    {
        [JsonProperty("pages")]
        public Dictionary<string, WikimediaPage> Pages { get; set; }
    }
}
