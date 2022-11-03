using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using APDiagnosticAndroid;
using APDiagnosticAndroid.Enums;
using APDiagnosticAndroid.Models;
using APDiagnosticAndroid.Structures;
using APDongleCommAndroid;
using DotNetSta;
using Newtonsoft.Json;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using Bajaj.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(RP1210_Android))]
namespace Bajaj.Droid.Dependencies
{
    public class RP1210_Android : IConnectionRP
    {
        Vocom1210 class1;
        string DebugTag = "Wifi Communication";
        DongleCommWin dongleCommWin;
        UDSDiagnostic dSDiagnostic;
        private APDiagnosticAndroid.Models.ReadDtcResponseModel readDTCResponse;

        public RP1210_Android()
        {
            class1 = new Vocom1210();
        }

        public async Task<ObservableCollection<BluetoothDevicesModel>> get_device_list()
        {
            var device_list = await class1.FindDongle();
            ObservableCollection<BluetoothDevicesModel> wifiConnector = new ObservableCollection<BluetoothDevicesModel>();

            if (device_list != null)
            {
                wifiConnector = new ObservableCollection<BluetoothDevicesModel>();
                foreach (var device in device_list)
                {
                    wifiConnector.Add(new BluetoothDevicesModel
                    {
                        Name = device.name,
                        Ip = device.ip,
                        Mac_Address = device.id
                    });
                }
            }
            return wifiConnector;
        }

        public async Task<string> ConnectDevice(string device_name)
        {
            dongleCommWin = new DongleCommWin();
            dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.RP1210);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
            dSDiagnostic = new UDSDiagnostic(dongleCommWin);

            var tx_header_temp = StaticData.ecu_info.FirstOrDefault().tx_header;
            var rx_header_temp = StaticData.ecu_info.FirstOrDefault().rx_header;
            var protocol = StaticData.ecu_info.FirstOrDefault().protocol.name;

            string fw_version = dongleCommWin.ConnectDevice(device_name, tx_header_temp, rx_header_temp, protocol);
            
            return fw_version;
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
                    readDTCResponse = await dSDiagnostic.ReadDTC(index);
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

                    result = await dSDiagnostic.ReadParameters(list.Count, list);
                    //result = await dSDiagnostic.ReadParameters(pidList.Count, list);
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

        public async Task<string> StartECUFlashing(string flashJson, Ecu2 ecu2, SeedkeyalgoFnIndex sklFN, List<EcuMapFile> ecu_map_file)
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

                //var flashConfig = new flashconfig
                //{
                //    addrdataformat = ecu2.flash_address_data_format,
                //    checksumsector = check_sum_type,
                //    diag_mode = Convert.ToByte(ecu2.flash_diagnostic_mode, 16),
                //    erasesector = erase_type,
                //    //flash_index = FLASHINDEXTYPE.GREAVES_BOSCH_BS6,
                //    sectorframetransferlen = Convert.ToUInt16(ecu2.sectorframetransferlen, 16),
                //    seedkeyindex = seedkeyindx,
                //    seedkeynumbytes = Convert.ToByte(ecu2.flash_seed_key_length, 16),
                //    sendseedbyte = Convert.ToByte(ecu2.sendseedbyte, 16),
                //    septime = Convert.ToByte(ecu2.flashsep_time, 16),
                //    FlashStatus = ecu2.flash_status,
                //};

                //ECUCalculateSeedkey ecuCalculateSeedkey;

                string response = string.Empty;

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                //if (flashConfig.FlashStatus == "FLASH_SML_BOSCH_BS6")
                //{
                //    await Task.Run(async () =>
                //    {
                //        response = await dSDiagnostic.StartFlashBoschBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //    });
                //}
                //else if (flashConfig.FlashStatus == "FLASH_SML_ADVANTEK_BS6")
                //{
                //    await Task.Run(async () =>
                //    {
                //        response = await dSDiagnostic.StartFlashAdvantekBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());

                //    });
                //}
                //else if (flashConfig.FlashStatus == "FLASH_SML_BOSCH_BS4")
                //{
                //    await Task.Run(async () =>
                //    {
                //        response = await dSDiagnostic.StartFlashBoschBS4(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //    });
                //}
                //else if (flashJson.Contains(".bin"))
                //{
                //    response = await dSDiagnostic.StartFlashBINfile(flashConfig, flashingMatrixData.NoOfSectors, flashJson, (int)0x1A);

                //}
                //else if (flashConfig.FlashStatus == "FLASH_KOEL_BOSCH_BS6")
                //{
                //    response = await dSDiagnostic.StartFlashBoschBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
                //}
                //else if (flashJson.Contains("\"bsl\"") || flashJson.Contains(".bin"))
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

                //await Task.Run(async () =>
                //{
                //    response = await dSDiagnostic.StartFlashBoschBS6(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
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


    }
}