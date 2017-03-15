using System.Collections.Generic;
using System.Linq;

namespace Paflamy
{
    public enum Stage
    {
        Start, Menu, Playing
    }

    public class Logic
    {
        public Stage Stage { get; private set; } = Stage.Start;

        public delegate void SimpleHandler();
        public event SimpleHandler LevelChanged;
        public event SimpleHandler StageChanged;

        public Level Level { get; private set; }
        public List<Level> LevelSet;

        public Logic(string levelSetRaw)
        {
            SetLevel(LevelInfo.GetRandom(7, 7, TileLock.None).ToLevel(), false);
            Level.Swap(1, 1, Level.Width - 2, Level.Height - 2);
            LevelSet = GetLevelSet(levelSetRaw);
        }

        private void SetLevel(Level l, bool randomize = true)
        {
            Level = l;

            if (randomize)
                Level.Randomize();

            LevelChanged?.Invoke();
        }

        private static List<Level> GetLevelSet(string levelSetRaw)
            => levelSetRaw.Trim().Split(' ').Select(raw => LevelInfo.Deserialize(raw).ToLevel()).ToList();

        public void Play(int levelIndex)
        {
            Stage = Stage.Playing;
            SetLevel(LevelSet[levelIndex]);
            OnStageChanged();
        }

        public void Start()
        {
            Stage = Stage.Menu;
            OnStageChanged();
        }

        private void OnStageChanged()
            => StageChanged?.Invoke();
    }
}
