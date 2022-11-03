using Rg.Plugins.Popup.Services;
using Bajaj.Interfaces;
using Bajaj.View.PopupPages;
using Bajaj.View.TerminalPart;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterDetailView : MasterDetailPage
    {
        public MasterDetailView(string mac_id)
        {
            InitializeComponent();
            try
            {

                //MessagingCenter.Subscribe<LiveParameterSelectedPage>(this, "OpenMasterDetailSwip", (sender) =>
                //{
                //    this.IsPresented = true;
                //});

                //MessagingCenter.Subscribe<LiveParameterSelectedPage>(this, "OpenMasterDetailSwip", (sender) =>
                //{
                //    this.IsPresented = true;
                //});

                MessagingCenter.Subscribe<View.IorTest.IorTestPlayPage>(this, "StartMasterDetailSwip", (sender) =>
                {
                    this.IsGestureEnabled = true;
                });

                MessagingCenter.Subscribe<View.IorTest.IorTestPlayPage>(this, "StopMasterDetailSwip", (sender) =>
                {
                    this.IsGestureEnabled = false;
                });

                MessagingCenter.Subscribe<View.ActuatorTest.ActuatorTestPage>(this, "StartMasterDetailSwip", (sender) =>
                {
                    this.IsGestureEnabled = true;
                });

                MessagingCenter.Subscribe<View.ActuatorTest.ActuatorTestPage>(this, "StopMasterDetailSwip", (sender) =>
                {
                    this.IsGestureEnabled = false;
                });

                MessagingCenter.Subscribe<LiveParameterSelectedPage>(this, "StartMasterDetailSwip", (sender) =>
                {
                    this.IsGestureEnabled = true;
                });

                MessagingCenter.Subscribe<LiveParameterSelectedPage>(this, "StopMasterDetailSwip", (sender) =>
                {
                    this.IsGestureEnabled = false;
                });

                var Version = DependencyService.Get<IVersionAndBuildNumber>().GetVersionNumber();
                AppV.Text = $"App Version {Version}";

                App.notify_technitian_name = LoginUserBY.Text = App.Current.Properties["MasterLoginUserBY"].ToString();
                LoginUserRoleBY.Text = App.Current.Properties["MasterLoginUserRoleBY"].ToString();

                string IsTerminal = App.Current.Properties["IsTerminal"].ToString();

                if (IsTerminal.Contains("True"))
                {
                    txtTeminal.IsVisible = true;
                    lineTeminal.IsVisible = true;
                }
                else
                {
                    txtTeminal.IsVisible = false;
                    lineTeminal.IsVisible = false;
                }

                lineTeminal.IsVisible = true;
                txtTeminal.IsVisible = true;

              
                //Detail = new NavigationPage(new JobCardPage(mac_id));
                IsPresented = false;
            }
            catch (Exception ex)
            {

            }
        }

        private async void BtnSignOutClicked(object sender, EventArgs e)
        {
            Application.Current.Properties["token"] = App.JwtToken = "";
            App.Current.SavePropertiesAsync();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    //top = 20;
                    break;
                case Device.Android:
                    App.Current.MainPage = new NavigationPage(new View.LoginPage());
                    break;
                case Device.UWP:
                    App.Current.MainPage = new View.LoginPage();
                    break;
                default:
                    //top = 0;
                    break;
            }
            //App.Current.MainPage = new NavigationPage(new LoginPage());
        }

        private void BtnExitClicked(object sender, EventArgs e)
        {
            DependencyService.Get<ICloseApplication>().closeApplication();
        }

        private void JobCard_Tapped(object sender, EventArgs e)
        {
            //var mac_id = DependencyService.Get<IDeviceMacAddress>().GetMacAddress();
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
            
            var AndroidID = Id;
            Detail = new NavigationPage(new JobCardPage(device_id));
            IsPresented = false;
        }

        private void BtnChangePasswordClicked(object sender, EventArgs e)
        {
            //Detail = new NavigationPage(new ChangePasswordPage());
            PopupNavigation.PushAsync(new Bajaj.View.PopupPages.ChangePasswordPage(string.Empty));
            IsPresented = false;
        }

        private void BtnTerminalClicked(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new TerminalPage(null));
            IsPresented = false;
        }

        private void BtnFirmwareClicked(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new FirmwareUpdatePage(string.Empty));
            IsPresented = false;
        }

        private void BtnUpdateFirmwareClicked(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new FirmwareUpdatePage(string.Empty));
            IsPresented = false;
        }

        [Obsolete]
        private void BtnSelectLanguageClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LanguagePopupPage());
            //Detail = new NavigationPage(new FirmwareUpdatePage());
            IsPresented = false;
        }

        private async void Logout_Tapped(object sender, EventArgs e)
        {
            try
            {
                ///Application.Current.Properties["user_id"] = "";
                //Application.Current.Properties["password"] = "";

                if (App.IsSameResponce != null)
                {
                    App.IsSameResponce.Clear();
                }
                App.Current.Properties["MasterLoginUserRoleBY"] = null;
                Application.Current.Properties["token"] = "";
                App.isRunningTimer = false;
                App.Current.SavePropertiesAsync();

                App.Current.MainPage = new NavigationPage(new LoginPage());
            }
            catch (Exception ex)
            {

            }
        }

        //string id = string.Empty;
        //public string Id
        //{
        //    get
        //    {
        //        if (!string.IsNullOrWhiteSpace(id))
        //            return id;

        //        id = Android.OS.Build.Serial;
        //        if (string.IsNullOrWhiteSpace(id) || id == Build.Unknown || id == "0")
        //        {
        //            try
        //            {
        //                var context = Android.App.Application.Context;
        //                id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
        //            }
        //            catch (Exception ex)
        //            {
        //                Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
        //            }
        //        }

        //        return id;
        //    }
        //}
    }
}