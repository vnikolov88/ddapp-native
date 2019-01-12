using System.Threading.Tasks;

namespace DDAppNative.AppCreator.Primers
{
    interface IAppCrawler
    {
        Task<string[]> GetAllUrlsAsync(string appCode);
    }
}
