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

        public string GetPreCacheDir()
        {
            return NSBundle.MainBundle.BundlePath;
        }

        public string GetLocalGPSLink(string gpsIntent)
        {
            return gpsIntent.Replace("geo:", "http://maps.apple.com/?ll=");
        }
    }
}