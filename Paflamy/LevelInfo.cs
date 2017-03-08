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
using Android.Content.Res;
using System.Collections;

namespace Paflamy
{
    public class LevelInfo
    {
        public readonly Color TopLeft;
        public readonly Color TopRight;
        public readonly Color BottomRight;
        public readonly Color BottomLeft;

        public readonly int Width, Height;
        
        public readonly TileLock TileLock;

        private const char delim = ';';

        public LevelInfo(int width, int height, Color topLeft, Color topRight, Color bottomRight, Color bottomLeft, TileLock tileLock)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            Width = width;
            Height = height;
            TileLock = tileLock;
            //Resources.System.GetString(Resource.String.LevelSet);
        }

        public Level ToLevel()
            => new Level(this);

        public string Serialized()
            => $"{TopLeft.ToArgb()}{delim}{TopRight.ToArgb()}{delim}{BottomRight.ToArgb()}{delim}{BottomLeft.ToArgb()}{delim}{Width}{delim}{Height}{delim}{(int)TileLock}";

        public static LevelInfo Deserialize(string s)
        {
            var split = s.Split(delim);
            return new LevelInfo(Int32.Parse(split[4]),
                                 Int32.Parse(split[5]),
                                 Color.FromArgb(Int32.Parse(split[0])),
                                 Color.FromArgb(Int32.Parse(split[1])),
                                 Color.FromArgb(Int32.Parse(split[2])),
                                 Color.FromArgb(Int32.Parse(split[3])),
                                 (TileLock)Int32.Parse(split[6]));
        }
    }
}