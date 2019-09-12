using System.Runtime.Serialization;

namespace PostPRsToTwitter
{
    [DataContract(Name="pull")]
    public class PullRequest
    {
        [DataMember(Name="number")]
        public string Number { get; set; }

        [DataMember(Name="html_url")]
        public string Url { get; set; }

        [DataMember(Name="user")]
        public GitHubUser User { get; set; }
    }

    [DataContract(Name="user")]
    public class GitHubUser
    {
        [DataMember(Name="login")]
        public string Username { get; set; }
    }

}