using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bajaj.ViewModel
{
    public class BluetoothDevicesViewModel : BaseViewModel
    {
        ApiServices services;
        public BluetoothDevicesViewModel()
        {
            services = new ApiServices();
            //BluetoothDeviceList = new ObservableCollection<BluetoothDevicesModel>();
            //foreach (var item in App.bluetoothDeviceds)
            //{
            //    BluetoothDeviceList.Add(
            //        new BluetoothDevicesModel
            //        {
            //            Mac_Address = item.Address,
            //            Name = item.Name
            //        });
            //}

            wifi_device = new List<BluetoothDevicesModel>();
            BluetoothDeviceList = new ObservableCollection<BluetoothDevicesModel>();
            GetDevices();
            //{
            //    new BluetoothDevicesModel
            //    {
            //        Mac_Address = "50:80:4A:D9:C7:F9",
            //        Name = "APTBudB"
            //    },
            //    new BluetoothDevicesModel
            //    {
            //        Mac_Address = "50:80:4A:D9:DA:BB",
            //        Name ="APTBudB"
            //    },
            //};
        }

        private ObservableCollection<BluetoothDevicesModel> bluetoothDeviceList;
        public ObservableCollection<BluetoothDevicesModel> BluetoothDeviceList
        {
            get => bluetoothDeviceList;
            set
            {
                bluetoothDeviceList = value;
                OnPropertyChanged("BluetoothDeviceList");
            }
        }

        private List<BluetoothDevicesModel> _wifi_device;
        public List<BluetoothDevicesModel> wifi_device
        {
            get => _wifi_device;
            set
            {
                _wifi_device = value;
                OnPropertyChanged("wifi_device");
            }
        }

        private bool _RefreshIsVisible;
        public bool RefreshIsVisible
        {
            get { return _RefreshIsVisible; }
            set { _RefreshIsVisible = value; OnPropertyChanged("RefreshIsVisible"); }
        }

        public async void get_wifi_device_list()
        {

        }
        public void GetDevices()
        {
            try
            {
                Task.Run(() =>
                {
                    //var device = DependencyService.Get<IBth>().PairedDevices();
                    BluetoothDeviceList = App.bluetoothDeviceds = new ObservableCollection<BluetoothDevicesModel>();

                    var device = DependencyService.Get<IBlueToothDevices>().SearchBT();

                    App.isRunningTimer = true;

                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                    {
                        BluetoothDeviceList = App.bluetoothDeviceds;
                        //if (BluetoothDeviceList.Count == 0)
                        //{
                        //    RefreshIsVisible = false;
                        //    DependencyService.Get<IBlueToothDevices>().SearchBT();
                        //}
                        //else
                        //{
                        //    RefreshIsVisible = true;
                        //}
                        return App.isRunningTimer;
                    });

                    //BluetoothDeviceList = App.bluetoothDeviceds;
                    //if (BluetoothDeviceList.Count == 0)
                    //{
                    //    RefreshIsVisible = false;
                    //    DependencyService.Get<IBlueToothDevices>().SearchBT();
                    //}
                    //else
                    //{
                    //    RefreshIsVisible = true;
                    //}
                });

            }
            catch (Exception ex)
            {
            }
        }

        public async Task<RegisterDongleRespons> RegisterDongle(RegisterDongleModel registerDongleModel)
        {
            try
            {
                var res = services.RegisterDongle(registerDongleModel, App.JwtToken);
                return res.Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
