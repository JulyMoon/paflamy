using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Paflamy
{
    public enum Stage
    {
        Start, Playing
    }

    public static class Game
    {
        public static Stage Stage { get; private set; } = Stage.Start;

        public static int Width => map.Width;
        public static int Height => map.Height;

        private static Map map;

        public static void Init()
        {
            var colors = GetRandomColors();
            map = new Map(7, 7, colors[0], colors[1], colors[2], colors[3], Lock.None);
            map.Swap(1, 1, Width - 2, Height - 2);
        }

        public static void Play()
        {
            Stage = Stage.Playing;
            var colors = GetRandomColors();
            map = new Map(9, 10, colors[0], colors[1], colors[2], colors[3], Lock.Borders);
            map.Randomize();
        }

        private static List<Color> GetRandomColors()
        {
            var colors = (KnownColor[])Enum.GetValues(typeof(KnownColor));

            var nums = new List<int>();
            for (int i = 0; i < 4; ++i)
            {
                int rand;
                do
                {
                    rand = ExtensionMethods.Random.Next(colors.Length);
                } while (nums.Contains(rand));
                nums.Add(rand);
            }

            return new List<Color> { Color.FromKnownColor(colors[nums[0]]),
                                     Color.FromKnownColor(colors[nums[1]]),
                                     Color.FromKnownColor(colors[nums[2]]),
                                     Color.FromKnownColor(colors[nums[3]]) };
        }

        public static void Swap(int x1, int y1, int x2, int y2)
            => map.Swap(x1, y1, x2, y2);

        public static Color Get(int x, int y)
            => map.Get(x, y);

        public static bool IsLocked(int x, int y)
            => map.IsLocked(x, y);

        public static bool IsSolved
            => map.IsSolved();
    }
}
