using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

namespace Paflamy
{
    [Activity(Label = "Paflamy",
        MainLauncher = true,
        Icon = "@drawable/icon",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
#if __ANDROID_11__
		,HardwareAccelerated=false
#endif
        )]
    public class MainActivity : Activity
    {
        private PaflamyView view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            view = new PaflamyView(this);
            SetContentView(view);
        }

        public override void OnBackPressed()
        {
            //base.OnBackPressed();
        }

        protected override void OnPause()
        {
            base.OnPause();
            view.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            view.Resume();
        }
    }
}

