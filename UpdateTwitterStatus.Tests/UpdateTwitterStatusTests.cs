using Moq;
using NUnit.Framework;
using PostPRsToTwitter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class UpdateTwitterStatusTests
    {
        HashSet<string> _postedTweets;
        List<PullRequest> _prList;

        [SetUp]
        public void Setup()
        {
            _postedTweets = new HashSet<string>();
            _prList = new List<PullRequest>();
        }

        [Test]
        public void Test_UpdateTwitterStatus_RunWithNoRunHistory_ShouldPost2Tweets()
        {
            var mockRunHistory = new Mock<RunHistory>();
            mockRunHistory.SetupGet(s => s.PostedTweets).Returns(_postedTweets);

            var mockGithubClient = new Mock<GitHubHttpClient>();  
            var _prList = new List<PullRequest>();
            
            var pr1 = new PullRequest();
            pr1.Url = "url1";
            pr1.Number = "1";
            pr1.User = new GitHubUser();
            pr1.User.Username = "user1";
            _prList.Add(pr1);
            
            var pr2 = new PullRequest();
            pr2.Url = "url2";
            pr2.Number = "2";
            pr2.User = new GitHubUser();
            pr2.User.Username = "user2";
            _prList.Add(pr2);

            mockGithubClient.Setup(s => s.GetNewPullRequests(mockRunHistory.Object)).Returns(Task.FromResult(_prList));
            
            var mockTwitterClient = new Mock<TwitterHttpClient>("apiKey", "apiKeySecret", "accessToken", "accessTokenSecret"); 
            mockTwitterClient.Setup(s => s.PostTweet("New PR from " + pr1.User.Username + ": " + pr1.Url)).Returns(Task.FromResult("a 200 response"));
            mockTwitterClient.Setup(s => s.PostTweet("New PR from " + pr2.User.Username + ": " + pr2.Url)).Returns(Task.FromResult("a 200 response"));

            UpdateTwitterStatus app = new UpdateTwitterStatus(mockTwitterClient.Object, mockGithubClient.Object, mockRunHistory.Object);
            app.Run().Wait();
            
            mockTwitterClient.Verify(s => s.PostTweet("New PR from " + pr1.User.Username + ": " + pr1.Url), Times.Once());
            mockTwitterClient.Verify(s => s.PostTweet("New PR from " + pr2.User.Username + ": " + pr2.Url), Times.Once());
        }
    }
}