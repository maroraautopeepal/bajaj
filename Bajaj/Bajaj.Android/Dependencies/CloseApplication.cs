
using Android.App;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseApplication))]
namespace Bajaj.Droid.Dependencies
{
    public class CloseApplication : ICloseApplication
    {
        public void closeApplication()
        {
            var activity = (Activity)Xamarin.Forms.Forms.Context;
            activity.FinishAffinity();
        }
    }
}