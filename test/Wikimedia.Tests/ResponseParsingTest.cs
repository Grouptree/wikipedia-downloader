using System;
using System.Linq;
using NUnit.Framework;

namespace Wikimedia
{
    [TestFixture(Description = "Parsing WikimediaResponse and other objects")]
    [Category("Unit")]
    [TestOf(typeof(WikimediaResponse))]
    public class ResponseParsingTest
    {
        [Test]
        public void RequiresResponseBody()
        {
            var expectedMessage = $"Value cannot be null.{Environment.NewLine}Parameter name: responseBody";
            var e = Assert.Throws<ArgumentNullException>(() => WikimediaResponse.Parse(null));
            Assert.AreEqual(expectedMessage, e.Message);
        }

        [Test]
        public void HasRawResponse()
        {
            var resp = WikimediaResponse.Parse("{ 'foo': 'Bar' }");
            Assert.AreEqual("{ 'foo': 'Bar' }", resp.Raw);
        }

        [Test]
        public void Parseserror()
        {
            var resp = WikimediaResponse.Parse("{'error':{}}");
            Assert.NotNull(resp.Error);
        }

        [Test]
        public void ParsesErrorCode()
        {
            var resp = WikimediaResponse.Parse("{'error':{'code':'foocode'}}");
            Assert.AreEqual("foocode", resp.Error?.Code);
        }

        [Test]
        public void ParsesErrorInfo()
        {
            var resp = WikimediaResponse.Parse("{'error':{'info':'bad stuff happened'}}");
            Assert.AreEqual("bad stuff happened", resp.Error?.Info);
        }

        [Test]
        public void ParsesErrorSeeAlso()
        {
            var resp = WikimediaResponse.Parse("{'error':{'*':'see this link'}}");
            Assert.AreEqual("see this link", resp.Error?.SeeAlso);
        }

        [Test]
        public void KnowsIfNotMaxLagError()
        {
            var resp = WikimediaResponse.Parse("{'error':{'code':'foobar'}}");
            Assert.False(resp.Error?.IsMaxLag);
        }

        [Test]
        public void KnowsIfMaxLagError()
        {
            var resp = WikimediaResponse.Parse("{'error':{'code':'maxlag'}}");
            Assert.True(resp.Error?.IsMaxLag);
        }

        [Test]
        public void ResponseKnowsIfMaxLagError()
        {
            var resp = WikimediaResponse.Parse("{'error':{'code':'maxlag'}}");
            Assert.True(resp.HasMaxLagError);
        }

        [Test]
        public void ParsesSecondsLagged()
        {
            var resp = WikimediaResponse.Parse("{'error':{'code':'maxlag','info':'Blah blah 5 seconds lagged lorem ipsum'}}");
            Assert.AreEqual(5, resp.Error.SecondsLagged);
        }

        [Test]
        public void ThrowsForSecondsLaggedWhenNotMaxLag()
        {
            var resp = WikimediaResponse.Parse("{'error':{'code':'foobar'}}");
            Assert.Throws<InvalidOperationException>(() => { int x = resp.Error.SecondsLagged; });
        }

        [Test]
        public void ReturnsZeroWhenSecondsLaggedInfoNotAvailable()
        {
            var resp = WikimediaResponse.Parse("{'error':{'code':'maxlag'}}");
            Assert.AreEqual(0, resp.Error?.SecondsLagged);
        }

        [Test]
        public void ParsesQueryResponse()
        {
            var resp = WikimediaResponse.Parse("{'query':{}}");
            Assert.NotNull(resp.Query);
        }

        [Test]
        public void ParsesQueryPages()
        {
            var resp = WikimediaResponse.Parse("{'query':{pages:{'a':{}, 'b':{}}}}");
            Assert.AreEqual(new[] { "a", "b" }, resp.Query.Pages.Keys.ToArray());
        }

        [Test]
        public void ParsesQueryPageIds()
        {
            var resp = WikimediaResponse.Parse("{'query':{pages:{'a':{'pageid': '1234'}}}}");
            Assert.AreEqual(1234, resp.Query.Pages["a"].PageId);
        }

        [Test]
        public void ParsesQueryPageTitles()
        {
            var resp = WikimediaResponse.Parse("{'query':{pages:{'a':{'title': 'foo bar'}}}}");
            Assert.AreEqual("foo bar", resp.Query.Pages["a"].Title);
        }

        [Test]
        public void ParsesQueryPageHtmlExtracts()
        {
            var resp = WikimediaResponse.Parse("{'query':{pages:{'a':{'extract': '<p>hello</p>'}}}}");
            Assert.AreEqual("<p>hello</p>", resp.Query.Pages["a"].HtmlExtract);
        }

        [Test]
        public void ParsesQueryPageLength()
        {
            var resp = WikimediaResponse.Parse("{'query':{pages:{'a':{'length': '123'}}}}");
            Assert.AreEqual(123, resp.Query.Pages["a"].Length);
        }

        [Test]
        public void ParsesQueryPageUrls()
        {
            var resp = WikimediaResponse.Parse("{'query':{pages:{'a':{'canonicalurl': 'http://foo.com/bar'}}}}");
            Assert.AreEqual("http://foo.com/bar", resp.Query.Pages["a"].Url);
        }
    }
}
