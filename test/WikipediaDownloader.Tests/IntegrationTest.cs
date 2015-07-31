using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;

namespace WikipediaDownloader
{
    [TestFixture(Description = "Downloader class", TestOf = typeof(Downloader))]
    [Category("Integration")]
    public class IntegrationTest
    {
        // NOTE: this test depends on a network connection
        // and the Wikipedia site being available at the moment it's run.
        // It also makes assumptions about the returned content that may not be true in the future.

        [Test]
        public async Task GetsAnArticleByItsId()
        {
            var http = new Http();
            var downloader = new Downloader(http);
            var page = await downloader.DownloadArticle(31717);
            Assert.AreEqual(31717, page.PageId);
            Assert.AreEqual("United Kingdom", page.Title);
            StringAssert.StartsWith("<p>The <b>United Kingdom of Great Britain and Northern Ireland</b>", page.HtmlExtract);
        }

        [Test]
        public async Task ReturnsNullIfArticleNotFound()
        {
            var http = new Http();
            var downloader = new Downloader(http);
            var page = await downloader.DownloadArticle(-1);
            Assert.Null(page);
        }
    }
}
