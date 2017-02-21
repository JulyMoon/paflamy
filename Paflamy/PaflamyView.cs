using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using Android.Views;
using Android.Content;
using Android.Util;
using System.Drawing;

namespace Paflamy
{
    class PaflamyView : AndroidGameView
    {
        private const string DBG_TAG = "PAFLAMY";

        private readonly int SCREEN_WIDTH;
        private readonly int SCREEN_HEIGHT;

        private const int TILE_WIDTH = 30;
        private const int TILE_HEIGHT = 45;

        private float objx, objy;

        private double time;

        public PaflamyView(Context context) : base(context)
        {
            SCREEN_WIDTH = Resources.DisplayMetrics.WidthPixels;
            SCREEN_HEIGHT = Resources.DisplayMetrics.HeightPixels;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Log.Verbose(DBG_TAG, $"w: {SCREEN_WIDTH}, h: {SCREEN_HEIGHT}");

            GL.ClearColor(0, 0, 0, 1);
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, SCREEN_WIDTH, SCREEN_HEIGHT, 0, -1, 1);
            GL.MatrixMode(All.Modelview);
            GL.LoadIdentity();

            Run();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            time += e.Time;

            objx = ((float)Math.Cos(time) + 1) / 2 * (SCREEN_WIDTH - TILE_WIDTH);
            objy = ((float)Math.Sin(time) + 1) / 2 * (SCREEN_HEIGHT - TILE_HEIGHT);
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
                TILE_WIDTH, 0,
                0, TILE_HEIGHT,
                TILE_WIDTH, TILE_HEIGHT
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

            DrawTile(objx, objy, Color.Red);

            SwapBuffers();
        }

        private void GLColor4(Color c)
            => GL.Color4(c.R, c.G, c.B, c.A);

        protected override void CreateFrameBuffer()
        {
            try
            {
                Log.Verbose(DBG_TAG, "Loading with default settings");
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose(DBG_TAG, ex.ToString());
            }

            try
            {
                Log.Verbose(DBG_TAG, "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose(DBG_TAG, ex.ToString());
            }

            throw new Exception("Can't load egl, aborting");
        }
    }
}
