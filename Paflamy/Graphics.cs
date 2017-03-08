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
            float bHeight = (SCREEN_HEIGHT - MenuTileSize * Game.Level.Height) / 3;
            StartButton = new RectangleF(bWidth, MenuTileSize * Game.Level.Height + bHeight, bWidth, bHeight);
            Game.LevelChanged += CalcTileSize;
        }

        public static void CalcTileSize()
        {
            TileWidth = (SCREEN_WIDTH - 2 * HORI_BORDER) / Game.Level.Width;
            TileHeight = (SCREEN_HEIGHT - 2 * VERT_BORDER) / Game.Level.Height;
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
            GL.Scale(TileWidth, TileHeight, 0);
            
            DrawTile(Input.DragTileX, Input.DragTileY);

            GL.PopMatrix();
        }

        private static void DrawLevel(Level level, float xOffset, float yOffset, float tileWidth, float tileHeight)
        {
            GL.PushMatrix();
            GL.Translate(xOffset, yOffset, 0);
            GL.Scale(tileWidth, tileWidth, 0);

            for (int x = 0; x < Game.Level.Width; ++x)
                for (int y = 0; y < Game.Level.Height; ++y)
                    if (!Input.Dragging || x != Input.DragTileX || y != Input.DragTileY)
                    {
                        GL.PushMatrix();
                        GL.Translate(x, y, 0);

                        DrawTile(x, y);

                        GL.PopMatrix();
                    }

            GL.PopMatrix();
        }

        private static void DrawTile(int x, int y)
        {
            DrawRectangle(Game.Level.Get(x, y));

            if (Game.Level.IsLocked(x, y))
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

            GL.PushMatrix();
            GL.Translate(StartButton.X, StartButton.Y, 0);
            GL.Scale(StartButton.Width, StartButton.Height, 0);

            DrawRectangle(StartColor);

            GL.PopMatrix();
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