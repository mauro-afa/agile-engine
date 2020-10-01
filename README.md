# agile-engine
To run just execute ImageGallerySearch.sln and run on any build type. It will by default open up the browser with the address search/Nikon as parameter that returns some result

# Missing things
First of all... This was my first time using a rest API in C#, I work on a daily basis using a WCF server.
I am using IMemoryCache to handle caching, I set it to expire at 10 minutes. My idea was to edit AgileEngineWrapper.cs to create a timer using Reactive, also set a locking variable and in turn reload the images, the locking variable was to sleep a request that could have been made in the middle while the code is refreshing.

I also wanted to add a couple unit tests, this is why I created AgileEngineWrapper's singleton using an interface so I could mock it using moq.