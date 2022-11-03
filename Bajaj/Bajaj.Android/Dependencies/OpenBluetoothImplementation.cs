using Android.Content;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using DependencyAttribute = Xamarin.Forms.DependencyAttribute;

[assembly: Dependency(typeof(OpenBluetoothImplementation))]
namespace Bajaj.Droid.Dependencies
{
    public class OpenBluetoothImplementation : IOpenBluetoothPage
    {
        public void OpenBluetoothSettingPage()
        {
            Intent intentOpenBluetoothSettings = new Intent();
            intentOpenBluetoothSettings.SetAction(Android.Provider.Settings.ActionBluetoothSettings);
            Android.App.Application.Context.StartActivity(intentOpenBluetoothSettings);
        }
    }
}