using Moq;
using Moq.Protected;
using NUnit.Framework;
using PostPRsToTwitter;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace Tests
{
    [TestFixture]
    public class GitHubHttpClientTests
    {
        [Test]
        public async Task Test_GitHubHttpClient_GetNewPullRequestsNoRunHistory_ShouldReturn2PRsToPost()
        {
            var testJson = "[{\"html_url\":\"dummy_url_1\",\"number\":1,\"user\":{\"login\":\"TestUser1\"}},{\"html_url\":\"dummy_url_2\",\"number\":2,\"user\":{\"login\":\"TestUser2\"}}]";
            var testUri = new Uri("https://test_uri.com");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            
            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(testJson),
                }));
        
            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.BaseAddress = testUri;

            GitHubHttpClient client = new GitHubHttpClient(httpClient);

            var mockRunHistory = new Mock<RunHistory>();
            mockRunHistory.SetupGet(s => s.PostedTweets).Returns(new HashSet<string>());

            var prs = await client.GetNewPullRequests(mockRunHistory.Object);
            
            Assert.AreEqual(2, prs.Count);
        }

        [Test]
        public async Task Test_GitHubHttpClient_GetNewPullRequestsRunHistory_ShouldReturn1PRToPost()
        {
            var testJson = "[{\"html_url\":\"dummy_url_1\",\"number\":1,\"user\":{\"login\":\"TestUser1\"}},{\"html_url\":\"dummy_url_2\",\"number\":2,\"user\":{\"login\":\"TestUser2\"}}]";
            var testUri = new Uri("https://test_uri.com");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            
            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(testJson),
                }));
        
            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.BaseAddress = testUri;

            GitHubHttpClient client = new GitHubHttpClient(httpClient);

            var mockRunHistory = new Mock<RunHistory>();
            HashSet<string> history = new HashSet<string>();
            history.Add("1");
            mockRunHistory.SetupGet(s => s.PostedTweets).Returns(history);

            var prs = await client.GetNewPullRequests(mockRunHistory.Object);
            
            Assert.AreEqual(1, prs.Count);
        }

        [Test]
        public async Task Test_GitHubHttpClient_GetNewPullRequestsNoNewPRs_ShouldNotReturnAnyPRs()
        {
            var testJson = "[{\"html_url\":\"dummy_url_1\",\"number\":1,\"user\":{\"login\":\"TestUser1\"}},{\"html_url\":\"dummy_url_2\",\"number\":2,\"user\":{\"login\":\"TestUser2\"}}]";
            var testUri = new Uri("https://test_uri.com");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            
            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(testJson),
                }));
        
            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.BaseAddress = testUri;

            GitHubHttpClient client = new GitHubHttpClient(httpClient);

            var mockRunHistory = new Mock<RunHistory>();
            HashSet<string> history = new HashSet<string>();
            history.Add("1");
            history.Add("2");
            mockRunHistory.SetupGet(s => s.PostedTweets).Returns(history);

            var prs = await client.GetNewPullRequests(mockRunHistory.Object);
            
            Assert.AreEqual(0, prs.Count);
        }

        [Test]
        public async Task Test_GitHubHttpClient_APICallReturns500Error_ShouldReturn0PRToPost()
        {
            var testJson = "[{\"error\":\"something broke\"}]";
            var testUri = new Uri("https://test_uri.com");
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            
            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(testJson),
                }));
        
            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.BaseAddress = testUri;

            GitHubHttpClient client = new GitHubHttpClient(httpClient);

            var mockRunHistory = new Mock<RunHistory>();
            HashSet<string> history = new HashSet<string>();
            history.Add("1");
            mockRunHistory.SetupGet(s => s.PostedTweets).Returns(history);

            var prs = await client.GetNewPullRequests(mockRunHistory.Object);
            
            Assert.AreEqual(0, prs.Count);
        }
    }
}