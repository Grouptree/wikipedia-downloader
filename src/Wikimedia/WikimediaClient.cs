using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Wikimedia
{
    public class WikimediaClient
    {
        const string BASE_URL = "https://en.wikipedia.org/w/api.php";

        readonly IHttpClient _httpClient;
        readonly string _userAgent;
        readonly int _maxLag;
        readonly int _minimumDelaySeconds;
        readonly string _baseUrl;
        readonly string _baseQueryUrl;

        public IHttpClient HttpClient => _httpClient;
        public string UserAgent => _userAgent;
        public int MaxLag => _maxLag;
        public int MinimumDelaySeconds => _minimumDelaySeconds;
        public string BaseUrl => _baseUrl;
        public string BaseQueryUrl => _baseQueryUrl;

        public WikimediaClient(IHttpClient httpClient, string userAgent, int maxLag, int minimumDelaySeconds = 5)
        {
            Check.NotNull(httpClient, nameof(httpClient));
            Check.NotNull(userAgent, nameof(userAgent));
            _httpClient = httpClient;
            _userAgent = userAgent;
            _maxLag = maxLag;
            _minimumDelaySeconds = minimumDelaySeconds;
            _baseUrl = $"{BASE_URL}?format=json&maxlag={MaxLag}";
            _baseQueryUrl = $"{BaseUrl}&action=query&prop=extracts|info&inprop=url&exlimit=max&exintro=1";
        }

        public WikimediaClient(string userAgent, int maxLag, int minimumDelaySeconds = 5)
            : this(Wikimedia.HttpClient.Default, userAgent, maxLag, minimumDelaySeconds)
        {
        }

        public EventHandler<WikimediaResponse> LaggedResponse;

        public virtual void OnLaggedResponse(WikimediaResponse response)
        {
            LaggedResponse?.Invoke(this, response);
        }

        public async Task<WikimediaQueryResponse> Query(params string[] urlParameters)
        {
            while (true) {
                var url = BaseQueryUrl + "&" + string.Join("&", urlParameters);
                var respBody = await HttpClient.Get(url, UserAgent);
                var response = WikimediaResponse.Parse(respBody);
                if (!response.HasMaxLagError) return response.Query;
                OnLaggedResponse(response);
                var secondstoWait = Math.Max(MinimumDelaySeconds, response.Error.SecondsLagged);
                await Task.Delay(secondstoWait * 1000).ConfigureAwait(false);
            }
        }

        public Task<WikimediaQueryResponse> GetMaxRandomArticles()
        {
            return Query("generator=random", "grnnamespace=0", "grnlimit=max");
        }
    }
}
