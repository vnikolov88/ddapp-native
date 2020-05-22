using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDAppNative.AppCreator.Builders
{
    class CodeStackiOS : ICodeStack
    {
        readonly string _codeBaseDir;
        public CodeStackiOS(string codeBaseDir)
        {
            _codeBaseDir = codeBaseDir ?? throw new ArgumentNullException(nameof(codeBaseDir));
        }

        public void FillInCode(AppBuildState state)
        {
            var variables = new Dictionary<string, string> {
                { "{{APP_CODE}}", state.AppCode },
                { "{{DISPLAY_NAME}}", state.DisplayName },
                { "{{SERVICE_HOST}}", state.ServiceHost },
                { "{{SERVICE_INITIAL_URL}}", state.ServiceInitialUrl },
                { "{{BUILD_NUMBER}}", state.BuildNumber },
                { "{{APP_VERSION}}", state.AppVersion },
                { "{{BUNDLE_IDENTIFIER}}", state.BundleIdentifier },
                { "{{ONE_SIGNAL_IDENTIFIER}}", state.OneSignalIdentifier },
                { "{{IGNORE_URLS}}", string.Join(',', state.IgnoreUrls.Select(x => $"\"{x}\"")) },
                { "{{CACHES}}", state.Caches.Aggregate(new StringBuilder(), (curr, next) => curr.Append($"\r\n    <BundleResource Include=\"Resources\\{next}\" />")).ToString() }
            };

            var appBaseDir = $"./{state.AppCode}";
            var appBuildBaseDir = $"{appBaseDir}/{state.BuildNumber}";

            FileUtils.ReplaceInFiles($"{appBuildBaseDir}/iOS", variables);
        }

        public AppBuildState PrepareCodeBase(AppBuildState state)
        {
            var appBaseDir = $"./{state.AppCode}";
            var appBuildBaseDir = $"{appBaseDir}/{state.BuildNumber}";
            
            FileUtils.CopyFiles(_codeBaseDir, $"{appBuildBaseDir}/iOS");
            FileUtils.CopyFile("DDAppNative.Common.dll", $"{appBuildBaseDir}/iOS/DDAppNative.Common.dll");

            return state;
        }
    }
}
