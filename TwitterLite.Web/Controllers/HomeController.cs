using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private readonly IBlobRepository _blobRepository;
        private readonly string _path = Directory.GetCurrentDirectory();// + "//downloadedFiles//";
        private readonly string _userFilePath;
        private readonly string _tweetFilePath;
        private List<string> twitterFeed = null;

        public HomeController(IUserService userService, ITweetService tweetService, IConfiguration configuration, IBlobRepository blobRepository)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _tweetService = tweetService ?? throw new ArgumentNullException(nameof(tweetService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _blobRepository = blobRepository ?? throw new ArgumentNullException(nameof(blobRepository));

            _userFilePath = $"{_path}{Common.Constants.Constants.UserFile}";
            _tweetFilePath = $"{_path}{Common.Constants.Constants.TweetFile}";
        }

        public async Task<IActionResult> Index()
        {
            //download files if they do not exist
            if (!System.IO.File.Exists(_userFilePath))
                await _blobRepository.DownloadBlobAsFileAsync(Common.Constants.Constants.UserFile);

            if (!System.IO.File.Exists(_tweetFilePath))
                await _blobRepository.DownloadBlobAsFileAsync(Common.Constants.Constants.TweetFile);

            return View();
        }

        [HttpGet]
        public List<string> FetchTwitterFeed()
        {
            if(!System.IO.File.Exists(_userFilePath))
                return new List<string> { "Error: user.txt does not exist please upload" };

            if (!System.IO.File.Exists(_tweetFilePath)) 
                return new List<string> { "Error: tweet.txt does not exist please upload" };

            var userDictionary = _userService.BuildandRegisterUsers(_userFilePath, true);
            _tweetService.BuildTwitterFeed(_tweetFilePath, userDictionary);

            twitterFeed = _tweetService.RenderAllTwitterFeeds(userDictionary);
            return twitterFeed;
        }

        [HttpPost("UploadAsset")]
        [RequestSizeLimit(bytes: 10485760)]
        public async Task UploadAsset(List<IFormFile> files, bool appendedFile)
        {
            foreach (var file in files)
            {
                //upload to blob storage
                await _blobRepository.UploadToBlobAsync(file.FileName, file.OpenReadStream());

                //if brand new user.txt file then clear checkpoint data
                if (!appendedFile && file.FileName == Common.Constants.Constants.UserFile)
                    _userService.DeleteCheckpointFiles();

                //save copy locally
                await _blobRepository.DownloadBlobAsFileAsync(file.FileName);
            }
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
