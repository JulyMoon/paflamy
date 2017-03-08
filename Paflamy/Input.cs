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
        public static float mx { get; private set; }
        public static float my { get; private set; }

        public static bool dragging { get; private set; }

        public static int dragTileX { get; private set; }
        public static int dragTileY { get; private set; }

        public static float dragOffsetX { get; private set; }
        public static float dragOffsetY { get; private set; }

        private static void HandleMenuTouch(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down &&
                Graphics.StartButton.IntersectsWith(new RectangleF(e.GetX(), e.GetY(), 1, 1)))
            {
                Game.Play();
            }
        }

        private static void HandleGameTouch(MotionEvent e)
        {
            mx = e.GetX();
            my = e.GetY();

            if (e.Action != MotionEventActions.Down && e.Action != MotionEventActions.Up)
                return;

            Util.Log(Game.GetLevelString());

            float xx = mx - Graphics.HORI_BORDER;
            float yy = my - Graphics.VERT_BORDER;

            if (xx >= 0 && yy >= 0)
            {
                float ox = xx % Graphics.TileWidth;
                float oy = yy % Graphics.TileHeight;

                int ix = (int)(xx / Graphics.TileWidth);
                int iy = (int)(yy / Graphics.TileHeight);
                if (ix < Game.Width && iy < Game.Height && !Game.IsLocked(ix, iy))
                {
                    if (e.Action == MotionEventActions.Down)
                    {
                        dragging = true;
                        dragTileX = ix;
                        dragTileY = iy;
                        dragOffsetX = ox;
                        dragOffsetY = oy;
                    }
                    else if (dragging) // this is NOT redundant
                                       // start dragging a locked tile and drop on a regular one
                                       // in that case the event is up but we're not dragging anything
                    {
                        dragging = false;

                        if (dragTileX != ix || dragTileY != iy)
                        {
                            Game.Swap(dragTileX, dragTileY, ix, iy);
                            if (Game.IsSolved)
                                Util.Log("SOLVED");
                        }
                    }

                }
                else if (dragging)
                {
                    dragging = false;
                }
            }
            else if (dragging)
            {
                dragging = false;
            }
        }

        public static bool OnTouch(MotionEvent e)
        {
            switch (Game.Stage)
            {
                case Stage.Playing: HandleGameTouch(e); break;
                case Stage.Start: HandleMenuTouch(e); break;
                default: throw new Exception();
            }

            return true;
        }
    }
}