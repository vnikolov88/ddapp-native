using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDAppNative.AppCreator.Primers
{
    interface IAppCrawler
    {
        Task<string[]> GetAllUrlsAsync(IEnumerable<string> ignoreUrls);
    }
}
