using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using Android.Widget;
using Firebase.Messaging;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using Bajaj.View;
using System;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationHelper))]
namespace Bajaj.Droid.Dependencies
{
    internal class NotificationHelper : INotification
    {
        public void SubscribeUserNotificationId(string user_id)
        {
            try
            {
                FirebaseMessaging.Instance.SubscribeToTopic(user_id);
                Toast.MakeText(Android.App.Application.Context, "Token subscribed", ToastLength.Long).Show();
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(Android.App.Application.Context, "Token not subscribed", ToastLength.Long).Show();
            }
        }

        public void UnsubscribeUserNotificationId(string user_id)
        {
            try
            {
                FirebaseMessaging.Instance.UnsubscribeFromTopic(user_id);
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(Android.App.Application.Context, "Token not unsubscribed", ToastLength.Long).Show();
            }
        }
    }
}