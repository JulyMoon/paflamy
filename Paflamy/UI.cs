using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;

using Android.Views;
using Android.Content;
using Android.Util;
using System.Collections.Generic;
using Size = System.Drawing.SizeF;
using System.Linq;

namespace Paflamy
{
    public static class UI
    {
        public static int SCREEN_WIDTH { get; private set; }
        public static int SCREEN_HEIGHT { get; private set; }

        public const float HORI_BORDER = 0;
        public const float VERT_BORDER = 0;

        public const float MENU_LEVEL_SCALE = 0.6f;
        public static float MENU_X_PADDING;
        public static float MENU_Y_PADDING;
        public static float MENU_LEVEL_MARGIN;
        public static float MENU_LEVEL_DIST;
        public static float MENU_LEVEL_WIDTH;
        public const int MENU_NEIGHBOR_COUNT = 2;
        public const double MENU_SCROLL_TIME = 0.5;

        public static RectangleF StartButton { get; private set; }
        public static readonly Color StartColor = Color.DodgerBlue;

        public static float TileWidth { get; private set; }
        public static float TileHeight { get; private set; }
        public static float MenuTileSize { get; private set; }
        public static List<Size> TileSizes { get; private set; }

        public static int MenuLevelIndex { get; private set; }
        public static float MenuOffset { get; set; }

        public static float MouseX { get; private set; }
        public static float MouseY { get; private set; }

        public static bool Dragging { get; private set; }

        public static int DragTileX { get; private set; }
        public static int DragTileY { get; private set; }

        public static float DragOffsetX { get; private set; }
        public static float DragOffsetY { get; private set; }

        private static float menuDragStartX;
        private static float menuStartOffset;
        private static float menuLastOffset;

        private static bool prevDragging;
        private static bool tap;

        private static double menuScrollTime;
        private static float menuScrollGlobalStartOffset;
        private static float menuScrollGlobalEndOffset;

        public static void Init(int width, int height)
        {
            SCREEN_WIDTH = width;
            SCREEN_HEIGHT = height;

            SetTileSize();

            MenuTileSize = TileWidth;
            MENU_X_PADDING = (1 - MENU_LEVEL_SCALE) / 2 * SCREEN_WIDTH;
            MENU_Y_PADDING = 0.15f * SCREEN_HEIGHT;
            MENU_LEVEL_MARGIN = 0.09f * SCREEN_WIDTH;
            MENU_LEVEL_WIDTH = SCREEN_WIDTH * MENU_LEVEL_SCALE;
            MENU_LEVEL_DIST = MENU_LEVEL_WIDTH + MENU_LEVEL_MARGIN;

            float bWidth = SCREEN_WIDTH / 3f;
            float bHeight = (SCREEN_HEIGHT - SCREEN_WIDTH) / 3f;
            StartButton = new RectangleF(bWidth, SCREEN_WIDTH + bHeight, bWidth, bHeight);
            Game.LevelChanged += SetTileSize;

            TileSizes = new List<Size>();
            foreach (var level in Game.LevelSet)
            {
                GetTileSize(level, out float w, out float h);
                TileSizes.Add(new Size(w, h));
            }
        }

