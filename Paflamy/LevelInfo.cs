using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Drawing;

namespace Paflamy
{
    public class LevelInfo
    {
        public readonly Color TopLeft;
        public readonly Color TopRight;
        public readonly Color BottomRight;
        public readonly Color BottomLeft;

        public readonly int Width, Height;
        
        public readonly Lock Lock;

        private const char delim = ';';

        public LevelInfo(int width, int height, Color topLeft, Color topRight, Color bottomRight, Color bottomLeft, Lock l)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            Width = width;
            Height = height;
            Lock = l;
        }

        public string Serialize()
            => $"{TopLeft.ToArgb()}{delim}{TopRight.ToArgb()}{delim}{BottomRight.ToArgb()}{delim}{BottomLeft.ToArgb()}{delim}{Width}{delim}{Height}{delim}{(int)Lock}";

        public LevelInfo Deserialize(string s)
        {
            var split = s.Split(delim);
            return new LevelInfo(Int32.Parse(split[4]),
                                 Int32.Parse(split[5]),
                                 Color.FromArgb(Int32.Parse(split[0])),
                                 Color.FromArgb(Int32.Parse(split[1])),
                                 Color.FromArgb(Int32.Parse(split[2])),
                                 Color.FromArgb(Int32.Parse(split[3])),
                                 (Lock)Int32.Parse(split[6]));
        }
    }
}