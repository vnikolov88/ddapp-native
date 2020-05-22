using DDAppNative.AppCreator.Builders;
using DDAppNative.AppCreator.Primers;
using DDAppNative.Common;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DDAppNative.AppCreator
{
    internal class Program
    {
        static readonly string codeBaseDiriOS = ".\\Templates\\iOS";
        static readonly string codeBaseDirAndroid = ".\\Templates\\Android";
        static readonly string resourceDir = "..\\..\\..\\..\\Resources\\";
        static IAppCrawler _appCrawler;
        static ICacheBuilder _cacheBuilder;
        static ICodeStack _codeStackiOS;
        static ICodeStack _codeStackAndroid;
        static IResourceStack _resourceStackiOS;
        static IResourceStack _resourceStackAndroid;

        // AppCreator.exe {appCode}
        private static void Main(string[] args)
        {
            var appCode = args[0];
            var appSettings = AppSettings.BuildSettings(resourceDir, appCode);
            var baseUri = new Uri($"{appSettings.AppHostBaseUrl}");

            var appBuildState = new AppBuildState {
                AppCode = appCode,
                DisplayName = appSettings.AppDisplayName,
                ServiceHost = appSettings.AppHostBaseUrl,
                ServiceInitialUrl = appSettings.AppHostInitialUrl ?? appSettings.AppHostBaseUrl,
                AppVersion = appSettings.AppVersion,
                OneSignalIdentifier = appSettings.OneSignalIdentifier,
                BundleIdentifier = appSettings.BundleIdentifier,
                IgnoreUrls = appSettings.IgnoreUrls
            };

            #region Get build number

            var appBaseDir = $"./{appBuildState.AppCode}";
            Directory.CreateDirectory(appBaseDir);
            // Check build number
            var buildDirs = new DirectoryInfo(appBaseDir).GetDirectories("*").Where(x => Regex.IsMatch(x.Name, $"^[0-9]*$"));
            var lastBuild = buildDirs.Select(x => uint.Parse(x.Name))
                .OrderByDescending(x => x)
                .FirstOrDefault();

            appBuildState.BuildNumber = $"{lastBuild + 1}";

            #endregion Get build number

            #region Create Code Base

            _codeStackiOS = new CodeStackiOS(codeBaseDiriOS);
            _codeStackAndroid = new CodeStackAndroid(codeBaseDirAndroid);

            Console.WriteLine("Prepare code base for iOS");
            appBuildState = _codeStackiOS.PrepareCodeBase(appBuildState);
            Console.WriteLine("Prepare code base for Android");
            appBuildState = _codeStackAndroid.PrepareCodeBase(appBuildState);

            #endregion Code Base

            #region Cache

            _appCrawler = new HttpAppCrawler(baseUri);
            _cacheBuilder = new LocalCacheBuilder(baseUri);

            Console.WriteLine("Get all remote URL's");
            var cacheList = _appCrawler.GetAllUrlsAsync(appSettings.IgnoreUrls).GetAwaiter().GetResult();
            
            Console.WriteLine("Build local cache");
            appBuildState = _cacheBuilder.BuildLocalCacheAsync(appBuildState, cacheList).GetAwaiter().GetResult();

            #endregion Cache

            #region Populate Code Base

            Console.WriteLine("Fill in code base for iOS");
            _codeStackiOS.FillInCode(appBuildState);
            Console.WriteLine("Fill in code base for Android");
            _codeStackAndroid.FillInCode(appBuildState);

            #endregion Populate Code Base

            #region Copy Resource Package

            _resourceStackiOS = new ResourceStackiOS(resourceDir);
            _resourceStackAndroid = new ResourceStackAndroid(resourceDir);

            Console.WriteLine("Copy resources for iOS");
            _resourceStackiOS.FillInResources(appBuildState);
            Console.WriteLine("Copy resources for Android");
            _resourceStackAndroid.FillInResources(appBuildState);

            #endregion Copy Resource Package

            Console.WriteLine("Done!");
            Console.WriteLine("Exiting...");
        }
    }
}
