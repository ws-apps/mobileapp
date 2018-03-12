using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.Plugins.Visibility;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    [NestedPresentation]
    public partial class TimeEntriesLogViewController : MvxViewController<TimeEntriesLogViewModel>
    {
        public TimeEntriesLogViewController()
            : base(nameof(TimeEntriesLogViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //TableView config
            var source = new TimeEntriesLogViewSource(TimeEntriesTableView);
            TimeEntriesTableView.Source = source;

            //Add negative bottom inset, so that footers won't stick to the bottom of the screen
            var bottomContentInset = -source.GetHeightForFooter(TimeEntriesTableView, 0);
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                var bottomSafeAreaInset = UIApplication.SharedApplication.KeyWindow.SafeAreaInsets.Bottom;
                bottomContentInset -= bottomSafeAreaInset;
            }
            var tableViewContentInset = TimeEntriesTableView.ContentInset;
            tableViewContentInset.Bottom = bottomContentInset;
            TimeEntriesTableView.ContentInset = tableViewContentInset;

            //Converters
            var visibilityConverter = new MvxVisibilityValueConverter();
            var invertedVisibilityConverter = new MvxInvertedVisibilityValueConverter();

            var bindingSet = this.CreateBindingSet<TimeEntriesLogViewController, TimeEntriesLogViewModel>();

            //Text
            bindingSet.Bind(EmptyStateTextLabel).To(vm => vm.EmptyStateText);
            bindingSet.Bind(EmptyStateTitleLabel).To(vm => vm.EmptyStateTitle);

            //Time entries log
            bindingSet.Bind(source).To(vm => vm.TimeEntries);

            //Visibility
            bindingSet.Bind(EmptyStateView)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsEmpty)
                      .WithConversion(visibilityConverter);

            bindingSet.Bind(TimeEntriesTableView)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsEmpty)
                      .WithConversion(invertedVisibilityConverter);

            bindingSet.Bind(EmptyStateImageView)
                      .For(v => v.BindVisibility())
                      .To($"{nameof(TimeEntriesLogViewModel.IsEmpty)}&&!{nameof(TimeEntriesLogViewModel.IsWelcome)}")
                      .WithConversion(visibilityConverter);

            bindingSet.Bind(WelcomeImageView)
                      .For(v => v.BindVisibility())
                      .To($"{nameof(TimeEntriesLogViewModel.IsEmpty)}&&{nameof(TimeEntriesLogViewModel.IsWelcome)}")
                      .WithConversion(visibilityConverter);

            //Commands
            bindingSet.Bind(source)
                      .For(s => s.SelectionChangedCommand)
                      .To(vm => vm.EditCommand);

            bindingSet.Bind(source)
                      .For(v => v.ContinueTimeEntryCommand)
                      .To(vm => vm.ContinueTimeEntryCommand);


            bindingSet.Bind(source)
                      .For(v => v.DeleteTimeEntryCommand)
                      .To(vm => vm.DeleteCommand);

            bindingSet.Apply();
        }

        internal void Reload()
        {
            var range = new NSRange(0, TimeEntriesTableView.NumberOfSections());
            var indexSet = NSIndexSet.FromNSRange(range);
            TimeEntriesTableView.ReloadSections(indexSet, UITableViewRowAnimation.None);
        }
    }
}
