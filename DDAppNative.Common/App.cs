
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Xamarin.Forms;

namespace DDAppNative.Common
{
    public class App : Application // superclass new in 1.3
    {
        private const string DDAppLocalUrl = "http://127.0.0.1:9696/";
        private WebServer _webServer;
        private Task _webServerTask;
        private object _webServerLock = new object();
        private CancellationTokenSource _webServerShutdownToken;
        private DateTime _nextOnDevicePopup;
        private const ulong TimeFromLastPopupMs = 1000;
        private static ApplicationCache _cache;
        private INative _nativeService;

        public class WebPage : ContentPage
        {
            private readonly string _startUrl;

            public WebPage(string startUrl)
            {
                _startUrl = startUrl ?? throw new ArgumentNullException(nameof(startUrl));
                
                var browser = new WebView();
                browser.Source = _startUrl;
                Content = browser;
            }
        }

        public App(string appCode, string appHostBaseAddress)
        {
            StartWebProxy();
            _nativeService = DependencyService.Get<INative>();
            var cacheBaseDir = _nativeService.GetCacheDir();
            var preCacheBaseDir = _nativeService.GetPreCacheDir();
            _cache = new ApplicationCache(appHostBaseAddress, cacheBaseDir);

            _cache.LoadPreCache(preCacheBaseDir);

            MainPage = new WebPage($"{DDAppLocalUrl}{appCode}/Page1");
        }

        private void StartWebProxy()
        {
            lock (_webServerLock)
            {
                if (_webServer == null)
                {
                    _webServerShutdownToken = new CancellationTokenSource();
                    _webServer = new WebServer(DDAppLocalUrl);
                    _webServer.OnGet( async (IHttpContext context, CancellationToken ct) => {
                        var url = context.Request.RawUrl;
                        if(url.StartsWith("/partial/ondevice/") || url.StartsWith("/ondevice/"))
                        {
                            var _now = DateTime.Now;
                            if (_nextOnDevicePopup > _now)
                                return true;

                            try
                            {
                                var deviceUrl = url.Substring(url.IndexOf("/ondevice/") + "/ondevice/".Length);
                                if (deviceUrl.StartsWith("geo:")) deviceUrl = _nativeService.GetLocalGPSLink(deviceUrl);
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    Device.OpenUri(new Uri(deviceUrl));
                                });

                                _nextOnDevicePopup = _now.AddMilliseconds(TimeFromLastPopupMs);
                                Debug.Print($"Open ONDEVICE {url}");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                return context.JsonExceptionResponse(ex);
                            }
                        }
                        else
                        {
                            try
                            {
                                await _cache.SendAndUpdateAsync(url, context.Response);

                                Debug.Print($"PageLoad {url}");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Debug.Print("PageLoad ERROR");
                                return context.JsonExceptionResponse(ex);
                            }
                        }
                    });

                    _webServerTask = Task.Run(async () => await _webServer.RunAsync(_webServerShutdownToken.Token));

                    Debug.Print("SERVER STARTED !!!");
                }
            }
        }

        private void StopWebProxy()
        {
            lock (_webServerLock)
            {
                if (_webServer != null)
                {
                    _webServerShutdownToken.Cancel();

                    _webServerTask.Wait();
                    _webServerShutdownToken.Dispose();
                    _webServer.Listener.Stop();
                    _webServer.Dispose();
                    _webServer = null;

                    Debug.Print("SERVER KILLED !!!");
                }
            }
        }

        protected override void OnSleep()
        {
            StopWebProxy();
            base.OnSleep();
        }

        protected override void OnResume()
        {
            StartWebProxy();
            base.OnResume();
        }
    }
}

