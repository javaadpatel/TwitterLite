using System.Collections.Generic;
using TwitterLite.Contracts.Models;

namespace TwitterLite.Contracts.Services
{
    public interface ITweetService
    {
        /// <summary>Processes file and returns a list a list of users</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        void BuildTwitterFeed(string fileName, SortedDictionary<string, User> users);

        List<string> RenderAllTwitterFeeds(SortedDictionary<string, User> users);
    }
}
