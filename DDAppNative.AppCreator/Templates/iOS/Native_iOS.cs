using DDAppNative.Common;
using DDAppNative.iOS;
using Foundation;
using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(Native_iOS))]

namespace DDAppNative.iOS
{
    public class Native_iOS : INative 
	{
		public string GetCacheDir() 
		{
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, "..", "Library");
        }

        public void LoadPreCache()
        {
            var preCachedFiles = new DirectoryInfo(NSBundle.MainBundle.BundlePath).GetFiles($"Caches*");
            foreach (var file in preCachedFiles)
            {
                var newDest = $"{GetCacheDir()}/{file.Name}";
                if (File.Exists(newDest)) continue;
                file.CopyTo(newDest);
            }
        }

        public string GetLocalGPSLink(string gpsIntent)
        {
            return gpsIntent.Replace("geo:", "http://maps.apple.com/?ll=");
        }
    }
}