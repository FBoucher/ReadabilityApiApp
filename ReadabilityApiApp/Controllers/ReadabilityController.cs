using CSharp.Readability.Api.Interfaces;
using CSharp.Readability.Api.Models;
using CSharp.Readability.Connect;
using Spring.Social.OAuth1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ReadabilityApiApp.Controllers
{
    public class ReadabilityController : ApiController
    {
        // Configure the Callback URL
        readonly IOAuth1ServiceProvider<IReadability> _readabilityProvider = new ReadabilityServiceProvider(ConfigurationManager.AppSettings["ReadabilityApiKey"], ConfigurationManager.AppSettings["ReadabilityApiSecret"]);
        private IReadability _realAPI;

        private IReadability RealAPI
        {
            get
            {
                if (_realAPI == null)
                {
                    _realAPI = _readabilityProvider.GetApi(ConfigurationManager.AppSettings["AccessToken"], ConfigurationManager.AppSettings["Secret"]);
                }
                return _realAPI;
            }
        }

        [Route("GetAll")]
        [HttpGet]
        public IEnumerable<BookmarkDetails> GetAll()
        {
            var bookmarks = RealAPI.BookmarkOperations.GetReadingListBookmarksAsync(1, 50, "-date_added", "").Result;
            return bookmarks.Bookmarks;
        }
    }
}
