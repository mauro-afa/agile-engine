using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallerySearch
{
    public interface IAgileEngine
    {
        void Authenticate();
        AgileEngineImages GetImages();
        AgileEngineImages GetImages(string pageNumber);
        void GetImageDetail(ref AgileEngineImage image);

        void CacheImages(IMemoryCache cache);
    }
}
