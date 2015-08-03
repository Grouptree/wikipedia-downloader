using System;
using Newtonsoft.Json;

namespace Wikimedia
{
    public class WikimediaPage
    {
        [JsonProperty("pageid")]
        public int PageId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("extract")]
        public string HtmlExtract { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("missing")]
        internal string Missing { get; set; }

        public bool IsMissing => Missing != null;

        [JsonProperty("canonicalurl")]
        public string Url { get; set; }
    }
}
