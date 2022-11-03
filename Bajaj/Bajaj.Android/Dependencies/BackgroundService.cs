using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.View.ViewModel;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;

namespace Bajaj.Droid.Dependencies
{
    //[Service(Label = "BackgroundService")]
    public class BackgroundService //: Service
    {
        //bool InternetConnection = false;
        ////bool isRunningTimer = true;
        //ApiServices services;
        //string UserID = string.Empty;
        //int Count = 0;

        //private Timer TimeTracker = new Timer(60000);
        //public const int ServiceRunningNotifID = 9000;

        //private static Context context = global::Android.App.Application.Context;
        //private NotificationCompat.Builder mBuilder;
        //[return: GeneratedEnum]
        //public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        //{
        //    try
        //    {
        //        services = new ApiServices();
        //        App.IsSameResponce = new List<ResponseJobCardModel>();
        //        string Role = App.Current.Properties["MasterLoginUserRoleBY"].ToString();
        //        if (Role == "expert")
        //        {
        //            App.isRunningTimer = true;
        //            var UID = App.Current.Properties["LoginUserId"].ToString();
        //            if (UID != null)
        //            {
        //                UserID = Convert.ToString(UID);
        //                Notification notif = new Notification();

        //                Device.StartTimer(new TimeSpan(0, 0, 10), () =>
        //                {
        //                    Count++;
        //                    if (Count == 60)
        //                    {
        //                        if (App.IsSameResponce != null)
        //                        {
        //                            App.IsSameResponce.Clear();
        //                        }
        //                        InternetConnection = false;
        //                        Count = 0;
        //                    }
        //                    Device.BeginInvokeOnMainThread(async () =>
        //                    {
        //                        try
        //                        {
        //                            var _isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
        //                            if (_isReachable)
        //                            {
        //                                // do something every 10 seconds
        //                                string UserID = App.Current.Properties["LoginUserId"].ToString();
        //                                var ExpertNotificationCount = services.GetExpertRequestList(UserID);
        //                                if (ExpertNotificationCount.Result.results.Count() != 0)
        //                                {
        //                                    foreach (var item in ExpertNotificationCount.Result.results)
        //                                    {
        //                                        var YesOrNo = App.IsSameResponce.Where(x => x.id == item.id && x.job_card_session == item.job_card_session && x.status == item.status).Count();
        //                                        if (YesOrNo == 0)
        //                                        {
        //                                            //notif = DependencyService.Get<INotification>().CreateNotification(item.remote_session_id, item.status);
        //                                            //App.IsSameResponce.Add(new ResponseJobCardModel
        //                                            //{
        //                                            //    id = item.id,
        //                                            //    job_card_session = item.job_card_session,
        //                                            //    status = item.status,
        //                                            //    expert_user = item.expert_user,
        //                                            //    request_status = item.request_status
        //                                            //});
        //                                        }
        //                                    }
        //                                }
        //                                InternetConnection = false;
        //                            }
        //                            else
        //                            {
        //                                if (InternetConnection == false)
        //                                {
        //                                    App.IsSameResponce.Clear();
        //                                    //notif = DependencyService.Get<INotification>().CreateNotification("Please check Internet Connection!!", "Internet Connection Issue!!");
        //                                    InternetConnection = true;
        //                                }
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {

        //                        }
        //                    });
        //                    return App.isRunningTimer; // runs again, or false to stop
        //                });
        //            }
        //        }
        //        else
        //        {
        //            if (App.IsSameResponce != null)
        //            {
        //                App.IsSameResponce.Clear();
        //            }
        //            App.isRunningTimer = false;
        //            InternetConnection = false;
        //            OnDestroy();
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    //Notification notif = new Notification();
        //    //Device.StartTimer(new TimeSpan(0, 0, 10), () =>
        //    //{
        //    //    //MessagingCenter.Send<string>(counter.ToString(), "CounterValue");
        //    //    //counter = +1;

        //    //    Notification notif = DependencyService.Get<INotification>().CreateNotification("SPTutorials", "ABCD");

        //    //    return App.IsExpert;
        //    //});

        //    //StartForeground(ServiceRunningNotifID, notif);

        //    return StartCommandResult.Sticky;
        //}
        //public override IBinder OnBind(Intent intent)
        //{
        //    return null;
        //}
        //public override void OnDestroy()
        //{
        //    StopSelf();
        //    App.isRunningTimer = false;
        //    base.OnDestroy();
        //}
    }
}