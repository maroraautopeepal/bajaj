using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using MultiEventController;
using MultiEventController.Models;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System.Collections;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppFeaturePage : DisplayAlertPage
    {
        ApiServices services;

        public AppFeaturePage()
        {
            InitializeComponent();

            try
            {
                services = new ApiServices();
            }
            catch (Exception ex)
            {

            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                vin.Text = "Vin : " + App.vin;
                if (!string.IsNullOrEmpty(App.firmwareVersion))
                {
                    if (App.ConnectedVia == "BT")
                    {
                        img_connect_vai.Source = "ic_bluetooth_white.png";
                        txt_version.Text = App.firmwareVersion;
                    }
                    else if (App.ConnectedVia == "USB")
                    {
                        img_connect_vai.Source = "ic_usb_white.png";
                        txt_version.Text = App.firmwareVersion;
                    }
                    else if (App.ConnectedVia == "WIFI")
                    {
                        img_connect_vai.Source = "ic_wifi_white.png";
                        txt_version.Text = App.firmwareVersion;
                    }
                    else
                    {
                        img_connect_vai.Source = "ic_rp1210.png";
                        txt_version.Text = App.firmwareVersion;
                    } 
                }
            }
            catch (Exception ex)
            {

            }
        }
        
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();
            return true;
        }
        
        

        private async void BtnUnlock_Clicked(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                await Task.Delay(100);
                await this.Navigation.PushAsync(new UnlockEcuPage(null));
            }
        }

        private async void BtnFault_Clicked(object sender, EventArgs e)
        {
            try
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);

                    await Navigation.PushAsync(new DtcListPage());
                }
            }
            catch (Exception ex)
            {
            }
        }
        

        private async void BtnLiveParameter_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new LiveParameterSelectPage());
            }
            catch (Exception ex)
            {

            }
        }

        private async void BtnWriteParameter_Clicked(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(50);
                        await Navigation.PushAsync(new WriteParameterPage());
                    }
                });
            }
            catch (Exception ex)
            {

            }
        }
        
        
        private async void BtnFlahECU_Clicked(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        await Navigation.PushAsync(new FlashECUPage(null));
                    }
                });
            }
            catch (Exception ex)
            {

            }

        }

        private async void ConnBT_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (App.ConnectedVia == "BT")
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        var result = await DisplayAlert("Alert", "Do you want to exit this Session", "Ok", "Cancel");
                        if (result)
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                Application.Current.MainPage = new NavigationPage(new ConnectionPage());
                            });
                        }
                    }
                }
                else if (App.ConnectedVia == "USB")
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        var result = await DisplayAlert("Alert", "Do you want to exit this Session", "Ok", "Cancel");
                        if (result)
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                Application.Current.MainPage = new NavigationPage(new ConnectionPage());
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        

        public void show_alert(string title, string message, bool btnCancel, bool btnOk)
        {
            Working = true;
            TitleText = title;
            MessageText = message;
            OkVisible = btnOk;
            CancelVisible = btnCancel;
            CancelCommand = new Command(() =>
            {
                Working = false;
            });
        }


        private async void BtnIorTest_Clicked(object sender, EventArgs e)
        {
            try
            {
                await this.Navigation.PushAsync(new View.IorTest.IorTestPage(null));
            }
            catch (Exception ex)
            {

            }
        }

        private async void BtnActuatorTest_Clicked(object sender, EventArgs e)
        {
            try
            {
                await this.Navigation.PushAsync(new View.ActuatorTest.ActuatorTestPage(null));
            }
            catch (Exception ex)
            {
            }
        }
    }
}