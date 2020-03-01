using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TwitterLite.Contracts.Models;
using TwitterLite.Contracts.Services;

namespace TwitterLite.Services.Services
{
    public class UserService : IUserService
    {
        #region Constructor and Fields
        private readonly string _follows = "follows";
        public readonly string _userCheckpointFileName = "user_checkpoint.txt";
        public readonly string _userCheckpointMetadataFileName = "user_metadata.txt";

        private string _path = Directory.GetCurrentDirectory();// + "\\downloadedFiles\\";

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
            }
        }

        public UserService()
        {
        }
        #endregion

        public SortedDictionary<string, User> BuildandRegisterUsers(string fileName, bool useCheckpoint = false)
        {
            if (!File.Exists(fileName))
                throw new FileLoadException($"The file {fileName} was not found");

            SortedDictionary<string, User> users = new SortedDictionary<string, User>();
            double lineCounter = 0;
            bool isFileReadSuccess = false;

            string fileHash = null; bool continueReadingFile; double continueFromLine = 0;
            if (useCheckpoint)
            {
                (continueReadingFile, users, continueFromLine, fileHash) = UseCheckpointFile(fileName);

                if (!continueReadingFile)
                    return users;
            }

            try
            {
                using (var fileStream = File.OpenRead(fileName))
                using (var streamReader = new StreamReader(fileStream)) //automatically detects file encoding
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        //skip lines if we've used the checkpoint file
                        if (lineCounter < continueFromLine)
                        {
                            lineCounter++;
                            continue;
                        }

                        ProcessUser(line, users);

                        lineCounter++;
                    }
                }

                isFileReadSuccess = true;
                return users;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //only create a checkpoint and metadata file if the read was successful and checkpoints are enabled
                if (isFileReadSuccess && useCheckpoint)
                {
                    //create a summary snapshot of the file for faster processing
                    CreateMetadataFile(Path, fileHash, lineCounter);
                    CreateCheckpointFile(Path, users);
                }
            }
        }

        public 
            (bool continueReadingFile, SortedDictionary<string, User> users, double continueFromLine, string fileHash) 
            UseCheckpointFile(string fileName)
        {
            //compute file hash
            string fileHash = ComputeFileHash(fileName);
            SortedDictionary<string, User> users = new SortedDictionary<string, User>();

            //parse metadata file
            var userCheckpoint = ParseMetadataFile(Path);
            double continueFromLine = 0;
            bool continueReadingFile = true;

            if (userCheckpoint != null)
            {
                continueFromLine = userCheckpoint.LastReadLine;
                users = ReadCheckpointFile(Path);
                continueReadingFile = userCheckpoint?.LastReadHash != fileHash;
            }

            return (continueReadingFile, users, continueFromLine, fileHash);
        }

        #region Checkpoint and Metadata File Methods

        /// <summary>Creates the metadata file with a default name of user_metadata.txt</summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileHash">The file hash.</param>
        /// <param name="lineCounter">The line counter.</param>
        /// <returns></returns>
        public bool CreateMetadataFile(string filePath, string fileHash, double lineCounter)
        {
            string metadaFilePath = $"{filePath}/{_userCheckpointMetadataFileName}";

            try
            {
                using (var fileStream = File.Create(metadaFilePath))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    var userCheckpointModel = new UserCheckpointMetadata { LastReadHash = fileHash, LastReadLine = lineCounter };
                    streamWriter.WriteLine(JsonConvert.SerializeObject(userCheckpointModel));
                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        /// <summary>Reads the checkpoint file.</summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public SortedDictionary<string, User> ReadCheckpointFile(string filePath)
        {
            try
            {
                SortedDictionary<string, User> users = new SortedDictionary<string, User>();
                //Todo extreact the stream reader into a function that returns a streamreader
                using (var fileStream = File.OpenRead($"{filePath}/{_userCheckpointFileName}"))
                using (var streamReader = new StreamReader(fileStream)) //automatically detects file encoding
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        ProcessUser(line, users);
                    }
                }
                return users;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Parses the metadata file.</summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public UserCheckpointMetadata ParseMetadataFile(string filePath)
        {
            string metadaFilePath = $"{filePath}/{_userCheckpointMetadataFileName}";

            try
            {
                //check if the file exists
                if (!File.Exists(metadaFilePath))
                    return null;

                using (var fileStream = File.OpenRead(metadaFilePath))
                using (var streamReader = new StreamReader(fileStream))
                {
                    String line = streamReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<UserCheckpointMetadata>(line);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>Creates the checkpoint file.</summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        public bool CreateCheckpointFile(string filePath, SortedDictionary<string, User> users)
        {
            string checkPointFilePath = $"{filePath}/{_userCheckpointFileName}";
            try
            {
                using (var fileStream = File.Create(checkPointFilePath))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    foreach (var user in users.Values)
                    {
                        if (!user.FollowingUserNames.Any())
                            continue;

                        string formattedLine = $"{user.Name} {_follows} {String.Join(",", user.FollowingUserNames)}";
                        streamWriter.WriteLine(formattedLine);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        /// <summary>Deletes the checkpoint files.</summary>
        public void DeleteCheckpointFiles()
        {
            string checkPointFilePath = $"{Path}/{_userCheckpointFileName}";
            string metadataFilePath = $"{Path}/{_userCheckpointMetadataFileName}";
            List<string> files = new List<string> { checkPointFilePath, metadataFilePath };

            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        public string ComputeFileHash(string fileName)
        {
            using (var fileStream = File.OpenRead(fileName))
            {
                var hash = (SHA256CryptoServiceProvider.Create()).ComputeHash(fileStream);
                var hashString = Convert.ToBase64String(hash);
                return hashString;
            }
        }
        #endregion

        /// <summary>Processes the user.</summary>
        /// <param name="line">The line of the file currently being read</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Could not process user, line does not contain {_follows} keyword. " +
        ///                  $"Line with error: {line}</exception>
        public (string userName, List<string> followingUserNames) ProcessUserString(string line)
        {
            if (!line.Contains(_follows))
                throw new ArgumentException($"Could not process user, line does not contain {_follows} keyword. " +
                    $"Line with error: {line}");

            //split the line at the _follows keyword,
            //producing a string array with [0] = userName & [1] = followingUsers
            var lineParts = line.Split(_follows);

            var userName = lineParts[0].Trim();

            //remove any duplicates in the list and whitespace
            var followingUsers = lineParts[1].Split(",").Distinct().Select(x => x.Trim()).ToList();

            return (userName, followingUsers);
        }

        /// <summary>Processes the user.</summary>
        /// <param name="line">The line.</param>
        /// <param name="users">The users.</param>
        public void ProcessUser(string line, SortedDictionary<string, User> users)
        {
            (var userName, var followingUsers) = ProcessUserString(line);

            //create users if they don't exist
            User tempUser = null;
            if (!users.TryGetValue(userName, out tempUser))
            {
                tempUser = new User { Name = userName };
                tempUser.Publisher.OnChange += tempUser.FollowUser;
                users.Add(tempUser.Name, tempUser);
            }

            //loop through users they are following
            foreach (var followingUserName in followingUsers)
            {
                ProcessFollower(followingUserName, tempUser, users);
            }
        }

        /// <summary>Processes the follower.</summary>
        /// <param name="followingUserName">Name of the following user.</param>
        /// <param name="temp">The temporary.</param>
        /// <param name="users">The users.</param>
        public void ProcessFollower(string followingUserName, User temp, SortedDictionary<string, User> users)
        {
            //if the user is not currently following this user
            User tempFollowingUser = null;
            if (!temp.FollowingUserNames.Contains(followingUserName))
            {
                //if this user does not exist yet, create the user
                if (!users.TryGetValue(followingUserName, out tempFollowingUser))
                {
                    tempFollowingUser = new User { Name = followingUserName };
                    //subscribe this user to themselves
                    tempFollowingUser.Publisher.OnChange += tempFollowingUser.FollowUser;
                    users.Add(followingUserName, tempFollowingUser);
                }

                //update list of users that user is following
                temp.FollowingUserNames.Add(followingUserName);

                //subscribe to the user
                tempFollowingUser.Publisher.OnChange += temp.FollowUser;
            }
        }
    }
}
