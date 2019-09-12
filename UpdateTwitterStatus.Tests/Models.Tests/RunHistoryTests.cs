using NUnit.Framework;
using PostPRsToTwitter;
using System;
using System.Xml.Linq;

namespace Tests
{
    [TestFixture]
    public class RunHistoryTests
    {
        [Test]
        public void Test_RunHistory_Constructor_PostedTweetsShouldContain3Items()
        {
            DateTime dt = DateTime.UtcNow;
            XElement root = new XElement("PullRequests");
            root.SetAttributeValue("lastrun", dt);
            root.Add(new XElement("PullRequest", "1"));
            root.Add(new XElement("PullRequest", "2"));
            root.Add(new XElement("PullRequest", "3"));

            XDocument doc = new XDocument();
            doc.Add(root);

            var runHistory = new RunHistory(doc);

            Assert.AreEqual(3, runHistory.PostedTweets.Count);
            Assert.AreEqual(dt, runHistory.LastRun);
        }

    }
}