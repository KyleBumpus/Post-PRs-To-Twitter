using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostPRsToTwitter
{
    public class UpdateTwitterStatus
    {
        private TwitterHttpClient twitterClient; 
        private  GitHubHttpClient gitHubHttpClient;
        public RunHistory RunHistory { get; set; }

        /// <summary>
        /// Constructor sets API clients and run history
        /// </summary>
        /// <param name="twitterClient"> Twitter API client </param>
        /// <param name="gitHubClient"> Github API client </param>
        /// <param name="runHistory"> The run history of tweets posted </param>
        public UpdateTwitterStatus(TwitterHttpClient twitterClient, GitHubHttpClient gitHubClient, RunHistory runHistory)
        {
            this.twitterClient = twitterClient;
            this.gitHubHttpClient = gitHubClient;
            this.RunHistory = runHistory;
        }

        /// <summary>
        /// Run the program. First get new PRs from the GitHub API, dedup against PRs we've already tweeted,
        /// and tweet out the rest. Then persist the updated state in the RunHistory.
        /// </summary>
        public async Task Run()
        {
            var pullRequests = await gitHubHttpClient.GetNewPullRequests(RunHistory);
            var prsTweeted = new HashSet<string>();

            foreach (var pr in pullRequests)
            {
                var response = await twitterClient.PostTweet("New PR from " + pr.User.Username + ": " + pr.Url);
                
                if(response != null)
                    prsTweeted.Add(pr.Number);
            }

            //We only want to update the LastRun datetime if there were new PRs to tweet this time.
            if(prsTweeted.Count > 0) 
                RunHistory.UpdateRunHistory(DateTime.Now, prsTweeted); 
        }

        static void Main(string[] args)
        {
            //see README.md for info on how to set secrets on your system
            var apiKey = Environment.GetEnvironmentVariable("TwitterApiKey");
            var apiKeySecret = Environment.GetEnvironmentVariable("TwitterApiKeySecret");
            var accessToken = Environment.GetEnvironmentVariable("TwitterAccessToken");
            var accessTokenSecret = Environment.GetEnvironmentVariable("TwitterAccessTokenSecret");

            if(apiKey != null && apiKeySecret != null && accessToken != null && accessTokenSecret != null) {
                var twitterClient = new TwitterHttpClient(apiKey, apiKeySecret, accessToken, accessTokenSecret);
                var gitHubHttpClient = new GitHubHttpClient();
                var runHistory = new RunHistory();

                var app = new UpdateTwitterStatus(twitterClient, gitHubHttpClient, runHistory);
                app.Run().Wait();
            } else {
                Console.Error.WriteLine("Could not read Twitter secrets from environment variables. Aborting.");
            }
        }
    }

}

