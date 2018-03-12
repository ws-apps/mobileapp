// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.Views.Log
{
	[Register ("TimeEntriesLogEmptyStateViewCell")]
	partial class TimeEntriesLogEmptyStateViewCell
	{
		[Outlet]
		UIKit.UIView ClientPlaceHolderView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ClientPlaceholderWidth { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint DescriptionPlaceholderWidth { get; set; }

		[Outlet]
		UIKit.UIView ProjectPlaceholderView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ProjectPlaceholderWidth { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ProjectPlaceholderView != null) {
				ProjectPlaceholderView.Dispose ();
				ProjectPlaceholderView = null;
			}

			if (DescriptionPlaceholderWidth != null) {
				DescriptionPlaceholderWidth.Dispose ();
				DescriptionPlaceholderWidth = null;
			}

			if (ProjectPlaceholderWidth != null) {
				ProjectPlaceholderWidth.Dispose ();
				ProjectPlaceholderWidth = null;
			}

			if (ClientPlaceholderWidth != null) {
				ClientPlaceholderWidth.Dispose ();
				ClientPlaceholderWidth = null;
			}

			if (ClientPlaceHolderView != null) {
				ClientPlaceHolderView.Dispose ();
				ClientPlaceHolderView = null;
			}
		}
	}
}
