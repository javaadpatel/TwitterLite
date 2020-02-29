using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TwitterLite.Contracts.Services;
using TwitterLite.Web.Models;

namespace TwitterLite.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITweetService _tweetService;
        private readonly IConfiguration _configuration;
        private List<string> twitterFeed = null;

        public HomeController(IUserService userService, ITweetService tweetService, IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _tweetService = tweetService ?? throw new ArgumentNullException(nameof(tweetService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public List<string> FetchTwitterFeed()
        {
            string userFile = _configuration["SourceFiles:UserFile"];
            string tweetFile = _configuration["SourceFiles:TweetFile"];

            var userDictionary = _userService.BuildandRegisterUsers(userFile, true);
            _tweetService.BuildTwitterFeed(tweetFile, userDictionary);

            twitterFeed = _tweetService.RenderAllTwitterFeeds(userDictionary);
            return twitterFeed;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
