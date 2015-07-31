using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikipediaDownloader
{
    public class Downloader
    {
        readonly IHttp _http;

        public IHttp Http => _http;

        public Downloader(IHttp http)
        {
            if (http == null)
                throw new ArgumentNullException(nameof(http));
            _http = http;
        }

        public async Task<WikipediaPage> DownloadArticle(int pageId)
        {
            var url = $"https://en.wikipedia.org/w/api.php?action=query&pageids={pageId}&prop=extracts&format=json";
            var result = await Http.Get(url).ConfigureAwait(false);
            return WikipediaPage.Parse(result).FirstOrDefault();
        }

        public async Task<WikipediaPage> DownloadRandomArticle()
        {
            var url = $"https://en.wikipedia.org/w/api.php?action=query&generator=random&grnnamespace=0&prop=extracts&format=json";
            var result = await Http.Get(url).ConfigureAwait(false);
            return WikipediaPage.Parse(result).First();
        }
    }
}
