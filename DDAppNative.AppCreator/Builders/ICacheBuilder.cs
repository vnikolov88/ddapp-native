using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDAppNative.AppCreator.Builders
{
    interface ICacheBuilder
    {
        Task<AppBuildState> BuildLocalCacheAsync(AppBuildState state, IEnumerable<string> cacheList);
    }
}
