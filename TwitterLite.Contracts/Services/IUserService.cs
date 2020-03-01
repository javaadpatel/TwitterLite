using System.Collections.Generic;
using TwitterLite.Contracts.Models;

namespace TwitterLite.Contracts.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Processes the user.txt file and returns a sorted dictionary with the key of user name and value of User model
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="useCheckpoint">Flag to set whether a checkpoint file and metadata should be created</param>
        /// <returns></returns>
        SortedDictionary<string, User> BuildandRegisterUsers(string fileName, bool useCheckpoint = false);


        /// <summary>Deletes the checkpoint files.</summary>
        void DeleteCheckpointFiles();
    }
}
