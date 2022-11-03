using Acr.UserDialogs;
using Bajaj.Model;
using Bajaj.ViewModel;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Bajaj.View.TerminalPart;
using Bajaj.Interfaces;
using Plugin.Connectivity;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WifiDevicesPage : DisplayAlertPage
    {
        DongleViewModel dongleViewModel;
        BluetoothDevicesViewModel viewModel;
        JobCardListModel jobCard;
        string[] TerminalCommands;
        bool IsFirmwareUpdate = false;
        List<BluetoothDevicesModel> tempWifiList;

        public WifiDevicesPage(JobCardListModel jobCardSession, ObservableCollection<BluetoothDevicesModel> available_dogles, string[] TerminalCommands, bool IsFirmwareUpdate)
        {
            try
            {
                InitializeComponent();
                jobCard = new JobCardListModel();
                this.IsFirmwareUpdate = IsFirmwareUpdate;
                this.TerminalCommands = TerminalCommands;
                BindingContext = viewModel = new BluetoothDevicesViewModel();
                jobCard = jobCardSession;
                tempWifiList = new List<BluetoothDevicesModel>();
                //viewModel.wifi_device = available_dogles.Where(x=>x.Name == "\"OBD2-0AD4\"").ToList();
                viewModel.wifi_device = available_dogles.Where(e => e.Name.Contains("obd2") || e.Name.Contains("vocom")).ToList();
                tempWifiList = viewModel.wifi_device;
            }
            catch (Exception ex)
            {

            }
        }


        string DebugTag = "Wifi Communication";
        [Obsolete]
        private async void connect_dongle_clicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (App.ConnectedVia == "RP1210")
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        try
                        {
                            await Task.Delay(100);
                            var item = (BluetoothDevicesModel)((ImageButton)sender).BindingContext;

                            var firmwareVersion = await DependencyService.Get<Interfaces.IConnectionRP>().ConnectDevice(item.Ip);

                            App.firmwareVersion = firmwareVersion;

                            if (firmwareVersion == "")
                            {
                                await DisplayAlert("", "Failled to connect", "Ok");
                            }
                            else
                            {
                                await DisplayAlert("", "Client Connected", "Ok");
                                await Navigation.PushAsync(new AppFeaturePage());
                            }

                            //bool device_connected = App.class1.ConnectDevice(item.Ip);

                            //if (device_connected)
                            //{
                            //    bool client_status = App.class1.ClientConnect("ISO15765:Baud=500000,Channel=1");
                            //    if (client_status)
                            //    {
                            //        await DisplayAlert("", "Client Connected", "Ok");

                            //        Debug.WriteLine("------ Get Firmware -----", DebugTag);
                            //        string fw_version = App.class1.GetFirmwareVersion();
                            //        Debug.WriteLine(fw_version, DebugTag);

                            //        Debug.WriteLine("------ Set Flow Control -----", DebugTag);
                            //        //val =command_id + msg_type_code + self.rxCAN_id + self.rx_extende_addrs + self.txCAN_id + self.tx_extende_addrs + BlockSize + STmin + STMIN_tx

                            //        byte[] val = HexStringToByteArray("0022" + "02" + "000007e8" + "00" + "000007e0" + "00" + "00" + "00" + "ffff");
                            //        string fw_version1 = App.class1.SendCommand(val);
                            //        Debug.WriteLine(fw_version1, DebugTag);


                            //        Debug.WriteLine("------ Set Message Filter -----", DebugTag);
                            //        // if protocol is "ISO15765" the put the "exteded_mask" else skip "exteded_mask"
                            //        // if protocol is "ISO15765" the put the "rx_extende_addrs" else skip "rx_extende_addrs"

                            //        //val = command_id + msg_type_code + mask + ["", exteded_mask][self.protocol_name == "ISO15765"] + self.rxCAN_id + ["", self.rx_extende_addrs][self.protocol_name == "ISO15765"]

                            //        byte[] val2 = HexStringToByteArray("0009" + "02" + "ffffffff" + "ff" + "000007e8" + "00");
                            //        string fw_version2 = App.class1.SendCommand(val2);
                            //        Debug.WriteLine(fw_version2, DebugTag);

                            //        //await Navigation.PushAsync(new FunctionPage(class1));
                            //        await Navigation.PushAsync(new AppFeaturePage(jobCard, fw_version, null, null, string.Empty));
                            //    }
                            //    else
                            //    {
                            //        await DisplayAlert("", "Failled to connect", "Ok");
                            //    }
                            //}
                            //else
                            //{
                            //    await DisplayAlert("", "Device not connected", "Ok");
                            //}
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                }
                else
                {
                    try
                    {
                        using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                        {
                            await Task.Delay(100);
                            var device = (BluetoothDevicesModel)((ImageButton)sender).BindingContext;
                            if (IsFirmwareUpdate)
                            {
                                var mac_id = await DependencyService.Get<Interfaces.IConnectionWifi>().GetDongleMacID(device.Ip, false);

                                if (!string.IsNullOrEmpty(mac_id))
                                {
                                    RegisterDongleModel RDM = new RegisterDongleModel
                                    {
                                        MacId = mac_id,
                                        device_type = "obd_dongle_wifie"
                                    };
                                    RegisterDongleRespons RDR = new RegisterDongleRespons();
                                    var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                                    if (isReachable)
                                    {
                                        RDR = await viewModel.RegisterDongle(RDM);
                                    }
                                    else
                                    {
                                        var JsonListData = DependencyService.Get<ISaveLocalData>().GetData("LastUsedDongle");
                                        var error = JsonConvert.DeserializeObject<RegDongleRespons>(JsonListData);
                                        RDR.errorRes = error;
                                    }

                                    if (RDR.errorRes != null)
                                    {
                                        if (RDR.errorRes.is_active == true && RDR.errorRes.mac_id == mac_id)
                                        {
                                            //       if (!string.IsNullOrEmpty(firmwareVersion[0]))
                                            //{
                                            var firmwareVersion = await DependencyService.Get<Interfaces.IConnectionWifi>().SetDongleProperties();
                                            if (!string.IsNullOrEmpty(firmwareVersion))
                                            {
                                                App.firmwareVersion = firmwareVersion;
                                                App.ConnectedVia = "WIFI";
                                                await this.Navigation.PushAsync(new FirmwareUpdatePage(firmwareVersion));
                                            }
                                            else
                                            {
                                                await this.DisplayAlert("Error", "Firmware version not found.", "OK");
                                            }

                                            //}
                                            //      else
                                            //    {
                                            //      await this.DisplayAlert("Connection not Established", "WIFI Dongle connection are not established", "OK");
                                            // }
                                        }
                                        else
                                        {
                                            await DisplayAlert("Please Check Internet Connection !!", "Dongle not Approved. Please contact Admin ( You can use last Dongle ) !!", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("", "Dongle not Approved. Please contact Admin !!", "OK");
                                    }
                                }
                                else
                                {
                                    await this.DisplayAlert("Connection not Established", "WIFI Dongle connection are not established", "OK");
                                }
                                //var FirmwareVersion = await DependencyService.Get<IConnectionWifi>().GetFirmware(viewModel.wifi_device.FirstOrDefault().Ip);
                                //await this.Navigation.PushAsync(new FirmwareUpdatePage(FirmwareVersion));
                            }
                            else if (TerminalCommands == null)
                            {
                                var mac_id = await DependencyService.Get<Interfaces.IConnectionWifi>().GetDongleMacID(device.Ip, false);

                                if (!string.IsNullOrEmpty(mac_id))
                                {
                                    RegisterDongleModel RDM = new RegisterDongleModel
                                    {
                                        MacId = mac_id,
                                        device_type = "obd_dongle_wifie"
                                    };
                                    RegisterDongleRespons RDR = new RegisterDongleRespons();
                                    var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                                    if (isReachable)
                                    {
                                        RDR = await viewModel.RegisterDongle(RDM);
                                    }
                                    else
                                    {
                                        var JsonListData = DependencyService.Get<ISaveLocalData>().GetData("LastUsedDongle");
                                        var error = JsonConvert.DeserializeObject<RegDongleRespons>(JsonListData);
                                        RDR.errorRes = error;
                                    }

                                    //RDR.errorRes = new RegDongleRespons();
                                    if (RDR.errorRes != null)
                                    {
                                        //RDR.errorRes.is_active = true;
                                        //RDR.errorRes.mac_id = mac_id;

                                        if (RDR.errorRes.is_active == true && RDR.errorRes.mac_id == mac_id)
                                        {
                                            //       if (!string.IsNullOrEmpty(firmwareVersion[0]))
                                            //{
                                            var firmwareVersion = await DependencyService.Get<Interfaces.IConnectionWifi>().SetDongleProperties();
                                            if (!string.IsNullOrEmpty(firmwareVersion))
                                            {
                                                App.firmwareVersion = firmwareVersion;
                                                App.ConnectedVia = "WIFI";
                                                await Navigation.PushAsync(new AppFeaturePage());
                                            }
                                            else
                                            {
                                                await this.DisplayAlert("Error", "Firmware version not found.", "OK");
                                            }

                                            //}
                                            //      else
                                            //    {
                                            //      await this.DisplayAlert("Connection not Established", "WIFI Dongle connection are not established", "OK");
                                            // }
                                        }
                                        else
                                        {
                                            await DisplayAlert("Please Check Internet Connection !!", "Dongle not Approved. Please contact Admin ( You can use last Dongle ) !!", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("", "Dongle not Approved. Please contact Admin !!", "OK");
                                    }
                                }
                                else
                                {
                                    await this.DisplayAlert("Connection not Established", "WIFI Dongle connection are not established", "OK");
                                }
                            }
                            else
                            {
                                var commands = await DependencyService.Get<Interfaces.IConnectionWifi>().SendTerminalCommands(device.Ip, TerminalCommands);
                                if (commands != null)
                                {
                                    if (jobCard == null)
                                    {
                                        App.ConnectedVia = "WIFI";
                                        var device_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
                                        Application.Current.MainPage = new NavigationPage(new MasterDetailView(device_id) { Detail = new NavigationPage(new TerminalPage(commands)) });
                                        //await Navigation.PushAsync(new AppFeaturePage(jobCard, await firmwareVersion, null, null, string.Empty));
                                    }
                                }
                                else
                                {
                                    await this.DisplayAlert("Connection not Established", "WIFI Dongle connection are not established", "OK");
                                }
                            }
                            //WifiDevidesItem.SelectedItem = null;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        WifiDevidesItem.SelectedItem = null;
                    }

                }

            });
        }
        private async void enable_hotspot(object sender, EventArgs e)
        {
            //await DependencyService.Get<Interfaces.IConnectionWifi>().enable_hotspots();         
            var result = await DisplayAlert("Alert", "This Step to approve you to write the SSID and Password of the internet hotspot into your WIFI Dongle. To do this put your dongle in hotspot mode and connect phone to the dongle (OBD2_....) ?", "Ok", "Cancel");
            if (result)
            {
                await Task.Run(() =>
                {
                    DependencyService.Get<Interfaces.IConnectionWifi>().Connect_dongle();
                });

                await PopupNavigation.PushAsync(new PopupPages.ConfigureWifiDongle());
            }
        }

        private void add_device(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var input = await Acr.UserDialogs.UserDialogs.Instance.PromptAsync("Enter IP address of device", "IP Address", "Confirm", "Cancel");

                if (input.Ok)
                {
                    tempWifiList.Add(new BluetoothDevicesModel
                    {
                        Ip = input.Text.Trim(),
                        Mac_Address = input.Text.Trim(),
                        Name = "obd2"
                    });
                    viewModel.wifi_device = new List<BluetoothDevicesModel>();
                    viewModel.wifi_device = tempWifiList;
                }
            });
        }

        private void WifiDevidesItem_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            WifiDevidesItem.SelectedItem = null;
        }

        private byte[] HexStringToByteArray(String hex)
        {
            hex = hex.Replace(" ", "");
            int numberChars = hex.Length;
            if (numberChars % 2 != 0)
            {
                hex = "0" + hex;
                numberChars++;
            }
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}