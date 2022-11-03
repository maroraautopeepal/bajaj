using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.View;
using MultiEventController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Plugin.Connectivity;
//using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;


namespace Bajaj
{
    public partial class App : Xamarin.Forms.Application
    {
        ApiServices services = new ApiServices();
        public static ControlEventManager controlEventManager = null;

        public static bool IsInternet = false;

        public static string MasterLoginUserBY = string.Empty;
        public static string MasterLoginUserRoleBY = string.Empty;
        public static string UserName = string.Empty;
        public static int LoginUserID = 0;

        public static string JwtToken = string.Empty;
        public static string SelectedModel = string.Empty;
        public static double ScreenHeight = 0;
        public static string SessionId = string.Empty;
        public static string ConnectedVia = string.Empty;
        public static bool NetConnected = true;
        public static int model_id = 0;
        public static int sub_model_id = 0;
        public static bool is_login = false;
        public static ObservableCollection<BluetoothDevicesModel> bluetoothDeviceds = new ObservableCollection<BluetoothDevicesModel>();
        public static List<string> ecu_list = new List<string>();
        public static string firmwareVersion = string.Empty;
        public static bool PageFreezIsEnable = false;
        public static bool IsRemote = false;

        public static string notify_jobcard_id = string.Empty;
        public static string notify_technitian_name = string.Empty;
        public static string notify_workshop_name = string.Empty;
        public static string notify_vehicle_model = string.Empty;
        public static string notify_state = string.Empty;
        public static string notify_expert_id = string.Empty;
        public static string notify_expert_first_name = string.Empty;
        public static string notify_workshop_add = string.Empty;

        public static AllModelsModel ForOfflineJobCardCreate;

        public static bool PageISTrueOrNot = false;

        public static bool ContentPageIsEnabled { get; set; }
        public static bool NotGoBack = true;
        public static bool PageOneToOtherGoBack = false;
        public static bool ExpertIsNotConnected = true;

        public static bool IsExpert = false;
        public static List<ResponseJobCardModel> IsSameResponce = null;
        public static bool isRunningTimer = false;
        public static bool DCTPage = true;
        public static bool INFOPage = true;
        public static bool TreeSurveyPage = true;
        public static bool ControlBYExpert = true;
        public static bool Notification = true;
        public static bool TreeL = true;
        public static JobCardListModel JCM;
        public static string RemoteSessionId = string.Empty;

        public static string dongle_type = string.Empty;

        public static AllOemModel selectedOem = new AllOemModel();

        public static string whoamiToken = string.Empty;
        public static string loginToken = string.Empty;
        public static string vin = string.Empty;

        public App()
        {
            InitializeComponent();
            //ThemeManager.ChangeTheme("GreenTheme");
            //App.Current.On<Xamarin.Forms.PlatformConfiguration.Windows>().SetImageDirectory("Assets");

            try
            {

                var token = App.Current.Properties["token"].ToString();
                App.Current.SavePropertiesAsync();
                ContentPageIsEnabled = true;
                if (string.IsNullOrEmpty(token))
                {
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            break;
                        case Device.Android:
                            //MainPage = new NavigationPage(new View.LoginPage());
                            MainPage = new NavigationPage(new View.DealerIdPage());
                            break;
                        case Device.UWP:
                            MainPage = new NavigationPage(new View.LoginPage());
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    App.JwtToken = token;
                    var device_id = (dynamic)null;
                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            device_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
                            break;
                        case Device.UWP:
                            device_id = "1234567890";
                            break;
                        default:
                            break;
                    }

                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            break;
                        case Device.Android:
                            MainPage = new NavigationPage(new View.LoginPage());
                            //MainPage = new NavigationPage(new MasterDetailView(device_id) { Detail = new NavigationPage(new JobCardPage(device_id)) });
                            break;
                        case Device.UWP:
                            MainPage = new NavigationPage(new MasterDetailView(device_id) { Detail = new NavigationPage(new JobCardPage(device_id)) });
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        break;
                    case Device.Android:
                        //MainPage = new NavigationPage(new View.LoginPage());
                        MainPage = new NavigationPage(new View.DealerIdPage());
                        break;
                    case Device.UWP:
                        MainPage = new NavigationPage(new View.LoginPage());
                        break;
                    default:
                        break;
                }
            }

            controlEventManager = new ControlEventManager();

