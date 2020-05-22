using DDAppNative.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DDAppNative.AppCreator.Builders
{
    class LocalCacheBuilder : ICacheBuilder
    {
        private readonly Uri _appHostBase;

        public LocalCacheBuilder(Uri appHostBase)
        {
            _appHostBase = appHostBase ?? throw new ArgumentNullException(nameof(appHostBase));
        }

        public async Task<AppBuildState> BuildLocalCacheAsync(AppBuildState state, IEnumerable<string> cacheList)
        {
            var appBaseDir = $"./{state.AppCode}/{state.BuildNumber}/Caches";
            Directory.CreateDirectory(appBaseDir);
             var applicationCache = new ApplicationCache(_appHostBase.ToString(), appBaseDir);

            var fileCaches = new List<string>();
            var unsucessfullCaches = new List<string>();
            foreach (var cacheUrl in cacheList)
            {
                try
                {
                    var url = new Uri(_appHostBase, $"{cacheUrl}").ToString();
                    var fileName = $"Caches{cacheUrl.GetGUID()}";
                    var filePath = $"{appBaseDir}/{fileName}";

                    Console.WriteLine($"{fileName} <== {cacheUrl}");
                    var remoteCache = await applicationCache.CheckRemoteCacheAsync(url, string.Empty).ConfigureAwait(false);
                    if (remoteCache != null)
                    {
                        using (var cache = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            var outputStreams = new List<Stream> { cache };

                            await applicationCache.WriteRemoteCacheAsync(remoteCache, cache, outputStreams).ConfigureAwait(false);

                            fileCaches.Add(fileName);
                        }
                    }
                }
                catch
                {
                    unsucessfullCaches.Add(cacheUrl);
                }
            }

            foreach (var url in unsucessfullCaches) Console.WriteLine($"Unable to cache {url}");
            state.Caches = fileCaches;

            return state;
        }
    }
}
