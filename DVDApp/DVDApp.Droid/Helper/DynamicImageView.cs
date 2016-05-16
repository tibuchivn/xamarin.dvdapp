using Android.Content;
using Android.Util;
using Android.Widget;
using Orientation = Android.Content.Res.Orientation;

namespace DVDApp.Droid.Helper
{
    public class DynamicImageView : ImageView
    {
        public DynamicImageView(Context context, IAttributeSet attributeSet) : base(context, attributeSet)
        {

        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var orientation = this.Resources.Configuration.Orientation;
            if (orientation == Orientation.Portrait)
            {
                int width = MeasureSpec.GetSize(widthMeasureSpec);
                int height = width * Drawable.IntrinsicHeight / Drawable.IntrinsicWidth;
                SetMeasuredDimension(width, height);
            }
            else
            {
                int height = MeasureSpec.GetSize(widthMeasureSpec);
                int width = height * Drawable.IntrinsicWidth / Drawable.IntrinsicHeight;
                SetMeasuredDimension(width, height);
            }
            
        }
    }
}