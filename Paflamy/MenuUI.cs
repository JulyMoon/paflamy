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
using Size = System.Drawing.SizeF;

namespace Paflamy
{
    public class MenuUI
    {
        public bool StartStage { get; private set; } = true;

        public const float MENU_LEVEL_SCALE = 0.6f;
        public const int MENU_NEIGHBOR_COUNT = 2;
        public const double MENU_SCROLL_TIME = 0.5;
        public const double MENU_SNAP_TIME = 0.1;
        public const double MTP_FADEOUT_TIME = 0.8; // mtp = menu-to-playing transition
        // THIS SHOULD ALWAYS BE TRUE: MTP_FADEOUT_TIME <= MTP_ZOOMIN_START_TIME + MTP_ZOOMIN_TIME
        public const double MTP_ZOOMIN_TIME = 1.2;
        public const double MTP_ZOOMIN_DELAY = 0.2;
        public float MENU_X_PADDING { get; private set; }
        public float MENU_Y_PADDING { get; private set; }
        public float MENU_LEVEL_MARGIN { get; private set; }
        public float MENU_LEVEL_DIST { get; private set; }
        public float MENU_LEVEL_WIDTH { get; private set; }

        public RectangleF StartButton { get; private set; }
        public static readonly Color StartButtonColor = Color.DodgerBlue;
        public static readonly Color BackgroundColor = Color.FromArgb(247, 239, 210);
        public Level StartLevel { get; private set; }

        public event UI.SimpleHandler SwitchToPlaying;

        public float MenuTileSize { get; private set; }

        public int MenuLevelIndex { get; private set; }
        public float MenuOffset { get; private set; }

        public bool MenuToPlaying { get; private set; }
        public double MTPBackgroundCoverAlpha { get; private set; }
        public float MTPLevelScale { get; private set; }
        public float MTPMenuOffset { get; private set; }
        public float MTPMenuXPadding { get; private set; }
        public float MTPMenuYPadding { get; private set; }

        private double mtpTime;

        private bool dragging;
        private float menuDragStartX;
        private float menuDragStartY;
        private float menuStartOffset;
        private float menuLastOffset;

        private bool prevDragging;
        private bool touchIsTap;

        private double menuSnapTime;
        private bool menuSnapOngoing;

        private double menuScrollTime;
        private float menuScrollGlobalStartOffset;
        private float menuScrollGlobalEndOffset;

        private readonly UI ui;
        private readonly Game game;

        public MenuUI(UI ui, Game game)
        {
            this.ui = ui;
            this.game = game;

            StartLevel = LevelInfo.GetRandom(7, 7, TileLock.None).ToLevel();
            StartLevel.Swap(1, 1, StartLevel.Width - 2, StartLevel.Height - 2);

            ui.GetTileSize(StartLevel, out float mts, out float _);
            MenuTileSize = mts;

            MENU_X_PADDING = (1 - MENU_LEVEL_SCALE) / 2 * ui.SCREEN_WIDTH;
            MENU_Y_PADDING = 0.15f * ui.SCREEN_HEIGHT;
            MENU_LEVEL_MARGIN = 0.09f * ui.SCREEN_WIDTH;
            MENU_LEVEL_WIDTH = ui.SCREEN_WIDTH * MENU_LEVEL_SCALE;
            MENU_LEVEL_DIST = MENU_LEVEL_WIDTH + MENU_LEVEL_MARGIN;

            float bWidth = ui.SCREEN_WIDTH / 3f;
            float bHeight = (ui.SCREEN_HEIGHT - ui.SCREEN_WIDTH) / 3f;
            StartButton = new RectangleF(bWidth, ui.SCREEN_WIDTH + bHeight, bWidth, bHeight);
        }

        public void ResetAnimations()
        {
            MenuToPlaying = false;
            MenuOffset = 0;
        }

        private void NormalizeOffset(int index, float offset, out int nIndex, out float nOffset)
        {
            float adjOffset = -offset + MENU_LEVEL_DIST / 2;

            nIndex = index + (int)(adjOffset / MENU_LEVEL_DIST) + (adjOffset < 0 ? -1 : 0);
            nOffset = -((adjOffset % MENU_LEVEL_DIST) + (adjOffset < 0 ? MENU_LEVEL_DIST : 0) - MENU_LEVEL_DIST / 2);
        }

