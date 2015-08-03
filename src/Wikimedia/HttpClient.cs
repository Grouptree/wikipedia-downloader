using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace Wikimedia
{
    public class HttpClient : IHttpClient
    {
        public static readonly IHttpClient Default = new HttpClient();

        HttpClient()
        {
        }

        public async Task<string> Get(string url, string userAgent)
        {
            Check.NotNull(url, nameof(url));
            Check.NotNull(userAgent, nameof(userAgent));
            var req = new HttpRequestMessage {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            };
            req.Headers.Add("User-Agent", userAgent);
            req.Headers.Add("accept", "application/json");
            HttpResponseMessage response;
            using (var client = new System.Net.Http.HttpClient())
                response = await client.SendAsync(req).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.ReasonPhrase);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
