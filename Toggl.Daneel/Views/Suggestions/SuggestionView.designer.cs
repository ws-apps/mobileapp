// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel
{
	[Register ("SuggestionView")]
	partial class SuggestionView
	{
		[Outlet]
		UIKit.UILabel ClientLabel { get; set; }

		[Outlet]
		UIKit.UILabel DescriptionLabel { get; set; }

		[Outlet]
		UIKit.UIImageView ProjectDot { get; set; }

		[Outlet]
		UIKit.UILabel ProjectLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DescriptionLabel != null) {
				DescriptionLabel.Dispose ();
				DescriptionLabel = null;
			}

			if (ProjectDot != null) {
				ProjectDot.Dispose ();
				ProjectDot = null;
			}

			if (ProjectLabel != null) {
				ProjectLabel.Dispose ();
				ProjectLabel = null;
			}

			if (ClientLabel != null) {
				ClientLabel.Dispose ();
				ClientLabel = null;
			}
		}
	}
}
