using Bajaj.Controls;
using Bajaj.Droid.Renders;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
//using Xamarin.Forms;
//using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
namespace Bajaj.Droid.Renders
{
    [Obsolete]
    public class CustomEntryRenderer : Xamarin.Forms.Platform.Android.EntryRenderer
    {

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null) return;

            Control.SetBackgroundDrawable(null);
        }
    }
}
