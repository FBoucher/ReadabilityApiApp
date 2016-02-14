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

        private BookmarkDetails SearchArticle(string Title, DateTime PublishDate, int Pass)
        {

            var retryFactor = 2 * Pass;
            var fromDate = PublishDate.AddDays(-1 * retryFactor);
            var toDate = PublishDate.AddDays(retryFactor);

            var bookmarks = RealAPI.BookmarkOperations.GetReadingListBookmarksAsync(1, 50, "-date_added", "", fromDate, toDate).Result;

            var result = from b in bookmarks.Bookmarks
                         where b.Article.Title == Title
                         select b as BookmarkDetails;

            if (result.Count() > 0)
            {
                return result.First<BookmarkDetails>();
            }

            if (Pass <= 3)
            {
                return SearchArticle(Title, PublishDate, Pass + 1);
            }

            return null;
        }

        [Route("GetAll")]
        [HttpGet]
        public IEnumerable<BookmarkDetails> GetAll()
        {
            var bookmarks = RealAPI.BookmarkOperations.GetReadingListBookmarksAsync(1, 50, "-date_added", "").Result;
            return bookmarks.Bookmarks;
        }

        /// <summary>
        /// Search an article by the title starting to the date to more recent.
        /// </summary>
        /// <param name="Title">Book or article title.</param>
        /// <param name="PublishDate"></param>
        /// <returns></returns>
        [Route("SearchByTitle")]
        [HttpGet]
        public BookmarkDetails SearchByTitle(string Title, DateTime PublishDate)
        {
            return SearchArticle(Title, PublishDate, 1);
        }

        [Route("ArchiveBookmark")]
        [HttpPost]
        public HttpResponseMessage ArchiveBookmark(int BookmarkId)
        {

            var result = RealAPI.BookmarkOperations.UpdateBookmarkAsync(BookmarkId, false, true, null, null).Result;

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);
            return response;

        }

        [Route("AddBookmark")]
        [HttpPut]
        public HttpResponseMessage AddBookmark(string Url, bool Favorite = false, bool Archived = false)
        {
            var result = RealAPI.BookmarkOperations.AddBookmarkAsync(Url, Favorite, Archived).Result;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);
            return response;
        }

        [Route("DeleteBookmark")]
        [HttpDelete]
        public HttpResponseMessage DeleteBookmark(int BookmarkId)
        {
            var result = RealAPI.BookmarkOperations.DeleteBookmarkAsync(BookmarkId);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);
            return response;
        }

        [Route("GetBookmark")]
        [HttpGet]
        public HttpResponseMessage GetBookmark(int BookmarkId)
        {
            var result = RealAPI.BookmarkOperations.GetBookmark(BookmarkId);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);
            return response;
        }

        [Route("UpdateBookmark")]
        [HttpPost]
        public HttpResponseMessage UpdateBookmark(int BookmarkId, bool Favorite = false, bool Archive = false, double? ReadPercent = 0, DateTime? DateOpened = default(DateTime?))
        {
            var result = RealAPI.BookmarkOperations.UpdateBookmarkAsync(BookmarkId, Favorite, Archive, ReadPercent, DateOpened).Result;

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);
            return response;
        }

    }
}
