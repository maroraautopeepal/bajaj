
using Android.App;
using Android.Content;
using Android.OS;

namespace Bajaj.Droid
{
    [Activity(Theme = "@style/splashTheme", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            var mainIntent = new Intent(Application.Context, typeof(MainActivity));

            if (Intent.Extras != null)
            {
                mainIntent.PutExtras(Intent.Extras);
            }
            mainIntent.SetFlags(ActivityFlags.SingleTop);

            StartActivity(mainIntent);
        }
        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}