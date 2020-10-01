using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ImageGallerySearch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private IMemoryCache _cache;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<SearchController> _logger;

        public SearchController(ILogger<SearchController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet("{SearchTerm}")]
        public IEnumerable<AgileEngineImage> Get(string SearchTerm)
        {
            var imageditc = _cache.Get(Constants.CACHE_IMAGES) as Dictionary<int, AgileEngineImages>;
            var finalList = new List<AgileEngineImage>();

            foreach(KeyValuePair<int, AgileEngineImages> entry in imageditc)
            {
                finalList.AddRange(entry.Value.Pictures.Select(x => x)
                    .Where(single =>
                    (single.Author ?? "").Contains(SearchTerm) ||
                    (single.Camera ?? "").Contains(SearchTerm) ||
                    (single.Full_Picture ?? "").Contains(SearchTerm) ||
                    (single.Id ?? "").Contains(SearchTerm) ||
                    (single.Cropped_Picture ?? "").Contains(SearchTerm)).ToList());
            }
            return finalList.ToArray();
        }
    }
}
