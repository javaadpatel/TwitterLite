using FluentAssertions;
using Moq;
using NUnit.Framework;
using TwitterLite.Contracts.Services;
using TwitterLite.Services.Services;
using System.Collections.Generic;
using TwitterLite.Contracts.Models;
using System;
using System.IO;
using System.Linq;

namespace TwitterLite.Services.Tests
{
    [TestFixture]
    public class TweetServiceTests
    {
        #region Fields and Constructor
        private readonly string _tweetFileSingleLine;
        private readonly string _tweetFileMultiLine;
        private readonly TweetService _sut;
        private readonly string _basePath = Directory.GetCurrentDirectory() + "\\assets\\tweets\\";
        private Mock<IUserService> _userService;

        public TweetServiceTests()
        {
            _userService = new Mock<IUserService>().SetupAllProperties();
            _sut = new TweetService();
            _tweetFileSingleLine = $"{_basePath}singletweet.txt";
            _tweetFileMultiLine = $"{_basePath}multipletweets.txt";
        }
        #endregion

        #region Test Setup

        [SetUp]
        public void Setup()
        {
        }
        #endregion

        #region Helper Methods
        private SortedDictionary<string, User> _BuildUsersForSingleTweet()
        {
            var vitalik = new User { Name = "Vitalik" };
            var kent = new User { Name = "Kent" };
            var users = new SortedDictionary<string, User>
            {
                {
                    vitalik.Name,
                    vitalik
                },
                {
                    kent.Name,
                    kent
                }
            };
            //setup kent to follow Vitalik
            users[vitalik.Name].Publisher.OnChange += users[kent.Name].FollowUser;
            //setup Vitalik to follow Vitalik
            users[vitalik.Name].Publisher.OnChange += users[vitalik.Name].FollowUser;

            return users;
        }

        private SortedDictionary<string, User> _BuildUsersForMultipleTweets()
        {
            var vitalik = new User { Name = "Vitalik" };
            var michael = new User { Name = "Michael" };
            var kent = new User { Name = "Kent" };
            var veronica = new User { Name = "Veronica" };
            var users = new SortedDictionary<string, User>
            {
                {
                    vitalik.Name,
                    vitalik
                },
                {
                    michael.Name,
                    michael
                },
                {
                    kent.Name,
                    kent
                },
                {
                    veronica.Name,
                    veronica
                }
            };
            //register users
            users[vitalik.Name].Publisher.OnChange += users[kent.Name].FollowUser;
            users[vitalik.Name].Publisher.OnChange += users[michael.Name].FollowUser;
            users[vitalik.Name].Publisher.OnChange += users[veronica.Name].FollowUser;
            users[vitalik.Name].Publisher.OnChange += users[vitalik.Name].FollowUser;

            users[michael.Name].Publisher.OnChange += users[michael.Name].FollowUser;
            users[michael.Name].Publisher.OnChange += users[veronica.Name].FollowUser;

            users[kent.Name].Publisher.OnChange += users[kent.Name].FollowUser;
            users[kent.Name].Publisher.OnChange += users[michael.Name].FollowUser;

            users[veronica.Name].Publisher.OnChange += users[veronica.Name].FollowUser;
            users[veronica.Name].Publisher.OnChange += users[michael.Name].FollowUser;
            users[veronica.Name].Publisher.OnChange += users[vitalik.Name].FollowUser;

           

            return users;
        }

        private Dictionary<string, List<Tweet>> UserTweets()
        {
            Dictionary<string, List<Tweet>> userTweets = new Dictionary<string, List<Tweet>>();
            var vitalik = new User { Name = "Vitalik" };
            var michael = new User { Name = "Michael" };
            var kent = new User { Name = "Kent" };
            var veronica = new User { Name = "Veronica" };

            //add tweets
            List<Tweet> tweets = new List<Tweet>
            {
                new Tweet(vitalik.Name, "ETH is the future" ),
                new Tweet(michael.Name, "There are only two hard things in Computer Science: cache invalidation, naming things and off-by-1 errors."),
                new Tweet(kent.Name, "For it should never feel the high complexity of the future of preference"),
                new Tweet(veronica.Name, "Worried about shifting your focus from now another sister will surely become to have bugs, only!"),
                new Tweet(michael.Name, "If your linter tortures you, its complexity and we're building?")
            };

            userTweets.Add(vitalik.Name, new List<Tweet> { tweets[0], tweets[3] });
            userTweets.Add(michael.Name, new List<Tweet> { tweets[0], tweets[1], tweets[2], tweets[3], tweets[4] });
            userTweets.Add(kent.Name ,new List<Tweet> { tweets[0], tweets[2] });
            userTweets.Add(veronica.Name, new List<Tweet> { tweets[0], tweets[1], tweets[3], tweets[4] });

            return userTweets;
        }
        #endregion

        [Test]
        public void Constructor_Should_Create_Instance()
        {
            var instance = new TweetService();
            instance.Should().NotBeNull();
        }

        #region BuildTwitterFeed Method
        [Test]
        public void BuildTwitterFeed_SingleTweet_Should_Return_UsersWithTweets()
        {
            var users = _BuildUsersForSingleTweet();
            var tweetMessage = "ETH is the future";

            //build twitter feed
            _sut.BuildTwitterFeed(_tweetFileSingleLine, users);

            //assert that tweet was added to both users
            foreach (var userKey in users.Keys)
            {
                users[userKey].Tweets.Should().NotBeEmpty();
                users[userKey].Tweets.First().Message.Should().BeEquivalentTo(tweetMessage, "because this was what the user tweeted");
            }
        }

        [Test]
        public void BuildTwitterFeed_MultipleTweets_Should_Return_UsersWithTweets()
        {
            var users = _BuildUsersForMultipleTweets();
            var expectedTweets = UserTweets();

            //build twitter feed
            _sut.BuildTwitterFeed(_tweetFileMultiLine, users);

            //assert that tweet was added to both users
            foreach (var userKey in users.Keys)
            {
                users[userKey].Tweets.Should().NotBeEmpty();
                users[userKey].Tweets.Should().BeEquivalentTo(expectedTweets[userKey]);
            }
        }
        #endregion

        #region RenderTwitterFeed Method
        [Test]
        public void RenderTwitterFeed_SingleTweet_Should_CreateAFeed()
        {
            var user = new User
            {
                Name = "Vitalik",
                Tweets = new List<Tweet> 
                { 
                    new Tweet ("Vitalik", "ETH is the future")
                }
            };

            List<string> expectedFeed = new List<string> { "Vitalik", $"\t @Vitalik: ETH is the future" };
            var twitterFeed = _sut.RenderTwitterFeed(user);

            twitterFeed.Should().BeEquivalentTo(expectedFeed, "because the rendered feed includes one tweet by the author");
        }
        #endregion

        #region ProcessTweet Method
        [Test]
        public void ProcessTweet_Should_Return_Tweet()
        {
            var line = "Vitalik> Things like tornado cash and uniswap, kyber";
            var expectedTweet = new Tweet("Vitalik", "Things like tornado cash and uniswap, kyber");
            var tweet = _sut.ProcessTweet(line);

            tweet.Should().BeEquivalentTo(expectedTweet, "because the line contains the author and message");
        }

        [Test]
        public void ProcessTweet_MissingTweetCharacter_Should_ThrowException()
        {
            var line = "Vitalik Things like tornado cash and uniswap, kyber";
            Assert.That(() => _sut.ProcessTweet(line), Throws.Exception.TypeOf<ArgumentException>());
        }
        #endregion

    }
}