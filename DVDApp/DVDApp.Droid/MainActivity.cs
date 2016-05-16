using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using DVDApp.Droid.Helper;

namespace DVDApp.Droid
{
	[Activity (Label = "DVDApp.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			//// Get our button from the layout resource,
			//// and attach an event to it
			//Button button = FindViewById<Button> (Resource.Id.myButton);
			
			//button.Click += delegate {
			//	button.Text = string.Format ("{0} clicks!", count++);
			//};

		    var mainImageView = FindViewById<DynamicImageView>(Resource.Id.mainImageView);
            //var imageUrl = Core.GetImageNoAsync();
            //var imageBitmap = GetImageBitmapFromUrl(imageUrl);
            //mainImageView.SetImageBitmap(imageBitmap);
            SlideImages(mainImageView);
        }

	    private async void SlideImages(DynamicImageView imageViewer)
	    {
	        const int interval = 3000;//3s
	        do
	        {
                var imageUrl = await Core.GetImage();
                var imageBitmap = GetImageBitmapFromUrl(imageUrl);
                imageViewer.SetImageBitmap(imageBitmap);
	            await Task.Delay(interval);
	        } while (true);
	    }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }
    }
}


