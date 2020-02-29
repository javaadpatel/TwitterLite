using System;

namespace TwitterLite.Contracts.Models
{
    public class Tweet : EventArgs
    {
        public string Author { get; private set; }
        public string Message { get; private set; }
        public Tweet(string author, string message)
        {
            if (String.IsNullOrEmpty(author))
                throw new ArgumentNullException(nameof(author));

            if (String.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            Author = author;
            Message = message;
        }
    }
}
