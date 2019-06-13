using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDAppNative.AppCreator.Builders
{
    class CodeStackAndroid : ICodeStack
    {
        readonly string _codeBaseDir;
        public CodeStackAndroid(string codeBaseDir)
        {
            _codeBaseDir = codeBaseDir ?? throw new ArgumentNullException(nameof(codeBaseDir));
        }

        public void FillInCode(AppBuildState state)
        {
            var variables = new Dictionary<string, string> {
                { "{{APP_CODE}}", state.AppCode },
                { "{{DISPLAY_NAME}}", state.DisplayName },
                { "{{SERVICE_HOST}}", state.ServiceHost },
                { "{{BUILD_NUMBER}}", state.BuildNumber },
                { "{{APP_VERSION}}", state.AppVersion },
                { "{{BUNDLE_IDENTIFIER}}", state.BundleIdentifier },
                { "{{CACHES}}", state.Caches.Aggregate(new StringBuilder(), (curr, next) => curr.Append($"\r\n    <AndroidAsset Include=\"Resources\\{next}\" />")).ToString() }
            };

            var appBaseDir = $"./{state.AppCode}";
            var appBuildBaseDir = $"{appBaseDir}/{state.BuildNumber}";

            FileUtils.ReplaceInFiles($"{appBuildBaseDir}/Android", variables);
        }

        public AppBuildState PrepareCodeBase(AppBuildState state)
        {
            var appBaseDir = $"./{state.AppCode}";
            var appBuildBaseDir = $"{appBaseDir}/{state.BuildNumber}";

            FileUtils.CopyFiles(_codeBaseDir, $"{appBuildBaseDir}/Android");

            return state;
        }
    }
}
