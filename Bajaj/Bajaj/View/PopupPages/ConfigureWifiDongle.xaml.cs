using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfigureWifiDongle : PopupPage
    {
        public ConfigureWifiDongle()
        {
            InitializeComponent();
        }

        private async void close_popup(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async void BtnSubmit_Clicked(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(DeviceSSIDtxt.Text))
            {
                await DisplayAlert("", "Please Enter SSID", "Ok");
                return;
            }

            if (string.IsNullOrEmpty(DevicePasswordtxt.Text))
            {
                await DisplayAlert("", "Please Enter Password", "Ok");
                return;
            }

            var ip_address = await DependencyService.Get<Interfaces.IConnectionWifi>().Write_SSIDPassword(DeviceSSIDtxt.Text, DevicePasswordtxt.Text);

            await PopupNavigation.Instance.PopAsync();
        }

        private async void BtnCancel_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}