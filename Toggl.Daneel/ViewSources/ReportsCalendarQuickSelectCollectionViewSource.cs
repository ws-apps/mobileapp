using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Binding.iOS.Views;
using Toggl.Daneel.Views.Reports;
using Toggl.Foundation.MvvmCross.ViewModels.Calendar.QuickSelectShortcuts;
using Toggl.Multivac;
using UIKit;
using Math = System.Math;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ReportsCalendarQuickSelectCollectionViewSource
        : MvxCollectionViewSource, IUICollectionViewDelegateFlowLayout
    {
        private const int cellHeight = 32;
        private const int horizontalPadding = 20;
        private const string cellIdentifier = nameof(ReportsCalendarQuickSelectViewCell);

        private readonly UIStringAttributes titleAttributes;

        public ReportsCalendarQuickSelectCollectionViewSource(
            UICollectionView collectionView, UIFont font) : base(collectionView)
        {
            Ensure.Argument.IsNotNull(collectionView, nameof(collectionView));
            Ensure.Argument.IsNotNull(font, nameof(font));

            titleAttributes = new UIStringAttributes
            {
                Font = font
            };

            collectionView.RegisterNibForCell(ReportsCalendarQuickSelectViewCell.Nib, cellIdentifier);
        }

        protected override UICollectionViewCell GetOrCreateCellFor(UICollectionView collectionView, NSIndexPath indexPath, object item)
            => collectionView.DequeueReusableCell(cellIdentifier, indexPath) as UICollectionViewCell;

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = GetOrCreateCellFor(collectionView, indexPath, item);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = item;

            return cell;
        }

        public override nint NumberOfSections(UICollectionView collectionView) => 1;

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
            => ItemsSource.Count();

        protected override object GetItemAt(NSIndexPath indexPath)
            => ItemsSource.ElementAt((int)indexPath.Item);

        [Export("collectionView:layout:sizeForItemAtIndexPath:")]
        public CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            var quickSelectShortcut = (CalendarBaseQuickSelectShortcut)GetItemAt(indexPath);
            var title = new NSString(quickSelectShortcut.Title);
            var titleSize = title.GetSizeUsingAttributes(titleAttributes);

            return new CGSize(
                Math.Ceiling(titleSize.Width) + horizontalPadding,
                cellHeight
            );
        }
    }
}
