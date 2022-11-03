using Android.Bluetooth;
using Android.Content;
using Bajaj.Model;
using System.Linq;

namespace Bajaj.Droid.Dependencies
{
    public class BluetoothDeviceReceiver : BroadcastReceiver
    {
        public static BluetoothAdapter Adapter => BluetoothAdapter.DefaultAdapter;

        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            //ObservableCollection<BluetoothDevicesModel> list = new ObservableCollection<BluetoothDevicesModel>();
            // Found a device
            switch (action)
            {
                case BluetoothDevice.ActionFound:
                    // Get the device
                    var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                    if (device.Name == "OBDII BT Dongle" || device.Name == "APtBudB" || device.Name == "wikitek")
                    {
                        var list = App.bluetoothDeviceds.FirstOrDefault(x => x.Mac_Address == device.Address);
                        if (list == null)
                        {
                            App.bluetoothDeviceds.Add(
                                new BluetoothDevicesModel
                                {
                                    Mac_Address = device.Address,
                                    Name = device.Name,
                                });
                        }


                        //App.bluetoothDeviceds = new ObservableCollection<BluetoothDevicesModel>(list.Distinct(new ItemEqualityComparer()).ToList() as List<BluetoothDevicesModel>); 
                        BluetoothStatic.bluetoothDevices.Add(device);
                        // Only update the adapter with items which are not bonded
                        //if (device.BondState != Bond.Bonded)
                        //{
                        //    MainActivity.GetInstance().UpdateAdapter(new DataItem(device.Name, device.Address));
                        //}
                    }
                    break;
                case BluetoothAdapter.ActionDiscoveryStarted:
                    //MainActivity.GetInstance().UpdateAdapterStatus("Discovery Started...");
                    break;
                case BluetoothAdapter.ActionDiscoveryFinished:
                    App.isRunningTimer = false;
                    //MainActivity.GetInstance().UpdateAdapterStatus("Discovery Finished.");
                    break;
                default:
                    break;
            }
        }
        //public override void OnReceive(Context context, Intent intent)
        //{
        //    var action = intent.Action;

        //    // Found a device
        //    switch (action)
        //    {
        //        case BluetoothDevice.ActionFound:
        //            // Get the device
        //            var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
        //            App.bluetoothDeviceds.Add(
        //                new BluetoothDeviced
        //                {
        //                    Address = device.Address,
        //                    Name = device.Name,
        //                });
        //            BluetoothStatic.bluetoothDevices.Add(device);
        //            if (device.BondState != Bond.Bonded)
        //            {
        //                MainActivity.GetInstance().UpdateAdapter(new DataItem(device.Name, device.Address));
        //            }

        //            break;
        //        case BluetoothAdapter.ActionDiscoveryStarted:
        //            MainActivity.GetInstance().UpdateAdapterStatus("Discovery Started...");
        //            break;
        //        case BluetoothAdapter.ActionDiscoveryFinished:
        //            MainActivity.GetInstance().UpdateAdapterStatus("Discovery Finished.");
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //class ItemEqualityComparer : IEqualityComparer<BluetoothDevicesModel>
        //{
        //    public bool Equals(BluetoothDevicesModel x, BluetoothDevicesModel y)
        //    {
        //        // Two items are equal if their keys are equal.
        //        return x.Mac_Address== y.Mac_Address;
        //    }

        //    public int GetHashCode(BluetoothDevicesModel obj)
        //    {
        //        return obj.Mac_Address.GetHashCode();
        //    }

        //}
    }
}