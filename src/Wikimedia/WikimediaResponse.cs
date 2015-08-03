using System;
using Newtonsoft.Json;

namespace Wikimedia
{
    public class WikimediaResponse
    {
        [JsonProperty("query")]
        public WikimediaQueryResponse Query { get; set; }

        [JsonProperty("error")]
        public WikimediaResponseError Error { get; set; }

        public string Raw { get; private set; }

        public bool HasMaxLagError => Error?.Code == "maxlag";

        public static WikimediaResponse Parse(string responseBody)
        {
            Check.NotNull(responseBody, nameof(responseBody));
            var resp = JsonConvert.DeserializeObject<WikimediaResponse>(responseBody);
            resp.Raw = responseBody;
            return resp;
        }
    }
}
