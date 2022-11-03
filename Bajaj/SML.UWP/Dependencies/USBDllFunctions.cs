using APDiagnostic;
using APDiagnostic.Enums;
using APDiagnostic.Models;
using APDiagnostic.Structures;
using APDongleCommWin;
using SML.Interfaces;
using SML.UWP.Dependencies;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Usb;
using Xamarin.Forms;

[assembly: Dependency(typeof(USBDllFunctions))]
namespace SML.UWP.Dependencies
{
    public class USBDllFunctions : IConnectionUSB
    {
        DongleCommWin dongleCommWin;
        UDSDiagnostic dSDiagnostic;
        private APDiagnostic.Models.ReadDtcResponseModel readDTCResponse;
        SerialDevice serialDevice = null;
        DeviceInformationCollection devices = null;
        DeviceInformation deviceInfo = null;
        //ECUCalculateSeedkey calculateSeedkey;


        string DebugTag = "Write SSID PASSWORD";

        public async Task<string> connection()
        {
            try
            {
                //string selector = SerialDevice.GetDeviceSelector("COM5");
                var deviceSelector = SerialDevice.GetDeviceSelector();

                devices = await DeviceInformation.FindAllAsync(deviceSelector);
                if (devices.Count > 0)
                {
                    deviceInfo = devices.FirstOrDefault(x => x.Name.Contains("Silicon Labs CP210x"));
                    if (deviceInfo != null)
                    {
                        serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);

                        if (serialDevice != null)
                        {
                            if (deviceInfo.Id.Contains("202"))
                            {
                                serialDevice.BaudRate = 460800;
                            }
                            else
                            {
                                serialDevice.BaudRate = 115200;
                            }

                            string tx_header_temp = string.Empty;
                            string rx_header_temp = string.Empty;

                            var ProtocolValue = (dynamic)null;
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
                                    tx_header_temp = "07E0";
                                    rx_header_temp = "07E8";
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


                            serialDevice.ReadTimeout = TimeSpan.FromMilliseconds(2);
                            serialDevice.WriteTimeout = TimeSpan.FromMilliseconds(2);
                            //serialDevice.Handshake = SerialHandshake.RequestToSendXOnXOff;
                            dongleCommWin = new DongleCommWin(serialDevice, ProtocolValue, Convert.ToUInt32(tx_header_temp, 16), Convert.ToUInt32(rx_header_temp, 16), 0x00, 0x10, 0x10, 0x10);
                            dongleCommWin.InitializePlatform(APDongleCommWin.ENUMS.Platform.UWP, APDongleCommWin.ENUMS.Connectivity.USB);
                            dSDiagnostic = new UDSDiagnostic(dongleCommWin);
                            return "Dongle Connected";
                        }
                        else
                        {
                            return "Dongle Already Connected !!!";
                        }
                    }
                    else
                    {
                        return "Dongle not found";
                    }
                }
                else
                {
                    return "Dongle not found";
                }
            }
            catch (Exception ex)
            {
                return $"{ex.Message}\n\n{ex.StackTrace}";
            }
        }
        public async void DisconnectUSB()
        {
            var donglereset = await dongleCommWin.Dongle_Reset();
            dongleCommWin.USB_Disconnect();
        }
        public async Task<string> usb_again_connect(string RouterSSID, string RouterPassword)
        {
            //TcpClient tcp_Client = null;
            string return_value = string.Empty;
            try
            {
                //var bytes = HexStringToByteArray("500c47568afe56214e238000ffc3");

                var dongle_response = await connection();
                if (dongle_response == "Dongle Connected")
                {
                    return_value = "Doungle Connected";
                    var securityAccess = await dongleCommWin.SecurityAccess();
                    var securityResult = (byte[])securityAccess;

                    return_value = $"{return_value}\n\nSecurity : {ByteArrayToString(securityResult)}";

                    //// SSID
                    string ssidcmd = "1601" + ToHex(RouterSSID) + "00";
                    string SSIDCommand = "20" + (RouterSSID.Length + 5).ToString("X2") + ssidcmd;

                    var dataBytes_ = HexStringToByteArray(ssidcmd);
                    var byte6Checksum = Crc16CcittKermit.ComputeChecksum(dataBytes_);

                    string finalcmd = SSIDCommand + Convert.ToString(byte6Checksum, 16);

                    Debug.WriteLine("SSID Send Start -------------" + finalcmd + "-------------");
                    //var byteData = HexStringToByteArray(usbcmd);
                    var write_ssid = await dongleCommWin.WriteSsid(finalcmd);
                    var write_ssid_result = (byte[])write_ssid;
                    return_value = $"{return_value}\nSsid : {ByteArrayToString(write_ssid_result)}";

                    Debug.WriteLine("SSID Send End Response -------------" + BitConverter.ToString(write_ssid_result) + "-------------");


                    if (!string.IsNullOrEmpty(RouterPassword))
                    {
                        //// SSID PASSWORD
                        //string pw = "20" + (RouterPassword.Length + 5).ToString("X2") + "1701" + ToHex(RouterPassword) + "00";
                        //string wifipwcmd = ReadChecksum(RouterPassword, pw);
                        //Debug.WriteLine("Password Send Start -------------" + wifipwcmd + "-------------");
                        //var write_password = await dongleCommWin.WritePassword(wifipwcmd);
                        //var write_password_result = (byte[])securityAccess;
                        //Debug.WriteLine("Password Send End Response -------------" + BitConverter.ToString(write_password_result) + "-------------");


                        string passwordcmd = "1701" + ToHex(RouterPassword) + "00";
                        string PasswordCommand = "20" + (RouterPassword.Length + 5).ToString("X2") + passwordcmd;

                        var dataBytes_pass = HexStringToByteArray(passwordcmd);
                        var byte6Checksum_pass = Crc16CcittKermit.ComputeChecksum(dataBytes_pass);

                        string finalcmd_pass = PasswordCommand + Convert.ToString(byte6Checksum_pass, 16);

                        Debug.WriteLine("PASSWORD Send Start -------------" + finalcmd_pass + "-------------");
                        //var byteData = HexStringToByteArray(usbcmd);
                        var write_password = await dongleCommWin.WritePassword(finalcmd_pass);
                        var write_password_result = (byte[])write_password;
                        return_value = $"{return_value}\nPassword : {ByteArrayToString(write_password_result)}";

                        Debug.WriteLine("PASSWORD Send End Response -------------" + BitConverter.ToString(write_password_result) + "-------------");
                    }
                }

                else
                {
                    return_value = $"Doungle Not Connected";
                }


                return return_value;
            }
            catch (Exception ex)
            {
                //tcp_Client.Close();
                return $"{return_value}\n\n{ex.Message}";
            }
        }


