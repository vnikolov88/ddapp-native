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
    }
}