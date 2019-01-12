using System.IO;
using System.Net;

namespace DDAppNative.Common
{
    public class CacheCandidateState : CacheState
    {
        public Stream ResponseStream { get; set; }
    }

    public class CacheState
    {
        public string AppPageVersion { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ContentType { get; set; }
        public long ContentLength64 { get; set; }
    }
}
