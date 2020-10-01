using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace ImageGallerySearch
{
    public sealed class AgileEngineWrapper : IAgileEngine
    {
        private AgileEngineAuth _auth = null;
        private readonly Uri _baseAddress = new Uri("http://interview.agileengine.com");
        private static readonly Lazy<IAgileEngine> _instance = new Lazy<IAgileEngine>(() => new AgileEngineWrapper());

        private const string IMAGES_ENDPOINT = "images";

        public AgileEngineWrapper()
        {
            Authenticate();
        }

        public static IAgileEngine Instance { get => _instance.Value; }

        public void GetImageDetail(ref AgileEngineImage image)
        {
            if (_auth == null) throw new Exception("Need to be authenticated first!");

            using (var client = new HttpClient())
            {
                client.BaseAddress = _baseAddress;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _auth.Token);

                var imagesTask = client.GetAsync($"{IMAGES_ENDPOINT}/{image.Id}");
                imagesTask.Wait();

                var imagesTaskResult = imagesTask.Result;

                if (imagesTaskResult.IsSuccessStatusCode)
                {
                    var readImages = imagesTaskResult.Content.ReadAsStringAsync();
                    readImages.Wait();

                    var newImage = JsonConvert.DeserializeObject<AgileEngineImage>(readImages.Result);

                    image.Author = newImage.Author;
                    image.Camera = newImage.Camera;
                    image.Full_Picture = newImage.Full_Picture;
                }
            }
        }

        public AgileEngineImages GetImages()
        {
            if (_auth == null) throw new Exception("Need to be authenticated first!");

            AgileEngineImages images = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = _baseAddress;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _auth.Token);

                var imagesTask = client.GetAsync(IMAGES_ENDPOINT);
                imagesTask.Wait();

                var imagesTaskResult = imagesTask.Result;

                if(imagesTaskResult.IsSuccessStatusCode)
                {
                    var readImages = imagesTaskResult.Content.ReadAsStringAsync();
                    readImages.Wait();

                    images = JsonConvert.DeserializeObject<AgileEngineImages>(readImages.Result);
                }
            }

            return images;
        }

        public AgileEngineImages GetImages(string pageNumber)
        {
            if (_auth == null) throw new Exception("Need to be authenticated first!");
            //if (pageNumber < 1) throw new ArgumentException("Number should be more than 0");

            AgileEngineImages images = null;

            using (var client = new HttpClient())
            {
                var uriBuilder = new UriBuilder($"{_baseAddress}{IMAGES_ENDPOINT}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["page"] = pageNumber;

                uriBuilder.Query = query.ToString();

                //client.BaseAddress = _baseAddress;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _auth.Token);

                var imagesTask = client.GetAsync(uriBuilder.ToString());
                imagesTask.Wait();

                var imagesTaskResult = imagesTask.Result;

                if (imagesTaskResult.IsSuccessStatusCode)
                {
                    var readImages = imagesTaskResult.Content.ReadAsStringAsync();
                    readImages.Wait();

                    images = JsonConvert.DeserializeObject<AgileEngineImages>(readImages.Result);
                }
            }

            return images;
        }

        public void Authenticate()
        {
            _auth = null;

            using (var client = new HttpClient())
            {
                var url = "http://interview.agileengine.com/auth";
                var data = new StringContent("{ \"apiKey\": \"23567b218376f79d9415\" }", Encoding.UTF8, "application/json");

                var task = client.PostAsync(url, data);
                task.Wait();

                var taskResponse = task.Result;

                if (taskResponse.IsSuccessStatusCode)
                {
                    var content = taskResponse.Content.ReadAsStringAsync();
                    content.Wait();

                    _auth = JsonConvert.DeserializeObject<AgileEngineAuth>(content.Result);
                }
            }
        }

        public void CacheImages(IMemoryCache cache)
        {
            Dictionary<int, AgileEngineImages> imagePages = new Dictionary<int, AgileEngineImages>();
            //First we do the default load.
            var firstPage = GetImages();

            //Then we recursively run LoadImages until the images run out basically...
            LoadImages(firstPage.GetNextPage(), ref imagePages);
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15));
            cache.Set(Constants.CACHE_IMAGES, imagePages, cacheEntryOptions);
        }

        private void LoadImages(string pageNumber, ref Dictionary<int, AgileEngineImages> dictionary)
        {
            try
            {
                var subsequentImages = GetImages(pageNumber);

                subsequentImages.Pictures.ForEach(single =>
                {
                    GetImageDetail(ref single);
                });

                dictionary.Add(subsequentImages.Page, subsequentImages);

                if(subsequentImages.HasMore)
                {
                    LoadImages(subsequentImages.GetNextPage(), ref dictionary);
                }
            }
            catch(Exception ex)
            {
                //No need to stop, we can just move on...
            }
        }
    }
}
