using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace PostPRsToTwitter
{
    public class TwitterHttpClient
    {
        private readonly HttpClient client;
        private const string requestUrl = "https://api.twitter.com/1.1/statuses/update.json";
        readonly string apiKey, apiKeySecret, accessToken, accessTokenSecret;

        /// <summary>
        /// Constructor takes in Twitter API credentials and creates a new HttpClient();
        /// </summary>
        /// <param name="apiKey"> Twitter API key</param>
        /// <param name="apiKeySecret"> Twitter API key secret</param>
        /// <param name="accessToken"> Twitter API access token</param>
        /// <param name="accessTokenSecret"> Twitter API access token secret</param>
        public TwitterHttpClient(string apiKey, string apiKeySecret, string accessToken, string accessTokenSecret) 
            : this(apiKey, apiKeySecret, accessToken, accessTokenSecret, new HttpClient())
        {
        }

        /// <summary>
        /// Constructor takes in Twitter API credentials and creates a new HttpClient();
        /// </summary>
        /// <param name="apiKey"> Twitter API key </param>
        /// <param name="apiKeySecret"> Twitter API key secret </param>
        /// <param name="accessToken"> Twitter API access token </param>
        /// <param name="accessTokenSecret"> Twitter API access token secret </param>
        /// <param name="client"> Instance of a System.Net.Http.HttpClient </param>
        public TwitterHttpClient(string apiKey, string apiKeySecret, string accessToken, string accessTokenSecret, HttpClient client)
        {
            this.apiKey = apiKey;
            this.apiKeySecret = apiKeySecret;
            this.accessToken = accessToken;
            this.accessTokenSecret = accessTokenSecret;
            this.client = client;
        }

        
        /// <summary>
        /// Post a tweet
        /// Twitter doc: https://developer.twitter.com/en/docs/tweets/post-and-engage/api-reference/post-statuses-update
        /// </summary>
        /// <param name="tweet"> String of the text to be tweeted</param>
        public virtual async Task<string> PostTweet(string tweet)
        {   
            var data = new Dictionary<string, string>();
			
            data.Add("status", tweet );
            // Now add all the OAuth headers we'll need to use when constructing the hash.
            data.Add("oauth_consumer_key", apiKey);
            data.Add("oauth_signature_method", "HMAC-SHA1");
            data.Add("oauth_timestamp", GetTimestamp(DateTime.Now).ToString());
            data.Add("oauth_nonce", Guid.NewGuid().ToString("N")); //random 32 bit alphanumeric string
            data.Add("oauth_token", accessToken);
            data.Add("oauth_version", "1.0");

            // Generate the OAuth signature and add it to our payload.
            data.Add("oauth_signature", GenerateSignature(data));

            // Build the OAuth HTTP Header from the data.
            string oAuthHeader = GenerateOAuthHeader(data);

            // Build the form data (exclude OAuth stuff that's already in the header).
            var formData = new FormUrlEncodedContent(data.Where(item => !item.Key.StartsWith("oauth_")));

            return await SendTweet(oAuthHeader, formData);
	    }

        /// <summary>
        /// Generate an OAuth signature from OAuth header values.
        /// Twitter doc: https://developer.twitter.com/en/docs/basics/authentication/guides/creating-a-signature
        /// </summary>
        /// <param name="data"> Dictionary containing key-value pairs needed to generate the oauth_signature for authentication header </param>
        private string GenerateSignature(Dictionary<string, string> data)
        {
            var sigString = string.Join(
                "&",
                data
                    .Union(data)
                    .Select(item => string.Format("{0}={1}", Uri.EscapeDataString(item.Key), Uri.EscapeDataString(item.Value)))
                    .OrderBy(s => s)
            );

            var fullSigData = string.Format(
                "{0}&{1}&{2}",
                "POST",
                Uri.EscapeDataString(requestUrl),
                Uri.EscapeDataString(sigString.ToString())
            );

            var hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(string.Format("{0}&{1}", apiKeySecret, accessTokenSecret)));

            return Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(fullSigData.ToString())));
        }

        /// <summary>
        /// Generate the raw OAuth HTML header from the values (including signature).
        /// Twitter doc: https://developer.twitter.com/en/docs/basics/authentication/guides/authorizing-a-request.html
        /// </summary>
        /// <param name="data"> Dictionary containing key-value pairs needed to generate the the oauth authentication header </param>
        private string GenerateOAuthHeader(Dictionary<string, string> data)
        {
            return "OAuth " + string.Join(
                ", ",
                data
                    .Where(item => item.Key.StartsWith("oauth_"))
                    .Select(item => string.Format("{0}=\"{1}\"", Uri.EscapeDataString(item.Key), Uri.EscapeDataString(item.Value)))
                    .OrderBy(s => s) //OAuth spec specifices alphabetical order, and Twitter seems to enforce it
            );
        }

        /// <summary>
        /// Send HTTP Request to the Twitter update status api
        /// Twitter doc: https://developer.twitter.com/en/docs/tweets/post-and-engage/api-reference/post-statuses-update
        /// </summary>
        /// <param name="oAuthHeader"> String of the oauth header </param>
        /// <param name="formData"> Url encoded string with Post data.</param>
        /// <returns>
        /// Returns the response body if successful, null otherwise
        /// </returns>
        public virtual async Task<string> SendTweet(string oAuthHeader, FormUrlEncodedContent formData)
        {
            string response = null;
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", oAuthHeader);

            using (var httpResp = await client.PostAsync(requestUrl, formData))
            {
                try
                {
                    httpResp.EnsureSuccessStatusCode();
                    response = await httpResp.Content.ReadAsStringAsync();
                }
                catch(HttpRequestException e)
                {                  
                    Console.Error.WriteLine("\tError posting Tweet: " + e.Message);
                }

            }
            return response;
        }
            
        /// <summary>
        /// Gets the timestamp in seconds since the unix epoc
        /// </summary>
        /// <param name="DateTime"> DateTime for now </param>
        /// <returns>
        /// Number of seconds since unix epoc
        /// </returns>
        private double GetTimestamp(DateTime now) 
        { 
            DateTime epoc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = now.ToUniversalTime() - epoc;
            return Math.Floor(diff.TotalSeconds);
        }

    }
}