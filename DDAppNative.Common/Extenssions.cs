namespace DDAppNative.Common
{
    public static class StringExtensions
    {
        public static ulong GetGUID(this string self)
        {
            ulong hash = 5381;

            foreach (var c in self)
                hash = ((hash << 5) + hash) + c;

            return hash;
        }
    }
}
