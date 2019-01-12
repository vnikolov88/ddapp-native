namespace DDAppNative.Common
{
    public interface INative
    {
        string GetCacheDir();
        string GetPreCacheDir();
        string GetLocalGPSLink(string gpsIntent);
    }
}
