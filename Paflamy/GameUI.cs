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
    public class GameUI
    {
        public static readonly Color BackgroundColor = Color.Black;

        public float MouseX { get; private set; }
        public float MouseY { get; private set; }

        public bool Dragging { get; private set; }

        public int DragTileX { get; private set; }
        public int DragTileY { get; private set; }

        public float DragOffsetX { get; private set; }
        public float DragOffsetY { get; private set; }

        private readonly UI ui;
        private readonly Game game;

        public GameUI(UI ui, Game game)
        {
            this.ui = ui;
            this.game = game;
        }

        public void HandleTouch(MotionEvent e)
        {
            MouseX = e.GetX();
            MouseY = e.GetY();

            if (e.Action != MotionEventActions.Down && e.Action != MotionEventActions.Up)
                return;

            float xx = MouseX;
            float yy = MouseY - ui.LEVEL_VERTICAL_GAP;

            if (xx >= 0 && yy >= 0)
            {
                float ox = xx % ui.TileSizes[game.LevelIndex].Width;
                float oy = yy % ui.TileSizes[game.LevelIndex].Height;

                int ix = (int)(xx / ui.TileSizes[game.LevelIndex].Width);
                int iy = (int)(yy / ui.TileSizes[game.LevelIndex].Height);
                if (ix < game.LevelSet[game.LevelIndex].Width && iy < game.LevelSet[game.LevelIndex].Height && !game.LevelSet[game.LevelIndex].IsLocked(ix, iy))
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
                            game.LevelSet[game.LevelIndex].Swap(DragTileX, DragTileY, ix, iy);
                            if (game.LevelSet[game.LevelIndex].IsSolved())
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
    }
}