        private string ReadChecksum(string input, string command)
        {
            var dataBytes = HexStringToByteArray(command.Substring(4));
            var Checksum = Crc16CcittKermit.ComputeChecksum(dataBytes);
            command = command + Checksum.ToString("X4");

            var byte1 = "00";
            var byte2 = (command.Length / 2).ToString("X4");
            var byte3 = DateTime.Now.ToString("hhmmssff");
            var byte4 = DateTime.Now.ToString("00000000");
            //var byte5 = ByteArrayToString(Encoding.ASCII.GetBytes(command));
            var dataBytes_ = HexStringToByteArray(byte1 + byte2 + byte3 + byte4 + command);
            var byte6Checksum = Crc16CcittKermit.ComputeChecksum(dataBytes_);
            var returnstr = byte1 + byte2 + byte3 + byte4 + command + byte6Checksum.ToString("X4");
            return returnstr;
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

        private string ToHex(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
                sb.AppendFormat("{0:X2}", (int)c);
            return sb.ToString().Trim();
        }


        public async Task<string> Connect()
        {
            try
            {
                var dongle_response = await connection();
                if (dongle_response == "Dongle Connected")
                {
                    string tx_header_temp = string.Empty;
                    string rx_header_temp = string.Empty;


                    tx_header_temp = StaticData.ecu_info.FirstOrDefault().tx_header;
                    rx_header_temp = StaticData.ecu_info.FirstOrDefault().rx_header;

                    var securityAccess = await dongleCommWin.SecurityAccess();
                    var securityResult = (byte[])securityAccess;
                    var securityResponse = ByteArrayToString(securityResult);
                    var setProtocol = await dongleCommWin.Dongle_SetProtocol(4);
                    var ProtocolResult = (byte[])setProtocol;
                    var ProtocolResponse = ByteArrayToString(ProtocolResult);
                    var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header_temp);
                    var HeaderResult = (byte[])setHeader;
                    var HeaderResponse = ByteArrayToString(HeaderResult);
                    var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(rx_header_temp);
                    var HeaderMarkResult = (byte[])setHeaderMask;
                    var HeaderMarkResponse = ByteArrayToString(HeaderMarkResult);

                    var setp2max = await dongleCommWin.CAN_SetP2Max("2710");
                    var setp2maxbyte = (byte[])setp2max;
                    var setp2maxstr = ByteArrayToString(setp2maxbyte);


                    var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                    var firmwareResult = (byte[])firmwareVersion;
                    var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");

                    var version = ByteArrayToString(firmwareResult);
                    getver(ver);
                    return ver;
                }
                else
                {
                    return dongle_response;
                }
                //return $"security Response = {securityResponse} \nProtocol Response = {ProtocolResponse} \nHeader Response = {HeaderResponse} \nHeader Mark Response = {HeaderMarkResponse}";

            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async Task<bool> DisconnectUsb()
        {
            if (serialDevice != null)
            {
                serialDevice.Dispose();
                serialDevice = null;
                return true;
            }
            else
            {
                return true;
            }
        }

        public async Task<Model.ReadDtcResponseModel> ReadDtc(string dtc_index)
        {
            try
            {
                APDiagnostic.Models.ReadDtcResponseModel readDTCResponse = new APDiagnostic.Models.ReadDtcResponseModel();
                Model.ReadDtcResponseModel readDtcResponseModel = new Model.ReadDtcResponseModel();
                ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
                await Task.Run(async () =>
                {
                    readDTCResponse = await dSDiagnostic.ReadDTC(index);
                });

                readDtcResponseModel.dtcs = readDTCResponse.dtcs;
                readDtcResponseModel.status = readDTCResponse.status;
                readDtcResponseModel.noofdtc = readDTCResponse.noofdtc;
                //string dtcCode = string.Empty;
                //var responseLlength = readDTCResponse.dtcs.GetLength(0);
                //for (int i = 0; i <= responseLlength - 1; i++)
                //{
                //    dtcCode += readDTCResponse[i, 0].ToString() + " - ";
                //    Console.WriteLine(i);

                //}

                //var bytesData = (byte[])readDTCResponse;

                //if (bytesData[7] == 0x7f)
                //{
                //    readDTCResponse = await dongleCommWin.ReadData();
                //}
                return readDtcResponseModel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Model.ReadDtcResponseModel> oldReadDtc(string dtc_index)
        {
            try
            {
                if (dSDiagnostic != null)
                {
                    Model.ReadDtcResponseModel readDtcResponseModel = new Model.ReadDtcResponseModel();
                    ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);

                    await Task.Run(async () =>
                    {
                        readDTCResponse = await dSDiagnostic.ReadDTC(index);
                    });

                    readDtcResponseModel.dtcs = readDTCResponse.dtcs;
                    readDtcResponseModel.status = readDTCResponse.status;
                    readDtcResponseModel.noofdtc = readDTCResponse.noofdtc;
                    return readDtcResponseModel;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Model.ClearDtcResponseModel> GreavesClearDtc(string dtc_index)
        {
            try
            {
                object result = new object();
                //string result = string.Empty;
                //Model.ReadDtcResponseModel readDtcResponseModel = new Model.ReadDtcResponseModel();
                ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
                await Task.Run(async () =>
                {
                    //result = await dSDiagnostic.ClearDTC(index);
                    result = await dSDiagnostic.ClearDTC(index);
                });

                var res = JsonConvert.SerializeObject(result);
                var res_list = JsonConvert.DeserializeObject<Model.ClearDtcResponseModel>(res);
                return res_list;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ObservableCollection<Model.ReadPidPresponseModel>> ReadPid(ObservableCollection<Model.ReadParameterPID> pidList)
        {
            try
            {
                object result = new object();
                ObservableCollection<ReadParameterPID> list = new ObservableCollection<ReadParameterPID>();
                foreach (var item in pidList)
                {
                    var MessageValueList = new List<SelectedParameterMessage>();
                    if (item.messages != null)
                    {
                        foreach (var MessageItem in item.messages)
                        {
                            MessageValueList.Add(new SelectedParameterMessage { pid = item.pid, code = MessageItem.code, message = MessageItem.message });
                        }
                    }

                    var List = MessageValueList.Where(x => x.pid == item.pid).ToList();

                    list.Add(
                        new ReadParameterPID
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
                            messages = List
                        });
                }

                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.ReadParameters(pidList.Count, list);
                    var res = JsonConvert.SerializeObject(result);
                    var res_list = JsonConvert.DeserializeObject<ObservableCollection<Model.ReadPidPresponseModel>>(res);
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

        //public async Task<ObservableCollection<Models.ReadPidPresponseModel>> ReadPid(ObservableCollection<Models.ReadParameterPID> pidList)
        //{
        //    try
        //    {
        //        object result = new object();
        //        ObservableCollection<APDiagnostic.Models.ReadParameterPID> list = new ObservableCollection<ReadParameterPID>();
        //        foreach (var item in pidList)
        //        {
        //            list.Add(
        //                new ReadParameterPID
        //                {
        //                    datatype = item.datatype,
        //                    IsBitcoded = item.IsBitcoded,
        //                    noofBits = item.noofBits,
        //                    noOfBytes = item.noOfBytes,
        //                    offset = item.offset,
        //                    pid = item.pid,
        //                    resolution = item.resolution,
        //                    startBit = item.startBit,
        //                    startByte = item.startByte,
        //                    //totalBytes= item.totalBytes,
        //                    totalLen = item.totalLen,
        //                    pidNumber = item.pidNumber,
        //                    pidName = item.pidName
        //                });
        //        }

        //        var respo = await Task.Run(async () =>
        //        {
        //            result = await dSDiagnostic.ReadParameters(pidList.Count, list);
        //            var res = JsonConvert.SerializeObject(result);
        //            var res_list = JsonConvert.DeserializeObject<ObservableCollection<Models.ReadPidPresponseModel>>(res);
        //            return res_list;
        //            //status = Response.ECUResponseStatus;
        //        });

        //        return respo;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        public void getver(string hexString)
        {
            try
            {
                uint num = uint.Parse(hexString, System.Globalization.NumberStyles.AllowHexSpecifier);

                byte[] floatVals = BitConverter.GetBytes(num);
                float f = BitConverter.ToSingle(floatVals, 0);
                Console.WriteLine("float convert = {0}", f);
            }
            catch (Exception ex)
            {
            }
        }

        public void GetVersion(string hexComm)
        {
            string hexValues = "48 65 6C 6C 6F 20 57 6F 72 6C 64 21";
            string[] hexValuesSplit = hexValues.Split(' ');
            foreach (string hex in hexValuesSplit)
            {
                // Convert the number expressed in base-16 to an integer.
                int value = Convert.ToInt32(hex, 16);
                // Get the character corresponding to the integral value.
                string stringValue = Char.ConvertFromUtf32(value);
                char charValue = (char)value;
                Console.WriteLine("hexadecimal value = {0}, int value = {1}, char value = {2} or {3}",
                                hex, value, stringValue, charValue);
            }
        }
        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        //public async Task<object> ReadPid(List<Models.PidCode> pidList)
        //{
        //    try
        //    {
        //        object result = new object();
        //        ObservableCollection<APDiagnostic.Models.ReadParameterPID> list = new ObservableCollection<APDiagnostic.Models.ReadParameterPID>();
        //        foreach (var item in pidList)
        //        {
        //            list.Add(
        //                new ReadParameterPID
        //                {
        //                    pid = item.code,
        //                    totalLen = item.code.Length / 2,
        //                    //totalbyte -
        //                    startByte = item.byte_position,
        //                    noOfBytes = item.length,


        //                    IsBitcoded = item.bitcoded,
        //                    //noofBits = (int?)item.start_bit_position - (int?)item.end_bit_position + 1,
        //                    startBit = Convert.ToInt32(item.start_bit_position),
        //                    noofBits = item.end_bit_position.GetValueOrDefault() - item.start_bit_position.GetValueOrDefault() + 1,
        //                    resolution = Convert.ToInt32(item.resolution),
        //                    offset = Convert.ToInt32(item.offset),

        //                    datatype = item.message_type,
        //                    //totalBytes = item.length,
        //                    pidNumber = item.id,
        //                    pidName = item.long_name

        //                });
        //        }

        //        await Task.Run(async () =>
        //        {
        //            result = await dSDiagnostic.ReadParameters(pidList.Count, list);
        //        });

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}


        public async Task<ObservableCollection<Model.WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList)
        {
            try
            {
                object result = new object();
                WriteParameterIndex index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), write_pid_intdex);
                ObservableCollection<WriteParameterPID> list = new ObservableCollection<WriteParameterPID>();
                foreach (var item in pidList)
                {
                    SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), item.seedkeyindex);
                    WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), item.writepamindex);
                    list.Add(
                        new WriteParameterPID
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

                await Task.Run(async () =>
                {
                    result = await dSDiagnostic.WriteParameters(list.Count, index, list);
                });
                var res = JsonConvert.SerializeObject(result);
                var res_list = JsonConvert.DeserializeObject<ObservableCollection<Model.WriteParameter_Status>>(res);
                return res_list;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #region Write Parameter Old Code
        //public async Task<ObservableCollection<Models.WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<Models.WriteParameterPID> pidList)
        //{
        //    try
        //    {
        //        object result = new object();
        //        WriteParameterIndex index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), write_pid_intdex);
        //        ObservableCollection<WriteParameterPID> list = new ObservableCollection<WriteParameterPID>();
        //        foreach (var item in pidList)
        //        {
        //            SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), item.seedkeyindex);
        //            WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), item.writepamindex);
        //            list.Add(
        //                new WriteParameterPID
        //                {
        //                    seedkeyindex = seed_index,//item.seedkeyindex,
        //                    writepamindex = write_index, //item.writepamindex,
        //                    writeparadata = item.writeparadata,
        //                    writeparadatasize = item.writeparadatasize,
        //                    writeparapid = item.writeparapid,
        //                    ReadParameterPID_DataType = item.ReadParameterPID_DataType,
        //                    pid = item.pid,
        //                    startByte = item.startByte,
        //                    totalBytes = item.totalBytes
        //                    //writeparaName = item.
        //                });
        //        }

        //        await Task.Run(async () =>
        //        {
        //            result = await dSDiagnostic.WriteParameters(list.Count, index, list);
        //        });
        //        var res = JsonConvert.SerializeObject(result);
        //        var res_list = JsonConvert.DeserializeObject<ObservableCollection<Models.WriteParameter_Status>>(res);
        //        return res_list;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        #endregion
        public async Task<object> StartECUFlashing(string flashJson, Model.Ecu2 ecu2, Model.SeedkeyalgoFnIndex sklFIN, List<Model.EcuMapFile> ecu_map_file)

        {
            try
            {
                var jsonData = JsonConvert.DeserializeObject<FlashingMatrixData>(flashJson);

                EraseSectorEnum erase_type = (EraseSectorEnum)Enum.Parse(typeof(EraseSectorEnum), ecu2.flash_erase_type);
                ChecksumSectorEnum check_sum_type = (ChecksumSectorEnum)Enum.Parse(typeof(ChecksumSectorEnum), ecu2.flash_check_sum_type);
                SEEDKEYINDEXTYPE seedkeyindx = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), sklFIN.value);

                //if (ecu2.flash_address_data_format == "0x33")
                //{
                //    flashconfig.addrdataformat = 0x33;
                //}
                //else if (ecu2.flash_address_data_format == "0x44")
                //{
                //    flashconfig.addrdataformat = 0x44;
                //}
                //else
                //{
                //    // return_status = WRONG_ADDRESS_DATA_FORMAT;
                //    // return return_status;
                //}




                //BOSH

                //var flashConfig = new flashconfig
                //{
                //    addrdataformat = 0x33,
                //    //sectorframetransferlen = 0x30,
                //    sectorframetransferlen = 0x03B0,
                //    // seedkeyindex=0x00 //-num,
                //    seedkeynumbytes = 0x04,
                //    //sendsectorchksum = false,
                //    sendseedbyte = 0x09,
                //    septime = 0x00,
                //    diag_mode = 0x02,
                //    flash_index = 0x00,
                //    erasesector = EraseSectorEnum.ERASEBYSECTOR,
                //    checksumsector = ChecksumSectorEnum.COMPAREBYSECTOR
                //};


                // advantek

                //var flashConfig = new flashconfig
                //{
                //    addrdataformat = 0x44,
                //    //sectorframetransferlen = 0x0fc,
                //    sectorframetransferlen = 0x0fc,
                //    seedkeyindex = seedkeyindx,
                //    seedkeynumbytes = 0x10,
                //    //sendsectorchksum = false,
                //    sendseedbyte = 0x01,
                //    septime = 0x00,
                //    diag_mode = 0x02,
                //    //flash_index = 0x00,
                //    erasesector = EraseSectorEnum.ERASEALLATONCE,
                //    checksumsector = ChecksumSectorEnum.NOCHECKSUM
                //};

                //var static_json = JsonConvert.SerializeObject(flashConfig);

                var flashConfig = new flashconfig
                {
                    addrdataformat = Convert.ToByte(ecu2.flash_address_data_format, 16),
                    checksumsector = check_sum_type,
                    diag_mode = Convert.ToByte(ecu2.flash_diagnostic_mode, 16),
                    erasesector = erase_type,
                    //flash_index = FLASHINDEXTYPE.GREAVES_BOSCH_BS6,
                    sectorframetransferlen = Convert.ToUInt16(ecu2.sectorframetransferlen, 16),
                    seedkeyindex = seedkeyindx,
                    seedkeynumbytes = Convert.ToByte(ecu2.flash_seed_key_length, 16),
                    sendseedbyte = Convert.ToByte(ecu2.sendseedbyte, 16),
                    septime = Convert.ToByte(ecu2.flashsep_time, 16),
                };
                var dynamic_json = JsonConvert.SerializeObject(flashConfig);
                string response = string.Empty;

                ////var flashConfig = new flashconfig();
                ////flashConfig.addrdataformat = byte.Parse(ecu2.flash_address_data_format);//Convert.ToByte(ecu2.flash_address_data_format);
                ////flashConfig.sectorframetransferlen = 0x0fc;//byte.Parse(ecu2.sectorframetransferlen);
                ////flashConfig.seedkeynumbytes = Convert.ToByte(ecu2.flash_seed_key_length);
                ////flashConfig.sendseedbyte = Convert.ToByte(ecu2.sendseedbyte);
                ////flashConfig.septime = Convert.ToByte(ecu2.flashsep_time);
                ////flashConfig.diag_mode = Convert.ToByte(ecu2.flash_diagnostic_mode);
                ////flashConfig.erasesector = (EraseSectorEnum)Enum.Parse(typeof(EraseSectorEnum), ecu2.flash_erase_type);
                ////flashConfig.checksumsector = (ChecksumSectorEnum)Enum.Parse(typeof(ChecksumSectorEnum), ecu2.flash_check_sum_type);

                //flashConfig.checksumsector = ChecksumSectorEnum.COMPAREBYSECTOR;
                //var dynamic_json = JsonConvert.SerializeObject(flashConfig);
                //var flashConfig = new flashconfig
                //{
                //    addrdataformat = 0x33,
                //    sectorframetransferlen = 0x30,
                //    // seedkeyindex=0x00 //-num,
                //    seedkeynumbytes = 0x04,
                //    //sendsectorchksum = false,
                //    sendseedbyte = 0x09,
                //    septime = 0x00,
                //    diag_mode = 0x02,
                //    flash_index = 0x00,
                //    erasesector = EraseSectorEnum.ERASEBYSECTOR,
                //    checksumsector = ChecksumSectorEnum.COMPAREBYSECTOR
                //};


                /* ECU Mem Map                        */
                /* Sector 1 - 0x80004000 - 0x80013fff */
                /* Sector 2 - 0x80080000 - 0x8017ffff */
                /* Sector 3 - 0x80020000 - 0x8007ffff */

                //if (ecu_map_file.Count == jsonData.NoOfSectors)
                //{
                //}

                //////int i = 0;
                ////////ecu_map_file.Reverse();
                //////foreach (var map_file in jsonData.SectorData)
                //////{
                //////    jsonData.SectorData[i].ECUMemMapStartAddress = map_file.ECUMemMapStartAddress;
                //////    jsonData.SectorData[i].ECUMemMapEndAddress = map_file.ECUMemMapEndAddress;
                //////    i++;
                //////}
                ///

                /* BOSCH*/

                //jsonData.SectorData[0].ECUMemMapStartAddress = "80004000";
                //jsonData.SectorData[0].ECUMemMapEndAddress = "80013fff";

                //jsonData.SectorData[1].ECUMemMapStartAddress = "80000000";
                //jsonData.SectorData[1].ECUMemMapEndAddress = "80003fff";


                //jsonData.SectorData[2].ECUMemMapStartAddress = "80080000";
                //jsonData.SectorData[2].ECUMemMapEndAddress = "8017ffff";

                //jsonData.SectorData[3].ECUMemMapStartAddress = "80020000";
                //jsonData.SectorData[3].ECUMemMapEndAddress = "8007ffff";

                /* ADVANTEK */

                //jsonData.SectorData[0].ECUMemMapStartAddress = "00020000";
                //jsonData.SectorData[0].ECUMemMapEndAddress = "0004FFFF";

                //jsonData.SectorData[1].ECUMemMapStartAddress = "00050000";
                //jsonData.SectorData[1].ECUMemMapEndAddress = "000FFFFF";




                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                await Task.Run(async () =>
                {
                    if (jsonData != null)
                    {
                        response = await dSDiagnostic.StartFlashBosch(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                    }
                });

                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime); return response;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void Cancel()
        {
            //throw new NotImplementedException();
        }
        public async Task<string> ClearDtc(string dtc_index)
        {
            try
            {
                string status = string.Empty;
                object result = new object();
                ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
                await Task.Run(async () =>
                {
                    result = await dSDiagnostic.ClearDTC(index);
                    var res = JsonConvert.SerializeObject(result);
                    var Response = JsonConvert.DeserializeObject<Model.ClearDtcResponseModel>(res);
                    status = Response.ECUResponseStatus;
                });

                return status;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public async Task TXheader(string tx_header)
        {
            var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header);
            var HeaderResult = (byte[])setHeader;
            var HeaderResponse = ByteArrayToString(HeaderResult);
            Debug.WriteLine("------DTC TX Header Set------" + tx_header + " ", "Header Response--" + HeaderResponse);
        }
        public async Task RXheader(string rx_header)
        {
            var setHeader = await dongleCommWin.CAN_SetRxHeaderMask(rx_header);
            var HeaderResult = (byte[])setHeader;
            var HeaderResponse = ByteArrayToString(HeaderResult);
            Debug.WriteLine("------DTC RX Header Set------" + rx_header + " ", "RX Header Response--" + HeaderResponse);
        }
    }

    #region Old Code
    //class USBDllFunctions : IConnectionUSB
    //{
    //    DongleCommWin dongleCommWin;
    //    UDSDiagnostic dSDiagnostic;
    //    private APDiagnostic.Models.ReadDtcResponseModel readDTCResponse;
    //    SerialDevice serialDevice = null;
    //    DeviceInformationCollection devices = null;
    //    DeviceInformation deviceInfo = null;
    //    //ECUCalculateSeedkey calculateSeedkey;


    //    string DebugTag = "Write SSID PASSWORD";

    //    public async Task<string> connection()
    //    {
    //        try
    //        {
    //            var Values = DisconnectUsb();

    //            //string selector = SerialDevice.GetDeviceSelector("COM5");
    //            var deviceSelector = SerialDevice.GetDeviceSelector();

    //            devices = await DeviceInformation.FindAllAsync(deviceSelector);
    //            if (devices.Count > 0)
    //            {
    //                deviceInfo = devices.FirstOrDefault(x => x.Name.Contains("Silicon Labs CP210x"));
    //                if (deviceInfo != null)
    //                {
    //                    serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);

    //                    if (serialDevice != null)
    //                    {
    //                        if (deviceInfo.Id.Contains("202"))
    //                        {
    //                            serialDevice.BaudRate = 460800;
    //                        }
    //                        else
    //                        {
    //                            serialDevice.BaudRate = 115200;
    //                        }

    //                        serialDevice.ReadTimeout = TimeSpan.FromMilliseconds(2);
    //                        serialDevice.WriteTimeout = TimeSpan.FromMilliseconds(2);
    //                        //serialDevice.Handshake = SerialHandshake.RequestToSendXOnXOff;
    //                        dongleCommWin = new DongleCommWin(serialDevice, APDongleCommWin.Protocol.ISO15765_500KB_11BIT_CAN, 0x7E0, 0x7E8, 0x00, 0x10, 0x10, 0x10);
    //                        dongleCommWin.InitializePlatform(APDongleCommWin.ENUMS.Platform.UWP, APDongleCommWin.ENUMS.Connectivity.USB);
    //                        dSDiagnostic = new UDSDiagnostic(dongleCommWin);
    //                        return "Dongle Connected";
    //                    }
    //                    else
    //                    {
    //                        return "Dongle Already Connected !!!";
    //                    }
    //                }
    //                else
    //                {
    //                    return "Dongle not found";
    //                }
    //            }
    //            else
    //            {
    //                return "Dongle not found";
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            return $"{ex.Message}\n\n{ex.StackTrace}";
    //        }
    //    }
    //    public async Task<string> Connect()
    //    {
    //        try
    //        {
    //            var dongle_response = await connection();
    //            if (dongle_response == "Dongle Connected")
    //            {

    //                var securityAccess = await dongleCommWin.SecurityAccess();
    //                var securityResult = (byte[])securityAccess;
    //                var securityResponse = ByteArrayToString(securityResult);
    //                var setProtocol = await dongleCommWin.Dongle_SetProtocol(2);
    //                var ProtocolResult = (byte[])setProtocol;
    //                var ProtocolResponse = ByteArrayToString(ProtocolResult);
    //                var setHeader = await dongleCommWin.CAN_SetTxHeader("07e0");
    //                var HeaderResult = (byte[])setHeader;
    //                var HeaderResponse = ByteArrayToString(HeaderResult);
    //                var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask("07e8");
    //                var HeaderMarkResult = (byte[])setHeaderMask;
    //                var HeaderMarkResponse = ByteArrayToString(HeaderMarkResult);

    //                var setp2max = await dongleCommWin.CAN_SetP2Max("2710");
    //                var setp2maxbyte = (byte[])setp2max;
    //                var setp2maxstr = ByteArrayToString(setp2maxbyte);


    //                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
    //                var firmwareResult = (byte[])firmwareVersion;
    //                var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");

    //                var version = ByteArrayToString(firmwareResult);
    //                getver(ver);
    //                return ver;
    //            }
    //            else
    //            {
    //                return dongle_response;
    //            }
    //            //return $"security Response = {securityResponse} \nProtocol Response = {ProtocolResponse} \nHeader Response = {HeaderResponse} \nHeader Mark Response = {HeaderMarkResponse}";

    //        }
    //        catch (Exception ex)
    //        {
    //            return "";
    //        }
    //    }
    //    public async Task<bool> DisconnectUsb()
    //    {
    //        if (serialDevice != null)
    //        {
    //            serialDevice.Dispose();
    //            serialDevice = null;
    //            return true;
    //        }
    //        else
    //        {
    //            return true;
    //        }
    //    }

    //    public async Task Oldconnection()
    //    {
    //        int Count = 0;
    //        string deviceID = string.Empty;
    //        DeviceInformation deviceInfo = null;
    //        string aqsFilter = SerialDevice.GetDeviceSelector();
    //        var deviceInformation = await DeviceInformation.FindAllAsync(aqsFilter);

    //        var myDevices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync();
    //        var resultList = myDevices.Where(s => s.Name.Contains("Silicon")).ToList<DeviceInformation>();

    //        for (Count = 0; Count < deviceInformation.Count; Count++)
    //        {
    //            System.Diagnostics.Debug.WriteLine("UART Port: " + deviceInformation[Count].Name + ", " + deviceInformation[Count].Id);
    //            if (deviceInformation[Count].Name.Contains("Silicon"))
    //            {
    //                deviceInfo = deviceInformation[Count];
    //                break;
    //            }
    //        }//We are looking for a FDTI USB to serial USB device

    //        if (deviceInfo.Name.Contains("Silicon"))
    //        {
    //            //----- SERIAL PORT FOUND -----
    //            var ConfigAppPort = await SerialDevice.FromIdAsync(deviceInfo.Id);

    //            //Configure serial settings
    //            //ConfigAppPort.BaudRate = 115200;
    //            ConfigAppPort.BaudRate = 460800;

    //            ConfigAppPort.ReadTimeout = TimeSpan.FromMilliseconds(2);
    //            ConfigAppPort.WriteTimeout = TimeSpan.FromMilliseconds(2);

    //            dongleCommWin = new DongleCommWin(ConfigAppPort, Protocol.ISO15765_500KB_11BIT_CAN
    //                , 0x7E0, 0x7E8, 0x00, 0x10, 0x10, 0x10);
    //            dongleCommWin.InitializePlatform(APDongleCommWin.ENUMS.Platform.UWP, APDongleCommWin.ENUMS.Connectivity.USB);

    //            dSDiagnostic = new UDSDiagnostic(dongleCommWin);
    //        }
    //        //#region OLD
    //        //try
    //        //{
    //        //    string selector = SerialDevice.GetDeviceSelector("COM3");
    //        //    var deviceSelector = SerialDevice.GetDeviceSelector();
    //        //    DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);
    //        //    if (devices.Count > 0)
    //        //    {
    //        //        DeviceInformation deviceInfo = devices[0];
    //        //        SerialDevice serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id);
    //        //        serialDevice.ReadTimeout = TimeSpan.FromMilliseconds(2);
    //        //        serialDevice.WriteTimeout = TimeSpan.FromMilliseconds(2);
    //        //        dongleCommWin = new DongleCommWin(serialDevice, APDongleCommWin.Protocol.ISO15765_500KB_11BIT_CAN, 0x7E0, 0x7E8, 0x00, 0x10, 0x10, 0x10);

    //        //        dSDiagnostic = new UDSDiagnostic(dongleCommWin);



    //        //    }
    //        //    else
    //        //    {

    //        //    }
    //        //}
    //        //catch (Exception ex)
    //        //{
    //        //} 
    //        //#endregion
    //    }
    //    public async Task<string> OldConnect()
    //    {
    //        try
    //        {
    //            await connection();
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
    //            getver(ver);
    //            return ver;
    //            //return $"security Response = {securityResponse} \nProtocol Response = {ProtocolResponse} \nHeader Response = {HeaderResponse} \nHeader Mark Response = {HeaderMarkResponse}";
    //        }
    //        catch (Exception ex)
    //        {
    //            return "";
    //        }
    //    }

    //    public async Task<Model.ReadDtcResponseModel> ReadDtc(string dtc_index)
    //    {
    //        try
    //        {
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

    //    public async Task<object> OldClearDtc(string dtc_index)
    //    {
    //        try
    //        {
    //            string status = string.Empty;
    //            object result = new object();
    //            ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
    //            await Task.Run(async () =>
    //            {
    //                result = await dSDiagnostic.ClearDTC(index);
    //                var res = JsonConvert.SerializeObject(result);
    //                var Response = JsonConvert.DeserializeObject<Model.ClearDtcResponseModel>(res);
    //                status = Response.ECUResponseStatus;
    //            });

    //            return status;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }

    //    public async Task<object> OldReadPid(ObservableCollection<Model.ReadParameterPID> pidList)
    //    {
    //        try
    //        {
    //            object result = new object();
    //            ObservableCollection<APDiagnostic.Models.ReadParameterPID> list = new ObservableCollection<ReadParameterPID>();
    //            foreach (var item in pidList)
    //            {
    //                list.Add(
    //                    new ReadParameterPID
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
    //                        //totalBytes= item.totalBytes,
    //                        totalLen = item.totalLen,
    //                        pidNumber = item.pidNumber,
    //                        pidName = item.pidName
    //                    });
    //            }

    //            await Task.Run(async () =>
    //            {
    //                result = await dSDiagnostic.ReadParameters(pidList.Count, list);
    //            });

    //            return result;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }

    //    //public async Task<object> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList)
    //    //{
    //    //    try
    //    //    {
    //    //        object result = new object();
    //    //        WriteParameterIndex index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), "UDS_DS1003_SK0B0C");
    //    //        ObservableCollection<WriteParameterPID> list = new ObservableCollection<WriteParameterPID>();
    //    //        foreach (var item in pidList)
    //    //        {
    //    //            SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), item.seedkeyindex);
    //    //            WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), item.writepamindex);
    //    //            list.Add(
    //    //                new WriteParameterPID
    //    //                {
    //    //                    seedkeyindex = seed_index,//item.seedkeyindex,
    //    //                    writepamindex = write_index, //item.writepamindex,
    //    //                    writeparadata = item.writeparadata,
    //    //                    writeparadatasize = item.writeparadatasize,
    //    //                    writeparapid = item.writeparapid,
    //    //                    //writeparaName = item.
    //    //                });
    //    //        }

    //    //        await Task.Run(async () =>
    //    //        {
    //    //            result = await dSDiagnostic.WriteParameters(pidList.Count, index, list);
    //    //        });

    //    //        return result;
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        return null;
    //    //    }
    //    //}

    //    public void getver(string hexString)
    //    {
    //        try
    //        {
    //            uint num = uint.Parse(hexString, System.Globalization.NumberStyles.AllowHexSpecifier);

    //            byte[] floatVals = BitConverter.GetBytes(num);
    //            float f = BitConverter.ToSingle(floatVals, 0);
    //            Console.WriteLine("float convert = {0}", f);
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }

    //    public void GetVersion(string hexComm)
    //    {
    //        string hexValues = "48 65 6C 6C 6F 20 57 6F 72 6C 64 21";
    //        string[] hexValuesSplit = hexValues.Split(' ');
    //        foreach (string hex in hexValuesSplit)
    //        {
    //            // Convert the number expressed in base-16 to an integer.
    //            int value = Convert.ToInt32(hex, 16);
    //            // Get the character corresponding to the integral value.
    //            string stringValue = Char.ConvertFromUtf32(value);
    //            char charValue = (char)value;
    //            Console.WriteLine("hexadecimal value = {0}, int value = {1}, char value = {2} or {3}",
    //                            hex, value, stringValue, charValue);
    //        }
    //    }
    //    private string ByteArrayToString(byte[] ba)
    //    {
    //        string hex = BitConverter.ToString(ba);
    //        return hex.Replace("-", "");
    //    }

    //    public void Cancel()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    //Task<string> IConnectionUSB.ClearDtc(string indexKey)
    //    //{
    //    //    throw new NotImplementedException();
    //    //}

    //    public async Task<object> StartECUFlashing(string flashJson, Model.Ecu2 ecu2, Model.SeedkeyalgoFnIndex sklFIN, List<Model.EcuMapFile> ecu_map_file)
    //    {
    //        try
    //        {
    //            var jsonData = JsonConvert.DeserializeObject<FlashingMatrixData>(flashJson);

    //            //if (ecu2.flash_address_data_format == "0x33")
    //            //{
    //            //    flashconfig.addrdataformat = 0x33;
    //            //}
    //            //else if (ecu2.flash_address_data_format == "0x44")
    //            //{
    //            //    flashconfig.addrdataformat = 0x44;
    //            //}
    //            //else
    //            //{
    //            //    // return_status = WRONG_ADDRESS_DATA_FORMAT;
    //            //    // return return_status;
    //            //}




    //            //BOSH

    //            var flashConfig = new flashconfig
    //            {
    //                addrdataformat = 0x33,
    //                //sectorframetransferlen = 0x30,
    //                sectorframetransferlen = 0x03B0,
    //                // seedkeyindex=0x00 //-num,
    //                seedkeynumbytes = 0x04,
    //                //sendsectorchksum = false,
    //                sendseedbyte = 0x09,
    //                septime = 0x00,
    //                diag_mode = 0x02,
    //                flash_index = 0x00,
    //                erasesector = EraseSectorEnum.ERASEBYSECTOR,
    //                checksumsector = ChecksumSectorEnum.COMPAREBYSECTOR
    //            };


    //            //var flashConfig = new flashconfig();

    //            //flashConfig.addrdataformat = byte.Parse(ecu2.flash_address_data_format);//Convert.ToByte(ecu2.flash_address_data_format);
    //            //flashConfig.sectorframetransferlen = Convert.ToByte(ecu2.sectorframetransferlen);
    //            ////flashConfig.sectorframetransferlen = 0x30;

    //            //flashConfig.seedkeynumbytes = Convert.ToByte(ecu2.flash_seed_key_length);
    //            //flashConfig.sendseedbyte = Convert.ToByte(ecu2.sendseedbyte);

    //            //flashConfig.septime = Convert.ToByte(ecu2.flashsep_time);
    //            //flashConfig.diag_mode = Convert.ToByte(ecu2.flash_diagnostic_mode);
    //            ////flash_index = Convert.ToByte(ecu2.),
    //            //flashConfig.erasesector = (EraseSectorEnum)Enum.Parse(typeof(EraseSectorEnum), ecu2.flash_erase_type);
    //            //flashConfig.checksumsector = (ChecksumSectorEnum)Enum.Parse(typeof(ChecksumSectorEnum), ecu2.flash_check_sum_type);
    //            //flashConfig.checksumsector = ChecksumSectorEnum.COMPAREBYSECTOR;

    //            //var flashConfig = new flashconfig
    //            //{
    //            //    addrdataformat = 0x33,
    //            //    sectorframetransferlen = 0x30,
    //            //    // seedkeyindex=0x00 //-num,
    //            //    seedkeynumbytes = 0x04,
    //            //    //sendsectorchksum = false,
    //            //    sendseedbyte = 0x09,
    //            //    septime = 0x00,
    //            //    diag_mode = 0x02,
    //            //    flash_index = 0x00,
    //            //    erasesector = EraseSectorEnum.ERASEBYSECTOR,
    //            //    checksumsector = ChecksumSectorEnum.COMPAREBYSECTOR
    //            //};


    //            /* ECU Mem Map                        */
    //            /* Sector 1 - 0x80004000 - 0x80013fff */
    //            /* Sector 2 - 0x80080000 - 0x8017ffff */
    //            /* Sector 3 - 0x80020000 - 0x8007ffff */

    //            //if (ecu_map_file.Count == jsonData.NoOfSectors)
    //            //{
    //            //}
    //            //int i = 0;
    //            //ecu_map_file.Reverse();
    //            //foreach (var map_file in ecu_map_file)
    //            //{
    //            //    jsonData.SectorData[i].ECUMemMapStartAddress = map_file.start_address;
    //            //    jsonData.SectorData[i].ECUMemMapEndAddress = map_file.end_address;
    //            //    i++;
    //            //}

    //            jsonData.SectorData[0].ECUMemMapStartAddress = "80004000";
    //            jsonData.SectorData[0].ECUMemMapEndAddress = "80013fff";

    //            jsonData.SectorData[1].ECUMemMapStartAddress = "80000000";
    //            jsonData.SectorData[1].ECUMemMapEndAddress = "80003fff";


    //            jsonData.SectorData[2].ECUMemMapStartAddress = "80080000";
    //            jsonData.SectorData[2].ECUMemMapEndAddress = "8017ffff";

    //            jsonData.SectorData[3].ECUMemMapStartAddress = "80020000";
    //            jsonData.SectorData[3].ECUMemMapEndAddress = "8007ffff";


    //            string response = string.Empty;

    //            Stopwatch stopWatch = new Stopwatch();
    //            stopWatch.Start();
    //            await Task.Run(async () =>
    //            {
    //                response = await dSDiagnostic.StartFlashBosch(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
    //            });

    //            stopWatch.Stop();
    //            // Get the elapsed time as a TimeSpan value.
    //            TimeSpan ts = stopWatch.Elapsed;

    //            // Format and display the TimeSpan value.
    //            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
    //                ts.Hours, ts.Minutes, ts.Seconds,
    //                ts.Milliseconds / 10);
    //            Console.WriteLine("RunTime " + elapsedTime); return response;
    //        }
    //        catch (Exception ex)
    //        {
    //            return ex.Message;
    //        }
    //    }

    //    public async Task<ObservableCollection<Model.WriteParameter_Status>> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList)
    //    {
    //        try
    //        {
    //            WriteParameterIndex index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), write_pid_intdex);
    //            ObservableCollection<APDiagnostic.Models.WriteParameterPID> list = new ObservableCollection<APDiagnostic.Models.WriteParameterPID>();
    //            foreach (var item in pidList)
    //            {
    //                SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), item.seedkeyindex);
    //                WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), item.writepamindex);
    //                list.Add(
    //                    new APDiagnostic.Models.WriteParameterPID
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
    //                var res_list = JsonConvert.DeserializeObject<ObservableCollection<Model.WriteParameter_Status>>(res);
    //                return res_list;
    //            });
    //            return respo;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }

    //    public async Task<string> ClearDtc(string indexKey)
    //    {
    //        try
    //        {
    //            string status = string.Empty;
    //            object result = new object();
    //            ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), indexKey);
    //            await Task.Run(async () =>
    //            {
    //                result = await dSDiagnostic.ClearDTC(index);
    //                var res = JsonConvert.SerializeObject(result);
    //                var Response = JsonConvert.DeserializeObject<Model.ClearDtcResponseModel>(res);
    //                status = Response.ECUResponseStatus;
    //            });

    //            return status;
    //        }
    //        catch (Exception ex)
    //        {
    //            return "";
    //        }
    //    }

    //    public async Task<ObservableCollection<Model.ReadPidPresponseModel>> ReadPid(ObservableCollection<Model.ReadParameterPID> pidList)
    //    {
    //        try
    //        {
    //            object result = new object();
    //            ObservableCollection<APDiagnostic.Models.ReadParameterPID> list = new ObservableCollection<APDiagnostic.Models.ReadParameterPID>();
    //            foreach (var item in pidList)
    //            {
    //                var MessageModels = new List<APDiagnostic.Models.SelectedParameterMessage>();
    //                if (item.messages != null)
    //                {
    //                    foreach (var MessageItem in item.messages)
    //                    {
    //                        MessageModels.Add(new APDiagnostic.Models.SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
    //                    }
    //                }
    //                list.Add(
    //                    new APDiagnostic.Models.ReadParameterPID
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
    //                        messages = MessageModels
    //                    });
    //            }

    //            var respo = await Task.Run(async () =>
    //            {
    //                result = await dSDiagnostic.ReadParameters(pidList.Count, list);
    //                var res = JsonConvert.SerializeObject(result);
    //                var res_list = JsonConvert.DeserializeObject<ObservableCollection<Model.ReadPidPresponseModel>>(res);
    //                return res_list;
    //            });
    //            return respo;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }
    //}
    #endregion
}
