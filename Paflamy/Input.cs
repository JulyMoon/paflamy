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

namespace Paflamy
{
    public static class Input
    {
        public static float MouseX { get; private set; }
        public static float MouseY { get; private set; }

        public static bool Dragging { get; private set; }

        public static int DragTileX { get; private set; }
        public static int DragTileY { get; private set; }

        public static float DragOffsetX { get; private set; }
        public static float DragOffsetY { get; private set; }

        public static float MenuDragStart { get; private set; }
        public static float MenuStartOffset { get; private set; }

        private static void HandleStartTouch(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down &&
                Graphics.StartButton.IntersectsWith(new RectangleF(e.GetX(), e.GetY(), 1, 1)))
            {
                Game.Start();
            }
        }

        private static void HandleGameTouch(MotionEvent e)
        {
            MouseX = e.GetX();
            MouseY = e.GetY();

            if (e.Action != MotionEventActions.Down && e.Action != MotionEventActions.Up)
                return;

            // DEBUG ZONE START

            if (Util.DEBUG)
            {
                if (e.Action == MotionEventActions.Down)
                {
                    Game.NewLevel();
                    Util.Log(Game.Level.Serialized());
                }
                
                return;
            }

            // DEBUG ZONE END

            float xx = MouseX - Graphics.HORI_BORDER;
            float yy = MouseY - Graphics.VERT_BORDER;

            if (xx >= 0 && yy >= 0)
            {
                float ox = xx % Graphics.TileWidth;
                float oy = yy % Graphics.TileHeight;

                int ix = (int)(xx / Graphics.TileWidth);
                int iy = (int)(yy / Graphics.TileHeight);
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
            if (e.Action == MotionEventActions.Down)
            {
                MenuDragStart = e.GetX();
                MenuStartOffset = Graphics.MenuOffset;
            }
            else if (e.Action == MotionEventActions.Move)
            {
                Graphics.MenuOffset = MenuStartOffset + e.GetX() - MenuDragStart;
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
    }
}