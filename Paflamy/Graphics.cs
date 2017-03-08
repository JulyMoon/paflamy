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
        public static int SCREEN_WIDTH { get; private set; }
        public static int SCREEN_HEIGHT { get; private set; }

        public const float HORI_BORDER = 0;
        public const float VERT_BORDER = 0;

        public const float MENU_SCALE = 0.6f;
        public static float MENU_X_PADDING;
        public static float MENU_Y_PADDING;

        public static RectangleF StartButton { get; private set; }
        public static readonly Color StartColor = Color.DodgerBlue;

        public static float TileWidth { get; private set; }
        public static float TileHeight { get; private set; }
        public static float MenuTileSize { get; private set; }

        private static List<Size> tileSizes;

        public static void Init(int width, int height)
        {
            SCREEN_WIDTH = width;
            SCREEN_HEIGHT = height;

            SetTileSize();

            MenuTileSize = TileWidth;
            MENU_X_PADDING = (1 - MENU_SCALE) / 2 * SCREEN_WIDTH;
            MENU_Y_PADDING = 0.15f * SCREEN_HEIGHT;

            float bWidth = SCREEN_WIDTH / 3f;
            float bHeight = (SCREEN_HEIGHT - SCREEN_WIDTH) / 3f;
            StartButton = new RectangleF(bWidth, SCREEN_WIDTH + bHeight, bWidth, bHeight);
            Game.LevelChanged += SetTileSize;

            tileSizes = new List<Size>();
            foreach (var level in Game.LevelSet)
            {
                GetTileSize(level, out float w, out float h);
                tileSizes.Add(new Size(w, h));
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

        public static void OnLoad()
        {
            Util.Log($"w: {SCREEN_WIDTH}, h: {SCREEN_HEIGHT}");

            GL.ClearColor(247f / 255, 239f / 255, 210f / 255, 1);
            GL.PointSize(Math.Min(SCREEN_WIDTH, SCREEN_HEIGHT) * 0.009f);
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, SCREEN_WIDTH, SCREEN_HEIGHT, 0, -1, 1);
            GL.MatrixMode(All.Modelview);
            GL.LoadIdentity();
        }

        private static void DrawDrag()
        {
            GL.PushMatrix();
            GL.Translate(Input.MouseX - Input.DragOffsetX, Input.MouseY - Input.DragOffsetY, 0);
            GL.Scale(TileWidth, TileHeight, 1);
            
            DrawTile(Game.Level, Input.DragTileX, Input.DragTileY);

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
                    if (!Input.Dragging || x != Input.DragTileX || y != Input.DragTileY)
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

        private static void DrawPlayingStage()
        {
            DrawLevel(Game.Level, HORI_BORDER, VERT_BORDER, TileWidth, TileHeight);

            if (Input.Dragging)
                DrawDrag();
        }

        private static void DrawStartStage()
        {
            DrawLevel(Game.Level, 0, 0, MenuTileSize, MenuTileSize);
            DrawStartButton();
        }

        private static void DrawStartButton()
        {
            GL.PushMatrix();
            GL.Translate(StartButton.X, StartButton.Y, 0);
            GL.Scale(StartButton.Width, StartButton.Height, 1);

            DrawRectangle(StartColor);

            GL.PopMatrix();
        }

        private static void DrawMenuStage()
        {
            DrawLevel(Game.LevelSet[0], MENU_X_PADDING, MENU_Y_PADDING, tileSizes[0].Width, tileSizes[0].Height, MENU_SCALE);
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

        private static void GLColor4(Color c)
           => GL.Color4(c.R, c.G, c.B, c.A);
    }
}