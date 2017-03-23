using System;
using Android.Views;
using System.Drawing;

namespace Paflamy
{
    public class MenuUI
    {
        public bool StartStage { get; private set; } = true;

        public const float LEVEL_SCALE = 0.6f;
        public const int NEIGHBORING_LEVEL_COUNT = 2;
        public const double SCROLL_TIME = 0.5;
        public const double SNAP_TIME = 0.1;
        public const double MTP_FADEOUT_TIME = 0.8; // mtp = menu-to-playing transition
        // THIS SHOULD ALWAYS BE TRUE: MTP_FADEOUT_TIME <= MTP_ZOOMIN_DELAY + MTP_ZOOMIN_TIME
        public const double MTP_ZOOMIN_TIME = 1.2;
        public const double MTP_ZOOMIN_DELAY = 0.2;
        public float X_PADDING { get; private set; }
        public float Y_PADDING { get; private set; }
        public float LEVEL_MARGIN { get; private set; }
        public float LEVEL_DISTANCE { get; private set; }
        public float LEVEL_WIDTH { get; private set; }

        public RectangleF StartButton { get; private set; }
        public static readonly Color StartButtonColor = Color.DodgerBlue;
        public static readonly Color BackgroundColor = Color.FromArgb(247, 239, 210);
        public Level StartLevel { get; private set; }

        public event UI.SimpleHandler SwitchToPlaying;

        public float StartTileSize { get; private set; }

        public int LevelIndex { get; private set; }
        public float Offset { get; private set; }

        public bool MTPOngoing { get; private set; }
        public double MTPBackgroundCoverAlpha { get; private set; }
        public float MTPLevelScale { get; private set; }
        public float MTPOffset { get; private set; }
        public float MTPXPadding { get; private set; }
        public float MTPYPadding { get; private set; }

        private double mtpTime;

        private bool dragging;
        private float dragStartX;
        private float dragStartY;
        private float startOffset;
        private float lastOffset;

        private bool prevDragging;
        private bool touchIsTap;

        private double snapTime;
        private bool snapOngoing;

        private double scrollTime;
        private float scrollGlobalStartOffset;
        private float scrollGlobalEndOffset;

        private readonly UI ui;
        private readonly Game game;

        public MenuUI(UI ui, Game game)
        {
            this.ui = ui;
            this.game = game;

            StartLevel = LevelInfo.GetRandom(7, 7, TileLock.None).ToLevel();
            StartLevel.Swap(1, 1, StartLevel.Width - 2, StartLevel.Height - 2);

            ui.GetTileSize(StartLevel, out float mts, out float _);
            StartTileSize = mts;

            X_PADDING = (1 - LEVEL_SCALE) / 2 * ui.SCREEN_WIDTH;
            Y_PADDING = 0.15f * ui.SCREEN_HEIGHT;
            LEVEL_MARGIN = 0.09f * ui.SCREEN_WIDTH;
            LEVEL_WIDTH = ui.SCREEN_WIDTH * LEVEL_SCALE;
            LEVEL_DISTANCE = LEVEL_WIDTH + LEVEL_MARGIN;

            float bWidth = ui.SCREEN_WIDTH / 3f;
            float bHeight = (ui.SCREEN_HEIGHT - ui.SCREEN_WIDTH) / 3f;
            StartButton = new RectangleF(bWidth, ui.SCREEN_WIDTH + bHeight, bWidth, bHeight);
        }

        public void ResetAnimations()
        {
            MTPOngoing = false;
            Offset = 0;
        }

        private void NormalizeOffset(int index, float offset, out int nIndex, out float nOffset)
        {
            float adjOffset = -offset + LEVEL_DISTANCE / 2;

            nIndex = index + (int)(adjOffset / LEVEL_DISTANCE) + (adjOffset < 0 ? -1 : 0);
            nOffset = -((adjOffset % LEVEL_DISTANCE) + (adjOffset < 0 ? LEVEL_DISTANCE : 0) - LEVEL_DISTANCE / 2);
        }

        private float GetGlobalOffset(int index, float offset)
            => -index * LEVEL_DISTANCE + offset;

