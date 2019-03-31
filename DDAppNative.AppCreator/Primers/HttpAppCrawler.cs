using HtmlAgilityPack;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DDAppNative.AppCreator.Primers
{
    class HttpAppCrawler : IAppCrawler
    {
        private readonly Uri _appHostBase;
        public HttpAppCrawler(Uri appHostBase)
        {
            _appHostBase = appHostBase ?? throw new ArgumentNullException(nameof(appHostBase));
        }

        public async Task<string[]> GetAllUrlsAsync(string appCode)
        {
            var cacheList = new[] { $"/{appCode}/Page1" };

            for (var i = 0; i < cacheList.Length; ++i)
            {
                Console.WriteLine(cacheList[i]);
                // Don't load more then one layer of external resources
                if (cacheList[i].StartsWith("http")) continue;
                try
                {
                    var uri = new Uri(_appHostBase, $"{cacheList[i]}").AbsoluteUri;
                    var doc = await new HtmlWeb().LoadFromWebAsync(uri);
                    var resources = doc.DocumentNode.Descendants("link")
                                                      .Select(a => a.GetAttributeValue("href", null))
                                                      .Where(u => !string.IsNullOrEmpty(u)).ToList();
                    resources.AddRange(doc.DocumentNode.Descendants("a")
                                                      .Select(a => a.GetAttributeValue("href", null))
                                                      .Where(u => !string.IsNullOrEmpty(u)).ToList()
                                                      );
                    resources.AddRange(doc.DocumentNode.Descendants("script")
                                                      .Select(a => a.GetAttributeValue("src", null))
                                                      .Where(u => !string.IsNullOrEmpty(u)).ToList()
                                                      );
                    resources.AddRange(doc.DocumentNode.Descendants("img")
                                                      .Select(a => a.GetAttributeValue("src", null))
                                                      .Where(u => !string.IsNullOrEmpty(u)).ToList()
                                                      );

                    var tempCahce = cacheList.ToList();
                    // Full URL
                    tempCahce.AddRange(resources.Where(x => !x.Contains("ondevice")).Select(x => x.Replace("./", $"/{appCode}/")).Distinct());
                    // Partial Copy
                    tempCahce.AddRange(resources.Where(x => !x.Contains("ondevice") && !x.StartsWith('.') && !x.StartsWith('/') && !x.StartsWith("http")).Distinct().Select(x => $"/partial/{appCode}/{x}").ToList());
                    cacheList = tempCahce.Distinct().ToArray();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to load {cacheList[i]} skipping...");
                }
            }

            return cacheList;
        }
    }
}
