namespace DDAppNative.Common
{
    public interface INative
    {
        string GetCacheDir();
        void LoadPreCache();
        string GetLocalGPSLink(string gpsIntent);
    }
}
