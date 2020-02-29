using System;

namespace TwitterLite.Contracts.Models
{
    public class Publisher
    {
        public event EventHandler<Tweet> OnChange = delegate { };

        public void SendTweet(Tweet tweet)
        {
            OnChange(this, tweet);
        }
    }
}
