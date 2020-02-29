using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TwitterLite.Contracts.Models;
using TwitterLite.Services.Services;

namespace TwitterLite.Services.Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        #region Fields and Constructor
        private readonly UserService _sut;
        private readonly string _basePath = Directory.GetCurrentDirectory() + "\\assets\\user\\";
        private readonly string _userFileSingleLine;
        private readonly string _userFileMultiLine;
        private readonly string _userFileMultiLinePt1;
        private readonly string _userFileMultiLinePt2;

        public UserServiceTests()
        {
            _userFileSingleLine = $"{_basePath}singleuser_singlefollow.txt";
            _userFileMultiLine = $"{_basePath}multiuser_multifollow.txt";

            _userFileMultiLinePt1 = $"{_basePath}multiuser_multifollow_pt1.txt";
            _userFileMultiLinePt2 = $"{_basePath}multiuser_multifollow_pt2.txt";

            _sut = new UserService();
            //set base path
            _sut.Path = _basePath;
        }
        #endregion

        #region Test Setup and Teardown
        /*Runs once per test, before test*/
        [SetUp]
        public void Setup()
        {
        }

        /*Runs once per test, after test*/
        [TearDown]
        public void Teardown()
        {
            //delete files that might have been created
            List<string> files = new List<string> { "user_metadata.txt", "user_checkpoint.txt" };
            foreach (var file in files)
            {
                var filePath = $"{_basePath}{file}";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        #endregion

        #region Helper Functions
        private SortedDictionary<string, User> CreateUsers_SingleUserFollow()
        {
            var users = new SortedDictionary<string, User>
            {
                {
                    "Michael",
                    new User
                    {
                        Name = "Michael",
                        FollowingUserNames = new HashSet<string>{"Kent"}
                    }
                },
                {
                    "Kent",
                    new User
                    {
                        Name = "Kent"
                    }
                }
            };

            return users;
        }

        private SortedDictionary<string, User> CreateMultipleUsers_MultipleFollows()
        {
            var users = new SortedDictionary<string, User>
            {
                {
                    "Michael",
                    new User
                    {
                        Name = "Michael",
                        FollowingUserNames = new HashSet<string>{"Kent", "Veronica", "Vitalik"}
                    }
                },
                {
                    "Kent",
                    new User
                    {
                        Name = "Kent",
                        FollowingUserNames = new HashSet<string>{"Vitalik"}
                    }
                },
                {
                    "Veronica",
                    new User
                    {
                        Name = "Veronica",
                        FollowingUserNames = new HashSet<string>{"Michael","Vitalik"}
                    }
                },
                {
                    "Vitalik",
                    new User
                    {
                        Name = "Vitalik",
                        FollowingUserNames = new HashSet<string>{ "Veronica"}
                    }
                }
            };
            return users;
        }

        private SortedDictionary<string, User> CreateMultipleUsers_Checkpoint()
        {
            var users = CreateMultipleUsers_MultipleFollows();
            users["Michael"].FollowingUserNames.Add("Javaad");
            users.Add(
                "Javaad",
                new User
                {
                    Name = "Javaad",
                    FollowingUserNames = new HashSet<string> { "Kent", "Veronica", "Michael", "Vitalik" }
                }
            );

            return users;
        }
        #endregion


        [Test]
        public void Constructor_Should_Create_Instance()
        {
            var instance = new UserService();
            instance.Should().NotBeNull();
        }

        #region BuildandRegisterUsers Method
        [Test]
        public void BuildandRegisterUsers_SingleLine_Should_Return_ListOfUsers()
        {
            var expectedUsers = CreateUsers_SingleUserFollow();

            var users = _sut.BuildandRegisterUsers(_userFileSingleLine);

            users.Should().NotBeNull();
            users.Should().BeEquivalentTo(expectedUsers, "because the file contains this user and users he follows");
        }

        [Test]
        public void BuildandRegisterUsers_SingleLine_Should_Return_OrderedListOfUsers()
        {
            var expectedUsers = CreateUsers_SingleUserFollow();

            var users = _sut.BuildandRegisterUsers(_userFileSingleLine);

            users.Keys.Should().BeInAscendingOrder("because the dictionary is sorted");
        }

        [Test]
        public void BuildandRegisterUsers_MultiLine_Should_Return_ListOfUsers()
        {
            var expectedUsers = CreateMultipleUsers_MultipleFollows();

            var users = _sut.BuildandRegisterUsers(_userFileMultiLine);

            users.Should().NotBeNull();
            users.Should().BeEquivalentTo(expectedUsers, "because the file contains multiple users who follow other users");
        }

        [Test]
        public void BuildandRegisterUsers_Should_BuildACheckpointFile_When_UseCheckpointEnabled()
        {
            _sut.BuildandRegisterUsers(_userFileSingleLine, useCheckpoint: true);

            bool metadataFileExists = File.Exists($"{_basePath}{_sut._userCheckpointMetadataFileName}");
            bool checkpointFileExists = File.Exists($"{_basePath}{_sut._userCheckpointMetadataFileName}");

            metadataFileExists.Should().BeTrue("because a metadata file should be created when using checkpoints");
            checkpointFileExists.Should().BeTrue("because a checkpoint file should be created when using checkpoints");
        }

          [Test]
        public void BuildandRegisterUsers_Should_NotBuildACheckpointFile_When_UseCheckpointEnabled()
        {
            _sut.BuildandRegisterUsers(_userFileSingleLine, useCheckpoint: false);

            bool metadataFileExists = File.Exists($"{_basePath}{_sut._userCheckpointMetadataFileName}");
            bool checkpointFileExists = File.Exists($"{_basePath}{_sut._userCheckpointMetadataFileName}");

            metadataFileExists.Should().BeFalse("because a metadata file should not be created when using not checkpoints");
            checkpointFileExists.Should().BeFalse("because a checkpoint file should not be becreated when using not checkpoints");
        }

        [Test]
        public void BuildAndRegisterUsers_Should_ContinueFromLastCheckpoint()
        {
            var expectedUsers = CreateMultipleUsers_Checkpoint();

            var users = _sut.BuildandRegisterUsers(_userFileMultiLinePt1, true);
            users = _sut.BuildandRegisterUsers(_userFileMultiLinePt2, true);

            users.Should().NotBeNull("because the files contain users");
            users.Should().BeEquivalentTo(expectedUsers, "because the checkpoint was used as the starting point");
        }

        #endregion

        [Test]
        public void UseCheckpointFile_Should_Return_Tuple_WhenFileExists()
        {
            //create metadata file and checkpoint file
            

            //(bool continueReadingFile, SortedDictionary<string, User> users, double continueFromLine, string fileHash) =
            //    _sut.UseCheckpointFile(_userFileSingleLine);


        }

        #region Checkpoint and Metadata File Methods
        [Test]
        public void CreateMetadataFile_Should_CreateFile()
        {
            string fileHash = "afilehash";
            double lineCounter = 2;
            string metadataFileName = "user_metadata.txt";
            _sut.CreateMetadataFile(_basePath, fileHash, lineCounter);

            bool fileExists = File.Exists($"{_basePath}{metadataFileName}");

            fileExists.Should().BeTrue("Because we created the metadata file");
        }

        [Test]
        public void ParseMetadataFile_Should_Return_UserCheckpointMetadata()
        {
            string fileHash = "afilehash";
            double lineCounter = 2;
            _sut.CreateMetadataFile(_basePath, fileHash, lineCounter);

            var userCheckpointMetadata = _sut.ParseMetadataFile(_basePath);

            userCheckpointMetadata.Should().NotBeNull();
            userCheckpointMetadata.LastReadHash.Should().BeEquivalentTo(fileHash, "because we stored the file hash");
            userCheckpointMetadata.LastReadLine.Should().Be(lineCounter, "because we stored the line counter");
        }

        [Test]
        public void ParseMetadataFile_Should_Return_NullWhenNoFileFound()
        {
            var userCheckpointMetadata = _sut.ParseMetadataFile("randomPath");

            userCheckpointMetadata.Should().BeNull("because the file does not exist");
        }


        [Test]
        public void CreateCheckpointFile_Should_CreateFile()
        {
            string checkpointFileName = "user_checkpoint.txt";
            _sut.CreateCheckpointFile(_basePath, CreateUsers_SingleUserFollow());

            bool fileExists = File.Exists($"{_basePath}{checkpointFileName}");

            fileExists.Should().BeTrue("Because we created the metadata file");
        }

        [Test]
        public void ReadCheckpointFile_Should_Return_UserDictionarySaved()
        {
            _sut.CreateCheckpointFile(_basePath, CreateUsers_SingleUserFollow());
            var users = _sut.ReadCheckpointFile(_basePath);

            users.Should().NotBeNull("because saved users in the checkpoint file");
            users.Should().BeEquivalentTo(CreateUsers_SingleUserFollow(), "because these are the users we stored");
        }

        [Test]
        public void ComputeFileHash_Should_Return_FileHash()
        {
            string fileHash = _sut.ComputeFileHash(_userFileSingleLine);
            fileHash.Should().NotBeNullOrEmpty();
        }
        #endregion

        #region ProcessUser Method
        [Test]
        public void ProcessUser_SingleLine_Should_Return_ListOfUsers()
        {
            var line = "Michael follows Kent";
            var expectedUsers = new Dictionary<string, User>
            {
                {
                    "Michael",
                    new User
                    {
                        Name = "Michael",
                        FollowingUserNames = new HashSet<string>{"Kent"}
                    }
                },
                {
                    "Kent",
                    new User
                    {
                        Name = "Kent"
                    }
                }
            };

            var users = new SortedDictionary<string, User>();
            _sut.ProcessUser(line, users);

            users.Should().NotBeNull();
            users.Should().BeEquivalentTo(expectedUsers, "because the file contains this user and users he follows");
        }
        #endregion

        #region ProcessFollower Method
        [Test]
        public void ProcessFollower_Should_CreateFollowersAndSubscribeUser()
        {
            var michael = new User { Name = "Michael" };
            var followingUserName = "Kent";
            SortedDictionary<string, User> users = new SortedDictionary<string, User> { { michael.Name, michael } };
            _sut.ProcessFollower(followingUserName, michael, users);

            users.Should().ContainKeys(new List<string> { michael.Name, followingUserName });
            users[michael.Name].FollowingUserNames.Should().BeEquivalentTo(new List<string> { followingUserName });
        }

        [Test]
        public void ProcessFollower_Should_SubscribeUserAndAddTweetWhenPublished()
        {
            //create and subscribe
            var michael = new User { Name = "Michael" };
            var followingUserName = "Kent";
            SortedDictionary<string, User> users = new SortedDictionary<string, User> { { michael.Name, michael } };
            _sut.ProcessFollower(followingUserName, michael, users);

            //tweet
            var tweet = new Tweet("Kent", "Free Eth for lyf");
            users[followingUserName].Publisher.SendTweet(tweet);

            //assert tweet added to users' list of tweets
            users[michael.Name].Tweets.Should().BeEquivalentTo(new List<Tweet> { tweet });
            users[followingUserName].Tweets.Should().BeEquivalentTo(new List<Tweet> { tweet });
        }
        #endregion

        #region ProcessUserString Method
        [Test]
        public void ProcessUser_SingleUserSingleFollower_Should_ReturnSingleUserWithFollower()
        {
            string line = "Michael follows Kent";
            (string userName, List<string> followingUserNames) expectedResult = ("Michael", new List<string>{ "Kent"});
            var result = _sut.ProcessUserString(line);

            result.Should().BeEquivalentTo(expectedResult, "since we have one user who is following one user");
        }

        [Test]
        public void ProcessUser_SingleUserMultipleFollower_Should_Process()
        {
            string line = "Michael follows Kent, Veronica";
            (string userName, List<string> followingUserNames) expectedResult = ("Michael", new List<string>{ "Kent", "Veronica"});
            var result = _sut.ProcessUserString(line);

            result.Should().BeEquivalentTo(expectedResult, "since we have one user following multiple users");
        }

        [Test]
        public void ProcessUser_MissingFollow_Should_ThrowException()
        {
            string line = "Michael Kent, Veronica";
            Assert.That(() => _sut.ProcessUserString(line), Throws.Exception.TypeOf<ArgumentException>());
        }
        #endregion

    }
}