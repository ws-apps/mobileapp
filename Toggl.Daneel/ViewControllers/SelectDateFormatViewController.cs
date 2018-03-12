﻿using MvvmCross.Binding.BindingContext;
using MvvmCross.iOS.Views;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public sealed partial class SelectDateFormatViewController
        : MvxViewController<SelectDateFormatViewModel>
    {
        public SelectDateFormatViewController() : base(nameof(SelectDateFormatViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DateFormatsTableViewSource(DateFormatsTableView);
            DateFormatsTableView.Source = source;

            var bindingSet = this.CreateBindingSet<SelectDateFormatViewController, SelectDateFormatViewModel>();

            bindingSet.Bind(source).To(vm => vm.DateTimeFormats);

            bindingSet.Bind(BackButton).To(vm => vm.CloseCommand);

            bindingSet.Bind(source)
                      .For(v => v.SelectionChangedCommand)
                      .To(vm => vm.SelectFormatCommand);

            bindingSet.Apply();
        }
    }
}

