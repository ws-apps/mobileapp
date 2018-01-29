using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme")]
    public sealed class StartTimeEntryActivity : MvxAppCompatActivity<StartTimeEntryViewModel>
    {
        public View SuggestionsContainer { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.StartTimeEntryActivity);

            OverridePendingTransition(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_fade_out);
        }

        protected override void OnResume()
        {
            base.OnResume();
            FindViewById<EditText>(Resource.Id.StartTimeEntryDescriptionTextField).RequestFocus();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_bottom);
        }
    }
}
