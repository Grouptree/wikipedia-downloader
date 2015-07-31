using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WikipediaDownloader
{
    public class WikipediaPage
    {
        [JsonProperty("pageid")]
        public int PageId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("extract")]
        public string HtmlExtract { get; set; }

        [JsonProperty("missing")]
        internal string Missing { get; set; }

        internal static WikipediaPage[] Parse(string jsonResult)
        {
            var response = JsonConvert.DeserializeObject<WikimediaResponse>(jsonResult);
            return response.Query.Pages.Values.Where(p => p.Missing == null).ToArray();
        }

        class WikimediaResponse
        {
            [JsonProperty("query")]
            public WikimediaQueryResponse Query { get; set; }
        }

        class WikimediaQueryResponse
        {
            [JsonProperty("pages")]
            public Dictionary<string, WikipediaPage> Pages { get; set; }
        }
    }
}
