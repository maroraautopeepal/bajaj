using SML.Interfaces;
using SML.Model;
using SML.UWP.Dependencies;
using SML.UWP.WifiConf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.NetworkOperators;
using Xamarin.Forms;

[assembly: Dependency(typeof(WifiConfiguration))]
namespace SML.UWP.Dependencies
{
    public class WifiConfiguration : IConnectionWifi
    {
        private WiFiAdapter wifiAdapter;
        private WiFiAvailableNetwork wifiSelectedNetwork;
        public async Task<ObservableCollection<BluetoothDevicesModel>> get_device_list()
        {
            try
            {
                ObservableCollection<BluetoothDevicesModel> available_dogles = new ObservableCollection<BluetoothDevicesModel>();
                var accessAllowed = WiFiAdapter.RequestAccessAsync();

                var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                wifiAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);

                await wifiAdapter.ScanAsync();
                var wifiList = new List<string>();
                foreach (var network in wifiAdapter.NetworkReport.AvailableNetworks)
                {
                    available_dogles.Add(
                        new BluetoothDevicesModel
                        {
                            Name = network.Ssid,
                            Mac_Address = network.Bssid
                        }
                        );
                }
                return available_dogles;
            }
            catch (Exception ex)
            {
                return null;
            }


        }

        public async Task<ObservableCollection<BluetoothDevicesModel>> enable_hotspots()
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var hotspot = await HotspotManager.CreateAsync("Redmi", "123456789");

                hotspot.ClientConnected += (_, clientInfo) => Console.WriteLine($"CON | {ToString(clientInfo)}");
                hotspot.ClientDisconnected += (_, clientInfo) => Console.WriteLine($"DIS | {ToString(clientInfo)}");

                Console.Error.WriteLine("Starting hotspot ... press \"q\" and Enter to quit");

                var hotspotTask = hotspot.RunAsync(cancellationTokenSource.Token);

                // wait for 'q'
                while (Convert.ToChar(Console.Read()) != 'q') ;

                Console.Error.WriteLine("Stopping hotspot ...");
                cancellationTokenSource.Cancel();
                hotspotTask.GetAwaiter().GetResult();
                //wiFiDirectHotspotManager_ = new WiFiDirectHotspotManager();
                //wiFiDirectHotspotManager_.AdvertisementAborted += OnAdvertisementAborted;
                //wiFiDirectHotspotManager_.AdvertisementStarted += OnAdvertisementStarted;
                //wiFiDirectHotspotManager_.AdvertisementStopped += OnAdvertisementStopped;
                //wiFiDirectHotspotManager_.AsyncException += OnAsyncException;
                //wiFiDirectHotspotManager_.DeviceConnected += OnDeviceConnected;
                //wiFiDirectHotspotManager_.DeviceDisconnected += OnDeviceDisconnected;

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static string ToString(NetworkOperatorTetheringClient client)
        {
            string hostNames = string.Join(" | ", client.HostNames);
            return $"{client.MacAddress} | {hostNames}";
        }

        public void Connect_dongle()
        {
            throw new NotImplementedException();
        }

        public Task<string> get_ipaddress()
        {
            throw new NotImplementedException();
        }

        public Task<string> Write_SSIDPassword(string RouterSSID, string RouterPassword)
        {
            throw new NotImplementedException();
        }

        public Task Connect_WIFI(string IP, string Port)
        {
            throw new NotImplementedException();
        }

        public Task<string> Connect(string IP)
        {
            throw new NotImplementedException();
        }

        public Task<ReadDtcResponseModel> ReadDtc(string indexKey)
        {
            throw new NotImplementedException();
        }

        public Task<string> ClearDtc(string indexKey)
        {
            throw new NotImplementedException();
        }

        public Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<ReadParameterPID> pidList)
        {
            throw new NotImplementedException();
        }

        public Task<ObservableCollection<WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<WriteParameterPID> pidList)
        {
            throw new NotImplementedException();
        }

        public Task<object> StartECUFlashing(string flashJson, Ecu2 ecu2, SeedkeyalgoFnIndex sklFIN, List<EcuMapFile> ecu_map_file)
        {
            throw new NotImplementedException();
        }
    }

}
