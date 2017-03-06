namespace Paflamy
{
    public static class Util
    {
        private const string DBG_TAG = "PAFLAMY";

        public static void Log(string s)
            => Android.Util.Log.Verbose(DBG_TAG, s);
    }
}