using APDongleCommWin.ENUMS;
using APDongleCommWin.Helper;
using APDongleCommWin.Models;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
namespace APDongleCommWin
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

    #region New
    public class DongleCommWin : ICANCommands, IWifiUSBHandler, IDongleHandler
    {
        #region Properties
        TaskCompletionSource<object> tskCmSsrc = null;
        SerialPort comPort = null;

        public TcpClient TcpClient { get; }

        //SimpleTCP.SimpleTcpClient tcpClient;
        Protocol protocol;
        string DebugTag = "ELM-DEBUG";
        string ActiveConnection = null;
        string Wifi = "Wifi";
        string USB = "USB";

        private SerialDevice serialPort = null;


        private SerialDevice serialPort2 = null;
        DataWriter dataWriteObject2 = null;
        DataReader dataReaderObject2 = null;
        private ResponseArrayStatus responseStructure;
        public Platform Platform;
        public Connectivity Connectivity;
        private NetworkStream Stream = null;
        #endregion

        #region Ctor
        public void InitializePlatform(Platform platform, Connectivity connectivity)
        {
            Platform = platform;

            Connectivity = connectivity;
        }

        public DongleCommWin()
        {
            //comPort = new System.IO.Ports.SerialPort();
            //tskCmSsrc = new TaskCompletionSource<object>();
            //tcpClient = new SimpleTCP.SimpleTcpClient();
            Debug.WriteLine("Inside ELM", DebugTag);
            //BluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            //if (BluetoothAdapter == null)
            //    Debug.WriteLine("No Bluetooth adapter found.", DebugTag);
            //else
            //    Debug.WriteLine("Bluetooth Adapter found!!");

            //if (!BluetoothAdapter.IsEnabled)
            //    Debug.WriteLine("Bluetooth adapter is not enabled.", DebugTag);
            //else
            //    Debug.WriteLine("Bluetooth Adapter enabled!", DebugTag);
        }

        public DongleCommWin(TcpClient tcpClient, NetworkStream networkStream, Protocol protocolVersion, UInt32 txHeader,
            UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
        {
            Debug.WriteLine("Inside ELM327 SerialDevice CTOR", DebugTag);
            TcpClient = tcpClient;
            protocol = protocolVersion;
            ActiveConnection = Wifi;
            Stream = tcpClient.GetStream();
        }



        //public DongleCommWin(SimpleTcpClient client, Protocol protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
        //{
        //    Debug.WriteLine("Inside ELM327 SimpleTcpClient CTOR", DebugTag);
        //    tcpClient = client;
        //    protocol = protocolVersion;

        //    tcpClient.StringEncoder = Encoding.UTF8;
        //    tcpClient.DataReceived += TcpClient_DataReceived;

        //    if (tskCmSsrc == null)
        //    {
        //        tskCmSsrc = new TaskCompletionSource<object>();
        //    }

        //    ActiveConnection = Wifi;
        //}

        public DongleCommWin(SerialDevice serialDevice, int protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
        {
            Debug.WriteLine("Inside ELM327 SerialDevice CTOR", DebugTag);
            serialPort = serialDevice;
            serialPort2 = serialDevice;
            protocol = (Protocol)protocolVersion;
            ActiveConnection = USB;
        }


        public async Task<object> ReadData()
        {
            if (Platform == Platform.UWP && Connectivity == Connectivity.USB)
            {
                object response = null;

                try
                {
                    Debug.WriteLine("---------INSIDE EXTRA READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                    using (DataReader dataReader = new DataReader(serialPort.InputStream))
                    {
                        uint bytesToRead = 0;
                        bytesToRead = await dataReader.LoadAsync(1024);
                        var serialBytesReceived = new byte[bytesToRead];
                        dataReader.ReadBytes(serialBytesReceived);

                        response = serialBytesReceived;

                        if (response != null)
                        {
                            var responseBytes = (byte[])response;
                            Debug.WriteLine("EXTRA READ DATA RSPONSE = " + ByteArrayToString(responseBytes) + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                        }
                        else
                        {
                            Debug.WriteLine("ELSE EXTRA READ Data = " + response.ToString() + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                        }
                        dataReader.DetachStream();
                        dataReader.DetachBuffer();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return response;
            }
            else if (Platform == Platform.UWP && Connectivity == Connectivity.WiFi)
            {
                var wifiCommands = await GetWifiCommand(Stream);
                return wifiCommands;
            }
            return null;

        }
        //public DongleCommWin(SerialPort serialPort, Protocol protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
        //{
        //    Debug.WriteLine("Inside ELM327 SerialPort CTOR", DebugTag);
        //    comPort = serialPort;
        //    protocol = protocolVersion;

        //    if (tskCmSsrc == null)
        //    {
        //        tskCmSsrc = new TaskCompletionSource<object>();
        //    }

        //    bool error = false;

        //    try  //always try to use this try and catch method to open your port. 
        //         //if there is an error your program will not display a message instead of freezing.
        //    {
        //        //Open Port
        //        if (!comPort.IsOpen)
        //            comPort.Open();
        //        comPort.DataReceived += SerialPortDataReceived;  //Check for received data. When there is data in the receive buffer,
        //                                                         //it will raise this event, we need to subscribe to it to know when there is data
        //    }
        //    catch (UnauthorizedAccessException) { error = true; }
        //    catch (System.IO.IOException) { error = true; }
        //    catch (ArgumentException) { error = true; }

        //    if (error)
        //        Debug.WriteLine("Could not open the COM port. Most likely it is already in use, has been removed, or is unavailable. or COM Port unavailable", DebugTag);

        //    //if the port is open, Change the Connect button to disconnect, enable the send button.
        //    //and disable the groupBox to prevent changing configuration of an open port.
        //    if (comPort.IsOpen)
        //    {
        //        // return true;

        //    }

        //    //return false;
        //    ActiveConnection = USB;
        //}
        #endregion

        #region Methods


        //----------------------------------------------------------------------------
        // Method Name   : BT_ConnectDevice
        // Input         : DEVICENAME
        // Output        : status of ConnectDevice form of bool
        // Purpose       : Connect with a BT / BLE Device and generate a handle
        // Date          : 20-08-20
        //---------------------------------------------------------------------------
        private bool USB_Connect(string deviceName)
        {
            //bool error = false;

            //try  //always try to use this try and catch method to open your port. 
            //     //if there is an error your program will not display a message instead of freezing.
            //{
            //    //Open Port
            //    comPort.Open();
            //    comPort.DataReceived += SerialPortDataReceived;  //Check for received data. When there is data in the receive buffer,
            //                                                     //it will raise this event, we need to subscribe to it to know when there is data
            //}
            //catch (UnauthorizedAccessException) { error = true; }
            //catch (System.IO.IOException) { error = true; }
            //catch (ArgumentException) { error = true; }

            //if (error)
            //    Debug.WriteLine("Could not open the COM port. Most likely it is already in use, has been removed, or is unavailable. or COM Port unavailable", DebugTag);

            ////if the port is open, Change the Connect button to disconnect, enable the send button.
            ////and disable the groupBox to prevent changing configuration of an open port.
            //if (comPort.IsOpen)
            //{
            //    return true;

            //}

            return false;

        }

        #region EventsHandler
        //private void TcpClient_DataReceived(object sender, SimpleTCP.Message e)
        //{
        //    var data = e.MessageString;
        //    tskCmSsrc?.TrySetResult(e.Data);

        //    Debug.WriteLine("TcpClient_DataReceived = " + e.Data + "\n", DebugTag);
        //}

        private async void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //await Task.Delay(5000);
            var serialPort = (SerialPort)sender;
            var data = serialPort.ReadExisting();
            tskCmSsrc?.TrySetResult(data);

            Debug.WriteLine("SerialPortDataReceived = " + data + "\n", DebugTag);

        }
        #endregion

        #region SendCommand

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
        public async Task WriteBytesAsync(DataWriter dataWriteObject, byte[] bytesTosend)
        {
            dataWriteObject.WriteBytes(bytesTosend);
            var bytesWritten = await dataWriteObject.StoreAsync();
            Debug.WriteLine("BytesWritten =  " + bytesWritten.ToString(), DebugTag);
            //Task<UInt32> storeAsyncTask;
            //int offset = 0;
            //do
            //{
            //    //var ftt = HexStringToByteArray("43B43601000000000000000000000000000000005555AAAA00000000000000000000000000000000000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF000000000000000020FE60FFA000E001200360040005A0054006800700400040004000400040004000400040004000402003B00440060807D00798086009F00A800C100EF100830113025F02A602ED023303CB034704BE040008000A000CCC0D001000140018001C001E00207805780578057805780578057805780578057805EE02EE02EE02EE02EE02EE02EE02EE02EE02EE02C201C201C201C201C201C201C201C201C201C20100000000000000000000000000000000000000006AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF4006D0076009F00A800CA00FC012E0157017401F20FE60FFA000E001200360040005A00540068007C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C8002C012C012C012C012C012C012C012C012C012C01DC01DC01DC01DC01DC01DC01DC01DC01DC01DC0158025802580258025802580258025802580258029C029C029C029C029C029C029C029C029C029C02250325032503250325032503250325032503250384038403840384038403840384038403840384038403840384038403840384038403840384038403002000240028002C003000340038003A003C00400040083F083B2937CD340E2F73279822971C0800000090012003B0044006D0076009F00A800C480DD80E6810C0125014E0157017E803780508076009F00A800CA00F941188137C1570176419581B401F1027E02E004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040A809");
            //    //bytesTosend = ftt;
            //    int buffersize = bytesTosend.Length;
            //    //if (buffersize > 1000) { buffersize = 1000; }
            //    byte[] sendBuffer = new byte[buffersize];
            //    System.Buffer.BlockCopy(bytesTosend, offset, sendBuffer, 0, buffersize);

            //    dataWriteObject.WriteBytes(sendBuffer);

            //    var bytesWritten =  await dataWriteObject.StoreAsync();
            //    //await Task.Run(async () =>
            //    //{

            //    //    //await dataWriteObject.FlushAsync();

            //    //});

            //    offset += buffersize;
            //} while (offset < bytesTosend.Length);
        }
        public async Task<object> SendNewCommand(byte[] command, Action<string> onDataRecevied)
        {
            if (Platform == Platform.UWP && Connectivity == Connectivity.USB)
            {
                Debug.WriteLine("SENDING COMMAND " + ByteArrayToString(command), DebugTag);
                object response = null;

                try
                {
                    using (DataWriter dataWriteObject = new DataWriter(serialPort2.OutputStream))
                    {
                        await WriteBytesAsync(dataWriteObject, command);
                        if (dataWriteObject != null)
                        {
                            dataWriteObject.DetachStream();
                        }
                    }
                    if (dataReaderObject2 == null)
                        dataReaderObject2 = new DataReader(serialPort2.InputStream);
                    /* Read data in from the serial port */
                    Task<UInt32> bytesToReadh;

                    //Create task to read the bytes
                    bytesToReadh = dataReaderObject2.LoadAsync(9024).AsTask();
                    UInt32 bytesReadh = await bytesToReadh;

                    //Get the bytes from the ECU
                    var serialBytesReceivedh = new byte[bytesReadh];
                    dataReaderObject2.ReadBytes(serialBytesReceivedh);

                    response = serialBytesReceivedh;

                    if (serialBytesReceivedh.Length != 0)
                    {
                        var stringResponse = ByteArrayToString(serialBytesReceivedh);
                        Debug.WriteLine("Response Received =  " + stringResponse + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                    }
                    return response;

                }

                catch (Exception ex)
                {
                    if (ex.GetType().Name == "TaskCanceledException")
                    {
                        Debug.WriteLine("EXCEPTION Received =  " + ex.StackTrace, DebugTag);
                        var excetion = ex.Message;
                        USB_Disconnect();
                    }

                    else
                    {

                    }
                }
                return response;
            }


            return null;

            //if (comPort.IsOpen)
            //{
            //    bool error = false;
            //    if (command != null)        //if text mode is selected, send data as tex
            //    {
            //        // Send the user's text straight out the port 
            //        //ComPort.Write(command);


            //    }
            //    else                    //if Hex mode is selected, send data in hexadecimal
            //    {
            //        try
            //        {
            //            // Convert the user's string of hex digits (example: E1 FF 1B) to a byte array
            //            byte[] data = HexStringToByteArray(command.ToString());

            //            // Send the binary data out the port
            //            comPort.Write(data, 0, data.Length);
            //        }
            //        catch (FormatException) { error = true; }

            //        // Inform the user if the hex string was not properly formatted
            //        catch (ArgumentException) { error = true; }

            //        //if (error) MessageBox.Show(this, "Not properly formatted hex string: " + txtSend.Text + "\n" + "example: E1 FF 1B", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //        Debug.WriteLine("Not properly formatted hex string: " + command + "\n" + "example: E1 FF 1B Format Error", DebugTag);

            //    }
            //}

            //if (ActiveConnection == Wifi)
            //    if (tcpClient.TcpClient.Connected)
            //    {
            //        byte[] data = HexStringToByteArray(command.ToString());

            //        // Send the binary data out the port
            //        tcpClient.Write(data);
            //    }
        }
        //public async Task<byte[]> GetWifiCommand()
        //{
        //    byte[] rbuffer = new byte[1024];
        //    byte[] RetArray = new byte[] { };
        //    byte[] ActualBytes = new byte[] { };

        //    Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
        //    int readByte = await TcpClient.GetStream().ReadAsync(rbuffer, 0, rbuffer.Length);

        //    RetArray = new byte[readByte];
        //    Array.Copy(rbuffer, 0, RetArray, 0, readByte);


        //    //FF0007003ECD820000000040057F19787ED10B54FF0107003ECE4E0000000041055902FF050400502269005006270050008700500088005000870050080600500001005000010050048000AF226A0023065000AF019300AF13000023061500AF161500500617005006160050065500AF05010050011800AF048700AF0219005003800023212200AF212700AF2135005002010050062D00500261005002620050061A0050061A0050061A00500001005000010050062F0050062F0050062F00500336005003350050068B00500601005006070050060700AF0607005006070050060700500607005006070050060700500607005006070050060D0050061C0050062B0050062B0050062B0050062B0050062B0050062B0050019100500191005006060023ED502406

        //    Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);

        //    var byteLength = RetArray.Length.ToString("d2");
        //    string hexValue = RetArray[1].ToString("D2") + RetArray[2].ToString("D2");
        //    int decValue = Convert.ToInt32(hexValue, 16);

        //    Array.Resize(ref ActualBytes, decValue);
        //    Array.Copy(RetArray, 11, ActualBytes, 0, decValue);

        //    if (RetArray.Length > ActualBytes.Length+12+2)
        //    {

        //        string hexValue2 = RetArray[ActualBytes.Length+2+11+1].ToString("D2") + RetArray[ActualBytes.Length+2+11+2].ToString("D2");
        //        int decValue2 = Convert.ToInt32(hexValue2, 16);
        //        var tempArray = ActualBytes;
        //        Array.Resize(ref ActualBytes, decValue2);
        //        Array.Copy(RetArray, 11+ tempArray.Length+11, ActualBytes, 0, decValue2);
        //    }
        //    Debug.WriteLine("--------- DATA RESPONSE-------" + ByteArrayToString(ActualBytes), DebugTag);
        //    return ActualBytes;
        //}

        public async Task<byte[]> GetWifiCommand(NetworkStream stream)
        {
            try
            {
                byte[] ActualBytes = new byte[] { };
                await Task.Run(async () =>
                {
                    //TimeSpan timeout = ;
                    byte[] rbuffer = new byte[1024];
                    byte[] RetArray = new byte[] { };

                    //int bytesRead = -5;
                    Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                    //Task<int> readByte = TcpClient.GetStream().BeginRead(rbuffer,0, rbuffer.Length,null,null);//BeginRead(rbuffer, 0, rbuffer.Length);


                    int bytesRead = await stream.ReadAsync(rbuffer, 0, rbuffer.Length);
                    //await stream.FlushAsync();
                    RetArray = new byte[bytesRead];
                    Array.Copy(rbuffer, 0, RetArray, 0, bytesRead);


                    //FF0007003ECD820000000040057F19787ED10B54FF0107003ECE4E0000000041055902FF050400502269005006270050008700500088005000870050080600500001005000010050048000AF226A0023065000AF019300AF13000023061500AF161500500617005006160050065500AF05010050011800AF048700AF0219005003800023212200AF212700AF2135005002010050062D00500261005002620050061A0050061A0050061A00500001005000010050062F0050062F0050062F00500336005003350050068B00500601005006070050060700AF0607005006070050060700500607005006070050060700500607005006070050060D0050061C0050062B0050062B0050062B0050062B0050062B0050062B0050019100500191005006060023ED502406

                    Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);

                    //var byteLength = RetArray.Length.ToString("d2");
                    //string hexValue = RetArray[1].ToString("D2") + RetArray[2].ToString("D2");
                    //int decValue = Convert.ToInt32(hexValue, 16);

                    //Array.Resize(ref ActualBytes, decValue);
                    //Array.Copy(RetArray, 11, ActualBytes, 0, decValue);

                    //if (RetArray.Length > ActualBytes.Length + 12 + 2)
                    //{

                    //    string hexValue2 = RetArray[ActualBytes.Length + 2 + 11 + 1].ToString("D2") + RetArray[ActualBytes.Length + 2 + 11 + 2].ToString("D2");
                    //    int decValue2 = Convert.ToInt32(hexValue2, 16);
                    //    var tempArray = ActualBytes;
                    //    Array.Resize(ref ActualBytes, decValue2);
                    //    Array.Copy(RetArray, 11 + tempArray.Length + 11, ActualBytes, 0, decValue2);
                    //}
                    //Debug.WriteLine("--------- DATA RESPONSE-------" + ByteArrayToString(ActualBytes), DebugTag);
                    //return ActualBytes;

                    UInt16 wifistartindex = 0;
                    UInt16 actualdatastartindex = 0; // first index where to read the actual data from after wifi header
                    UInt16 decValue = 0;
                    UInt16 updateddecValue = 0;

                    while (wifistartindex < bytesRead)
                    {
                        //var byteLength = RetArray.Length.ToString("d4");
                        //string hexValue = RetArray[wifistartindex+1].ToString() + RetArray[wifistartindex+2].ToString();
                        decValue = (UInt16)((((UInt16)RetArray[wifistartindex + 1]) << 8) + ((UInt16)RetArray[wifistartindex + 2]));

                        updateddecValue += decValue;

                        Array.Resize(ref ActualBytes, updateddecValue);
                        Array.Copy(RetArray, wifistartindex + 11, ActualBytes, actualdatastartindex, decValue);
                        wifistartindex += (UInt16)(13 + decValue);
                        actualdatastartindex += decValue;
                    }


                    Debug.WriteLine("--------- DATA RESPONSE-------" + ByteArrayToString(ActualBytes), DebugTag);
                });
                return ActualBytes;

            }
            catch (Exception ex)
            {
                return null;
            }

        }
        //public async Task<byte[]> GetWifiCommand()
        //{
        //    try
        //    {
        //        byte[] rbuffer = new byte[1024];
        //        byte[] RetArray = new byte[] { };
        //        byte[] ActualBytes = new byte[] { };

        //        Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

        //        //int readByte = await Stream.ReadAsync(rbuffer, 0, rbuffer.Length);
        //        int readByte = await TcpClient.GetStream().ReadAsync(rbuffer, 0, rbuffer.Length);

        //        RetArray = new byte[readByte];
        //        Array.Copy(rbuffer, 0, RetArray, 0, readByte);

        //        Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);

        //        UInt16 wifistartindex = 0;
        //        UInt16 actualdatastartindex = 0; // first index where to read the actual data from after wifi header
        //        UInt16 decValue = 0;
        //        UInt16 updateddecValue = 0;

        //        while (wifistartindex < readByte)
        //        {
        //            //var byteLength = RetArray.Length.ToString("d4");
        //            //string hexValue = RetArray[wifistartindex+1].ToString() + RetArray[wifistartindex+2].ToString();
        //            decValue = (UInt16)((((UInt16)RetArray[wifistartindex + 1]) << 8) + ((UInt16)RetArray[wifistartindex + 2]));

        //            updateddecValue += decValue;

        //            Array.Resize(ref ActualBytes, updateddecValue);
        //            Array.Copy(RetArray, wifistartindex + 11, ActualBytes, actualdatastartindex, decValue);
        //            wifistartindex += (UInt16)(13 + decValue);
        //            actualdatastartindex += decValue;
        //        }


        //        Debug.WriteLine("--------- DATA RESPONSE-------" + ByteArrayToString(ActualBytes), DebugTag);
        //        return ActualBytes;
        //    }
        //    catch (Exception ex)
        //    {

        //        return null;
        //    }
        //}

        public async Task<object> SendCommand(byte[] command, Action<string> onDataRecevied)
        {
            if (Platform == Platform.UWP && Connectivity == Connectivity.USB)
            {
                Debug.WriteLine("SENDING COMMAND " + ByteArrayToString(command), DebugTag);
                Debug.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                object response = null;

                try
                {
                    //// Create the data writer object backed by the in-memory stream.

                    using (DataWriter dataWriteObject = new DataWriter(serialPort.OutputStream))
                    {
                        DeviceInformationCollection devices = null;
                        DeviceInformation deviceInfo = null;
                        var deviceSelector = SerialDevice.GetDeviceSelector();
                        devices = await DeviceInformation.FindAllAsync(deviceSelector);
                        deviceInfo = devices.FirstOrDefault(x => x.Name.Contains("Silicon Labs CP210x"));
                        if (deviceInfo != null)
                        {

                        }

                        await WriteBytesAsync(dataWriteObject, command);
                        if (dataWriteObject != null)
                        {
                            dataWriteObject.DetachStream();
                            dataWriteObject.DetachBuffer();
                        }
                    }
                    using (DataReader dataReaderObject = new DataReader(serialPort.InputStream))
                    {

                        /* Read data in from the serial port */
                        Task<UInt32> bytesToReadh;

                        //Create task to read the bytes
                        bytesToReadh = dataReaderObject.LoadAsync(9024).AsTask();
                        UInt32 bytesReadh = await bytesToReadh;

                        //Get the bytes from the ECU
                        var serialBytesReceivedh = new byte[bytesReadh];
                        dataReaderObject.ReadBytes(serialBytesReceivedh);

                        response = serialBytesReceivedh;

                        if (serialBytesReceivedh.Length != 0)
                        {
                            var stringResponse = ByteArrayToString(serialBytesReceivedh);
                            Debug.WriteLine("Response Received =  " + stringResponse + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                        }

                        dataReaderObject.DetachStream();
                        dataReaderObject.DetachBuffer();
                    }


                    return response;

                    //if (serialPort != null)
                    //{
                    //    if (dataReaderObject == null)
                    //        dataReaderObject = new DataReader(serialPort.InputStream);

                    //    if (dataWriteObject == null)
                    //        dataWriteObject = new DataWriter(serialPort.OutputStream);

                    //    //Write bytes to the ECU
                    //    dataWriteObject.WriteBytes(command);
                    //    //var storeAsyncTask = dataWriteObject.StoreAsync().GetResults();
                    //    //if (storeAsyncTask > 0)
                    //    //{
                    //    //    Debug.WriteLine("Data Written Successsfully !", DebugTag);
                    //    //}
                    //    //Check the bytes are written
                    //    uint bytesWritten = await serialPort.OutputStream.WriteAsync(dataWriteObject.DetachBuffer());

                    //    Debug.WriteLine("Command Send =  " + ByteArrayToString(command) + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                    //    try
                    //    {
                    //        /* Read data in from the serial port */
                    //        Task<UInt32> bytesToRead;

                    //        //Create task to read the bytes
                    //        bytesToRead = dataReaderObject.LoadAsync(1024).AsTask();
                    //        UInt32 bytesRead = await bytesToRead;

                    //        //Get the bytes from the ECU
                    //        var serialBytesReceived = new byte[bytesRead];
                    //        dataReaderObject.ReadBytes(serialBytesReceived);

                    //        response = serialBytesReceived;

                    //        if (serialBytesReceived.Length != 0)
                    //        {
                    //            var stringResponse = ByteArrayToString(serialBytesReceived);
                    //            Debug.WriteLine("Response Received =  " + stringResponse + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Debug.WriteLine("EXCEPTION Received =  " + ex.StackTrace, DebugTag);
                    //        var excetion = ex.Message;
                    //    }
                    //    finally
                    //    {

                    //        //dataWriteObject.Dispose();


                    //        //dataWriteObject = null;


                    //        ////dataReaderObject.Dispose();
                    //        //dataReaderObject = null;


                    //    }
                    //}

                    //else
                    //{

                    //    Debug.WriteLine("Length = 0", DebugTag);
                    //}

                }

                catch (Exception ex)
                {
                    if (ex.GetType().Name == "TaskCanceledException")
                    {
                        Debug.WriteLine("EXCEPTION Received =  " + ex.StackTrace, DebugTag);
                        var excetion = ex.Message;
                        USB_Disconnect();
                    }

                    else
                    {

                    }
                }
                return response;
            }

            else if (Platform == Platform.UWP && Connectivity == Connectivity.WiFi)
            {
                try
                {
                    Debug.WriteLine("Command WiFi Payload =  " + ByteArrayToString(command), DebugTag);

                    //00 000e 00000000 00000000 500c47568afe56214e238000ffc3 ce2c

                    //00 - Tx sequence counter
                    //000e - length of the command(command length + 2)
                    //00000000 - Timestamp - hhmmssmm
                    //00000000 - Reserved - always 00
                    //500c47568afe56214e238000ffc3 - actual command - payload
                    //ce2c - crc of entire length from tx counter to the payload.

                    var byte1 = "00";
                    var byte2 = command.Length.ToString("X4");
                    var byte3 = "00000000";// DateTime.Now.ToString("hhmmssff");
                    var byte4 = "00000000";// DateTime.Now.ToString("00000000");
                    var byte5 = ByteArrayToString(command);
                    var dataBytes = HexStringToByteArray(byte1 + byte2 + byte3 + byte4 + byte5);
                    //var dataBytes = HexStringToByteArray(byte5);

                    //var byte6Checksum = Crc16CcittKermit.ComputeChecksum();

                    string crc = Crc16CcittKermit.ComputeChecksum(dataBytes).ToString("X2");
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

                    var finalwificmd = ByteArrayToString(dataBytes) + crc.ToString();
                    var byteData = HexStringToByteArray(finalwificmd);
                    Debug.WriteLine("SENDING WiFi COMMAND " + finalwificmd, DebugTag);
                    //System.Net.ServicePointManager.SecurityProtocol |=SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    //System.Net.ServicePointManager.Expect100Continue = false;
                    await TcpClient.GetStream().WriteAsync(byteData, 0, byteData.Length);
                    //await TcpClient.GetStream().FlushAsync();
                    var wifiCommand = await GetWifiCommand(Stream);
                    return wifiCommand;
                }
                catch (Exception ex)
                {
                }
            }

            return null;

        }

        public bool USB_Disconnect()
        {

            //if (dataReaderObject != null)
            //{
            //    dataReaderObject.DetachStream();
            //    dataReaderObject = null;
            //}

            //if (dataWriteObject != null)
            //{
            //    dataWriteObject.DetachStream();
            //    dataWriteObject = null;
            //}

            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;


            return true;
        }

        public bool Wifi_Disconnect()
        {
            //tcpClient.Disconnect();
            return true;
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

        public async Task<object> WriteSsid(string command)
        {
            Debug.WriteLine("------SecurityAccess------", DebugTag);

            //string command = "500C47568AFE56214E238000FFC3";
            var bytesCommand = HexStringToByteArray(command);

            //SerialPortDataReceived(this, null);
            var response = await SendCommand(bytesCommand, (obj) => { WriteConsole(command, obj); }); ;

            return response;
        }

        public async Task<object> WritePassword(string command)
        {
            Debug.WriteLine("------SecurityAccess------", DebugTag);

            //string command = "500C47568AFE56214E238000FFC3";
            var bytesCommand = HexStringToByteArray(command);

            //SerialPortDataReceived(this, null);
            var response = await SendCommand(bytesCommand, (obj) => { WriteConsole(command, obj); }); ;

            return response;
        }

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
            string command = "200402" + protocolVersion.ToString("D2");
            var bytesCommand = HexStringToByteArray(command);
            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
            if (crc.Length == 3)
                crc = "0" + crc;
            byte[] sendBytes = HexStringToByteArray(command + crc);
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
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
            if (protocol == Protocol.ISO15765_250KB_11BIT_CAN || protocol == Protocol.ISO15765_500KB_11BIT_CAN || protocol == Protocol.ISO15765_1MB_11BIT_CAN || protocol == Protocol.I250KB_11BIT_CAN || protocol == Protocol.I500KB_11BIT_CAN || protocol == Protocol.I1MB_11BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_11BIT_CAN)
            {
                command = "200504" + txHeader.ToString();
                var bytesCommand = HexStringToByteArray(command);
                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
                if (crc.Length == 3)
                    crc = "0" + crc;
                sendBytes = HexStringToByteArray(command + crc);
            }
            else if (protocol == Protocol.ISO15765_250Kb_29BIT_CAN || protocol == Protocol.ISO15765_500KB_29BIT_CAN || protocol == Protocol.ISO15765_1MB_29BIT_CAN || protocol == Protocol.I250Kb_29BIT_CAN || protocol == Protocol.I500KB_29BIT_CAN || protocol == Protocol.I1MB_29BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_29BIT_CAN)
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
            if (protocol == Protocol.ISO15765_250KB_11BIT_CAN || protocol == Protocol.ISO15765_500KB_11BIT_CAN || protocol == Protocol.ISO15765_1MB_11BIT_CAN || protocol == Protocol.I250KB_11BIT_CAN || protocol == Protocol.I500KB_11BIT_CAN || protocol == Protocol.I1MB_11BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_11BIT_CAN)
            {
                command = "200506" + rxhdrmsk.ToString();
                var bytesCommand = HexStringToByteArray(command);
                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
                if (crc.Length == 3)
                    crc = "0" + crc;
                sendBytes = HexStringToByteArray(command + crc);
            }
            else if (protocol == Protocol.ISO15765_250Kb_29BIT_CAN || protocol == Protocol.ISO15765_500KB_29BIT_CAN || protocol == Protocol.ISO15765_1MB_29BIT_CAN || protocol == Protocol.I250Kb_29BIT_CAN || protocol == Protocol.I500KB_29BIT_CAN || protocol == Protocol.I1MB_29BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_29BIT_CAN)
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


        public async Task<ResponseArrayStatus> CAN_NewTxRx(int framelength, string txdata)
        {
            Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
            object response = null;
            int dataLength = framelength + 2; //crc
            string command = string.Empty;
            //if (dataLength.ToString().Length==2)
            //{
            //    //2 bytes
            //     command = "40" + dataLength.ToString("X2") + txdata.ToString();
            //}
            //else
            //{
            //    //3 bytes
            //     command = "4" + dataLength.ToString("X2") + txdata.ToString();
            //}

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
            byte[] sendBytes = HexStringToByteArray(command + crc);

            response = await SendNewCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

            if (response != null)
            {
                var ecuResponseBytes = (byte[])response;
                ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);
                if (dataStatus == "READAGAIN")
                    while (dataStatus == "READAGAIN")
                    {
                        var responseReadAgain = await ReadData();
                        var ecuResponseReadBytes = (byte[])responseReadAgain;
                        ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
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

            return responseStructure;

        }
        //public async Task<object> CAN_RxData()
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<ResponseArrayStatus> CAN_TxRx(int framelength, string txdata)
        {
            Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
            object response = null;
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
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            //await Task.Delay(500);
            nooftimessent++;

            if (Convert.ToString(response).Contains("dhcps: send_offer>>udp_sendto result 0") == true)
            {
                Debug.WriteLine("--------- stripping off unwanted characters from response dhcps: send_offer >> udp_sendto result 0------------==" + "ELMZ");
                string ValueToReplace = "dhcps: send_offer>>udp_sendto result 0";
                response = (string)response.ToString().Replace(ValueToReplace, "");
                //var ecuResponseBytes = (byte[])response;
            }

            if (response != null)
            {
                var ecuResponseBytes = (byte[])response;

                ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);

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
                        ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
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
            return responseStructure;

        }

        public async Task<ResponseArrayStatus> oldCAN_TxRx(int framelength, string txdata)
        {
            Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
            object response = null;
            int dataLength = framelength + 2; //crc
            string command = string.Empty;
            //if (dataLength.ToString().Length==2)
            //{
            //    //2 bytes
            //     command = "40" + dataLength.ToString("X2") + txdata.ToString();
            //}
            //else
            //{
            //    //3 bytes
            //     command = "4" + dataLength.ToString("X2") + txdata.ToString();
            //}

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
            else if (crc.Length == 2)
            {
                crc = "00" + crc;
            }
            else if (crc.Length == 1)
            {
                crc = "000" + crc;
            }
            Debug.WriteLine("CRC =" + crc, DebugTag);
            byte[] sendBytes = HexStringToByteArray(command + crc);

            UInt16 nooftimessent = 0;

        STARTOVERAGAIN:
            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
            nooftimessent++;

            // if the response contains this string "dhcps: send_offer>>udp_sendto result 0" strip this off the response and continue
            if (Convert.ToString(response).Contains("dhcps: send_offer>>udp_sendto result 0") == true)
            {
                Debug.WriteLine("--------- stripping off unwanted characters from response dhcps: send_offer >> udp_sendto result 0------------==" + "ELMZ");
                //(string)response.ToString().Replace(dhcps: send_offer>>udp_sendto result 0', '');
                //var ecuResponseBytes = (byte[])response;

            }


            if (response != null)
            {


                var ecuResponseBytes = (byte[])response;

                ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);

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
                        ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
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
            return responseStructure;

        }

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


        #endregion

        #endregion


    }

    public class Crc16CcittKermit
    {
        private static ushort[] table = {

    0x0000,0x1021,0x2042,0x3063,0x4084,0x50a5,0x60c6,0x70e7,

    0x8108,0x9129,0xa14a,0xb16b,0xc18c,0xd1ad,0xe1ce,0xf1ef,

    0x1231,0x0210,0x3273,0x2252,0x52b5,0x4294,0x72f7,0x62d6,

    0x9339,0x8318,0xb37b,0xa35a,0xd3bd,0xc39c,0xf3ff,0xe3de,

    0x2462,0x3443,0x0420,0x1401,0x64e6,0x74c7,0x44a4,0x5485,

    0xa56a,0xb54b,0x8528,0x9509,0xe5ee,0xf5cf,0xc5ac,0xd58d,

    0x3653,0x2672,0x1611,0x0630,0x76d7,0x66f6,0x5695,0x46b4,

    0xb75b,0xa77a,0x9719,0x8738,0xf7df,0xe7fe,0xd79d,0xc7bc,

    0x48c4,0x58e5,0x6886,0x78a7,0x0840,0x1861,0x2802,0x3823,

    0xc9cc,0xd9ed,0xe98e,0xf9af,0x8948,0x9969,0xa90a,0xb92b,

    0x5af5,0x4ad4,0x7ab7,0x6a96,0x1a71,0x0a50,0x3a33,0x2a12,

    0xdbfd,0xcbdc,0xfbbf,0xeb9e,0x9b79,0x8b58,0xbb3b,0xab1a,

    0x6ca6,0x7c87,0x4ce4,0x5cc5,0x2c22,0x3c03,0x0c60,0x1c41,

    0xedae,0xfd8f,0xcdec,0xddcd,0xad2a,0xbd0b,0x8d68,0x9d49,

    0x7e97,0x6eb6,0x5ed5,0x4ef4,0x3e13,0x2e32,0x1e51,0x0e70,

    0xff9f,0xefbe,0xdfdd,0xcffc,0xbf1b,0xaf3a,0x9f59,0x8f78,

    0x9188,0x81a9,0xb1ca,0xa1eb,0xd10c,0xc12d,0xf14e,0xe16f,

    0x1080,0x00a1,0x30c2,0x20e3,0x5004,0x4025,0x7046,0x6067,

    0x83b9,0x9398,0xa3fb,0xb3da,0xc33d,0xd31c,0xe37f,0xf35e,

    0x02b1,0x1290,0x22f3,0x32d2,0x4235,0x5214,0x6277,0x7256,

    0xb5ea,0xa5cb,0x95a8,0x8589,0xf56e,0xe54f,0xd52c,0xc50d,

    0x34e2,0x24c3,0x14a0,0x0481,0x7466,0x6447,0x5424,0x4405,

    0xa7db,0xb7fa,0x8799,0x97b8,0xe75f,0xf77e,0xc71d,0xd73c,

    0x26d3,0x36f2,0x0691,0x16b0,0x6657,0x7676,0x4615,0x5634,

    0xd94c,0xc96d,0xf90e,0xe92f,0x99c8,0x89e9,0xb98a,0xa9ab,

    0x5844,0x4865,0x7806,0x6827,0x18c0,0x08e1,0x3882,0x28a3,

    0xcb7d,0xdb5c,0xeb3f,0xfb1e,0x8bf9,0x9bd8,0xabbb,0xbb9a,

    0x4a75,0x5a54,0x6a37,0x7a16,0x0af1,0x1ad0,0x2ab3,0x3a92,

    0xfd2e,0xed0f,0xdd6c,0xcd4d,0xbdaa,0xad8b,0x9de8,0x8dc9,

    0x7c26,0x6c07,0x5c64,0x4c45,0x3ca2,0x2c83,0x1ce0,0x0cc1,

    0xef1f,0xff3e,0xcf5d,0xdf7c,0xaf9b,0xbfba,0x8fd9,0x9ff8,

    0x6e17,0x7e36,0x4e55,0x5e74,0x2e93,0x3eb2,0x0ed1,0x1ef0

};

        public static ushort ComputeChecksum(params byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException();
            //ushort crc = 0;
            //for (int i = 0; i < buffer.Length; ++i)
            //{
            //    crc = (ushort)((crc >> 8) ^ table[(crc ^ buffer[i]) & 0xff]);
            //}
            //return crc;

            ushort tmp;
            ushort crc = 0xffff;

            for (int i = 0; i < buffer.Length; i++)
            {
                tmp = (ushort)((crc >> 8) ^ buffer[i]);
                crc = (ushort)((crc << 8) ^ table[tmp]);
            }
            return crc;
        }

        public static ushort oldComputeChecksum(params byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException();
            //ushort crc = 0;
            //for (int i = 0; i < buffer.Length; ++i)
            //{
            //    crc = (ushort)((crc >> 8) ^ table[(crc ^ buffer[i]) & 0xff]);
            //}
            //return crc;

            ushort tmp;
            ushort crc = 0xffff;

            for (int i = 0; i < buffer.Length; i++)
            {
                tmp = (ushort)((crc >> 8) ^ buffer[i]);
                crc = (ushort)((crc << 8) ^ table[tmp]);
            }
            return crc;
        }

        public static byte[] ComputeChecksumBytes(params byte[] buffer)
        {
            return BitConverter.GetBytes(ComputeChecksum(buffer));
        }
    }
    #endregion

    #region Old Code
    //    public class DongleCommWin : ICANCommands, IWifiUSBHandler, IDongleHandler
    //    {
    //        #region Properties
    //        TaskCompletionSource<object> tskCmSsrc = null;
    //        SerialPort comPort = null;

    //        public TcpClient TcpClient { get; }

    //        //SimpleTCP.SimpleTcpClient tcpClient;
    //        Protocol protocol;
    //        string DebugTag = "ELM-DEBUG";
    //        string ActiveConnection = null;
    //        string Wifi = "Wifi";
    //        string USB = "USB";

    //        private SerialDevice serialPort = null;


    //        private SerialDevice serialPort2 = null;
    //        DataWriter dataWriteObject2 = null;
    //        DataReader dataReaderObject2 = null;
    //        private ResponseArrayStatus responseStructure;
    //        public Platform Platform;
    //        public Connectivity Connectivity;
    //        #endregion

    //        #region Ctor
    //        public void InitializePlatform(Platform platform, Connectivity connectivity)
    //        {
    //            Platform = platform;

    //            Connectivity = connectivity;
    //        }

    //        public DongleCommWin()
    //        {
    //            //comPort = new System.IO.Ports.SerialPort();
    //            //tskCmSsrc = new TaskCompletionSource<object>();
    //            //tcpClient = new SimpleTCP.SimpleTcpClient();
    //            Debug.WriteLine("Inside ELM", DebugTag);
    //            //BluetoothAdapter = BluetoothAdapter.DefaultAdapter;
    //            //if (BluetoothAdapter == null)
    //            //    Debug.WriteLine("No Bluetooth adapter found.", DebugTag);
    //            //else
    //            //    Debug.WriteLine("Bluetooth Adapter found!!");

    //            //if (!BluetoothAdapter.IsEnabled)
    //            //    Debug.WriteLine("Bluetooth adapter is not enabled.", DebugTag);
    //            //else
    //            //    Debug.WriteLine("Bluetooth Adapter enabled!", DebugTag);
    //        }

    //        public DongleCommWin(TcpClient tcpClient, Protocol protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
    //        {
    //            Debug.WriteLine("Inside ELM327 SerialDevice CTOR", DebugTag);
    //            TcpClient = tcpClient;

    //            protocol = protocolVersion;
    //            ActiveConnection = USB;
    //        }



    //        //public DongleCommWin(SimpleTcpClient client, Protocol protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
    //        //{
    //        //    Debug.WriteLine("Inside ELM327 SimpleTcpClient CTOR", DebugTag);
    //        //    tcpClient = client;
    //        //    protocol = protocolVersion;

    //        //    tcpClient.StringEncoder = Encoding.UTF8;
    //        //    tcpClient.DataReceived += TcpClient_DataReceived;

    //        //    if (tskCmSsrc == null)
    //        //    {
    //        //        tskCmSsrc = new TaskCompletionSource<object>();
    //        //    }

    //        //    ActiveConnection = Wifi;
    //        //}

    //        public DongleCommWin(SerialDevice serialDevice, Protocol protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
    //        {
    //            Debug.WriteLine("Inside ELM327 SerialDevice CTOR", DebugTag);
    //            serialPort = serialDevice;
    //            serialPort2 = serialDevice;
    //            protocol = protocolVersion;
    //            ActiveConnection = USB;
    //        }


    //        public async Task<object> ReadData()
    //        {
    //            if (Platform == Platform.UWP && Connectivity == Connectivity.USB)
    //            {
    //                object response = null;

    //                try
    //                {
    //                    Debug.WriteLine("---------INSIDE EXTRA READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //                    using (DataReader dataReader = new DataReader(serialPort.InputStream))
    //                    {
    //                        uint bytesToRead = 0;
    //                        bytesToRead = await dataReader.LoadAsync(1024);
    //                        var serialBytesReceived = new byte[bytesToRead];
    //                        dataReader.ReadBytes(serialBytesReceived);

    //                        response = serialBytesReceived;

    //                        if (response != null)
    //                        {
    //                            var responseBytes = (byte[])response;
    //                            Debug.WriteLine("EXTRA READ DATA RSPONSE = " + ByteArrayToString(responseBytes) + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //                        }
    //                        else
    //                        {
    //                            Debug.WriteLine("ELSE EXTRA READ Data = " + response.ToString() + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
    //                        }
    //                        dataReader.DetachStream();
    //                        dataReader.DetachBuffer();
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    throw ex;
    //                }

    //                return response;
    //            }
    //            else if (Platform == Platform.UWP && Connectivity == Connectivity.WiFi)
    //            {
    //                var wifiCommands = await GetWifiCommand();
    //                return wifiCommands;
    //            }
    //            return null;

    //        }
    //        //public DongleCommWin(SerialPort serialPort, Protocol protocolVersion, UInt32 txHeader, UInt32 rxHeader, UInt16 paddingByte, UInt16 p2max, UInt16 blkseqcntr, UInt16 septime)
    //        //{
    //        //    Debug.WriteLine("Inside ELM327 SerialPort CTOR", DebugTag);
    //        //    comPort = serialPort;
    //        //    protocol = protocolVersion;

    //        //    if (tskCmSsrc == null)
    //        //    {
    //        //        tskCmSsrc = new TaskCompletionSource<object>();
    //        //    }

    //        //    bool error = false;

    //        //    try  //always try to use this try and catch method to open your port. 
    //        //         //if there is an error your program will not display a message instead of freezing.
    //        //    {
    //        //        //Open Port
    //        //        if (!comPort.IsOpen)
    //        //            comPort.Open();
    //        //        comPort.DataReceived += SerialPortDataReceived;  //Check for received data. When there is data in the receive buffer,
    //        //                                                         //it will raise this event, we need to subscribe to it to know when there is data
    //        //    }
    //        //    catch (UnauthorizedAccessException) { error = true; }
    //        //    catch (System.IO.IOException) { error = true; }
    //        //    catch (ArgumentException) { error = true; }

    //        //    if (error)
    //        //        Debug.WriteLine("Could not open the COM port. Most likely it is already in use, has been removed, or is unavailable. or COM Port unavailable", DebugTag);

    //        //    //if the port is open, Change the Connect button to disconnect, enable the send button.
    //        //    //and disable the groupBox to prevent changing configuration of an open port.
    //        //    if (comPort.IsOpen)
    //        //    {
    //        //        // return true;

    //        //    }

    //        //    //return false;
    //        //    ActiveConnection = USB;
    //        //}
    //        #endregion

    //        #region Methods
    //        //----------------------------------------------------------------------------
    //        // Method Name   : BT_ConnectDevice
    //        // Input         : DEVICENAME
    //        // Output        : status of ConnectDevice form of bool
    //        // Purpose       : Connect with a BT / BLE Device and generate a handle
    //        // Date          : 20-08-20
    //        //---------------------------------------------------------------------------
    //        private bool USB_Connect(string deviceName)
    //        {
    //            //bool error = false;

    //            //try  //always try to use this try and catch method to open your port. 
    //            //     //if there is an error your program will not display a message instead of freezing.
    //            //{
    //            //    //Open Port
    //            //    comPort.Open();
    //            //    comPort.DataReceived += SerialPortDataReceived;  //Check for received data. When there is data in the receive buffer,
    //            //                                                     //it will raise this event, we need to subscribe to it to know when there is data
    //            //}
    //            //catch (UnauthorizedAccessException) { error = true; }
    //            //catch (System.IO.IOException) { error = true; }
    //            //catch (ArgumentException) { error = true; }

    //            //if (error)
    //            //    Debug.WriteLine("Could not open the COM port. Most likely it is already in use, has been removed, or is unavailable. or COM Port unavailable", DebugTag);

    //            ////if the port is open, Change the Connect button to disconnect, enable the send button.
    //            ////and disable the groupBox to prevent changing configuration of an open port.
    //            //if (comPort.IsOpen)
    //            //{
    //            //    return true;

    //            //}

    //            return false;

    //        }

    //        #region EventsHandler
    //        //private void TcpClient_DataReceived(object sender, SimpleTCP.Message e)
    //        //{
    //        //    var data = e.MessageString;
    //        //    tskCmSsrc?.TrySetResult(e.Data);

    //        //    Debug.WriteLine("TcpClient_DataReceived = " + e.Data + "\n", DebugTag);
    //        //}

    //        private async void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    //        {
    //            //await Task.Delay(5000);
    //            var serialPort = (SerialPort)sender;
    //            var data = serialPort.ReadExisting();
    //            tskCmSsrc?.TrySetResult(data);

    //            Debug.WriteLine("SerialPortDataReceived = " + data + "\n", DebugTag);

    //        }
    //        #endregion

    //        #region SendCommand

    //        public async Task<object> SendCommand(string randomCommand)
    //        {
    //            Debug.WriteLine("------SendCommand------", DebugTag);
    //            object response = null;

    //            string command = randomCommand.ToString();
    //            var bytesCommand = HexStringToByteArray(command);

    //            byte[] sendBytes = HexStringToByteArray(command);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); }); ;

    //            return response;

    //        }
    //        public async Task WriteBytesAsync(DataWriter dataWriteObject, byte[] bytesTosend)
    //        {

    //            dataWriteObject.WriteBytes(bytesTosend);
    //            var bytesWritten = await dataWriteObject.StoreAsync();
    //            Debug.WriteLine("BytesWritten =  " + bytesWritten.ToString(), DebugTag);
    //            //Task<UInt32> storeAsyncTask;
    //            //int offset = 0;
    //            //do
    //            //{
    //            //    //var ftt = HexStringToByteArray("43B43601000000000000000000000000000000005555AAAA00000000000000000000000000000000000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF000000000000000020FE60FFA000E001200360040005A0054006800700400040004000400040004000400040004000402003B00440060807D00798086009F00A800C100EF100830113025F02A602ED023303CB034704BE040008000A000CCC0D001000140018001C001E00207805780578057805780578057805780578057805EE02EE02EE02EE02EE02EE02EE02EE02EE02EE02C201C201C201C201C201C201C201C201C201C20100000000000000000000000000000000000000006AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF4006D0076009F00A800CA00FC012E0157017401F20FE60FFA000E001200360040005A00540068007C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C8002C012C012C012C012C012C012C012C012C012C01DC01DC01DC01DC01DC01DC01DC01DC01DC01DC0158025802580258025802580258025802580258029C029C029C029C029C029C029C029C029C029C02250325032503250325032503250325032503250384038403840384038403840384038403840384038403840384038403840384038403840384038403002000240028002C003000340038003A003C00400040083F083B2937CD340E2F73279822971C0800000090012003B0044006D0076009F00A800C480DD80E6810C0125014E0157017E803780508076009F00A800CA00F941188137C1570176419581B401F1027E02E004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040A809");
    //            //    //bytesTosend = ftt;
    //            //    int buffersize = bytesTosend.Length;
    //            //    //if (buffersize > 1000) { buffersize = 1000; }
    //            //    byte[] sendBuffer = new byte[buffersize];
    //            //    System.Buffer.BlockCopy(bytesTosend, offset, sendBuffer, 0, buffersize);

    //            //    dataWriteObject.WriteBytes(sendBuffer);

    //            //    var bytesWritten =  await dataWriteObject.StoreAsync();
    //            //    //await Task.Run(async () =>
    //            //    //{

    //            //    //    //await dataWriteObject.FlushAsync();

    //            //    //});

    //            //    offset += buffersize;
    //            //} while (offset < bytesTosend.Length);
    //        }
    //        public async Task<object> SendNewCommand(byte[] command, Action<string> onDataRecevied)
    //        {
    //            if (Platform == Platform.UWP && Connectivity == Connectivity.USB)
    //            {
    //                Debug.WriteLine("SENDING COMMAND " + ByteArrayToString(command), DebugTag);
    //                object response = null;

    //                try
    //                {
    //                    using (DataWriter dataWriteObject = new DataWriter(serialPort2.OutputStream))
    //                    {
    //                        await WriteBytesAsync(dataWriteObject, command);
    //                        if (dataWriteObject != null)
    //                        {
    //                            dataWriteObject.DetachStream();
    //                        }
    //                    }
    //                    if (dataReaderObject2 == null)
    //                        dataReaderObject2 = new DataReader(serialPort2.InputStream);
    //                    /* Read data in from the serial port */
    //                    Task<UInt32> bytesToReadh;

    //                    //Create task to read the bytes
    //                    bytesToReadh = dataReaderObject2.LoadAsync(9024).AsTask();
    //                    UInt32 bytesReadh = await bytesToReadh;

    //                    //Get the bytes from the ECU
    //                    var serialBytesReceivedh = new byte[bytesReadh];
    //                    dataReaderObject2.ReadBytes(serialBytesReceivedh);

    //                    response = serialBytesReceivedh;

    //                    if (serialBytesReceivedh.Length != 0)
    //                    {
    //                        var stringResponse = ByteArrayToString(serialBytesReceivedh);
    //                        Debug.WriteLine("Response Received =  " + stringResponse + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //                    }
    //                    return response;

    //                }

    //                catch (Exception ex)
    //                {
    //                    if (ex.GetType().Name == "TaskCanceledException")
    //                    {
    //                        Debug.WriteLine("EXCEPTION Received =  " + ex.StackTrace, DebugTag);
    //                        var excetion = ex.Message;
    //                        USB_Disconnect();
    //                    }

    //                    else
    //                    {

    //                    }
    //                }
    //                return response;
    //            }


    //            return null;

    //            //if (comPort.IsOpen)
    //            //{
    //            //    bool error = false;
    //            //    if (command != null)        //if text mode is selected, send data as tex
    //            //    {
    //            //        // Send the user's text straight out the port 
    //            //        //ComPort.Write(command);


    //            //    }
    //            //    else                    //if Hex mode is selected, send data in hexadecimal
    //            //    {
    //            //        try
    //            //        {
    //            //            // Convert the user's string of hex digits (example: E1 FF 1B) to a byte array
    //            //            byte[] data = HexStringToByteArray(command.ToString());

    //            //            // Send the binary data out the port
    //            //            comPort.Write(data, 0, data.Length);
    //            //        }
    //            //        catch (FormatException) { error = true; }

    //            //        // Inform the user if the hex string was not properly formatted
    //            //        catch (ArgumentException) { error = true; }

    //            //        //if (error) MessageBox.Show(this, "Not properly formatted hex string: " + txtSend.Text + "\n" + "example: E1 FF 1B", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
    //            //        Debug.WriteLine("Not properly formatted hex string: " + command + "\n" + "example: E1 FF 1B Format Error", DebugTag);

    //            //    }
    //            //}

    //            //if (ActiveConnection == Wifi)
    //            //    if (tcpClient.TcpClient.Connected)
    //            //    {
    //            //        byte[] data = HexStringToByteArray(command.ToString());

    //            //        // Send the binary data out the port
    //            //        tcpClient.Write(data);
    //            //    }
    //        }
    //        public async Task<byte[]> GetWifiCommand()
    //        {
    //            byte[] rbuffer = new byte[1024];
    //            byte[] RetArray = new byte[] { };
    //            byte[] ActualBytes = new byte[] { };

    //            Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
    //            int readByte = await TcpClient.GetStream().ReadAsync(rbuffer, 0, rbuffer.Length);

    //            RetArray = new byte[readByte];
    //            Array.Copy(rbuffer, 0, RetArray, 0, readByte);


    //            //FF0007003ECD820000000040057F19787ED10B54FF0107003ECE4E0000000041055902FF050400502269005006270050008700500088005000870050080600500001005000010050048000AF226A0023065000AF019300AF13000023061500AF161500500617005006160050065500AF05010050011800AF048700AF0219005003800023212200AF212700AF2135005002010050062D00500261005002620050061A0050061A0050061A00500001005000010050062F0050062F0050062F00500336005003350050068B00500601005006070050060700AF0607005006070050060700500607005006070050060700500607005006070050060D0050061C0050062B0050062B0050062B0050062B0050062B0050062B0050019100500191005006060023ED502406

    //            Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);

    //            var byteLength = RetArray.Length.ToString("d2");
    //            string hexValue = RetArray[1].ToString("D2") + RetArray[2].ToString("D2");
    //            int decValue = Convert.ToInt32(hexValue, 16);

    //            Array.Resize(ref ActualBytes, decValue);
    //            Array.Copy(RetArray, 11, ActualBytes, 0, decValue);

    //            if (RetArray.Length > ActualBytes.Length + 12 + 2)
    //            {

    //                string hexValue2 = RetArray[ActualBytes.Length + 2 + 11 + 1].ToString("D2") + RetArray[ActualBytes.Length + 2 + 11 + 2].ToString("D2");
    //                int decValue2 = Convert.ToInt32(hexValue2, 16);
    //                var tempArray = ActualBytes;
    //                Array.Resize(ref ActualBytes, decValue2);
    //                Array.Copy(RetArray, 11 + tempArray.Length + 11, ActualBytes, 0, decValue2);
    //            }
    //            Debug.WriteLine("--------- DATA RESPONSE-------" + ByteArrayToString(ActualBytes), DebugTag);
    //            return ActualBytes;
    //        }

    //        public async Task<object> SendCommand(byte[] command, Action<string> onDataRecevied)
    //        {
    //            if (Platform == Platform.UWP && Connectivity == Connectivity.USB)
    //            {
    //                Debug.WriteLine("SENDING COMMAND " + ByteArrayToString(command), DebugTag);
    //                Debug.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
    //                object response = null;

    //                try
    //                {
    //                    //// Create the data writer object backed by the in-memory stream.

    //                    using (DataWriter dataWriteObject = new DataWriter(serialPort.OutputStream))
    //                    {
    //                        await WriteBytesAsync(dataWriteObject, command);
    //                        if (dataWriteObject != null)
    //                        {
    //                            dataWriteObject.DetachStream();
    //                            dataWriteObject.DetachBuffer();

    //                        }
    //                    }
    //                    using (DataReader dataReaderObject = new DataReader(serialPort.InputStream))
    //                    {

    //                        /* Read data in from the serial port */
    //                        Task<UInt32> bytesToReadh;

    //                        //Create task to read the bytes
    //                        bytesToReadh = dataReaderObject.LoadAsync(9024).AsTask();
    //                        UInt32 bytesReadh = await bytesToReadh;

    //                        //Get the bytes from the ECU
    //                        var serialBytesReceivedh = new byte[bytesReadh];
    //                        dataReaderObject.ReadBytes(serialBytesReceivedh);

    //                        response = serialBytesReceivedh;

    //                        if (serialBytesReceivedh.Length != 0)
    //                        {
    //                            var stringResponse = ByteArrayToString(serialBytesReceivedh);
    //                            Debug.WriteLine("Response Received =  " + stringResponse + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //                        }

    //                        dataReaderObject.DetachStream();
    //                        dataReaderObject.DetachBuffer();
    //                    }


    //                    return response;

    //                    //if (serialPort != null)
    //                    //{
    //                    //    if (dataReaderObject == null)
    //                    //        dataReaderObject = new DataReader(serialPort.InputStream);

    //                    //    if (dataWriteObject == null)
    //                    //        dataWriteObject = new DataWriter(serialPort.OutputStream);

    //                    //    //Write bytes to the ECU
    //                    //    dataWriteObject.WriteBytes(command);
    //                    //    //var storeAsyncTask = dataWriteObject.StoreAsync().GetResults();
    //                    //    //if (storeAsyncTask > 0)
    //                    //    //{
    //                    //    //    Debug.WriteLine("Data Written Successsfully !", DebugTag);
    //                    //    //}
    //                    //    //Check the bytes are written
    //                    //    uint bytesWritten = await serialPort.OutputStream.WriteAsync(dataWriteObject.DetachBuffer());

    //                    //    Debug.WriteLine("Command Send =  " + ByteArrayToString(command) + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
    //                    //    try
    //                    //    {
    //                    //        /* Read data in from the serial port */
    //                    //        Task<UInt32> bytesToRead;

    //                    //        //Create task to read the bytes
    //                    //        bytesToRead = dataReaderObject.LoadAsync(1024).AsTask();
    //                    //        UInt32 bytesRead = await bytesToRead;

    //                    //        //Get the bytes from the ECU
    //                    //        var serialBytesReceived = new byte[bytesRead];
    //                    //        dataReaderObject.ReadBytes(serialBytesReceived);

    //                    //        response = serialBytesReceived;

    //                    //        if (serialBytesReceived.Length != 0)
    //                    //        {
    //                    //            var stringResponse = ByteArrayToString(serialBytesReceived);
    //                    //            Debug.WriteLine("Response Received =  " + stringResponse + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);

    //                    //        }
    //                    //    }
    //                    //    catch (Exception ex)
    //                    //    {
    //                    //        Debug.WriteLine("EXCEPTION Received =  " + ex.StackTrace, DebugTag);
    //                    //        var excetion = ex.Message;
    //                    //    }
    //                    //    finally
    //                    //    {

    //                    //        //dataWriteObject.Dispose();


    //                    //        //dataWriteObject = null;


    //                    //        ////dataReaderObject.Dispose();
    //                    //        //dataReaderObject = null;


    //                    //    }
    //                    //}

    //                    //else
    //                    //{

    //                    //    Debug.WriteLine("Length = 0", DebugTag);
    //                    //}

    //                }

    //                catch (Exception ex)
    //                {
    //                    if (ex.GetType().Name == "TaskCanceledException")
    //                    {
    //                        Debug.WriteLine("EXCEPTION Received =  " + ex.StackTrace, DebugTag);
    //                        var excetion = ex.Message;
    //                        USB_Disconnect();
    //                    }

    //                    else
    //                    {

    //                    }
    //                }
    //                return response;
    //            }

    //            else if (Platform == Platform.UWP && Connectivity == Connectivity.WiFi)
    //            {
    //                Debug.WriteLine("Command WiFi Send =  " + ByteArrayToString(command), DebugTag);

    //                //00 000e 00000000 00000000 500c47568afe56214e238000ffc3 ce2c

    //                //00 - Tx sequence counter
    //                //000e - length of the command(command length + 2)
    //                //00000000 - Timestamp - hhmmssmm
    //                //00000000 - Reserved - always 00
    //                //500c47568afe56214e238000ffc3 - actual command - payload
    //                //ce2c - crc of entire length from tx counter to the payload.

    //                var byte1 = "00";
    //                var byte2 = command.Length.ToString("X4");
    //                var byte3 = DateTime.Now.ToString("hhmmssff");
    //                var byte4 = DateTime.Now.ToString("00000000");
    //                var byte5 = ByteArrayToString(command);
    //                var dataBytes = HexStringToByteArray(byte1 + byte2 + byte3 + byte4 + byte5);
    //                //var byte6Checksum = Crc16CcittKermit.ComputeChecksum();

    //                string crc = Crc16CcittKermit.ComputeChecksum(dataBytes).ToString("X2");
    //                if (crc.Length == 3)
    //                {
    //                    crc = "0" + crc;
    //                }
    //                if (crc.Length == 2)
    //                {
    //                    crc = "00" + crc;
    //                }
    //                if (crc.Length == 1)
    //                {
    //                    crc = "000" + crc;
    //                }
    //                Debug.WriteLine("CRC =" + crc, DebugTag);

    //                var bytes = ByteArrayToString(dataBytes) + crc.ToString();
    //                var byteData = HexStringToByteArray(bytes);
    //                Debug.WriteLine("SENDING WiFi COMMAND " + ByteArrayToString(byteData), DebugTag);

    //                await TcpClient.GetStream().WriteAsync(byteData, 0, byteData.Length);

    //                var wifiCommand = await GetWifiCommand();
    //                return wifiCommand;
    //            }

    //            return null;

    //            //if (comPort.IsOpen)
    //            //{
    //            //    bool error = false;
    //            //    if (command != null)        //if text mode is selected, send data as tex
    //            //    {
    //            //        // Send the user's text straight out the port 
    //            //        //ComPort.Write(command);


    //            //    }
    //            //    else                    //if Hex mode is selected, send data in hexadecimal
    //            //    {
    //            //        try
    //            //        {
    //            //            // Convert the user's string of hex digits (example: E1 FF 1B) to a byte array
    //            //            byte[] data = HexStringToByteArray(command.ToString());

    //            //            // Send the binary data out the port
    //            //            comPort.Write(data, 0, data.Length);
    //            //        }
    //            //        catch (FormatException) { error = true; }

    //            //        // Inform the user if the hex string was not properly formatted
    //            //        catch (ArgumentException) { error = true; }

    //            //        //if (error) MessageBox.Show(this, "Not properly formatted hex string: " + txtSend.Text + "\n" + "example: E1 FF 1B", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
    //            //        Debug.WriteLine("Not properly formatted hex string: " + command + "\n" + "example: E1 FF 1B Format Error", DebugTag);

    //            //    }
    //            //}

    //            //if (ActiveConnection == Wifi)
    //            //    if (tcpClient.TcpClient.Connected)
    //            //    {
    //            //        byte[] data = HexStringToByteArray(command.ToString());

    //            //        // Send the binary data out the port
    //            //        tcpClient.Write(data);
    //            //    }
    //        }

    //        public bool USB_Disconnect()
    //        {

    //            //if (dataReaderObject != null)
    //            //{
    //            //    dataReaderObject.DetachStream();
    //            //    dataReaderObject = null;
    //            //}

    //            //if (dataWriteObject != null)
    //            //{
    //            //    dataWriteObject.DetachStream();
    //            //    dataWriteObject = null;
    //            //}

    //            if (serialPort != null)
    //            {
    //                serialPort.Dispose();
    //            }
    //            serialPort = null;


    //            return true;
    //        }

    //        public bool Wifi_Disconnect()
    //        {
    //            //tcpClient.Disconnect();
    //            return true;
    //        }

    //        #endregion

    //        #region Helper
    //        //Convert a string of hex digits (example: E1 FF 1B) to a byte array. 
    //        //The string containing the hex digits (with or without spaces)
    //        //Returns an array of bytes. </returns>
    //        //private byte[] HexStringToByteArray(string s)
    //        //{
    //        //    s = s.Replace(" ", "");
    //        //    byte[] buffer = new byte[s.Length / 2];
    //        //    for (int i = 0; i < s.Length; i += 2)
    //        //        buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
    //        //    return buffer;
    //        //}
    //        static byte[] HexToBytes(string input)
    //        {
    //            byte[] result = new byte[input.Length / 2];
    //            for (int i = 0; i < result.Length; i++)
    //            {
    //                result[i] = Convert.ToByte(input.Substring(2 * i, 2), 16);
    //            }
    //            return result;
    //        }

    //        private void WriteConsole(string input, string output)
    //        {
    //            Debug.WriteLine("Command = " + input + "\n" + "Output = " + output, DebugTag);
    //        }

    //        private byte[] HexStringToByteArray(String hex)
    //        {
    //            hex = hex.Replace(" ", "");
    //            int numberChars = hex.Length;
    //            if (numberChars % 2 != 0)
    //            {
    //                hex = "0" + hex;
    //                numberChars++;
    //            }
    //            byte[] bytes = new byte[numberChars / 2];
    //            for (int i = 0; i < numberChars; i += 2)
    //                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
    //            return bytes;
    //        }
    //        //----------------------------------------------------------------------------
    //        // Method Name   : ByteArrayToString
    //        // Input         : byte array
    //        // Output        : string
    //        // Purpose       : Function to convert byte array to string 
    //        // Date          : 27Sept16
    //        //----------------------------------------------------------------------------
    //        private string ByteArrayToString(byte[] ba)
    //        {
    //            string hex = BitConverter.ToString(ba);
    //            return hex.Replace("-", "");
    //        }
    //        //----------------------------------------------------------------------------
    //        // Method Name   : BT_GetDevices
    //        // Input         : NA
    //        // Output        : Collection of Bluetoothdevices with their status,name, address.
    //        // Purpose       : Get list of all devices with a specific BT/BLE name and with status paired and unpaired
    //        // Date          : 20-08-20
    //        //----------------------------------------------------------------------------

    //        #endregion

    //        #region WifiServices
    //        private object Wifi_GetDevices()
    //        {
    //            throw new NotImplementedException();
    //        }

    //        //private object Wifi_ConnectAP()
    //        //{
    //        //    tcpClient.StringEncoder = Encoding.UTF8;
    //        //    tcpClient.DataReceived += TcpClient_DataReceived;

    //        //    var clientStatus = tcpClient.Connect("Host", Convert.ToInt32("Port"));
    //        //    if (clientStatus.TcpClient.Connected == true)
    //        //    {
    //        //        ActiveConnection = Wifi;
    //        //        return true;
    //        //    }
    //        //    return false;
    //        //}

    //        private object Wifi_WriteSSIDPW()
    //        {
    //            throw new NotImplementedException();
    //        }

    //        private object Wifi_ConnectStation()
    //        {
    //            throw new NotImplementedException();
    //        }
    //        #endregion

    //        #region DongleServices

    //        public async Task<object> SecurityAccess()
    //        {
    //            Debug.WriteLine("------SecurityAccess------", DebugTag);

    //            string command = "500C47568AFE56214E238000FFC3";
    //            var bytesCommand = HexStringToByteArray(command);

    //            //SerialPortDataReceived(this, null);
    //            var response = await SendCommand(bytesCommand, (obj) => { WriteConsole(command, obj); }); ;

    //            return response;
    //        }
    //        //----------------------------------------------------------------------------
    //        // Method Name   : Dongle_Reset
    //        // Input         : NA
    //        // Output        : object
    //        // Purpose       : Reset all vehicle Communication related parameters to its default value
    //        // Date          : 20-08-20
    //        //----------------------------------------------------------------------------
    //        public async Task<object> Dongle_Reset()
    //        {
    //            Debug.WriteLine("------Inside Dongle_Reset------", DebugTag);
    //            //"0x20-0x01-0x01"
    //            string command = "200301";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            var response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        //----------------------------------------------------------------------------
    //        // Method Name   : Dongle_SetProtocol
    //        // Input         : NA
    //        // Output        : object
    //        // Purpose       : Set Vehicle Communication Protocol
    //        // Date          : 20-08-20
    //        //----------------------------------------------------------------------------
    //        public async Task<object> Dongle_SetProtocol(int protocolVersion)
    //        {
    //            object response = null;
    //            //   "0x20-0x02-0x02-0x < protocol > "

    //            Debug.WriteLine("------Dongle_SetProtocol------", DebugTag);
    //            string command = "200402" + protocolVersion.ToString("D2");
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        //----------------------------------------------------------------------------
    //        // Method Name   : Dongle_GetProtocol
    //        // Input         : NA
    //        // Output        : object
    //        // Purpose       : Get Vehicle Communication Protocol
    //        // Date          : 20-08-20
    //        //----------------------------------------------------------------------------
    //        public async Task<object> Dongle_GetProtocol()
    //        {
    //            Debug.WriteLine("------Dongle_GetProtocol------", DebugTag);
    //            object response = null;
    //            //"0x20-0x01-0x03"

    //            string command = "200303";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        //----------------------------------------------------------------------------
    //        // Method Name   : Dongle_GetFimrwareVersion
    //        // Input         : NA
    //        // Output        : object
    //        // Purpose       : Get firmware version
    //        // Date          : 20-08-20
    //        //----------------------------------------------------------------------------
    //        public async Task<object> Dongle_GetFimrwareVersion()
    //        {
    //            Debug.WriteLine("------Dongle_GetFimrwareVersion------", DebugTag);
    //            object response = null;
    //            string command = "200314";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }


    //        #endregion

    //        #region CANMethods
    //        public async Task<object> CAN_SetTxHeader(string txHeader)
    //        {
    //            Debug.WriteLine("------CAN_SetTxHeader------", DebugTag);
    //            //            "if protocol is one of the following: ISO15765-250KB-11BIT-CAN, ISO15765-500KB-11BIT-CAN, ISO15765-1MB-11BIT-CAN, 250KB-11BIT-CAN, 500KB-11BIT-CAN, 1MB-11BIT-CAN, OE-IVN-250KBPS-11BIT-CAN, OE-IVN-500KBPS-11BIT-CAN,OE-IVN-1MBPS-11BIT-CAN 
    //            //0x20-0x03-0x04-0xxx-0xyy

    //            //if protocol is one of the following: ISO15765 - 250KB - 29BIT - CAN, ISO15765 - 500KB - 29BIT - CAN, ISO15765 - 1MB - 29BIT - CAN, 250KB - 29BIT - CAN, 500KB - 29BIT - CAN, 1MB - 29BIT - CAN, OE - IVN - 250KBPS - 29BIT - CAN, OE - IVN - 500KBPS - 29BIT - CAN,OE - IVN - 1MBPS - 29BIT - CAN
    //            //0x20-0x05-0x04-0xpp-0xqq-0xrr-0xss"

    //            object response = null;
    //            string command = string.Empty;
    //            byte[] sendBytes = null;
    //            if (protocol == Protocol.ISO15765_250KB_11BIT_CAN || protocol == Protocol.ISO15765_500KB_11BIT_CAN || protocol == Protocol.ISO15765_1MB_11BIT_CAN || protocol == Protocol.I250KB_11BIT_CAN || protocol == Protocol.I500KB_11BIT_CAN || protocol == Protocol.I1MB_11BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_11BIT_CAN)
    //            {
    //                command = "200504" + txHeader.ToString();
    //                var bytesCommand = HexStringToByteArray(command);
    //                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
    //                if (crc.Length == 3)
    //                    crc = "0" + crc;
    //                sendBytes = HexStringToByteArray(command + crc);
    //            }
    //            else if (protocol == Protocol.ISO15765_250Kb_29BIT_CAN || protocol == Protocol.ISO15765_500KB_29BIT_CAN || protocol == Protocol.ISO15765_1MB_29BIT_CAN || protocol == Protocol.I250Kb_29BIT_CAN || protocol == Protocol.I500KB_29BIT_CAN || protocol == Protocol.I1MB_29BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_29BIT_CAN)
    //            {
    //                command = "200704" + txHeader.ToString();
    //                var bytesCommand = HexStringToByteArray(command);
    //                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4], bytesCommand[5], bytesCommand[6]).ToString("x2");
    //                if (crc.Length == 3)
    //                    crc = "0" + crc;
    //                sendBytes = HexStringToByteArray(command + crc);
    //            }
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

    //            return response;
    //        }

    //        public async Task<object> CAN_GetTxHeader()
    //        {
    //            Debug.WriteLine("------Dongle_SetProtocol------", DebugTag);
    //            object response = null;
    //            //"0x20-0x01-0x05"
    //            string command = "200305";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_SetRxHeaderMask(string rxhdrmsk)
    //        {
    //            Debug.WriteLine("------CAN_SetRxHeaderMask------", DebugTag);
    //            object response = null;
    //            //            "if protocol is one of the following: ISO15765-250KB-11BIT-CAN, ISO15765-500KB-11BIT-CAN, ISO15765-1MB-11BIT-CAN, 250KB-11BIT-CAN, 500KB-11BIT-CAN, 1MB-11BIT-CAN, OE-IVN-250KBPS-11BIT-CAN, OE-IVN-500KBPS-11BIT-CAN,OE-IVN-1MBPS-11BIT-CAN 
    //            //0x20-0x03-0x06-0xxx-0xyy

    //            //if protocol is one of the following: ISO15765 - 250KB - 29BIT - CAN, ISO15765 - 500KB - 29BIT - CAN, ISO15765 - 1MB - 29BIT - CAN, 250KB - 29BIT - CAN, 500KB - 29BIT - CAN, 1MB - 29BIT - CAN, OE - IVN - 250KBPS - 29BIT - CAN, OE - IVN - 500KBPS - 29BIT - CAN,OE - IVN - 1MBPS - 29BIT - CAN
    //            //0x20-0x05-0x06-0xpp-0xqq-0xrr-0xss"

    //            string command = string.Empty;
    //            byte[] sendBytes = null;
    //            if (protocol == Protocol.ISO15765_250KB_11BIT_CAN || protocol == Protocol.ISO15765_500KB_11BIT_CAN || protocol == Protocol.ISO15765_1MB_11BIT_CAN || protocol == Protocol.I250KB_11BIT_CAN || protocol == Protocol.I500KB_11BIT_CAN || protocol == Protocol.I1MB_11BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_11BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_11BIT_CAN)
    //            {
    //                command = "200506" + rxhdrmsk.ToString();
    //                var bytesCommand = HexStringToByteArray(command);
    //                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
    //                if (crc.Length == 3)
    //                    crc = "0" + crc;
    //                sendBytes = HexStringToByteArray(command + crc);
    //            }
    //            else if (protocol == Protocol.ISO15765_250Kb_29BIT_CAN || protocol == Protocol.ISO15765_500KB_29BIT_CAN || protocol == Protocol.ISO15765_1MB_29BIT_CAN || protocol == Protocol.I250Kb_29BIT_CAN || protocol == Protocol.I500KB_29BIT_CAN || protocol == Protocol.I1MB_29BIT_CAN || protocol == Protocol.OE_IVN_250KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_500KBPS_29BIT_CAN || protocol == Protocol.OE_IVN_1MBPS_29BIT_CAN)
    //            {
    //                command = "200706" + rxhdrmsk.ToString();
    //                var bytesCommand = HexStringToByteArray(command);
    //                string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4], bytesCommand[5], bytesCommand[6]).ToString("x2");
    //                if (crc.Length == 3)
    //                    crc = "0" + crc;
    //                sendBytes = HexStringToByteArray(command + crc);
    //            }
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;

    //        }

    //        public async Task<object> CAN_GetRxHeaderMask()
    //        {
    //            Debug.WriteLine("------CAN_GetRxHeaderMask------", DebugTag);
    //            object response = null;
    //            //"0x20-0x01-0x07"

    //            string command = "200307";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_SetP1Min(string p1min)
    //        {
    //            Debug.WriteLine("------CAN_GetRxHeaderMask------", DebugTag);
    //            object response = null;
    //            //"0x20-0x02-0x0c-0x < xx > "

    //            string command = "20040c" + p1min.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_GetP1Min()
    //        {
    //            Debug.WriteLine("------CAN_GetP1Min------", DebugTag);
    //            object response = null;
    //            //"0x20-0x01-0x0d"

    //            string command = "20030d";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_SetP2Max(string p2max)
    //        {
    //            Debug.WriteLine("------CAN_SetP2Max------", DebugTag);
    //            object response = null;
    //            //"0x20-0x03-0x0e-0x < xx >-0x < yy > "

    //            string command = "20050e" + p2max.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_GetP2Max()
    //        {
    //            Debug.WriteLine("------CAN_GetP2Max------", DebugTag);
    //            object response = null;
    //            //"0x20-0x01-0x0f"

    //            string command = "20030f";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);

    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_StartTP()
    //        {
    //            Debug.WriteLine("------CAN_StartTP------", DebugTag);
    //            object response = null;
    //            //"0x20-0x01-0x10"

    //            string command = "200310";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_StopTP()
    //        {
    //            Debug.WriteLine("------CAN_StopTP------", DebugTag);
    //            object response = null;
    //            //"0x20,0x01, 0x11"

    //            string command = "200311";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_StartPadding(string paddingByte)
    //        {
    //            Debug.WriteLine("------CAN_StartPadding------", DebugTag);
    //            object response = null;
    //            //"0x20,0x02,0x12, 0x < xx > "

    //            string command = "200412" + paddingByte.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;

    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_StopPadding()
    //        {
    //            Debug.WriteLine("------CAN_StopPadding------", DebugTag);
    //            object response = null;
    //            //"0x20- 0x01-0x13"

    //            string command = "200313";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //                crc = "0" + crc;

    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> CAN_TxData(string txdata)
    //        {
    //            Debug.WriteLine("------CAN_StartPadding------", DebugTag);
    //            object response = null;
    //            ////data requests are of 2 types. If length of message <1000> then use 4x command, if not, use 1x command
    //            //0x4 < l >
    //            //0x < ll >
    //            string command = "40" + txdata.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            var crcBytesComputation = HexStringToByteArray(txdata);
    //            string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("x2");
    //            if (crc.Length == 3)
    //            {
    //                crc = "0" + crc;
    //            }
    //            byte[] sendBytes = HexStringToByteArray(command + crc);

    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;

    //        }


    //        public async Task<ResponseArrayStatus> CAN_NewTxRx(int framelength, string txdata)
    //        {
    //            Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
    //            object response = null;
    //            int dataLength = framelength + 2; //crc
    //            string command = string.Empty;
    //            //if (dataLength.ToString().Length==2)
    //            //{
    //            //    //2 bytes
    //            //     command = "40" + dataLength.ToString("X2") + txdata.ToString();
    //            //}
    //            //else
    //            //{
    //            //    //3 bytes
    //            //     command = "4" + dataLength.ToString("X2") + txdata.ToString();
    //            //}

    //            var firstbyte = 0x40 | ((dataLength >> 8) & 0x0f);
    //            var secondbyte = dataLength & 0xff;

    //            command = firstbyte.ToString("X2") + secondbyte.ToString("X2") + txdata.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            var crcBytesComputation = HexStringToByteArray(txdata);
    //            string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("X2");
    //            if (crc.Length == 3)
    //            {
    //                crc = "0" + crc;
    //            }
    //            byte[] sendBytes = HexStringToByteArray(command + crc);

    //            response = await SendNewCommand(sendBytes, (obj) => { WriteConsole(command, obj); });

    //            if (response != null)
    //            {
    //                var ecuResponseBytes = (byte[])response;
    //                ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);
    //                if (dataStatus == "READAGAIN")
    //                    while (dataStatus == "READAGAIN")
    //                    {
    //                        var responseReadAgain = await ReadData();
    //                        var ecuResponseReadBytes = (byte[])responseReadAgain;
    //                        ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
    //                        dataStatus = dataReadStatus;
    //                        responseStructure = new ResponseArrayStatus
    //                        {
    //                            ECUResponse = ecuResponseReadBytes,
    //                            ECUResponseStatus = dataReadStatus,
    //                            ActualDataBytes = actualReadBytes


    //                        };
    //                        Debug.WriteLine("------EXTRA READ DATA START ------", DebugTag);
    //                        if (responseStructure?.ECUResponse != null)
    //                            Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseReadBytes), DebugTag);
    //                        if (responseStructure?.ActualDataBytes != null)
    //                            Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                        Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                        Debug.WriteLine("------EXTRA READ DATA END ------", DebugTag);
    //                    }
    //                else
    //                {

    //                    responseStructure = new ResponseArrayStatus
    //                    {
    //                        ECUResponse = ecuResponseBytes,
    //                        ECUResponseStatus = dataStatus,
    //                        ActualDataBytes = actualDataBytes
    //                    };
    //                    Debug.WriteLine("------ECU RESPONE START ------", DebugTag);
    //                    if (responseStructure?.ECUResponse != null)
    //                        Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(responseStructure?.ECUResponse), DebugTag);
    //                    if (responseStructure?.ActualDataBytes != null)
    //                        Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                    Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                    Debug.WriteLine("------ECU RESPONE END ------", DebugTag);

    //                }
    //            }

    //            return responseStructure;

    //        }
    //        //public async Task<object> CAN_RxData()
    //        //{
    //        //    throw new NotImplementedException();
    //        //}

    //        public async Task<ResponseArrayStatus> CAN_TxRx(int framelength, string txdata)
    //        {
    //            Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
    //            object response = null;
    //            int dataLength = framelength + 2; //crc
    //            string command = string.Empty;

    //            var firstbyte = 0x40 | ((dataLength >> 8) & 0x0f);
    //            var secondbyte = dataLength & 0xff;

    //            command = firstbyte.ToString("X2") + secondbyte.ToString("X2") + txdata.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            var crcBytesComputation = HexStringToByteArray(txdata);
    //            string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("X2");
    //            if (crc.Length == 3)
    //            {
    //                crc = "0" + crc;
    //            }
    //            if (crc.Length == 2)
    //            {
    //                crc = "00" + crc;
    //            }
    //            if (crc.Length == 1)
    //            {
    //                crc = "000" + crc;
    //            }
    //            Debug.WriteLine("CRC =" + crc, DebugTag);
    //            byte[] sendBytes = HexStringToByteArray(command + crc);

    //            UInt16 nooftimessent = 0;

    //        STARTOVERAGAIN:
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            nooftimessent++;

    //            if (Convert.ToString(response).Contains("dhcps: send_offer>>udp_sendto result 0") == true)
    //            {
    //                Debug.WriteLine("--------- stripping off unwanted characters from response dhcps: send_offer >> udp_sendto result 0------------==" + "ELMZ");
    //                string ValueToReplace = "dhcps: send_offer>>udp_sendto result 0";
    //                response = (string)response.ToString().Replace(ValueToReplace, "");
    //                //var ecuResponseBytes = (byte[])response;
    //            }

    //            if (response != null)
    //            {
    //                var ecuResponseBytes = (byte[])response;

    //                ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);

    //                if (dataStatus == "SENDAGAIN")
    //                {
    //                    while (dataStatus == "SENDAGAIN")
    //                    {
    //                        Debug.WriteLine("------SENDAGAIN ------" + Convert.ToString(nooftimessent), DebugTag);

    //                        if (nooftimessent <= 5)
    //                        {
    //                            goto STARTOVERAGAIN;
    //                        }
    //                        else
    //                        {
    //                            /* stop sending again - Problem with the device */
    //                            responseStructure = new ResponseArrayStatus
    //                            {
    //                                ECUResponse = ecuResponseBytes,
    //                                ECUResponseStatus = "DONGLEERROR_SENDAGAINTHRESHOLDCROSSED",
    //                                ActualDataBytes = actualDataBytes
    //                            };
    //                            break;
    //                        }

    //                        //response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //                        //if (response != null)
    //                        //{
    //                        //    var ecuResponseBytes2 = (byte[])response;
    //                        //    ResponseArrayDecoding.CheckResponse(ecuResponseBytes2, sendBytes, out byte[] actualDataBytes2, out string dataStatus2);
    //                        //    dataStatus = dataStatus2;
    //                        //    responseStructure = new ResponseArrayStatus
    //                        //    {
    //                        //        ECUResponse = ecuResponseBytes2,
    //                        //        ECUResponseStatus = dataStatus2,
    //                        //        ActualDataBytes = actualDataBytes2
    //                        //    };
    //                        //    Debug.WriteLine("------SENDAGAIN DATA START ------", DebugTag);
    //                        //    if (responseStructure?.ECUResponse != null)
    //                        //        Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseBytes2), DebugTag);
    //                        //    if (responseStructure?.ActualDataBytes != null)
    //                        //        Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                        //    Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                        //    Debug.WriteLine("------SENDAGAIN DATA END ------", DebugTag);
    //                        //}
    //                    }
    //                }
    //                else if (dataStatus == "READAGAIN")
    //                {
    //                    while (dataStatus == "READAGAIN")
    //                    {
    //                        var responseReadAgain = await ReadData();
    //                        var ecuResponseReadBytes = (byte[])responseReadAgain;
    //                        ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
    //                        dataStatus = dataReadStatus;
    //                        responseStructure = new ResponseArrayStatus
    //                        {
    //                            ECUResponse = ecuResponseReadBytes,
    //                            ECUResponseStatus = dataReadStatus,
    //                            ActualDataBytes = actualReadBytes
    //                        };
    //                        Debug.WriteLine("------EXTRA READ DATA START ------", DebugTag);
    //                        if (responseStructure?.ECUResponse != null)
    //                            Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseReadBytes), DebugTag);
    //                        if (responseStructure?.ActualDataBytes != null)
    //                            Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                        Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                        Debug.WriteLine("------EXTRA READ DATA END ------", DebugTag);
    //                    }
    //                }
    //                else
    //                {
    //                    responseStructure = new ResponseArrayStatus
    //                    {
    //                        ECUResponse = ecuResponseBytes,
    //                        ECUResponseStatus = dataStatus,
    //                        ActualDataBytes = actualDataBytes
    //                    };
    //                    Debug.WriteLine("------ECU RESPONE START ------", DebugTag);
    //                    if (responseStructure?.ECUResponse != null)
    //                        Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(responseStructure?.ECUResponse), DebugTag);
    //                    if (responseStructure?.ActualDataBytes != null)
    //                        Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                    Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                    Debug.WriteLine("------ECU RESPONE END ------", DebugTag);
    //                }
    //            }
    //            return responseStructure;

    //        }


    //        public async Task<ResponseArrayStatus> OldCAN_TxRx(int framelength, string txdata)
    //        {
    //            Debug.WriteLine("------Start CAN_TxRx------", DebugTag);
    //            object response = null;
    //            int dataLength = framelength + 2; //crc
    //            string command = string.Empty;
    //            //if (dataLength.ToString().Length==2)
    //            //{
    //            //    //2 bytes
    //            //     command = "40" + dataLength.ToString("X2") + txdata.ToString();
    //            //}
    //            //else
    //            //{
    //            //    //3 bytes
    //            //     command = "4" + dataLength.ToString("X2") + txdata.ToString();
    //            //}

    //            var firstbyte = 0x40 | ((dataLength >> 8) & 0x0f);
    //            var secondbyte = dataLength & 0xff;

    //            command = firstbyte.ToString("X2") + secondbyte.ToString("X2") + txdata.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            var crcBytesComputation = HexStringToByteArray(txdata);
    //            string crc = Crc16CcittKermit.ComputeChecksum(crcBytesComputation).ToString("X2");
    //            if (crc.Length == 3)
    //            {
    //                crc = "0" + crc;
    //            }
    //            else if (crc.Length == 2)
    //            {
    //                crc = "00" + crc;
    //            }
    //            else if (crc.Length == 1)
    //            {
    //                crc = "000" + crc;
    //            }
    //            Debug.WriteLine("CRC =" + crc, DebugTag);
    //            byte[] sendBytes = HexStringToByteArray(command + crc);

    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });


    //            if (response != null)
    //            {
    //                var ecuResponseBytes = (byte[])response;

    //                ResponseArrayDecoding.CheckResponse(ecuResponseBytes, sendBytes, out byte[] actualDataBytes, out string dataStatus);

    //                if (dataStatus == "SENDAGAIN")
    //                {
    //                    while (dataStatus == "SENDAGAIN")
    //                    {
    //                        Debug.WriteLine("------SENDAGAIN ------", DebugTag);
    //                        response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //                        if (response != null)
    //                        {
    //                            var ecuResponseBytes2 = (byte[])response;
    //                            ResponseArrayDecoding.CheckResponse(ecuResponseBytes2, sendBytes, out byte[] actualDataBytes2, out string dataStatus2);
    //                            dataStatus = dataStatus2;
    //                            responseStructure = new ResponseArrayStatus
    //                            {
    //                                ECUResponse = ecuResponseBytes2,
    //                                ECUResponseStatus = dataStatus2,
    //                                ActualDataBytes = actualDataBytes2


    //                            };
    //                            Debug.WriteLine("------SENDAGAIN DATA START ------", DebugTag);
    //                            if (responseStructure?.ECUResponse != null)
    //                                Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseBytes2), DebugTag);
    //                            if (responseStructure?.ActualDataBytes != null)
    //                                Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                            Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                            Debug.WriteLine("------SENDAGAIN DATA END ------", DebugTag);
    //                        }
    //                    }
    //                }
    //                else if (dataStatus == "READAGAIN")
    //                {
    //                    while (dataStatus == "READAGAIN")
    //                    {
    //                        var responseReadAgain = await ReadData();
    //                        var ecuResponseReadBytes = (byte[])responseReadAgain;
    //                        ResponseArrayDecoding.CheckResponse(ecuResponseReadBytes, sendBytes, out byte[] actualReadBytes, out string dataReadStatus);
    //                        dataStatus = dataReadStatus;
    //                        responseStructure = new ResponseArrayStatus
    //                        {
    //                            ECUResponse = ecuResponseReadBytes,
    //                            ECUResponseStatus = dataReadStatus,
    //                            ActualDataBytes = actualReadBytes


    //                        };
    //                        Debug.WriteLine("------EXTRA READ DATA START ------", DebugTag);
    //                        if (responseStructure?.ECUResponse != null)
    //                            Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(ecuResponseReadBytes), DebugTag);
    //                        if (responseStructure?.ActualDataBytes != null)
    //                            Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                        Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                        Debug.WriteLine("------EXTRA READ DATA END ------", DebugTag);
    //                    }
    //                }
    //                else
    //                {
    //                    responseStructure = new ResponseArrayStatus
    //                    {
    //                        ECUResponse = ecuResponseBytes,
    //                        ECUResponseStatus = dataStatus,
    //                        ActualDataBytes = actualDataBytes
    //                    };
    //                    Debug.WriteLine("------ECU RESPONE START ------", DebugTag);
    //                    if (responseStructure?.ECUResponse != null)
    //                        Debug.WriteLine("------ECUResponse ------" + ByteArrayToString(responseStructure?.ECUResponse), DebugTag);
    //                    if (responseStructure?.ActualDataBytes != null)
    //                        Debug.WriteLine("------ActualDataBytes ------" + ByteArrayToString(responseStructure?.ActualDataBytes), DebugTag);
    //                    Debug.WriteLine("------ECUResponseStatus ------" + responseStructure.ECUResponseStatus, DebugTag);
    //                    Debug.WriteLine("------ECU RESPONE END ------", DebugTag);

    //                }
    //            }
    //            return responseStructure;

    //        }

    //        //public async Task<object> CAN_TxRx(int frameLength, string txdata)
    //        //{
    //        //    Debug.WriteLine("------CAN_StartPadding------", DebugTag);
    //        //    object response = null;

    //        //    string command = "40" + txdata.ToString();
    //        //    var bytesCommand = HexStringToByteArray(command);
    //        //    string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3], bytesCommand[4]).ToString("x2");
    //        //    byte[] sendBytes = HexStringToByteArray(command + crc);

    //        //    response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); }); ;

    //        //    return response;

    //        //}   

    //        public async Task<object> SetBlkSeqCntr(string blklen)
    //        {
    //            Debug.WriteLine("------SetBlkSeqCntr------", DebugTag);
    //            object response = null;
    //            //"0x20- 0x02 - 0x08- 0xxx(0x00 to 0x40)"

    //            string command = "200408" + blklen.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //            if (crc.Length == 3)
    //            {
    //                crc = "0" + crc;
    //            }
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> GetBlkSeqCntr()
    //        {
    //            Debug.WriteLine("------GetBlkSeqCntr------", DebugTag);
    //            object response = null;
    //            //"0x20- 0x01-0x09"
    //            string command = "200309";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //            {
    //                crc = "0" + crc;
    //            }
    //            byte[] sendBytes = HexStringToByteArray(command + crc);

    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> SetSepTime(string septime)
    //        {
    //            Debug.WriteLine("------SetSepTime------", DebugTag);
    //            object response = null;
    //            //"0x20-0x02-0x0A-0xxx"

    //            string command = "20040A" + septime.ToString();
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2], bytesCommand[3]).ToString("x2");
    //            if (crc.Length == 3)
    //            {
    //                crc = "0" + crc;
    //            }
    //            byte[] sendBytes = HexStringToByteArray(command + crc);

    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); });
    //            return response;
    //        }

    //        public async Task<object> GetSepTime()
    //        {
    //            Debug.WriteLine("------GetSepTime------", DebugTag);
    //            object response = null;
    //            //"0x20-0x01-0x0B"
    //            string command = "20030B";
    //            var bytesCommand = HexStringToByteArray(command);
    //            string crc = Crc16CcittKermit.ComputeChecksum(bytesCommand[2]).ToString("x2");
    //            if (crc.Length == 3)
    //            {
    //                crc = "0" + crc;
    //            }
    //            byte[] sendBytes = HexStringToByteArray(command + crc);
    //            response = await SendCommand(sendBytes, (obj) => { WriteConsole(command, obj); }); ;

    //            return response;
    //        }


    //        #endregion

    //        #endregion
    //    }

    //    public class Crc16CcittKermit
    //    {
    //        private static ushort[] table = {

    //    0x0000,0x1021,0x2042,0x3063,0x4084,0x50a5,0x60c6,0x70e7,

    //    0x8108,0x9129,0xa14a,0xb16b,0xc18c,0xd1ad,0xe1ce,0xf1ef,

    //    0x1231,0x0210,0x3273,0x2252,0x52b5,0x4294,0x72f7,0x62d6,

    //    0x9339,0x8318,0xb37b,0xa35a,0xd3bd,0xc39c,0xf3ff,0xe3de,

    //    0x2462,0x3443,0x0420,0x1401,0x64e6,0x74c7,0x44a4,0x5485,

    //    0xa56a,0xb54b,0x8528,0x9509,0xe5ee,0xf5cf,0xc5ac,0xd58d,

    //    0x3653,0x2672,0x1611,0x0630,0x76d7,0x66f6,0x5695,0x46b4,

    //    0xb75b,0xa77a,0x9719,0x8738,0xf7df,0xe7fe,0xd79d,0xc7bc,

    //    0x48c4,0x58e5,0x6886,0x78a7,0x0840,0x1861,0x2802,0x3823,

    //    0xc9cc,0xd9ed,0xe98e,0xf9af,0x8948,0x9969,0xa90a,0xb92b,

    //    0x5af5,0x4ad4,0x7ab7,0x6a96,0x1a71,0x0a50,0x3a33,0x2a12,

    //    0xdbfd,0xcbdc,0xfbbf,0xeb9e,0x9b79,0x8b58,0xbb3b,0xab1a,

    //    0x6ca6,0x7c87,0x4ce4,0x5cc5,0x2c22,0x3c03,0x0c60,0x1c41,

    //    0xedae,0xfd8f,0xcdec,0xddcd,0xad2a,0xbd0b,0x8d68,0x9d49,

    //    0x7e97,0x6eb6,0x5ed5,0x4ef4,0x3e13,0x2e32,0x1e51,0x0e70,

    //    0xff9f,0xefbe,0xdfdd,0xcffc,0xbf1b,0xaf3a,0x9f59,0x8f78,

    //    0x9188,0x81a9,0xb1ca,0xa1eb,0xd10c,0xc12d,0xf14e,0xe16f,

    //    0x1080,0x00a1,0x30c2,0x20e3,0x5004,0x4025,0x7046,0x6067,

    //    0x83b9,0x9398,0xa3fb,0xb3da,0xc33d,0xd31c,0xe37f,0xf35e,

    //    0x02b1,0x1290,0x22f3,0x32d2,0x4235,0x5214,0x6277,0x7256,

    //    0xb5ea,0xa5cb,0x95a8,0x8589,0xf56e,0xe54f,0xd52c,0xc50d,

    //    0x34e2,0x24c3,0x14a0,0x0481,0x7466,0x6447,0x5424,0x4405,

    //    0xa7db,0xb7fa,0x8799,0x97b8,0xe75f,0xf77e,0xc71d,0xd73c,

    //    0x26d3,0x36f2,0x0691,0x16b0,0x6657,0x7676,0x4615,0x5634,

    //    0xd94c,0xc96d,0xf90e,0xe92f,0x99c8,0x89e9,0xb98a,0xa9ab,

    //    0x5844,0x4865,0x7806,0x6827,0x18c0,0x08e1,0x3882,0x28a3,

    //    0xcb7d,0xdb5c,0xeb3f,0xfb1e,0x8bf9,0x9bd8,0xabbb,0xbb9a,

    //    0x4a75,0x5a54,0x6a37,0x7a16,0x0af1,0x1ad0,0x2ab3,0x3a92,

    //    0xfd2e,0xed0f,0xdd6c,0xcd4d,0xbdaa,0xad8b,0x9de8,0x8dc9,

    //    0x7c26,0x6c07,0x5c64,0x4c45,0x3ca2,0x2c83,0x1ce0,0x0cc1,

    //    0xef1f,0xff3e,0xcf5d,0xdf7c,0xaf9b,0xbfba,0x8fd9,0x9ff8,

    //    0x6e17,0x7e36,0x4e55,0x5e74,0x2e93,0x3eb2,0x0ed1,0x1ef0

    //};

    //        public static ushort ComputeChecksum(params byte[] buffer)
    //        {
    //            if (buffer == null) throw new ArgumentNullException();
    //            //ushort crc = 0;
    //            //for (int i = 0; i < buffer.Length; ++i)
    //            //{
    //            //    crc = (ushort)((crc >> 8) ^ table[(crc ^ buffer[i]) & 0xff]);
    //            //}
    //            //return crc;

    //            ushort tmp;
    //            ushort crc = 0xffff;

    //            for (int i = 0; i < buffer.Length; i++)
    //            {
    //                tmp = (ushort)((crc >> 8) ^ buffer[i]);
    //                crc = (ushort)((crc << 8) ^ table[tmp]);
    //            }
    //            return crc;
    //        }

    //        public static byte[] ComputeChecksumBytes(params byte[] buffer)
    //        {
    //            return BitConverter.GetBytes(ComputeChecksum(buffer));
    //        }
    //    }
    #endregion
}
