using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;

namespace WikipediaDownloader
{
    [TestFixture(Description = "Downloader class", TestOf = typeof(Downloader))]
    [Category("Unit")]
    public class DownloaderTest
    {
        [Test]
        public async Task GetsAnArticleByItsId()
        {
            var sampleJson = @"{
                ""batchcomplete"": """",
                ""query"": {
                        ""pages"": {
                            ""10257550"": {
                                ""pageid"": 10257550,
                                ""ns"": 0,
                                ""title"": ""ACEnet"",
                                ""extract"": ""<p><b>ACEnet</b> or the <i>Atlantic Computational Excellence Network</i> is a partnership...""
                            }
                        }
                    }
                }";
            var url = $"https://en.wikipedia.org/w/api.php?action=query&pageids=10257550&prop=extracts&format=json";
            var server = new Mock<IHttp>();
            server.Setup(s => s.Get(url)).ReturnsAsync(sampleJson);
            var downloader = new Downloader(server.Object);
            var page = await downloader.DownloadArticle(10257550);
            Assert.AreEqual(10257550, page.PageId);
            Assert.AreEqual("ACEnet", page.Title);
            Assert.AreEqual("<p><b>ACEnet</b> or the <i>Atlantic Computational Excellence Network</i> is a partnership...", page.HtmlExtract);
        }

        [Test]
        public async Task GetsRandomArticle()
        {
            var sampleJson = @"{
                ""batchcomplete"": """",
                ""query"": {
                        ""pages"": {
                            ""1234"": {
                                ""pageid"": 1234,
                                ""ns"": 0,
                                ""title"": ""Foo"",
                                ""extract"": ""<p>FooBar</p>""
                            }
                        }
                    }
                }";
            var url = "https://en.wikipedia.org/w/api.php?action=query&generator=random&grnnamespace=0&prop=extracts&format=json";
            var server = new Mock<IHttp>();
            server.Setup(s => s.Get(url)).ReturnsAsync(sampleJson);
            var downloader = new Downloader(server.Object);
            var page = await downloader.DownloadRandomArticle();
            Assert.AreEqual(1234, page.PageId);
            Assert.AreEqual("Foo", page.Title);
            Assert.AreEqual("<p>FooBar</p>", page.HtmlExtract);
        }
    }
}
