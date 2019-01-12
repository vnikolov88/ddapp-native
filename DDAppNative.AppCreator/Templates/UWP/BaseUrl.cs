using DoctorHelpMobile.UWP;
using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(BaseUrl))]
namespace DoctorHelpMobile.UWP
{
    public class BaseUrl : IBaseUrl
    {
        public string GetCacheDir()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, "Caches");
        }

        public string GetPreCacheDir()
        {
            return "ms-appdata:///";
        }
    }
}
