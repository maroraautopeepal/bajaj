using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.ViewModel;
using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Bajaj.View.TerminalPart;
using System.Linq;
using Bajaj.Services;
//using DotNetSta;
//using System.Diagnostics;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothDevicesPage : DisplayAlertPage
    {
        DongleViewModel dongleViewModel;
        BluetoothDevicesViewModel viewModel;
        ApiServices services;
        List<string> Ecu;

        public BluetoothDevicesPage()
        {
            try
            {
                InitializeComponent();
                services = new ApiServices();

                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    Task.Delay(200);
                    dongleViewModel = new DongleViewModel();
                    BindingContext = viewModel = new BluetoothDevicesViewModel();
                    Ecu = new List<string> { "protocol", "tx_header", };
                }
            }
            catch (Exception ex)
            {
            }
        }
        bool isconnected = false;


        private async void Conect_Clicked(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                string mac_id = string.Empty;
                try
                {
                    await Task.Delay(100);
                    var device = (BluetoothDevicesModel)((ImageButton)sender).BindingContext;
                    isconnected = true;

                    var res = await DependencyService.Get<IBth>().Start(device.Mac_Address, 50, true);
                    if (res == "connected")
                    {
                        //mac_id = await DependencyService.Get<IBth>().GetDongleMacID(false);

                        if (!string.IsNullOrEmpty(device.Mac_Address))
                        {
                            await DependencyService.Get<IBlueToothDevices>().EndScanning();

                            var firmwareVersion = await DependencyService.Get<Interfaces.IBth>().SetDongleProperties();
                            if (!string.IsNullOrEmpty(firmwareVersion))
                            {
                                App.ConnectedVia = "BT";
                                DependencyService.Get<IToastMessage>().Show("------BT Connected------");
                                isconnected = false;
                                App.firmwareVersion = firmwareVersion;
                                await Navigation.PushAsync(new ConnectedPage(firmwareVersion));
                            }
                            else
                            {
                                await DisplayAlert("", "Firmware not found !!", "OK");
                            }
                        }
                        else
                        {
                            await this.DisplayAlert("", "Dongle mac id not found", "OK");
                        }
                    }
                    else
                    {
                        await this.DisplayAlert("", "Dongle not connected\nTry again", "OK");
                    }
                }
                catch (Exception ex)
                {
                }

            }
        }


        //private async void Conect_Clicked(object sender, EventArgs e)
        //{
        //    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
        //    {
        //        try
        //        {
        //            await Task.Delay(100);

        //            if (StaticData.ecu_info != null && StaticData.ecu_info.Any())
        //            {
        //                if (!string.IsNullOrEmpty(StaticData.ecu_info[0].protocol.elm))
        //                {
        //                    RegisterDongleRespons RDR = new RegisterDongleRespons();
        //                    var device = (BluetoothDevicesModel)((ImageButton)sender).BindingContext;
        //                    isconnected = true;
        //                    var res = await DependencyService.Get<IBth>().Start(device.Mac_Address, 50, true);
        //                    if (res == "connected")
        //                    {
        //                        //StaticData.ecu_info
        //                        App.dongle_type = "elm";
        //                        var mac_id = await DependencyService.Get<IBth>().GetDongleMacID(false, StaticData.ecu_info[0].protocol.name, 
        //                            Convert.ToUInt32(StaticData.ecu_info[0].protocol.elm), StaticData.ecu_info[0].tx_header, StaticData.ecu_info[0].rx_header);
        //                        if (!string.IsNullOrEmpty(device.Mac_Address))
        //                        {
        //                            RegisterDongleModel RDM = new RegisterDongleModel
        //                            {
        //                                MacId = device.Mac_Address,
        //                                device_type = "obd_dongle_ble"
        //                            };

        //                            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
        //                            if (isReachable)
        //                            {
        //                                RDR = await viewModel.RegisterDongle(RDM);
        //                            }
        //                            else
        //                            {
        //                                var JsonListData = DependencyService.Get<ISaveLocalData>().GetData("LastUsedDongle");
        //                                var error = JsonConvert.DeserializeObject<RegDongleRespons>(JsonListData);
        //                                RDR.errorRes = error;
        //                            }

        //                            if (RDR.errorRes != null)
        //                            {
        //                                if (RDR.errorRes.is_active == true && RDR.errorRes.mac_id == device.Mac_Address)
        //                                {
        //                                    //if (device.Name == "APtBudB")
        //                                    //{
        //                                    //    Android.App.Application.Context.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionBluetoothSettings));
        //                                    //}
        //                                    if (IsFirmwareUpdate)
        //                                    {
        //                                        var FirmwareVersion = await DependencyService.Get<IBth>().GetFirmware();
        //                                        await this.Navigation.PushAsync(new FirmwareUpdatePage(FirmwareVersion));
        //                                    }
        //                                    if (TerminalCommands != null)
        //                                    {
        //                                        var commands = await DependencyService.Get<IBth>().SendTerminalCommands(TerminalCommands);
        //                                        App.ConnectedVia = "BT";

        //                                        DependencyService.Get<IToastMessage>().Show("------BT Connected------");

        //                                        isconnected = false;
        //                                        var device_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
        //                                        Application.Current.MainPage = new NavigationPage(new MasterDetailView(device_id)
        //                                        {
        //                                            Detail = new NavigationPage(
        //                                            new TerminalPage(commands))
        //                                        });
        //                                        //await Navigation.PushAsync(new TerminalPage(commands));
        //                                    }
        //                                    else
        //                                    {
        //                                        var firmwareVersion = await DependencyService.Get<IBth>().SetDongleProperties(StaticData.ecu_info[0].protocol.elm);
        //                                        App.firmwareVersion = firmwareVersion;
        //                                        if (!string.IsNullOrEmpty(firmwareVersion))
        //                                        {
        //                                            App.ConnectedVia = "BT";

        //                                            DependencyService.Get<IToastMessage>().Show("------BT Connected------");

        //                                            //await Application.Current.MainPage.Navigation.PushAsync(new AppFeaturePage(jobCard, $"Firmware Version {firmwareVersion}"));
        //                                            isconnected = false;

        //                                            //Application.Current.MainPage = new MasterDetailPage()
        //                                            //{
        //                                            //    Detail = new NavigationPage(new AppFeaturePage(jobCard, firmwareVersion))
        //                                            //};


        //                                            await Navigation.PushAsync(new AppFeaturePage(jobCard, firmwareVersion, null, null, string.Empty));
        //                                            //await Navigation.PushModalAsync(new AppFeaturePage(jobCard, $"Firmware Version {firmwareVersion}"));
        //                                        }
        //                                        else
        //                                        {
        //                                            await DisplayAlert("Alert", "Firmware not found", "OK");
        //                                            return;
        //                                        }

        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    await DisplayAlert("Please Check Internet Connection !!", "Dongle not Approved. Please contact Admin ( You can use last Dongle ) !!", "OK");
        //                                }
        //                            }
        //                            else
        //                            {
        //                                await DisplayAlert("", "Dongle not Approved. Please contact Admin !!", "OK");
        //                            }
        //                        }
        //                        else
        //                        {
        //                        }
        //                    }
        //                    else
        //                    {
        //                    }
        //                }
        //                else
        //                {
        //                    await DisplayAlert("Alert", "Unsupported protocol", "OK");
        //                }
        //            }
        //            else
        //            {
        //                await DisplayAlert("Alert", "Ecu not found", "OK");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //}

        //private async void Conect_Clicked(object sender, EventArgs e)
        //{
        //    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
        //    {
        //        try
        //        {
        //            await Task.Delay(100);
        //            var device = (BluetoothDevicesModel)((ImageButton)sender).BindingContext;
        //            isconnected = true;
        //            if (device.Mac_Address != "")
        //            {
        //                RegisterDongleModel RDM = new RegisterDongleModel
        //                {
        //                    MacId = device.Mac_Address,
        //                    device_type = "obd_dongle_ble"
        //                };
        //                RegisterDongleRespons RDR = new RegisterDongleRespons();
        //                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
        //                if (isReachable)
        //                {
        //                    RDR = await viewModel.RegisterDongle(RDM);
        //                }
        //                else
        //                {
        //                    var JsonListData = DependencyService.Get<ISaveLocalData>().GetData("LastUsedDongle");
        //                    var error = JsonConvert.DeserializeObject<RegDongleRespons>(JsonListData);
        //                    RDR.errorRes = error;
        //                }

        //                if (RDR.errorRes != null)
        //                {
        //                    if (RDR.errorRes.is_active == true && RDR.errorRes.mac_id == device.Mac_Address)
        //                    {
        //                        //if (device.Name == "APtBudB")
        //                        //{
        //                        //    Android.App.Application.Context.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionBluetoothSettings));
        //                        //}
        //                        var res = await DependencyService.Get<IBth>().Start(device.Mac_Address, 50, true);
        //                        if (res == "connected")
        //                        {
        //                            if(IsFirmwareUpdate)
        //                            {
        //                                var FirmwareVersion = await DependencyService.Get<IBth>().GetFirmware();
        //                                await this.Navigation.PushAsync(new FirmwareUpdatePage(FirmwareVersion));
        //                            }
        //                            if (TerminalCommands != null)
        //                            {
        //                                var commands = await DependencyService.Get<IBth>().SendTerminalCommands(TerminalCommands);
        //                                App.ConnectedVia = "BT";

        //                                DependencyService.Get<IToastMessage>().Show("------BT Connected------");

        //                                isconnected = false;
        //                                var device_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
        //                                Application.Current.MainPage = new NavigationPage(new MasterDetailView(device_id) { Detail = new NavigationPage(
        //                                    new TerminalPage(commands)) });
        //                                //await Navigation.PushAsync(new TerminalPage(commands));
        //                            }
        //                            else
        //                            {
        //                                var firmwareVersion = await DependencyService.Get<IBth>().Connect();
        //                                App.firmwareVersion = firmwareVersion;
        //                                if(!string.IsNullOrEmpty(firmwareVersion))
        //                                {
        //                                    App.ConnectedVia = "BT";

        //                                    DependencyService.Get<IToastMessage>().Show("------BT Connected------");

        //                                    //await Application.Current.MainPage.Navigation.PushAsync(new AppFeaturePage(jobCard, $"Firmware Version {firmwareVersion}"));
        //                                    isconnected = false;

        //                                    //Application.Current.MainPage = new MasterDetailPage()
        //                                    //{
        //                                    //    Detail = new NavigationPage(new AppFeaturePage(jobCard, firmwareVersion))
        //                                    //};


        //                                    await Navigation.PushAsync(new AppFeaturePage(jobCard, firmwareVersion, null, null, string.Empty));
        //                                    //await Navigation.PushModalAsync(new AppFeaturePage(jobCard, $"Firmware Version {firmwareVersion}"));
        //                                }
        //                                else
        //                                {
        //                                    await DisplayAlert("Alert", "Firmware not found", "OK");
        //                                    return;
        //                                }

        //                            }
        //                        }
        //                        else
        //                        {
        //                            //await this.DisplayAlert("", "Bluetooth Device already Connected Device", "OK");
        //                            //var res = await DependencyService.Get<IBth>().Start(device.Mac_Address, 250, true);
        //                            //var firmwareVersion = await DependencyService.Get<IBth>().Connect();
        //                            //App.ConnectedVia = "BT";

        //                            ////await Application.Current.MainPage.Navigation.PushAsync(new AppFeaturePage(jobCard, $"Firmware Version {firmwareVersion}"));
        //                            //await Navigation.PushAsync(new AppFeaturePage(jobCard, firmwareVersion));
        //                            //await Navigation.PushModalAsync(new AppFeaturePage(jobCard, $"Firmware Version {firmwareVersion}"));
        //                            //App.Current.MainPage = new NavigationPage(new ConnectionPage(jobCard));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        await DisplayAlert("Please Check Internet Connection !!", "Dongle not Approved. Please contact Admin ( You can use last Dongle ) !!", "OK");
        //                    }
        //                }
        //                else
        //                {
        //                    await DisplayAlert("", "Dongle not Approved. Please contact Admin !!", "OK");
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //}

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

        private void BTNRefresh_Clicked(object sender, EventArgs e)
        {
            BindingContext = viewModel = new BluetoothDevicesViewModel();
            viewModel.GetDevices();

            //Intent intentOpenBluetoothSettings = new Intent();
            //intentOpenBluetoothSettings.SetAction(Android.Provider.Settings.ActionBluetoothSettings);
            //Android.App.Application.Context.StartActivity(intentOpenBluetoothSettings);
        }
    }
}