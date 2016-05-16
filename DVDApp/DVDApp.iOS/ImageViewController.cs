
using System;
using System.Drawing;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace DVDApp.iOS
{
    public partial class ImageViewController : UIViewController
    {
        public ImageViewController(IntPtr handle) : base(handle)
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        private async void SlideImages()
        {
            const int interval = 3000;//3s
            do
            {
                var imageUrl = await Core.GetImage();
                var uiImage = FromUrl(imageUrl);
                imgViewer.Image = FromUrl(imageUrl);
                await Task.Delay(interval);
            } while (true);
        }

        #region View lifecycle

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
            //var imageUrl = Core.GetImageNoAsync();
            //imgViewer.Image = FromUrl(imageUrl);
            SlideImages();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
        }

        #endregion

        static UIImage FromUrl(string uri)
        {
            using (var url = new NSUrl(uri))
            using (var data = NSData.FromUrl(url))
                return UIImage.LoadFromData(data);
        }
    }
}