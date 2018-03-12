using System;
using Foundation;
using UIKit;

namespace Toggl.Daneel.Views.Log
{
    public sealed partial class TimeEntriesLogEmptyStateViewCell : UITableViewCell
    {
        private static readonly Random Random = new Random();

        private const int minClientWidth = 74;
        private const int maxClientWidth = 84;
        private const int minProjectWidth = 42;
        private const int maxProjectWidth = 84;
        private const int minDescriptionWidth = 74;
        private const int maxDescriptionWidth = 110;

        private static readonly UIColor[] Colors =
        {
            UIColor.FromRGB(237f / 255f, 210f / 255f, 255f / 255f),
            UIColor.FromRGB(198f / 255f, 237f / 255f, 245f / 255f)
        };

        public static readonly NSString Key = new NSString(nameof(TimeEntriesLogEmptyStateViewCell));
        public static readonly UINib Nib;

        static TimeEntriesLogEmptyStateViewCell()
        {
            Nib = UINib.FromName(nameof(TimeEntriesLogEmptyStateViewCell), NSBundle.MainBundle);
        }

        public TimeEntriesLogEmptyStateViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            //1/3 chance of being visible
            var clientPlaceholderVisible = Random.Next() % 3 == 0;
            if (clientPlaceholderVisible)
                ClientPlaceholderWidth.Constant = Random.Next(minClientWidth, maxClientWidth);
            else
                ClientPlaceHolderView.Hidden = true;
            
            ProjectPlaceholderWidth.Constant = Random.Next(minProjectWidth, maxProjectWidth);
            ProjectPlaceholderView.BackgroundColor = Colors[Random.Next(0, Colors.Length)];
            DescriptionPlaceholderWidth.Constant = Random.Next(minDescriptionWidth, maxDescriptionWidth);

            SetNeedsLayout();
            SetNeedsUpdateConstraints();
        }
    }
}
