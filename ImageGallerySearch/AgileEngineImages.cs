using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallerySearch
{
    public class AgileEngineImages
    {
        public List<AgileEngineImage> Pictures { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public bool HasMore { get; set; }

        public AgileEngineImages()
        {
            Pictures = new List<AgileEngineImage>();
        }

        public string GetNextPage()
        {
            return (++Page).ToString();
        }
    }
}
