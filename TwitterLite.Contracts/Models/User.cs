using System.Collections.Generic;

namespace TwitterLite.Contracts.Models
{
    /// <summary>A twitter.lite user</summary>
    public class User
    {
        public string Name { get; set; }

        public List<Tweet> Tweets { get; set; } = new List<Tweet>();

        public Publisher Publisher { get; set; } = new Publisher();

        public HashSet<string> FollowingUserNames = new HashSet<string>();

        public void FollowUser(object sender, Tweet tweet)
        {
            try
            {
                Tweets.Add(tweet);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
