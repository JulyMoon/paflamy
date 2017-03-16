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
        
        public List<Level> LevelSet { get; private set; }
        public int LevelIndex { get; private set; }

        public Logic(string levelSetRaw)
        {
            LevelSet = GetLevelSet(levelSetRaw);
        }

        private void SetLevel(int levelIndex, bool randomize = true)
        {
            LevelIndex = levelIndex;

            if (randomize)
                LevelSet[LevelIndex].Randomize();

            LevelChanged?.Invoke();
        }

        private static List<Level> GetLevelSet(string levelSetRaw)
            => levelSetRaw.Trim().Split(' ').Select(raw => LevelInfo.Deserialize(raw).ToLevel()).ToList();

        public void Play(int levelIndex)
        {
            Stage = Stage.Playing;
            SetLevel(levelIndex);
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
