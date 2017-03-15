using System;
using System.Drawing;

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

        public const float MENU_LEVEL_SCALE = 0.6f;
        public const int MENU_NEIGHBOR_COUNT = 2;
        public const double MENU_SCROLL_TIME = 0.5;
        public const double MENU_SNAP_TIME = 0.1;
        public float MENU_X_PADDING { get; private set; }
        public float MENU_Y_PADDING { get; private set; }
        public float MENU_LEVEL_MARGIN { get; private set; }
        public float MENU_LEVEL_DIST { get; private set; }
        public float MENU_LEVEL_WIDTH { get; private set; }
        public double TAP_THRESHOLD_DIST { get; private set; }

        public RectangleF StartButton { get; private set; }
        public static readonly Color StartColor = Color.DodgerBlue;
        public static readonly Color PlayingColor = Color.Black;
        public static readonly Color MenuColor = Color.FromArgb(247, 239, 210);

        public float TileWidth { get; private set; }
        public float TileHeight { get; private set; }
        public float MenuTileSize { get; private set; }
        public List<Size> TileSizes { get; private set; }

        public int MenuLevelIndex { get; private set; }
        public float MenuOffset { get; set; }

        public float MouseX { get; private set; }
        public float MouseY { get; private set; }

        public bool Dragging { get; private set; }

        public int DragTileX { get; private set; }
        public int DragTileY { get; private set; }

        public float DragOffsetX { get; private set; }
        public float DragOffsetY { get; private set; }

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

        private readonly Logic logic;

        public UI(Logic logic, int width, int height)
        {
            this.logic = logic;

            SCREEN_WIDTH = width;
            SCREEN_HEIGHT = height;

            SetTileSize();

            MenuTileSize = TileWidth;
            MENU_X_PADDING = (1 - MENU_LEVEL_SCALE) / 2 * SCREEN_WIDTH;
            MENU_Y_PADDING = 0.15f * SCREEN_HEIGHT;
            MENU_LEVEL_MARGIN = 0.09f * SCREEN_WIDTH;
            MENU_LEVEL_WIDTH = SCREEN_WIDTH * MENU_LEVEL_SCALE;
            MENU_LEVEL_DIST = MENU_LEVEL_WIDTH + MENU_LEVEL_MARGIN;
            TAP_THRESHOLD_DIST = 0.05f * SCREEN_WIDTH;
            LEVEL_VERTICAL_GAP = 0.05f * SCREEN_HEIGHT;

            float bWidth = SCREEN_WIDTH / 3f;
            float bHeight = (SCREEN_HEIGHT - SCREEN_WIDTH) / 3f;
            StartButton = new RectangleF(bWidth, SCREEN_WIDTH + bHeight, bWidth, bHeight);
            logic.LevelChanged += SetTileSize;

            TileSizes = new List<Size>();
            foreach (var level in logic.LevelSet)
            {
                GetTileSize(level, out float w, out float h);
                TileSizes.Add(new Size(w, h));
            }
        }

        private void SetTileSize()
        {
            GetTileSize(logic.Level, out float tileWidth, out float tileHeight);
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        private void GetTileSize(Level level, out float width, out float height)
        {
            width = (float)SCREEN_WIDTH / level.Width;
            height = (SCREEN_HEIGHT - 2 * LEVEL_VERTICAL_GAP) / level.Height;
        }

        private static float Smooth(double x)
             => (float)Math.Sin(Math.PI / 2 * x); // => (float)((1 - Math.Cos(Math.PI * x)) / 2);

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
            if (!Dragging && MenuOffset != 0)
            {
                if (prevDragging && menuLastOffset != 0)
                {
                    menuSnapOngoing = false;

                    NormalizeOffset(MenuLevelIndex, MenuOffset, out int scrollStartIndex, out float scrollStartOffset);
                    int scrollEndIndex = scrollStartIndex + (scrollStartIndex == MenuLevelIndex ? -Math.Sign(menuLastOffset) : 0);

                    if (scrollEndIndex < 0)
                        scrollEndIndex = 0;
                    else if (scrollEndIndex >= logic.LevelSet.Count)
                        scrollEndIndex = logic.LevelSet.Count - 1;

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

            prevDragging = Dragging;
        }

        public void OnUpdate(double dt)
        {
            switch (logic.Stage)
            {
                case Stage.Playing: break;
                case Stage.Menu: UpdateMenuStage(dt); break;
                case Stage.Start: break;
                default: throw new Exception();
            }
        }

        public bool OnTouch(MotionEvent e)
        {
            switch (logic.Stage)
            {
                case Stage.Playing: HandleGameTouch(e); break;
                case Stage.Start: HandleStartTouch(e); break;
                case Stage.Menu: HandleMenuTouch(e); break;
                default: throw new Exception();
            }

            return true;
        }

        private void HandleStartTouch(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down && StartButton.IntersectsWith(new RectangleF(e.GetX(), e.GetY(), 1, 1)))
                logic.Start();
        }

        private void HandleGameTouch(MotionEvent e)
        {
            MouseX = e.GetX();
            MouseY = e.GetY();

            if (e.Action != MotionEventActions.Down && e.Action != MotionEventActions.Up)
                return;

            float xx = MouseX;
            float yy = MouseY - LEVEL_VERTICAL_GAP;

            if (xx >= 0 && yy >= 0)
            {
                float ox = xx % TileWidth;
                float oy = yy % TileHeight;

                int ix = (int)(xx / TileWidth);
                int iy = (int)(yy / TileHeight);
                if (ix < logic.Level.Width && iy < logic.Level.Height && !logic.Level.IsLocked(ix, iy))
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
                            logic.Level.Swap(DragTileX, DragTileY, ix, iy);
                            if (logic.Level.IsSolved())
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

        private void HandleMenuTouch(MotionEvent e)
        {
            Dragging = e.Action != MotionEventActions.Up;

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
                        if (Math.Pow(menuLastOffset, 2) + Math.Pow(e.GetY() - menuDragStartY, 2) > Math.Pow(TAP_THRESHOLD_DIST, 2)) // tap &&
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
                        logic.Play(MenuLevelIndex);
                    }

                    break;
            }
        }
    }
}