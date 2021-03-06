﻿using Android.App;
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
            => view.Back();

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