            MessagingCenter.Unsubscribe<string>(this, "MoveToView");
            MessagingCenter.Subscribe<string>(this, "MoveToView", (value) =>
            {
                try
                {
                    var token = App.Current.Properties["token"].ToString();
                    if (string.IsNullOrEmpty(token))
                    {
                        switch (Device.RuntimePlatform)
                        {
                            case Device.iOS:
                                //top = 20;
                                break;
                            case Device.Android:
                                App.IsSameResponce.Clear();
                                App.Current.Properties["MasterLoginUserRoleBY"] = null;
                                App.Current.Properties["LoginUserId"] = null;
                                MainPage = new NavigationPage(new View.LoginPage());
                                break;
                            case Device.UWP:
                                MainPage = new NavigationPage(new View.LoginPage());
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        MainPage = new NavigationPage(new View.RemoteRequestPage());
                    }
                }
                catch (Exception ex)
                {
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            //top = 20;
                            break;
                        case Device.Android:
                            App.IsSameResponce.Clear();
                            App.Current.Properties["MasterLoginUserRoleBY"] = null;
                            App.Current.Properties["LoginUserId"] = null;
                            MainPage = new NavigationPage(new View.LoginPage());
                            break;
                        case Device.UWP:
                            MainPage = new NavigationPage(new View.LoginPage());
                            break;
                        default:
                            break;
                    }
                }
            });
        }

        public static void OnitExpert(string OwnerUserId, string ToUserId)
        {
            CurrentUserEvent.Instance.OwnerUserId = OwnerUserId;
            CurrentUserEvent.Instance.ToUserId = ToUserId;
            if (OwnerUserId == "RemoteSessionClosedByExpert" && ToUserId == "RemoteSessionAreClosed")
            {
                CurrentUserEvent.Instance.IsExpert = false;
                CurrentUserEvent.Instance.IsRemote = false;
            }
            else
            {
                CurrentUserEvent.Instance.IsExpert = true;
                CurrentUserEvent.Instance.IsRemote = true;
            }
        }
        public static void InitTechnician(string OwnerUserId, string ToUserId)
        {
            ContentPageIsEnabled = false;
            CurrentUserEvent.Instance.OwnerUserId = ToUserId;
            CurrentUserEvent.Instance.ToUserId = OwnerUserId;
            CurrentUserEvent.Instance.IsExpert = false;
            if (OwnerUserId == "RemoteSessionClosedByExpert" && ToUserId == "RemoteSessionAreClosed")
            {
                CurrentUserEvent.Instance.IsRemote = false;
            }
            else
            {
                CurrentUserEvent.Instance.IsRemote = true;
            }
        }

        public static void Expert(string ExpertID, string UserToken)
        {
            CurrentUserEvent.Instance.OwnerUserId = UserToken;
            CurrentUserEvent.Instance.ToUserId = ExpertID;
            CurrentUserEvent.Instance.IsExpert = true;
            CurrentUserEvent.Instance.IsRemote = true;
        }
        public static void Technician(string UserToken, string ExpertID)
        {
            ContentPageIsEnabled = false;
            CurrentUserEvent.Instance.OwnerUserId = ExpertID;
            CurrentUserEvent.Instance.ToUserId = UserToken;
            CurrentUserEvent.Instance.IsExpert = false;
        }


        protected async override void OnStart()
        {
            //await controlEventManager.Init();
            //IsInternet = CrossConnectivity.Current.IsConnected;
            //CrossConnectivity.Current.ConnectivityChanged += (sender, args) => 
            //{
            //    IsInternet = args.IsConnected;
            //    if (CurrentUserEvent.Instance.IsRemote)
            //    {
            //        if (!IsInternet)
            //        {
            //            Current.MainPage.DisplayAlert("", "Your Internet is disconnected\nYour remote session is closed.", "OK");
            //            CurrentUserEvent.Instance.IsRemote = false;
            //        }
            //    }
            //};

            //CrossConnectivity.Current.ConnectivityChanged += HandleConnectivityChanged;
            //CrossConnectivity.Current.ConnectivityChanged += UpdateNetworkInfo;
        }

        //private void UpdateNetworkInfo(object sender, ConnectivityChangedEventArgs e)
        //{
        //    var connectionType = CrossConnectivity.Current.ConnectionTypes.FirstOrDefault();
        //    //ConNet.Text = connectionType.ToString();
        //}

        //void HandleConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        //{
        //    if (e.IsConnected)
        //    {

        //    }
        //    else
        //    {

        //    }
        //    //Type currentPage = this.MainPage.GetType();
        //    //if (e.IsConnected 
        //    //    && currentPage != typeof(MainPage)) this.MainPage = new MainPage();
        //    //else if (!e.IsConnected && currentPage != typeof(MainPage)) this.MainPage = new MainPage();
        //}

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