        private float GetGlobalOffset(int index, float offset)
            => -index * MENU_LEVEL_DIST + offset;

        private void UpdateMenuStage(double dt)
        {
            if (!dragging && MenuOffset != 0)
            {
                if (prevDragging && menuLastOffset != 0)
                {
                    menuSnapOngoing = false;

                    NormalizeOffset(MenuLevelIndex, MenuOffset, out int scrollStartIndex, out float scrollStartOffset);
                    int scrollEndIndex = scrollStartIndex + (scrollStartIndex == MenuLevelIndex ? -Math.Sign(menuLastOffset) : 0);

                    if (scrollEndIndex < 0)
                        scrollEndIndex = 0;
                    else if (scrollEndIndex >= game.LevelSet.Count)
                        scrollEndIndex = game.LevelSet.Count - 1;

                    menuScrollGlobalStartOffset = GetGlobalOffset(scrollStartIndex, scrollStartOffset);
                    menuScrollGlobalEndOffset = GetGlobalOffset(scrollEndIndex, 0);

                    menuScrollTime = 0;
                }
                else if (menuScrollTime / MENU_SCROLL_TIME < 1)
                {
                    menuScrollTime += dt;

                    // ----------
                    NormalizeOffset
                    (
                        0,
                        menuScrollGlobalStartOffset + Smooth(menuScrollTime / MENU_SCROLL_TIME) * (menuScrollGlobalEndOffset - menuScrollGlobalStartOffset),
                        out int nIndex,
                        out float nOffset
                    );
                    // ----------

                    MenuLevelIndex = nIndex;
                    MenuOffset = nOffset;
                }
                else
                {
                    MenuOffset = 0; // the smooth function brings it very close to 0 (due to float inaccuracy) at the end of the animation
                                    // but I have to set to 0 here to avoid going into this 'if'
                                    //scrollProgress = 0; // isn't necessary
                }
            }
            else if (menuSnapOngoing)
            {
                if (menuSnapTime / MENU_SNAP_TIME < 1)
                {
                    menuSnapTime += dt;
                    MenuOffset = menuStartOffset + Smooth(menuSnapTime / MENU_SNAP_TIME) * menuLastOffset;
                }
                else
                {
                    menuSnapOngoing = false;
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
            MTPLevelScale = MENU_LEVEL_SCALE + zoominSmooth * (1 - MENU_LEVEL_SCALE);
            MTPMenuOffset = MenuOffset * (1 - zoominSmooth);
            MTPMenuXPadding = MENU_X_PADDING * (1 - zoominSmooth);
            MTPMenuYPadding = MENU_Y_PADDING + zoominSmooth * (ui.LEVEL_VERTICAL_GAP - MENU_Y_PADDING);
        }

        private static float Smooth(double x)
            => (float)Math.Sin(Math.PI / 2 * x); // => (float)((1 - Math.Cos(Math.PI * x)) / 2);

        public void Update(double dt)
        {
            if (MenuToPlaying)
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
                    menuDragStartX = e.GetX();
                    menuDragStartY = e.GetY();
                    menuStartOffset = MenuOffset;
                    menuLastOffset = 0;
                    touchIsTap = true;

                    break;

                case MotionEventActions.Move:
                    menuLastOffset = e.GetX() - menuDragStartX;

                    if (touchIsTap)
                    {
                        if (Math.Pow(menuLastOffset, 2) + Math.Pow(e.GetY() - menuDragStartY, 2) > Math.Pow(ui.TAP_THRESHOLD_DIST, 2)) // tap &&
                        {
                            touchIsTap = false;
                            menuSnapTime = 0;
                            menuSnapOngoing = true;
                        }
                    }
                    else if (!menuSnapOngoing)
                    {
                        MenuOffset = menuStartOffset + menuLastOffset;
                    }

                    break;

                case MotionEventActions.Up:
                    if (touchIsTap)
                    {
                        game.LevelIndex = MenuLevelIndex;

                        MenuToPlaying = true;
                        mtpTime = 0;

                        CalculateMTP();
                    }

                    break;
            }
        }
    }
}