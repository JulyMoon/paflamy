using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Paflamy
{
    public static class Util
    {
        private const string DBG_TAG = "PAFLAMY";
        public const bool DEBUG = true;

        public static readonly Random Random = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Log(string s)
            => Android.Util.Log.Verbose(DBG_TAG, s);
    }
}