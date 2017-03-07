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

namespace Paflamy
{
    public static class Graphics
    {
        public static int SCREEN_WIDTH { get; private set; }
        public static int SCREEN_HEIGHT { get; private set; }

        public const float HORI_BORDER = 0;
        public const float VERT_BORDER = 0;

        public static RectangleF StartButton { get; private set; }
        public static readonly Color StartColor = Color.DodgerBlue;

        public static float TileWidth { get; private set; }
        public static float TileHeight { get; private set; }
        public static float MenuTileSize { get; private set; }

        public static void Init(int width, int height)
        {
            SCREEN_WIDTH = width;
            SCREEN_HEIGHT = height;

            CalcTileSize();

            MenuTileSize = TileWidth;

            float bWidth = SCREEN_WIDTH / 3f;
            float bHeight = (SCREEN_HEIGHT - MenuTileSize * Game.Height) / 3;
            StartButton = new RectangleF(bWidth, MenuTileSize * Game.Height + bHeight, bWidth, bHeight);
            Game.MapChanged += CalcTileSize;
        }

        public static void CalcTileSize()
        {
            TileWidth = (SCREEN_WIDTH - 2 * HORI_BORDER) / Game.Width;
            TileHeight = (SCREEN_HEIGHT - 2 * VERT_BORDER) / Game.Height;
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
            if (!Input.dragging)
                return;

            DrawRectangle(Input.mx - Input.dragOffsetX, Input.my - Input.dragOffsetY, TileWidth, TileHeight, Game.Get(Input.dragTileX, Input.dragTileY));
        }

        private static void DrawMap()
        {
            GL.PushMatrix();
            GL.Translate(HORI_BORDER, VERT_BORDER, 0);

            for (int x = 0; x < Game.Width; ++x)
                for (int y = 0; y < Game.Height; ++y)
                    if (!Input.dragging || x != Input.dragTileX || y != Input.dragTileY)
                        DrawGridTile(x, y);

            GL.PopMatrix();
        }

        private static void DrawGridTile(int x, int y)
        {
            DrawRectangle(x * TileWidth, y * TileHeight, TileWidth, TileHeight, Game.Get(x, y));

            if (Game.IsLocked(x, y))
            {
                GLColor4(Color.Black);

                GL.EnableClientState(All.VertexArray);

                var vertices = new float[]
                {
                    (x + 0.5f) * TileWidth, (y + 0.5f) * TileHeight
                };

                GL.VertexPointer(2, All.Float, 0, vertices);

                GL.DrawArrays(All.Points, 0, 1);
                GL.DisableClientState(All.VertexArray);
            }
        }

        private static void DrawRectangle(float x, float y, float width, float height, Color color)
        {
            GL.PushMatrix();
            GL.Translate(x, y, 0);

            GLColor4(color);

            GL.EnableClientState(All.VertexArray);

            var vertices = new float[]
            {
                0, 0,
                width, 0,
                0, height,
                width, height
            };

            GL.VertexPointer(2, All.Float, 0, vertices);

            GL.DrawArrays(All.TriangleStrip, 0, 4);
            GL.DisableClientState(All.VertexArray);

            GL.PopMatrix();
        }

        private static void DrawPlayingStage()
        {
            DrawMap();
            DrawDrag();
        }

        private static void DrawStartStage()
        {
            for (int x = 0; x < Game.Width; ++x)
                for (int y = 0; y < Game.Height; ++y)
                    DrawRectangle(x * MenuTileSize, y * MenuTileSize, MenuTileSize, MenuTileSize, Game.Get(x, y));

            DrawRectangle(StartButton.X, StartButton.Y, StartButton.Width, StartButton.Height, StartColor);
        }

        public static void OnRender(double dt)
        {
            GL.Clear((uint)All.ColorBufferBit);

            switch (Game.Stage)
            {
                case Stage.Playing: DrawPlayingStage(); break;
                case Stage.Start: DrawStartStage(); break;
                default: throw new Exception();
            }
        }

        private static void GLColor4(Color c)
           => GL.Color4(c.R, c.G, c.B, c.A);
    }
}