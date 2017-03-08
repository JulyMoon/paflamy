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

        private static int levelIndex;

        public static void Init(string levelSetRaw)
        {
            level = LevelInfo.GetRandom(7, 7, TileLock.None).ToLevel();
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
            NewLevel();
        }

        public static void NewLevel()
        {
            //level = LevelInfo.GetRandom().ToLevel();
            level = levelSet[(levelIndex++) % levelSet.Count].ToLevel();
            //level.Randomize();
            OnLevelChanged();
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
