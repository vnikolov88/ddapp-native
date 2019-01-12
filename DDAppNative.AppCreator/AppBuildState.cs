using System;
using System.Collections.Generic;
using System.Text;

namespace DDAppNative.AppCreator
{
    struct AppBuildState
    {
        public string AppCode { get; set; }
        public string ServiceHost { get; set; }
        public string BuildNumber { get; set; }
        public string AppVersion { get; set; }
        public string BundleIdentifier { get; set; }
        public IEnumerable<string> Caches { get; set; }
    }
}
