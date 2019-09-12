using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace PostPRsToTwitter
{
    public class GitHubHttpClient
    {
        private readonly HttpClient client;
        private const string requestUrl = "https://api.github.com/repos/KyleBumpus/dummy-repo/pulls?per_page=100";

        /// <summary>
        /// Default Constructor,
        /// </summary>
        public GitHubHttpClient() : this(new HttpClient())
        {
        }

        /// <summary>
        /// Constructor sets rest client
        /// </summary>
        /// <param name="httpClient"> Instance of System.Net.Http.HttpClient </param>
        public GitHubHttpClient(HttpClient httpClient)
        {
            this.client = httpClient;
        }

        /// <summary>
        /// Gets list of open pull requests from Github API
        /// </summary>
        /// <returns> Returns a List of PullRequests </returns>
        private async Task<List<PullRequest>> GetPullRequests()
        {
            List<PullRequest> pullRequests = new List<PullRequest>();
            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", "C# HttpClient");
            
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    pullRequests = JsonConvert.DeserializeObject<List<PullRequest>>(content);
                }
                catch(HttpRequestException e)
                {
                    Console.Error.WriteLine("Error connecting to Github API:\n" + e.ToString());
                }
                
                return pullRequests;
            }
        }

        /// <summary>
        /// Gets a list of open pull requests via GetPullRequests() method and dedups against PRs already
        /// tweeted as stored in the RunHistory
        /// </summary>
        /// <param name="runHistory"> Instance of RunHistory containing a list of previously-tweeted PRs</param>
        /// <returns> List of PullRequests that haven't already been tweeted </returns>
        public virtual async Task<List<PullRequest>> GetNewPullRequests(RunHistory runHistory)
        {
            var newPRs = new List<PullRequest>();  
            var pullRequests = await GetPullRequests();

            foreach(var pr in pullRequests)
            {
                if(!runHistory.PostedTweets.Contains(pr.Number))
                {
                    newPRs.Add(pr);
                }
            }

            return newPRs;
        }
        
    }
}
