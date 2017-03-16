using System.Collections.Generic;
using System.Linq;

namespace Paflamy
{
    public class Game
    {
        public List<Level> LevelSet { get; private set; }
        public int LevelIndex { get; set; }

        public Game(string levelSetRaw)
        {
            LevelSet = GetLevelSet(levelSetRaw);
        }

        private static List<Level> GetLevelSet(string levelSetRaw)
            => levelSetRaw.Trim().Split(' ').Select(raw => LevelInfo.Deserialize(raw).ToLevel()).ToList();
    }
}
