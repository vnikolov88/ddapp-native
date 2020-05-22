using Plugin.Geolocator;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Xamarin.Forms;
using Com.OneSignal;
using System.Collections.Generic;

namespace DDAppNative.Common
{
    public class App : Application
    {
        private CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
        private static string DDAppLocalUrl = $"http://127.0.0.1:{new Random().Next(9696, 9898)}";
        private WebServer _webServer;
        private Task _webServerTask;
        private object _webServerLock = new object();
        private CancellationTokenSource _webServerShutdownToken;
        private static DateTime _nextOnDevicePopup;
        private const ulong TimeFromLastPopupMs = 1000;
        protected static ApplicationCache _cache;
        protected static INative _nativeService;
        protected ICollection<string> _ignoreUrls;
        protected AppMainPage _appMainPage;

        public class AppMainPage : ContentPage
        {
            private readonly string _startUrl;
            private WebView _browser;

            public AppMainPage(string startUrl)
            {
                _startUrl = startUrl ?? throw new ArgumentNullException(nameof(startUrl));

                ToLoadingState();
            }

            private void ToLoadingState()
            {
                var _layout = new StackLayout()
                {
                    VerticalOptions = LayoutOptions.Center,
                    HeightRequest = 120,
                };
                _layout.Children.Add(new ActivityIndicator()
                {
                    IsVisible = true,
                    IsRunning = true,
                    HeightRequest = 120,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                });
                Content = _layout;
            }

            public void ToWebState()
            {
                _browser = new WebView()
                {
                    Source = _startUrl
                };
                _browser.Navigated += _browser_Navigated;
                _browser.Navigating += _browser_Navigating;
                
                Content = _browser;
            }

            private void _browser_Navigated(object sender, WebNavigatedEventArgs e)
            {
                _browser.EvaluateJavaScriptAsync("window.open = function(open) { return function (url, name, features) { window.location.href = url; return window; }; } (window.open);");
            }

            private void _browser_Navigating(object sender, WebNavigatingEventArgs e)
            {
                if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                {
                    if (e.Url.StartsWith("geo:") ||
                        e.Url.StartsWith("tel:") ||
                        (new Uri(e.Url).IsAbsoluteUri && (
                            !e.Url.StartsWith(_startUrl) &&
                            !e.Url.StartsWith(DDAppLocalUrl) &&
                            !e.Url.StartsWith("https://landbot.io") &&
                            !e.Url.StartsWith("https://www.youtube") &&
                            !e.Url.StartsWith("https://amsel.zabaria.de") &&
                            !e.Url.StartsWith("https://daisho.firebaseapp.com")
                        )) ||
                        e.Url.EndsWith(".pdf"))
                    {
                        e.Cancel = true;
                        var deviceUrl = e.Url;
                        var _now = DateTime.Now;
                        if (_nextOnDevicePopup > _now)
                            return;

                        if (deviceUrl.StartsWith("geo:")) deviceUrl = _nativeService.GetLocalGPSLink(deviceUrl);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Device.OpenUri(new Uri(deviceUrl));
                        });
                        _nextOnDevicePopup = _now.AddMilliseconds(TimeFromLastPopupMs);
                    }
                }
            }

            protected override bool OnBackButtonPressed()
            {
                if (_browser != null)
                {
                    _browser.GoBack();

                    return true;
                }
                else
                    return false;
            }
        }

        public App(
            string appHostBaseUrl,
            string appHostInitialUrl,
            string oneSignalIdentifier,
            ICollection<string> ignoreUrls)
        {
            _ignoreUrls = ignoreUrls ?? new List<string>();
            _appMainPage = new AppMainPage($"{DDAppLocalUrl}{appHostInitialUrl}");
            MainPage = _appMainPage;

            StartWebProxy();
            _nativeService = DependencyService.Get<INative>();
            var cacheBaseDir = _nativeService.GetCacheDir();
            _cache = new ApplicationCache(appHostBaseUrl, cacheBaseDir);

            Task.Run(async () =>
            {
                _nativeService.LoadPreCache();
                await Task.Delay(1000);

                Device.BeginInvokeOnMainThread(() =>
                {
                    _appMainPage.ToWebState();

                    if (!string.IsNullOrWhiteSpace(oneSignalIdentifier))
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(3000);
                            OneSignal.Current.StartInit(oneSignalIdentifier).EndInit();
                        });
                    }
                });
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
                        if(url.StartsWith("/partial/ondevice/") || url.StartsWith("/ondevice/") || url.StartsWith("geo:") || url.StartsWith("tel:"))
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
#warning EmbedIO update breaks this kind of flow
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

