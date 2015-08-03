using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;

namespace Wikimedia
{
    [TestFixture(Description = "Parsing WikimediaResponse and other objects")]
    [Category("Unit")]
    [TestOf(typeof(WikimediaClient))]
    public class WikimediaClientTest
    {
        [Test]
        public void RequiresHttpClient()
        {
            var expectedMessage = $"Value cannot be null.{Environment.NewLine}Parameter name: httpClient";
            var e = Assert.Throws<ArgumentNullException>(() => new WikimediaClient(null, "foo", 5));
            Assert.AreEqual(expectedMessage, e.Message);
        }

        [Test]
        public void HasHttpClient()
        {
            var httpClient = Mock.Of<IHttpClient>();
            var client = new WikimediaClient(httpClient, "foo", 5);
            Assert.AreSame(httpClient, client.HttpClient);
        }

        [Test]
        public void RequiresUserAgent()
        {
            var httpClient = Mock.Of<IHttpClient>();
            var expectedMessage = $"Value cannot be null.{Environment.NewLine}Parameter name: userAgent";
            var e = Assert.Throws<ArgumentNullException>(() => new WikimediaClient(httpClient, null, 5));
            Assert.AreEqual(expectedMessage, e.Message);
        }

        [Test]
        public void HasUserAgent()
        {
            var httpClient = Mock.Of<IHttpClient>();
            var client = new WikimediaClient(httpClient, "foo", 5);
            Assert.AreEqual("foo", client.UserAgent);
        }

        [Test]
        public void HasMaxLag()
        {
            var httpClient = Mock.Of<IHttpClient>();
            var client = new WikimediaClient(httpClient, "foo", 5);
            Assert.AreEqual(5, client.MaxLag);
        }

        [Test]
        public void HasBaseUrl()
        {
            var httpClient = Mock.Of<IHttpClient>();
            var client = new WikimediaClient(httpClient, "foo", 25);
            Assert.AreEqual("https://en.wikipedia.org/w/api.php?format=json&maxlag=25", client.BaseUrl);
        }

        [Test]
        public void HasBaseQueryUrl()
        {
            var httpClient = Mock.Of<IHttpClient>();
            var client = new WikimediaClient(httpClient, "foo", 52);
            Assert.AreEqual("https://en.wikipedia.org/w/api.php?format=json&maxlag=52&action=query&prop=extracts|info&inprop=url&exlimit=max&exintro=1", client.BaseQueryUrl);
        }

        [Test]
        public void UsesDefaultClientWhenNotSupplied()
        {
            var client = new WikimediaClient("foo", 2);
            Assert.AreSame(HttpClient.Default, client.HttpClient);
        }

        [Test]
        public void ExecutesQueriesViaHttpClient()
        {
            var httpClient = new Mock<IHttpClient>(MockBehavior.Strict);
            var client = new WikimediaClient(httpClient.Object, "Foo", 5);
            var task = client.Query("foo=bar");
            httpClient.Verify(s => s.Get(client.BaseQueryUrl + "&foo=bar", "Foo"));
        }

        [Test]
        public async Task ReturnsQueryResponses()
        {
            var httpClient = new Mock<IHttpClient>(MockBehavior.Strict);
            var client = new WikimediaClient(httpClient.Object, "Foo", 5);
            httpClient.Setup(c => c.Get(client.BaseQueryUrl + "&foo=bar", "Foo")).ReturnsAsync("{'query':{}}");
            var response = await client.Query("foo=bar");
            Assert.NotNull(response);
        }

        [Test]
        public async Task ReturnsQueryResponsesWithPages()
        {
            var httpClient = new Mock<IHttpClient>(MockBehavior.Strict);
            var client = new WikimediaClient(httpClient.Object, "Foo", 5);
            httpClient.Setup(c => c.Get(client.BaseQueryUrl + "&foo=bar", "Foo")).ReturnsAsync("{'query':{'pages':{}}}");
            var response = await client.Query("foo=bar");
            Assert.NotNull(response.Pages);
        }

        [Test]
        public async Task ReturnsQueryResponsesWithPageData()
        {
            var httpClient = new Mock<IHttpClient>(MockBehavior.Strict);
            var client = new WikimediaClient(httpClient.Object, "Foo", 5);
            httpClient.Setup(c => c.Get(client.BaseQueryUrl + "&foo=bar", "Foo")).ReturnsAsync("{'query':{'pages':{'a':{'title':'Hello world'}}}}");
            var response = await client.Query("foo=bar");
            Assert.AreEqual("Hello world", response.Pages["a"].Title);
        }

        [Test]
        public async Task QueriesMaxNumberOfRandomArticles()
        {
            var httpClient = new Mock<IHttpClient>(MockBehavior.Strict);
            var client = new WikimediaClient(httpClient.Object, "Foo", 5);
            var expectedUrl = client.BaseQueryUrl + "&generator=random&grnnamespace=0&grnlimit=max";
            httpClient.Setup(c => c.Get(expectedUrl, "Foo")).ReturnsAsync("{'query':{}}");
            var response = await client.GetMaxRandomArticles();
            Assert.NotNull(response);
        }

        [Test]
        public async Task RetriesHttpQueryWhileHasMaxLagError()
        {
            var httpClient = new Mock<IHttpClient>(MockBehavior.Strict);
            var client = new WikimediaClient(httpClient.Object, "FooAgent", 5, minimumDelaySeconds: 0); // To avoid delays during tests
            var errorResponse = "{'error':{'code':'maxlag','info':'0 seconds lagged'}}";
            var successResponse = "{'query':{}}";
            // Sets up mock responses in sequence to simulate a maxlag errors followed by a success response the third time around
            httpClient
                .SetupSequence(c => c.Get(client.BaseQueryUrl + "&foo=bar", "FooAgent"))
                .Returns(Task.FromResult(errorResponse))
                .Returns(Task.FromResult(errorResponse))
                .Returns(Task.FromResult(successResponse));
            var response = await client.Query("foo=bar");
            Assert.NotNull(response);
        }
    }
}
