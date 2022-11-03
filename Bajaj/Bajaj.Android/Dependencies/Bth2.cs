using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using APDiagnosticAndroid;
using APDiagnosticAndroid.Enums;
using APDiagnosticAndroid.Models;
using APDiagnosticAndroid.Structures;
//using APDongleCommWin;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using Bajaj.Model;
using Java.IO;
using Java.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Console = System.Console;
using Debug = System.Diagnostics.Debug;
using AutoELM327;
using APDongleCommAndroid;
using APDongleCommAnroid.Models;

[assembly: Xamarin.Forms.Dependency(typeof(Bth2))]
namespace Bajaj.Droid.Dependencies
{
    public class Bth2 : IBth
    {
        const string TARGET_UUID = "00001105-0000-1000-8000-00805f9b34fb";
        BluetoothSocket socket = null;
        OutputStreamInvoker outStream = null;
        InputStreamInvoker inStream = null;
        InputStreamReader inputStreamReader = null;
        BufferedReader bufferedReader = null;
        DongleCommWin dongleCommWin;
        UDSDiagnostic dSDiagnostic;

        APELMDongleComm elmDongleCommWin;
        UDSDiagnosticElm uDSDiagnosticElm;
        private APDiagnosticAndroid.Models.ReadDtcResponseModel readDTCResponse;

        private CancellationTokenSource _ct { get; set; }

        const int RequestResolveError = 1000;

        private Context context;

        public Bth2(Context context)
        {
            this.context = context;
        }

        public Bth2()
        {
        }

        public async Task<bool> CheckBtConnection()
        {
            if (socket == null)
                return false;

            return true;
        }

        public async Task<string> Start(string name, int sleepTime, bool readAsCharArray)
        {
            //Task.Run(() => Connect(name, sleepTime, readAsCharArray));
            //await Connect(name, sleepTime, readAsCharArray);

            if (socket == null)
            {
                await Task.Delay(100);
                var isCompletedSuccessfully = Task.Run(async () => loop(name, sleepTime, readAsCharArray)).Wait(TimeSpan.FromSeconds(70));

                if (isCompletedSuccessfully)
                {
                    return "connected";
                }
                else
                {
                    return "not connected";
                }
                //Task.Factory.StartNew(async () =>
                //{
                //    Connect(name, sleepTime, readAsCharArray);
                //}).Result.ConfigureAwait(false);
            }
            else
            {
                socket.Close();
                socket = null;
                return "not connected";
            }
        }

        public async void StartTesterPresent()
        {
            var result = await dongleCommWin.CAN_StartTP();
        }

        public async void StopTesterPresent()
        {
            var result = await dongleCommWin.CAN_StopTP();
        }

