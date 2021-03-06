using Android.Views;
using System.Collections.Generic;
using Size = System.Drawing.SizeF;

namespace Paflamy
{
    public class UI
    {
        public int SCREEN_WIDTH { get; private set; }
        public int SCREEN_HEIGHT { get; private set; }

        public float LEVEL_VERTICAL_GAP { get; private set; }
        public double TAP_THRESHOLD_DIST { get; private set; }

        public bool PlayingStage { get; private set; }

        public delegate void SimpleHandler();
        public event SimpleHandler StageChanged;
        
        public List<Size> TileSizes { get; private set; }

        private readonly Game game;
        public readonly GameUI GameUI;
        public readonly MenuUI MenuUI;

        public UI(Game game, int width, int height)
        {
            this.game = game;

            SCREEN_WIDTH = width;
            SCREEN_HEIGHT = height;

            TAP_THRESHOLD_DIST = 0.05f * SCREEN_WIDTH;
            LEVEL_VERTICAL_GAP = 0.05f * SCREEN_HEIGHT;

            TileSizes = new List<Size>();
            foreach (var level in game.LevelSet)
            {
                GetTileSize(level, out float w, out float h);
                TileSizes.Add(new Size(w, h));
            }

            GameUI = new GameUI(this, game);
            MenuUI = new MenuUI(this, game);
            MenuUI.SwitchToPlaying += (() => { ChangeStage(true); });
        }

        private void ChangeStage(bool playing)
        {
            PlayingStage = playing;
            StageChanged?.Invoke();
        }

        public void Back()
        {
            if (PlayingStage)
            {
                MenuUI.ResetAnimations();
                ChangeStage(false);
            }
        }

        public void GetTileSize(Level level, out float width, out float height)
        {
            width = (float)SCREEN_WIDTH / level.Width;
            height = (SCREEN_HEIGHT - 2 * LEVEL_VERTICAL_GAP) / level.Height;
        }

        public void OnUpdate(double dt)
        {
            if (!PlayingStage)
                MenuUI.Update(dt);
        }

        public bool OnTouch(MotionEvent e)
        {
            if (PlayingStage)
                GameUI.HandleTouch(e);
            else
                MenuUI.HandleTouch(e);

            return true;
        }
    }
}