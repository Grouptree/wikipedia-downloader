using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
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
            var mongoDb = GetMongoDBFromArgs(args);
            if (mongoDb == null) {
                Console.WriteLine("No MongoDB connection string specified, or string contains no database name.");
                Environment.Exit(1);
                return;
            }
            MainAsync(mongoDb).GetAwaiter().GetResult();
        }

        static IMongoDatabase GetMongoDBFromArgs(string[] args)
        {
            if (args.Length < 1) return null;
            if (args[0] == null) return null;
            MongoUrl connectionString;
            try {
                connectionString = new MongoUrl(args[0]);
            }
            catch (MongoConfigurationException) {
                return null;
            }
            if (connectionString.DatabaseName == null) return null;
            var client = new MongoClient(connectionString);
            return client.GetDatabase(connectionString.DatabaseName);
        }

        static async Task MainAsync(IMongoDatabase db)
        {
            var client = new WikimediaClient(USER_CLIENT, MAX_LAG);
            client.LaggedResponse += (sender, response) =>
                Console.WriteLine($"Response was lagged {response.Error.SecondsLagged} seconds. Waiting to issue next query.");

            var articles = db.GetCollection<BsonDocument>("Articles");
            await articles.Indexes.CreateOneAsync("{PageId: 1}");
            var start = await articles.CountAsync("{}");
            Console.WriteLine($"Starting download. {start.ToString("#,###,##0")} articles currently into the database. Press Ctrl+C to interrupt.");

            // Keep the same number of download tasks running at the same time
            int MAX_TASKS = 50;
            var queue = new List<Task>(MAX_TASKS);
            for(int i = 0; i < MAX_TASKS; i++) {
                queue.Add(InsertRandomDocumentsIntoDatabase(client, articles));
            }
            int completedTasks = 0;
            var sw = new Stopwatch();
            sw.Start();
            while (true) {
                // keep replacing finished tasks with new ones until the process is interrupted by the user.
                var finishedTask = await Task.WhenAny(queue);
                queue.Remove(finishedTask);
                queue.Add(InsertRandomDocumentsIntoDatabase(client, articles));
                completedTasks++;
                if(completedTasks % 100 == 0) {
                    var total = await articles.CountAsync("{}");
                    var soFar = total - start;
                    var totalSeconds = sw.Elapsed.TotalSeconds;
                    var speed = soFar / (double)totalSeconds;
                    Console.WriteLine($"Total of {total.ToString("#,###,##0")} articles inserted into the database ({speed.ToString("0.00")} articles/sec). Press Ctrl+C to interrupt.");
                }
            }
        }

        static BsonDocument DocumentFromPage(WikimediaPage page)
        {
            return new BsonDocument {
                { "PageId", page.PageId },
                { "Title", page.Title },
                { "Extract", page.HtmlExtract },
                { "Url", page.Url }
            };
        }

        static async Task InsertRandomDocumentsIntoDatabase(WikimediaClient client, IMongoCollection<BsonDocument> collection)
        {
            var response = await client.GetMaxRandomArticles();
            var updates = response.Pages.Values.Select(page => {
                var doc = DocumentFromPage(page);
                return collection.ReplaceOneAsync(
                    Builders<BsonDocument>.Filter.Eq(d => d["PageId"], page.PageId), doc,
                    new UpdateOptions { IsUpsert = true });
            });
            await Task.WhenAll(updates);
        }
    }
}
