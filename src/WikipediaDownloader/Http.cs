using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WikipediaDownloader
{
    public class Http : IHttp
    {
        public async Task<string> Get(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            var req = new HttpRequestMessage {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            };
            req.Headers.Add("User-Agent", "Grouptree WikipediaDownloader (https://github.com/Grouptree/wikipedia-downloader; support@grouptree.com)");
            req.Headers.Add("accept", "application/json");
            HttpResponseMessage response;
            using (var client = new HttpClient())
                response = await client.SendAsync(req).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.ReasonPhrase);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
