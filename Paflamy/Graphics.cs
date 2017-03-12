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
    public static class Graphics
    {
        public static void OnLoad()
        {
            //Util.Log($"w: {UI.SCREEN_WIDTH}, h: {UI.SCREEN_HEIGHT}");

            GL.ClearColor(247f / 255, 239f / 255, 210f / 255, 1);
            GL.PointSize(Math.Min(UI.SCREEN_WIDTH, UI.SCREEN_HEIGHT) * 0.009f);
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, UI.SCREEN_WIDTH, UI.SCREEN_HEIGHT, 0, -1, 1);
            GL.MatrixMode(All.Modelview);
            GL.LoadIdentity();
        }

        private static void DrawDrag()
        {
            GL.PushMatrix();
            GL.Translate(UI.MouseX - UI.DragOffsetX, UI.MouseY - UI.DragOffsetY, 0);
            GL.Scale(UI.TileWidth, UI.TileHeight, 1);

            DrawTile(Game.Level, UI.DragTileX, UI.DragTileY);

            GL.PopMatrix();
        }

        private static void DrawLevel(Level level, float xOffset, float yOffset, float tileWidth, float tileHeight, float scale = 1)
        {
            GL.PushMatrix();
            GL.Translate(xOffset, yOffset, 0);
            GL.Scale(tileWidth, tileHeight, 1);
            GL.Scale(scale, scale, 1);

            for (int x = 0; x < level.Width; ++x)
                for (int y = 0; y < level.Height; ++y)
                    if (Game.Stage != Stage.Playing || !UI.Dragging || x != UI.DragTileX || y != UI.DragTileY)
                    {
                        GL.PushMatrix();
                        GL.Translate(x, y, 0);

                        DrawTile(level, x, y);

                        GL.PopMatrix();
                    }

            GL.PopMatrix();
        }

        private static void DrawTile(Level level, int x, int y)
        {
            DrawRectangle(level.Get(x, y));

            if (level.IsLocked(x, y))
                DrawLockedDot();
        }

        private static void DrawLockedDot()
        {
            GLColor4(Color.Black);

            GL.EnableClientState(All.VertexArray);

            var vertices = new float[] { 0.5f, 0.5f };

            GL.VertexPointer(2, All.Float, 0, vertices);

            GL.DrawArrays(All.Points, 0, 1);
            GL.DisableClientState(All.VertexArray);
        }

        private static void DrawRectangle(Color color)
        {
            GLColor4(color);

            GL.EnableClientState(All.VertexArray);

            var vertices = new float[]
            {
                0, 0,
                1, 0,
                0, 1,
                1, 1
            };

            GL.VertexPointer(2, All.Float, 0, vertices);

            GL.DrawArrays(All.TriangleStrip, 0, 4);
            GL.DisableClientState(All.VertexArray);
        }

        private static void DrawStartButton()
        {
            GL.PushMatrix();
            GL.Translate(UI.StartButton.X, UI.StartButton.Y, 0);
            GL.Scale(UI.StartButton.Width, UI.StartButton.Height, 1);

            DrawRectangle(UI.StartColor);

            GL.PopMatrix();
        }

        public static void OnRender(double dt)
        {
            GL.Clear((uint)All.ColorBufferBit);

            switch (Game.Stage)
            {
                case Stage.Playing: DrawPlayingStage(); break;
                case Stage.Menu: DrawMenuStage(); break;
                case Stage.Start: DrawStartStage(); break;
                default: throw new Exception();
            }
        }

        private static void DrawPlayingStage()
        {
            DrawLevel(Game.Level, UI.HORI_BORDER, UI.VERT_BORDER, UI.TileWidth, UI.TileHeight);

            if (UI.Dragging)
                DrawDrag();
        }

        private static void DrawStartStage()
        {
            DrawLevel(Game.Level, 0, 0, UI.MenuTileSize, UI.MenuTileSize);
            DrawStartButton();
        }

        private static void DrawMenuStage()
        {
            for (int j = -UI.MENU_NEIGHBOR_COUNT; j <= UI.MENU_NEIGHBOR_COUNT; ++j)
            {
                int i = UI.MenuLevelIndex + j;
                if (i >= 0 && i < Game.LevelSet.Count)
                {
                    DrawLevel(Game.LevelSet[i],
                              UI.MENU_X_PADDING + UI.MENU_LEVEL_DIST * j + UI.MenuOffset,
                              UI.MENU_Y_PADDING,
                              UI.TileSizes[i].Width,
                              UI.TileSizes[i].Height,
                              UI.MENU_LEVEL_SCALE);
                }
            }
        }

        private static void GLColor4(Color c)
           => GL.Color4(c.R, c.G, c.B, c.A);
    }
}