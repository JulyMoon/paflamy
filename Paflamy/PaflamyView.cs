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
    public class PaflamyView : AndroidGameView
    {
        private readonly Logic logic;
        private readonly UI ui;
        private readonly Graphics graphics;

        public PaflamyView(Context context) : base(context)
        {
            logic = new Logic(context.Resources.GetString(Resource.String.LevelSet));
            ui = new UI(logic, Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);
            graphics = new Graphics(logic, ui);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            graphics.OnLoad();

            Run();
        }

        public override bool OnTouchEvent(MotionEvent e)
            => ui.OnTouch(e);

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            graphics.OnRender(e.Time);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            ui.OnUpdate(e.Time);
        }

        protected override void CreateFrameBuffer()
        {
            try
            {
                Util.Log("Loading with default settings");
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Util.Log(ex.ToString());
            }

            try
            {
                Util.Log("Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Util.Log(ex.ToString());
            }

            throw new Exception("Can't load egl, aborting");
        }
    }
}
