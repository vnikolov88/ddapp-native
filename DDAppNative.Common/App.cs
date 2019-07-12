using Plugin.Geolocator;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Xamarin.Forms;
using Com.OneSignal;

namespace DDAppNative.Common
{
    public class App : Application // superclass new in 1.3
    {
        private CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
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
            private WebView _browser;

            public WebPage(string startUrl)
            {
                _startUrl = startUrl ?? throw new ArgumentNullException(nameof(startUrl));
                
                _browser = new WebView();
                _browser.Source = _startUrl;
                Content = _browser;
            }

            protected override bool OnBackButtonPressed()
            {
                _browser.GoBack();

                return true;
            }
        }

        public App(
            string appCode,
            string appHostBaseAddress,
            string oneSignalIdentifier)
        {
            StartWebProxy();
            _nativeService = DependencyService.Get<INative>();
            var cacheBaseDir = _nativeService.GetCacheDir();
            _cache = new ApplicationCache(appHostBaseAddress, cacheBaseDir);

            _nativeService.LoadPreCache();

            MainPage = new WebPage($"{DDAppLocalUrl}{appCode}/Page1");

            Task.Run(async () =>
            {
                await Task.Delay(2000);
                OneSignal.Current.StartInit(oneSignalIdentifier).EndInit();
            });
        }

        private void StartWebProxy()
        {
            lock (_webServerLock)
            {
                if (_webServer == null)
                {
                    _webServerShutdownToken = new CancellationTokenSource();
                    _webServer = new WebServer(new string[] { DDAppLocalUrl }, Unosquare.Labs.EmbedIO.Constants.RoutingStrategy.Regex, HttpListenerMode.Microsoft);
                    _webServer.OnGet( async (IHttpContext context, CancellationToken ct) => {
                        var url = context.Request.RawUrl;
                        if(url.StartsWith("/partial/ondevice/") || url.StartsWith("/ondevice/"))
                        {
                            try
                            {
                                var deviceUrl = url.Substring(url.IndexOf("/ondevice/") + "/ondevice/".Length);
                                if (deviceUrl.StartsWith("location:current"))
                                {
                                    var position = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(10));
                                    return await context.StringResponseAsync($"{position.Latitude.ToString(_defaultCulture)},{position.Longitude.ToString(_defaultCulture)}", contentType: "text/plain", cancellationToken: ct);
                                }
                                else if (deviceUrl.StartsWith("location:last"))
                                {
                                    var position = await CrossGeolocator.Current.GetLastKnownLocationAsync();
                                    return await context.StringResponseAsync($"{position.Latitude.ToString(_defaultCulture)},{position.Longitude.ToString(_defaultCulture)}", contentType: "text/plain", cancellationToken: ct);
                                }
                                else
                                {
                                    var _now = DateTime.Now;
                                    if (_nextOnDevicePopup > _now)
                                        return true;

                                    if (deviceUrl.StartsWith("geo:")) deviceUrl = _nativeService.GetLocalGPSLink(deviceUrl);
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        Device.OpenUri(new Uri(deviceUrl));
                                    });
                                    _nextOnDevicePopup = _now.AddMilliseconds(TimeFromLastPopupMs);
                                    Debug.Print($"Open ONDEVICE {url}");
                                    return true;
                                }
                            }
                            catch (Exception ex)
                            {
                                return await context.JsonExceptionResponseAsync(ex, cancellationToken: ct);
                            }
                        }
                        else
                        {
                            try
                            {
                                await _cache.SendAndUpdateAsync(url, context.Response, cancellationToken: ct);

                                Debug.Print($"PageLoad {url}");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Debug.Print("PageLoad ERROR");
                                return await context.JsonExceptionResponseAsync(ex, cancellationToken: ct);
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
                    //_webServerShutdownToken.Dispose();
                    //_webServer.Listener.Stop();
                    //_webServer.Dispose();
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

