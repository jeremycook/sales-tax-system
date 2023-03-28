using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cohub.WebApp.Areas.Search.Controllers
{
    [Area("Search")]
    [Route("search")]
    public class SearchController : Controller
    {
        public SearchController()
        {
        }

        [HttpGet]
        public IActionResult Index(string term)
        {
            term ??= string.Empty;
            term = term.Trim();

            return View(model: term);
        }

        /// <summary>
        /// Open search description endpoint.
        /// See: https://www.chromium.org/tab-to-search
        /// </summary>
        /// <returns></returns>
        [HttpGet("osd.xml")]
        public IActionResult OpenSearchDescription()
        {
            Response.GetTypedHeaders().ContentType = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/opensearchdescription+xml");
            return View();
        }
    }
}
