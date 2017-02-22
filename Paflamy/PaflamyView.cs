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
    class PaflamyView : AndroidGameView
    {
        private const string DBG_TAG = "PAFLAMY";

        private readonly int SCREEN_WIDTH;
        private readonly int SCREEN_HEIGHT;

        private const float HORI_BORDER = 0;
        private const float VERT_BORDER = 0;

        private float tileWidth;
        private float tileHeight;
        
        private float mx, my;

        private bool dragging;
        private int dragTileX, dragTileY;
        private float dragOffsetX, dragOffsetY;

        private Game game = new Game();

        public PaflamyView(Context context) : base(context)
        {
            SCREEN_WIDTH = Resources.DisplayMetrics.WidthPixels;
            SCREEN_HEIGHT = Resources.DisplayMetrics.HeightPixels;

            tileWidth = (SCREEN_WIDTH - 2 * HORI_BORDER) / game.Width;
            tileHeight = (SCREEN_HEIGHT - 2 * VERT_BORDER) / game.Height;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Log($"w: {SCREEN_WIDTH}, h: {SCREEN_HEIGHT}");

            GL.ClearColor(0, 0, 0, 1);
            GL.PointSize(Math.Min(SCREEN_WIDTH, SCREEN_HEIGHT) * 0.009f);
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, SCREEN_WIDTH, SCREEN_HEIGHT, 0, -1, 1);
            GL.MatrixMode(All.Modelview);
            GL.LoadIdentity();

            Run();
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            mx = e.GetX();
            my = e.GetY();

            if (e.Action != MotionEventActions.Down && e.Action != MotionEventActions.Up)
                return true;

            float xx = mx - HORI_BORDER;
            float yy = my - VERT_BORDER;

            if (xx >= 0 && yy >= 0)
            {
                float ox = xx % tileWidth;
                float oy = yy % tileHeight;

                int ix = (int)(xx / tileWidth);
                int iy = (int)(yy / tileHeight);
                if (ix < game.Width && iy < game.Height && !game.IsLocked(ix, iy))
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
                            game.Swap(dragTileX, dragTileY, ix, iy);
                            if (game.IsSolved)
                                Log("SOLVED");
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

            return true;
        }

        private void DrawDrag()
        {
            if (!dragging)
                return;

            DrawTile(mx - dragOffsetX, my - dragOffsetY, game.Get(dragTileX, dragTileY));
        }

        private void DrawMap()
        {
            GL.PushMatrix();
            GL.Translate(HORI_BORDER, VERT_BORDER, 0);

            for (int x = 0; x < game.Width; ++x)
                for (int y = 0; y < game.Height; ++y)
                    if (!dragging || x != dragTileX || y != dragTileY)
                        DrawGridTile(x, y);

            GL.PopMatrix();
        }

        private void DrawGridTile(int x, int y)
        {
            DrawTile(x * tileWidth, y * tileHeight, game.Get(x, y));

            if (game.IsLocked(x, y))
            {
                GLColor4(Color.Black);

                GL.EnableClientState(All.VertexArray);

                var vertices = new float[]
                {
                    (x + 0.5f) * tileWidth, (y + 0.5f) * tileHeight
                };

                GL.VertexPointer(2, All.Float, 0, vertices);

                GL.DrawArrays(All.Points, 0, 1);
                GL.DisableClientState(All.VertexArray);
            }
        }

        private void DrawTile(float x, float y, Color tile)
        {
            GL.PushMatrix();
            GL.Translate(x, y, 0);

            GLColor4(tile);

            GL.EnableClientState(All.VertexArray);

            var vertices = new float[]
            {
                0, 0,
                tileWidth, 0,
                0, tileHeight,
                tileWidth, tileHeight
            };

            GL.VertexPointer(2, All.Float, 0, vertices);

            GL.DrawArrays(All.TriangleStrip, 0, 4);
            GL.DisableClientState(All.VertexArray);

            GL.PopMatrix();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear((uint)All.ColorBufferBit);

            DrawMap();
            DrawDrag();

            SwapBuffers();
        }

        private static void GLColor4(Color c)
            => GL.Color4(c.R, c.G, c.B, c.A);

        public static void Log(string s)
            => Android.Util.Log.Verbose(DBG_TAG, s);

        protected override void CreateFrameBuffer()
        {
            try
            {
                Log("Loading with default settings");
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            try
            {
                Log("Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            throw new Exception("Can't load egl, aborting");
        }
    }
}
