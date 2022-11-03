using Bajaj.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Bajaj.Interfaces
{
    public interface IWifiConnector
    {
        Task<ObservableCollection<BluetoothDevicesModel>> ConnectToWifi(string ssid, string password);
    }
}
