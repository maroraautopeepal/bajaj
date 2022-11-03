using Android.Bluetooth;
using Android.Hardware.Usb;
using APDiagnosticAndroid;
using APDiagnosticAndroid.Enums;
using APDiagnosticAndroid.Models;
using APDiagnosticAndroid.Structures;
//using APDongleCommWin;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using Bajaj.Model;
using Hoho.Android.UsbSerial.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Android.Widget;
using Hoho.Android.UsbSerial.Driver;
using APDongleCommAndroid;
using APDongleCommAnroid.Models;
using System.IO;

[assembly: Dependency(typeof(Usb_Android))]




namespace Bajaj.Droid.Dependencies
{
    public class Usb_Android : IConnectionUSB
    {
        BluetoothSocket socket = null;
        DongleCommWin dongleCommWin;
        DongleCommWin dongleComm;
        UDSDiagnostic dSDiagnostic;
        //private APDiagnostic.Models.ReadDtcResponseModel readDTCResponse;
        SerialInputOutputManager serialIoManager;
        //UsbManager usbManager;

        UsbSerialPort port;
        SerialInputOutputManager inputOutputManager;

        string tx_header_temp = string.Empty;
        string rx_header_temp = string.Empty;
        int ProtocolValue = 0;
        public async Task<string> GetDongleMacID(bool is_disconnct)
        {
            try
            {
                string mac_id = string.Empty;
                //if (port != null)
                //{
                //    port.Close();
                //}

                //if (inputOutputManager != null)
                //{
                //    inputOutputManager.Close();
                //}
                if (!is_disconnct)
                {

                    var mainActivity = MainActivity.Instance;
                    if (mainActivity != null)
                    {
                        port = mainActivity.ReturnPort();
                        inputOutputManager = mainActivity.inputOutputManager();

                        //string ProtocolNameValue = StaticData.ecu_info.FirstOrDefault().protocol.name;//.protocol.name;
                        //string value = ProtocolNameValue.Replace("-", "_");

                        //tx_header_temp = StaticData.ecu_info.FirstOrDefault().tx_header;
                        //rx_header_temp = StaticData.ecu_info.FirstOrDefault().rx_header;

                        /////////////////////////////////////////////////////////////////////
                        ProtocolValue = 02;
                        tx_header_temp = "07e0";
                        rx_header_temp = "07e8";
                        /////////////////////////////////////////////////////////////////////

                        dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, ProtocolValue, Convert.ToUInt32(tx_header_temp, 16), Convert.ToUInt32(rx_header_temp, 16), 0x00, 0x10, 0x10, 0x10);

                        //dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x07E0, 0x07E8, 0x00, 0x10, 0x10, 0x10);
                        dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.USB);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
                        dSDiagnostic = new UDSDiagnostic(dongleCommWin);
                        if (dongleCommWin != null)
                        {
                            //var setProtocol = await dongleCommWin.Dongle_SetProtocol(ProtocolValue);
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
                }
                return mac_id;

            }
            catch (Exception ex)
            {
                port.Close();
                inputOutputManager.Close();
                return "";
            }
        }

        public async void SetDongleProperties(string ProtocolName, string tx_header_temp, string rx_header_temp)
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


