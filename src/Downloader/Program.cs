using System;

using System.Threading.Tasks;
using Wikimedia;

namespace WikipediaDownloader
{
    class Program
    {
        // Reference: https://www.mediawiki.org/wiki/API:Main_page#Identifying_your_client
        const string USER_CLIENT = "Grouptree WikipediaDownloader (https://github.com/Grouptree/wikipedia-downloader; support@grouptree.com)";

        // Reference: https://www.mediawiki.org/wiki/API:Etiquette
        // Reference: https://www.mediawiki.org/wiki/Manual:Maxlag_parameter
        const int MAX_LAG = 1;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var client = new WikimediaClient(USER_CLIENT, MAX_LAG);
            client.LaggedResponse += (sender, response) =>
                Console.WriteLine($"Response was lagged {response.Error.SecondsLagged} seconds. Waiting to issue next query.");

            while (true) {
                var response = await client.GetMaxRandomArticles();
                foreach (var page in response.Pages.Values) {
                    Console.WriteLine($"I got a page = {page.Title}");
                }
            }
        }
    }
}
