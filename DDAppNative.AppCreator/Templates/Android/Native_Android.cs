using Android.Webkit;
using DDAppNative.Android;
using DDAppNative.Common;
using System;
using System.IO;
using System.Linq;
using Xamarin.Forms;

[assembly: Dependency(typeof(Native_Android))]

namespace DDAppNative.Android
{
    public class Native_Android : INative 
	{
		public string GetCacheDir() 
		{
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return documents;
        }

        public void LoadPreCache()
        {
            var preCachedFiles = Forms.Context.Assets.List("Resources").Where(x => x.StartsWith("Caches"));

            foreach (var file in preCachedFiles)
            {
                var newDest = $"{GetCacheDir()}/{file}";
                if (File.Exists(newDest)) continue;

                using (var readStream = Forms.Context.Assets.Open($"Resources/{file}"))
                {
                    using (var writeStream = File.OpenWrite(newDest))
                    {
                        readStream.CopyTo(writeStream);
                    }
                }
            }
        }

        public string GetLocalGPSLink(string gpsIntent)
        {
            return gpsIntent;
        }

        public void SetupCookies(Xamarin.Forms.WebView webView)
        {
            CookieManager.Instance.Flush();
            CookieManager.AllowFileSchemeCookies();
            CookieManager.SetAcceptFileSchemeCookies(true);
            CookieManager.Instance.AcceptCookie();
            //CookieManager.Instance.AcceptThirdPartyCookies(webView);
            CookieManager.Instance.SetAcceptCookie(true);
            //CookieManager.Instance.SetAcceptThirdPartyCookies(webView, true);
        }
    }
}