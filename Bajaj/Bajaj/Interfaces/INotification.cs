using Android.App;
using System;

namespace Bajaj.Interfaces
{
    public interface INotification
    {
        void SubscribeUserNotificationId(string user_id);
        void UnsubscribeUserNotificationId(string user_id);
    }
}