        public async Task<TestRoutineResponseModel> SetTestRoutineCommand(string seed_key, string write_para_index, string start_command)
        {
            TestRoutineResponseModel response = new TestRoutineResponseModel();
            try
            {
                object result = new object();
                SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), seed_key);
                WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), write_para_index);

                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.StartIdIOR(seed_index, write_index, start_command);
                    var res = JsonConvert.SerializeObject(result);
                    response = JsonConvert.DeserializeObject<TestRoutineResponseModel>(res);
                    return response;
                });

                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<TestRoutineResponseModel> ContinueIorTest(string seed_key, string write_para_index, string start_command, string request_command, string stop_command, bool test_condition, int bit_position, List<string> active_command,
           string stopped_command, string fail_command, bool is_stop, int time_base, bool is_timebase)
        {
            TestRoutineResponseModel response = new TestRoutineResponseModel();
            try
            {
                object result = new object();
                SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), seed_key);
                WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), write_para_index);

                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.IORTestParameters2(seed_index, write_index, start_command, request_command,
                        stop_command, test_condition, bit_position, active_command, stopped_command, fail_command, is_stop,
                        time_base, is_timebase);
                    var res = JsonConvert.SerializeObject(result);
                    response = JsonConvert.DeserializeObject<TestRoutineResponseModel>(res);
                    return response;
                });

                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<TestRoutineResponseModel> RequestIorTest(string request_command)
        {
            TestRoutineResponseModel response = new TestRoutineResponseModel();
            try
            {
                object result = new object();
                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.RequestIdIOR(request_command);
                    var res = JsonConvert.SerializeObject(result);
                    response = JsonConvert.DeserializeObject<TestRoutineResponseModel>(res);
                    return response;
                });

                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<TestRoutineResponseModel> StopIorTest(string stop_command)
        {
            TestRoutineResponseModel response = new TestRoutineResponseModel();
            try
            {
                object result = new object();
                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.StopIdIOR(stop_command);
                    var res = JsonConvert.SerializeObject(result);
                    response = JsonConvert.DeserializeObject<TestRoutineResponseModel>(res);
                    return response;
                });

                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task loop(string address, int sleepTime, bool readAsCharArray)
        {
            BluetoothDevice device = null;
            BluetoothDevice paired_device = null;
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

            _ct = new CancellationTokenSource();
            while (_ct.IsCancellationRequested == false)
            {
                try
                {
                    Thread.Sleep(sleepTime);

                    adapter = BluetoothAdapter.DefaultAdapter;

                    //List<BluetoothDevice> L = new List<BluetoothDevice>();
                    //foreach (BluetoothDevice d in adapter.BondedDevices)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("D: " + d.Name + " " + d.Address + " " + d.BondState.ToString());
                    //    L.Add(d);
                    //}
                    //device = L.Find(j => j.Address == name);

                    device = BluetoothStatic.bluetoothDevices.FirstOrDefault(x => x.Address == address);//bluetoothDevice;
                    paired_device = BluetoothAdapter.DefaultAdapter.BondedDevices.
                        FirstOrDefault(x => x.Address == address);

                    if (paired_device == null)
                    {
                        //var isCompletedSuccessfully = Task.Run(async () =>
                        var returnValue = device.CreateBond();
                        var state = device.BondState;
                        //).Wait(TimeSpan.FromSeconds(70));

                        //var paired = await Task.Run( () =>
                        //{
                        //    var returnValue = device.CreateBond();
                        //    var state = device.BondState;
                        //    return true;
                        //});
                        //if(paired)
                        //{
                        //    paired_device = BluetoothAdapter.DefaultAdapter.BondedDevices.
                        //   FirstOrDefault(x => x.Address == address);
                        //    connectionMethod(paired_device);
                        //}
                        //return;

                    }
                    if (device == null)
                    {
                        //System.Diagnostics.Debug.WriteLine("Named device not found.");
                        //Xamarin.Forms.MessagingCenter.Send<App, string>((App)Xamarin.Forms.Application.Current, "TerminalLog", "Named device not found.");
                    }
                    else
                    {
                        //connectionMethod(device);
                        UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
                        if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
                            socket = device.CreateInsecureRfcommSocketToServiceRecord(uuid);
                        else
                            socket = device.CreateRfcommSocketToServiceRecord(uuid);

                        if (socket != null)
                        {
                            socket.Connect();
                            if (socket.IsConnected)
                            {
                                //socket.OutputStream.ReadTimeout= 400;
                                //socket.InputStream.ReadTimeout = 400;
                                outStream = (OutputStreamInvoker)socket.OutputStream;
                                inputStreamReader = new InputStreamReader(socket.InputStream);
                                bufferedReader = new BufferedReader(inputStreamReader);
                                break;
                            }
                        }
                        else
                            System.Diagnostics.Debug.WriteLine("BthSocket = null");
                    }
                }
                catch (Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine("EXCEPTION: " + ex.Message);
                }
                finally
                {
                    //if (socket != null)
                    //    socket.Close();
                    //device = null;
                    //adapter = null;
                }
            }
            System.Diagnostics.Debug.WriteLine("Exit the external loop");
        }

        public void Cancel()
        {
            if (_ct != null)
            {
                System.Diagnostics.Debug.WriteLine("Send a cancel to task!");
                _ct.Cancel();
            }
        }
        public ObservableCollection<BluetoothDevicesModel> PairedDevices()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            ObservableCollection<BluetoothDevicesModel> devices = new ObservableCollection<BluetoothDevicesModel>();
            //devices.Add("");
            var device_list = adapter.BondedDevices.Where(x => x.Name == "OBDII BT Dongle" || x.Name == "APtBudB");
            foreach (var bd in device_list)
                devices.Add(new BluetoothDevicesModel { Name = bd.Name, Mac_Address = bd.Address });
            BluetoothDevice bluetoothDevice = adapter.GetRemoteDevice("");
            return devices;
        }


        public async Task<string> GetDongleMacID(bool is_disconnct)
        {
            string mac_id = string.Empty;
            try
            {
                if (!is_disconnct)
                {

                    string ProtocolNameValue = StaticData.ecu_info.FirstOrDefault().protocol.name;//.protocol.name;
                    string value = ProtocolNameValue.Replace("-", "_");

                    tx_header_temp = StaticData.ecu_info.FirstOrDefault().tx_header;
                    rx_header_temp = StaticData.ecu_info.FirstOrDefault().rx_header;

                    switch (value)
                    {
                        case "ISO15765_250KB_11BIT_CAN":
                            ProtocolValue = 00;
                            break;
                        case "ISO15765_250Kb_29BIT_CAN":
                            ProtocolValue = 01;
                            break;
                        case "ISO15765_500KB_11BIT_CAN":
                            ProtocolValue = 02;
                            break;
                        case "ISO15765_500KB_29BIT_CAN":
                            ProtocolValue = 03;
                            break;
                        case "ISO15765_1MB_11BIT_CAN":
                            ProtocolValue = 04;
                            break;
                        case "ISO15765_1MB_29BIT_CAN":
                            ProtocolValue = 05;
                            break;
                        case "I250KB_11BIT_CAN":
                            ProtocolValue = 06;
                            break;
                        case "I250Kb_29BIT_CAN":
                            ProtocolValue = 07;
                            break;
                        case "I500KB_11BIT_CAN":
                            ProtocolValue = 08;
                            break;
                        case "I500KB_29BIT_CAN":
                            ProtocolValue = 09;
                            break;
                        case "I1MB_11BIT_CAN":
                            ProtocolValue = (int)0x0A;
                            break;
                        case "I1MB_29BIT_CAN":
                            ProtocolValue = (int)0x0B;
                            break;
                        case "OE_IVN_250KBPS_11BIT_CAN":
                            ProtocolValue = (int)0x0C;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_250KBPS_29BIT_CAN":
                            ProtocolValue = (int)0x0D;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_500KBPS_11BIT_CAN":
                            ProtocolValue = (int)0x0E;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_500KBPS_29BIT_CAN":
                            ProtocolValue = (int)0x0F;
                            //tx_header_temp = "07E0";
                            //rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_1MBPS_11BIT_CAN":
                            ProtocolValue = (int)0x10;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_1MBPS_29BIT_CAN":
                            ProtocolValue = (int)0x11;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        case "CANOPEN_250KBPS_11BIT_CAN":
                            ProtocolValue = (int)0x12;
                            tx_header_temp = "0626";
                            rx_header_temp = "05A6";
                            break;
                        case "CANOPEN_250KBPS_29BIT_CAN":
                            ProtocolValue = (int)0x13;
                            tx_header_temp = "0626";
                            rx_header_temp = "05A6";
                            break;
                        case "CANOPEN_500KBPS_11BIT_CAN":
                            ProtocolValue = (int)0x14;
                            tx_header_temp = "0626";
                            rx_header_temp = "05A6";
                            break;
                        case "CANOPEN_500KBPS_29BIT_CAN":
                            ProtocolValue = (int)0x15;
                            tx_header_temp = "0626";
                            rx_header_temp = "05A6";
                            break;
                        case "CANOPEN_1MBPS_11BIT_CAN":
                            ProtocolValue = (int)0x16;
                            tx_header_temp = "0626";
                            rx_header_temp = "05A6";
                            break;
                        case "CANOPEN_1MBPS_29BIT_CAN":
                            ProtocolValue = (int)0x17;
                            tx_header_temp = "0626";
                            rx_header_temp = "05A6";
                            break;

                        case "XMODEM_500KBPS_29BIT_CAN":
                            ProtocolValue = (int)0x1B;
                            tx_header_temp = "00000803";
                            rx_header_temp = "00000801";
                            break;
                        default:
                            break;
                    }

                    dongleCommWin = new DongleCommWin(socket, value, Convert.ToUInt32(tx_header_temp, 16), Convert.ToUInt32(rx_header_temp, 16), 0x00, 0x10, 0x10, 0x10);
                    dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.Bluetooth);
                    dSDiagnostic = new UDSDiagnostic(dongleCommWin);

                    protocol = (AutoELM327.Enums.ProtocolEnum)Enum.Parse(typeof(AutoELM327.Enums.ProtocolEnum), value);
                    elmDongleCommWin = new APELMDongleComm(socket, protocol);
                    elmDongleCommWin.InitializePlatform(AutoELM327.Enums.Platform.Android, AutoELM327.Enums.Connectivity.Bluetooth);
                    uDSDiagnosticElm = new UDSDiagnosticElm(elmDongleCommWin);


                    if (dongleCommWin != null)
                    {
                        //var setProtocol = await dongleComm.Dongle_SetProtocol(ProtocolValue);
                        var securityAccess = await dongleCommWin.SecurityAccess();
                        //var GetMacId = await dongleComm.GetWifiMacId();
                        //var GetMacId1 = (byte[])GetMacId;
                        //mac_id = GetMacId1[3].ToString("X2") + ":" +
                        //    GetMacId1[4].ToString("X2") + ":" +
                        //    GetMacId1[5].ToString("X2") + ":" +
                        //    GetMacId1[6].ToString("X2") + ":" +
                        //    GetMacId1[7].ToString("X2") + ":" +
                        //    GetMacId1[8].ToString("X2");
                        //return MacId;
                    }
                }
                return "Demo";

            }
            catch (Exception ex)
            {
                //port.Close();
                //inputOutputManager.Close();
                return "";
            }
        }

        AutoELM327.Enums.ProtocolEnum protocol;
        string tx_header_temp = string.Empty;
        string rx_header_temp = string.Empty;
        int ProtocolValue = 0;

        public async Task<string> GetDongleMacID(bool is_disconnct, string protocol_name, uint protocol_value, string tx_header, string rx_header)
        {
            try
            {
                string MacId = string.Empty;
                //clients.Close();
                if (socket != null)
                {
                    //socket.Close();
                }

                if (!is_disconnct)
                {
                    #region COMMENTED CODE
                    //string ProtocolNameValue = StaticData.ecu_info.FirstOrDefault().protocol.name;//.protocol.name;               
                    //string value = ProtocolNameValue.Replace("-", "_");


                    //tx_header_temp = StaticData.ecu_info.FirstOrDefault().tx_header;
                    //rx_header_temp = StaticData.ecu_info.FirstOrDefault().rx_header;

                    //switch (value)
                    //{
                    //    case "ISO15765_250KB_11BIT_CAN":
                    //        ProtocolValue = 00;

                    //        break;
                    //    case "ISO15765_250Kb_29BIT_CAN":
                    //        ProtocolValue = 01;
                    //        break;
                    //    case "ISO15765_500KB_11BIT_CAN":
                    //        ProtocolValue = 02;
                    //        break;
                    //    case "ISO15765_500KB_29BIT_CAN":
                    //        ProtocolValue = 03;
                    //        break;
                    //    case "ISO15765_1MB_11BIT_CAN":
                    //        ProtocolValue = 04;
                    //        break;
                    //    case "ISO15765_1MB_29BIT_CAN":
                    //        ProtocolValue = 05;
                    //        break;
                    //    case "I250KB_11BIT_CAN":
                    //        ProtocolValue = 06;
                    //        break;
                    //    case "I250Kb_29BIT_CAN":
                    //        ProtocolValue = 07;
                    //        break;
                    //    case "I500KB_11BIT_CAN":
                    //        ProtocolValue = 08;
                    //        break;
                    //    case "I500KB_29BIT_CAN":
                    //        ProtocolValue = 09;
                    //        break;
                    //    case "I1MB_11BIT_CAN":
                    //        ProtocolValue = (int)0x0A;
                    //        break;
                    //    case "I1MB_29BIT_CAN":
                    //        ProtocolValue = (int)0x0B;
                    //        break;
                    //    case "OE_IVN_250KBPS_11BIT_CAN":
                    //        ProtocolValue = (int)0x0C;
                    //        tx_header_temp = "07E0";
                    //        rx_header_temp = "07E8";
                    //        break;
                    //    case "OE_IVN_250KBPS_29BIT_CAN":
                    //        ProtocolValue = (int)0x0D;
                    //        tx_header_temp = "07E0";
                    //        rx_header_temp = "07E8";
                    //        break;
                    //    case "OE_IVN_500KBPS_11BIT_CAN":
                    //        ProtocolValue = (int)0x0E;
                    //        tx_header_temp = "07E0";
                    //        rx_header_temp = "07E8";
                    //        break;
                    //    case "OE_IVN_500KBPS_29BIT_CAN":
                    //        ProtocolValue = (int)0x0F;
                    //        tx_header_temp = "07E0";
                    //        rx_header_temp = "07E8";
                    //        break;
                    //    case "OE_IVN_1MBPS_11BIT_CAN":
                    //        ProtocolValue = (int)0x10;
                    //        tx_header_temp = "07E0";
                    //        rx_header_temp = "07E8";
                    //        break;
                    //    case "OE_IVN_1MBPS_29BIT_CAN":
                    //        ProtocolValue = (int)0x11;
                    //        tx_header_temp = "07E0";
                    //        rx_header_temp = "07E8";
                    //        break;
                    //    default:
                    //        break;
                    //}
                    #endregion

                    tx_header_temp = tx_header;
                    rx_header_temp = rx_header;
                    protocol = (AutoELM327.Enums.ProtocolEnum)Enum.Parse(typeof(AutoELM327.Enums.ProtocolEnum), protocol_name);
                    //socket.InputStream.IsDataAvailable();
                    elmDongleCommWin = new APELMDongleComm(socket, protocol);
                    elmDongleCommWin.InitializePlatform(AutoELM327.Enums.Platform.Android, AutoELM327.Enums.Connectivity.Bluetooth);
                    uDSDiagnosticElm = new UDSDiagnosticElm(elmDongleCommWin);
                    //var securityAccess = await elmDongleCommWin.SecurityAccess();

                    #region COMMENTED CODE
                    ////dongleCommWin = new DongleCommWin(socket, ProtocolValue, Convert.ToUInt32(tx_header_temp, 16), Convert.ToUInt32(rx_header_temp, 16), 0x00, 0x10, 0x10, 0x10);

                    //////dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x07E0, 0x07E8, 0x00, 0x10, 0x10, 0x10);
                    ////dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.Bluetooth);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
                    ////dSDiagnostic = new UDSDiagnostic(dongleCommWin);
                    ////var securityAccess = await dongleCommWin.SecurityAccess();
                    ////var GetMacId = await dongleCommWin.GetWifiMacId();
                    ////var GetMacId1 = (byte[])GetMacId;
                    ////MacId = GetMacId1[3].ToString("X2") + ":" +
                    ////    GetMacId1[4].ToString("X2") + ":" +
                    ////    GetMacId1[5].ToString("X2") + ":" +
                    ////    GetMacId1[6].ToString("X2") + ":" +
                    ////    GetMacId1[7].ToString("X2") + ":" +
                    ////    GetMacId1[8].ToString("X2");
                    ///
                    #endregion
                }
                return MacId;
            }
            catch (Exception ex)
            {
                socket.Close();
                return "";
            }
        }

        public async Task<string> SetDongleProperties()
        {
            try
            {
                string firmware_version = string.Empty;

                //ProtocolValue = Convert.ToInt32(StaticData.ecu_info.FirstOrDefault().protocol.autopeepal, 16);

                /////////////////////////////////////////////////////////////////////
                ProtocolValue = 02;
                tx_header_temp = "07e0";
                rx_header_temp = "07e8";
                /////////////////////////////////////////////////////////////////////
                

                dongleCommWin = new DongleCommWin(socket, ProtocolValue, Convert.ToUInt32(tx_header_temp, 16), Convert.ToUInt32(rx_header_temp, 16), 0x00, 0x10, 0x10, 0x10);
                dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.Bluetooth);
                dSDiagnostic = new UDSDiagnostic(dongleCommWin);

                var securityAccess = await dongleCommWin.SecurityAccess();
                var setProtocol = await dongleCommWin.Dongle_SetProtocol(ProtocolValue);
                var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header_temp);
                var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(rx_header_temp);
                var setPadding = await dongleCommWin.CAN_StartPadding("00");
                //var setp2max = await dongleComm.CAN_SetP2Max("2710");

                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                var firmwareResult = (byte[])firmwareVersion;
                firmware_version = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");

                return firmware_version;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async Task SetDongleProperties(string ProtocolName, string tx_header_temp, string rx_header_temp)
        {

            try
            {
                var protocol_Name = Convert.ToInt32(ProtocolName, 16);
                var setProtocol = await dongleCommWin.Dongle_SetProtocol(protocol_Name);
                var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header_temp);
                var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(rx_header_temp);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<string> SetDongleProperties(string protocol_name)
        {
            try
            {
                var setProtocol = await elmDongleCommWin.Dongle_SetProtocol(protocol_name);


                if (protocol_name.Contains("CAN"))
                {

                    var setHeader = await elmDongleCommWin.CAN_SetTxHeader(tx_header_temp);

                    var setHeaderMask = await elmDongleCommWin.CAN_SetRxHeaderMask(rx_header_temp);
                }
                else
                {
                    var setHeader = await elmDongleCommWin.ISOK_SetHeader(tx_header_temp, rx_header_temp);
                }
                //if (is_padding)
                //{
                //    var setpadding = await dongleCommWin.CAN_StartPadding();
                //}
                //else
                //{
                //    var setpadding = await dongleCommWin.CAN_StopPadding();
                //}

                var ver = await elmDongleCommWin.Dongle_GetFimrwareVersion();
                return (string)ver;
                //ver = firmwareVersion.ToString();
                #region COMMENTED CODE

                //var setProtocol = await dongleCommWin.Dongle_SetProtocol(ProtocolValue);

                //var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header_temp);

                //var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(rx_header_temp);

                //var setPadding = await dongleCommWin.CAN_StartPadding("00");

                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                //var firmwareResult = (byte[])firmwareVersion;
                //var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                #endregion
                //return (string)ver;
            }
            catch (Exception ex)
            {
                return "";
            }

        }
        public async Task<string> Connect()
        {
            //byte[] ba = Encoding.Default.GetBytes("500C47568AFE56214E238000FFC3");
            //byte[] ba = Encoding.Default.GetBytes("500C65EFA86574FE9A890018CE16");
            //SendCommand("500C47568AFE56214E238000FFC3");

            var firmware_version = await SendMessage("500C47568AFE56214E238000FFC3");
            return firmware_version;
        }

        private async Task<string> SendMessage(string message)
        {
            try
            {
                var setProtocol = await dongleCommWin.Dongle_SetProtocol(ProtocolValue);

                var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header_temp);

                var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(rx_header_temp);

                var setPadding = await dongleCommWin.CAN_StartPadding("00");

                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                var firmwareResult = (byte[])firmwareVersion;
                var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");

                return ver;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async Task<Model.ReadDtcResponseModel> ReadDtc(string dtc_index)
        {
            try
            {

                if (dtc_index == "UDS-2BYTE-DTC")
                {
                    dtc_index = "UDS_2BYTE12_DTC";
                }

                Model.ReadDtcResponseModel readDtcResponseModel = new Model.ReadDtcResponseModel();
                ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
                await Task.Run(async () =>
                {
                    //ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
                    readDTCResponse = await dSDiagnostic.ReadDTC(index);
                    //readDTCResponse = await dSDiagnostic.ReadDTC(index);
                });

                readDtcResponseModel.dtcs = readDTCResponse.dtcs;
                readDtcResponseModel.status = readDTCResponse.status;
                readDtcResponseModel.noofdtc = readDTCResponse.noofdtc;
                return readDtcResponseModel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> ClearDtc(string dtc_index)
        {
            try
            {
                if (dtc_index == "UDS-4BYTES")
                {
                    dtc_index = "UDS_4BYTES";
                }

                string status = string.Empty;
                object result = new object();
                ClearDTCIndex index = (ClearDTCIndex)Enum.Parse(typeof(ClearDTCIndex), dtc_index);
                await Task.Run(async () =>
                {

                    result = await dSDiagnostic.ClearDTC(index);
                    var res = JsonConvert.SerializeObject(result);
                    var Response = JsonConvert.DeserializeObject<ClearDtcResponseModel>(res);
                    status = Response.ECUResponseStatus;
                });

                return status;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #region IVN DTC
        public async Task<ObservableCollection<IvnReadDtcResponseModel>> IVN_ReadDtc(List<string> FrameIDC)
        {
            try
            {
                ObservableCollection<IvnResponseArrayStatus> IvnReadDTCResponse = new ObservableCollection<IvnResponseArrayStatus>();

                ObservableCollection<IvnReadDtcResponseModel> FrameResponseList = new ObservableCollection<IvnReadDtcResponseModel>();

                await Task.Run(async () =>
                {
                    IvnReadDTCResponse = await dongleCommWin.SetIvnFrame(FrameIDC);
                });
                IvnReadDtcResponseModel ivnReadDtcResponseModel;
                if (IvnReadDTCResponse != null || IvnReadDTCResponse.Count > 0)
                {
                    foreach (var item in IvnReadDTCResponse.ToList())
                    {
                        ivnReadDtcResponseModel = new IvnReadDtcResponseModel();
                        if (item.ActualDataBytes == null)
                        {
                            ivnReadDtcResponseModel.ActualDataBytes = null;
                        }
                        else
                        {
                            ivnReadDtcResponseModel.ActualDataBytes = item.ActualDataBytes;
                        }

                        ivnReadDtcResponseModel.ECUResponse = ConvertedByteToString(item.ECUResponse);
                        ivnReadDtcResponseModel.ECUResponseStatus = item.ECUResponseStatus;
                        ivnReadDtcResponseModel.Frame = item.Frame;
                        FrameResponseList.Add(ivnReadDtcResponseModel);
                    }
                }
                return FrameResponseList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ObservableCollection<ReadPidPresponseModel>> IVN_ReadPid(ObservableCollection<Model.IVN_SelectedPID> IVNpidList)
        {
            try
            {
                object result = new object();
                ObservableCollection<APDiagnosticAndroid.Models.IVN_SelectedPID> list = new ObservableCollection<APDiagnosticAndroid.Models.IVN_SelectedPID>();

                var PIDID = new List<APDiagnosticAndroid.Models.PIDFrameId>();
                foreach (var item in IVNpidList)
                {
                    foreach (var item1 in item.frame_ids)
                    {
                        
                        var newFrame = new APDiagnosticAndroid.Models.PIDFrameId
                        {
                            FramID = item1.FramID,
                            pid_description = item1.pid_description,
                            start_byte = item1.start_byte,
                            @byte = item1.@byte,
                            bit_coded = item1.bit_coded,
                            start_bit = item1.start_bit,
                            no_of_bits = item1.no_of_bits,
                            resolution = item1.resolution,
                            offset = item1.offset,
                            unit = item1.unit,
                            message_type = item1.message_type,
                            frame_of_pid_message = new List<APDiagnosticAndroid.Models.FrameOfPidMessage>(),
                            endian = item1.endian,
                            num_type = item1.num_type,
                        };
                        foreach(var item2 in item1.frame_of_pid_message)
                        {
                            newFrame.frame_of_pid_message.Add( new APDiagnosticAndroid.Models.FrameOfPidMessage
                            {
                                code = item2.code,
                                message = item2.message,
                            });
                        }
                        PIDID.Add(newFrame);
                    }

                    list.Add(new APDiagnosticAndroid.Models.IVN_SelectedPID { frame_id = item.frame_id, frame_ids = PIDID });
                }

                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.IVN_ReadParameters(IVNpidList.Count, list);
                    var res = JsonConvert.SerializeObject(result);
                    var res_list = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(res);
                    return res_list;
                });
                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        //static string ConvertedByteToString(byte[] bytes)
        //{
        //    using (var stream = new MemoryStream(bytes))
        //    {
        //        using (var stremReader = new StreamReader(stream))
        //        {
        //            return stremReader.ReadToEnd();
        //        }
        //    }
        //}
        #endregion

        public async Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<Model.ReadParameterPID> pidList)
        {
            try
            {
                object result = new object();
                ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID>();
                foreach (var item in pidList)
                {
                    var MessageValueList = new List<APDiagnosticAndroid.Models.SelectedParameterMessage>();
                    if (item.messages != null)
                    {
                        foreach (var MessageItem in item.messages)
                        {
                            MessageValueList.Add(new APDiagnosticAndroid.Models.SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
                        }
                    }

                    list.Add(
                        new APDiagnosticAndroid.Models.ReadParameterPID
                        {
                            datatype = item.datatype,
                            IsBitcoded = item.IsBitcoded,
                            noofBits = item.noofBits,
                            noOfBytes = item.noOfBytes,
                            offset = item.offset,
                            pid = item.pid,
                            resolution = item.resolution,
                            startBit = item.startBit,
                            startByte = item.startByte,
                            totalBytes = item.totalBytes,
                            totalLen = item.totalLen,
                            pidNumber = item.pidNumber,
                            pidName = item.pidName,
                            messages = MessageValueList
                        });
                }

                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.ReadParameters(pidList.Count, list);
                    var res = JsonConvert.SerializeObject(result);
                    var res_list = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(res);
                    return res_list;
                    //status = Response.ECUResponseStatus;
                });

                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<PidCode> pidList)
        {
            try
            {
                object result = new object();
                ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID>();
                foreach (var item in pidList)
                {
                    var MessageValueList = new List<APDiagnosticAndroid.Models.SelectedParameterMessage>();
                    if (item.messages != null)
                    {
                        foreach (var MessageItem in item.messages)
                        {
                            MessageValueList.Add(new APDiagnosticAndroid.Models.SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
                        }
                    }

                    list.Add(
                        new APDiagnosticAndroid.Models.ReadParameterPID
                        {
                            datatype = item.message_type,
                            IsBitcoded = item.bitcoded,
                            noofBits = item.end_bit_position.GetValueOrDefault() - item.start_bit_position.GetValueOrDefault() + 1,
                            noOfBytes = item.length,
                            offset = item.offset,
                            pid = item.code,
                            resolution = item.resolution,
                            startBit = Convert.ToInt32(item.start_bit_position),
                            startByte = item.byte_position,
                            //totalBytes= item.totalBytes,
                            totalLen = item.code.Length / 2,
                            pidNumber = item.id,
                            pidName = item.short_name,
                            messages = MessageValueList

                        });
                }

                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.ReadParameters(pidList.Count, list);
                    var res = JsonConvert.SerializeObject(result);
                    var res_list = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(res);
                    return res_list;
                    //status = Response.ECUResponseStatus;
                });

                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ObservableCollection<WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList)
        {
            try
            {
                WriteParameterIndex index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), write_pid_intdex);
                ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID>();
                foreach (var item in pidList)
                {
                    SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), item.seedkeyindex);
                    WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), item.writepamindex);
                    list.Add(
                        new APDiagnosticAndroid.Models.WriteParameterPID
                        {
                            seedkeyindex = seed_index,//item.seedkeyindex,
                            writepamindex = write_index, //item.writepamindex,
                            writeparadata = item.writeparadata,
                            writeparadatasize = item.writeparadatasize,
                            writeparapid = item.writeparapid,
                            ReadParameterPID_DataType = item.ReadParameterPID_DataType,
                            pid = item.pid,
                            startByte = item.startByte,
                            totalBytes = item.totalBytes
                            //writeparaName = item.
                        });
                }
                var respo = await Task.Run(async () =>
                {
                    var result = await dSDiagnostic.WriteParameters(pidList.Count, index, list);
                    var res = JsonConvert.SerializeObject(result);
                    var res_list = JsonConvert.DeserializeObject<ObservableCollection<WriteParameter_Status>>(res);
                    return res_list;
                });
                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ObservableCollection<WriteParameter_Status>> WriteAtuatorTest(string ior_test_fn_index, ObservableCollection<Model.WriteParameterPID> pidList, bool IsPlay)
        {
            try
            {
                WriteParameterIndex index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), ior_test_fn_index);
                ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID>();
                foreach (var item in pidList)
                {
                    SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), item.seedkeyindex);
                    WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), item.writepamindex);
                    list.Add(
                        new APDiagnosticAndroid.Models.WriteParameterPID
                        {
                            seedkeyindex = seed_index,//item.seedkeyindex,
                            writepamindex = write_index, //item.writepamindex,
                            writeparadata = item.writeparadata,
                            writeparadatasize = item.writeparadatasize,
                            writeparapid = item.writeparapid,
                            ReadParameterPID_DataType = item.ReadParameterPID_DataType,
                            pid = item.pid,
                            startByte = item.startByte,
                            totalBytes = item.totalBytes
                            //writeparaName = item.
                        });
                }
                var respo = await Task.Run(async () =>
                {
                    var result = await dSDiagnostic.AtuatorTestWriteParameters(pidList.Count, index, list, IsPlay);
                    var res = JsonConvert.SerializeObject(result);
                    var res_list = JsonConvert.DeserializeObject<ObservableCollection<WriteParameter_Status>>(res);
                    return res_list;
                });
                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        static string ConvertedByteToString(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var stremReader = new StreamReader(stream))
                {
                    return stremReader.ReadToEnd();
                }
            }
        }

        //private async Task ConnectionStatus()
        //{
        //    if (socket.IsConnected)
        //    {
        //        socket.Dispose();
        //    }
        //}

        public string ConnectedORNot()
        {
            if (socket.IsConnected)
            {
                socket.Dispose();
                return "SokectConnected";
            }
            else
            {
                return "SokectConnected";
            }
        }
        public string MessageToSend = String.Empty;

        //private async Task loop1(string name, int sleepTime, bool readAsCharArray)
        //{
        //    BluetoothDevice device = null;
        //    BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        //    BluetoothSocket bthSocket = null;
        //    BluetoothServerSocket bthServerSocket = null;

        //    UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
        //    bthServerSocket = adapter.ListenUsingRfcommWithServiceRecord("TLCI Barcode Scanner", uuid);

        //    _ct = new CancellationTokenSource();

        //    while (_ct.IsCancellationRequested == false)
        //    {
        //        try
        //        {
        //            Thread.Sleep(50);

        //            adapter = BluetoothAdapter.DefaultAdapter;

        //            if (adapter == null)
        //                Debug.Write("No bluetooth adapter found!");
        //            else
        //                Debug.Write("Adapter found!");

        //            if (!adapter.IsEnabled)
        //                Debug.Write("Bluetooth adapter is not enabled.");
        //            else
        //                Debug.Write("Adapter found!");

        //            Debug.Write("Try to connect to " + name);

        //            var aa = adapter.BondedDevices.Count;
        //            foreach (var bondedDevice in adapter.BondedDevices)
        //            {
        //                Debug.Write("Paired devices found: " + bondedDevice.Address.ToUpper());

        //                if (bondedDevice.Address == name)
        //                {
        //                    Debug.Write("Found " + bondedDevice.Address + ". Try to connect with it!");
        //                    device = bondedDevice;
        //                    Debug.Write(bondedDevice.Type.ToString());
        //                    break;
        //                }
        //            }

        //            if (device == null)
        //                Debug.Write("Named device not found.");
        //            else
        //            {
        //                bthSocket = bthServerSocket.Accept();

        //                adapter.CancelDiscovery();

        //                if (bthSocket != null)
        //                {
        //                    Debug.Write("Connected");

        //                    if (bthSocket.IsConnected)
        //                    {
        //                        var mReader = new InputStreamReader(bthSocket.InputStream);
        //                        var buffer = new BufferedReader(mReader);

        //                        while (_ct.IsCancellationRequested == false)
        //                        {
        //                            if (MessageToSend != null)
        //                            {
        //                                var chars = MessageToSend.ToCharArray();
        //                                var bytes = new List<byte>();

        //                                foreach (var character in chars)
        //                                {
        //                                    bytes.Add((byte)character);
        //                                }

        //                                await bthSocket.OutputStream.WriteAsync(bytes.ToArray(), 0, bytes.Count);

        //                                MessageToSend = null;
        //                            }
        //                        }

        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.Write(ex);
        //            Debug.Write(ex.Message);
        //        }
        //        finally
        //        {
        //            if (bthSocket != null)
        //                bthSocket.Close();

        //            device = null;
        //            adapter = null;
        //        }
        //    }
        //}

        //private async Task loop(string address, int sleepTime, bool readAsCharArray)
        //{
        //    BluetoothDevice device = null;
        //    BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        //    BluetoothServerSocket bthServerSocket = null;

        //    _ct = new CancellationTokenSource();
        //    while (_ct.IsCancellationRequested == false)
        //    {
        //        try
        //        {
        //            Thread.Sleep(sleepTime);

        //            adapter = BluetoothAdapter.DefaultAdapter;
        //            device = BluetoothStatic.bluetoothDevices.FirstOrDefault(x => x.Address == address);//bluetoothDevice;
        //            BluetoothDevice paired_device = BluetoothAdapter.DefaultAdapter.BondedDevices.
        //                FirstOrDefault(x => x.Address == address);

        //            if (paired_device == null)
        //            {

        //                var returnValue = device.CreateBond();
        //                var state = device.BondState;
        //            }
        //            if (device == null)
        //            {
        //                //System.Diagnostics.Debug.WriteLine("Named device not found.");
        //                //Xamarin.Forms.MessagingCenter.Send<App, string>((App)Xamarin.Forms.Application.Current, "TerminalLog", "Named device not found.");
        //            }
        //            else
        //            {
        //                UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
        //                //bthServerSocket = adapter.ListenUsingRfcommWithServiceRecord("TLCI Barcode Scanner", uuid);

        //                UUID uuids = device.GetUuids()[0].Uuid;

        //                if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
        //                    socket = device.CreateInsecureRfcommSocketToServiceRecord(uuid);
        //                else
        //                    socket = device.CreateRfcommSocketToServiceRecord(uuid);
        //                if (socket != null)
        //                {
        //                    //await Task.Delay(2000);
        //                    socket.Connect();
        //                    if (socket.IsConnected)
        //                    {
        //                        outStream = (OutputStreamInvoker)socket.OutputStream;
        //                        inputStreamReader = new InputStreamReader(socket.InputStream);
        //                        bufferedReader = new BufferedReader(inputStreamReader);
        //                        break;
        //                    }
        //                }
        //                else
        //                    System.Diagnostics.Debug.WriteLine("BthSocket = null");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Diagnostics.Debug.WriteLine("EXCEPTION: " + ex.Message);
        //        }
        //        finally
        //        {
        //            //if (socket != null)
        //            //    socket.Close();
        //            //device = null;
        //            //adapter = null;
        //        }
        //    }
        //    System.Diagnostics.Debug.WriteLine("Exit the external loop");
        //}




        //public void connectionMethod(BluetoothDevice device)
        //{
        //    try
        //    {
        //        UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
        //        if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
        //            socket = device.CreateInsecureRfcommSocketToServiceRecord(uuid);
        //        else
        //            socket = device.CreateRfcommSocketToServiceRecord(uuid);

        //        if (socket != null)
        //        {
        //            socket.Connect();
        //            if (socket.IsConnected)
        //            {
        //                outStream = (OutputStreamInvoker)socket.OutputStream;
        //                inputStreamReader = new InputStreamReader(socket.InputStream);
        //                bufferedReader = new BufferedReader(inputStreamReader);
        //                return;
        //            }
        //        }
        //        else
        //            System.Diagnostics.Debug.WriteLine("BthSocket = null");
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        #region Terminal
        public async Task<string> GetDongleMacIDForTerminal(bool is_disconnct, string protocol)
        {
            string mac_id = string.Empty;
            try
            {
                if (!is_disconnct)
                {

                    string ProtocolNameValue = StaticData.ecu_info.FirstOrDefault().protocol.name;//.protocol.name;
                    string value = ProtocolNameValue.Replace("-", "_");

                    tx_header_temp = StaticData.ecu_info.FirstOrDefault().tx_header;
                    rx_header_temp = StaticData.ecu_info.FirstOrDefault().rx_header;

                    switch (value)
                    {
                        case "ISO15765_250KB_11BIT_CAN":
                            ProtocolValue = 00;
                            break;
                        case "ISO15765_250Kb_29BIT_CAN":
                            ProtocolValue = 01;
                            break;
                        case "ISO15765_500KB_11BIT_CAN":
                            ProtocolValue = 02;
                            break;
                        case "ISO15765_500KB_29BIT_CAN":
                            ProtocolValue = 03;
                            break;
                        case "ISO15765_1MB_11BIT_CAN":
                            ProtocolValue = 04;
                            break;
                        case "ISO15765_1MB_29BIT_CAN":
                            ProtocolValue = 05;
                            break;
                        case "I250KB_11BIT_CAN":
                            ProtocolValue = 06;
                            break;
                        case "I250Kb_29BIT_CAN":
                            ProtocolValue = 07;
                            break;
                        case "I500KB_11BIT_CAN":
                            ProtocolValue = 08;
                            break;
                        case "I500KB_29BIT_CAN":
                            ProtocolValue = 09;
                            break;
                        case "I1MB_11BIT_CAN":
                            ProtocolValue = (int)0x0A;
                            break;
                        case "I1MB_29BIT_CAN":
                            ProtocolValue = (int)0x0B;
                            break;
                        case "OE_IVN_250KBPS_11BIT_CAN":
                            ProtocolValue = (int)0x0C;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_250KBPS_29BIT_CAN":
                            ProtocolValue = (int)0x0D;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_500KBPS_11BIT_CAN":
                            ProtocolValue = (int)0x0E;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_500KBPS_29BIT_CAN":
                            ProtocolValue = (int)0x0F;
                            //tx_header_temp = "07E0";
                            //rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_1MBPS_11BIT_CAN":
                            ProtocolValue = (int)0x10;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        case "OE_IVN_1MBPS_29BIT_CAN":
                            ProtocolValue = (int)0x11;
                            tx_header_temp = "07E0";
                            rx_header_temp = "07E8";
                            break;
                        default:
                            break;
                    }

                    dongleCommWin = new DongleCommWin(socket, value, 0x00, 0x00, 0x00, 0x10, 0x10, 0x10);
                    dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.Bluetooth);
                    dSDiagnostic = new UDSDiagnostic(dongleCommWin);
                    if (dongleCommWin != null)
                    {
                        //var setProtocol = await dongleComm.Dongle_SetProtocol(ProtocolValue);
                        var securityAccess = await dongleCommWin.SecurityAccess();
                        var GetMacId = await dongleCommWin.GetWifiMacId();
                        var GetMacId1 = (byte[])GetMacId;
                        mac_id = GetMacId1[3].ToString("X2") + ":" +
                            GetMacId1[4].ToString("X2") + ":" +
                            GetMacId1[5].ToString("X2") + ":" +
                            GetMacId1[6].ToString("X2") + ":" +
                            GetMacId1[7].ToString("X2") + ":" +
                            GetMacId1[8].ToString("X2");
                        //return MacId;
                    }

                }
                return mac_id;

            }
            catch (Exception ex)
            {
                return "";
            }
        }
        #endregion

        public async Task<string> unlockEcu(ResultUnlock unlockData)
        {
            string tx_id = unlockData.tx_id;
            string tx_frame = unlockData.tx_frame;
            string tx_frequency = unlockData.tx_frequency;
            string tx_totalTime = unlockData.tx_total_time;
            string rx_id = unlockData.rx_id;
            string protocolValue = unlockData.protocol.autopeepal;

            SetDongleProperties(protocolValue, tx_id, rx_id);

            string response = string.Empty;

            response = await dSDiagnostic.StartEcuUnlocking(tx_frame, tx_frequency, tx_totalTime);
            return response;
        }

        public async Task<string> StartECUFlashing(string flashJson, string interpreter, Ecu2 ecu2, SeedkeyalgoFnIndex sklFN, List<EcuMapFile> ecu_map_file)
        {
            try
            {
                var jsonData = JsonConvert.DeserializeObject<APDiagnosticAndroid.Models.FlashingMatrixData>(flashJson);

                //EraseSectorEnum erase_type = (EraseSectorEnum)Enum.Parse(typeof(EraseSectorEnum), ecu2.flash_erase_type);
                //ChecksumSectorEnum check_sum_type = (ChecksumSectorEnum)Enum.Parse(typeof(ChecksumSectorEnum), ecu2.flash_check_sum_type);
                SEEDKEYINDEXTYPE seedkeyindx = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), sklFN.value);

                APDiagnosticAndroid.Models.FlashingMatrixData flashingMatrixData = new APDiagnosticAndroid.Models.FlashingMatrixData();
                flashingMatrixData = JsonConvert.DeserializeObject<APDiagnosticAndroid.Models.FlashingMatrixData>(flashJson);
                //byte asdb = Convert.ToByte(ecu2.flash_address_data_format);
                //UInt16 acc = Convert.ToUInt16(ecu2.sectorframetransferlen,16);

                var flashConfig = new flashconfig
                {
                    //addrdataformat = ecu2.flash_address_data_format,
                    //checksumsector = check_sum_type,
                    //diag_mode = Convert.ToByte(ecu2.flash_diagnostic_mode, 16),
                    //erasesector = erase_type,
                    //flash_index = FLASHINDEXTYPE.GREAVES_BOSCH_BS6,
                    sectorframetransferlen = Convert.ToUInt16(ecu2.sectorframetransferlen, 16),
                    seedkeyindex = seedkeyindx,
                    seedkeynumbytes = Convert.ToByte(ecu2.flash_seed_key_length, 16),
                    //sendseedbyte = Convert.ToByte(ecu2.sendseedbyte, 16),
                    //septime = Convert.ToByte(ecu2.flashsep_time, 16),
                    FlashStatus = ecu2.flash_status,
                };


                string response = string.Empty;

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                if (flashConfig.FlashStatus == "FLASH_SML_BOSCH_BS6")
                {
                    await Task.Run(async () =>
                    {
                        response = await dSDiagnostic.StartFlashBoschBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                    });
                }
                else if (flashConfig.FlashStatus == "FLASH_SML_ADVANTEK_BS6")
                {
                    await Task.Run(async () =>
                    {
                        //response = await dSDiagnostic.StartFlashAdvantekBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());

                    });
                }
                else if (flashConfig.FlashStatus == "FLASH_SML_BOSCH_BS4")
                {
                    await Task.Run(async () =>
                    {
                        response = await dSDiagnostic.StartFlashBoschBS4(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                    });
                }
                else if (flashJson.Contains(".bin"))
                {
                    response = await dSDiagnostic.StartFlashBINfile(flashConfig, flashingMatrixData.NoOfSectors, flashJson, (int)0x1A);

                }
                else if (flashConfig.FlashStatus == "FLASH_KOEL_BOSCH_BS6")
                {
                    response = await dSDiagnostic.StartFlashBoschBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                }
                else if (flashJson.Contains("\"bsl\"") || flashJson.Contains(".bin"))
                {
                    if (ecu2.flash_status == "PIAGGIO_EV_MCU_CANOPEN_125KBPS")
                    {
                        //dSDiagnostic.Test();
                        response = await dSDiagnostic.StartFlashCurtis125KBPS(flashJson, tx_header_temp, rx_header_temp, ProtocolValue);
                    }
                    else if (flashJson.Contains(".bin"))
                    {
                        response = await dSDiagnostic.StartFlashBINfile(flashConfig, flashingMatrixData.NoOfSectors, flashJson, (int)0x1A);

                    }
                    else
                    {
                        //dSDiagnostic.Test();
                        response = await dSDiagnostic.StartFlashCurtis500KBPS(flashJson, tx_header_temp, rx_header_temp, ProtocolValue);
                    }
                }
                else if (flashConfig.FlashStatus == "FLASH_GREAVES_ADVANTEK_BS6" ||
                         flashConfig.FlashStatus == "GREAVES_FLASH_ADVANTEKBS6")
                {
                    await Task.Run(async () =>
                    {
                        //response = await dSDiagnostic.StartFlashBosch(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                    });
                    
                }
                else if (flashConfig.FlashStatus == "FLASH_BAJAJ_KTM_BS6")
                {
                    response = await dSDiagnostic.FlashInterpreter(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray(), interpreter);
                }
                
                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
                Debug.WriteLine("------RunTime------" + elapsedTime, "");

                return response;
            }
            catch (Exception ex)
            {
                return $"ERROR : {ex.Message}";
            }
        }

        public async Task TXheader(string tx_header)
        {
            var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header);
            var HeaderResult = (byte[])setHeader;
            var HeaderResponse = ByteArrayToString(HeaderResult);
            Debug.WriteLine("------DTC TX Header Set------" + tx_header + " ", "\n--Header Response--" + HeaderResponse);
        }
        public async Task RXheader(string rx_header)
        {
            var setHeader = await dongleCommWin.CAN_SetRxHeaderMask(rx_header);
            var HeaderResult = (byte[])setHeader;
            var HeaderResponse = ByteArrayToString(HeaderResult);
            Debug.WriteLine("------DTC RX Header Set------" + rx_header + " ", "RX Header Response--" + HeaderResponse);
        }

        public async Task<string[]> SendTerminalCommands(string[] commands)
        {
            string[] vs_res = new string[7];
            try
            {
                dongleCommWin = new DongleCommWin(socket, Convert.ToInt32(commands[0]), Convert.ToUInt32(commands[1], 16), Convert.ToUInt32(commands[2], 16), 0x00, 0x10, 0x10, 0x10);
                dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.Bluetooth);
                dSDiagnostic = new UDSDiagnostic(dongleCommWin);

                var securityAccess = await dongleCommWin.SecurityAccess();


                var setProtocol = await dongleCommWin.Dongle_SetProtocol(Convert.ToInt32(commands[0]));
                vs_res[0] = commands[0];
                vs_res[5] = commands[5];
                var ProtocolResult = (byte[])setProtocol;

                var setHeader = await dongleCommWin.CAN_SetTxHeader(commands[1]);
                vs_res[1] = commands[1];
                //var HeaderResult = (byte[])setHeader;
                //var HeaderResponse = ByteArrayToString(HeaderResult);
                var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(commands[2]);
                vs_res[2] = commands[2];
                //var HeaderMarkResult = (byte[])setHeaderMask;
                //var HeaderMarkResponse = ByteArrayToString(HeaderMarkResult);
                if (!string.IsNullOrEmpty(commands[3]))
                {
                    var setpadding = await dongleCommWin.CAN_StartPadding("00");
                    vs_res[3] = "Enabled";
                }
                else
                {
                    vs_res[3] = "Disabled";
                }

                if (!string.IsNullOrEmpty(commands[4]))
                {
                    var setpadding = await dongleCommWin.CAN_StartPadding("00");
                    vs_res[4] = "Enabled";
                }
                else
                {
                    vs_res[4] = "Disabled";
                }

                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                var firmwareResult = (byte[])firmwareVersion;
                var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                vs_res[6] = ver;
                return vs_res;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> SetData(string commands)
        {
            string response_command = string.Empty;
            var setHeader = await dSDiagnostic.SetDataData(commands);

            if (setHeader.ECUResponseStatus.Contains("NOERROR"))
            {
                response_command = ByteArrayToString(setHeader.ActualDataBytes);
            }
            else
            {
                response_command = setHeader.ECUResponseStatus;
            }

            return response_command;
        }

        public async Task<string> GetFirmware1()
        {
            try
            {
                //var securityAccess = await dongleCommWin.SecurityAccess();

                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                var firmwareResult = (byte[])firmwareVersion;
                var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                return ver;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> GetFirmware()
        {
            try
            {
                dongleCommWin = new DongleCommWin(socket, 0x00, 0x00, 0x00, 0x00, 0x10, 0x10, 0x10);
                dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.Bluetooth);
                dSDiagnostic = new UDSDiagnostic(dongleCommWin);

                //var securityAccess = await dongleCommWin.SecurityAccess();

                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                var firmwareResult = (byte[])firmwareVersion;
                var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                return ver;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string[]> GetSsidPassword()
        {
            string[] Response = new string[4];

            var Default_SSID = await dongleCommWin.CAN_Get_Default_SSID();

            var Default_Password = await dongleCommWin.CAN_Get_Default_Password();

            var User_SSID = await dongleCommWin.CAN_Get_User_SSID();

            var User_Password = await dongleCommWin.CAN_Get_User_Password();

            return Response;
        }



        public async Task<string> SendFotaCommand(string command)
        {
            try
            {
                var response = await dongleCommWin.Dongle_SetFota(command);
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        //public async Task<string> GetMacIdCommand(string command)
        //{
        //    try
        //    {
        //        var response = await dongleCommWin.Dongle_SetFota(command);
        //        return "";
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";
        //    }
        //}


        /// <summary>
        /// /
        /// </summary>
        /// <param name="message"></param>
        //private async void SendMessageBT(string message)
        ////{
        //    try
        //    {
        //        //byte[] bytes = Encoding.UTF8.GetBytes(message);
        //        string trimmed = String.Concat(message.Where(c => !Char.IsWhiteSpace(c)));
        //        var bytes = StringToByteArray(trimmed);
        //        await outStream.WriteAsync(bytes, 0, bytes.Length);
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}



        //private async void SendCommand(string message)
        //{
        //    // byte[] bytes = Encoding.ASCII.GetBytes(message);
        //    byte[] bytes = Encoding.UTF8.GetBytes(message);
        //    await outStream.WriteAsync(bytes, 0, bytes.Length);
        //}


        //private void ReadData(out string data)
        //{
        //    byte[] rbuffer = new byte[1024];
        //    byte[] RetArray = new byte[] { };
        //    try
        //    {

        //        // Read data from the device
        //        while (!socket.InputStream.CanRead)
        //        {
        //            //Console.WriteLine("------------------------------------------------");
        //        }
        //        int readByte = socket.InputStream.Read(rbuffer, 0, rbuffer.Length);

        //        RetArray = new byte[readByte];
        //        Array.Copy(rbuffer.ToArray(), 0, RetArray, 0, readByte);
        //        //Console.WriteLine(Array2Text(RetArray, false) + "\n" + "[" + RetArray.Length.ToString() + "]");

        //    }
        //    catch (Java.IO.IOException e)
        //    {

        //    }
        //    data = "";
        //}



        //private byte[] HexStringToByteArray(String hex)
        //{
        //    hex = hex.Replace(" ", "");
        //    int numberChars = hex.Length;
        //    if (numberChars % 2 != 0)
        //    {
        //        hex = "0" + hex;
        //        numberChars++;
        //    }
        //    byte[] bytes = new byte[numberChars / 2];
        //    for (int i = 0; i < numberChars; i += 2)
        //        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        //    return bytes;
        //}


        //private async Task Connect(string deviceName, int sleepTime, bool readAsCharArray)
        //{
        //    try
        //    {
        //        _ct = new CancellationTokenSource();
        //        while (_ct.IsCancellationRequested == false)
        //        {
        //            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        //            if (adapter == null)
        //                System.Diagnostics.Debug.WriteLine("No Bluetooth adapter found.");
        //            else if (!adapter.IsEnabled)
        //                System.Diagnostics.Debug.WriteLine("Bluetooth adapter is not enabled.");

        //            List<BluetoothDevice> L = new List<BluetoothDevice>();
        //            foreach (BluetoothDevice d in adapter.BondedDevices)
        //            {
        //                System.Diagnostics.Debug.WriteLine("D: " + d.Name + " " + d.Address + " " + d.BondState.ToString());
        //                L.Add(d);
        //            }

        //            BluetoothDevice device = null;
        //            device = L.Find(j => j.Address == deviceName);

        //            if (device == null)
        //                System.Diagnostics.Debug.WriteLine("Named device not found.");
        //            else
        //            {
        //                System.Diagnostics.Debug.WriteLine("Device has been found: " + device.Name + " " + device.Address + " " + device.BondState.ToString());
        //            }

        //            socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString(TARGET_UUID));
        //            //socket = device.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString(TARGET_UUID));
        //            await socket.ConnectAsync();
        //            //socket.Connect();

        //            if (socket != null && socket.IsConnected)
        //                System.Diagnostics.Debug.WriteLine("Connection successful!");
        //            else
        //                System.Diagnostics.Debug.WriteLine("Connection failed!");

        //            inStream = (InputStreamInvoker)socket.InputStream;
        //            outStream = (OutputStreamInvoker)socket.OutputStream;

        //            if (socket != null && socket.IsConnected)
        //            {
        //                Task t = new Task(() => Listen(inStream));
        //                t.Start();
        //            }
        //            else throw new Exception("Socket not existing or not connected.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
        //public static byte[] StringToByteArray(string hex)
        //{
        //    return Enumerable.Range(0, hex.Length).
        //           Where(x => 0 == x % 2).
        //           Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
        //           ToArray();
        //}

        //public string ASCIITOHex(string ascii)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    byte[] inputBytes = Encoding.UTF8.GetBytes(ascii);
        //    foreach (byte b in inputBytes)
        //    {
        //        sb.Append(string.Format("{0:x2}", b));
        //    }
        //    return sb.ToString();
        //}

        //private async void Listen(Stream inStream)
        //{
        //    //bool Listening = true;
        //    System.Diagnostics.Debug.WriteLine("Listening has been started.");
        //    byte[] uintBuffer = new byte[sizeof(uint)]; // This reads the first 4 bytes which form an uint that indicates the length of the string message.
        //    byte[] textBuffer; // This will contain the string message.

        //    // Keep listening to the InputStream while connected.
        //    while (_ct.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            // This one blocks until it gets 4 bytes.
        //            await inStream.ReadAsync(uintBuffer, 0, uintBuffer.Length);
        //            uint readLength = BitConverter.ToUInt32(uintBuffer, 0);

        //            textBuffer = new byte[readLength];
        //            // Here we know for how many bytes we are looking for.
        //            await inStream.ReadAsync(textBuffer, 0, (int)readLength);

        //            string s = Encoding.UTF8.GetString(textBuffer);
        //            System.Diagnostics.Debug.WriteLine("Received: " + s);
        //        }
        //        catch (Java.IO.IOException e)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Error: " + e.Message);
        //            ///Listening = false;
        //            break;
        //        }
        //    }
        //    System.Diagnostics.Debug.WriteLine("Listening has ended.");
        //}


        public async Task<ReadPidPresponseModel> SetTestRoutineCommand(string command)
        {
            ReadPidPresponseModel response = new ReadPidPresponseModel();
            try
            {
                object result = new object();

                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.TestRoutine(command);
                    var res = JsonConvert.SerializeObject(result);
                    response = JsonConvert.DeserializeObject<ReadPidPresponseModel>(res);
                    return response;
                });

                return respo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        static float flashingPercent;
        public async Task<float> FlashingData()
        {

            try
            {
                flashingPercent = await dSDiagnostic.GetRuntimeFlashPercent();

                return flashingPercent;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }


    #region Old Code
    //public class Bth2 : IBth
    //{
    //    const string TARGET_UUID = "00001105-0000-1000-8000-00805f9b34fb";
    //    BluetoothSocket socket = null;
    //    OutputStreamInvoker outStream = null;
    //    InputStreamInvoker inStream = null;
    //    InputStreamReader inputStreamReader = null;
    //    BufferedReader bufferedReader = null;
    //    DongleCommWin dongleCommWin;
    //    UDSDiagnostic dSDiagnostic;

    //    private APDiagnosticAndroid.Models.ReadDtcResponseModel readDTCResponse;

    //    private CancellationTokenSource _ct { get; set; }

    //    const int RequestResolveError = 1000;

    //    private Context context;

    //    public Bth2(Context context)
    //    {
    //        this.context = context;
    //    }
    //    public Bth2()
    //    {
    //    }
    //    public async Task<string> Start(string name, int sleepTime, bool readAsCharArray)
    //    {
    //        //Task.Run(() => Connect(name, sleepTime, readAsCharArray));
    //        //await Connect(name, sleepTime, readAsCharArray);

    //        if (socket == null)
    //        {
    //            await Task.Delay(100);
    //            var isCompletedSuccessfully = Task.Run(async () => loop(name, sleepTime, readAsCharArray)).Wait(TimeSpan.FromSeconds(70));

    //            if (isCompletedSuccessfully)
    //            {
    //                return "connected";
    //            }
    //            else
    //            {
    //                return "not connected";
    //            }
    //            //Task.Factory.StartNew(async () =>
    //            //{
    //            //    Connect(name, sleepTime, readAsCharArray);
    //            //}).Result.ConfigureAwait(false);
    //        }
    //        else
    //        {
    //            socket.Close();
    //            socket = null;
    //            return "not connected";
    //        }
    //    }

    //    public void Cancel()
    //    {
    //        if (_ct != null)
    //        {
    //            System.Diagnostics.Debug.WriteLine("Send a cancel to task!");
    //            _ct.Cancel();
    //        }
    //    }
    //    public ObservableCollection<BluetoothDevicesModel> PairedDevices()
    //    {
    //        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
    //        ObservableCollection<BluetoothDevicesModel> devices = new ObservableCollection<BluetoothDevicesModel>();
    //        //devices.Add("");
    //        var device_list = adapter.BondedDevices.Where(x => x.Name == "OBDII BT Dongle" || x.Name == "APtBudB");
    //        foreach (var bd in device_list)
    //            devices.Add(new BluetoothDevicesModel { Name = bd.Name, Mac_Address = bd.Address });
    //        BluetoothDevice bluetoothDevice = adapter.GetRemoteDevice("");
    //        return devices;
    //    }

    //    public async Task<string> Connect()
    //    {
    //        //byte[] ba = Encoding.Default.GetBytes("500C47568AFE56214E238000FFC3");
    //        //byte[] ba = Encoding.Default.GetBytes("500C65EFA86574FE9A890018CE16");
    //        //SendCommand("500C47568AFE56214E238000FFC3");

    //        var firmware_version = await SendMessage("500C47568AFE56214E238000FFC3");
    //        return firmware_version;
    //    }

    //    public async Task<Model.ReadDtcResponseModel> ReadDtc(string dtc_index)
    //    {
    //        try
    //        {
    //            if (dtc_index == "UDS-2BYTE-DTC")
    //            {
    //                dtc_index = "UDS_2BYTE12_DTC";
    //            }

    //            Model.ReadDtcResponseModel readDtcResponseModel = new Model.ReadDtcResponseModel();
    //            ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
    //            await Task.Run(async () =>
    //            {
    //                readDTCResponse = await dSDiagnostic.ReadDTC(index);
    //            });

    //            readDtcResponseModel.dtcs = readDTCResponse.dtcs;
    //            readDtcResponseModel.status = readDTCResponse.status;
    //            readDtcResponseModel.noofdtc = readDTCResponse.noofdtc;
    //            return readDtcResponseModel;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }

    //    public async Task<string> ClearDtc(string dtc_index)
    //    {
    //        try
    //        {
    //            if (dtc_index == "UDS-4BYTES")
    //            {
    //                dtc_index = "UDS_4BYTES";
    //            }

    //            string status = string.Empty;
    //            object result = new object();
    //            ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
    //            await Task.Run(async () =>
    //            {

    //                result = await dSDiagnostic.ClearDTC(index);
    //                var res = JsonConvert.SerializeObject(result);
    //                var Response = JsonConvert.DeserializeObject<ClearDtcResponseModel>(res);
    //                status = Response.ECUResponseStatus;
    //            });

    //            return status;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }

    //    public async Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<Model.ReadParameterPID> pidList)
    //    {
    //        try
    //        {
    //            object result = new object();
    //            ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID>();
    //            foreach (var item in pidList)
    //            {
    //                var MessageValueList = new List<APDiagnosticAndroid.Models.SelectedParameterMessage>();
    //                if (item.messages != null)
    //                {
    //                    foreach (var MessageItem in item.messages)
    //                    {
    //                        MessageValueList.Add(new APDiagnosticAndroid.Models.SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
    //                    }
    //                }

    //                list.Add(
    //                    new APDiagnosticAndroid.Models.ReadParameterPID
    //                    {
    //                        datatype = item.datatype,
    //                        IsBitcoded = item.IsBitcoded,
    //                        noofBits = item.noofBits,
    //                        noOfBytes = item.noOfBytes,
    //                        offset = item.offset,
    //                        pid = item.pid,
    //                        resolution = item.resolution,
    //                        startBit = item.startBit,
    //                        startByte = item.startByte,
    //                        totalBytes = item.totalBytes,
    //                        totalLen = item.totalLen,
    //                        pidNumber = item.pidNumber,
    //                        pidName = item.pidName,
    //                        messages = MessageValueList
    //                    });
    //            }

    //            var respo = await Task.Run(async () =>
    //             {
    //                 result = await dSDiagnostic.ReadParameters(pidList.Count, list);
    //                 var res = JsonConvert.SerializeObject(result);
    //                 var res_list = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(res);
    //                 return res_list;
    //                 //status = Response.ECUResponseStatus;
    //             });

    //            return respo;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }

    //    public async Task<ObservableCollection<WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList)
    //    {
    //        try
    //        {
    //            WriteParameterIndex index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), write_pid_intdex);
    //            ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID>();
    //            foreach (var item in pidList)
    //            {
    //                SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), item.seedkeyindex);
    //                WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), item.writepamindex);
    //                list.Add(
    //                    new APDiagnosticAndroid.Models.WriteParameterPID
    //                    {
    //                        seedkeyindex = seed_index,//item.seedkeyindex,
    //                        writepamindex = write_index, //item.writepamindex,
    //                        writeparadata = item.writeparadata,
    //                        writeparadatasize = item.writeparadatasize,
    //                        writeparapid = item.writeparapid,
    //                        ReadParameterPID_DataType = item.ReadParameterPID_DataType,
    //                        pid = item.pid,
    //                        startByte = item.startByte,
    //                        totalBytes = item.totalBytes
    //                        //writeparaName = item.
    //                    });
    //            }
    //            var respo = await Task.Run(async () =>
    //            {
    //                var result = await dSDiagnostic.WriteParameters(pidList.Count, index, list);
    //                var res = JsonConvert.SerializeObject(result);
    //                var res_list = JsonConvert.DeserializeObject<ObservableCollection<WriteParameter_Status>>(res);
    //                return res_list;
    //            });
    //            return respo;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }

    //    private async Task<string> SendMessage(string message)
    //    {
    //        try
    //        {

    //            //dongleCommWin = new DongleCommWin(socket, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x7E0, 0x7E8, 0x00, 0x10, 0x10, 0x10);
    //            dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.Bluetooth);
    //            dSDiagnostic = new UDSDiagnostic(dongleCommWin);

    //            var securityAccess = await dongleCommWin.SecurityAccess();
    //            var securityResult = (byte[])securityAccess;
    //            var securityResponse = ByteArrayToString(securityResult);
    //            var setProtocol = await dongleCommWin.Dongle_SetProtocol(2);
    //            var ProtocolResult = (byte[])setProtocol;
    //            var ProtocolResponse = ByteArrayToString(ProtocolResult);
    //            var setHeader = await dongleCommWin.CAN_SetTxHeader("07e0");
    //            var HeaderResult = (byte[])setHeader;
    //            var HeaderResponse = ByteArrayToString(HeaderResult);
    //            var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask("07e8");
    //            var HeaderMarkResult = (byte[])setHeaderMask;
    //            var HeaderMarkResponse = ByteArrayToString(HeaderMarkResult);
    //            var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
    //            var firmwareResult = (byte[])firmwareVersion;
    //            var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");

    //            var version = ByteArrayToString(firmwareResult);

    //            byte[] byteArray = Encoding.ASCII.GetBytes(message);
    //            await socket.OutputStream.WriteAsync(byteArray, 0, byteArray.Length);
    //            await socket.OutputStream.FlushAsync();

    //            byte[] Rbuffer = new byte[1024];
    //            byte[] RArray = new byte[] { };
    //            try
    //            {
    //                int readByte = socket.InputStream.Read(Rbuffer, 0, Rbuffer.Length);
    //                RArray = new byte[readByte];
    //                Array.Copy(Rbuffer.ToArray(), 0, RArray, 0, readByte);
    //                string ReceivedMessage = ConvertedByteToString(RArray);
    //            }
    //            catch (Exception ex)
    //            {

    //                throw;
    //            }

    //            return ver;

    //            //uint messageLength = (uint)message.Length;
    //            //byte[] countBuffer = BitConverter.GetBytes(messageLength);
    //            //byte[] buffer = HexStringToByteArray(message);

    //            //await socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    //            //await socket.OutputStream.FlushAsync();
    //            //await outStream.WriteAsync(buffer, 0, buffer.Length);
    //            //await outStream.WriteAsync(buffer, 0, buffer.Length);
    //            //ReadData(out string data);

    //        }
    //        catch (Exception ex)
    //        {
    //            return "";
    //        }
    //    }
    //    static string ConvertedByteToString(byte[] bytes)
    //    {
    //        using (var stream = new MemoryStream(bytes))
    //        {
    //            using (var stremReader = new StreamReader(stream))
    //            {
    //                return stremReader.ReadToEnd();
    //            }
    //        }
    //    }

    //    //private async Task ConnectionStatus()
    //    //{
    //    //    if (socket.IsConnected)
    //    //    {
    //    //        socket.Dispose();
    //    //    }
    //    //}

    //    public string ConnectedORNot()
    //    {
    //        if (socket.IsConnected)
    //        {
    //            socket.Dispose();
    //            return "SokectConnected";
    //        }
    //        else
    //        {
    //            return "SokectConnected";
    //        }
    //    }
    //    public string MessageToSend = String.Empty;

    //    private async Task loop1(string name, int sleepTime, bool readAsCharArray)
    //    {
    //        BluetoothDevice device = null;
    //        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
    //        BluetoothSocket bthSocket = null;
    //        BluetoothServerSocket bthServerSocket = null;

    //        UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
    //        bthServerSocket = adapter.ListenUsingRfcommWithServiceRecord("TLCI Barcode Scanner", uuid);

    //        _ct = new CancellationTokenSource();

    //        while (_ct.IsCancellationRequested == false)
    //        {
    //            try
    //            {
    //                Thread.Sleep(250);

    //                adapter = BluetoothAdapter.DefaultAdapter;

    //                if (adapter == null)
    //                    Debug.Write("No bluetooth adapter found!");
    //                else
    //                    Debug.Write("Adapter found!");

    //                if (!adapter.IsEnabled)
    //                    Debug.Write("Bluetooth adapter is not enabled.");
    //                else
    //                    Debug.Write("Adapter found!");

    //                Debug.Write("Try to connect to " + name);

    //                var aa = adapter.BondedDevices.Count;
    //                foreach (var bondedDevice in adapter.BondedDevices)
    //                {
    //                    Debug.Write("Paired devices found: " + bondedDevice.Address.ToUpper());

    //                    if (bondedDevice.Address == name)
    //                    {
    //                        Debug.Write("Found " + bondedDevice.Address + ". Try to connect with it!");
    //                        device = bondedDevice;
    //                        Debug.Write(bondedDevice.Type.ToString());
    //                        break;
    //                    }
    //                }

    //                if (device == null)
    //                    Debug.Write("Named device not found.");
    //                else
    //                {
    //                    bthSocket = bthServerSocket.Accept();

    //                    adapter.CancelDiscovery();

    //                    if (bthSocket != null)
    //                    {
    //                        Debug.Write("Connected");

    //                        if (bthSocket.IsConnected)
    //                        {
    //                            var mReader = new InputStreamReader(bthSocket.InputStream);
    //                            var buffer = new BufferedReader(mReader);

    //                            while (_ct.IsCancellationRequested == false)
    //                            {
    //                                if (MessageToSend != null)
    //                                {
    //                                    var chars = MessageToSend.ToCharArray();
    //                                    var bytes = new List<byte>();

    //                                    foreach (var character in chars)
    //                                    {
    //                                        bytes.Add((byte)character);
    //                                    }

    //                                    await bthSocket.OutputStream.WriteAsync(bytes.ToArray(), 0, bytes.Count);

    //                                    MessageToSend = null;
    //                                }
    //                            }

    //                        }
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                Debug.Write(ex);
    //                Debug.Write(ex.Message);
    //            }
    //            finally
    //            {
    //                if (bthSocket != null)
    //                    bthSocket.Close();

    //                device = null;
    //                adapter = null;
    //            }
    //        }
    //    }

    //    private async Task loop(string address, int sleepTime, bool readAsCharArray)
    //    {
    //        BluetoothDevice device = null;
    //        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
    //        BluetoothServerSocket bthServerSocket = null;

    //        _ct = new CancellationTokenSource();
    //        while (_ct.IsCancellationRequested == false)
    //        {
    //            try
    //            {
    //                Thread.Sleep(sleepTime);

    //                adapter = BluetoothAdapter.DefaultAdapter;

    //                //List<BluetoothDevice> L = new List<BluetoothDevice>();
    //                //foreach (BluetoothDevice d in adapter.BondedDevices)
    //                //{
    //                //    System.Diagnostics.Debug.WriteLine("D: " + d.Name + " " + d.Address + " " + d.BondState.ToString());
    //                //    L.Add(d);
    //                //}
    //                //device = L.Find(j => j.Address == name);

    //                device = BluetoothStatic.bluetoothDevices.FirstOrDefault(x => x.Address == address);//bluetoothDevice;
    //                BluetoothDevice paired_device = BluetoothAdapter.DefaultAdapter.BondedDevices.
    //                    FirstOrDefault(x => x.Address == address);

    //                if (paired_device == null)
    //                {

    //                    var returnValue = device.CreateBond();
    //                    var state = device.BondState;
    //                }
    //                if (device == null)
    //                {
    //                    //System.Diagnostics.Debug.WriteLine("Named device not found.");
    //                    //Xamarin.Forms.MessagingCenter.Send<App, string>((App)Xamarin.Forms.Application.Current, "TerminalLog", "Named device not found.");
    //                }
    //                else
    //                {
    //                    UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
    //                    //bthServerSocket = adapter.ListenUsingRfcommWithServiceRecord("TLCI Barcode Scanner", uuid);

    //                    UUID uuids = device.GetUuids()[0].Uuid;

    //                    if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
    //                        socket = device.CreateInsecureRfcommSocketToServiceRecord(uuid);
    //                    else
    //                        socket = device.CreateRfcommSocketToServiceRecord(uuid);

    //                    //  socket = bthServerSocket.Accept();

    //                    //   adapter.CancelDiscovery();
    //                    if (socket != null)
    //                    {
    //                        //await Task.Delay(2000);
    //                        socket.Connect();
    //                        if (socket.IsConnected)
    //                        {
    //                            outStream = (OutputStreamInvoker)socket.OutputStream;
    //                            inputStreamReader = new InputStreamReader(socket.InputStream);
    //                            bufferedReader = new BufferedReader(inputStreamReader);
    //                            break;
    //                        }
    //                    }
    //                    else
    //                        System.Diagnostics.Debug.WriteLine("BthSocket = null");
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                System.Diagnostics.Debug.WriteLine("EXCEPTION: " + ex.Message);
    //            }
    //            finally
    //            {
    //                //if (socket != null)
    //                //    socket.Close();
    //                //device = null;
    //                //adapter = null;
    //            }
    //        }
    //        System.Diagnostics.Debug.WriteLine("Exit the external loop");
    //    }


    //    //private async Task loop(string address, int sleepTime, bool readAsCharArray)
    //    //{
    //    //    BluetoothDevice device = null;
    //    //    BluetoothDevice paired_device = null;
    //    //    BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

    //    //    _ct = new CancellationTokenSource();
    //    //    while (_ct.IsCancellationRequested == false)
    //    //    {
    //    //        try
    //    //        {
    //    //            Thread.Sleep(sleepTime);

    //    //            adapter = BluetoothAdapter.DefaultAdapter;

    //    //            //List<BluetoothDevice> L = new List<BluetoothDevice>();
    //    //            //foreach (BluetoothDevice d in adapter.BondedDevices)
    //    //            //{
    //    //            //    System.Diagnostics.Debug.WriteLine("D: " + d.Name + " " + d.Address + " " + d.BondState.ToString());
    //    //            //    L.Add(d);
    //    //            //}
    //    //            //device = L.Find(j => j.Address == name);

    //    //            device = BluetoothStatic.bluetoothDevices.FirstOrDefault(x => x.Address == address);//bluetoothDevice;
    //    //            paired_device = BluetoothAdapter.DefaultAdapter.BondedDevices.
    //    //                FirstOrDefault(x => x.Address == address);

    //    //            if (paired_device == null)
    //    //            {
    //    //                //var isCompletedSuccessfully = Task.Run(async () =>
    //    //                var returnValue = device.CreateBond();
    //    //                var state = device.BondState;
    //    //                //).Wait(TimeSpan.FromSeconds(70));

    //    //                //var paired = await Task.Run( () =>
    //    //                //{
    //    //                //    var returnValue = device.CreateBond();
    //    //                //    var state = device.BondState;
    //    //                //    return true;
    //    //                //});
    //    //                //if(paired)
    //    //                //{
    //    //                //    paired_device = BluetoothAdapter.DefaultAdapter.BondedDevices.
    //    //                //   FirstOrDefault(x => x.Address == address);
    //    //                //    connectionMethod(paired_device);
    //    //                //}
    //    //                //return;

    //    //            }
    //    //            if (device == null)
    //    //            {
    //    //                //System.Diagnostics.Debug.WriteLine("Named device not found.");
    //    //                //Xamarin.Forms.MessagingCenter.Send<App, string>((App)Xamarin.Forms.Application.Current, "TerminalLog", "Named device not found.");
    //    //            }
    //    //            else
    //    //            {
    //    //                //connectionMethod(device);
    //    //                UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
    //    //                if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
    //    //                    socket = device.CreateInsecureRfcommSocketToServiceRecord(uuid);
    //    //                else
    //    //                    socket = device.CreateRfcommSocketToServiceRecord(uuid);

    //    //                if (socket != null)
    //    //                {
    //    //                    socket.Connect();
    //    //                    if (socket.IsConnected)
    //    //                    {
    //    //                        outStream = (OutputStreamInvoker)socket.OutputStream;
    //    //                        inputStreamReader = new InputStreamReader(socket.InputStream);
    //    //                        bufferedReader = new BufferedReader(inputStreamReader);
    //    //                        break;
    //    //                    }
    //    //                }
    //    //                else
    //    //                    System.Diagnostics.Debug.WriteLine("BthSocket = null");
    //    //            }
    //    //        }
    //    //        catch (Exception ex)
    //    //        {
    //    //            System.Diagnostics.Debug.WriteLine("EXCEPTION: " + ex.Message);
    //    //        }
    //    //        finally
    //    //        {
    //    //            //if (socket != null)
    //    //            //    socket.Close();
    //    //            //device = null;
    //    //            //adapter = null;
    //    //        }
    //    //    }
    //    //    System.Diagnostics.Debug.WriteLine("Exit the external loop");
    //    //}

    //    public void connectionMethod(BluetoothDevice device)
    //    {
    //        try
    //        {
    //            UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
    //            if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
    //                socket = device.CreateInsecureRfcommSocketToServiceRecord(uuid);
    //            else
    //                socket = device.CreateRfcommSocketToServiceRecord(uuid);

    //            if (socket != null)
    //            {
    //                socket.Connect();
    //                if (socket.IsConnected)
    //                {
    //                    outStream = (OutputStreamInvoker)socket.OutputStream;
    //                    inputStreamReader = new InputStreamReader(socket.InputStream);
    //                    bufferedReader = new BufferedReader(inputStreamReader);
    //                    return;
    //                }
    //            }
    //            else
    //                System.Diagnostics.Debug.WriteLine("BthSocket = null");
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }

    //    private string ByteArrayToString(byte[] ba)
    //    {
    //        string hex = BitConverter.ToString(ba);
    //        return hex.Replace("-", "");
    //    }


    //    public async Task<object> StartECUFlashing(string flashJson, Ecu2 ecu2, SeedkeyalgoFnIndex sklFN, List<EcuMapFile> ecu_map_file)
    //    {
    //        try
    //        {
    //            var jsonData = JsonConvert.DeserializeObject<FlashingMatrixData>(flashJson);

    //            EraseSectorEnum erase_type = (EraseSectorEnum)Enum.Parse(typeof(EraseSectorEnum), ecu2.flash_erase_type);
    //            ChecksumSectorEnum check_sum_type = (ChecksumSectorEnum)Enum.Parse(typeof(ChecksumSectorEnum), ecu2.flash_check_sum_type);
    //            SEEDKEYINDEXTYPE seedkeyindx = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), sklFN.value);

    //            //byte asdb = Convert.ToByte(ecu2.flash_address_data_format);
    //            //UInt16 acc = Convert.ToUInt16(ecu2.sectorframetransferlen,16);

    //            var flashConfig = new flashconfig
    //            {
    //                //addrdataformat = Convert.ToByte(ecu2.flash_address_data_format, 16),
    //                checksumsector = check_sum_type,
    //                diag_mode = Convert.ToByte(ecu2.flash_diagnostic_mode, 16),
    //                erasesector = erase_type,
    //                //flash_index = FLASHINDEXTYPE.GREAVES_BOSCH_BS6,
    //                sectorframetransferlen = Convert.ToUInt16(ecu2.sectorframetransferlen, 16),
    //                seedkeyindex = seedkeyindx,
    //                seedkeynumbytes = Convert.ToByte(ecu2.flash_seed_key_length, 16),
    //                sendseedbyte = Convert.ToByte(ecu2.sendseedbyte, 16),
    //                septime = Convert.ToByte(ecu2.flashsep_time, 16),
    //            };


    //            string response = string.Empty;

    //            Stopwatch stopWatch = new Stopwatch();
    //            stopWatch.Start();

    //            await Task.Run(async () =>
    //            {
    //                //response = await dSDiagnostic.StartFlashBosch(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
    //            });

    //            stopWatch.Stop();
    //            // Get the elapsed time as a TimeSpan value.
    //            TimeSpan ts = stopWatch.Elapsed;

    //            // Format and display the TimeSpan value.
    //            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
    //                ts.Hours, ts.Minutes, ts.Seconds,
    //                ts.Milliseconds / 10);
    //            Console.WriteLine("RunTime " + elapsedTime);
    //            Debug.WriteLine("------RunTime------" + elapsedTime, "");


    //            // MessageDialog showDialog = new MessageDialog(response);
    //            //showDialog.Commands.Add(new UICommand("Ok")
    //            //{
    //            //    Id = 0
    //            //});
    //            //showDialog.Commands.Add(new UICommand("No")
    //            //{
    //            //    Id = 1
    //            //});
    //            //showDialog.DefaultCommandIndex = 0;
    //            //showDialog.CancelCommandIndex = 1;
    //            //var results = await showDialog.ShowAsync();

    //            return response;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }


    //    /// <summary>
    //    /// /
    //    /// </summary>
    //    /// <param name="message"></param>
    //    //private async void SendMessageBT(string message)
    //    ////{
    //    //    try
    //    //    {
    //    //        //byte[] bytes = Encoding.UTF8.GetBytes(message);
    //    //        string trimmed = String.Concat(message.Where(c => !Char.IsWhiteSpace(c)));
    //    //        var bytes = StringToByteArray(trimmed);
    //    //        await outStream.WriteAsync(bytes, 0, bytes.Length);
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //    }
    //    //}



    //    //private async void SendCommand(string message)
    //    //{
    //    //    // byte[] bytes = Encoding.ASCII.GetBytes(message);
    //    //    byte[] bytes = Encoding.UTF8.GetBytes(message);
    //    //    await outStream.WriteAsync(bytes, 0, bytes.Length);
    //    //}


    //    //private void ReadData(out string data)
    //    //{
    //    //    byte[] rbuffer = new byte[1024];
    //    //    byte[] RetArray = new byte[] { };
    //    //    try
    //    //    {

    //    //        // Read data from the device
    //    //        while (!socket.InputStream.CanRead)
    //    //        {
    //    //            //Console.WriteLine("------------------------------------------------");
    //    //        }
    //    //        int readByte = socket.InputStream.Read(rbuffer, 0, rbuffer.Length);

    //    //        RetArray = new byte[readByte];
    //    //        Array.Copy(rbuffer.ToArray(), 0, RetArray, 0, readByte);
    //    //        //Console.WriteLine(Array2Text(RetArray, false) + "\n" + "[" + RetArray.Length.ToString() + "]");

    //    //    }
    //    //    catch (Java.IO.IOException e)
    //    //    {

    //    //    }
    //    //    data = "";
    //    //}



    //    //private byte[] HexStringToByteArray(String hex)
    //    //{
    //    //    hex = hex.Replace(" ", "");
    //    //    int numberChars = hex.Length;
    //    //    if (numberChars % 2 != 0)
    //    //    {
    //    //        hex = "0" + hex;
    //    //        numberChars++;
    //    //    }
    //    //    byte[] bytes = new byte[numberChars / 2];
    //    //    for (int i = 0; i < numberChars; i += 2)
    //    //        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
    //    //    return bytes;
    //    //}


    //    //private async Task Connect(string deviceName, int sleepTime, bool readAsCharArray)
    //    //{
    //    //    try
    //    //    {
    //    //        _ct = new CancellationTokenSource();
    //    //        while (_ct.IsCancellationRequested == false)
    //    //        {
    //    //            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
    //    //            if (adapter == null)
    //    //                System.Diagnostics.Debug.WriteLine("No Bluetooth adapter found.");
    //    //            else if (!adapter.IsEnabled)
    //    //                System.Diagnostics.Debug.WriteLine("Bluetooth adapter is not enabled.");

    //    //            List<BluetoothDevice> L = new List<BluetoothDevice>();
    //    //            foreach (BluetoothDevice d in adapter.BondedDevices)
    //    //            {
    //    //                System.Diagnostics.Debug.WriteLine("D: " + d.Name + " " + d.Address + " " + d.BondState.ToString());
    //    //                L.Add(d);
    //    //            }

    //    //            BluetoothDevice device = null;
    //    //            device = L.Find(j => j.Address == deviceName);

    //    //            if (device == null)
    //    //                System.Diagnostics.Debug.WriteLine("Named device not found.");
    //    //            else
    //    //            {
    //    //                System.Diagnostics.Debug.WriteLine("Device has been found: " + device.Name + " " + device.Address + " " + device.BondState.ToString());
    //    //            }

    //    //            socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString(TARGET_UUID));
    //    //            //socket = device.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString(TARGET_UUID));
    //    //            await socket.ConnectAsync();
    //    //            //socket.Connect();

    //    //            if (socket != null && socket.IsConnected)
    //    //                System.Diagnostics.Debug.WriteLine("Connection successful!");
    //    //            else
    //    //                System.Diagnostics.Debug.WriteLine("Connection failed!");

    //    //            inStream = (InputStreamInvoker)socket.InputStream;
    //    //            outStream = (OutputStreamInvoker)socket.OutputStream;

    //    //            if (socket != null && socket.IsConnected)
    //    //            {
    //    //                Task t = new Task(() => Listen(inStream));
    //    //                t.Start();
    //    //            }
    //    //            else throw new Exception("Socket not existing or not connected.");
    //    //        }
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //    }
    //    //}
    //    //public static byte[] StringToByteArray(string hex)
    //    //{
    //    //    return Enumerable.Range(0, hex.Length).
    //    //           Where(x => 0 == x % 2).
    //    //           Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
    //    //           ToArray();
    //    //}

    //    //public string ASCIITOHex(string ascii)
    //    //{
    //    //    StringBuilder sb = new StringBuilder();
    //    //    byte[] inputBytes = Encoding.UTF8.GetBytes(ascii);
    //    //    foreach (byte b in inputBytes)
    //    //    {
    //    //        sb.Append(string.Format("{0:x2}", b));
    //    //    }
    //    //    return sb.ToString();
    //    //}

    //    //private async void Listen(Stream inStream)
    //    //{
    //    //    //bool Listening = true;
    //    //    System.Diagnostics.Debug.WriteLine("Listening has been started.");
    //    //    byte[] uintBuffer = new byte[sizeof(uint)]; // This reads the first 4 bytes which form an uint that indicates the length of the string message.
    //    //    byte[] textBuffer; // This will contain the string message.

    //    //    // Keep listening to the InputStream while connected.
    //    //    while (_ct.IsCancellationRequested)
    //    //    {
    //    //        try
    //    //        {
    //    //            // This one blocks until it gets 4 bytes.
    //    //            await inStream.ReadAsync(uintBuffer, 0, uintBuffer.Length);
    //    //            uint readLength = BitConverter.ToUInt32(uintBuffer, 0);

    //    //            textBuffer = new byte[readLength];
    //    //            // Here we know for how many bytes we are looking for.
    //    //            await inStream.ReadAsync(textBuffer, 0, (int)readLength);

    //    //            string s = Encoding.UTF8.GetString(textBuffer);
    //    //            System.Diagnostics.Debug.WriteLine("Received: " + s);
    //    //        }
    //    //        catch (Java.IO.IOException e)
    //    //        {
    //    //            System.Diagnostics.Debug.WriteLine("Error: " + e.Message);
    //    //            ///Listening = false;
    //    //            break;
    //    //        }
    //    //    }
    //    //    System.Diagnostics.Debug.WriteLine("Listening has ended.");
    //    //}
    //}
    #endregion
}