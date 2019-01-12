using DDAppNative.Android;
using DDAppNative.Common;
using System;
using Xamarin.Forms;

[assembly: Dependency(typeof(Native_Android))]

namespace DDAppNative.Android
{
    public class Native_Android : INative 
	{
		public string GetCacheDir () 
		{
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return documents;
        }

        public string GetPreCacheDir()
        {
            return "file:///android_asset/";
        }

        public string GetLocalGPSLink(string gpsIntent)
        {
            return gpsIntent;
        }
    }
}