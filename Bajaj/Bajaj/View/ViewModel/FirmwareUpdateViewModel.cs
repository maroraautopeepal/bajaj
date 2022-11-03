using Acr.UserDialogs;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class FirmwareUpdateViewModel : BaseViewModel
    {
        ApiServices services;
        Page page;
        public FirmwareUpdateViewModel(Page page, string FirmWareVersion)
        {
            try
            {
                services = new ApiServices();
                this.page = page;
                if (string.IsNullOrEmpty(FirmWareVersion))
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        //Task.Run(async () =>
                        //{
                        try
                        {
                            var result = await CheckDongleConnection();
                            if (result)
                            {
                                FirmwareUpdateModel model = new FirmwareUpdateModel
                                {
                                    type = "obd_dongle"
                                };
                                //await services.UpdateFirmware(model);
                                CurrFV = await DependencyService.Get<Interfaces.IConnectionWifi>().GetFirmware1();
                                var Result = await services.UpdateFirmware(model);

                                if (string.IsNullOrEmpty(Result.error))
                                {
                                    NewFV = Result.data.fotax_version;
                                    Url = "https://159.89.167.72" + Result.data.fotax_firmware;
                                    if (NewFV == CurrFV)
                                    {
                                        BtnEnable = false;
                                        Message = "Firmware versions are same\nNo Need to update firmware version";
                                    }
                                    else
                                    {
                                        BtnEnable = true;
                                        Message = "Firmware versions are not same\nYou Need to update firmware version.";
                                    }
                                }
                                
                            }
                            else
                            {
                                result = await page.DisplayAlert("Alert", "Your dongle is not connected\nDo you want to conncet dongle", "Ok", "Cancel");
                                if (result)
                                {
                                    var wifiConnector = await DependencyService.Get<Interfaces.IConnectionWifi>().get_device_list();
                                    await page.Navigation.PushAsync(new WifiDevicesPage(null, wifiConnector, null, true));
                                    //await page.Navigation.PushAsync(new BluetoothDevicesPage(null, null, true));
                                }
                                else
                                {
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UserDialogs.Instance.Alert(ex.Message, "Alert", "Ok");
                        }
                        //});
                    });
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message, "Alert", "Ok");
            }
        }

        #region Properties
        private string newFV;
        public string NewFV
        {
            get => newFV;
            set
            {
                newFV = value;
                OnPropertyChanged("NewFV");
            }
        }

        private string currFV;
        public string CurrFV
        {
            get => currFV;
            set
            {
                currFV = value;
                OnPropertyChanged("CurrFV");
            }
        }


        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }

        private string url;
        public string Url
        {
            get => url;
            set
            {
                url = value;
                OnPropertyChanged("Url");
            }
        }

        private bool btnEnable;
        public bool BtnEnable
        {
            get => btnEnable;
            set
            {
                btnEnable = value;
                OnPropertyChanged("BtnEnable");
            }
        }
        #endregion


        #region Methods
        public async Task<bool> CheckDongleConnection()
        {
            try
            {
                bool Result = false;
                App.ConnectedVia = "WIFI";
                switch (Device.RuntimePlatform)
                {
                    case Device.Android:
                        if (App.ConnectedVia == "USB")
                        {
                            //read_dtc = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadDtc(read_dtc_index);
                        }
                        else if (App.ConnectedVia == "BT")
                        {
                            Result = await DependencyService.Get<Interfaces.IBth>().CheckBtConnection();
                        }
                        else
                        {
                            Result = await DependencyService.Get<Interfaces.IConnectionWifi>().CheckConnection();
                        }
                        break;
                    case Device.UWP:
                        //read_dtc = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadDtc(read_dtc_index);
                        break;
                }
                return Result;
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message, "Alert", "Ok");
                return false;
            }
        }
        // Types
        //['fotax', 'coloc', 'obd_dongle', 'windows', 'android', 'linux']
        string ActualVersion = string.Empty;
        string SubVersion = string.Empty;
        public async void UpdateFirmware(string FirmwareVersion)
        {
            ActualVersion = string.Empty;


            FirmwareUpdateModel model = new FirmwareUpdateModel
            {
                type = "obd_dongle"
            };
            CurrFV = FirmwareVersion;
            var Result = await services.UpdateFirmware(model);

            if (string.IsNullOrEmpty(Result.error))
            {
                NewFV = Result.data.fotax_version;
                Url = "https://159.89.167.72" + Result.data.fotax_firmware;
                if (NewFV == CurrFV)
                {
                    BtnEnable = false;
                    Message = "Firmware versions are same\nNo Need to update firmware version";
                }
                else
                {
                    BtnEnable = true;
                    Message = "Firmware versions are not same\nYou Need to update firmware version.";
                }
            }
            else
            {
                BtnEnable = false;
                await page.DisplayAlert("Alert", Result.error, "Ok");
            }

        }

        #endregion
    }
}