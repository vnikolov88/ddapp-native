using System;
using System.Collections.Generic;
using System.Text;

namespace DDAppNative.AppCreator
{
    struct AppBuildState
    {
        public string AppCode { get; set; }
        public string DisplayName { get; set; }
        public string ServiceHost { get; set; }
        public string ServiceInitialUrl { get; set; }
        public string BuildNumber { get; set; }
        public string AppVersion { get; set; }
        public string BundleIdentifier { get; set; }
        public string OneSignalIdentifier { get; set; }
        public IEnumerable<string> IgnoreUrls { get; set; }
        public IEnumerable<string> Caches { get; set; }
    }
}
