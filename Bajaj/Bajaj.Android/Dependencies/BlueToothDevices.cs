using Android.Bluetooth;
using Android.Content;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
//using Java.IO;
using Java.Util;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(BlueToothDevices))]
namespace Bajaj.Droid.Dependencies
{
    public class BlueToothDevices : IBlueToothDevices
    {
        // First Security  50 0C 47 56 8A FE 56 21 4E 23 80 00 FF C3
        // Second Security  50 0C 65 EF A8 65 74 FE 9A 89 00 18 CE 16
        //private byte[] buffer;

        BluetoothSocket BthSocket = null;
        BluetoothDeviceReceiver receiver;
        //BufferedReader inReader = null;
        //BufferedWriter outReader = null;
        //buffer = new byte[1024];

        public Task SearchBT()
        {
            try
            {
                receiver = new BluetoothDeviceReceiver();
                RegisterBluetoothReceiver();
                StartScanning();
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void RegisterBluetoothReceiver()
        {
            try
            {
                var context = Android.App.Application.Context;
                context.RegisterReceiver(receiver, new IntentFilter(BluetoothDevice.ActionFound));
                context.RegisterReceiver(receiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryStarted));
                context.RegisterReceiver(receiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            }
            catch (Exception ex)
            {
            }
        }

        private static void StartScanning()
        {
            if (!BluetoothDeviceReceiver.Adapter.IsDiscovering)
                BluetoothDeviceReceiver.Adapter.StartDiscovery();
        }

        //public bool PairDevice(string bt_Name, string bt_add)
        //{
        //    bool returnValue = false;
        //    try
        //    {
        //        BluetoothDevice device = BluetoothStatic.bluetoothDevices.FirstOrDefault(x => x.Name == bt_Name);//bluetoothDevice;
        //        BluetoothDevice paired_device = BluetoothAdapter.DefaultAdapter.BondedDevices.
        //            FirstOrDefault(x => x.Name == bt_Name && x.Address == bt_add);
        //        returnValue = true;
        //        if (paired_device == null)
        //        {
        //            returnValue = device.CreateBond();
        //            var bond = device.BondState;
        //        }
        //        return returnValue;
        //    }
        //    catch (Exception ex)
        //    {
        //        //BthSocket.Close();
        //        return returnValue;
        //    }
        //}

        public bool PairDevice(string bt_Name, string bt_add)
        {
            bool returnValue = false;
            try
            {
                BluetoothDevice device = BluetoothStatic.bluetoothDevices.FirstOrDefault(x => x.Name == bt_Name);//bluetoothDevice;
                BluetoothDevice paired_device = BluetoothAdapter.DefaultAdapter.BondedDevices.
                    FirstOrDefault(x => x.Name == bt_Name && x.Address == bt_add);
                returnValue = true;
                if (paired_device == null)
                {

                    returnValue = device.CreateBond();
                    var state = device.BondState;
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                BthSocket.Close();
                return returnValue;
            }
        }
        public bool ConnectDevice(string bt_Name, string bt_add)
        {
            bool returnValue = false;
            try
            {
                returnValue = ConnectDongle(bt_Name, bt_add);
                if (!returnValue)
                {
                    returnValue = ConnectDongle(bt_Name, bt_add);
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                BthSocket.Close();
                return returnValue;
            }
        }

        private async Task ConnectDevice(string name)
        {
            BluetoothDevice device = null;
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            BluetoothSocket bthSocket = null;
            BluetoothServerSocket bthServerSocket = null;

            UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
            bthServerSocket = adapter.ListenUsingRfcommWithServiceRecord("TLCI Barcode Scanner", uuid);

            //_cancellationToken = new CancellationTokenSource();

            //while (_cancellationToken.IsCancellationRequested == false)
            //{
            //    try
            //    {
            //        Thread.Sleep(250);

            //        adapter = BluetoothAdapter.DefaultAdapter;

            //        if (adapter == null)
            //            Debug.Write("No bluetooth adapter found!");
            //        else
            //            Debug.Write("Adapter found!");

            //        if (!adapter.IsEnabled)
            //            Debug.Write("Bluetooth adapter is not enabled.");
            //        else
            //            Debug.Write("Adapter found!");

            //        Debug.Write("Try to connect to " + name);

            //        foreach (var bondedDevice in adapter.BondedDevices)
            //        {
            //            Debug.Write("Paired devices found: " + bondedDevice.Name.ToUpper());

            //            if (bondedDevice.Name.ToUpper().IndexOf(name.ToUpper()) >= 0)
            //            {
            //                Debug.Write("Found " + bondedDevice.Name + ". Try to connect with it!");
            //                device = bondedDevice;
            //                Debug.Write(bondedDevice.Type.ToString());
            //                break;
            //            }
            //        }

            //        if (device == null)

            //        else
            //        {
            //            bthSocket = bthServerSocket.Accept();

            //            adapter.CancelDiscovery();

            //            if (bthSocket != null)
            //            {


            //                if (bthSocket.IsConnected)
            //                {
            //                    var mReader = new InputStreamReader(bthSocket.InputStream);
            //                    var buffer = new BufferedReader(mReader);

            //                    while (ct.IsCancellationRequested == false)
            //                    {
            //                        if (MessageToSend != null)
            //                        {
            //                            var chars = MessageToSend.ToCharArray();
            //                            var bytes = new List<byte>();

            //                            foreach (var character in chars)
            //                            {
            //                                bytes.Add((byte)character);
            //                            }

            //                            await bthSocket.OutputStream.WriteAsync(bytes.ToArray(), 0, bytes.Count);


            //                        }
            //                    }

            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //    finally
            //    {
            //        if (bthSocket != null)
            //            bthSocket.Close();

            //        device = null;
            //        adapter = null;
            //    }
            //}
        }

        public bool ConnectDongle(string bt_Name, string bt_add)
        {

            try
            {

                //BluetoothDevice device = BluetoothStatic.bluetoothDevices.FirstOrDefault(x => x.Name == bluetoothDeviceName);//bluetoothDevice;

                //device.CreateBond();
                BluetoothDevice paired_device = BluetoothAdapter.DefaultAdapter.BondedDevices.
                    FirstOrDefault(x => x.Name == bt_Name && x.Address == bt_add);
                if (paired_device != null)
                {
                    UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
                    //UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                    if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
                        BthSocket = paired_device.CreateInsecureRfcommSocketToServiceRecord(uuid);
                    else
                        BthSocket = paired_device.CreateRfcommSocketToServiceRecord(uuid);

                    if (BthSocket != null)
                    {
                        //await BthSocket.ConnectAsync();
                        BthSocket.Connect();


                        if (BthSocket.IsConnected)
                        {
                            //List<byte> outputList = new List<byte>();
                            //byte[] ba = Encoding.Default.GetBytes("500C47568AFE56214E238000FFC3");
                            //byte[] ba = Encoding.Default.GetBytes("500C65EFA86574FE9A890018CE16");
                            //outputStream = BthSocket.OutputStream;

                            //inReader = new BufferedReader(new InputStreamReader(BthSocket.InputStream));
                            //outReader = new BufferedWriter(new OutputStreamWriter(BthSocket.OutputStream));
                            //outReader.Write("500C47568AFE56214E238000FFC3");

                            //outReader.Flush();
                            //Thread.Sleep(5 * 1000);
                            //var s = inReader.Ready();

                            //byte[] ba = StringToByteArray("500C47568AFE56214E238000FFC3");
                            //var resp = BthSocket.OutputStream.WriteAsync(ba.ToArray(), 0, ba.Length);
                            //BthSocket.OutputStream.FlushAsync();
                            //Thread.Sleep(1000);
                            //var read_resp = BthSocket.InputStream.ReadAsync(ba.ToArray(), 0, ba.Length).Result;
                        }
                        else
                        {

                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public async Task EndScanning()
        {
            BluetoothDeviceReceiver.Adapter.CancelDiscovery();
        }

        public void GetDongles()
        {
            try
            {
                BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null)
                    throw new Exception("No Bluetooth adapter found.");

                if (!adapter.IsEnabled)
                    throw new Exception("Bluetooth adapter is not enabled.");

                BluetoothDevice device = (from bd in adapter.BondedDevices
                                          where bd.Name == "NameOfTheDevice"
                                          select bd).FirstOrDefault();

                if (device == null)
                    throw new Exception("Named device not found.");
            }
            catch (Exception ex)
            {
            }
        }

        public string SendSecurityCommand(string SequirityCommand)
        {
            try
            {
                if (BthSocket.IsConnected)
                {
                    byte[] ba = StringToByteArray(SequirityCommand);
                    var resp = BthSocket.OutputStream.WriteAsync(ba.ToArray(), 0, ba.Length);
                    BthSocket.OutputStream.FlushAsync();
                    Thread.Sleep(1000);
                    //var read_resp = BthSocket.InputStream.ReadAsync(ba.ToArray(), 0, ba.Length).Result;
                }
                return "pass";
            }
            catch (Exception ex)
            {
                return "failed";
            }
        }

        public string SendDongleVersionCommand(string DongleVersionCommand)
        {
            try
            {
                if (BthSocket.IsConnected)
                {
                    byte[] ba = StringToByteArray(DongleVersionCommand);
                    var resp = BthSocket.OutputStream.WriteAsync(ba.ToArray(), 0, ba.Length);
                    BthSocket.OutputStream.FlushAsync();
                    Thread.Sleep(1000);
                    //var read_resp = BthSocket.InputStream.ReadAsync(ba.ToArray(), 0, ba.Length).Result;
                }
                return "pass";
            }
            catch (Exception ex)
            {
                return "failed";
            }
        }

        public string SendCommandToECU(string EcuCommand)
        {
            try
            {
                if (BthSocket.IsConnected)
                {
                    byte[] ba = StringToByteArray(EcuCommand);
                    var resp = BthSocket.OutputStream.WriteAsync(ba.ToArray(), 0, ba.Length);
                    BthSocket.OutputStream.FlushAsync();
                    Thread.Sleep(1000);
                    //var read_resp = BthSocket.InputStream.ReadAsync(ba.ToArray(), 0, ba.Length).Result;
                }
                return "pass";
            }
            catch (Exception ex)
            {
                return "failed";
            }
        }

    }
}