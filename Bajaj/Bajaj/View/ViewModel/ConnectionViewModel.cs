using Acr.UserDialogs;
using Android.Bluetooth;
using Bajaj.Interfaces;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class ConnectionViewModel : BaseViewModel
    {
        ApiServices services;
        readonly Page page;
        public bool IsGpsEnable = false;
        public ConnectionViewModel(Page page)
        {
            this.page = page;
            services = new ApiServices();
        }
            
        public ICommand BluetoothConnectCommand => new Command(async () =>
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                await Task.Delay(100);

                try
                {
                    Android.Bluetooth.BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                    if (!bluetoothAdapter.IsEnabled)
                    {
                        var result = await page.DisplayAlert("Alert", "Do you want to Enable Bluetooth ?", "Yes", "No");
                        if (result)
                        {
                            bluetoothAdapter.Enable();

                            await Task.Delay(200);

                            Android.Telephony.TelephonyManager mTelephonyMgr;
                            mTelephonyMgr = (Android.Telephony.TelephonyManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.TelephonyService);
                            if (IsGpsEnable == DependencyService.Get<IGpsDependencyService>().IsGpsEnable())
                            {
                                DependencyService.Get<IGpsDependencyService>().OpenSettings();
                            }
                        }
                        else
                            return;
                    }
                    else
                    {
                        Android.Telephony.TelephonyManager mTelephonyMgr;
                        mTelephonyMgr = (Android.Telephony.TelephonyManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.TelephonyService);
                        if (IsGpsEnable == DependencyService.Get<IGpsDependencyService>().IsGpsEnable())
                        {
                            DependencyService.Get<IGpsDependencyService>().OpenSettings();
                        }
                        else
                        {
                            await page.Navigation.PushAsync(new BluetoothDevicesPage());
                            //await Navigation.PushAsync(new BluetoothDevicesPage(jobCard));
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        });

        public ICommand UsbConnectCommand => new Command(async () =>
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                await Task.Delay(100);
                try
                {
                    MessagingCenter.Send<ConnectionViewModel, string>(this, "connect_usb", "connection");
                    var mac_id = await DependencyService.Get<Interfaces.IConnectionUSB>().GetDongleMacID(false);

                    if (!string.IsNullOrEmpty(mac_id))
                    {
                        var firmwareVersion = await DependencyService.Get<Interfaces.IConnectionUSB>().SetDongleProperties();
                        if (!string.IsNullOrEmpty(firmwareVersion))
                        {
                            App.firmwareVersion = firmwareVersion;
                            App.ConnectedVia = "USB";
                            await page.Navigation.PushAsync(new ConnectedPage(firmwareVersion));
                        }
                        else
                        {
                            await page.DisplayAlert("Error", "Firmware version not found.", "OK");
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        });
    }
}
