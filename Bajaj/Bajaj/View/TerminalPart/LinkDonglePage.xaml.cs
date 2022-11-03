using Acr.UserDialogs;
using Android.Bluetooth;
using Bajaj.Interfaces;
using Bajaj.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.TerminalPart
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkDonglePage : ContentPage
    {
        ProtocolViewModel viewModel;
        public LinkDonglePage()
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new ProtocolViewModel();
            }
            catch (Exception ex)
            {
            }
        }

        public bool IsGpsEnable;
        private async void BluetoothConnectionClicked(object sender, EventArgs e)
        {

            try
            {
                if (viewModel.SelectedProtocol == null)
                {
                    await DisplayAlert("Alert", "Please select a protocol", "Ok");
                    return;
                }

                if (string.IsNullOrEmpty(viewModel.TxHeader))
                {
                    await DisplayAlert("Alert", "Please enter tx header", "Ok");
                    return;
                }
                else
                {
                    //var reg = System.Text.RegularExpressions.Regex.IsMatch(viewModel.TxHeader, @"\A\b[0-9a-fA-F]+\b\Z");
                    if (!System.Text.RegularExpressions.Regex.IsMatch(viewModel.TxHeader, @"\A\b[0-9a-fA-F]+\b\Z") || (viewModel.TxHeader.Length != 4 && viewModel.TxHeader.Length != 8))
                    {
                        await DisplayAlert("Alert", "Please enter valid tx header", "Ok");
                        return;
                    }
                }

                if (string.IsNullOrEmpty(viewModel.RxHeader))
                {
                    await DisplayAlert("Alert", "Please enter rx header", "Ok");
                    return;
                }
                else
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(viewModel.RxHeader, @"\A\b[0-9a-fA-F]+\b\Z") ||
                        (viewModel.RxHeader.Length == 4 && viewModel.RxHeader.Length == 8))
                    {
                        await DisplayAlert("Alert", "Please enter valid rx header", "Ok");
                        return;
                    }
                }

                viewModel.Commands[0] = Convert.ToString(viewModel.SelectedProtocol.ProtocolIndex);
                viewModel.Commands[5] = viewModel.SelectedProtocol.Protocol;
                viewModel.Commands[1] = viewModel.TxHeader;
                viewModel.Commands[2] = viewModel.RxHeader;
                if (viewModel.IsPadding)
                {
                    viewModel.Commands[3] = "00";
                }

                if (viewModel.IsTesterPresent)
                {
                    viewModel.Commands[4] = "001";
                }



                Android.Bluetooth.BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                if (!bluetoothAdapter.IsEnabled)
                {
                    var result = await DisplayAlert("Alert", "Can I Enable Bluetooth & GPS ?", "Yes", "No");
                    if (result)
                    {
                        bluetoothAdapter.Enable();

                        await Task.Delay(200);

                        if (IsGpsEnable == DependencyService.Get<IGpsDependencyService>().IsGpsEnable())
                        {
                            DependencyService.Get<IGpsDependencyService>().OpenSettings();
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (IsGpsEnable == DependencyService.Get<IGpsDependencyService>().IsGpsEnable())
                    {
                        DependencyService.Get<IGpsDependencyService>().OpenSettings();
                    }
                    else
                    {
                        //await Navigation.PushAsync(new BluetoothDevicesPage(null, viewModel.Commands,false));
                    }


                    //Android.Telephony.TelephonyManager mTelephonyMgr;
                    //mTelephonyMgr = (Android.Telephony.TelephonyManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.TelephonyService);
                    //if (mTelephonyMgr.MmsUserAgent.Contains("SAMSUNG"))
                    //{
                    //    if (IsGpsEnable == DependencyService.Get<IGpsDependencyService>().IsGpsEnable())
                    //    {
                    //        DependencyService.Get<IGpsDependencyService>().OpenSettings();
                    //    }
                    //    else
                    //    {
                    //        await Navigation.PushAsync(new BluetoothDevicesPage(jobCard));
                    //    }
                    //}
                    //else
                    //{
                    //    await Navigation.PushAsync(new BluetoothDevicesPage(jobCard));
                    //}

                    //var device = DependencyService.Get<IBth>().PairedDevices();
                }
            }
            catch (Exception ex)
            {
            }
        }
        private async void UsbConnectionClicked(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                try
                {
                    await Task.Delay(100);
                    if (viewModel.SelectedProtocol == null)
                    {
                        await DisplayAlert("Alert", "Please select a protocol", "Ok");
                        return;
                    }

                    if (string.IsNullOrEmpty(viewModel.TxHeader))
                    {
                        await DisplayAlert("Alert", "Please enter tx header", "Ok");
                        return;
                    }
                    else
                    {
                        //var reg = System.Text.RegularExpressions.Regex.IsMatch(viewModel.TxHeader, @"\A\b[0-9a-fA-F]+\b\Z");
                        if (!System.Text.RegularExpressions.Regex.IsMatch(viewModel.TxHeader, @"\A\b[0-9a-fA-F]+\b\Z") || (viewModel.TxHeader.Length != 4 && viewModel.TxHeader.Length != 8))
                        {
                            await DisplayAlert("Alert", "Please enter valid tx header", "Ok");
                            return;
                        }
                    }

                    if (string.IsNullOrEmpty(viewModel.RxHeader))
                    {
                        await DisplayAlert("Alert", "Please enter rx header", "Ok");
                        return;
                    }
                    else
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(viewModel.RxHeader, @"\A\b[0-9a-fA-F]+\b\Z") ||
                            (viewModel.RxHeader.Length == 4 && viewModel.RxHeader.Length == 8))
                        {
                            await DisplayAlert("Alert", "Please enter valid rx header", "Ok");
                            return;
                        }
                    }

                    viewModel.Commands[0] = Convert.ToString(viewModel.SelectedProtocol.ProtocolIndex);

                    viewModel.Commands[1] = viewModel.TxHeader;
                    viewModel.Commands[2] = viewModel.RxHeader;
                    if (viewModel.IsPadding)
                    {
                        viewModel.Commands[3] = "00";
                    }

                    if (viewModel.IsTesterPresent)
                    {
                        viewModel.Commands[4] = "001";
                    }
                    viewModel.Commands[5] = Convert.ToString(viewModel.SelectedProtocol.Protocol);

                    MessagingCenter.Send<LinkDonglePage, string>(this, "connect_usb", "connection");

                    //Connect();  
                    //var firmwareVersion = await DependencyService.Get<Interfaces.IConnectionUSB>().Connect();
                    //App.firmwareVersion = firmwareVersion;

                    //responseLabel.Text = result.ToString();

                    var commands = await DependencyService.Get<IConnectionUSB>().SendTerminalCommands(viewModel.Commands);
                    if (commands != null)
                    {
                        App.ConnectedVia = "USB";
                        var device_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
                        Application.Current.MainPage = new NavigationPage(new MasterDetailView(device_id) { Detail = new NavigationPage(new TerminalPage(commands)) });
                        //await Navigation.PushAsync(new AppFeaturePage(jobCard, firmwareVersion));
                        //if (firmwareVersion == "0.1.0.1")
                        //{
                        //    await Navigation.PushAsync(new AppFeaturePage(jobCard, firmwareVersion));
                        //}
                        //else
                        //{
                        //    await this.DisplayAlert("Try Again", "Not Able to Connect USB", "OK");
                        //}
                    }
                    else
                    {
                        await this.DisplayAlert("Try Again", "Not Able to Connect USB", "OK");
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        private async void WifiConnectionClicked(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                try
                {
                    await Task.Delay(100);
                    if (viewModel.SelectedProtocol == null)
                    {
                        await DisplayAlert("Alert", "Please select a protocol", "Ok");
                        return;
                    }

                    if (string.IsNullOrEmpty(viewModel.TxHeader))
                    {
                        await DisplayAlert("Alert", "Please enter tx header", "Ok");
                        return;
                    }
                    else
                    {
                        //var reg = System.Text.RegularExpressions.Regex.IsMatch(viewModel.TxHeader, @"\A\b[0-9a-fA-F]+\b\Z");
                        if (!System.Text.RegularExpressions.Regex.IsMatch(viewModel.TxHeader, @"\A\b[0-9a-fA-F]+\b\Z") || (viewModel.TxHeader.Length != 4 && viewModel.TxHeader.Length != 8))
                        {
                            await DisplayAlert("Alert", "Please enter valid tx header", "Ok");
                            return;
                        }
                    }

                    if (string.IsNullOrEmpty(viewModel.RxHeader))
                    {
                        await DisplayAlert("Alert", "Please enter rx header", "Ok");
                        return;
                    }
                    else
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(viewModel.RxHeader, @"\A\b[0-9a-fA-F]+\b\Z") ||
                            (viewModel.RxHeader.Length == 4 && viewModel.RxHeader.Length == 8))
                        {
                            await DisplayAlert("Alert", "Please enter valid rx header", "Ok");
                            return;
                        }
                    }

                    viewModel.Commands[0] = Convert.ToString(viewModel.SelectedProtocol.ProtocolIndex);

                    viewModel.Commands[1] = viewModel.TxHeader;
                    viewModel.Commands[2] = viewModel.RxHeader;
                    if (viewModel.IsPadding)
                    {
                        viewModel.Commands[3] = "00";
                    }

                    if (viewModel.IsTesterPresent)
                    {
                        viewModel.Commands[4] = "001";
                    }
                    viewModel.Commands[5] = Convert.ToString(viewModel.SelectedProtocol.Protocol);
                    App.ConnectedVia = "WIFI";
                    var wifiConnector = await DependencyService.Get<Interfaces.IConnectionWifi>().get_device_list();
                    await Navigation.PushAsync(new WifiDevicesPage(null, wifiConnector, viewModel.Commands,false));
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}