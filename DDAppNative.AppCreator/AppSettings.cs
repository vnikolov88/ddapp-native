using Newtonsoft.Json;
using System.IO;

namespace DDAppNative.AppCreator
{
    class AppSettings
    {
        public static AppSettings BuildSettings(string resourceDir, string appCode)
        {
            var appBaseDir = $"{resourceDir}/{appCode}/app.json";
            var fileContent = File.ReadAllText(appBaseDir);
            return JsonConvert.DeserializeObject<AppSettings>(fileContent);
        }

        public string AppVersion { get; set; }
        public string BundleIdentifier { get; set; }
        public string AppHostBaseUrl { get; set; }
        public string AppDisplayName { get; set; }
    }
}
