using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wikimedia
{
    [TestFixture(Description = "Integration live tests")]
    [Category("Integration")]
    public class IntegrationTest
    {
        // NOTE: this test fixture depends on a network connection
        // and the Wikipedia site being available at the moment it's run.

        const string USER_CLIENT = "Grouptree WikipediaDownloader (https://github.com/Grouptree/wikipedia-downloader; support@grouptree.com)";

        [Test]
        public async Task GetsRandomArticles()
        {
            var client = new WikimediaClient(USER_CLIENT, 5);
            var response = await client.GetMaxRandomArticles();
            Assert.IsNotEmpty(response.Pages);
        }
    }
}