        public async Task<string> SetDongleProperties()
        {
            try
            {
                string firmware_version = string.Empty;

                if (dongleCommWin != null)
                {
                    var setProtocol = await dongleCommWin.Dongle_SetProtocol(ProtocolValue);
                    var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header_temp);
                    var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(rx_header_temp);
                    var setPadding = await dongleCommWin.CAN_StartPadding("00");
                    //var setp2max = await dongleCommWin.CAN_SetP2Max("2710");

                    var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                    var firmwareResult = (byte[])firmwareVersion;
                    firmware_version = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                }
                return firmware_version;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async void DisconnectUSB()
        {
            var donglereset = await dongleCommWin.Dongle_Reset();
            dongleCommWin.USB_Disconnect();
        }
        public async Task<string> ConnectOld()
        {
            try
            {
                string firmware_version = string.Empty;
                var mainActivity = MainActivity.Instance;
                if (mainActivity != null)
                {
                    var port = mainActivity.ReturnPort();
                    var inputOutputManager = mainActivity.inputOutputManager();

                    //dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x7E0, 0x7E8, 0x00, 0x10, 0x10, 0x10);
                    dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.USB);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
                    dSDiagnostic = new UDSDiagnostic(dongleCommWin);
                    if (dongleCommWin != null)
                    {
                        // DependencyService.Get<Interfaces.IToastMessage>().Show("Sending Command");
                        var ab = new byte[] { };
                        var securityAccess = await dongleCommWin.SecurityAccess();
                        var securityResult = (byte[])securityAccess;


                        //if (securityResult == null)
                        //{
                        //    serialIoManager = new SerialInputOutputManager(port)
                        //    {
                        //        BaudRate = 460800,
                        //        DataBits = 8,
                        //        StopBits = StopBits.One,
                        //        Parity = Parity.None,
                        //    };

                        //    //serialIoManager.Open(usbManager);
                        //    securityAccess = await dongleCommWin.SecurityAccess();
                        //    securityResult = (byte[])securityAccess;                            
                        //}

                        var securityResponse = ByteArrayToString(securityResult);
                        var setProtocol = await dongleCommWin.Dongle_SetProtocol(2);
                        var ProtocolResult = (byte[])setProtocol;
                        var ProtocolResponse = ByteArrayToString(ProtocolResult);
                        var setHeader = await dongleCommWin.CAN_SetTxHeader("07e0");
                        var HeaderResult = (byte[])setHeader;
                        var HeaderResponse = ByteArrayToString(HeaderResult);
                        var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask("07e8");
                        var HeaderMarkResult = (byte[])setHeaderMask;
                        var HeaderMarkResponse = ByteArrayToString(HeaderMarkResult);
                        //Thread.Sleep(100);
                        var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                        var firmwareResult = (byte[])firmwareVersion;
                        //firmware_version = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                        //firmware_version = "temp version";
                        //var version = ByteArrayToString(firmwareResult);
                        //getver(ver);
                    }
                }

                return firmware_version;
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
                APDiagnosticAndroid.Models.ReadDtcResponseModel readDTCResponse = new APDiagnosticAndroid.Models.ReadDtcResponseModel();
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

        public async Task<string> ClearDtc(string dtc_index)
        {
            try
            {
                string status = string.Empty;
                object result = new object();
                ClearDTCIndex index = (ClearDTCIndex)Enum.Parse(typeof(ClearDTCIndex), dtc_index);
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

        public async Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<Model.ReadParameterPID> pidList)
        {
            try
            {
                object result = new object();
                ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID>();
                foreach (var item in pidList)
                {
                    var MessageModels = new List<APDiagnosticAndroid.Models.SelectedParameterMessage>();
                    if (item.messages != null)
                    {
                        foreach (var MessageItem in item.messages)
                        {
                            MessageModels.Add(new APDiagnosticAndroid.Models.SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
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
                            messages = MessageModels
                        });
                }

                var respo = await Task.Run(async () =>
                {
                    result = await dSDiagnostic.ReadParameters(pidList.Count, list);
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
        //public async Task<object> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList)
        //{
        //    try
        //    {
        //        object result = new object();
        //        WriteParameterIndex index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), "UDS_DS1003_SK0B0C");
        //        ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID>();
        //        foreach (var item in pidList)
        //        {
        //            SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), item.seedkeyindex);
        //            WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), item.writepamindex);
        //            list.Add(
        //                new APDiagnosticAndroid.Models.WriteParameterPID
        //                {
        //                    seedkeyindex = seed_index,//item.seedkeyindex,
        //                    writepamindex = write_index, //item.writepamindex,
        //                    writeparadata = item.writeparadata,
        //                    writeparadatasize = item.writeparadatasize,
        //                    writeparapid = item.writeparapid,
        //                    //writeparaName = item.
        //                });
        //        }

        //        await Task.Run(async () =>
        //        {
        //            result = await dSDiagnostic.WriteParameters(pidList.Count, index, list);
        //        });

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}


        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        //public async Task<string> ClearDtc(string indexKey)
        //{
        //    object result = new object();
        //    string Status = string.Empty;
        //    APDiagnosticAndroid.Models.ReadDtcResponseModel readDTCResponse = new APDiagnosticAndroid.Models.ReadDtcResponseModel();
        //    Model.ReadDtcResponseModel readDtcResponseModel = new Model.ReadDtcResponseModel();
        //    ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), indexKey);
        //    await Task.Run(async () =>
        //    {
        //        result = await dSDiagnostic.ClearDTC(index);
        //        var res = JsonConvert.SerializeObject(result);
        //        var Response = JsonConvert.DeserializeObject<ClearDtcResponseModel>(res);
        //        Status = Response.ECUResponseStatus;
        //    });
        //    return Status;
        //}
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
                //switch (flashConfig.FlashStatus)
                //{
                //    case "FLASH_SML_BOSCH_BS6":
                //        await Task.Run(async () =>
                //        {
                //            response = await dSDiagnostic.StartFlashBoschBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        });

                //        break;
                //    case "FLASH_SML_ADVANTEK_BS6":
                //        await Task.Run(async () =>
                //        {
                //            response = await dSDiagnostic.StartFlashAdvantekBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());

                //        });

                //        break;
                //        //case "FLASH_VECV_CLUSTER1":
                //        //    await Task.Run(async () =>
                //        //    {
                //        //        response = await dSDiagnostic.StartFlashCluster1(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        //    });

                //        //    break;
                //        //case "FLASH_VECV_CLUSTER2":
                //        //    await Task.Run(async () =>
                //        //    {
                //        //        response = await dSDiagnostic.StartFlashCluster2(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        //    });

                //        //    break;
                //        //case "FLASH_VECV_CLUSTER3":
                //        //    await Task.Run(async () =>
                //        //    {
                //        //        response = await dSDiagnostic.StartFlashCluster3(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        //    });

                //        //    break;
                //        //case "FLASH_VECV_AMTBS4":
                //        //    await Task.Run(async () =>
                //        //    {
                //        //        response = await dSDiagnostic.StartFlashAMTBS4(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        //    });

                //        //    break;
                //        //case "FLASH_VECV_AMTBS6":
                //        //    await Task.Run(async () =>
                //        //    {
                //        //        response = await dSDiagnostic.StartFlashAMTBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        //    });
                //        //    break;
                //        //case "FLASH_VECV_DELHPICRS":
                //        //    await Task.Run(async () =>
                //        //    {
                //        //        response = await dSDiagnostic.StartFlashDelphiCRS(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        //    });
                //        //    break;
                //        //case "FLASH_VECV_DELPHITCIC":
                //        //    await Task.Run(async () =>
                //        //    {
                //        //        response = await dSDiagnostic.StartFlashDelphiTCIC(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        //    });
                //        //    break;
                //        //case "FLASH_VECV_NIRA":
                //        //    await Task.Run(async () =>
                //        //    {
                //        //        response = await dSDiagnostic.StartFlashNIRA(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        //    });
                //        //    break;
                //        //case "FLASH_VECV_BOSCHCV54":
                //        //    await Task.Run(async () =>
                //        //    {
                //        //        response = await dSDiagnostic.StartFlashBoschCV54(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //        //    });
                //        //    break;
                //}
                //if (flashJson.Contains("\"bsl\"") || flashJson.Contains(".bin"))
                //{
                //    if (ecu2.flash_status == "PIAGGIO_EV_MCU_CANOPEN_125KBPS")
                //    {
                //        //dSDiagnostic.Test();
                //        response = await dSDiagnostic.StartFlashCurtis125KBPS(flashJson, tx_header_temp, rx_header_temp, ProtocolValue);
                //    }
                //    else if (flashJson.Contains(".bin"))
                //    {
                //        response = await dSDiagnostic.StartFlashBINfile(flashConfig, flashingMatrixData.NoOfSectors, flashJson, (int)0x1A);

                //    }
                //    else
                //    {
                //        //dSDiagnostic.Test();
                //        response = await dSDiagnostic.StartFlashCurtis500KBPS(flashJson, tx_header_temp, rx_header_temp, ProtocolValue);
                //    }
                //}
                //else if (flashConfig.FlashStatus == "FLASH_KOEL_BOSCH_BS6")
                //{
                //    response = await dSDiagnostic.StartFlashBoschBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //}

                //await Task.Run(async () =>
                //{
                //    //response = await dSDiagnostic.StartFlashBoschBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //    response = await dSDiagnostic.StartFlashDelphiBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //});

                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
                Debug.WriteLine("------RunTime------" + elapsedTime, "");


                // MessageDialog showDialog = new MessageDialog(response);
                //showDialog.Commands.Add(new UICommand("Ok")
                //{
                //    Id = 0
                //});
                //showDialog.Commands.Add(new UICommand("No")
                //{
                //    Id = 1
                //});
                //showDialog.DefaultCommandIndex = 0;
                //showDialog.CancelCommandIndex = 1;
                //var results = await showDialog.ShowAsync();

                return response;
            }
            catch (Exception ex)
            {
                return $"ERROR : {ex.Message}";
            }
        }

        //public async Task<float> ReturnPersentValue()
        //{
        //    var respo = await Task.Run(async () =>
        //    {
        //        var result = await dSDiagnostic.GetRuntimeFlashPercent();
        //        return result;
        //    });
        //    return respo;
        //}

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

        public async Task<string[]> SendTerminalCommands(string[] commands)
        {
            try
            {
                string[] vs_res = new string[7];
                string firmware_version = string.Empty;
                var mainActivity = MainActivity.Instance;
                //DisconnectUSB();
                if (mainActivity != null)
                {
                    var port = mainActivity.ReturnPort();
                    var inputOutputManager = mainActivity.inputOutputManager();

                    //var ProtocolValue = (dynamic)null;

                    //string ProtocolNameValue = StaticData.ecu_info.FirstOrDefault().protocol.name;//.protocol.name;
                    //string value = ProtocolNameValue.Replace("-", "_");
                    //string tx_header_temp = string.Empty;
                    //string rx_header_temp = string.Empty;

                    //tx_header_temp = StaticData.ecu_info.FirstOrDefault().tx_header;
                    //rx_header_temp = StaticData.ecu_info.FirstOrDefault().rx_header;




                    dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, Convert.ToInt32(commands[0]), Convert.ToUInt32(commands[1], 16), Convert.ToUInt32(commands[2], 16), 0x00, 0x10, 0x10, 0x10);
                    //dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x07E0, 0x07E8, 0x00, 0x10, 0x10, 0x10);
                    dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.USB);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
                    dSDiagnostic = new UDSDiagnostic(dongleCommWin);
                    if (dongleCommWin != null)
                    {
                        //var ab = new byte[] { };
                        var securityAccess = await dongleCommWin.SecurityAccess();
                        var securityResult = (byte[])securityAccess;
                        var securityResponse = ByteArrayToString(securityResult);

                        //var donglereset = await dongleCommWin.Dongle_Reset();//

                        var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                        var firmwareResult = (byte[])firmwareVersion;
                        firmware_version = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                        vs_res[6] = firmware_version;
                        var setProtocol = await dongleCommWin.Dongle_SetProtocol(Convert.ToInt32(commands[0]));
                        var ProtocolResult = (byte[])setProtocol;
                        var ProtocolResponse = ByteArrayToString(ProtocolResult);
                        vs_res[0] = commands[0];
                        vs_res[5] = commands[5];
                        //if (value.Contains("IVN") == false)
                        //{
                        // headers should be set only after setting the protocol for non ivn ones. for ivn ones, we dont need to set these headers
                        var setHeader = await dongleCommWin.CAN_SetTxHeader(commands[1]);
                        vs_res[1] = commands[1];
                        //var setHeader = await dongleCommWin.CAN_SetTxHeader("07e0");
                        //var setHeader = await dongleCommWin.CAN_SetTxHeader("1BDA00F9");
                        var HeaderResult = (byte[])setHeader;
                        var HeaderResponse = ByteArrayToString(HeaderResult);


                        var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(commands[2]);
                        vs_res[2] = commands[2];

                        if (!string.IsNullOrEmpty(commands[3]))
                        {
                            var setpadding = await dongleCommWin.CAN_StartPadding(commands[3]);
                            vs_res[3] = "Enabled";
                        }
                        else
                        {
                            vs_res[3] = "Disabled";
                        }


                        //var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask("07e8");
                        //var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask("1BDAf900");
                        //var HeaderMarkResult = (byte[])setHeaderMask;
                        //var HeaderMarkResponse = ByteArrayToString(HeaderMarkResult);
                        //}

                    }
                }
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
            try
            {

                var setHeader = await dSDiagnostic.SetDataData(commands);

                if (setHeader == null)
                {
                    Android.Widget.Toast t = Toast.MakeText(Android.App.Application.Context, "NULL", ToastLength.Long);
                    t.Show();
                }

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
            catch (Exception ex)
            {
                return "STACKMESSAGE : " + ex.Message + "\n\n" + ex.StackTrace;
            }
        }

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

        public async Task<TestRoutineResponseModel> SetTestRoutineCommand(string seed_key, string write_para_index, string start_command, string request_command, string stop_command, bool test_condition, int bit_position, List<string> active_command,
            string stopped_command, string fail_command, bool is_stop, int time_base)
        {
            TestRoutineResponseModel response = new TestRoutineResponseModel();
            try
            {
                object result = new object();
                SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), seed_key);
                WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), write_para_index);

                var respo = await Task.Run(async () =>
                {
                    //result = await dSDiagnostic.IORTestParameters(seed_index, write_index, start_command, request_command,
                    //    stop_command, test_condition, bit_position, active_command, stopped_command, fail_command, is_stop,
                    //    time_base);
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

        public async void StartTesterPresent()
        {
            var result = await dongleCommWin.CAN_StartTP();
        }

        public async void StopTesterPresent()
        {
            var result = await dongleCommWin.CAN_StopTP();
        }
        //public async Task<object> StartECUFlashing(string flashJson)
        //{
        //    await Task.Run(async () =>
        //    {
        //        var result = await dSDiagnostic.StartFlash();
        //    });
        //}

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
        #endregion

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
                        foreach (var item2 in item1.frame_of_pid_message)
                        {
                            newFrame.frame_of_pid_message.Add(new APDiagnosticAndroid.Models.FrameOfPidMessage
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
    //public class Usb_Android : IConnectionUSB
    //{
    //    BluetoothSocket socket = null;
    //    DongleCommWin dongleCommWin;
    //    UDSDiagnostic dSDiagnostic;
    //    //private APDiagnostic.Models.ReadDtcResponseModel readDTCResponse;
    //    SerialInputOutputManager serialIoManager;

    //    UsbManager usbManager;


    //    public async Task<string> Connect()
    //    {
    //        try
    //        {
    //            string firmware_version = string.Empty;
    //            var mainActivity = MainActivity.Instance;
    //            if (mainActivity != null)
    //            {
    //                var port = mainActivity.ReturnPort();
    //                var inputOutputManager = mainActivity.inputOutputManager();
    //                dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x07E0, 0x07E8, 0x00, 0x10, 0x10, 0x10);
    //                dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.USB);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
    //                dSDiagnostic = new UDSDiagnostic(dongleCommWin);
    //                if (dongleCommWin != null)
    //                {
    //                    var ab = new byte[] { };
    //                    var securityAccess = await dongleCommWin.SecurityAccess();
    //                    var securityResult = (byte[])securityAccess;
    //                    var securityResponse = ByteArrayToString(securityResult);
    //                    var setProtocol = await dongleCommWin.Dongle_SetProtocol(2);
    //                    var ProtocolResult = (byte[])setProtocol;
    //                    var ProtocolResponse = ByteArrayToString(ProtocolResult);
    //                    var setHeader = await dongleCommWin.CAN_SetTxHeader("07e0");
    //                    //var setHeader = await dongleCommWin.CAN_SetTxHeader("1BDA00F9");
    //                    var HeaderResult = (byte[])setHeader;
    //                    var HeaderResponse = ByteArrayToString(HeaderResult);
    //                    var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask("07e8");
    //                    //var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask("1BDAf900");
    //                    var HeaderMarkResult = (byte[])setHeaderMask;
    //                    var HeaderMarkResponse = ByteArrayToString(HeaderMarkResult);
    //                    var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
    //                    var firmwareResult = (byte[])firmwareVersion;
    //                    firmware_version = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
    //                    // firmware_version = "";
    //                    //var version = ByteArrayToString(firmwareResult);
    //                    //getver(ver);
    //                }
    //            }
    //            return firmware_version;
    //        }
    //        catch (Exception ex)
    //        {
    //            return "";
    //        }
    //    }


    //    public async Task<string> ConnectOld()
    //    {
    //        try
    //        {
    //            string firmware_version = string.Empty;
    //            var mainActivity = MainActivity.Instance;
    //            if (mainActivity != null)
    //            {
    //                var port = mainActivity.ReturnPort();
    //                var inputOutputManager = mainActivity.inputOutputManager();

    //                dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x7E0, 0x7E8, 0x00, 0x10, 0x10, 0x10);
    //                dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.USB);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
    //                dSDiagnostic = new UDSDiagnostic(dongleCommWin);
    //                if (dongleCommWin != null)
    //                {
    //                    // DependencyService.Get<Interfaces.IToastMessage>().Show("Sending Command");
    //                    var ab = new byte[] { };
    //                    var securityAccess = await dongleCommWin.SecurityAccess();
    //                    var securityResult = (byte[])securityAccess;


    //                    //if (securityResult == null)
    //                    //{
    //                    //    serialIoManager = new SerialInputOutputManager(port)
    //                    //    {
    //                    //        BaudRate = 460800,
    //                    //        DataBits = 8,
    //                    //        StopBits = StopBits.One,
    //                    //        Parity = Parity.None,
    //                    //    };

    //                    //    //serialIoManager.Open(usbManager);
    //                    //    securityAccess = await dongleCommWin.SecurityAccess();
    //                    //    securityResult = (byte[])securityAccess;                            
    //                    //}

    //                    var securityResponse = ByteArrayToString(securityResult);
    //                    var setProtocol = await dongleCommWin.Dongle_SetProtocol(2);
    //                    var ProtocolResult = (byte[])setProtocol;
    //                    var ProtocolResponse = ByteArrayToString(ProtocolResult);
    //                    var setHeader = await dongleCommWin.CAN_SetTxHeader("07e0");
    //                    var HeaderResult = (byte[])setHeader;
    //                    var HeaderResponse = ByteArrayToString(HeaderResult);
    //                    var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask("07e8");
    //                    var HeaderMarkResult = (byte[])setHeaderMask;
    //                    var HeaderMarkResponse = ByteArrayToString(HeaderMarkResult);
    //                    //Thread.Sleep(100);
    //                    var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
    //                    var firmwareResult = (byte[])firmwareVersion;
    //                    //firmware_version = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
    //                    firmware_version = "temp version";
    //                    //var version = ByteArrayToString(firmwareResult);
    //                    //getver(ver);
    //                }
    //            }

    //            return firmware_version;
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
    //            APDiagnosticAndroid.Models.ReadDtcResponseModel readDTCResponse = new APDiagnosticAndroid.Models.ReadDtcResponseModel();
    //            Model.ReadDtcResponseModel readDtcResponseModel = new Model.ReadDtcResponseModel();
    //            ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
    //            await Task.Run(async () =>
    //            {
    //                readDTCResponse = await dSDiagnostic.ReadDTC(index);
    //            });

    //            readDtcResponseModel.dtcs = readDTCResponse.dtcs;
    //            readDtcResponseModel.status = readDTCResponse.status;
    //            readDtcResponseModel.noofdtc = readDTCResponse.noofdtc;

    //            //string dtcCode = string.Empty;
    //            //var responseLlength = readDTCResponse.dtcs.GetLength(0);
    //            //for (int i = 0; i <= responseLlength - 1; i++)
    //            //{
    //            //    dtcCode += readDTCResponse[i, 0].ToString() + " - ";
    //            //    Console.WriteLine(i);

    //            //}

    //            //var bytesData = (byte[])readDTCResponse;

    //            //if (bytesData[7] == 0x7f)
    //            //{
    //            //    readDTCResponse = await dongleCommWin.ReadData();
    //            //}
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
    //            return "";
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
    //                var MessageModels = new List<APDiagnosticAndroid.Models.SelectedParameterMessage>();
    //                if (item.messages != null)
    //                {
    //                    foreach (var MessageItem in item.messages)
    //                    {
    //                        MessageModels.Add(new APDiagnosticAndroid.Models.SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
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
    //                        messages = MessageModels
    //                    });
    //            }

    //            var respo = await Task.Run(async () =>
    //            {
    //                result = await dSDiagnostic.ReadParameters(pidList.Count, list);
    //                var res = JsonConvert.SerializeObject(result);
    //                var res_list = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(res);
    //                return res_list;
    //            });
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
    //    //public async Task<object> WritePid(string write_pid_intdex, ObservableCollection<Model.WriteParameterPID> pidList)
    //    //{
    //    //    try
    //    //    {
    //    //        object result = new object();
    //    //        WriteParameterIndex index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), "UDS_DS1003_SK0B0C");
    //    //        ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.WriteParameterPID>();
    //    //        foreach (var item in pidList)
    //    //        {
    //    //            SEEDKEYINDEXTYPE seed_index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), item.seedkeyindex);
    //    //            WriteParameterIndex write_index = (WriteParameterIndex)Enum.Parse(typeof(WriteParameterIndex), item.writepamindex);
    //    //            list.Add(
    //    //                new APDiagnosticAndroid.Models.WriteParameterPID
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


    //    private string ByteArrayToString(byte[] ba)
    //    {
    //        string hex = BitConverter.ToString(ba);
    //        return hex.Replace("-", "");
    //    }

    //    public void Cancel()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    //public async Task<string> ClearDtc(string indexKey)
    //    //{
    //    //    object result = new object();
    //    //    string Status = string.Empty;
    //    //    APDiagnosticAndroid.Models.ReadDtcResponseModel readDTCResponse = new APDiagnosticAndroid.Models.ReadDtcResponseModel();
    //    //    Model.ReadDtcResponseModel readDtcResponseModel = new Model.ReadDtcResponseModel();
    //    //    ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), indexKey);
    //    //    await Task.Run(async () =>
    //    //    {
    //    //        result = await dSDiagnostic.ClearDTC(index);
    //    //        var res = JsonConvert.SerializeObject(result);
    //    //        var Response = JsonConvert.DeserializeObject<ClearDtcResponseModel>(res);
    //    //        Status = Response.ECUResponseStatus;
    //    //    });
    //    //    return Status;
    //    //}
    //    public async Task<object> StartECUFlashing(string flashJson, Ecu2 ecu2, SeedkeyalgoFnIndex sklFN, List<EcuMapFile> ecu_map_file)
    //    {
    //        try
    //        {
    //            //dongleCommWin = new DongleCommWin(socket, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x7E0, 0x7E8, 0x00, 0x10, 0x10, 0x10);
    //            //dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.Bluetooth);
    //            //dSDiagnostic = new UDSDiagnostic(dongleCommWin);

    //            var jsonData = JsonConvert.DeserializeObject<FlashingMatrixData>(flashJson);

    //            EraseSectorEnum erase_type = (EraseSectorEnum)Enum.Parse(typeof(EraseSectorEnum), ecu2.flash_erase_type);
    //            ChecksumSectorEnum check_sum_type = (ChecksumSectorEnum)Enum.Parse(typeof(ChecksumSectorEnum), ecu2.flash_check_sum_type);
    //            SEEDKEYINDEXTYPE seedkeyindx = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), sklFN.value);
    //            //byte asdb = Convert.ToByte(ecu2.flash_address_data_format);
    //            //UInt16 acc = Convert.ToUInt16(ecu2.sectorframetransferlen,16);

    //            var flashConfig = new flashconfig
    //            {
    //                addrdataformat = Convert.ToByte(ecu2.flash_address_data_format, 16),
    //                checksumsector = check_sum_type,//   ChecksumSectorEnum.COMPAREBYSECTOR,
    //                diag_mode = Convert.ToByte(ecu2.flash_diagnostic_mode, 16),
    //                erasesector = erase_type, // EraseSectorEnum.ERASEBYSECTOR,
    //                flash_index = FLASHINDEXTYPE.GREAVES_BOSCH_BS6, // **********************  not used
    //                sectorframetransferlen = Convert.ToUInt16(ecu2.sectorframetransferlen, 16),
    //                seedkeyindex = seedkeyindx, // SEEDKEYINDEXTYPE.ATUL_ADVANTEK_BS6_A46,
    //                seedkeynumbytes = Convert.ToByte(ecu2.flash_seed_key_length, 16),
    //                sendseedbyte = Convert.ToByte(ecu2.sendseedbyte, 16),
    //                septime = Convert.ToByte(ecu2.flashsep_time, 16),
    //            };


    //            //var flashConfig = new flashconfig
    //            //{
    //            //    addrdataformat = 0x33,
    //            //    //sectorframetransferlen = 0x30,
    //            //    sectorframetransferlen = 0x03B0,
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

    //            //for(var i = 0; i < jsonData.NoOfSectors; i++)
    //            //{

    //            //}
    //            //jsonData.SectorData[0].ECUMemMapStartAddress = "80004000";
    //            //jsonData.SectorData[0].ECUMemMapEndAddress = "80013fff";

    //            //jsonData.SectorData[1].ECUMemMapStartAddress = "80000000";
    //            //jsonData.SectorData[1].ECUMemMapEndAddress = "80003fff";


    //            //jsonData.SectorData[2].ECUMemMapStartAddress = "80080000";
    //            //jsonData.SectorData[2].ECUMemMapEndAddress = "8017ffff";

    //            //jsonData.SectorData[3].ECUMemMapStartAddress = "80020000";
    //            //jsonData.SectorData[3].ECUMemMapEndAddress = "8007ffff";

    //            //jsonData.SectorData[0].JsonCheckSum = "0B59";
    //            //jsonData.SectorData[1].JsonCheckSum = "8208";
    //            //jsonData.SectorData[2].JsonCheckSum = "5c9f";


    //            string response = string.Empty;

    //            Stopwatch stopWatch = new Stopwatch();
    //            stopWatch.Start();
    //            await Task.Run(async () =>
    //            {
    //                //var result = await dSDiagnostic.StartFlash(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
    //                response = await dSDiagnostic.StartFlashBosch(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
    //                //response = await dSDiagnostic.StartFlashBoschH(jsonData.SectorData.ToArray());
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

    //    public async Task<float> ReturnPersentValue()
    //    {
    //        var respo = await Task.Run(async () =>
    //        {
    //            var result = await dSDiagnostic.GetRuntimeFlashPercent();
    //            return result;
    //        });
    //        return respo;
    //    }

    //    public async Task<bool> DisconnectUsb()
    //    {
    //        return false;
    //    }



    //    //public async Task<object> StartECUFlashing(string flashJson)
    //    //{
    //    //    await Task.Run(async () =>
    //    //    {
    //    //        var result = await dSDiagnostic.StartFlash();
    //    //    });
    //    //}
    //}
    #endregion
}