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
using Android.Content.Res;

namespace Paflamy
{
    public enum Stage
    {
        Start, Playing
    }

    public static class Game
    {
        public static Stage Stage { get; private set; } = Stage.Start;

        public static int Width => level.Width;
        public static int Height => level.Height;

        public delegate void SimpleHandler();

        public static event SimpleHandler LevelChanged;

        private static Level level;

        private static List<LevelInfo> levelSet;

        public static void Init(string levelSetRaw)
        {
            var colors = GetRandomColors();
            level = new Level(7, 7, colors[0], colors[1], colors[2], colors[3], TileLock.None);
            level.Swap(1, 1, Width - 2, Height - 2);
            levelSet = GetLevels(levelSetRaw);
        }

        public static string GetLevelString()
            => level.Serialized();

        private static List<LevelInfo> GetLevels(string levelSetRaw)
            => levelSetRaw.Trim().Split(' ').Select(raw => LevelInfo.Deserialize(raw)).ToList();

        private static void OnLevelChanged()
            => LevelChanged?.Invoke();

        public static void Play()
        {
            Stage = Stage.Playing;
            var colors = GetRandomColors();
            //level = new Level(9, 10, colors[0], colors[1], colors[2], colors[3], TileLock.Borders);
            level = levelSet[0].ToLevel();
            level.Randomize();
            OnLevelChanged();
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
                    rand = Util.Random.Next(colors.Length);
                } while (nums.Contains(rand));
                nums.Add(rand);
            }

            return new List<Color> { Color.FromKnownColor(colors[nums[0]]),
                                     Color.FromKnownColor(colors[nums[1]]),
                                     Color.FromKnownColor(colors[nums[2]]),
                                     Color.FromKnownColor(colors[nums[3]]) };
        }

        public static void Swap(int x1, int y1, int x2, int y2)
            => level.Swap(x1, y1, x2, y2);

        public static Color Get(int x, int y)
            => level.Get(x, y);

        public static bool IsLocked(int x, int y)
            => level.IsLocked(x, y);

        public static bool IsSolved
            => level.IsSolved();
    }
}
