// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace DVDApp.iOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView mainImageView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (mainImageView != null) {
				mainImageView.Dispose ();
				mainImageView = null;
			}
		}
	}
}