        private static void SetTileSize()
        {
            GetTileSize(Game.Level, out float tileWidth, out float tileHeight);
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        private static void GetTileSize(Level level, out float width, out float height)
        {
            width = (SCREEN_WIDTH - 2 * HORI_BORDER) / level.Width;
            height = (SCREEN_HEIGHT - 2 * VERT_BORDER) / level.Height;
        }

        private static float Smooth(double x)
             => (float)Math.Sin(Math.PI / 2 * x); // => (float)((1 - Math.Cos(Math.PI * x)) / 2);

        private static void NormalizeOffset(int index, float offset, out int nIndex, out float nOffset)
        {
            float adjOffset = -offset + MENU_LEVEL_DIST / 2;

            nIndex = index + (int)(adjOffset / MENU_LEVEL_DIST) + (adjOffset < 0 ? -1 : 0);
            nOffset = -((adjOffset % MENU_LEVEL_DIST) + (adjOffset < 0 ? MENU_LEVEL_DIST : 0) - MENU_LEVEL_DIST / 2);
        }

        private static float GetGlobalOffset(int index, float offset)
            => -index * MENU_LEVEL_DIST + offset;

        private static void UpdateMenuStage(double dt)
        {
            if (!Dragging && MenuOffset != 0)
            {
                if (prevDragging && menuLastOffset != 0)
                {
                    NormalizeOffset(MenuLevelIndex, MenuOffset, out int scrollStartIndex, out float scrollStartOffset);
                    int scrollEndIndex = scrollStartIndex + (scrollStartIndex == MenuLevelIndex ? -Math.Sign(menuLastOffset) : 0);

                    if (scrollEndIndex < 0)
                        scrollEndIndex = 0;
                    else if (scrollEndIndex >= Game.LevelSet.Count)
                        scrollEndIndex = Game.LevelSet.Count - 1;

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

            prevDragging = Dragging;
        }

        public static void OnUpdate(double dt)
        {
            switch (Game.Stage)
            {
                case Stage.Playing: break;
                case Stage.Menu: UpdateMenuStage(dt); break;
                case Stage.Start: break;
                default: throw new Exception();
            }
        }

        public static bool OnTouch(MotionEvent e)
        {
            switch (Game.Stage)
            {
                case Stage.Playing: HandleGameTouch(e); break;
                case Stage.Start: HandleStartTouch(e); break;
                case Stage.Menu: HandleMenuTouch(e); break;
                default: throw new Exception();
            }

            return true;
        }

        private static void HandleStartTouch(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down && StartButton.IntersectsWith(new RectangleF(e.GetX(), e.GetY(), 1, 1)))
                Game.Start();
        }

        private static void HandleGameTouch(MotionEvent e)
        {
            MouseX = e.GetX();
            MouseY = e.GetY();

            if (e.Action != MotionEventActions.Down && e.Action != MotionEventActions.Up)
                return;

            float xx = MouseX - HORI_BORDER;
            float yy = MouseY - VERT_BORDER;

            if (xx >= 0 && yy >= 0)
            {
                float ox = xx % TileWidth;
                float oy = yy % TileHeight;

                int ix = (int)(xx / TileWidth);
                int iy = (int)(yy / TileHeight);
                if (ix < Game.Level.Width && iy < Game.Level.Height && !Game.Level.IsLocked(ix, iy))
                {
                    if (e.Action == MotionEventActions.Down)
                    {
                        Dragging = true;
                        DragTileX = ix;
                        DragTileY = iy;
                        DragOffsetX = ox;
                        DragOffsetY = oy;
                    }
                    else if (Dragging) // this is NOT redundant
                                       // start dragging a locked tile and drop on a regular one
                                       // in that case the event is up but we're not dragging anything
                    {
                        Dragging = false;

                        if (DragTileX != ix || DragTileY != iy)
                        {
                            Game.Level.Swap(DragTileX, DragTileY, ix, iy);
                            if (Game.Level.IsSolved())
                                Util.Log("SOLVED");
                        }
                    }

                }
                else if (Dragging)
                {
                    Dragging = false;
                }
            }
            else if (Dragging)
            {
                Dragging = false;
            }
        }

        private static void HandleMenuTouch(MotionEvent e)
        {
            Dragging = e.Action != MotionEventActions.Up;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    menuDragStartX = e.GetX();
                    menuStartOffset = MenuOffset;
                    menuLastOffset = 0;
                    tap = true;

                    break;

                case MotionEventActions.Move:
                    menuLastOffset = e.GetX() - menuDragStartX;
                    MenuOffset = menuStartOffset + menuLastOffset;
                    tap = false;

                    break;

                case MotionEventActions.Up:
                    if (tap)
                    {
                        Game.Play(MenuLevelIndex);
                    }

                    break;
            }
        }
    }
}