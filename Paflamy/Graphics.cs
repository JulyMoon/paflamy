using System;
using System.Drawing;
using OpenTK.Graphics.ES11;

namespace Paflamy
{
    public class Graphics
    {
        private readonly Logic logic;
        private readonly UI ui;

        public Graphics(Logic logic, UI ui)
        {
            this.logic = logic;
            this.ui = ui;
        }

        public void OnLoad()
        {
            GL.ClearColor(247f / 255, 239f / 255, 210f / 255, 1);
            GL.PointSize(Math.Min(ui.SCREEN_WIDTH, ui.SCREEN_HEIGHT) * 0.009f);
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, ui.SCREEN_WIDTH, ui.SCREEN_HEIGHT, 0, -1, 1);
            GL.MatrixMode(All.Modelview);
            GL.LoadIdentity();
        }

        private void DrawDrag()
        {
            GL.PushMatrix();
            GL.Translate(ui.MouseX - ui.DragOffsetX, ui.MouseY - ui.DragOffsetY, 0);
            GL.Scale(ui.TileWidth, ui.TileHeight, 1);

            DrawTile(logic.Level, ui.DragTileX, ui.DragTileY);

            GL.PopMatrix();
        }

        private void DrawLevel(Level level, float xOffset, float yOffset, float tileWidth, float tileHeight, float scale = 1)
        {
            GL.PushMatrix();
            GL.Translate(xOffset, yOffset, 0);
            GL.Scale(tileWidth, tileHeight, 1);
            GL.Scale(scale, scale, 1);

            for (int x = 0; x < level.Width; ++x)
                for (int y = 0; y < level.Height; ++y)
                    if (logic.Stage != Stage.Playing || !ui.Dragging || x != ui.DragTileX || y != ui.DragTileY)
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

        private void DrawStartButton()
        {
            GL.PushMatrix();
            GL.Translate(ui.StartButton.X, ui.StartButton.Y, 0);
            GL.Scale(ui.StartButton.Width, ui.StartButton.Height, 1);

            DrawRectangle(UI.StartColor);

            GL.PopMatrix();
        }

        public void OnRender(double dt)
        {
            GL.Clear((uint)All.ColorBufferBit);

            switch (logic.Stage)
            {
                case Stage.Playing: DrawPlayingStage(); break;
                case Stage.Menu: DrawMenuStage(); break;
                case Stage.Start: DrawStartStage(); break;
                default: throw new Exception();
            }
        }

        private void DrawPlayingStage()
        {
            DrawLevel(logic.Level, UI.HORI_BORDER, UI.VERT_BORDER, ui.TileWidth, ui.TileHeight);

            if (ui.Dragging)
                DrawDrag();
        }

        private void DrawStartStage()
        {
            DrawLevel(logic.Level, 0, 0, ui.MenuTileSize, ui.MenuTileSize);
            DrawStartButton();
        }

        private void DrawMenuStage()
        {
            for (int j = -UI.MENU_NEIGHBOR_COUNT; j <= UI.MENU_NEIGHBOR_COUNT; ++j)
            {
                int i = ui.MenuLevelIndex + j;
                if (i >= 0 && i < logic.LevelSet.Count)
                {
                    DrawLevel(logic.LevelSet[i],
                              ui.MENU_X_PADDING + ui.MENU_LEVEL_DIST * j + ui.MenuOffset,
                              ui.MENU_Y_PADDING,
                              ui.TileSizes[i].Width,
                              ui.TileSizes[i].Height,
                              UI.MENU_LEVEL_SCALE);
                }
            }
        }

        private static void GLColor4(Color c)
           => GL.Color4(c.R, c.G, c.B, c.A);
    }
}