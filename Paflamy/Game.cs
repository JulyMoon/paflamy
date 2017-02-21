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
    class Game
    {
        public int Width => map.Width;
        public int Height => map.Height;

        private static readonly Random random = new Random();

        private Map map;
        public Game()
        {
            var colors = GetRandomColors();
            map = new Map(9, 10, colors[0], colors[1], colors[2], colors[3]);
        }

        private List<Color> GetRandomColors()
        {
            var colors = (KnownColor[])Enum.GetValues(typeof(KnownColor));

            var nums = new List<int>();
            for (int i = 0; i < 4; ++i)
            {
                int rand;
                do
                {
                    rand = random.Next(colors.Length);
                } while (nums.Contains(rand));
                nums.Add(rand);
            }

            return new List<Color> { Color.FromKnownColor(colors[nums[0]]),
                                     Color.FromKnownColor(colors[nums[1]]),
                                     Color.FromKnownColor(colors[nums[2]]),
                                     Color.FromKnownColor(colors[nums[3]]) };
        }

        public void Swap(int x1, int y1, int x2, int y2)
            => map.Swap(x1, y1, x2, y2);

        public Color Get(int x, int y)
            => map.Get(x, y);

        public bool IsLocked(int x, int y)
            => map.IsLocked(x, y);

        public bool IsSolved
            => map.IsSolved();
    }
}
