using System;
using System.Collections.Generic;
using System.IO;
using TwitterLite.Contracts.Models;
using TwitterLite.Contracts.Services;

namespace TwitterLite.Services.Services
{
    public class TweetService : ITweetService
    {
        private readonly string _tweetStartCharacter = ">";

        /// <summary>Processes the tweet file and sends all tweets</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="users"></param>
        /// <exception cref="FileLoadException">The file {fileName} was not found</exception>
        public void BuildTwitterFeed(string fileName, SortedDictionary<string, User> users)
        {
            if (!File.Exists(fileName))
                throw new FileLoadException($"The file {fileName} was not found");

            using (var fileStream = File.OpenRead(fileName))
            using (var streamReader = new StreamReader(fileStream)) //automatically detects file encoding
            {
                String tweetString;
                while ((tweetString = streamReader.ReadLine()) != null)
                {
                    //process tweet
                    var tweet = ProcessTweet(tweetString);

                    //pull user
                    User tweetUser = null;
                    if (!users.TryGetValue(tweet.Author, out tweetUser))
                    {
                        //user does not exist, must be a mistake with the file. ignore this tweet
                        //could possible throw an exception here or create the user
                        continue;
                    }

                    tweetUser.Publisher.SendTweet(tweet);
                }
            }
        }

        #region Tweet Timeline Rendering


        /// <summary>Renders all twitter feeds.</summary>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        public List<string> RenderAllTwitterFeeds(SortedDictionary<string, User> users)
        {
            List<string> twitterFeed = new List<string>();
            foreach (var user in users.Keys)
            {
                twitterFeed.AddRange(RenderTwitterFeed(users[user]));
            }

            return twitterFeed;
        }

        /// <summary>Renders the twitter feed for a user</summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public List<string> RenderTwitterFeed(User user)
        {
            List<string> renderedFeed = new List<string>();
            //add user
            renderedFeed.Add(user.Name);

            //add tweets
            foreach (var tweet in user.Tweets)
            {
                renderedFeed.Add($"\t @{tweet.Author}: {tweet.Message}");
            }

            return renderedFeed;
        }
        #endregion

        /// <summary>Processes the tweet.</summary>
        /// <param name="line">The line of the file currently being read</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Could not process user, line does not contain {_tweetStartCharacter} keyword. " +
        ///                 $"Line with error: {line}</exception>
        public Tweet ProcessTweet(string line)
        {
            if (!line.Contains(_tweetStartCharacter))
                throw new ArgumentException($"Could not process user, line does not contain {_tweetStartCharacter} keyword. " +
                   $"Line with error: {line}");

            //split the line at the _follows keyword,
            //producing a string array with [0] = userName & [1] = tweet
            var lineParts = line.Split(_tweetStartCharacter);

            return new Tweet(lineParts[0].Trim(), lineParts[1].Trim());
        }
    }
}
