using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;


namespace PostPRsToTwitter
{
    public class RunHistory
    {
        private const string runHistoryFilename = "RunHistory.xml";
        public DateTime LastRun { get; set; }
        public virtual HashSet<string> PostedTweets { get; set; }
       
        /// <summary>
        /// Default constructor
        /// </summary>
        public RunHistory() : this (null)
        {
        }

        /// <summary>
        /// Constructor loads RunHistory.xml from disk and populates the PostedTweets instance variable
        /// </summary>
        /// <param name="document"> XDocument containing run history. If null, read it from disk. </param>
        public RunHistory(XDocument document)
        {
            XDocument runHistoryDocument;
            //initialize LastRun to be unix epoc
            LastRun = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            PostedTweets = new HashSet<string>();

            try 
            {
                if(document == null) {
                    runHistoryDocument = LoadRunHistoryDocument();
                } else {
                    runHistoryDocument = document;
                }
                
                foreach (XElement pr in runHistoryDocument.Descendants("PullRequest"))
                {
                    PostedTweets.Add(pr.Value);
                }

                LastRun = DateTimeOffset.Parse(runHistoryDocument.Element("PullRequests").Attribute("lastrun").Value).UtcDateTime;
            }
            catch(Exception e) when (e is FileNotFoundException || e is FormatException || e is NullReferenceException || e is XmlException)
            {
                //couldn't read from file, meaning it was either malformed or didn't exist
                //logging to stdout and moving on
                Console.WriteLine("No previous runs detected. Tweeting all open pull requests.");
            }
        }

        /// <summary>
        /// Loads RunHistory.xml from disk and returns it as an XDocument
        /// </summary>
        /// <returns> Returns an XDocument reflecting contents of RunHistory.xml </returns>
        private XDocument LoadRunHistoryDocument()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var runHistoryFilepath = Path.Combine(currentDirectory, runHistoryFilename);
            var runHistory = XDocument.Load(runHistoryFilepath);

            return  runHistory;
        }

        /// <summary>
        /// Merges list of previously-tweeted PRs with Set of newly-tweeted PRs and persists the combined history
        /// </summary>
        /// <param name="datetime"> Current DateTime </param>
        /// <param name="toSave"> Set of PRs tweeted on this run to be persisted </param>
        public virtual void UpdateRunHistory(DateTime datetime, HashSet<string> added)
        {
            //merge the PRs we read from the file earlier with new PRs we got from github on this run
            PostedTweets.UnionWith(added);
            SaveRunHistory(datetime, PostedTweets);
        }

        /// <summary>
        /// Persists the new run history to disk
        /// </summary>
        /// <param name="datetime"> Current DateTime </param>
        /// <param name="toSave"> Set of PRs tweeted on this run to be persisted </param>
        private void SaveRunHistory(DateTime datetime, HashSet<string> toSave)
        {
            //create xml element to write
            XElement root = new XElement("PullRequests");
            root.SetAttributeValue("lastrun", datetime);

            foreach(var pr in toSave)
            {
                root.Add(new XElement("PullRequest", pr));
            }

            XDocument doc = new XDocument();
            doc.Add(root);

            var currentDirectory = Directory.GetCurrentDirectory();
            var runHistory = Path.Combine(currentDirectory, runHistoryFilename);

            try{
                //save to disk
                //if file already exists, overwrite it
                doc.Save(runHistory);
            }
            catch(Exception e) when (e is DirectoryNotFoundException)
            {
                Console.Error.WriteLine("Could not save updated Run Histor: " + e.Message);
            }
        }

    }

}