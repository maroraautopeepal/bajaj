using Android.Graphics;
using Android.Text;
using Android.Widget;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using System;
using Color = Android.Graphics.Color;

[assembly: Xamarin.Forms.Dependency(typeof(ToastAndroid))]
namespace Bajaj.Droid.Dependencies
{
    public class ToastAndroid : IToastMessage
    {
        [Obsolete]
        public void Show(string message)
        {
            Android.Widget.Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }
    }
}