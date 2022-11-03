using Android.Bluetooth;
using APDongleCommAndroid.Helper;
using APDongleCommAnroid;
using APDongleCommAnroid.ENUMS;
using APDongleCommAnroid.Helper;
using APDongleCommAnroid.Models;
using DotNetSta;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APDongleCommAndroid
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


    public class DongleCommWin : ICANCommands, IWifiUSBHandler, IDongleHandler
    {
        #region Properties
        private TaskCompletionSource<object> tskCmSsrc = null;
        private Protocol protocol;
        private string DebugTag = "ELM-DEBUG";
        private ResponseArrayStatus responseStructure;

        BluetoothSocket BluetoothSocket = null;
        SerialInputOutputManager SerialInputOutputManager = null;
        UsbSerialPort USBport = null;
        TcpClient TcpClient = null;
        TcpClient client = null;

        public Platform Platform = Platform.None;
        public Connectivity Connectivity = Connectivity.None;
        private NetworkStream Stream = null;

        Vocom1210 class1;

        #endregion

        #region Ctor
        public DongleCommWin()
        {
            Debug.WriteLine("Inside DongleComm", DebugTag);
            class1 = new Vocom1210();
        }
        public void InitializePlatform(Platform platform, Connectivity connectivity)
        {
            Platform = platform;
            Connectivity = connectivity;
        }
        public DongleCommWin(BluetoothSocket socket, int protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
        {
            Debug.WriteLine("Inside BluetoothSocket CTOR", DebugTag);
            BluetoothSocket = socket;
            protocol = (Protocol)protocolVersion;
        }

        public DongleCommWin(TcpClient client, NetworkStream networkStream, int protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
        {
            Debug.WriteLine("Inside ELM327 TcpClient CTOR", DebugTag);
            TcpClient = client;
            protocol = (Protocol)protocolVersion;
            Stream = networkStream;
        }

        public DongleCommWin(TcpClient client, NetworkStream networkStream, string protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
        {
            Debug.WriteLine("Inside ELM327 TcpClient CTOR", DebugTag);
            TcpClient = client;
            //protocol = protocolVersion;
            Stream = networkStream;
        }

        public DongleCommWin(SerialInputOutputManager serialInputOutputManager, UsbSerialPort port, int protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
        {
            Debug.WriteLine("Inside ELM327 SimpleTcpClient CTOR", DebugTag);
            SerialInputOutputManager = serialInputOutputManager;
            USBport = port;
            protocol = (Protocol)protocolVersion;

            if (tskCmSsrc == null)
            {
                tskCmSsrc = new TaskCompletionSource<object>();
            }

            //SerialInputOutputManager.DataReceived += (sender, e) =>
            //{
            //    tskCmSsrc?.TrySetResult(e.Data);
            //    Debug.WriteLine("SerialInputOutputManager.DataReceived"+ ByteArrayToString(e.Data), DebugTag);

            //};
        }

        public DongleCommWin(BluetoothSocket socket, string _protocol, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
        {
            Debug.WriteLine("Inside BluetoothSocket CTOR", DebugTag);
            BluetoothSocket = socket;
            protocol = (Protocol)Enum.Parse(typeof(Protocol), _protocol); ;
            //this.txHeader = txHeader;
        }
        #endregion

        #region Methods

        


        #region SendCommand
        public async Task<object> ReadData()
        {
            Debug.WriteLine("------Read Again Data------", DebugTag);
            if (Platform == Platform.Android && Connectivity == Connectivity.Bluetooth)
            {
                return await GetBTCommand();
            }
            if (Platform == Platform.Android && Connectivity == Connectivity.USB)
            {

                if (SerialInputOutputManager.IsOpen)
                {
                    return await GetUSBCommand();
                }
            }
            if (Platform == Platform.Android && Connectivity == Connectivity.WiFi)
            {
                return await GetWifiCommand();
            }
            if (Platform == Platform.Android && Connectivity == Connectivity.RP1210)
            {
                return await GetRP1210Command();
            }
            Debug.WriteLine("------END Read Again Data------", DebugTag);
            return null;
        }
        public async Task<object> SendCommand(string randomCommand)
        {
            Debug.WriteLine("------SendCommand------", DebugTag);
            object response = null;

            string command = randomCommand.ToString();
            var bytesCommand = HexStringToByteArray(command);

            byte[] sendBytes = HexStringToByteArray(command);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); }); ;

            return response;

        }

        public async Task<object> SendCommand(byte[] command, Action<string> onDataRecevied)
        {
            try
            {
                if (Platform == Platform.Android && Connectivity == Connectivity.Bluetooth)
                {
                    try
                    {
                        Debug.WriteLine("Command BT Send =  " + ByteArrayToString(command), DebugTag);
                        Debug.WriteLine("--------- BT Command Send TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                        //var t = Task.Run(() => 
                        //{
                        //    var data = BluetoothSocket.OutputStream.WriteAsync(command, 0, command.Length);
                        //    BluetoothSocket.OutputStream.Flush();
                        //    if (data.IsCompleted)
                        //    {
                        //        Debug.WriteLine("Command : send success", DebugTag);
                        //    }
                        //    else if (data.IsFaulted)
                        //    {
                        //        Debug.WriteLine("Command : send failed", DebugTag);
                        //    }
                        //    else if (data.IsCanceled)
                        //    {
                        //        Debug.WriteLine("Command : send cancelled", DebugTag);
                        //    }
                        //    else if (data.IsCompletedSuccessfully)
                        //    {
                        //        Debug.WriteLine("Command : send successfully", DebugTag);
                        //    }
                        //    else
                        //    {
                        //        Debug.WriteLine("Command : send other issue", DebugTag);
                        //    }
                        //    Debug.WriteLine($"Command : send status : {data.Status.ToString()}", DebugTag);
                        //});
                        //t.Wait();

                        //if (t.IsCompleted)
                        //{
                        //    Debug.WriteLine("TCommand : send success", DebugTag);
                        //}
                        //else if (t.IsFaulted)
                        //{
                        //    Debug.WriteLine("TCommand : send failed", DebugTag);
                        //}
                        //else if (t.IsCanceled)
                        //{
                        //    Debug.WriteLine("TCommand : send cancelled", DebugTag);
                        //}
                        //else if (t.IsCompletedSuccessfully)
                        //{
                        //    Debug.WriteLine("TCommand : send successfully", DebugTag);
                        //}
                        //else
                        //{
                        //    Debug.WriteLine("TCommand : send other issue", DebugTag);
                        //}
                        //Debug.WriteLine($"TCommand : send status : {t.Status.ToString()}", DebugTag);

                        BluetoothSocket.OutputStream.Write(command, 0, command.Length);
                        BluetoothSocket.OutputStream.Flush();

                        //BluetoothSocket.OutputStream.Close();
                        //Thread.Sleep(500);
                        var response = await GetBTCommand();
                        Debug.WriteLine("Command BT RESPONSE =  " + ByteArrayToString(response), DebugTag);
                        Debug.WriteLine("--------- BT Command RESPONSE TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                        return response;
                    }
                    catch (AggregateException ex)
                    {
                        Debug.WriteLine($"Command : send AggregateException", DebugTag);
                    }
                }
                else if (Platform == Platform.Android && Connectivity == Connectivity.USB)
                {
                    Debug.WriteLine("Command USB Send =  " + ByteArrayToString(command), DebugTag);
                    Debug.WriteLine("--------- USB Command Send TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                    if (SerialInputOutputManager.IsOpen)
                    {
                        //var byteRes = new byte[1024];
                        USBport.Write(command, 0);//TBD CHECK
                        var response = await GetUSBCommand();
                        if (response != null)
                        {
                            var byteResponse = (byte[])response;
                            Debug.WriteLine("Command USB RESPONSE =  " + ByteArrayToString(byteResponse), DebugTag);
                            Debug.WriteLine("--------- USB Command RESPONSE TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                            return response;
                        }
                    }
                }
                else if (Platform == Platform.Android && Connectivity == Connectivity.WiFi)
                {
                    #region mainCode
                    Debug.WriteLine("Command WiFi Send =  " + ByteArrayToString(command), DebugTag);

                    //00 000e 00000000 00000000 500c47568afe56214e238000ffc3 ce2c

                    //00 - Tx sequence counter
                    //000e - length of the command(command length + 2)
                    //00000000 - Timestamp - hhmmssmm
                    //00000000 - Reserved - always 00
                    //500c47568afe56214e238000ffc3 - actual command - payload
                    //ce2c - crc of entire length from tx counter to the payload.

                    var byte1 = "00";
                    var byte2 = (command.Length + 10).ToString("X4");
                    var byte3 = DateTime.Now.ToString("hhmmssff");
                    var byte4 = DateTime.Now.ToString("00000000");
                    var byte5 = ByteArrayToString(command);
                    var dataBytes = HexStringToByteArray(byte1 + byte2 + byte3 + byte4 + byte5);
                    var byte6Checksum = Crc16CcittKermit.ComputeChecksum(dataBytes);

                    var bytes = ByteArrayToString(dataBytes) + byte6Checksum.ToString("X4");
                    var byteData = HexStringToByteArray(bytes);
                    Stream = TcpClient.GetStream();
                    Debug.WriteLine("---------Sending Command " + ByteArrayToString(byteData) + "-------" + "--WRITE TIME--" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff"), DebugTag);
                    Stream.Write(byteData, 0, byteData.Length); // send with wifi headers
                    //Stream.Write(command, 0, command.Length); // send without wifi headers
                    Debug.WriteLine("---------Sent Command " + byteData + "-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff"), DebugTag);

                    byte[] WifiComandValue = await GetWifiCommand();

                    return WifiComandValue;
                    #endregion

                    #region trimmed
                    //Debug.WriteLine("Command WiFi Send =  " + ByteArrayToString(command), DebugTag);

                    //Stream = TcpClient.GetStream();
                    //Debug.WriteLine("---------Sending Command " + ByteArrayToString(command) + "-------" + "--WRITE TIME--" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff"), DebugTag);
                    //Stream.Write(command, 0, command.Length); // send with wifi headers
                    //                                          //Stream.Write(command, 0, command.Length); // send without wifi headers
                    //Debug.WriteLine("---------Sent Command " + command + "-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff"), DebugTag);

                    //byte[] WifiComandValue = await GetWifiCommand();

                    //return WifiComandValue; 
                    #endregion

                }
                else if (Platform == Platform.Android && Connectivity == Connectivity.RP1210)
                {
                    try
                    {
                        var fw_version = class1.SendMessaage(command);
                        Debug.WriteLine(fw_version, DebugTag);
                        byte[] ActualBytes = class1.ReadMessage();
                        var byteData = ByteArrayToString(ActualBytes);
                        Debug.WriteLine(byteData, DebugTag);
                        return ActualBytes;
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public async Task<byte[]> GetWifiCommand()
        {
            #region mainCode
            byte[] ActualBytes = new byte[] { };
            try
            {
                byte[] rbuffer = new byte[4500];
                byte[] RetArray = new byte[] { };


                Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                Stream.ReadTimeout = 5000;
                int readByte = Stream.Read(rbuffer, 0, rbuffer.Length);
                Debug.WriteLine("--------- Read Byte Lenth-------" + readByte, DebugTag);
                RetArray = new byte[readByte];
                Array.Copy(rbuffer, 0, RetArray, 0, readByte);

                Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);

                UInt16 wifistartindex = 0;
                UInt16 actualdatastartindex = 0; // first index where to read the actual data from after wifi header
                UInt16 decValue = 0;
                UInt16 updateddecValue = 0;

                while (wifistartindex < readByte)
                {
                    var byteLength = RetArray.Length.ToString("d4");
                    string hexValue = RetArray[wifistartindex + 1].ToString() + RetArray[wifistartindex + 2].ToString();
                    decValue = (UInt16)((((UInt16)RetArray[wifistartindex + 1]) << 8) + ((UInt16)RetArray[wifistartindex + 2]));

                    updateddecValue += decValue;

                    Array.Resize(ref ActualBytes, updateddecValue);
                    Array.Copy(RetArray, wifistartindex + 11, ActualBytes, actualdatastartindex, decValue);
                    wifistartindex += (UInt16)(13 + decValue);
                    actualdatastartindex += decValue;
                }

                Debug.WriteLine("--------- DATA RESPONSE-------" + ByteArrayToString(ActualBytes), DebugTag);
                return ActualBytes;
                //return RetArray;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unable to read data from the transport connection"))
                {
                    ActualBytes = Encoding.ASCII.GetBytes("Dongle disconnected"); ;
                }
                Debug.WriteLine("--------- ERROR EXCEPTION -------" + ex.Message + "-----" + ex.StackTrace, DebugTag);
                return ActualBytes;
            }
            #endregion

            #region Trimmed
            //try
            //{
            //    byte[] rbuffer = new byte[1024];
            //    byte[] RetArray = new byte[] { };
            //    byte[] ActualBytes = new byte[] { };

            //    Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
            //    //int readByte = await Stream.ReadAsync(rbuffer, 0, rbuffer.Length);

            //    //var task = Task.Run(async () =>
            //    //{
            //        int readByte = await Stream.ReadAsync(rbuffer, 0, rbuffer.Length);
            //        RetArray = new byte[readByte];
            //        Array.Copy(rbuffer, 0, RetArray, 0, readByte);
            //    //});
            //    //if (task.Wait(TimeSpan.FromMilliseconds(6200)))
            //    //    return RetArray;
            //    //else
            //    //{
            //    //    Console.WriteLine("Method Timed out");
            //    //    return null;
            //    //    //throw new Exception("Timed out");
            //    //}


            //    Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);

            //    return RetArray;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.StackTrace);
            //    return null;
            //} 
            #endregion

        }

        public async Task<byte[]> GetRP1210Command()
        {
            byte[] ActualBytes = class1.ReadAgainMessage();
            var byteData = ByteArrayToString(ActualBytes);
            Debug.WriteLine(byteData, DebugTag);

            var framelength = byteData.Length / 2;
            int dataLength = framelength + 2; //crc
            string command = string.Empty;

            var firstbyte = 0x40 | ((dataLength >> 8) & 0x0f);
            var secondbyte = dataLength & 0xff;

            command = firstbyte.ToString("X2") + secondbyte.ToString("X2") + byteData.ToString();
            var bytesCommand = HexStringToByteArray(command);
            var crcBytesComputation = HexStringToByteArray(byteData);
            string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("X2");
            if (crc.Length == 3)
            {
                crc = "0" + crc;
            }
            if (crc.Length == 2)
            {
                crc = "00" + crc;
            }
            if (crc.Length == 1)
            {
                crc = "000" + crc;
            }
            Debug.WriteLine("CRC =" + crc, DebugTag);
            byte[] sendBytes = HexStringToByteArray(command + crc);
            return sendBytes;
        }

        public async Task<byte[]> GetBTCommand()
        {
            try
            {
                byte[] rbuffer = new byte[4500];
                byte[] RetArray = new byte[] { };
                int readByte = 0;
                int packetsize = 0;

                Debug.WriteLine("---------BT INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                // Read data from the device
                while (!BluetoothSocket.InputStream.CanRead)
                {
                    Debug.WriteLine("------------------------------------------------");
                }

                BTREADAGAIN:

                //var timeout = TimeSpan.Parse("00:01:00");
                //var cancellationTokenSource = new CancellationTokenSource(timeout);
                //var cancellationToken = cancellationTokenSource.Token;

                //BluetoothSocket.InputStream.ReadTimeout = 500;// new TimeSpan(0, 0, 0, 0, 500);
                readByte += await BluetoothSocket.InputStream.ReadAsync(rbuffer, 0, rbuffer.Length);

                Debug.WriteLine($"--------- Response Byte {readByte}-------" + ByteArrayToString(RetArray) + "--READ TIME--" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff"), DebugTag);

                if ((readByte < 2) || (readByte == null))
                {
                    goto BTREADAGAIN;
                }
                else if (readByte >= 2)
                {
                    packetsize = ((rbuffer[0] & 0x0f) << 8) + rbuffer[1];
                    if (readByte < packetsize + 2)
                    {
                        goto BTREADAGAIN;
                    }
                }

                RetArray = new byte[readByte];
                Array.Copy(rbuffer, 0, RetArray, 0, readByte);

                Debug.WriteLine("--------- BT READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);
                // Debug.WriteLine("--------- BT READ DATA RESPONSE TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                return RetArray;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public async Task<byte[]> GetUSBCommand()
        {
            try
            {
                byte[] rbuffer = new byte[4500];


                Debug.WriteLine("---------USB INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                //var ss = USBport.Read(byteRes, 100);

                //var response = await tskCmSsrc.Task;

                // handle incoming data.
                byte[] RetArray = null;
                var len = USBport.Read(rbuffer, 0);//TBD CHECK
                if (len > 0)
                {
                    RetArray = new byte[len];
                    Array.Copy(rbuffer, RetArray, len);

                    //DataReceived.Raise(this, new SerialDataReceivedArgs(data));
                    Debug.WriteLine("---------USB READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);
                    // Debug.WriteLine("---------USB READ DATA RESPONSE TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                }
                return RetArray;
            }
            catch (Exception ex)
            {
                return null;
            }


        }
        public bool USB_Disconnect()
        {
            USBport.Close();
            return true;
        }
        public bool Wifi_Disconnect()
        {
            return true;
        }

        public async Task<ResponseArrayStatusivn> CAN_IVNRxFrame(string frameid)
        {
            ResponseArrayStatusivn frameresponse = new ResponseArrayStatusivn();
            try
            {


                byte repeatread = 1;
                var response = (dynamic)null;
                string response_str = string.Empty;

                //filter frame id -
                //var sethardhdrmskresp = await dongleCommWin.CAN_SetHardRxHeaderMask(frameid);
                var sendBytes = await CAN_SetHardRxHeaderMask(frameid);


                response = await SendCommand(sendBytes, (obj) => { WriteConsole(ByteArrayToString(sendBytes), obj); });
                var ecuResponseBytes = (byte[])response;
                ResponseArrayDecoding.CheckResponseIVN(ecuResponseBytes, sendBytes, "", out byte[] actualDataBytes, out string dataStatus);

                if (dataStatus == "READAGAIN")
                {
                    while (dataStatus == "READAGAIN")
                    {
                        var responseReadAgain = await ReadData();
                        var ecuResponseReadBytes = (byte[])responseReadAgain;
                        ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
                        dataStatus = dataReadStatus;

                        frameresponse = new ResponseArrayStatusivn
                        {
                            ECUResponseStatus = dataReadStatus,
                            ActualFrameBytes = actualReadBytes
                        };

                        Debug.WriteLine("------EXTRA READ DATA START ------", DebugTag);
                        if (frameresponse?.ActualFrameBytes != null)
                            Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseReadBytes), DebugTag);
                        if (frameresponse?.ActualFrameBytes != null)
                            Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(frameresponse?.ActualFrameBytes), DebugTag);
                        Debug.WriteLine("------ECUResponseStatus ------" + frameresponse.ECUResponseStatus, DebugTag);
                        Debug.WriteLine("------EXTRA READ DATA END ------", DebugTag);

                        if (frameresponse.ActualFrameBytes == null)
                        {
                            Debug.WriteLine("Command BT ACTUAL RESPONSE = NULL", DebugTag);
                        }
                        else
                        {
                            Debug.WriteLine("Command BT ACTUAL RESPONSE = " + ByteArrayToString(frameresponse.ActualFrameBytes), DebugTag);
                        }
                    }
                }
                else
                {
                    frameresponse = new ResponseArrayStatusivn
                    {
                        //Frame = frame,
                        //ECUResponse = ecuResponseBytes,
                        ECUResponseStatus = dataStatus,
                        ActualFrameBytes = actualDataBytes
                    };

                    //ResponeList.Add(ivnResponseArrayStatus);
                    if (frameresponse.ActualFrameBytes == null)
                    {
                        Debug.WriteLine("Command BT ACTUAL RESPONSE = NULL", DebugTag);
                    }
                    else
                    {
                        Debug.WriteLine("Command BT ACTUAL RESPONSE = " + ByteArrayToString(frameresponse.ActualFrameBytes), DebugTag);
                    }
                }
                return frameresponse;
            }
            catch (Exception ex)
            {
                frameresponse = new ResponseArrayStatusivn
                {
                    //Frame = frame,
                    //ECUResponse = ecuResponseBytes,
                    ECUResponseStatus = "NULL_ERROR",
                    ActualFrameBytes = null
                };
                return frameresponse;
            }


            ///
            //while (repeatread > 0) // check if we have the frame we desire in the message. if not read 5 times
            //{
            //    if (Platform == Platform.Android && Connectivity == Connectivity.Bluetooth)
            //    {
            //        response = await GetBTCommand();
            //    }

            //    if (Platform == Platform.Android && Connectivity == Connectivity.USB)
            //    {
            //        response = await GetUSBCommand();
            //    }

            //    if (Platform == Platform.Android && Connectivity == Connectivity.WiFi)
            //    {
            //        response = GetWifiCommand();
            //    }

            //    if (response == null)
            //    {
            //        repeatread--;
            //        //await Task.Delay(100);
            //    }
            //    else
            //    {
            //        //response_str = BitConverter.ToString(response);
            //        //response_str = response_str.Replace("-", "");

            //        //if (response_str.Contains("600E" + frameid) == false)
            //        //{
            //           repeatread--;
            //        //}
            //        //else
            //        //{
            //        //    break;
            //        //}

            //    }
            //}
            ////search for 600e followed by frame id in the response. 8 bytes following this would be the payload which has to be returned

            //if (response != null)
            //{
            //    //string frame_str = string.Empty;

            //    //if (response_str.Contains("600E" + frameid))
            //    //{
            //    //    string V = "600E" + frameid;
            //    //    string[] frame_str_bulk = response_str.Split(V);
            //    //    frame_str = frame_str_bulk[1].Substring(0, 16);
            //    //}

            //    //if (frame_str != "")
            //    //{
            //    //    byte[] payload = new byte[8];

            //    //    var HexToByte = HexStringToByteArray(frame_str);

            //    //    payload = HexToByte;

            //    //    //byte[] bytes = Encoding.ASCII.GetBytes(frame_str);
            //    //    //Array.Copy(bytes, 0, payload, 0, 8);

            //    //    //Array.Copy(response, 6, payload, 0, 8);

            //    //    //Array.Copy(HexToByte, 6, payload, 0, 8);

            //    //    frameresponse = new ResponseArrayStatusivn
            //    //    {
            //    //        ECUResponseStatus = "NOERROR",
            //    //        ActualFrameBytes = payload
            //    //    };
            //    //}
            //    //else
            //    //{
            //    //    frameresponse = new ResponseArrayStatusivn
            //    //    {
            //    //        ECUResponseStatus = "IVN_FRAME_NOTFOUND",
            //    //        ActualFrameBytes = response
            //    //    };
            //    //}



            //    frameresponse = new ResponseArrayStatusivn
            //    {
            //        ECUResponseStatus = "NOERROR",
            //        ActualFrameBytes = payload
            //    };
            //}
            //else
            //{
            //    frameresponse = new ResponseArrayStatusivn
            //    {
            //        ECUResponseStatus = "IVN_FRAME_NOTFOUND",
            //        ActualFrameBytes = response
            //    };
            //}


        }

        public async Task<byte[]> CAN_SetHardRxHeaderMask(string rxhdrmsk)
        {
            Debug.WriteLine("------CAN_SetHardRxHeaderMask------", DebugTag);
            object response = null;

            string command = string.Empty;

            

            string crc;

            if (rxhdrmsk.Length == 8)
            {
                command = "200720" + rxhdrmsk;

                var bytesCommand = HexStringToByteArray(command);
                crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4], bytesCommand[5], bytesCommand[6]).ToString("x2");
            }
            else
            {
                command = "200520" + rxhdrmsk;

                var bytesCommand = HexStringToByteArray(command);
                crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
            }
            if (crc.Length == 3)
                crc = "0" + crc;

            byte[] sendBytes = HexStringToByteArray(command + crc);
            // response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            // return response;
            return sendBytes;

        }


        public async Task<ObservableCollection<IvnResponseArrayStatus>> SetIvnFrame(List<string> FrameIDC)
        {
            ObservableCollection<IvnResponseArrayStatus> ResponeList = new ObservableCollection<IvnResponseArrayStatus>();
            IvnResponseArrayStatus ivnResponseArrayStatus;
            try
            {
                foreach (var frame in FrameIDC.ToList())
                {
                    Debug.WriteLine("------SET_IVN FRAME------", DebugTag);
                    object response = null;
                    //"0x20,0x02,0x12, 0x < xx > "
                    string command = string.Empty;
                    string crc = string.Empty;
                    if (frame.Length == 8)
                    {
                        command = "200720" + frame.ToString();
                        var bytesCommand = HexStringToByteArray(command);
                        crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4], bytesCommand[5], bytesCommand[6]).ToString("x2");
                        if (crc.Length == 3)
                            crc = "0" + crc;
                    }
                    else
                    {
                        command = "200520" + frame.ToString();
                        var bytesCommand = HexStringToByteArray(command);
                        crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
                        if (crc.Length == 3)
                            crc = "0" + crc;
                    }

                    byte[] sendBytes = HexStringToByteArray(command + crc);
                    response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
                    var ecuResponseBytes = (byte[])response;
                    ResponseArrayDecoding.CheckResponseIVN(ecuResponseBytes, sendBytes, "", out byte[] actualDataBytes, out string dataStatus);

                    if (dataStatus == "READAGAIN")
                    {
                        while (dataStatus == "READAGAIN")
                        {
                            var responseReadAgain = await ReadData();
                            var ecuResponseReadBytes = (byte[])responseReadAgain;
                            ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
                            dataStatus = dataReadStatus;
                            ivnResponseArrayStatus = new IvnResponseArrayStatus
                            {
                                Frame = frame,
                                ECUResponse = ecuResponseReadBytes,
                                ECUResponseStatus = dataReadStatus,
                                ActualDataBytes = actualReadBytes
                            };
                            ResponeList.Add(ivnResponseArrayStatus);
                            Debug.WriteLine("------EXTRA READ DATA START ------", DebugTag);
                            if (ivnResponseArrayStatus?.ECUResponse != null)
                                Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseReadBytes), DebugTag);
                            if (ivnResponseArrayStatus?.ActualDataBytes != null)
                                Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(ivnResponseArrayStatus?.ActualDataBytes), DebugTag);
                            Debug.WriteLine("------ECUResponseStatus ------" + ivnResponseArrayStatus.ECUResponseStatus, DebugTag);
                            Debug.WriteLine("------EXTRA READ DATA END ------", DebugTag);

                            if (ivnResponseArrayStatus.ActualDataBytes == null)
                            {
                                Debug.WriteLine("Command BT ACTUAL RESPONSE = NULL", DebugTag);
                            }
                            else
                            {
                                Debug.WriteLine("Command BT ACTUAL RESPONSE = " + ByteArrayToString(ivnResponseArrayStatus.ActualDataBytes), DebugTag);
                            }
                        }
                    }
                    else
                    {
                        ivnResponseArrayStatus = new IvnResponseArrayStatus
                        {
                            Frame = frame,
                            ECUResponse = ecuResponseBytes,
                            ECUResponseStatus = dataStatus,
                            ActualDataBytes = actualDataBytes
                        };

                        ResponeList.Add(ivnResponseArrayStatus);
                        if (ivnResponseArrayStatus.ActualDataBytes == null)
                        {
                            Debug.WriteLine("Command BT ACTUAL RESPONSE = NULL", DebugTag);
                        }
                        else
                        {
                            Debug.WriteLine("Command BT ACTUAL RESPONSE = " + ByteArrayToString(ivnResponseArrayStatus.ActualDataBytes), DebugTag);
                        }
                    }
                }

                return ResponeList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        #endregion

        #region Helper
        //Convert a string of hex digits (example: E1 FF 1B) to a byte array. 
        //The string containing the hex digits (with or without spaces)
        //Returns an array of bytes. </returns>
        //private byte[] HexStringToByteArray(string s)
        //{
        //    s = s.Replace(" ", "");
        //    byte[] buffer = new byte[s.Length / 2];
        //    for (int i = 0; i < s.Length; i += 2)
        //        buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
        //    return buffer;
        //}
        static byte[] HexToBytes(string input)
        {
            byte[] result = new byte[input.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(input.Substring(2 * i, 2), 16);
            }
            return result;
        }

        private void WriteConsole(string input, string output)
        {
            Debug.WriteLine("Command = " + input + "\n" + "Output = " + output, DebugTag);
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
        //----------------------------------------------------------------------------
        // Method Name   : ByteArrayToString
        // Input         : byte array
        // Output        : string
        // Purpose       : Function to convert byte array to string 
        // Date          : 27Sept16
        //----------------------------------------------------------------------------
        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
        //----------------------------------------------------------------------------
        // Method Name   : BT_GetDevices
        // Input         : NA
        // Output        : Collection of Bluetoothdevices with their status,name, address.
        // Purpose       : Get list of all devices with a specific BT/BLE name and with status paired and unpaired
        // Date          : 20-08-20
        //----------------------------------------------------------------------------

        #endregion

        #region WifiServices
        private object Wifi_GetDevices()
        {
            throw new NotImplementedException();
        }

        //private object Wifi_ConnectAP()
        //{
        //    tcpClient.StringEncoder = Encoding.UTF8;
        //    tcpClient.DataReceived += TcpClient_DataReceived;

        //    var clientStatus = tcpClient.Connect("Host", Convert.ToInt32("Port"));
        //    if (clientStatus.TcpClient.Connected == true)
        //    {
        //        ActiveConnection = Wifi;
        //        return true;
        //    }
        //    return false;
        //}

        private object Wifi_WriteSSIDPW()
        {
            throw new NotImplementedException();
        }

        private object Wifi_ConnectStation()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region DongleServices

        public async Task<object> SecurityAccess()
        {
            Debug.WriteLine("------SecurityAccess------", DebugTag);

            string command = "500C47568AFE56214E238000FFC3";
            var bytesCommand = HexStringToByteArray(command);

            //SerialPortDataReceived(this, null);
            var response = await SendCommand(bytesCommand, (obj) => { WriteConsole(command, obj); }); ;

            return response;
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
            //"0x20-0x01-0x01"
            string command = "200301";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            byte[] sendBytes = HexStringToByteArray(command + crc);
            if (crc.Length == 3)
                crc = "0" + crc;
            var response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        //----------------------------------------------------------------------------
        // Method Name   : Dongle_SetProtocol
        // Input         : NA
        // Output        : object
        // Purpose       : Set Vehicle Communication Protocol
        // Date          : 20-08-20
        //----------------------------------------------------------------------------
        public async Task<object> Dongle_SetProtocol(int protocolVersion)
        {
            object response = null;
            //   "0x20-0x02-0x02-0x < protocol > "

            Debug.WriteLine("------Dongle_SetProtocol------", DebugTag);
            string command = "200402" + protocolVersion.ToString("X2").Substring(0, 2);
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            protocol = (Protocol)protocolVersion;
            return response;
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
            //"0x20-0x01-0x03"

            string command = "200303";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
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
            string command = "200314";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        int read_byte = 0;
        public async Task<object> CAN_ClearSocket()
        {
            if (Platform == Platform.Android && Connectivity == Connectivity.Bluetooth)
            {
                byte[] rbuffer = new byte[1024];
                while (BluetoothSocket.InputStream.IsDataAvailable())
                {
                    //read_byte = await BluetoothSocket.InputStream.ReadAsync(rbuffer, 0, rbuffer.Length);
                    read_byte = BluetoothSocket.InputStream.ReadByte();
                }
                //Console.WriteLine("Socket Not Empty");
                //read_byte = await BluetoothSocket.InputStream.ReadAsync(rbuffer, 0, rbuffer.Length);
                //Console.WriteLine("Socket Empty");
                await BluetoothSocket.InputStream.FlushAsync();
            }
            else if (Platform == Platform.Android && Connectivity == Connectivity.USB)
            {
                bool IsRead = true;
                int count = 0;
                byte[] rbuffer = new byte[1024];


                Debug.WriteLine("---------Clear Garbage Data-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                //var ss = USBport.Read(byteRes, 100);

                //var response = await tskCmSsrc.Task;

                // handle incoming data.
                //USBport.
                while (IsRead == true)
                {
                    Debug.WriteLine("---------Clearing Garbage Data-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                    var len = USBport.Read(rbuffer, 100);//TBD CHECK
                    //USBport.
                    Debug.WriteLine("---------Cleared Garbage Data-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                    if (len < 1)
                    {
                        IsRead = false;
                    }
                    if (len > 0)
                    {
                        byte[] RetArray = new byte[len];
                        Array.Copy(rbuffer, RetArray, len);

                        //DataReceived.Raise(this, new SerialDataReceivedArgs(data));
                        Debug.WriteLine("---------USB READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);
                        // Debug.WriteLine("---------USB READ DATA RESPONSE TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                        //return RetArray;
                    }
                }
            }

            return null;
        }


        public async Task<object> Dongle_SetFota(string command1)
        {
            Debug.WriteLine("------Start_Fota------", DebugTag);
            object response = null;
            int lenght = 3 + command1.Length;

            string hexValue = lenght.ToString("X2");
            string command = "20" + hexValue + "19" + command1;
            //var bytesCommand = HexStringToByteArray(command);

            byte[] vs = new byte[command1.Length + 1];

            vs[0] = 0x19;

            for (int i = 1; i <= command1.Length; i++)
            {
                vs[i] = (byte)command1[i - 1];
            }





            string crc = Crc16CcittKermit.ComputeChecksum(vs).ToString("x2");
            if (crc.Length == 3)
            {
                crc = "0" + crc;
            }
            var hexString = BitConverter.ToString(vs);
            hexString = hexString.Replace("-", "");

            command = "20" + hexValue + hexString + crc;
            byte[] sendBytes = HexStringToByteArray( command);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> GetWifiMacId()
        {
            Debug.WriteLine("------Start_Get_Mac_Id------", DebugTag);
            object response = null;
            //"20 03 21 d5 b3"
            string command = "200321";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            //var ecuResponseBytes = (byte[])response;
            //ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);


            return response;
        }


        #endregion

        #region CANMethods
        public async Task<object> CAN_SetTxHeader(string txHeader)
        {
            Debug.WriteLine("------CAN_SetTxHeader------", DebugTag);
            //            "if protocol is one of the following: ISO15765-250KB-11BIT-CAN, ISO15765-500KB-11BIT-CAN, ISO15765-1MB-11BIT-CAN, 250KB-11BIT-CAN, 500KB-11BIT-CAN, 1MB-11BIT-CAN, OE-IVN-250KBPS-11BIT-CAN, OE-IVN-500KBPS-11BIT-CAN,OE-IVN-1MBPS-11BIT-CAN 
            //0x20-0x03-0x04-0xxx-0xyy

            //if protocol is one of the following: ISO15765 - 250KB - 29BIT - CAN, ISO15765 - 500KB - 29BIT - CAN, ISO15765 - 1MB - 29BIT - CAN, 250KB - 29BIT - CAN, 500KB - 29BIT - CAN, 1MB - 29BIT - CAN, OE - IVN - 250KBPS - 29BIT - CAN, OE - IVN - 500KBPS - 29BIT - CAN,OE - IVN - 1MBPS - 29BIT - CAN
            //0x20-0x05-0x04-0xpp-0xqq-0xrr-0xss"

            object response = null;
            string command = string.Empty;
            byte[] sendBytes = null;
            if (protocol == Protocol.ISO15765_250KB_11BIT_CAN ||
                protocol == Protocol.ISO15765_500KB_11BIT_CAN ||
                protocol == Protocol.ISO15765_1MB_11BIT_CAN ||
                protocol == Protocol.I250KB_11BIT_CAN ||
                protocol == Protocol.I500KB_11BIT_CAN ||
                protocol == Protocol.I1MB_11BIT_CAN ||
                protocol == Protocol.OE_IVN_250KBPS_11BIT_CAN ||
                protocol == Protocol.OE_IVN_500KBPS_11BIT_CAN ||
                protocol == Protocol.OE_IVN_1MBPS_11BIT_CAN ||
                protocol == Protocol.CANOPEN_125KBPS_11BIT_CAN||
                protocol == Protocol.CANOPEN_500KBPS_11BIT_CAN||
                protocol == Protocol.XMODEM_125KBPS_11BIT_CAN ||
                protocol == Protocol.XMODEM_500KBPS_11BIT_CAN)
            {
                command = "200504" + txHeader.ToString();
                var bytesCommand = HexStringToByteArray(command);
                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
                if (crc.Length == 3)
                    crc = "0" + crc;
                sendBytes = HexStringToByteArray(command + crc);
            }
            else if (protocol == Protocol.ISO15765_250Kb_29BIT_CAN || 
                protocol == Protocol.ISO15765_500KB_29BIT_CAN || 
                protocol == Protocol.ISO15765_1MB_29BIT_CAN || 
                protocol == Protocol.I250Kb_29BIT_CAN || 
                protocol == Protocol.I500KB_29BIT_CAN || 
                protocol == Protocol.I1MB_29BIT_CAN || 
                protocol == Protocol.OE_IVN_250KBPS_29BIT_CAN || 
                protocol == Protocol.OE_IVN_500KBPS_29BIT_CAN || 
                protocol == Protocol.OE_IVN_1MBPS_29BIT_CAN ||
                protocol == Protocol.XMODEM_500KBPS_29BIT_CAN ||
                protocol == Protocol.XMODEM_125KBPS_29BIT_CAN)
            {
                command = "200704" + txHeader.ToString();
                var bytesCommand = HexStringToByteArray(command);
                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4], bytesCommand[5], bytesCommand[6]).ToString("x2");
                if (crc.Length == 3)
                    crc = "0" + crc;
                sendBytes = HexStringToByteArray(command + crc);
            }
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

            return response;
        }

        public async Task<object> CAN_GetTxHeader()
        {
            Debug.WriteLine("------Dongle_SetProtocol------", DebugTag);
            object response = null;
            //"0x20-0x01-0x05"
            string command = "200305";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_SetRxHeaderMask(string rxhdrmsk)
        {
            Debug.WriteLine("------CAN_SetRxHeaderMask------", DebugTag);
            object response = null;
            //            "if protocol is one of the following: ISO15765-250KB-11BIT-CAN, ISO15765-500KB-11BIT-CAN, ISO15765-1MB-11BIT-CAN, 250KB-11BIT-CAN, 500KB-11BIT-CAN, 1MB-11BIT-CAN, OE-IVN-250KBPS-11BIT-CAN, OE-IVN-500KBPS-11BIT-CAN,OE-IVN-1MBPS-11BIT-CAN 
            //0x20-0x03-0x06-0xxx-0xyy

            //if protocol is one of the following: ISO15765 - 250KB - 29BIT - CAN, ISO15765 - 500KB - 29BIT - CAN, ISO15765 - 1MB - 29BIT - CAN, 250KB - 29BIT - CAN, 500KB - 29BIT - CAN, 1MB - 29BIT - CAN, OE - IVN - 250KBPS - 29BIT - CAN, OE - IVN - 500KBPS - 29BIT - CAN,OE - IVN - 1MBPS - 29BIT - CAN
            //0x20-0x05-0x06-0xpp-0xqq-0xrr-0xss"

            string command = string.Empty;
            byte[] sendBytes = null;
            if (protocol == Protocol.ISO15765_250KB_11BIT_CAN ||
                protocol == Protocol.ISO15765_500KB_11BIT_CAN ||
                protocol == Protocol.ISO15765_1MB_11BIT_CAN ||
                protocol == Protocol.I250KB_11BIT_CAN ||
                protocol == Protocol.I500KB_11BIT_CAN ||
                protocol == Protocol.I1MB_11BIT_CAN ||
                protocol == Protocol.OE_IVN_250KBPS_11BIT_CAN ||
                protocol == Protocol.OE_IVN_500KBPS_11BIT_CAN ||
                protocol == Protocol.OE_IVN_1MBPS_11BIT_CAN ||
                protocol == Protocol.CANOPEN_125KBPS_11BIT_CAN ||
                protocol == Protocol.CANOPEN_500KBPS_11BIT_CAN ||
                protocol == Protocol.XMODEM_125KBPS_11BIT_CAN ||
                protocol == Protocol.XMODEM_500KBPS_11BIT_CAN)
            {
                command = "200506" + rxhdrmsk.ToString();
                var bytesCommand = HexStringToByteArray(command);
                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
                if (crc.Length == 3)
                    crc = "0" + crc;
                sendBytes = HexStringToByteArray(command + crc);
            }
            else if (protocol == Protocol.ISO15765_250Kb_29BIT_CAN ||
                protocol == Protocol.ISO15765_500KB_29BIT_CAN ||
                protocol == Protocol.ISO15765_1MB_29BIT_CAN ||
                protocol == Protocol.I250Kb_29BIT_CAN ||
                protocol == Protocol.I500KB_29BIT_CAN ||
                protocol == Protocol.I1MB_29BIT_CAN ||
                protocol == Protocol.OE_IVN_250KBPS_29BIT_CAN ||
                protocol == Protocol.OE_IVN_500KBPS_29BIT_CAN ||
                protocol == Protocol.OE_IVN_1MBPS_29BIT_CAN ||
                protocol == Protocol.XMODEM_500KBPS_29BIT_CAN ||
                protocol == Protocol.XMODEM_125KBPS_29BIT_CAN)
            {
                command = "200706" + rxhdrmsk.ToString();
                var bytesCommand = HexStringToByteArray(command);
                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4], bytesCommand[5], bytesCommand[6]).ToString("x2");
                if (crc.Length == 3)
                    crc = "0" + crc;
                sendBytes = HexStringToByteArray(command + crc);
            }
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;

        }

        public async Task<object> CAN_GetRxHeaderMask()
        {
            Debug.WriteLine("------CAN_GetRxHeaderMask------", DebugTag);
            object response = null;
            //"0x20-0x01-0x07"

            string command = "200307";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_SetP1Min(string p1min)
        {
            Debug.WriteLine("------CAN_GetRxHeaderMask------", DebugTag);
            object response = null;
            //"0x20-0x02-0x0c-0x < xx > "

            string command = "20040c" + p1min.ToString();
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_GetP1Min()
        {
            Debug.WriteLine("------CAN_GetP1Min------", DebugTag);
            object response = null;
            //"0x20-0x01-0x0d"

            string command = "20030d";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            byte[] sendBytes = HexStringToByteArray(command + crc);
            if (crc.Length == 3)
                crc = "0" + crc;
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_SetP2Max(string p2max)
        {
            Debug.WriteLine("------CAN_SetP2Max------", DebugTag);
            object response = null;
            //"0x20-0x03-0x0e-0x < xx >-0x < yy > "

            string command = "20050e" + p2max.ToString();
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_GetP2Max()
        {
            Debug.WriteLine("------CAN_GetP2Max------", DebugTag);
            object response = null;
            //"0x20-0x01-0x0f"

            string command = "20030f";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);

            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_StartTP()
        {
            Debug.WriteLine("------CAN_StartTP------", DebugTag);
            object response = null;
            //"0x20-0x01-0x10"

            string command = "200310";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_StopTP()
        {
            Debug.WriteLine("------CAN_StopTP------", DebugTag);
            object response = null;
            //"0x20,0x01, 0x11"

            string command = "200311";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> SetTesterPresent(string comm)
        {
            Debug.WriteLine("------CAN_StartPadding------", DebugTag);
            object response = null;
            //"0x20,0x02,0x12, 0x < xx > "

            string command = "200412" + comm.ToString();
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;

            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_StartPadding(string paddingByte)
        {
            Debug.WriteLine("------CAN_StartPadding------", DebugTag);
            object response = null;
            //"0x20,0x02,0x12, 0x < xx > "

            string command = "200412" + paddingByte.ToString();
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;

            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_StopPadding()
        {
            Debug.WriteLine("------CAN_StopPadding------", DebugTag);
            object response = null;
            //"0x20- 0x01-0x13"

            string command = "200313";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;

            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> CAN_TxData(string txdata)
        {
            Debug.WriteLine("------CAN_StartPadding------", DebugTag);
            object response = null;
            ////data requests are of 2 types. If length of message <1000> then use 4x command, if not, use 1x command
            //0x4 < l >
            //0x < ll >
            string command = "40" + txdata.ToString();
            var bytesCommand = HexStringToByteArray(command);
            var crcBytesComputation = HexStringToByteArray(txdata);
            string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("x2");
            if (crc.Length == 3)
            {
                crc = "0" + crc;
            }
            byte[] sendBytes = HexStringToByteArray(command + crc);

            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;

        }

        //public async Task<object> CAN_RxData()
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<ResponseArrayStatus> CAN_TxRx(int framelength, string txdata)
        {
            Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
            object response = null;
            try
            {
                int dataLength = framelength + 2; //crc
                string command = string.Empty;

                var firstbyte = 0x40 | ((dataLength >> 8) & 0x0f);
                var secondbyte = dataLength & 0xff;

                command = firstbyte.ToString("X2") + secondbyte.ToString("X2") + txdata.ToString();
                var bytesCommand = HexStringToByteArray(command);
                var crcBytesComputation = HexStringToByteArray(txdata);
                string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("X2");
                if (crc.Length == 3)
                {
                    crc = "0" + crc;
                }
                if (crc.Length == 2)
                {
                    crc = "00" + crc;
                }
                if (crc.Length == 1)
                {
                    crc = "000" + crc;
                }
                Debug.WriteLine("CRC =" + crc, DebugTag);
                byte[] sendBytes = HexStringToByteArray(command + crc);

                UInt16 nooftimessent = 0;

                STARTOVERAGAIN:
                if (Platform == Platform.Android && Connectivity == Connectivity.RP1210)
                {
                    response = await SendCommand(crcBytesComputation, (obj) => { WriteConsole(command, obj); });
                }
                else
                {
                    response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
                }
                //await Task.Delay(500);
                nooftimessent++;
                //if(response=="")

                var res = ByteArrayToString((byte[])response);
                string str = Encoding.Default.GetString((byte[])response);

                if (str.Contains("Dongle disconnected"))
                {
                    responseStructure = new ResponseArrayStatus
                    {
                        ECUResponseStatus = "Dongle disconnected"
                    };
                    return responseStructure;
                }


                if (response != null)
                {
                    if (Convert.ToString(response).Contains("dhcps: send_offer>>udp_sendto result 0") == true)
                    {
                        Debug.WriteLine("--------- stripping off unwanted characters from response dhcps: send_offer >> udp_sendto result 0------------==" + "ELMZ");
                        string ValueToReplace = "dhcps: send_offer>>udp_sendto result 0";
                        response = (string)response.ToString().Replace(ValueToReplace, "");
                        //var ecuResponseBytes = (byte[])response;
                    }

                    byte[] actualDataBytes;
                    string dataStatus;

                    var ecuResponseBytes = (byte[])response;

                    if (Platform == Platform.Android && Connectivity == Connectivity.RP1210)
                        ResponseArrayDecoding.CheckResponseRP1210(ecuResponseBytes, sendBytes, out actualDataBytes, out dataStatus);
                    else
                        ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out actualDataBytes, out dataStatus);

                    if (dataStatus == "SENDAGAIN")
                    {
                        while (dataStatus == "SENDAGAIN")
                        {
                            Debug.WriteLine("------SENDAGAIN ------" + Convert.ToString(nooftimessent), DebugTag);

                            if (nooftimessent <= 5)
                            {
                                goto STARTOVERAGAIN;
                            }
                            else
                            {
                                /* stop sending again - Problem with the device */
                                responseStructure = new ResponseArrayStatus
                                {
                                    ECUResponse = ecuResponseBytes,
                                    ECUResponseStatus = "DONGLEERROR_SENDAGAINTHRESHOLDCROSSED",
                                    ActualDataBytes = actualDataBytes
                                };
                                break;
                            }

                            //response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
                            //if (response != null)
                            //{
                            //    var ecuResponseBytes2 = (byte[])response;
                            //    ResponseArrayDecoding.CheckResponse(ecuResponseBytes2, sendBytes, out byte[] actualDataBytes2, out string dataStatus2);
                            //    dataStatus = dataStatus2;
                            //    responseStructure = new ResponseArrayStatus
                            //    {
                            //        ECUResponse = ecuResponseBytes2,
                            //        ECUResponseStatus = dataStatus2,
                            //        ActualDataBytes = actualDataBytes2
                            //    };
                            //    Debug.WriteLine("------SENDAGAIN DATA START ------", DebugTag);
                            //    if (responseStructure?.ECUResponse != null)
                            //        Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseBytes2), DebugTag);
                            //    if (responseStructure?.ActualDataBytes != null)
                            //        Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
                            //    Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
                            //    Debug.WriteLine("------SENDAGAIN DATA END ------", DebugTag);
                            //}
                        }
                    }
                    else if (dataStatus == "READAGAIN")
                    {
                        while (dataStatus == "READAGAIN")
                        {
                            var responseReadAgain = await ReadData();
                            var ecuResponseReadBytes = (byte[])responseReadAgain;

                            byte[] actualReadBytes;
                            string dataReadStatus;

                            if (Platform == Platform.Android && Connectivity == Connectivity.RP1210)
                                ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out actualReadBytes, out dataReadStatus);
                            else
                                ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out actualReadBytes, out dataReadStatus);

                            dataStatus = dataReadStatus;
                            responseStructure = new ResponseArrayStatus
                            {
                                ECUResponse = ecuResponseReadBytes,
                                ECUResponseStatus = dataReadStatus,
                                ActualDataBytes = actualReadBytes
                            };
                            Debug.WriteLine("------EXTRA READ DATA START ------", DebugTag);
                            if (responseStructure?.ECUResponse != null)
                                Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseReadBytes), DebugTag);
                            if (responseStructure?.ActualDataBytes != null)
                                Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
                            Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
                            Debug.WriteLine("------EXTRA READ DATA END ------", DebugTag);
                        }
                    }
                    else
                    {
                        responseStructure = new ResponseArrayStatus
                        {
                            ECUResponse = ecuResponseBytes,
                            ECUResponseStatus = dataStatus,
                            ActualDataBytes = actualDataBytes
                        };
                        Debug.WriteLine("------ECU RESPONE START ------", DebugTag);
                        if (responseStructure?.ECUResponse != null)
                            Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(responseStructure?.ECUResponse), DebugTag);
                        if (responseStructure?.ActualDataBytes != null)
                            Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
                        Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
                        Debug.WriteLine("------ECU RESPONE END ------", DebugTag);
                    }
                }
                else
                {

                }
                return responseStructure;
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("non-connected sockets"))
                {
                    responseStructure = new ResponseArrayStatus
                    {
                        ECUResponse = null,
                        ECUResponseStatus = $"Dongle disconnected",
                        ActualDataBytes = null
                    };

                }
                else
                {
                    responseStructure = new ResponseArrayStatus
                    {
                        ECUResponse = null,
                        ECUResponseStatus = $"{ex.Message}",
                        ActualDataBytes = null
                    };

                }
                return responseStructure;
            }


        }

        //public async Task<ResponseArrayStatus> CAN_TxRx(int framelength, string txdata)
        //{
        //    Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
        //    object response = null;
        //    int dataLength = framelength + 2;
        //    string command = "40" + dataLength.ToString("X2") + txdata.ToString();
        //    var bytesCommand = HexStringToByteArray(command);
        //    var crcBytesComputation = HexStringToByteArray(txdata);
        //    string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("X2");
        //    if (crc.Length == 3)
        //    {
        //        crc = "0" + crc;
        //    }
        //    byte[] sendBytes = HexStringToByteArray(command + crc);

        //    response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

        //    if (response != null)
        //    {
        //        var ecuResponseBytes = (byte[])response;
        //        ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);
        //        if (dataStatus == "READAGAIN")
        //            while (dataStatus == "READAGAIN")
        //            {
        //                var responseReadAgain = await ReadData();
        //                var ecuResponseReadBytes = (byte[])responseReadAgain;
        //                ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
        //                dataStatus = dataReadStatus;
        //                responseStructure = new ResponseArrayStatus
        //                {
        //                    ECUResponse = ecuResponseReadBytes,
        //                    ECUResponseStatus = dataReadStatus,
        //                    ActualDataBytes = actualReadBytes


        //                };
        //                Debug.WriteLine("------READ DATA START ------", DebugTag);
        //                if (responseStructure?.ECUResponse != null)
        //                    Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseReadBytes), DebugTag);
        //                if (responseStructure?.ActualDataBytes != null)
        //                    Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
        //                Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
        //                Debug.WriteLine("------READ DATA END ------", DebugTag);
        //            }
        //        else
        //        {

        //            responseStructure = new ResponseArrayStatus
        //            {
        //                ECUResponse = ecuResponseBytes,
        //                ECUResponseStatus = dataStatus,
        //                ActualDataBytes = actualDataBytes
        //            };
        //            Debug.WriteLine("------ECU RESPONE WITHOUT READ START ------", DebugTag);
        //            if (responseStructure?.ECUResponse != null)
        //                Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(responseStructure?.ECUResponse), DebugTag);
        //            if (responseStructure?.ActualDataBytes != null)
        //                Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
        //            Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
        //            Debug.WriteLine("------ECU RESPONE WITHOUT READ END ------", DebugTag);

        //        }
        //    }

        //    return responseStructure;

        //}

        //public async Task<object> CAN_TxRx(int frameLength, string txdata)
        //{
        //    Debug.WriteLine("------CAN_StartPadding------", DebugTag);
        //    object response = null;

        //    string command = "40" + txdata.ToString();
        //    var bytesCommand = HexStringToByteArray(command);
        //    string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
        //    byte[] sendBytes = HexStringToByteArray(command + crc);

        //    response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); }); ;

        //    return response;

        //}   

        public async Task<object> SetBlkSeqCntr(string blklen)
        {
            Debug.WriteLine("------SetBlkSeqCntr------", DebugTag);
            object response = null;
            //"0x20- 0x02 - 0x08- 0xxx(0x00 to 0x40)"

            string command = "200408" + blklen.ToString();
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
            {
                crc = "0" + crc;
            }
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> GetBlkSeqCntr()
        {
            Debug.WriteLine("------GetBlkSeqCntr------", DebugTag);
            object response = null;
            //"0x20- 0x01-0x09"
            string command = "200309";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
            {
                crc = "0" + crc;
            }
            byte[] sendBytes = HexStringToByteArray(command + crc);

            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> SetSepTime(string septime)
        {
            Debug.WriteLine("------SetSepTime------", DebugTag);
            object response = null;
            //"0x20-0x02-0x0A-0xxx"

            string command = "20040A" + septime.ToString();
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
            {
                crc = "0" + crc;
            }
            byte[] sendBytes = HexStringToByteArray(command + crc);

            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            return response;
        }

        public async Task<object> GetSepTime()
        {
            Debug.WriteLine("------GetSepTime------", DebugTag);
            object response = null;
            //"0x20-0x01-0x0B"
            string command = "20030B";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
            if (crc.Length == 3)
            {
                crc = "0" + crc;
            }
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); }); ;

            return response;
        }

        public async Task<object> CAN_Get_Default_SSID()
        {
            Debug.WriteLine("------CAN_Get Default SSID------", DebugTag);
            object response = null;
            string command = string.Empty;
            byte[] sendBytes = null;
            command = "20042200";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

            return response;
        }

        public async Task<object> CAN_Get_Default_Password()
        {
            Debug.WriteLine("------CAN_Get Default Password------", DebugTag);
            object response = null;
            string command = string.Empty;
            byte[] sendBytes = null;
            command = "20042300";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

            return response;
        }

        public async Task<object> CAN_Get_User_SSID()
        {
            Debug.WriteLine("------CAN_Get User SSID------", DebugTag);
            object response = null;
            string command = string.Empty;
            byte[] sendBytes = null;
            command = "20042201";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

            return response;
        }

        public async Task<object> CAN_Get_User_Password()
        {
            Debug.WriteLine("------CAN_Get User Password------", DebugTag);
            object response = null;
            string command = string.Empty;
            byte[] sendBytes = null;
            command = "20042301";
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

            return response;
        }
        #endregion

        #endregion


        #region RP1210Methods
        public string ConnectDevice(string device_name, string txHeader, string rxHeader, string protocol)
        {
            bool device_connected = class1.ConnectDevice(device_name);
            bool client_status = false;
            string fw_version = "";

            if (device_connected)
            {
                if(protocol.Contains("500"))
                    client_status = class1.ClientConnect("ISO15765:Baud=500000,Channel=1");
                else
                    client_status = class1.ClientConnect("ISO15765:Baud=1000000,Channel=1");

                if (client_status)
                {
                    //await DisplayAlert("", "Client Connected", "Ok");

                    Debug.WriteLine("------ Get Firmware -----", DebugTag);
                    fw_version = class1.GetFirmwareVersion();
                    Debug.WriteLine(fw_version, DebugTag);

                    Debug.WriteLine("------ Set Flow Control -----", DebugTag);
                    //val =command_id + msg_type_code + self.rxCAN_id + self.rx_extende_addrs + self.txCAN_id + self.tx_extende_addrs + BlockSize + STmin + STMIN_tx

                    if(txHeader.Length == 4)
                    {
                        txHeader = "0000" + txHeader;
                        rxHeader = "0000" + rxHeader;
                    }

                    byte[] val = HexStringToByteArray("0022" + "02" + rxHeader + "00" + txHeader + "00" + "00" + "00" + "ffff");
                    //byte[] val = HexStringToByteArray("0022" + "02" + "000007e8" + "00" + "000007e0" + "00" + "00" + "00" + "ffff");
                    string fw_version1 = class1.SendCommand(val);
                    Debug.WriteLine(fw_version1, DebugTag);


                    Debug.WriteLine("------ Set Message Filter -----", DebugTag);
                    // if protocol is "ISO15765" the put the "exteded_mask" else skip "exteded_mask"
                    // if protocol is "ISO15765" the put the "rx_extende_addrs" else skip "rx_extende_addrs"

                    //val = command_id + msg_type_code + mask + ["", exteded_mask][self.protocol_name == "ISO15765"] + self.rxCAN_id + ["", self.rx_extende_addrs][self.protocol_name == "ISO15765"]

                    byte[] val2 = HexStringToByteArray("0009" + "02" + "ffffffff" + "ff" + rxHeader + "00");
                    //byte[] val2 = HexStringToByteArray("0009" + "02" + "ffffffff" + "ff" + "000007e8" + "00");
                    string fw_version2 = class1.SendCommand(val2);
                    Debug.WriteLine(fw_version2, DebugTag);
                }
            }
            return fw_version;
        }



        #endregion

    }



    //public class DongleCommWin : ICANCommands, IWifiUSBHandler, IDongleHandler
    //{
    //    #region Properties
    //    private TaskCompletionSource<object> tskCmSsrc = null;
    //    private Protocol protocol;
    //    private string DebugTag = "ELM-DEBUG";
    //    private ResponseArrayStatus responseStructure;

    //    BluetoothSocket BluetoothSocket = null;
    //    SerialInputOutputManager SerialInputOutputManager = null;
    //    UsbSerialPort USBport = null;
    //    TcpClient TcpClient = null;

    //    public Platform Platform = Platform.None;
    //    public Connectivity Connectivity = Connectivity.None;
    //    private NetworkStream Stream = null;

    //    #endregion

    //    #region Ctor
    //    public DongleCommWin()
    //    {
    //        Debug.WriteLine("Inside DongleComm", DebugTag);
    //    }
    //    public void InitializePlatform(Platform platform, Connectivity connectivity)
    //    {
    //        Platform = platform;
    //        Connectivity = connectivity;
    //    }
    //    public DongleCommWin(BluetoothSocket socket, Protocol protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
    //    {
    //        Debug.WriteLine("Inside BluetoothSocket CTOR", DebugTag);
    //        BluetoothSocket = socket;
    //        protocol = protocolVersion;
    //    }

    //    public DongleCommWin(TcpClient client, NetworkStream networkStream, Protocol protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
    //    {
    //        Debug.WriteLine("Inside ELM327 TcpClient CTOR", DebugTag);
    //        TcpClient = client;
    //        protocol = protocolVersion;
    //        Stream = networkStream;
    //    }

    //    public DongleCommWin(SerialInputOutputManager serialInputOutputManager, UsbSerialPort port, int protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
    //    {
    //        Debug.WriteLine("Inside ELM327 SimpleTcpClient CTOR", DebugTag);
    //        SerialInputOutputManager = serialInputOutputManager;
    //        USBport = port;
    //        protocol = (Protocol)protocolVersion;

    //        if (tskCmSsrc == null)
    //        {
    //            tskCmSsrc = new TaskCompletionSource<object>();
    //        }

    //        //SerialInputOutputManager.DataReceived += (sender, e) =>
    //        //{
    //        //    tskCmSsrc?.TrySetResult(e.Data);
    //        //    Debug.WriteLine("SerialInputOutputManager.DataReceived"+ ByteArrayToString(e.Data), DebugTag);

    //        //};
    //    }

    //    #endregion

    //    #region Methods

    //    #region SendCommand
    //    public async Task<object> ReadData()
    //    {
    //        Debug.WriteLine("------Read Again Data------", DebugTag);
    //        if (Platform == Platform.Android && Connectivity == Connectivity.Bluetooth)
    //        {
    //            return await GetBTCommand();
    //        }
    //        if (Platform == Platform.Android && Connectivity == Connectivity.USB)
    //        {

    //            if (SerialInputOutputManager.IsOpen)
    //            {
    //                return await GetUSBCommand();
    //            }
    //        }
    //        if (Platform == Platform.Android && Connectivity == Connectivity.WiFi)
    //        {
    //            return await GetWifiCommand();
    //        }
    //        Debug.WriteLine("------END Read Again Data------", DebugTag);
    //        return null;
    //    }
    //    public async Task<object> SendCommand(string randomCommand)
    //    {
    //        Debug.WriteLine("------SendCommand------", DebugTag);
    //        object response = null;

    //        string command = randomCommand.ToString();
    //        var bytesCommand = HexStringToByteArray(command);

    //        byte[] sendBytes = HexStringToByteArray(command);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); }); ;

    //        return response;

    //    }

    //    public async Task<object> SendCommand(byte[] command, Action<string> onDataRecevied)
    //    {
    //        if (Platform == Platform.Android && Connectivity == Connectivity.Bluetooth)
    //        {
    //            Debug.WriteLine("Command BT Send =  " + ByteArrayToString(command), DebugTag);
    //            Debug.WriteLine("--------- BT Command Send TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
    //            BluetoothSocket.OutputStream.Write(command, 0, command.Length);
    //            BluetoothSocket.OutputStream.Flush();
    //            //BluetoothSocket.OutputStream.Close();
    //            //Thread.Sleep(100);
    //            var response = await GetBTCommand();
    //            Debug.WriteLine("Command BT RESPONSE =  " + ByteArrayToString(response), DebugTag);
    //            Debug.WriteLine("--------- BT Command RESPONSE TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
    //            return response;
    //        }

    //        if (Platform == Platform.Android && Connectivity == Connectivity.USB)
    //        {
    //            Debug.WriteLine("Command USB Send =  " + ByteArrayToString(command), DebugTag);
    //            Debug.WriteLine("--------- USB Command Send TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //            if (SerialInputOutputManager.IsOpen)
    //            {
    //                var byteRes = new byte[1024];
    //                USBport.Write(command, 0);//TBD CHECK
    //                var response = await GetUSBCommand();
    //                if (response != null)
    //                {
    //                    var byteResponse = (byte[])response;
    //                    Debug.WriteLine("Command USB RESPONSE =  " + ByteArrayToString(byteResponse), DebugTag);
    //                    Debug.WriteLine("--------- USB Command RESPONSE TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //                    return response;
    //                }
    //            }
    //        }

    //        if (Platform == Platform.Android && Connectivity == Connectivity.WiFi)
    //        {
    //            Debug.WriteLine("Command WiFi Send =  " + ByteArrayToString(command), DebugTag);

    //            //00 000e 00000000 00000000 500c47568afe56214e238000ffc3 ce2c

    //            //00 - Tx sequence counter
    //            //000e - length of the command(command length + 2)
    //            //00000000 - Timestamp - hhmmssmm
    //            //00000000 - Reserved - always 00
    //            //500c47568afe56214e238000ffc3 - actual command - payload
    //            //ce2c - crc of entire length from tx counter to the payload.

    //            var byte1 = "00";
    //            var byte2 = command.Length.ToString("X4");
    //            var byte3 = DateTime.Now.ToString("hhmmssff");
    //            var byte4 = DateTime.Now.ToString("00000000");
    //            var byte5 = ByteArrayToString(command);
    //            var dataBytes = HexStringToByteArray(byte1 + byte2 + byte3 + byte4 + byte5);
    //            var byte6Checksum = Crc16CcittKermit.ComputeChecksum(dataBytes);

    //            var bytes = ByteArrayToString(dataBytes) + byte6Checksum.ToString("X4");
    //            var byteData = HexStringToByteArray(bytes);
    //            Stream = TcpClient.GetStream();
    //            Stream.Write(byteData, 0, byteData.Length);

    //            byte[] WifiComandValue = await GetWifiCommand();

    //            return WifiComandValue;
    //        }
    //        return null;
    //    }

    //    public async Task<byte[]> GetWifiCommand()
    //    {
    //        try
    //        {
    //            byte[] rbuffer = new byte[1024];
    //            byte[] RetArray = new byte[] { };
    //            byte[] ActualBytes = new byte[] { };

    //            Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //            int readByte = await Stream.ReadAsync(rbuffer, 0, rbuffer.Length);

    //            RetArray = new byte[readByte];
    //            Array.Copy(rbuffer, 0, RetArray, 0, readByte);

    //            Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);

    //            UInt16 wifistartindex = 0;
    //            UInt16 actualdatastartindex = 0; // first index where to read the actual data from after wifi header
    //            UInt16 decValue = 0;
    //            UInt16 updateddecValue = 0;

    //            while (wifistartindex < readByte)
    //            {
    //                //var byteLength = RetArray.Length.ToString("d4");
    //                //string hexValue = RetArray[wifistartindex+1].ToString() + RetArray[wifistartindex+2].ToString();
    //                decValue = (UInt16)((((UInt16)RetArray[wifistartindex + 1]) << 8) + ((UInt16)RetArray[wifistartindex + 2]));

    //                updateddecValue += decValue;

    //                Array.Resize(ref ActualBytes, updateddecValue);
    //                Array.Copy(RetArray, wifistartindex + 11, ActualBytes, actualdatastartindex, decValue);
    //                wifistartindex += (UInt16)(13 + decValue);
    //                actualdatastartindex += decValue;
    //            }


    //            Debug.WriteLine("--------- DATA RESPONSE-------" + ByteArrayToString(ActualBytes), DebugTag);
    //            return ActualBytes;
    //        }
    //        catch (Exception ex)
    //        {

    //            return null;
    //        }
    //    }

    //    public async Task<byte[]> GetBTCommand()
    //    {
    //        byte[] rbuffer = new byte[1024];
    //        byte[] RetArray = new byte[] { };

    //        Debug.WriteLine("---------BT INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
    //        // Read data from the device
    //        while (!BluetoothSocket.InputStream.CanRead)
    //        {
    //            Debug.WriteLine("------------------------------------------------");
    //        }

    //        int readByte = await BluetoothSocket.InputStream.ReadAsync(rbuffer, 0, rbuffer.Length);
    //        //int readByte =  BluetoothSocket.InputStream.Read(rbuffer, 0, rbuffer.Length);
    //        RetArray = new byte[readByte];
    //        Array.Copy(rbuffer, 0, RetArray, 0, readByte);

    //        //Console.WriteLine(Array2Text(RetArray, false) + "\n" + "[" + RetArray.Length.ToString() + "]");

    //        Debug.WriteLine("--------- BT READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);
    //        // Debug.WriteLine("--------- BT READ DATA RESPONSE TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //        return RetArray;
    //    }
    //    public async Task<byte[]> GetUSBCommand()
    //    {
    //        byte[] rbuffer = new byte[1024];


    //        Debug.WriteLine("---------USB INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //        //var ss = USBport.Read(byteRes, 100);

    //        //var response = await tskCmSsrc.Task;

    //        // handle incoming data.
    //        var len = USBport.Read(rbuffer, 0);//TBD CHECK
    //        if (len > 0)
    //        {
    //            byte[] RetArray = new byte[len];
    //            Array.Copy(rbuffer, RetArray, len);

    //            //DataReceived.Raise(this, new SerialDataReceivedArgs(data));
    //            Debug.WriteLine("---------USB READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);
    //            // Debug.WriteLine("---------USB READ DATA RESPONSE TIME-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
    //            return RetArray;
    //        }

    //        return null;
    //    }
    //    public bool USB_Disconnect()
    //    {
    //        return true;
    //    }
    //    public bool Wifi_Disconnect()
    //    {
    //        return true;
    //    }

    //    #endregion

    //    #region Helper
    //    //Convert a string of hex digits (example: E1 FF 1B) to a byte array. 
    //    //The string containing the hex digits (with or without spaces)
    //    //Returns an array of bytes. </returns>
    //    //private byte[] HexStringToByteArray(string s)
    //    //{
    //    //    s = s.Replace(" ", "");
    //    //    byte[] buffer = new byte[s.Length / 2];
    //    //    for (int i = 0; i < s.Length; i += 2)
    //    //        buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
    //    //    return buffer;
    //    //}
    //    static byte[] HexToBytes(string input)
    //    {
    //        byte[] result = new byte[input.Length / 2];
    //        for (int i = 0; i < result.Length; i++)
    //        {
    //            result[i] = Convert.ToByte(input.Substring(2 * i, 2), 16);
    //        }
    //        return result;
    //    }

    //    private void WriteConsole(string input, string output)
    //    {
    //        Debug.WriteLine("Command = " + input + "\n" + "Output = " + output, DebugTag);
    //    }

    //    private byte[] HexStringToByteArray(String hex)
    //    {
    //        hex = hex.Replace(" ", "");
    //        int numberChars = hex.Length;
    //        if (numberChars % 2 != 0)
    //        {
    //            hex = "0" + hex;
    //            numberChars++;
    //        }
    //        byte[] bytes = new byte[numberChars / 2];
    //        for (int i = 0; i < numberChars; i += 2)
    //            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
    //        return bytes;
    //    }
    //    //----------------------------------------------------------------------------
    //    // Method Name   : ByteArrayToString
    //    // Input         : byte array
    //    // Output        : string
    //    // Purpose       : Function to convert byte array to string 
    //    // Date          : 27Sept16
    //    //----------------------------------------------------------------------------
    //    private string ByteArrayToString(byte[] ba)
    //    {
    //        string hex = BitConverter.ToString(ba);
    //        return hex.Replace("-", "");
    //    }
    //    //----------------------------------------------------------------------------
    //    // Method Name   : BT_GetDevices
    //    // Input         : NA
    //    // Output        : Collection of Bluetoothdevices with their status,name, address.
    //    // Purpose       : Get list of all devices with a specific BT/BLE name and with status paired and unpaired
    //    // Date          : 20-08-20
    //    //----------------------------------------------------------------------------

    //    #endregion

    //    #region WifiServices
    //    private object Wifi_GetDevices()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    //private object Wifi_ConnectAP()
    //    //{
    //    //    tcpClient.StringEncoder = Encoding.UTF8;
    //    //    tcpClient.DataReceived += TcpClient_DataReceived;

    //    //    var clientStatus = tcpClient.Connect("Host", Convert.ToInt32("Port"));
    //    //    if (clientStatus.TcpClient.Connected == true)
    //    //    {
    //    //        ActiveConnection = Wifi;
    //    //        return true;
    //    //    }
    //    //    return false;
    //    //}

    //    private object Wifi_WriteSSIDPW()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private object Wifi_ConnectStation()
    //    {
    //        throw new NotImplementedException();
    //    }
    //    #endregion

    //    #region DongleServices

    //    public async Task<object> SecurityAccess()
    //    {
    //        Debug.WriteLine("------SecurityAccess------", DebugTag);

    //        string command = "500C47568AFE56214E238000FFC3";
    //        var bytesCommand = HexStringToByteArray(command);

    //        //SerialPortDataReceived(this, null);
    //        var response = await SendCommand(bytesCommand, (obj) => { WriteConsole(command, obj); }); ;

    //        return response;
    //    }
    //    //----------------------------------------------------------------------------
    //    // Method Name   : Dongle_Reset
    //    // Input         : NA
    //    // Output        : object
    //    // Purpose       : Reset all vehicle Communication related parameters to its default value
    //    // Date          : 20-08-20
    //    //----------------------------------------------------------------------------
    //    public async Task<object> Dongle_Reset()
    //    {
    //        Debug.WriteLine("------Inside Dongle_Reset------", DebugTag);
    //        //"0x20-0x01-0x01"
    //        string command = "200301";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        var response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    //----------------------------------------------------------------------------
    //    // Method Name   : Dongle_SetProtocol
    //    // Input         : NA
    //    // Output        : object
    //    // Purpose       : Set Vehicle Communication Protocol
    //    // Date          : 20-08-20
    //    //----------------------------------------------------------------------------
    //    public async Task<object> Dongle_SetProtocol(int protocolVersion)
    //    {
    //        object response = null;
    //        //   "0x20-0x02-0x02-0x < protocol > "

    //        Debug.WriteLine("------Dongle_SetProtocol------", DebugTag);
    //        string command = "200402" + protocolVersion.ToString("D2");
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    //----------------------------------------------------------------------------
    //    // Method Name   : Dongle_GetProtocol
    //    // Input         : NA
    //    // Output        : object
    //    // Purpose       : Get Vehicle Communication Protocol
    //    // Date          : 20-08-20
    //    //----------------------------------------------------------------------------
    //    public async Task<object> Dongle_GetProtocol()
    //    {
    //        Debug.WriteLine("------Dongle_GetProtocol------", DebugTag);
    //        object response = null;
    //        //"0x20-0x01-0x03"

    //        string command = "200303";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    //----------------------------------------------------------------------------
    //    // Method Name   : Dongle_GetFimrwareVersion
    //    // Input         : NA
    //    // Output        : object
    //    // Purpose       : Get firmware version
    //    // Date          : 20-08-20
    //    //----------------------------------------------------------------------------
    //    public async Task<object> Dongle_GetFimrwareVersion()
    //    {
    //        Debug.WriteLine("------Dongle_GetFimrwareVersion------", DebugTag);
    //        object response = null;
    //        string command = "200314";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }


    //    #endregion

    //    #region CANMethods
    //    public async Task<object> CAN_SetTxHeader(string txHeader)
    //    {
    //        Debug.WriteLine("------CAN_SetTxHeader------", DebugTag);
    //        //            "if protocol is one of the following: ISO15765-250KB-11BIT-CAN, ISO15765-500KB-11BIT-CAN, ISO15765-1MB-11BIT-CAN, 250KB-11BIT-CAN, 500KB-11BIT-CAN, 1MB-11BIT-CAN, OE-IVN-250KBPS-11BIT-CAN, OE-IVN-500KBPS-11BIT-CAN,OE-IVN-1MBPS-11BIT-CAN 
    //        //0x20-0x03-0x04-0xxx-0xyy

    //        //if protocol is one of the following: ISO15765 - 250KB - 29BIT - CAN, ISO15765 - 500KB - 29BIT - CAN, ISO15765 - 1MB - 29BIT - CAN, 250KB - 29BIT - CAN, 500KB - 29BIT - CAN, 1MB - 29BIT - CAN, OE - IVN - 250KBPS - 29BIT - CAN, OE - IVN - 500KBPS - 29BIT - CAN,OE - IVN - 1MBPS - 29BIT - CAN
    //        //0x20-0x05-0x04-0xpp-0xqq-0xrr-0xss"

    //        object response = null;
    //        string command = string.Empty;
    //        byte[] sendBytes = null;
    //        if (protocol == Protocol.ISO15765_250KB_11BIT_CAN || protocol == Protocol.ISO15765_500KB_11BIT_CAN || protocol == Protocol.ISO15765_1MB_11BIT_CAN || protocol == Protocol.I250KB_11BIT_CAN || protocol == Protocol.I500KB_11BIT_CAN || protocol == Protocol.I1MB_11BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_11BIT_CAN)
    //        {
    //            command = "200504" + txHeader.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            sendBytes = HexStringToByteArray(command + crc);
    //        }
    //        else if (protocol == Protocol.ISO15765_250Kb_29BIT_CAN || protocol == Protocol.ISO15765_500KB_29BIT_CAN || protocol == Protocol.ISO15765_1MB_29BIT_CAN || protocol == Protocol.I250Kb_29BIT_CAN || protocol == Protocol.I500KB_29BIT_CAN || protocol == Protocol.I1MB_29BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_29BIT_CAN)
    //        {
    //            command = "200704" + txHeader.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4], bytesCommand[5], bytesCommand[6]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            sendBytes = HexStringToByteArray(command + crc);
    //        }
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

    //        return response;
    //    }

    //    public async Task<object> CAN_GetTxHeader()
    //    {
    //        Debug.WriteLine("------Dongle_SetProtocol------", DebugTag);
    //        object response = null;
    //        //"0x20-0x01-0x05"
    //        string command = "200305";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_SetRxHeaderMask(string rxhdrmsk)
    //    {
    //        Debug.WriteLine("------CAN_SetRxHeaderMask------", DebugTag);
    //        object response = null;
    //        //            "if protocol is one of the following: ISO15765-250KB-11BIT-CAN, ISO15765-500KB-11BIT-CAN, ISO15765-1MB-11BIT-CAN, 250KB-11BIT-CAN, 500KB-11BIT-CAN, 1MB-11BIT-CAN, OE-IVN-250KBPS-11BIT-CAN, OE-IVN-500KBPS-11BIT-CAN,OE-IVN-1MBPS-11BIT-CAN 
    //        //0x20-0x03-0x06-0xxx-0xyy

    //        //if protocol is one of the following: ISO15765 - 250KB - 29BIT - CAN, ISO15765 - 500KB - 29BIT - CAN, ISO15765 - 1MB - 29BIT - CAN, 250KB - 29BIT - CAN, 500KB - 29BIT - CAN, 1MB - 29BIT - CAN, OE - IVN - 250KBPS - 29BIT - CAN, OE - IVN - 500KBPS - 29BIT - CAN,OE - IVN - 1MBPS - 29BIT - CAN
    //        //0x20-0x05-0x06-0xpp-0xqq-0xrr-0xss"

    //        string command = string.Empty;
    //        byte[] sendBytes = null;
    //        if (protocol == Protocol.ISO15765_250KB_11BIT_CAN || protocol == Protocol.ISO15765_500KB_11BIT_CAN || protocol == Protocol.ISO15765_1MB_11BIT_CAN || protocol == Protocol.I250KB_11BIT_CAN || protocol == Protocol.I500KB_11BIT_CAN || protocol == Protocol.I1MB_11BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_11BIT_CAN)
    //        {
    //            command = "200506" + rxhdrmsk.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            sendBytes = HexStringToByteArray(command + crc);
    //        }
    //        else if (protocol == Protocol.ISO15765_250Kb_29BIT_CAN || protocol == Protocol.ISO15765_500KB_29BIT_CAN || protocol == Protocol.ISO15765_1MB_29BIT_CAN || protocol == Protocol.I250Kb_29BIT_CAN || protocol == Protocol.I500KB_29BIT_CAN || protocol == Protocol.I1MB_29BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_29BIT_CAN)
    //        {
    //            command = "200706" + rxhdrmsk.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4], bytesCommand[5], bytesCommand[6]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            sendBytes = HexStringToByteArray(command + crc);
    //        }
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;

    //    }

    //    public async Task<object> CAN_GetRxHeaderMask()
    //    {
    //        Debug.WriteLine("------CAN_GetRxHeaderMask------", DebugTag);
    //        object response = null;
    //        //"0x20-0x01-0x07"

    //        string command = "200307";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_SetP1Min(string p1min)
    //    {
    //        Debug.WriteLine("------CAN_GetRxHeaderMask------", DebugTag);
    //        object response = null;
    //        //"0x20-0x02-0x0c-0x < xx > "

    //        string command = "20040c" + p1min.ToString();
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_GetP1Min()
    //    {
    //        Debug.WriteLine("------CAN_GetP1Min------", DebugTag);
    //        object response = null;
    //        //"0x20-0x01-0x0d"

    //        string command = "20030d";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_SetP2Max(string p2max)
    //    {
    //        Debug.WriteLine("------CAN_SetP2Max------", DebugTag);
    //        object response = null;
    //        //"0x20-0x03-0x0e-0x < xx >-0x < yy > "

    //        string command = "20050e" + p2max.ToString();
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_GetP2Max()
    //    {
    //        Debug.WriteLine("------CAN_GetP2Max------", DebugTag);
    //        object response = null;
    //        //"0x20-0x01-0x0f"

    //        string command = "20030f";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);

    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_StartTP()
    //    {
    //        Debug.WriteLine("------CAN_StartTP------", DebugTag);
    //        object response = null;
    //        //"0x20-0x01-0x10"

    //        string command = "200310";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_StopTP()
    //    {
    //        Debug.WriteLine("------CAN_StopTP------", DebugTag);
    //        object response = null;
    //        //"0x20,0x01, 0x11"

    //        string command = "200311";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_StartPadding(string paddingByte)
    //    {
    //        Debug.WriteLine("------CAN_StartPadding------", DebugTag);
    //        object response = null;
    //        //"0x20,0x02,0x12, 0x < xx > "

    //        string command = "200412" + paddingByte.ToString();
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;

    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_StopPadding()
    //    {
    //        Debug.WriteLine("------CAN_StopPadding------", DebugTag);
    //        object response = null;
    //        //"0x20- 0x01-0x13"

    //        string command = "200313";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //            crc = "0" + crc;

    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> CAN_TxData(string txdata)
    //    {
    //        Debug.WriteLine("------CAN_StartPadding------", DebugTag);
    //        object response = null;
    //        ////data requests are of 2 types. If length of message <1000> then use 4x command, if not, use 1x command
    //        //0x4 < l >
    //        //0x < ll >
    //        string command = "40" + txdata.ToString();
    //        var bytesCommand = HexStringToByteArray(command);
    //        var crcBytesComputation = HexStringToByteArray(txdata);
    //        string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("x2");
    //        if (crc.Length == 3)
    //        {
    //            crc = "0" + crc;
    //        }
    //        byte[] sendBytes = HexStringToByteArray(command + crc);

    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;

    //    }

    //    //public async Task<object> CAN_RxData()
    //    //{
    //    //    throw new NotImplementedException();
    //    //}

    //    public async Task<ResponseArrayStatus> CAN_TxRx(int framelength, string txdata)
    //    {
    //        Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
    //        object response = null;
    //        int dataLength = framelength + 2; //crc
    //        string command = string.Empty;

    //        var firstbyte = 0x40 | ((dataLength >> 8) & 0x0f);
    //        var secondbyte = dataLength & 0xff;

    //        command = firstbyte.ToString("X2") + secondbyte.ToString("X2") + txdata.ToString();
    //        var bytesCommand = HexStringToByteArray(command);
    //        var crcBytesComputation = HexStringToByteArray(txdata);
    //        string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("X2");
    //        if (crc.Length == 3)
    //        {
    //            crc = "0" + crc;
    //        }
    //        if (crc.Length == 2)
    //        {
    //            crc = "00" + crc;
    //        }
    //        if (crc.Length == 1)
    //        {
    //            crc = "000" + crc;
    //        }
    //        Debug.WriteLine("CRC =" + crc, DebugTag);
    //        byte[] sendBytes = HexStringToByteArray(command + crc);

    //        UInt16 nooftimessent = 0;

    //    STARTOVERAGAIN:
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        nooftimessent++;

    //        if (Convert.ToString(response).Contains("dhcps: send_offer>>udp_sendto result 0") == true)
    //        {
    //            Debug.WriteLine("--------- stripping off unwanted characters from response dhcps: send_offer >> udp_sendto result 0------------==" + "ELMZ");
    //            string ValueToReplace = "dhcps: send_offer>>udp_sendto result 0";
    //            response = (string)response.ToString().Replace(ValueToReplace, "");
    //            //var ecuResponseBytes = (byte[])response;
    //        }

    //        if (response != null)
    //        {
    //            var ecuResponseBytes = (byte[])response;

    //            ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);

    //            if (dataStatus == "SENDAGAIN")
    //            {
    //                while (dataStatus == "SENDAGAIN")
    //                {
    //                    Debug.WriteLine("------SENDAGAIN ------" + Convert.ToString(nooftimessent), DebugTag);

    //                    if (nooftimessent <= 5)
    //                    {
    //                        goto STARTOVERAGAIN;
    //                    }
    //                    else
    //                    {
    //                        /* stop sending again - Problem with the device */
    //                        responseStructure = new ResponseArrayStatus
    //                        {
    //                            ECUResponse = ecuResponseBytes,
    //                            ECUResponseStatus = "DONGLEERROR_SENDAGAINTHRESHOLDCROSSED",
    //                            ActualDataBytes = actualDataBytes
    //                        };
    //                        break;
    //                    }

    //                    //response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //                    //if (response != null)
    //                    //{
    //                    //    var ecuResponseBytes2 = (byte[])response;
    //                    //    ResponseArrayDecoding.CheckResponse(ecuResponseBytes2, sendBytes, out byte[] actualDataBytes2, out string dataStatus2);
    //                    //    dataStatus = dataStatus2;
    //                    //    responseStructure = new ResponseArrayStatus
    //                    //    {
    //                    //        ECUResponse = ecuResponseBytes2,
    //                    //        ECUResponseStatus = dataStatus2,
    //                    //        ActualDataBytes = actualDataBytes2
    //                    //    };
    //                    //    Debug.WriteLine("------SENDAGAIN DATA START ------", DebugTag);
    //                    //    if (responseStructure?.ECUResponse != null)
    //                    //        Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseBytes2), DebugTag);
    //                    //    if (responseStructure?.ActualDataBytes != null)
    //                    //        Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                    //    Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                    //    Debug.WriteLine("------SENDAGAIN DATA END ------", DebugTag);
    //                    //}
    //                }
    //            }
    //            else if (dataStatus == "READAGAIN")
    //            {
    //                while (dataStatus == "READAGAIN")
    //                {
    //                    var responseReadAgain = await ReadData();
    //                    var ecuResponseReadBytes = (byte[])responseReadAgain;
    //                    ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
    //                    dataStatus = dataReadStatus;
    //                    responseStructure = new ResponseArrayStatus
    //                    {
    //                        ECUResponse = ecuResponseReadBytes,
    //                        ECUResponseStatus = dataReadStatus,
    //                        ActualDataBytes = actualReadBytes
    //                    };
    //                    Debug.WriteLine("------EXTRA READ DATA START ------", DebugTag);
    //                    if (responseStructure?.ECUResponse != null)
    //                        Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseReadBytes), DebugTag);
    //                    if (responseStructure?.ActualDataBytes != null)
    //                        Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                    Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                    Debug.WriteLine("------EXTRA READ DATA END ------", DebugTag);
    //                }
    //            }
    //            else
    //            {
    //                responseStructure = new ResponseArrayStatus
    //                {
    //                    ECUResponse = ecuResponseBytes,
    //                    ECUResponseStatus = dataStatus,
    //                    ActualDataBytes = actualDataBytes
    //                };
    //                Debug.WriteLine("------ECU RESPONE START ------", DebugTag);
    //                if (responseStructure?.ECUResponse != null)
    //                    Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(responseStructure?.ECUResponse), DebugTag);
    //                if (responseStructure?.ActualDataBytes != null)
    //                    Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                Debug.WriteLine("------ECU RESPONE END ------", DebugTag);
    //            }
    //        }
    //        return responseStructure;

    //    }

    //    //public async Task<ResponseArrayStatus> CAN_TxRx(int framelength, string txdata)
    //    //{
    //    //    Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
    //    //    object response = null;
    //    //    int dataLength = framelength + 2;
    //    //    string command = "40" + dataLength.ToString("X2") + txdata.ToString();
    //    //    var bytesCommand = HexStringToByteArray(command);
    //    //    var crcBytesComputation = HexStringToByteArray(txdata);
    //    //    string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("X2");
    //    //    if (crc.Length == 3)
    //    //    {
    //    //        crc = "0" + crc;
    //    //    }
    //    //    byte[] sendBytes = HexStringToByteArray(command + crc);

    //    //    response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

    //    //    if (response != null)
    //    //    {
    //    //        var ecuResponseBytes = (byte[])response;
    //    //        ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);
    //    //        if (dataStatus == "READAGAIN")
    //    //            while (dataStatus == "READAGAIN")
    //    //            {
    //    //                var responseReadAgain = await ReadData();
    //    //                var ecuResponseReadBytes = (byte[])responseReadAgain;
    //    //                ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
    //    //                dataStatus = dataReadStatus;
    //    //                responseStructure = new ResponseArrayStatus
    //    //                {
    //    //                    ECUResponse = ecuResponseReadBytes,
    //    //                    ECUResponseStatus = dataReadStatus,
    //    //                    ActualDataBytes = actualReadBytes


    //    //                };
    //    //                Debug.WriteLine("------READ DATA START ------", DebugTag);
    //    //                if (responseStructure?.ECUResponse != null)
    //    //                    Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseReadBytes), DebugTag);
    //    //                if (responseStructure?.ActualDataBytes != null)
    //    //                    Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //    //                Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //    //                Debug.WriteLine("------READ DATA END ------", DebugTag);
    //    //            }
    //    //        else
    //    //        {

    //    //            responseStructure = new ResponseArrayStatus
    //    //            {
    //    //                ECUResponse = ecuResponseBytes,
    //    //                ECUResponseStatus = dataStatus,
    //    //                ActualDataBytes = actualDataBytes
    //    //            };
    //    //            Debug.WriteLine("------ECU RESPONE WITHOUT READ START ------", DebugTag);
    //    //            if (responseStructure?.ECUResponse != null)
    //    //                Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(responseStructure?.ECUResponse), DebugTag);
    //    //            if (responseStructure?.ActualDataBytes != null)
    //    //                Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //    //            Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //    //            Debug.WriteLine("------ECU RESPONE WITHOUT READ END ------", DebugTag);

    //    //        }
    //    //    }

    //    //    return responseStructure;

    //    //}

    //    //public async Task<object> CAN_TxRx(int frameLength, string txdata)
    //    //{
    //    //    Debug.WriteLine("------CAN_StartPadding------", DebugTag);
    //    //    object response = null;

    //    //    string command = "40" + txdata.ToString();
    //    //    var bytesCommand = HexStringToByteArray(command);
    //    //    string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
    //    //    byte[] sendBytes = HexStringToByteArray(command + crc);

    //    //    response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); }); ;

    //    //    return response;

    //    //}   

    //    public async Task<object> SetBlkSeqCntr(string blklen)
    //    {
    //        Debug.WriteLine("------SetBlkSeqCntr------", DebugTag);
    //        object response = null;
    //        //"0x20- 0x02 - 0x08- 0xxx(0x00 to 0x40)"

    //        string command = "200408" + blklen.ToString();
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //        if (crc.Length == 3)
    //        {
    //            crc = "0" + crc;
    //        }
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> GetBlkSeqCntr()
    //    {
    //        Debug.WriteLine("------GetBlkSeqCntr------", DebugTag);
    //        object response = null;
    //        //"0x20- 0x01-0x09"
    //        string command = "200309";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //        {
    //            crc = "0" + crc;
    //        }
    //        byte[] sendBytes = HexStringToByteArray(command + crc);

    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> SetSepTime(string septime)
    //    {
    //        Debug.WriteLine("------SetSepTime------", DebugTag);
    //        object response = null;
    //        //"0x20-0x02-0x0A-0xxx"

    //        string command = "20040A" + septime.ToString();
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //        if (crc.Length == 3)
    //        {
    //            crc = "0" + crc;
    //        }
    //        byte[] sendBytes = HexStringToByteArray(command + crc);

    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //        return response;
    //    }

    //    public async Task<object> GetSepTime()
    //    {
    //        Debug.WriteLine("------GetSepTime------", DebugTag);
    //        object response = null;
    //        //"0x20-0x01-0x0B"
    //        string command = "20030B";
    //        var bytesCommand = HexStringToByteArray(command);
    //        string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //        if (crc.Length == 3)
    //        {
    //            crc = "0" + crc;
    //        }
    //        byte[] sendBytes = HexStringToByteArray(command + crc);
    //        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); }); ;

    //        return response;
    //    }
    //    #endregion

    //    #endregion
    //}
}