        private void UpdateMenuStage(double dt)
        {
            if (!dragging && Offset != 0)
            {
                if (prevDragging && lastOffset != 0)
                {
                    snapOngoing = false;

                    NormalizeOffset(LevelIndex, Offset, out int scrollStartIndex, out float scrollStartOffset);
                    int scrollEndIndex = scrollStartIndex + (scrollStartIndex == LevelIndex ? -Math.Sign(lastOffset) : 0);

                    if (scrollEndIndex < 0)
                        scrollEndIndex = 0;
                    else if (scrollEndIndex >= game.LevelSet.Count)
                        scrollEndIndex = game.LevelSet.Count - 1;

                    scrollGlobalStartOffset = GetGlobalOffset(scrollStartIndex, scrollStartOffset);
                    scrollGlobalEndOffset = GetGlobalOffset(scrollEndIndex, 0);

                    scrollTime = 0;
                }
                else if (scrollTime / SCROLL_TIME < 1)
                {
                    scrollTime += dt;

                    // ----------
                    NormalizeOffset
                    (
                        0,
                        scrollGlobalStartOffset + Smooth(scrollTime / SCROLL_TIME) * (scrollGlobalEndOffset - scrollGlobalStartOffset),
                        out int nIndex,
                        out float nOffset
                    );
                    // ----------

                    LevelIndex = nIndex;
                    Offset = nOffset;
                }
                else
                {
                    Offset = 0; // the smooth function brings it very close to 0 (due to float inaccuracy) at the end of the animation
                                    // but I have to set to 0 here to avoid going into this 'if'
                                    //scrollProgress = 0; // isn't necessary
                }
            }
            else if (snapOngoing)
            {
                if (snapTime / SNAP_TIME < 1)
                {
                    snapTime += dt;
                    Offset = startOffset + Smooth(snapTime / SNAP_TIME) * lastOffset;
                }
                else
                {
                    snapOngoing = false;
                }
            }

            prevDragging = dragging;
        }

        private void UpdateMTP(double dt)
        {
            if (mtpTime / (MTP_ZOOMIN_DELAY + MTP_ZOOMIN_TIME) < 1)
            {
                mtpTime += dt;
                CalculateMTP();
            }
            else
                SwitchToPlaying?.Invoke();
        }

        private void CalculateMTP()
        {
            var fadeoutSmooth = Smooth(Math.Min(mtpTime / MTP_FADEOUT_TIME, 1));
            var zoominSmooth = Smooth(Math.Max(mtpTime - MTP_ZOOMIN_DELAY, 0) / MTP_ZOOMIN_TIME);

            MTPBackgroundCoverAlpha = fadeoutSmooth;
            MTPLevelScale = LEVEL_SCALE + zoominSmooth * (1 - LEVEL_SCALE);
            MTPOffset = Offset * (1 - zoominSmooth);
            MTPXPadding = X_PADDING * (1 - zoominSmooth);
            MTPYPadding = Y_PADDING + zoominSmooth * (ui.LEVEL_VERTICAL_GAP - Y_PADDING);
        }

        private static float Smooth(double x)
            => (float)Math.Sin(Math.PI / 2 * x); // => (float)((1 - Math.Cos(Math.PI * x)) / 2);

        public void Update(double dt)
        {
            if (MTPOngoing)
                UpdateMTP(dt);
            else
                UpdateMenuStage(dt);
        }

        public void HandleTouch(MotionEvent e)
        {
            if (StartStage)
            {
                if (e.Action == MotionEventActions.Down && StartButton.IntersectsWith(new RectangleF(e.GetX(), e.GetY(), 1, 1)))
                    StartStage = false;
            }
            else
                HandleMenuTouch(e);
        }

        private void HandleMenuTouch(MotionEvent e)
        {
            dragging = e.Action != MotionEventActions.Up;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    dragStartX = e.GetX();
                    dragStartY = e.GetY();
                    startOffset = Offset;
                    lastOffset = 0;
                    touchIsTap = true;

                    break;

                case MotionEventActions.Move:
                    lastOffset = e.GetX() - dragStartX;

                    if (touchIsTap)
                    {
                        if (Math.Pow(lastOffset, 2) + Math.Pow(e.GetY() - dragStartY, 2) > Math.Pow(ui.TAP_THRESHOLD_DIST, 2)) // tap &&
                        {
                            touchIsTap = false;
                            snapTime = 0;
                            snapOngoing = true;
                        }
                    }
                    else if (!snapOngoing)
                        Offset = startOffset + lastOffset;

                    break;

                case MotionEventActions.Up:
                    if (touchIsTap)
                    {
                        game.LevelIndex = LevelIndex;

                        MTPOngoing = true;
                        mtpTime = 0;

                        CalculateMTP();
                    }

                    break;
            }
        }
    }
}