﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class SelectProjectActivity : MvxAppCompatActivity<SelectProjectViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SelectProjectActivity);

            OverridePendingTransition(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_fade_out);
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_bottom);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                ViewModel.CloseCommand.Execute();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}