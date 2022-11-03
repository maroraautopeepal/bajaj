using Android.App;
using Android.Content;
using Android.Util;
using AndroidX.Core.App;
using Firebase.Messaging;
using System.Collections.Generic;

namespace Bajaj.Droid
{
    [Service(Name = "com.fotax.MyFirebaseMessagingService")]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFirebaseMsgService";

        //public override void OnNewToken(string p0)
        //{
        //    base.OnNewToken(p0);
        //    SendRegistrationToServer(p0);
        //}

        //void SendRegistrationToServer(string token)
        //{
        //    // Add custom implementation, as needed.
        //}

        public override void OnMessageReceived(RemoteMessage message)
        {
            try
            {

                Log.Debug(TAG, "From: " + message.From);
                var title = message.Data["title"];
                var jobcard = message.Data["body"];
                var technician = message.Data["technician"];
                var workshop = message.Data["workshop"];
                var model = message.Data["model"];
                var address = message.Data["address"];

                string body = $"Jobcard Number : {jobcard}\nTechnician Name : {technician}\nVehicle Model : {model}\nWorkshop : {workshop}\nAddress : {address}";
                SendNotification(title, body, message.Data);
            }
            catch (System.Exception ex)
            {
            }
        }

        void SendNotification(string title, string body, IDictionary<string, string> data)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            foreach (var key in data.Keys)
            {
                intent.PutExtra(key, data[key]);
            }

            var pendingIntent = PendingIntent.GetActivity(this,
                                                          MainActivity.NOTIFICATION_ID,
                                                          intent,
                                                          PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                                      .SetSmallIcon(SML.Droid.Resource.Drawable.ic_notification_icon)
                                      .SetContentTitle(title)
                                      .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
                                      //.SetContentText(body)
                                      .SetAutoCancel(true)
                                      .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NOTIFICATION_ID, notificationBuilder.Build());
        }
    }
}