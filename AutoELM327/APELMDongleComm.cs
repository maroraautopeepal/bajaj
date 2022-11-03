using Android.Bluetooth;
using Android.Util;
using AutoELM327.Enums;
using AutoELM327.Helper;
using AutoELM327.Models;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using Java.IO;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoELM327
{
    //----------------------------------------------------------------------------
    // Namespace        : AutoELM327
    // Class Name       : BluetoothHandler
    // Description      : This class implements IBluetoothHandler interface
    // Author           : Autopeepal  
    // Date             : 20-08-20
    // Notes            : 
    // Revision History : 
    //----------------------------------------------------------------------------
    public class APELMDongleComm : IBluetoothHandler, IATCommands
    {
        #region Properties
        private ResponseArrayStatus responseStructure;
        BluetoothAdapter BluetoothAdapter { get; set; } = null;
        public Platform Platform { get; private set; }
        public Connectivity Connectivity { get; private set; }
        public SerialInputOutputManager SerialInputOutputManager { get; }

        UsbSerialPort USBport = null;
        private ProtocolEnum protocol;
        BluetoothDevice device;
        BluetoothSocket BthSocket = null;
        InputStreamReader inputStreamReader = null;
        string DebugTag = "ELM-DEBUG";
        string rawData;
        #endregion

        #region Ctor
        public APELMDongleComm()
        {
            Debug.WriteLine("Inside ELM", DebugTag);
            BluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (BluetoothAdapter == null)
                Debug.WriteLine("No Bluetooth adapter found.", DebugTag);
            else
                Debug.WriteLine("Bluetooth Adapter found!!");

            if (!BluetoothAdapter.IsEnabled)
                Debug.WriteLine("Bluetooth adapter is not enabled.", DebugTag);
            else
                Debug.WriteLine("Bluetooth Adapter enabled!", DebugTag);
        }

        public APELMDongleComm(BluetoothSocket BluetoothSocket, ProtocolEnum ProtocolEnum)
        {
            Debug.WriteLine("Inside ELM BluetoothSocket CTOR", DebugTag);
            BthSocket = BluetoothSocket;
            protocol = ProtocolEnum;
            //this.inputStreamReader = inputStreamReader;
        }

        public void InitializePlatform(Platform platform, Connectivity connectivity)
        {
            Debug.WriteLine("Inside InitializePlatform", DebugTag);
            Platform = platform;
            Connectivity = connectivity;
        }

        public APELMDongleComm(SerialInputOutputManager serialInputOutputManager, UsbSerialPort port, ProtocolEnum protocolVersion)
        {
            Debug.WriteLine("Inside ELM327 SerialInputOutputManager CTOR", DebugTag);
            SerialInputOutputManager = serialInputOutputManager;
            USBport = port;
            protocol = protocolVersion;

        }
        #endregion

        #region Methods
        //----------------------------------------------------------------------------
        // Method Name   : BT_ConnectDevice
        // Input         : DEVICENAME
        // Output        : status of ConnectDevice form of bool
        // Purpose       : Connect with a BT / BLE Device and generate a handle
        // Date          : 20-08-20
        //---------------------------------------------------------------------------
        ////    public async Task <bool> BT_ConnectDevice(string deviceName)
        ////{
        ////    Debug.WriteLine("Try to connect to " + deviceName, DebugTag);
        ////    foreach (var bluetoothdevice in BluetoothAdapter.BondedDevices)
        ////    {
        ////        Debug.WriteLine("Paired devices found: " + bluetoothdevice.Name.ToUpper(), DebugTag);
        ////        if (bluetoothdevice.Name.ToUpper().IndexOf(deviceName.ToUpper()) >= 0)
        ////        {

        ////            Debug.WriteLine("Found " + bluetoothdevice.Name + ". Try to connect with it!", DebugTag);
        ////            device = bluetoothdevice;
        ////            break;
        ////        }
        ////    }

        ////    if (device == null)
        ////        Debug.WriteLine("Named device not found.", DebugTag);
        ////    else
        ////    {
        ////        UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
        ////        if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
        ////            BthSocket = device.CreateInsecureRfcommSocketToServiceRecord(uuid);
        ////        else
        ////            BthSocket = device.CreateRfcommSocketToServiceRecord(uuid);
        ////        if (BthSocket != null)
        ////            Debug.WriteLine("BthSocket NOT NULL", DebugTag);
        ////        try
        ////        {
        ////            BthSocket?.Connect();
        ////        }
        ////        catch (Exception ex)
        ////        {

        ////            Debug.WriteLine("EXCEPTION + " + ex.Message, DebugTag);
        ////            return false;
        ////        }

        ////    }



        ////    return true;
        ////}

        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        async Task<string> SendRequest(string CommandValue)
        {
            var firmware_version = await SendMessage(CommandValue + "\r");
            return firmware_version;
        }
        private async Task<string> SendMessage(string message)
        {
            try
            {
                var datastream = BthSocket.OutputStream;
                byte[] byteArray = Encoding.ASCII.GetBytes(message);
                await BthSocket.OutputStream.WriteAsync(byteArray, 0, byteArray.Length);
                await BthSocket.OutputStream.FlushAsync();
                var response = ReadAgain();
                if (!response.Contains(">"))
                {
                    var response2 = ReadAgain();
                }
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                string trimmed = String.Concat(message.Where(c => !Char.IsWhiteSpace(c)));
                return trimmed;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #region Original Code
        private string ReadAgain1()
        {
            byte[] Rbuffer = new byte[1024];
            byte[] RArray = new byte[] { };
            try
            {
                int readByte = BthSocket.InputStream.Read(Rbuffer, 0, Rbuffer.Length);
                RArray = new byte[readByte];
                Array.Copy(Rbuffer.ToArray(), 0, RArray, 0, readByte);
                string ReceivedMessage = ByteArrayToString(RArray);
                string ReceivedMessage2 = System.Text.Encoding.ASCII.GetString(RArray);
                return ReceivedMessage2;
            }
            catch (Java.IO.IOException ex)
            {
                return ex.Message;
            }
        }
        #endregion

        private string ReadAgain()
        {
            byte[] Rbuffer = new byte[1024];
            byte[] RArray = new byte[] { };
            try
            {
                int readByte = BthSocket.InputStream.Read(Rbuffer, 0, Rbuffer.Length);
                RArray = new byte[readByte];
                //Array.Copy(Rbuffer.ToArray(), 0, RArray, 0, readByte);
                Buffer.BlockCopy(Rbuffer.ToArray(), 0, RArray, 0, readByte);
                string ReceivedMessage = ByteArrayToString(RArray);
                string ReceivedMessage2 = System.Text.Encoding.ASCII.GetString(RArray);
                BthSocket.InputStream.Flush();
                return ReceivedMessage2;
            }
            catch (Java.IO.IOException ex)
            {
                return ex.Message;
            }
        }

        public string readInputStreamWithTimeout(int timeoutMillis)
        {
            long originalPosition = 0;

            if (BthSocket.InputStream.CanSeek)
            {
                originalPosition = BthSocket.InputStream.Position;
                BthSocket.InputStream.Position = 0;
            }

            try
            {
                int totalBytesRead = 0;
                int bytesRead;
                byte[] command = new byte[4096];
                while ((bytesRead = BthSocket.InputStream.Read(command, totalBytesRead, command.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == command.Length)
                    {
                        int nextByte = BthSocket.InputStream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[command.Length * 2];
                            Buffer.BlockCopy(command, 0, temp, 0, command.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            command = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = command;
                if (command.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(command, 0, buffer, 0, totalBytesRead);
                }
                string ReceivedMessage = ByteArrayToString(buffer);
                string ReceivedMessage2 = System.Text.Encoding.ASCII.GetString(buffer);
                return ReceivedMessage2;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            finally
            {
                if (BthSocket.InputStream.CanSeek)
                {
                    BthSocket.InputStream.Position = originalPosition;
                }
            }
        }


        //public int readInputStreamWithTimeout(byte[] command, int timeoutMillis)
        //{
        //    int bufferOffset = 0;
        //    long maxTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + timeoutMillis;
        //    while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < maxTimeMillis && bufferOffset < command.Length)
        //    {
        //        int readLength = java.lang.Math.min(is.available(), b.length - bufferOffset);
        //        // can alternatively use bufferedReader, guarded by isReady():
        //        int readResult = is.read(b, bufferOffset, readLength);
        //        if (readResult == -1) break;
        //        bufferOffset += readResult;
        //    }
        //    return bufferOffset;
        //}

        private void ReadData(out string data)
        {
            Debug.WriteLine("INSIDE READ DATA ", DebugTag);
            StringBuilder res = new StringBuilder();
            try
            {
                byte b = 0;
                // read until '>' arrives OR end of stream reached
                char c;
                // -1 if the end of the stream is reached
                while ((b = ((byte)BthSocket.InputStream.ReadByte())) > -1)
                {
                    c = (char)b;
                    if (c == '>') // read until '>' arrives
                    {
                        break;
                    }
                    res.Append(c);
                }

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }

            data = res.ToString().Replace("SEARCHING", "");

            /*
             * Data may have echo or informative text like "INIT BUS..." or similar.
             * The response ends with two carriage return characters. So we need to take
             * everything from the last carriage return before those two (trimmed above).
             */

            data = data.Replace("\\s", "");

            Debug.WriteLine("FINAL DATA " + res, DebugTag);

        }

        //----------------------------------------------------------------------------
        // Method Name   : BT_Disconnect
        // Input         : NA
        // Output        : status of DisConnect Device form of bool
        // Purpose       : Disconnect a BT dongle having a particular handle
        // Date          : 20-08-20
        //---------------------------------------------------------------------------
        public bool BT_Disconnect()
        {
            Debug.WriteLine("DISCONNECT BLUETOOTH DEVICE", DebugTag);
            if (BthSocket != null)
                BthSocket.Close();
            device = null;
            BluetoothAdapter = null;

            return true;
        }

        private async Task<string> SendCommand(string command, Action<string> onDataRecevied)
        {
            try
            {
                command = command + "\r";

                if (Platform == Platform.Android && Connectivity == Connectivity.Bluetooth)
                {
                    Debug.WriteLine("SENDING COMMAND " + command, DebugTag);
                    if (BthSocket != null)
                    {
                        StringBuilder collectiveResponse = new StringBuilder();
                        var datastream = BthSocket.OutputStream;
                        byte[] byteArray = Encoding.ASCII.GetBytes(command);
                        await BthSocket.OutputStream.FlushAsync();
                        await BthSocket.OutputStream.WriteAsync(byteArray, 0, byteArray.Length);
                        var response = ReadAgain();
                        Debug.WriteLine("------Dongle_Response------ = " + response, DebugTag);
                        collectiveResponse.Append(response);
                        //Debug.WriteLine("------Collective_Response------ = " + collectiveResponse.ToString(), DebugTag);
                        while (!response.EndsWith(">"))
                        {
                            response = ReadAgain();
                            if(response.Contains("bt socket closed, read return:"))
                            {
                                response += ">";
                            }
                            else
                            {
                                Debug.WriteLine("------ReadAgain_Response------ = " + response, DebugTag);
                                collectiveResponse.Append(response);
                            }
                            // Debug.WriteLine("------Collective_Response------ = " + collectiveResponse.ToString(), DebugTag);
                        }

                        var responseString = collectiveResponse.ToString();

                        Debug.WriteLine("------ResponseString------ = " + responseString.ToString(), DebugTag);
                        responseString = responseString.Replace(command, "");

                        char[] charsToTrim = { '\r', '>', '?', '\r' };
                        string actualString = responseString.Trim(charsToTrim);
                        Debug.WriteLine("------ActualString------ = " + actualString.ToString(), DebugTag);
                        return actualString;


                    }

                }
                if (Platform == Platform.Android && Connectivity == Connectivity.USB)
                {
                    Debug.WriteLine("Command USB Send =  " + command, DebugTag);
                    if (SerialInputOutputManager.IsOpen)
                    {
                        var byteRes = new byte[1024];
                        StringBuilder collectiveResponse = new StringBuilder();
                        byte[] byteArray = Encoding.ASCII.GetBytes(command);
                        USBport.Write(byteArray, 100);
                        var response = await GetUSBCommand();

                        Debug.WriteLine("------Dongle_Response------ = " + response, DebugTag);
                        collectiveResponse.Append(response);
                        //Debug.WriteLine("------Collective_Response------ = " + collectiveResponse.ToString(), DebugTag);
                        while (!response.EndsWith(">"))
                        {
                            response = await GetUSBCommand();
                            Debug.WriteLine("------ReadAgain_Response------ = " + response, DebugTag);
                            collectiveResponse.Append(response);
                            // Debug.WriteLine("------Collective_Response------ = " + collectiveResponse.ToString(), DebugTag);
                        }

                        var responseString = collectiveResponse.ToString();

                        Debug.WriteLine("------ResponseString------ = " + responseString.ToString(), DebugTag);
                        responseString = responseString.Replace(command, "");

                        char[] charsToTrim = { '\r', '>', '?', '\r' };
                        string actualString = responseString.Trim(charsToTrim);
                        Debug.WriteLine("------ActualString------ = " + actualString.ToString(), DebugTag);
                        return actualString;

                    }
                }

                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private async Task<string> ReadAgainData()
        {
            if (Platform == Platform.Android && Connectivity == Connectivity.Bluetooth)
            {
                Debug.WriteLine("-----------------------Read Again Data------------------ ", DebugTag);
                if (BthSocket != null)
                {
                    StringBuilder collectiveResponse = new StringBuilder();

                    var response = ReadAgain();
                    Debug.WriteLine("-------Read Again Dongle_Response------ = " + response, DebugTag);
                    collectiveResponse.Append(response);
                    //Debug.WriteLine("------Collective_Response------ = " + collectiveResponse.ToString(), DebugTag);
                    while (!response.EndsWith(">"))
                    {
                        response = ReadAgain();
                        Debug.WriteLine("-------Read Again ReadAgain_Response------ = " + response, DebugTag);
                        collectiveResponse.Append(response);
                        // Debug.WriteLine("------Collective_Response------ = " + collectiveResponse.ToString(), DebugTag);
                    }

                    var responseString = collectiveResponse.ToString();

                    Debug.WriteLine("-------Read Again ResponseString------ = " + responseString.ToString(), DebugTag);
                    responseString = responseString.Replace(" ", "");

                    char[] charsToTrim = { '\r', '>', '?', '\r' };
                    string actualString = responseString.Trim(charsToTrim);
                    Debug.WriteLine("------Read Again ActualString------ = " + actualString.ToString(), DebugTag);
                    return actualString;


                }

            }
            return string.Empty;

        }

        public async Task<string> GetUSBCommand()
        {
            byte[] rbuffer = new byte[1024];
            Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

            var len = USBport.Read(rbuffer, 200);
            if (len > 0)
            {
                byte[] RetArray = new byte[len];
                Array.Copy(rbuffer, RetArray, len);
                Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);
                string ReceivedMessage = ByteArrayToString(RetArray);
                string ReceivedMessage2 = System.Text.Encoding.ASCII.GetString(RetArray);
                return ReceivedMessage2;

            }

            return null;
        }
        private async Task<string> ReadAnswer()
        {
            try
            {
                rawData = "";
                int a = 0;

                System.Text.StringBuilder b = new System.Text.StringBuilder();
                char c;
                var mReader = new InputStreamReader(BthSocket.InputStream);
                var buffer = new BufferedReader(mReader);

                int readByte = await mReader.ReadAsync();

                //RetArray = new byte[readByte];
                //Array.Copy(rbuffer, 0, RetArray, 0, readByte);



                if (buffer.Ready())
                {
                    // I read...
                    char[] chr = new char[100];
                    var data = await buffer.ReadAsync(chr);

                }




                rawData = b.ToString();
                Log.Info("-----------------------------------", "RawData: " + rawData);
                BthSocket.InputStream.Flush();
                return rawData;
            }
            catch (System.Exception e)
            {
                Log.Info("", "" + e.Message);
                return "Exception";
            }
        }

        //----------------------------------------------------------------------------
        // Method Name   : BT_GetDevices
        // Input         : NA
        // Output        : Collection of Bluetoothdevices with their status,name, address.
        // Purpose       : Get list of all devices with a specific BT/BLE name and with status paired and unpaired
        // Date          : 20-08-20
        //----------------------------------------------------------------------------

        public ObservableCollection<BluetoothDevices> BT_GetDevices()
        {
            ObservableCollection<BluetoothDevices> pairedDeviceList = new ObservableCollection<BluetoothDevices>();
            foreach (var pairedDevice in BluetoothAdapter.BondedDevices)
            {
                System.Diagnostics.Debug.WriteLine("Paired devices found: Name :" + pairedDevice.Name);
                System.Diagnostics.Debug.WriteLine("Paired devices found: Address :" + pairedDevice.Address);

                var newDevice = new BluetoothDevices()
                {
                    DeviceAddress = pairedDevice.Address,
                    DeviceName = pairedDevice.Name,
                    Status = pairedDevice.BondState.ToString()
                };

                pairedDeviceList.Add(newDevice);
            }
            return pairedDeviceList;
        }

        private void WriteConsole(string input, string output)
        {
            Debug.WriteLine("Command = " + input + "\n" + "Output = " + output, DebugTag);
        }


        //----------------------------------------------------------------------------
        // Method Name   : Dongle_Reset
        // Input         : NA
        // Output        : object
        // Purpose       : Reset all vehicle Communication related parameters to its default value
        // Date          : 20-08-20
        //----------------------------------------------------------------------------
        public async Task<object> Dongle_Reset()
        {
            Debug.WriteLine("------Inside Dongle_Reset------", DebugTag);
            //var response1 = await SendRequest("ATE0");
            var response1 = await SendCommand("ATE0", (obj) => { WriteConsole("ATE0", obj); });//(no echo)
            var response2 = await SendCommand("ATL0", (obj) => { WriteConsole("ATL0", obj); });//(No linefeeds)
            var response3 = await SendCommand("ATZ", (obj) => { WriteConsole("ATZ", obj); });//(reset)
            var response4 = await SendCommand("ATI", (obj) => { WriteConsole("ATI", obj); });//(Get version)
            var response5 = await SendCommand("ATAL", (obj) => { WriteConsole("ATAL", obj); });//(allow > 7 byte messages)
            var response6 = await SendCommand("ATS0", (obj) => { WriteConsole("ATS0", obj); });//(printing spaces off)
            var response7 = await SendCommand("ATCFC1", (obj) => { WriteConsole("ATCFC1", obj); });//(allow flow control)"
            return response1 + "  " + response2 + "  " + response3 + "  " + response4 + "  " + response5 + "  " + response6 + "  " + response7;
        }

        //----------------------------------------------------------------------------
        // Method Name   : Dongle_SetProtocol
        // Input         : NA
        // Output        : object
        // Purpose       : Set Vehicle Communication Protocol
        // Date          : 20-08-20
        //----------------------------------------------------------------------------
        public async Task<object> Dongle_SetProtocol(string protocolEnum)
        {
            Debug.WriteLine("------Dongle_SetProtocol------", DebugTag);
            //ATSPx
            var command = "ATSP" + protocolEnum;
            object response = null;
            var response1 = await SendCommand(command, (obj) => { WriteConsole("ATSPx", obj); response = obj; });//(set protocol)
            return response1;
        }

        public async Task<object> Dongle_SetCan()
        {
            Debug.WriteLine("------Dongle_SetCan------", DebugTag);
            //ATSPx
            var command = "0100";// + ((int)protocolEnum).ToString();
            object response = null;
            var response1 = await SendCommand(command, (obj) => { WriteConsole("ATSPx", obj); response = obj; });//(set protocol)
            return response1;
        }

        public async Task<object> Dongle_FindProtocol()
        {
            Debug.WriteLine("------Dongle_SetCan------", DebugTag);
            //ATSPx
            var command = "atdp";// + ((int)protocolEnum).ToString();
            object response = null;
            var response1 = await SendCommand(command, (obj) => { WriteConsole("ATSPx", obj); response = obj; });//(set protocol)
            return response1;
        }

        public async Task<object> Dongle_Find(ProtocolEnum protocolEnum)
        {
            Debug.WriteLine("------Dongle_FindProtocol------", DebugTag);
            //ATSPx
            var command = "ATSP0" + ((int)protocolEnum).ToString();
            object response = null;
            var response1 = await SendCommand(command, (obj) => { WriteConsole("ATSPx", obj); response = obj; });//(set protocol)
            return response1;
        }

        //----------------------------------------------------------------------------
        // Method Name   : Dongle_SetProtocol
        // Input         : NA
        // Output        : object
        // Purpose       : Set Vehicle Communication Protocol
        // Date          : 20-08-20
        //----------------------------------------------------------------------------
        public async Task<ResponseArrayStatus> CAN_TxRx(int frameLength, string txrx)
        {
            try
            {

                Debug.WriteLine("------CAN_TxRxData------", DebugTag);
                //ATSPx
                var command = txrx;
                object response = null;

                
                
                response = await SendCommand(command, (obj) => { WriteConsole(command, obj); response = obj; });//(set protocol)

                var actualResponse = ExtractResponse(response);

                if (actualResponse.Contains(" ") || (actualResponse == "NODATA") || (actualResponse == "CANERROR"))
                {
                    responseStructure = new ResponseArrayStatus
                    {
                        ECUResponse = null,
                        ECUResponseStatus = "ECU_COMMUNICATION_ERROR",
                        ActualDataBytes = null
                    };
                }
                else
                {
                    if (string.IsNullOrEmpty(actualResponse))
                    {
                        responseStructure = new ResponseArrayStatus
                        {
                            ECUResponse = null,
                            ECUResponseStatus = (string)response,
                            ActualDataBytes = null
                        };
                    }
                    else
                    {
                        var ar = HexToBytes(actualResponse);
                        var ar1 = HexStringToByteArray(actualResponse);
                        ResponseArrayDecoding.CheckResponse(ar, out byte[] actualDataBytes, out string dataStatus);
                        //ResponseArrayDecoding.CheckResponse(HexStringToByteArray(actualResponse), out byte[] actualDataBytes, out string dataStatus);
                        if (dataStatus == "READAGAIN")
                        {
                            while (dataStatus == "READAGAIN")
                            {
                                var readAgainResponse = ReadAgainData();
                                var readAgainActualResponse = ExtractResponse(response);
                                ResponseArrayDecoding.CheckResponse(HexStringToByteArray(readAgainActualResponse), out byte[] actualDataBytes2, out string dataStatus2);
                                dataStatus = dataStatus2;
                                responseStructure = new ResponseArrayStatus
                                {
                                    ECUResponse = HexStringToByteArray(readAgainActualResponse),
                                    ECUResponseStatus = dataStatus,
                                    ActualDataBytes = HexStringToByteArray(readAgainActualResponse)
                                };
                            }
                        }
                        else
                        {
                            responseStructure = new ResponseArrayStatus
                            {
                                ECUResponse = HexStringToByteArray(actualResponse),
                                ECUResponseStatus = dataStatus,
                                ActualDataBytes = actualDataBytes
                            };
                        }
                    }
                }


                Debug.WriteLine("------ECU RESPONE START ------", DebugTag);
                if (responseStructure?.ECUResponse != null)
                    Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(responseStructure?.ECUResponse), DebugTag);
                if (responseStructure?.ActualDataBytes != null)
                    Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
                Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
                Debug.WriteLine("------ECU RESPONE END ------", DebugTag);

                return responseStructure;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        #region Helper


        private byte[] HexStringToByteArray(String hex)
        {
            try
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
                //byte[] bytes = Encoding.ASCII.GetBytes(hex);
                return bytes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        static byte[] HexToBytes(string input)
        {
            try
            {
                byte[] result = new byte[input.Length / 2];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = Convert.ToByte(input.Substring(2 * i, 2), 16);
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //----------------------------------------------------------------------------
        // Method Name   : BT_GetDevices
        // Input         : NA
        // Output        : Collection of Bluetoothdevices with their status,name, address.
        // Purpose       : Get list of all devices with a specific BT/BLE name and with status paired and unpaired
        // Date          : 20-08-20
        //----------------------------------------------------------------------------

        #endregion

        private string ExtractResponse(object CAN_TxRxData)
        {
            string responseValue = CAN_TxRxData.ToString();
            string responseString = responseValue;
            responseValue = responseValue.Trim('\r');
            string[] responseArray = responseValue.Split('\r');
            string output = string.Empty;

            //if (responseArray.Length > 1)
            //{

            //responseString = string.Empty; ;
            //responseArray = responseArray.Skip(0).ToArray();
            //string[] tempArray = new string[responseArray.Length - 1];

            //Array.Copy(responseArray, 1, tempArray, 0, responseArray.Length - 1);
            //responseArray = tempArray;




            //int counter = 0;
            UInt16 framelen = 0;
            int consframelen = 0;
            int totalbytescopied = 0;
            foreach (string item in responseArray)
            {
                if (item.Contains("SEARCHING"))
                {
                    /* Do Nothing */
                }
                else if (item.Length == 4) // this is the length of the multiple frame which needs to be neglected. store the length
                {
                    framelen = UInt16.Parse(item, System.Globalization.NumberStyles.HexNumber);
                    //framelen = Convert.ToUInt16(item);
                }
                else
                {
                    if (item.Contains(':') == true) // the line is a part of the multi frame message
                    {
                        string[] abd = item.Split(':');
                        abd[1] = abd[1].Replace(" ", String.Empty);

                        consframelen = abd[1].Length;
                        string tempstr = string.Empty;

                        tempstr = abd[1].Substring(0, Math.Min(consframelen, (framelen - totalbytescopied) * 2));

                        totalbytescopied += abd[1].Length / 2;

                        output += tempstr;

                    }
                    else
                    {
                        output += item; // this is a single frame message
                    }

                }
            }

            //    string specialChar = counter.ToString() + ":";
            //    if (responseArray[counter].Contains(specialChar))
            //    {
            //        responseString += responseArray[counter].Replace(specialChar, "");

            //    }
            //    else
            //    {
            //        responseString += responseArray[counter];
            //    }
            //}
            //counter++;
            //}

            //}
            output = output.Replace(" ", String.Empty);

            return output;
        }

        //----------------------------------------------------------------------------
        // Method Name   : Dongle_GetProtocol
        // Input         : NA
        // Output        : object
        // Purpose       : Get Vehicle Communication Protocol
        // Date          : 20-08-20
        //----------------------------------------------------------------------------
        public async Task<object> Dongle_GetProtocol()
        {
            Debug.WriteLine("------Dongle_GetProtocol------", DebugTag);
            object response = null;
            var response1 = await SendCommand("ATDP", (obj) => { WriteConsole("ATDP", obj); response = obj; });//(Describe current protocol)

            object response2 = null;
            var response3 = await SendCommand("ATDPN", (obj) => { WriteConsole("ATDPN", obj); response2 = obj; });//(describe current protocol by no)

            return response1 + "--" + response3;
        }

        //----------------------------------------------------------------------------
        // Method Name   : Dongle_GetFimrwareVersion
        // Input         : NA
        // Output        : object
        // Purpose       : Get firmware version
        // Date          : 20-08-20
        //----------------------------------------------------------------------------
        public async Task<object> Dongle_GetFimrwareVersion()
        {
            Debug.WriteLine("------Dongle_GetFimrwareVersion------", DebugTag);
            object response = null;
            var response1 = await SendCommand("ATI", (obj) => { WriteConsole("ATI", obj); response = obj; });//(get version)

            return response1;

        }

        //----------------------------------------------------------------------------
        // Method Name   : Dongle_ReadVoltageLevels
        // Input         : NA
        // Output        : object
        // Purpose       : Read Battery and ignition voltage levels
        // Date          : 20-08-20
        //----------------------------------------------------------------------------
        public async Task<object> Dongle_ReadVoltageLevels()
        {
            Debug.WriteLine("------Dongle_ReadVoltageLevels------", DebugTag);
            object response = null;
            var response1 = await SendCommand("ATRV", (obj) => { WriteConsole("ATRV", obj); response = obj; });//(get batt voltage)

            object response2 = null;
            var response3 = await SendCommand("ATIGN", (obj) => { WriteConsole("ATIGN", obj); response2 = obj; });//(get ign voltage)
            return response1 + "-" + response3;
        }

        public async Task<object> CAN_SetTxHeader(string setTxHeader)
        {
            Debug.WriteLine("------CAN_SetTxHeader------", DebugTag);
            var command = "ATSH";
            if (setTxHeader.Length <= 4)
            {
                command = command + setTxHeader.Substring(1, 3);
                object response = null;
                var response1 = await SendCommand(command, (obj) => { WriteConsole("ATSHxxx", obj); response = obj; });//(if protocol is 11 bit)
                return response1;
            }
            else
            {

                command = command + setTxHeader.Substring(2, 6);
                object response = null;
                var response1 = await SendCommand(command, (obj) => { WriteConsole("ATSHxxx", obj); response = obj; });//(if protocol is 11 bit)

                command = "ATCP" + setTxHeader.Substring(0, 2);
                var response3 = await SendCommand(command, (obj) => { WriteConsole("ATSHxxx", obj); response = obj; });//(if protocol is 11 bit)

                return response1 + "-" + response3;
            }
            //ATSHxxx(if protocol is 11 bit)

            //pp qq rr ss
            //ATSHqqrrss(if protocol is 29 bit)
            //ATCPpp





            //object response2 = null;
            //var response3 = await SendCommand("ATSHxxxxxxxx", (obj) => { WriteConsole("ATSHxxxxxxxx", obj); response2 = obj; });//if protocol is 29 bit)

        }

        public async Task<object> CAN_SetRxHeaderMask(string setRxHeader)
        {
            Debug.WriteLine("------CAN_SetRxHeaderMask------", DebugTag);
            var command = "ATCF";

            //command = command + setRxHeader.Substring(1,3);
            //object response = null;
            //var response1 = await SendCommand(command, (obj) => { WriteConsole("ATCFhhh", obj); response = obj; });//(if protocol is 11 bit)


            ////object response2 = null;
            ////var response3 = await SendCommand("ATCFhhhhhhhh", (obj) => { WriteConsole("ATCFhhhhhhhh", obj); response2 = obj; });//if protocol is 29 bit)
            //return response1;
            string response1;

            if (setRxHeader.Length <= 4)
            {
                command = command + setRxHeader.Substring(1, 3);
                object response = null;
                response1 = await SendCommand(command, (obj) => { WriteConsole("ATCFxxx", obj); response = obj; });//(if protocol is 11 bit)
                return response1;
            }
            else
            {

                command = "ATCF" + setRxHeader;
                var response3 = await SendCommand(command, (obj) => { WriteConsole("ATCFxxx", obj); response1 = obj; });//(if protocol is 11 bit)

                return response3;
            }

        }

        public async Task<object> ISOK_SetHeader(string setTxHeader, string setRxHeader)
        {
            Debug.WriteLine("------ISOK_SetTxHeader------", DebugTag);
            var command = "ATSH";
            command = command + 82 + setRxHeader + setTxHeader;
            object response = null;
            var response1 = await SendCommand(command, (obj) => { WriteConsole("ATSHxxx", obj); response = obj; });

            return response1;

        }

        public async Task<object> CAN_StartPadding()
        {
            Debug.WriteLine("------CAN_StartPadding------", DebugTag);
            object response = null;
            var response1 = await SendCommand("ATCAF1", (obj) => { WriteConsole("ATCAF1", obj); response = obj; });//(if protocol is 11 bit)

            return response1;

        }

        public async Task<object> CAN_StopPadding()
        {
            Debug.WriteLine("------CAN_StopPadding------", DebugTag);
            object response = null;
            var response1 = await SendCommand("ATCAF0", (obj) => { WriteConsole("ATCAF0", obj); response = obj; });//(if protocol is 11 bit)

            return response1;

        }

        public async Task<object> CAN_TxData()
        {
            Debug.WriteLine("------CAN_TxData------", DebugTag);
            throw new NotImplementedException();
        }

        public async Task<object> CAN_RxData()
        {
            Debug.WriteLine("------CAN_RxData------", DebugTag);
            throw new NotImplementedException();
        }

        bool IBluetoothHandler.BT_ConnectDevice(string deviceName)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
