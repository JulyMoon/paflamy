using System;
using System.Drawing;
using OpenTK.Graphics.ES11;

namespace Paflamy
{
    public class Graphics
    {
        private readonly Game game;
        private readonly UI ui;

        public Graphics(Game game, UI ui)
        {
            this.game = game;
            this.ui = ui;
            ui.StageChanged += HandleStageChange;
        }

        private void HandleStageChange()
            => GLClearColor(ui.PlayingStage ? GameUI.BackgroundColor : MenuUI.BackgroundColor);

        private static void GLClearColor(Color c)
            => GL.ClearColor(c.R / (float)byte.MaxValue, c.G / (float)byte.MaxValue, c.B / (float)byte.MaxValue, 1);

        public void OnLoad()
        {
            HandleStageChange();
            GL.PointSize(ui.SCREEN_WIDTH * 0.009f);
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, ui.SCREEN_WIDTH, ui.SCREEN_HEIGHT, 0, -1, 1);
            GL.MatrixMode(All.Modelview);
            GL.Enable(All.Blend);
            GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);
            GL.LoadIdentity();
        }

        private void DrawDrag()
        {
            GL.PushMatrix();
            GL.Translate(ui.Game.MouseX - ui.Game.DragOffsetX, ui.Game.MouseY - ui.Game.DragOffsetY, 0);
            GL.Scale(ui.TileSizes[game.LevelIndex].Width, ui.TileSizes[game.LevelIndex].Height, 1);

            DrawTile(game.LevelSet[game.LevelIndex], ui.Game.DragTileX, ui.Game.DragTileY);

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
                    if (!ui.PlayingStage || !ui.Game.Dragging || x != ui.Game.DragTileX || y != ui.Game.DragTileY)
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
            GL.Translate(ui.Menu.StartButton.X, ui.Menu.StartButton.Y, 0);
            GL.Scale(ui.Menu.StartButton.Width, ui.Menu.StartButton.Height, 1);

            DrawRectangle(MenuUI.StartButtonColor);

            GL.PopMatrix();
        }

        public void OnRender(double dt)
        {
            GL.Clear((uint)All.ColorBufferBit);

            if (ui.PlayingStage)
                DrawPlayingStage();
            else if (ui.Menu.StartStage)
                DrawStartStage();
            else if (ui.Menu.MenuToPlaying)
                DrawMTP();
            else
                DrawMenuStage();
        }

        private void DrawPlayingStage()
        {
            DrawLevel(game.LevelSet[game.LevelIndex], 0, ui.LEVEL_VERTICAL_GAP, ui.TileSizes[game.LevelIndex].Width, ui.TileSizes[game.LevelIndex].Height);

            if (ui.Game.Dragging)
                DrawDrag();
        }

        private void DrawStartStage()
        {
            DrawLevel(ui.Menu.StartLevel, 0, 0, ui.Menu.MenuTileSize, ui.Menu.MenuTileSize);
            DrawStartButton();
        }

        private void DrawMenuStage()
        {
            for (int j = -MenuUI.MENU_NEIGHBOR_COUNT; j <= MenuUI.MENU_NEIGHBOR_COUNT; ++j)
            {
                int i = ui.Menu.MenuLevelIndex + j;
                if (i >= 0 && i < game.LevelSet.Count)
                {
                    DrawLevel(game.LevelSet[i],
                              ui.Menu.MENU_X_PADDING + ui.Menu.MENU_LEVEL_DIST * j + ui.Menu.MenuOffset,
                              ui.Menu.MENU_Y_PADDING,
                              ui.TileSizes[i].Width,
                              ui.TileSizes[i].Height,
                              MenuUI.MENU_LEVEL_SCALE);
                }
            }
        }

        private void DrawMTP()
        {
            DrawMenuStage();

            GL.PushMatrix();
            GL.Scale(ui.SCREEN_WIDTH, ui.SCREEN_HEIGHT, 1);

            DrawRectangle(Color.FromArgb((int)(ui.Menu.MTPBackgroundCoverAlpha * Byte.MaxValue), GameUI.BackgroundColor));

            GL.PopMatrix();

            DrawLevel(game.LevelSet[game.LevelIndex],
                      ui.Menu.MTPMenuXPadding + ui.Menu.MTPMenuOffset,
                      ui.Menu.MTPMenuYPadding,
                      ui.TileSizes[game.LevelIndex].Width,
                      ui.TileSizes[game.LevelIndex].Height,
                      ui.Menu.MTPLevelScale);
        }

        private static void GLColor4(Color c)
           => GL.Color4(c.R, c.G, c.B, c.A);
    }
}