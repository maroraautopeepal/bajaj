using APDiagnosticAndroid.Enums;
using APDiagnosticAndroid.Helper;
using APDiagnosticAndroid.Models;
using APDiagnosticAndroid.Structures;
using APDongleCommAnroid.Models;
using APDongleCommAndroid;
using ECUSeedkeyPCL;

//using ECUSeedkeyPCL;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SEEDKEYINDEXTYPE = ECUSeedkeyPCL.SEEDKEYINDEXTYPE;
using Newtonsoft.Json;
using Xamarin.Forms;
using System.Collections;

namespace APDiagnosticAndroid
{
    public class UDSDiagnostic
    {
        #region Properties
        ReadDTCIndex DTCIndex = 0;
        private DongleCommWin dongleComm;
        private ECUCalculateSeedkey calculateSeedkey;
        #endregion

        #region CTOR
        public UDSDiagnostic(DongleCommWin dongleCommWin)
        {
            this.dongleComm = dongleCommWin;
        }
        #endregion

        #region ReadDTC
        public async Task<ReadDtcResponseModel> ReadDTC(ReadDTCIndex readDtcIndex)
        {
            DTCIndex = readDtcIndex;
            string status = string.Empty;
            string return_status = string.Empty;
            string[,] dtcarray = null;
            UInt16 dtcindex = 0;
            try
            {
                if (DTCIndex == ReadDTCIndex.KWP_2BYTE_DTC || DTCIndex == ReadDTCIndex.UDS_2BYTE12_DTC || DTCIndex == ReadDTCIndex.UDS_2BYTE13_DTC || DTCIndex == ReadDTCIndex.UDS_3BYTE_DTC)
                {
                    var frameLength = (dynamic)null;
                    //var responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "1902AF");
                    string DTCFunction = string.Empty;
                    if (DTCIndex == ReadDTCIndex.KWP_2BYTE_DTC)
                    {
                        frameLength = 4;
                        DTCFunction = "1800FFFF";
                    }
                    else
                    {
                        frameLength = 3;
                        DTCFunction = "1902AF";
                    }
                    var responseBytes = await this.dongleComm.CAN_TxRx(frameLength, DTCFunction);
                    status = responseBytes.ECUResponseStatus;
                    var actualData = responseBytes.ActualDataBytes;
                    return_status = string.Empty;
                    if (status == "NOERROR")
                    {
                        return_status = "NO_ERROR";
                        var Rxsize = actualData.Length;
                        var Rxarray = actualData;
                        string dtc_type = string.Empty;


                        /* 59 02 FF DTCHB DTCMB1 DTCLB1 DTCSTS1 DTCHB2 DTCMB2 DTCLB2 DTCSTS2 ..... DTCHBn DTCMBn DTCLBn DTCSTSn*/
                        var dtc_start_byte_index = 0;
                        var no_of_dtc = (Rxsize - 3) / 4;
                        if (DTCIndex == ReadDTCIndex.KWP_2BYTE_DTC)
                        {
                            dtc_start_byte_index = 2;
                            no_of_dtc = Rxarray[1];
                        }
                        else
                        {
                            dtc_start_byte_index = 3;
                            no_of_dtc = (Rxsize - 3) / 4;
                        }

                        dtcarray = new string[no_of_dtc, 2];
                        var i = 0;

                        while (i < no_of_dtc)
                        {

                            if ((DTCIndex != ReadDTCIndex.KWP_2BYTE_DTC) && ((Rxarray[dtc_start_byte_index + 3] == 0x40) || (Rxarray[dtc_start_byte_index + 3] == 0x50)))
                            {
                                /* dont consider these dtcs */
                            }
                            else
                            {
                                var value = 0;
                                var dtctypebits = (Rxarray[dtc_start_byte_index] & 0xC0) >> 6;

                                if (dtctypebits == 0x00)
                                {
                                    dtc_type = "P";
                                }
                                else if (dtctypebits == 0x01)
                                {
                                    dtc_type = "C";
                                }
                                else if (dtctypebits == 0x02)
                                {
                                    dtc_type = "B";
                                }
                                else if (dtctypebits == 0x03)
                                {
                                    dtc_type = "U";
                                }

                                if (DTCIndex == ReadDTCIndex.KWP_2BYTE_DTC)
                                {
                                    value = Rxarray[dtc_start_byte_index + 2] & 0x20; // status 
                                    switch (value)
                                    {
                                        case 0x00:
                                            dtcarray[dtcindex, 1] = "Inactive";
                                            break;
                                        case 0x20:
                                            dtcarray[dtcindex, 1] = "Active";
                                            break;

                                    }
                                }
                                else
                                {
                                    value = Rxarray[dtc_start_byte_index + 3] & 0x81; // status 
                                    switch (value)
                                    {
                                        case 0x00:
                                            dtcarray[dtcindex, 1] = "Inactive:LampOff";
                                            break;
                                        case 0x01:
                                            dtcarray[dtcindex, 1] = "Active:LampOff";
                                            break;
                                        case 0x80:
                                            dtcarray[dtcindex, 1] = "Inactive:LampOn";
                                            break;
                                        case 0x81:
                                            dtcarray[dtcindex, 1] = "Active:LampOn";
                                            break;
                                    }

                                }

                                switch (DTCIndex)
                                {
                                    case ReadDTCIndex.UDS_3BYTE_DTC:

                                        dtcarray[dtcindex, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2") + "-" + (Rxarray[dtc_start_byte_index + 2]).ToString("X2"); // correct this syntax
                                        break;
                                    case ReadDTCIndex.UDS_2BYTE12_DTC:
                                        dtcarray[dtcindex, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax
                                        break;
                                    case ReadDTCIndex.UDS_2BYTE13_DTC:
                                        dtcarray[dtcindex, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 2]).ToString("X2"); // correct this syntax
                                        break;
                                    case ReadDTCIndex.KWP_2BYTE_DTC:

                                        dtcarray[dtcindex, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax
                                        break;
                                }
                                dtcindex++;
                            }

                            if (DTCIndex == ReadDTCIndex.KWP_2BYTE_DTC)
                            {
                                dtc_start_byte_index += 3;
                            }
                            else
                            {
                                dtc_start_byte_index += 4;
                            }

                            i++;
                        }
                    }
                    else
                    {
                        return_status = status;
                    }
                }
                else if (DTCIndex == ReadDTCIndex.GENERIC_OBD)
                {
                    var frameLength = 1;
                    var responseBytes = await this.dongleComm.CAN_TxRx(frameLength, "03");

                    status = responseBytes.ECUResponseStatus;
                    var actualData03 = responseBytes.ActualDataBytes;

                    if (status == "NOERROR")
                    {
                        var Rxarray03 = actualData03;
                        //var Rxsize03 = Rxarray03[1];

                        frameLength = 1;
                        responseBytes = await this.dongleComm.CAN_TxRx(frameLength, "07");

                        status = responseBytes.ECUResponseStatus;
                        var actualData07 = responseBytes.ActualDataBytes;

                        return_status = string.Empty;
                        if (status == "NOERROR")
                        {
                            return_status = "NO_ERROR";
                            var Rxarray07 = actualData07;
                            //var Rxsize07 = Rxarray07[1];

                            string dtc_type = string.Empty;


                            /* All DTCs - 43 LEN DTCHB DTCLB1 DTCHB2 DTCLB2  ..... DTCHBn DTCLBn*/
                            /* Pending DTCs - 47 LEN DTCHB DTCLB1 DTCHB2 DTCLB2  ..... DTCHBn DTCLBn*/

                            //var dtc_start_byte_index = 2;
                            var no_of_dtc03 = Rxarray03[1];
                            var no_of_dtc07 = Rxarray07[1];

                            var dtcarray03 = new string[no_of_dtc03, 2];
                            var dtcarray07 = new string[no_of_dtc07, 2];

                            dtcarray = new string[no_of_dtc03 + no_of_dtc07, 2];

                            int i = 0;
                            for (i = 0; i < no_of_dtc03; i++)
                            {
                                var dtctypebits = (Rxarray03[(i * 2) + 2] & 0xC0) >> 6;

                                if (dtctypebits == 0x00)
                                {
                                    dtc_type = "P";

                                }
                                else if (dtctypebits == 0x01)
                                {
                                    dtc_type = "C";
                                }
                                else if (dtctypebits == 0x02)
                                {
                                    dtc_type = "B";
                                }
                                else if (dtctypebits == 0x03)
                                {
                                    dtc_type = "U";
                                }

                                dtcarray[i, 0] = dtc_type + (Rxarray03[(i * 2) + 2] & 0x3F).ToString("X2") + (Rxarray03[(i * 2) + 3]).ToString("X2");
                                dtcarray[i, 1] = "Current";

                            }

                            for (int j = 0; j < no_of_dtc07; j++)
                            {
                                var dtctypebits = (Rxarray07[(j * 2) + 2] & 0xC0) >> 6;

                                if (dtctypebits == 0x00)
                                {
                                    dtc_type = "P";

                                }
                                else if (dtctypebits == 0x01)
                                {
                                    dtc_type = "C";
                                }
                                else if (dtctypebits == 0x02)
                                {
                                    dtc_type = "B";
                                }
                                else if (dtctypebits == 0x03)
                                {
                                    dtc_type = "U";
                                }

                                dtcarray[i + j, 0] = dtc_type + (Rxarray07[(j * 2) + 2] & 0x3F).ToString("X2") + (Rxarray07[(j * 2) + 3]).ToString("X2");
                                dtcarray[i + j, 1] = "Pending";
                            }
                        }
                        else
                        {
                            return_status = status;
                        }
                    }
                    else
                    {
                        return_status = status;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            ReadDtcResponseModel readDtcResponseModel = new ReadDtcResponseModel
            {
                dtcs = dtcarray,
                noofdtc = dtcindex,
                status = return_status,
            };
            return readDtcResponseModel;
            //return dtcarray;
        }
        #endregion

        public async Task<ObservableCollection<ReadParameterResponseIvn>> IVN_ReadParameters(int noOFParameters, ObservableCollection<IVN_SelectedPID> readParameterCollection)
        {
            ObservableCollection<ReadParameterResponseIvn> databyteArray = new ObservableCollection<ReadParameterResponseIvn>();

            try
            {
                string dataValue = string.Empty;

                // filter each frame id one by one, read the buffer, decode values
                for (int i = 0; i < readParameterCollection.Count(); i++)
                {
                    foreach (var item in readParameterCollection.FirstOrDefault().frame_ids)
                    {
                        var getivnframe = await dongleComm.CAN_IVNRxFrame(item.FramID);

                        if (getivnframe.ECUResponseStatus == "NOERROR")
                        {
                            var pid_name = item.pid_description;

                            var Unit = item.unit;

                            string pid_type = Convert.ToString(item.message_type);

                            UInt16 pid_start_byte = Convert.ToUInt16(item.start_byte);

                            UInt16 pid_noofbytes = Convert.ToUInt16(item.@byte);

                            var pid_bitcoded = item.bit_coded;

                            UInt16 pid_startbit = 0;
                            UInt16 pid_noofbits = 0;

                            if (item.bit_coded != "0")
                            {
                                pid_startbit = Convert.ToUInt16(item.start_bit);

                                pid_noofbits = Convert.ToUInt16(item.no_of_bits);
                            }

                            double pid_resolution = Convert.ToDouble(item.resolution);

                            double pid_offset = Convert.ToDouble(item.offset);

                            //if (pid_type.Contains("CONTINOUS") == true)
                            if (pid_type.Contains("CONTINUOUS") == true)
                            {
                                Debug.WriteLine("---START CONTINUOUS--");
                                UInt32 unsignedpidintvalue = 0; // take double int data type

                                if (item.endian.Contains("LITTLE"))
                                {
                                    Array.Reverse(getivnframe.ActualFrameBytes, int.Parse(item.start_byte) - 1, pid_noofbytes);
                                }

                                for (int l = 0; l < pid_noofbytes; l++)
                                {
                                    unsignedpidintvalue |= Convert.ToUInt32(Convert.ToUInt32(getivnframe.ActualFrameBytes[pid_start_byte - 1 + l]) << ((pid_noofbytes - 1 - l) * 8));
                                    //unsignedpidintvalue = swapNibbles(unsignedpidintvalue);
                                }

                                
                                if (pid_bitcoded == "1")
                                {
                                    if (pid_noofbytes == 1)
                                    {
                                        var startBit = pid_startbit;
                                        byte mask = 0;
                                        for (int x = 0; x < pid_noofbits; x++)
                                        {
                                            mask |= (byte)(1 << (8 - startBit));
                                            //if (item.endian.Contains("LITTLE"))
                                            //{
                                            //    startBit--;
                                            //}
                                            //else
                                            //{
                                                startBit++;
                                            //}

                                        }
                                        unsignedpidintvalue = Convert.ToUInt32((unsignedpidintvalue & mask) >> ((pid_noofbytes * 8) - pid_startbit - pid_noofbits + 1));

                                    }
                                    else
                                    {

                                        var mask = (Convert.ToInt32((Math.Pow(16, pid_noofbytes * 2) - 1))) >> ((pid_noofbytes * 8) - pid_noofbits - pid_startbit + 1);
                                        unsignedpidintvalue = Convert.ToUInt32((unsignedpidintvalue >> ((pid_noofbytes * 8) - pid_noofbits - pid_startbit + 1)) & mask);
                                    }
                                    ////byte mask = 0x00; 
                                    //var mask = (Convert.ToInt32((Math.Pow(16, pid_noofbytes * 2) - 1))) >> (pid_startbit - 1);

                                    ////byte mask = 0xff >> (8 - pid_noofbytes);
                                    //unsignedpidintvalue = Convert.ToUInt32((unsignedpidintvalue >> ((pid_noofbytes * 8) - pid_noofbits - pid_startbit + 1)) & mask);
                                    ////unsignedpidintvalue &= mask;
                                    ////unsignedpidintvalue = Convert.ToUInt32((unsignedpidintvalue >> ((pid_noofbytes * 8) - pid_noofbits - pid_startbit + 1)) & mask1);
                                }

                                double float_pid_value = 0.0;

                                if (pid_type == "CONTINUOUS_2S_COMPLEMENT" || item.num_type == "SIGNED")
                                {
                                    Int32 signedpidintvalue = 0;
                                    if (pid_noofbytes == 1)
                                    {
                                        signedpidintvalue = (sbyte)unsignedpidintvalue;
                                    }
                                    else if (pid_noofbytes == 2)
                                    {
                                        signedpidintvalue = (Int16)unsignedpidintvalue;
                                    }
                                    else if (pid_noofbytes == 3)
                                    {
                                        signedpidintvalue = (Int32)unsignedpidintvalue;
                                    }
                                    else
                                    {
                                        signedpidintvalue = (Int32)unsignedpidintvalue;
                                    }

                                    float_pid_value = (signedpidintvalue * pid_resolution) + pid_offset;
                                }
                                else
                                {
                                    float_pid_value = (unsignedpidintvalue * pid_resolution) + pid_offset;
                                }

                                dataValue = float_pid_value.ToString();
                                Debug.WriteLine("---END CONTINUOUS--");
                            }
                            else if (pid_type == "ASCII")
                            {
                                /* we dont encounter bit coded messages here */

                                Debug.WriteLine("---START ASCII--");
                                byte[] outputlivepararray = new byte[pid_noofbytes];
                                for (int k = 0; k < pid_noofbytes; k++)
                                {
                                    outputlivepararray[pid_noofbytes - 1 - k] = getivnframe.ActualFrameBytes[pid_start_byte - 1 + k];
                                }
                                dataValue = BytesConverter.HexToASCII(ByteArrayToString(outputlivepararray));
                                if (dataValue.Contains('\0') == true)
                                {
                                    dataValue = "0";
                                }

                                Debug.WriteLine("---END ASCII--");
                            }
                            else if (pid_type == "BCD")
                            {
                                /* we dont encounter bitcoded messages here */

                                Debug.WriteLine("---START BCD--");
                                byte[] outputlivepararray = new byte[pid_noofbytes];
                                for (int k = 0; k < pid_noofbytes; k++)
                                {
                                    outputlivepararray[pid_noofbytes - 1 - k] = getivnframe.ActualFrameBytes[k + pid_start_byte - 1];
                                }


                                dataValue = ByteArrayToString(outputlivepararray);
                                Debug.WriteLine("---END BCD--");
                            }
                            else if (pid_type == "ENUMRATED")
                            {
                                UInt32 unsignedpidintvalue = 0; // take double int data type

                                //var tx_Frame_len = int.Parse(item.FramID)/2;
                                var Rxarray = getivnframe.ActualFrameBytes;

                                for (int m = 0; m < pid_noofbytes; m++)
                                {
                                    unsignedpidintvalue |= Convert.ToUInt32(Convert.ToUInt32(getivnframe.ActualFrameBytes[pid_start_byte - 1 + m]) << ((pid_noofbytes - 1 - m) * 8));
                                }

                                if (pid_bitcoded == "1")
                                {
                                    var startBit = pid_startbit;
                                    byte mask = 0;
                                    for (int x = 0; x < pid_noofbits; x++)
                                    {
                                        mask |= (byte)(1 << (8 - startBit));
                                        if (item.endian.Contains("LITTLE"))
                                        {
                                            startBit--;
                                        }
                                        else
                                        {
                                            startBit++;
                                        }
                                    }
                                    unsignedpidintvalue = Convert.ToUInt32((unsignedpidintvalue & mask) >> ((pid_noofbytes * 8) - pid_startbit - pid_noofbits + 1));
                                }

                                dataValue = Convert.ToString(unsignedpidintvalue);
                                if (item.frame_of_pid_message.Count > 0)
                                {

                                    foreach (var item1 in item.frame_of_pid_message)
                                    {
                                        if (Convert.ToUInt32(item1.code) == unsignedpidintvalue)
                                        {
                                            dataValue = item1.message;
                                        }
                                    }
                                }
                            }
                            databyteArray.Add(new ReadParameterResponseIvn
                            {
                                pidName = pid_name,
                                responseValue = dataValue,
                                Status = "NO_ERROR",
                                Unit = Unit,
                            });
                        }
                        else if (getivnframe.ECUResponseStatus == "ECUERROR_NORESPONSEFROMECU")
                        {
                            databyteArray.Add(new ReadParameterResponseIvn
                            {
                                pidName = item.pid_description,
                                responseValue = "ERR",
                                Status = "NoResponse",
                                Unit = "",
                            });
                        }
                        else
                        {
                            databyteArray.Add(new ReadParameterResponseIvn
                            {
                                pidName = item.pid_description,
                                responseValue = "ERR",
                                Status = "ERROR",
                                Unit = "",
                            });
                        }
                    }
                }

                var value = databyteArray;

                return databyteArray;
            }
            catch (Exception EX)
            {
                return databyteArray;
            }
        }


        #region Helper
        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
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

        static byte[] HexToBytes(string input)
        {
            byte[] result = new byte[input.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(input.Substring(2 * i, 2), 16);
            }
            return result;
        }
        #endregion

        #region ClearDTC
        ClearDTCIndex ClearDTCIndex = 0;
        public async Task<object> ClearDTC(ClearDTCIndex readDtcIndex)
        {
            ClearDTCIndex = readDtcIndex;
            object responseBytes = null;
            if (ClearDTCIndex == ClearDTCIndex.UDS_4BYTES)
            {
                var frameLength = 4;
                responseBytes = await this.dongleComm.CAN_TxRx(frameLength, "14FFFFFF");
            }
            else if (ClearDTCIndex == ClearDTCIndex.UDS_3BYTES || ClearDTCIndex == ClearDTCIndex.KWP)
            {
                var frameLength = 3;
                responseBytes = await this.dongleComm.CAN_TxRx(frameLength, "14FFFF");
            }
            else if (ClearDTCIndex == ClearDTCIndex.GENERIC_OBD)
            {
                var frameLength = 1;
                responseBytes = await this.dongleComm.CAN_TxRx(frameLength, "04");
            }
            return responseBytes;
        }
        #endregion

        #region ReadParameters

        public async Task<string[]> genericOBDSupportedPidList()
        {
            try
            {
                // check for pids 0100, 0120, 0140, 0160, 0180, 01A0, 01C0
                int noofsupportedpid = 0;
                string[] supportedpid = null;

                for (int i = 0; i < 0x0c; i++)
                {
                    var frameLength = 2;
                    var response = await this.dongleComm.CAN_TxRx(frameLength, "01" + (i).ToString("X2"));

                    if ((response.ECUResponseStatus == "NOERROR") && (response.ActualDataBytes[0] == 0x41)) // positive response
                    {
                        UInt32 cmpresp = 0;
                        if (response.ActualDataBytes.Length == 2)
                        {
                            /* Do Nothing */
                        }
                        else if (response.ActualDataBytes.Length == 3)
                        {
                            cmpresp = (UInt32)response.ActualDataBytes[2] << 24;
                        }
                        else if (response.ActualDataBytes.Length == 4)
                        {
                            cmpresp = (UInt32)((response.ActualDataBytes[2] << 24) + (response.ActualDataBytes[3] << 16));
                        }
                        else if (response.ActualDataBytes.Length == 5)
                        {
                            cmpresp = (UInt32)((response.ActualDataBytes[2] << 24) + (response.ActualDataBytes[3] << 16) + (response.ActualDataBytes[4]));
                        }
                        else
                        {
                            cmpresp = (UInt32)((response.ActualDataBytes[2] << 24) + (response.ActualDataBytes[3] << 16) + (response.ActualDataBytes[4] << 8) + response.ActualDataBytes[5]);

                        }



                        for (int j = 31; j >= 0; j--)
                        {
                            if ((cmpresp & (0x00000001 << j)) != 0x00)
                            {
                                noofsupportedpid++;
                                Array.Resize(ref supportedpid, noofsupportedpid);
                                supportedpid[noofsupportedpid - 1] = "01" + (32 - j).ToString("X2");
                            }
                        }
                    }
                }

                return supportedpid;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ObservableCollection<ReadParameterResponse>> ReadParameters(int noOFParameters, ObservableCollection<ReadParameterPID> readParameterCollection)
        {
            ObservableCollection<ReadParameterResponse> databyteArray = new ObservableCollection<ReadParameterResponse>();

            for (int i = 0; i < readParameterCollection.Count; i++)
            {
                var pidItem = readParameterCollection[i];
                //Total lenght of command
                var frameLength = pidItem.pid.Length / 2;
                string dataValue = string.Empty;
                //Convert PID TO Bytes
                byte[] pid = HexToBytes(pidItem.pid);
                Debug.WriteLine("---------------------START LOOP PID NAME-------------------------------------" + pidItem.pidName);

                //Send the parameter ID to the ECU
                var pidResponse = await dongleComm.CAN_TxRx(frameLength, pidItem.pid);
                try
                {
                    if (pidResponse.ECUResponseStatus == "NOERROR")
                    {
                        var pidBytesResponseString = ByteArrayToString(pidResponse.ActualDataBytes);
                        Debug.WriteLine("Response received = " + pidBytesResponseString);

                        var status = pidResponse.ECUResponseStatus;
                        var datasArray = pidResponse.ActualDataBytes;
                        var inputliveparaarray = readParameterCollection;

                        var Rxarray = datasArray;
                        double? float_pid_value = 0;

                        //var tx_Frame_len = pidResponse.ActualDataBytes.Length;
                        var tx_Frame_len = frameLength;
                        byte[] outputlivepararray = new byte[tx_Frame_len - 2];
                        if (status == "NOERROR")
                        {
                            Debug.WriteLine("---NOERROR LOOP");
                            if (inputliveparaarray[i].datatype == "CONTINUOUS")
                            {
                                Debug.WriteLine("---START CONTINUOUS--");
                                UInt32 unsignedpidintvalue = 0; // take double int data type
                                for (int j = 0; j < pidItem.noOfBytes; j++)
                                {
                                    unsignedpidintvalue |= Convert.ToUInt32(Convert.ToUInt32(Rxarray[pidItem.startByte + tx_Frame_len - 1 + j]) << ((pidItem.noOfBytes - 1 - j) * 8));
                                }

                                if (pidItem.IsBitcoded == true)
                                {
                                    var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
                                    unsignedpidintvalue = Convert.ToUInt32((unsignedpidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask);
                                }


                                if ((pidItem.readParameterIndex == ReadParameterIndex.UDS_2S_COMPLIMENT) && (pidItem.offset == 1))
                                {
                                    var signedpidintvalue = (uint)unsignedpidintvalue;
                                    float_pid_value = (signedpidintvalue * pidItem.resolution);
                                }

                                else
                                {
                                    float_pid_value = (unsignedpidintvalue * pidItem.resolution) + pidItem.offset;
                                }


                                //outputlivepararray[i] = Convert.ToByte(float_pid_value);
                                //outputlivepararray[i] = Convert.ToByte(unsignedpidintvalue);

                                var TreePlace = Convert.ToDecimal(float_pid_value).ToString("0.000");

                                //dataValue = float_pid_value.ToString();
                                dataValue = TreePlace;
                                Debug.WriteLine("---END CONTINUOUS--");
                            }
                            else if (inputliveparaarray[i].datatype == "ASCII")
                            {
                                /* we dont encounter bit coded messages here */
                                //outputlivepararray[i] = Rxarray[pidItem.startByte + tx_Frame_len - 2] till inputliveparaarray[i].noofbytes + '\0';
                                //22f190 62f19036363636 - "6666"
                                //0906   4606363636     - "6666"
                                Debug.WriteLine("---START ASCII--");
                                Array.Resize(ref outputlivepararray, pidItem.noOfBytes);
                                Array.Copy(Rxarray, pidItem.startByte + tx_Frame_len - 1, outputlivepararray, 0, pidItem.noOfBytes);
                                dataValue = BytesConverter.HexToASCII(ByteArrayToString(outputlivepararray));
                                dataValue = dataValue.Replace("\0", string.Empty);

                                //if (dataValue.Contains('\0') == true)
                                //{
                                //    dataValue = "NULL";
                                //}

                                Debug.WriteLine("---END ASCII--");
                            }
                            else if (inputliveparaarray[i].datatype == "BCD" || inputliveparaarray[i].datatype == "HEX")
                            {
                                /* we dont encounter bitcoded messages here */
                                //outputlivepararray[i] = hex2str(Rxarray[startbyte + tx_Frame_len - 2] till inputliveparaarray[i].noofbytes) + '\0';

                                //22f190 62f19036363636 - "36363636"
                                //0906   4606363636     - ""

                                Debug.WriteLine("---START BCD--");
                                Array.Resize(ref outputlivepararray, pidItem.noOfBytes);
                                Array.Copy(Rxarray, pidItem.startByte + tx_Frame_len - 1, outputlivepararray, 0, pidItem.noOfBytes);
                                var new_data = outputlivepararray.TakeWhile((v, index) => outputlivepararray.Skip(index).Any(w => w != 0x00)).ToArray();
                                dataValue = ByteArrayToString(new_data);
                                Debug.WriteLine("---END BCD--");
                            }
                            else if (inputliveparaarray[i].datatype == "ENUMRATED")

                            {
                                UInt32 pidintvalue = 0; // take double int data type
                                for (int j = 0; j < pidItem.noOfBytes; j++)
                                {
                                    pidintvalue |= Convert.ToUInt32(Convert.ToUInt32(Rxarray[pidItem.startByte + tx_Frame_len - 1 + j]) << ((pidItem.noOfBytes - 1 - j) * 8));
                                }

                                if (pidItem.IsBitcoded)
                                {
                                    var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
                                    pidintvalue = Convert.ToUInt32((pidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask);
                                }
                                dataValue = Convert.ToString(pidintvalue);
                                if (inputliveparaarray[i].messages.Count > 0)
                                {

                                    foreach (var item in inputliveparaarray[i].messages)
                                    {
                                        if (Convert.ToUInt32(item.code) == pidintvalue)
                                        {
                                            dataValue = item.message;
                                        }
                                    }
                                }
                            }
                            else if (inputliveparaarray[i].datatype == "IQA")
                            {
                                char[] iqa_lookup_x = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8' };
                                byte[] iqaecuarray = new byte[32] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
                                string[] iqaecuarrayx = new string[4];

                                Array.Copy(Rxarray, 3, iqaecuarray, 0, Rxarray.Length - 3);



                                // KOEL injindx < 1
                                for (int injindx = 0; injindx < 4; injindx++)
                                {
                                    byte[] rxdata = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                                    Array.Copy(iqaecuarray, 0 + (injindx * 8), rxdata, 0 , 8);

                                    byte[] iqa_y = new byte[7];
                                    byte[] iqa_x = new byte[7];


                                    iqa_y[0] = (byte)((rxdata[0] & 0xF8) >> 3);
                                    iqa_y[1] = (byte)((((UInt16)(rxdata[0] & 0x07) << 8) | ((UInt16)(rxdata[1] & 0xC0))) >> 6);
                                    iqa_y[2] = (byte)((rxdata[1] & 0x3E) >> 1);
                                    iqa_y[3] = (byte)((((UInt16)(rxdata[1] & 0x01) << 8) | ((UInt16)(rxdata[2] & 0xF0))) >> 4);
                                    iqa_y[4] = (byte)((((UInt16)(rxdata[2] & 0x0F) << 8) | ((UInt16)(rxdata[3] & 0x80))) >> 7);
                                    iqa_y[5] = (byte)((rxdata[3] & 0x7C) >> 2);
                                    iqa_y[6] = (byte)((rxdata[4] & 0xF8) >> 3);

                                    //convert it to alphanumeric
                                    iqa_x[0] = Convert.ToByte(iqa_lookup_x[iqa_y[0]]);
                                    iqa_x[1] = Convert.ToByte(iqa_lookup_x[iqa_y[1]]);
                                    iqa_x[2] = Convert.ToByte(iqa_lookup_x[iqa_y[2]]);
                                    iqa_x[3] = Convert.ToByte(iqa_lookup_x[iqa_y[3]]);
                                    iqa_x[4] = Convert.ToByte(iqa_lookup_x[iqa_y[4]]);
                                    iqa_x[5] = Convert.ToByte(iqa_lookup_x[iqa_y[5]]);
                                    iqa_x[6] = Convert.ToByte(iqa_lookup_x[iqa_y[6]]);

                                    Debug.WriteLine("---START iqa--");
                                    
                                    var Se_Value = BytesConverter.HexToASCII(ByteArrayToString(iqa_x));
                                    dataValue = dataValue+ Se_Value+",";
                                    Debug.WriteLine("---END iqa--");

                                }
                                Console.WriteLine($"IQA Value : {dataValue}");


                            }
                            else if (inputliveparaarray[i].datatype.Contains("GEARRATIO_IDM") == true)
                            {
                                UInt16 gearratioraw = 0;
                                float gearratio = 0;

                                for (int tmp = 0; tmp < 16; tmp++)
                                {
                                    gearratioraw = (ushort)(((UInt16)Rxarray[3 + (tmp * 2)] << 8) + ((UInt16)Rxarray[4 + (tmp * 2)]));
                                    gearratio = (float)((gearratioraw * pidItem.resolution) + pidItem.offset);
                                    dataValue += Convert.ToDecimal(gearratio).ToString("0.000");
                                    dataValue += ", ";
                                }
                            }

                            var dataItem = new ReadParameterResponse
                            {
                                Status = pidResponse.ECUResponseStatus,
                                DataArray = outputlivepararray,
                                pidName = pidItem.pidName,
                                pidNumber = pidItem.pidNumber,
                                responseValue = dataValue
                            };
                            databyteArray.Add(dataItem);
                        }
                        else
                        {
                            outputlivepararray[i] = 0x00;
                            var dataItem = new ReadParameterResponse
                            {
                                Status = pidResponse.ECUResponseStatus,
                                DataArray = outputlivepararray,
                                pidName = pidItem.pidName,
                                pidNumber = pidItem.pidNumber,
                                responseValue = dataValue

                            };
                            databyteArray.Add(dataItem);
                        }
                    }
                    else
                    {
                        //outputlivepararray[i] = 0x00;
                        var dataItem = new ReadParameterResponse
                        {
                            Status = pidResponse.ECUResponseStatus,
                            //DataArray = outputlivepararray,
                            pidName = pidItem.pidName,
                            pidNumber = pidItem.pidNumber,
                            responseValue = pidResponse.ECUResponseStatus

                        };
                        databyteArray.Add(dataItem);
                    }
                }
                catch (Exception excepption)
                {
                    var dataItem = new ReadParameterResponse
                    {
                        Status = pidResponse.ECUResponseStatus,
                        //DataArray = outputlivepararray,
                        pidName = pidItem.pidName,
                        pidNumber = pidItem.pidNumber,
                        responseValue = excepption.Message

                    };
                    databyteArray.Add(dataItem);
                }
                Debug.WriteLine(" END LOOP PID NAME =" + pidItem.pidName);

                #region OldImplementation

                #endregion

            }
            return databyteArray;
        }

        //public async Task<ObservableCollection<ReadParameterResponse>> ReadParameters(int noOFParameters, ObservableCollection<ReadParameterPID> readParameterCollection)
        //{
        //    ObservableCollection<ReadParameterResponse> databyteArray = new ObservableCollection<ReadParameterResponse>();

        //    for (int i = 0; i < readParameterCollection.Count; i++)
        //    {
        //        var pidItem = readParameterCollection[i];
        //        //Total lenght of command
        //        var frameLength = pidItem.totalLen;
        //        string dataValue = string.Empty;
        //        //Convert PID TO Bytes
        //        byte[] pid = HexToBytes(pidItem.pid);
        //        Debug.WriteLine(" START LOOP PID NAME =" + pidItem.pidName);

        //        //Send the parameter ID to the ECU
        //        var pidResponse = await dongleCommWin.CAN_TxRx(frameLength, pidItem.pid);
        //        try
        //        {

        //            if (pidResponse.ECUResponseStatus == "NOERROR")
        //            {
        //                var pidBytesResponseString = ByteArrayToString(pidResponse.ActualDataBytes);
        //                Debug.WriteLine("Response received = " + pidBytesResponseString);

        //                var status = pidResponse.ECUResponseStatus;
        //                var datasArray = pidResponse.ActualDataBytes;
        //                var inputliveparaarray = readParameterCollection;

        //                var Rxarray = datasArray;
        //                double? float_pid_value = 0;


        //                //var tx_Frame_len = pidResponse.ActualDataBytes.Length;
        //                var tx_Frame_len = frameLength;
        //                byte[] outputlivepararray = new byte[tx_Frame_len - 2];
        //                if (status == "NOERROR")
        //                {
        //                    Debug.WriteLine("---NOERROR LOOP");
        //                    if (inputliveparaarray[i].datatype == "CONTINUOUS")
        //                    {
        //                        Debug.WriteLine("---START CONTINUOUS--");
        //                        int unsignedpidintvalue = 0; // take double int data type
        //                        for (int j = 0; j < pidItem.noOfBytes; j++)
        //                        {
        //                            unsignedpidintvalue |= Rxarray[pidItem.startByte + tx_Frame_len - j] << (j * 8);
        //                        }

        //                        if (pidItem.IsBitcoded == true)
        //                        {
        //                            var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
        //                            unsignedpidintvalue = (unsignedpidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask;
        //                        }


        //                        if (pidItem.readParameterIndex == ReadParameterIndex.UDS_2S_COMPLIMENT)
        //                        {
        //                            var signedpidintvalue = (uint)unsignedpidintvalue;
        //                            float_pid_value = (signedpidintvalue * pidItem.resolution) + pidItem.offset;
        //                        }
        //                        else
        //                        {
        //                            float_pid_value = (unsignedpidintvalue * pidItem.resolution) + pidItem.offset;
        //                        }


        //                        //outputlivepararray[i] = Convert.ToByte(float_pid_value);
        //                        //outputlivepararray[i] = Convert.ToByte(unsignedpidintvalue);
        //                        dataValue = float_pid_value.ToString();
        //                        Debug.WriteLine("---END CONTINUOUS--");
        //                    }
        //                    else if (inputliveparaarray[i].datatype == "ASCII")

        //                    {
        //                        /* we dont encounter bit coded messages here */
        //                        //outputlivepararray[i] = Rxarray[pidItem.startByte + tx_Frame_len - 2] till inputliveparaarray[i].noofbytes + '\0';
        //                        //22f190 62f19036363636 - "6666"
        //                        //0906   4606363636     - "6666"
        //                        Debug.WriteLine("---START ASCII--");
        //                        Array.Copy(Rxarray, pidItem.startByte, outputlivepararray, 0, tx_Frame_len - 2);
        //                        dataValue = BytesConverter.HexToASCII(ByteArrayToString(outputlivepararray));
        //                        Debug.WriteLine("---END ASCII--");
        //                    }
        //                    else if (inputliveparaarray[i].datatype == "BCD")

        //                    {
        //                        /* we dont encounter bitcoded messages here */
        //                        //outputlivepararray[i] = hex2str(Rxarray[startbyte + tx_Frame_len - 2] till inputliveparaarray[i].noofbytes) + '\0';

        //                        //22f190 62f19036363636 - "36363636"
        //                        //0906   4606363636     - ""

        //                        Debug.WriteLine("---START BCD--");
        //                        Array.Copy(Rxarray, pidItem.startByte, outputlivepararray, 0, tx_Frame_len - 2);
        //                        dataValue = ByteArrayToString(outputlivepararray);
        //                        Debug.WriteLine("---END BCD--");
        //                    }
        //                    else if (inputliveparaarray[i].datatype == "ENUMERATED")

        //                    {
        //                        var pidintvalue = 0; // take double int data type
        //                        for (int j = 0; j < pidItem.noOfBytes; j++)
        //                        {
        //                            pidintvalue |= Rxarray[pidItem.startByte + tx_Frame_len - 2] << ((pidItem.noOfBytes - 1) * 8);
        //                        }

        //                        if (pidItem.IsBitcoded)
        //                        {
        //                            var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
        //                            pidintvalue = (pidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask;
        //                        }

        //                        //outputlivepararray[i] = hex2str(pidintvalue); // If enumeration related to hex2str(pidintvalue) not found, then it will return the hex value itself
        //                        //for (int j = 0; j < noorrowsinenumstruct; j++) // u will have to compute the number of rows in enumoptions structure 
        //                        //{
        //                        //    if (outputlivepararray[i] == enumoptions[j][1])
        //                        //        outputlivepararray[i] = enumoptions[j][2]; // assign the enum value here
        //                        //}
        //                    }
        //                    else if (inputliveparaarray[i].datatype == "GREAVES_BOSCH_BS6_IQA")

        //                    {
        //                        /*SPECIAL LOGIC - WILL TAKE CARE OF THIS LATER */
        //                    }
        //                    else if (inputliveparaarray[i].datatype == "IQA")
        //                    {
        //                        char[] iqa_lookup_x = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8' };
        //                        byte[] rxdata = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        //                        Array.Copy(Rxarray, 3, rxdata, 0, Rxarray.Length - 3);

        //                        byte[] iqa_y = new byte[7];
        //                        byte[] iqa_x = new byte[7];

        //                        iqa_y[0] = Convert.ToByte(rxdata[0] >> 3);
        //                        iqa_y[1] = Convert.ToByte(rxdata[1] >> 3);
        //                        iqa_y[2] = Convert.ToByte(rxdata[2] >> 3);
        //                        iqa_y[3] = Convert.ToByte(rxdata[3] >> 3);
        //                        iqa_y[4] = Convert.ToByte(rxdata[4] >> 3);
        //                        iqa_y[5] = Convert.ToByte(rxdata[5] >> 3);
        //                        iqa_y[6] = Convert.ToByte(rxdata[7] >> 3);

        //                        //convert it to alphanumeric
        //                        iqa_x[0] = Convert.ToByte(iqa_lookup_x[iqa_y[0]]);
        //                        iqa_x[1] = Convert.ToByte(iqa_lookup_x[iqa_y[1]]);
        //                        iqa_x[2] = Convert.ToByte(iqa_lookup_x[iqa_y[2]]);
        //                        iqa_x[3] = Convert.ToByte(iqa_lookup_x[iqa_y[3]]);
        //                        iqa_x[4] = Convert.ToByte(iqa_lookup_x[iqa_y[4]]);
        //                        iqa_x[5] = Convert.ToByte(iqa_lookup_x[iqa_y[5]]);
        //                        iqa_x[6] = Convert.ToByte(iqa_lookup_x[iqa_y[6]]);
        //                        //iqa_x[7] = Convert.ToByte(iqa_lookup_x[iqa_y[7]]);
        //                        /*SPECIAL LOGIC - WILL TAKE CARE OF THIS LATER */

        //                        Debug.WriteLine("---START ASCII--");
        //                        outputlivepararray = Rxarray;
        //                        dataValue = BytesConverter.HexToASCII(ByteArrayToString(iqa_x));
        //                        Debug.WriteLine("---END ASCII--");


        //                        ////string iqainput = "DBG64N";
        //                        ////UInt32[] iqainputlookup = new UInt32[7];
        //                        ////UInt32 ecuinpuths = 0, ecuinputls = 0;

        //                        ////for (i = 0; i < 7; i++)
        //                        ////{
        //                        ////    for (int j = 0; j < 32; j++)
        //                        ////    {
        //                        ////        if (iqainput[i] == iqa_lookup_x[j])
        //                        ////        {
        //                        ////            iqainputlookup[i] = Convert.ToByte(j);
        //                        ////        }
        //                        ////    }


        //                        ////    ecuinpuths = ((iqainputlookup[0] & 0x0000001F) << 27) | ((iqainputlookup[1] & 0x0000001F) << 22) | ((iqainputlookup[2] & 0x0000001F) << 17) | ((iqainputlookup[3] & 0x0000001F) << 12) | ((iqainputlookup[4] & 0x0000001F) << 7) | ((iqainputlookup[5] & 0x0000001F) << 2);
        //                        ////    ecuinputls = ((iqainputlookup[6] & 0x0000001F) << 27);

        //                        ////    byte[] tx = new byte[10];
        //                        ////    tx[0] = 0x2E;
        //                        ////    tx[1] = 0x02;
        //                        ////    tx[2] = 0x50;
        //                        ////    tx[3] = Convert.ToByte((ecuinpuths & 0xFF000000) >> 24);
        //                        ////    tx[4] = Convert.ToByte((ecuinpuths & 0x00FF0000) >> 16);
        //                        ////    tx[5] = Convert.ToByte((ecuinpuths & 0x0000FF00) >> 8);
        //                        ////    tx[6] = Convert.ToByte((ecuinpuths & 0x000000FF));
        //                        ////    tx[7] = Convert.ToByte((ecuinputls & 0xFF000000) >> 24);
        //                        ////    tx[8] = Convert.ToByte((ecuinputls & 0x00FF0000) >> 16);
        //                        ////    tx[9] = Convert.ToByte((ecuinputls & 0x0000FF00) >> 8);
        //                        ////    tx[10] = Convert.ToByte((ecuinputls & 0x000000FF));




        //                    }

        //                    var dataItem = new ReadParameterResponse
        //                    {
        //                        Status = pidResponse.ECUResponseStatus,
        //                        DataArray = outputlivepararray,
        //                        pidName = pidItem.pidName,
        //                        pidNumber = pidItem.pidNumber,
        //                        responseValue = dataValue


        //                    };
        //                    databyteArray.Add(dataItem);
        //                }
        //                else
        //                {
        //                    outputlivepararray[i] = 0x00;
        //                    var dataItem = new ReadParameterResponse
        //                    {
        //                        Status = pidResponse.ECUResponseStatus,
        //                        DataArray = outputlivepararray,
        //                        pidName = pidItem.pidName,
        //                        pidNumber = pidItem.pidNumber,
        //                        responseValue = dataValue

        //                    };
        //                    databyteArray.Add(dataItem);
        //                }
        //            }
        //            else
        //            {
        //                //outputlivepararray[i] = 0x00;
        //                var dataItem = new ReadParameterResponse
        //                {
        //                    Status = pidResponse.ECUResponseStatus,
        //                    //DataArray = outputlivepararray,
        //                    pidName = pidItem.pidName,
        //                    pidNumber = pidItem.pidNumber,
        //                    responseValue = pidResponse.ECUResponseStatus

        //                };
        //                databyteArray.Add(dataItem);
        //            }
        //        }
        //        catch (Exception excepption)
        //        {
        //            var dataItem = new ReadParameterResponse
        //            {
        //                Status = pidResponse.ECUResponseStatus,
        //                //DataArray = outputlivepararray,
        //                pidName = pidItem.pidName,
        //                pidNumber = pidItem.pidNumber,
        //                responseValue = excepption.Message

        //            };
        //            databyteArray.Add(dataItem);
        //        }
        //        Debug.WriteLine(" END LOOP PID NAME =" + pidItem.pidName);

        //        #region OldImplementation
        //        //while (pidBytesResponse[7] == 0x7f)
        //        //{
        //        //    //pidResponse = await dongleCommWin.ReadData();
        //        //    //pidBytesResponse = (byte[])pidResponse;
        //        //}

        //        //Check if the 1st Byte is 62 and the total byte length of the response is total byes +3 

        //        //OLD func
        //        //if (dataArray != null)
        //        //{
        //        //    //Create an object of data array of the actual data in the response received from the ECU
        //        //    //var actualResponseDataByteArray = new byte[pidItem.totalBytes];

        //        //    //Copy those data bytes into the actual response data array
        //        //    //Array.Copy(pidBytesResponse, 7 + frameLength, actualResponseDataByteArray, 0, pidItem.noOfBytes);

        //        //    //Check if the PID is NOT BitEncoded
        //        //    if (!pidItem.IsBitcoded)
        //        //    {
        //        //        //Create a object for the data byte array
        //        //        //var dataByteArray = new byte[pidItem.noOfBytes];

        //        //        //Copy the data bytes to the data byte array
        //        //        //Array.Copy(actualResponseDataByteArray, pidItem.startByte, dataByteArray, 0, pidItem.noOfBytes);

        //        //        //Add the bytes array to the collection

        //        //        var dataItem = new ReadParameterResponse
        //        //        {
        //        //            Status = dataStatus,
        //        //            DataArray = dataArray
        //        //        };
        //        //        databyteArray.Add(dataItem);
        //        //    }
        //        //    //Check if the PID is BitEncoded
        //        //    else if (pidItem.IsBitcoded)
        //        //    {
        //        //        // Create an object for the bit array
        //        //        var dataBitArrayResponse = new byte[pidItem.noofBits];

        //        //        //Copy the designated byte into the dataBitArray
        //        //        Array.Copy(dataArray, pidItem.startByte, dataBitArrayResponse, 0, pidItem.noOfBytes);

        //        //        //Get the hex string of the byte
        //        //        var hexString = BitConverter.ToString(dataBitArrayResponse);

        //        //        //Get the eight bit binary string
        //        //        string binarystring = String.Join("0000", hexString.Select(
        //        //            number => Convert.ToString(Convert.ToInt32(number.ToString(), 16), 2).PadLeft(4, '0')
        //        //            ));


        //        //        var actualNumberArray = "00000000".ToCharArray();
        //        //        var binarystringArray = binarystring.ToCharArray();

        //        //        //Copy the initial array to the binary array
        //        //        Array.Copy(binarystringArray, pidItem.startBit, actualNumberArray, 0, pidItem.noofBits);

        //        //        string binaryStr = actualNumberArray.ToString();

        //        //        //convert the strings to byte
        //        //        var byteArray = Enumerable.Range(0, int.MaxValue / 8)
        //        //                                  .Select(i => i * 8)    // get the starting index of which char segment
        //        //                                  .TakeWhile(i => i < binaryStr.Length)
        //        //                                  .Select(i => binaryStr.Substring(i, 8)) // get the binary string segments
        //        //                                  .Select(s => Convert.ToByte(s, 2)) // convert to byte
        //        //                                  .ToArray();

        //        //        var dataItem = new ReadParameterResponse
        //        //        {
        //        //            Status = dataStatus,
        //        //            DataArray = byteArray
        //        //        };
        //        //        databyteArray.Add(dataItem);
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    var dataItem = new ReadParameterResponse
        //        //    {
        //        //        Status = dataStatus,
        //        //        DataArray = pidBytesResponse
        //        //    };
        //        //    databyteArray.Add(dataItem);
        //        //}
        //        #endregion

        //    }
        //    return databyteArray;
        //}

        #endregion

        #region WriteParameters
        public async Task<ObservableCollection<WriteParameterResponse>> WriteParameters(int noOFParameters, WriteParameterIndex writeParameterIndex, ObservableCollection<WriteParameterPID> writeParameterCollection)
        {
            try
            {
                ObservableCollection<WriteParameterResponse> WriteParameterCollection = new ObservableCollection<WriteParameterResponse>();
                foreach (var pidItem in writeParameterCollection)
                {
                    var writeParamIndex = writeParameterIndex;
                    byte diagnosticsmode = 0x00;
                    byte getseedindex = 0x00;

                    if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK090A)
                    {
                        /* send 10 03, followed by 27 09, 27 0A */
                        diagnosticsmode = 0x03;
                        getseedindex = 0x09;

                    }
                    else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0102)
                    {
                        /* send 10 03, followed by 27 01, 27 02 */
                        diagnosticsmode = 0x03;
                        getseedindex = 0x01;
                    }
                    else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0B0C)
                    {
                        /* send 10 03, followed by 27 0B, 27 0C */
                        diagnosticsmode = 0x03;
                        getseedindex = 0x0B;
                    }
                    else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0304)
                    {
                        /* send 10 03, followed by 27 03, 27 04 */
                        diagnosticsmode = 0x03;
                        getseedindex = 0x03;
                    }
                    else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0506)
                    {
                        /* send 10 03, followed by 27 03, 27 04 */
                        diagnosticsmode = 0x03;
                        getseedindex = 0x05;
                    }


                    byte[] TxFrame = new byte[2];
                    /* Send start diagnostics mode command */
                    TxFrame[0] = 0x10;
                    TxFrame[1] = diagnosticsmode;
                    int frameLength = 2;

                    //Send the parameter ID to the ECU
                    var pidResponse = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                    string return_status = string.Empty;

                    if (pidResponse.ECUResponseStatus == "NOERROR")
                    {
                        /* Send get seed command to ECU */
                        TxFrame[0] = 0x27;
                        TxFrame[1] = getseedindex;
                        var tx_Frame_len = 2;
                        var pidResponsebytes = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                        var status = pidResponsebytes.ECUResponseStatus;

                        if (status == "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED")
                        {
                            Thread.Sleep(11000);

                            pidResponsebytes = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                            status = pidResponsebytes.ECUResponseStatus;
                        }

                        if (status == "NOERROR")
                        {
                            var datasArrays = pidResponsebytes.ActualDataBytes;
                            var Rxarray = pidResponsebytes.ActualDataBytes;
                            var Rxsize = Rxarray.Length;
                            int seedkeyindex = getseedindex;

                            var seedarray = new byte[Rxsize - 2];
                            /* get seed from the response and send it to the seedkey dll to get the key */
                            //for (int i = 0; i < Rxsize - 2; i++)
                            //{
                            //    seedarray[i] = Rxarray[i + 2];
                            //}
                            Array.Copy(Rxarray, 2, seedarray, 0, Rxsize - 2);

                            //status = getkeyfromseed(seedkeyindex, seedsize, seedarray, &keysize, keyarray);

                            byte numkeybytes = new byte();

                            byte[] actualKey = new byte[32];
                            calculateSeedkey = new ECUCalculateSeedkey();
                            Byte seedkeylen = 0;

                            if ((pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SML_BOSCH_BS6_PROD||
                                (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.KOEL_BOSCH_BS4)) ||
                                    (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.BAJAJ_RIPEMD160_SECURITY))
                            {
                                seedkeylen = 8;
                            }
                            else if ((pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_BS6) ||
                                    (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_AMT_BS6))
                            {
                                seedkeylen = 4;
                            }
                            else if (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_TCIC)
                            {
                                seedkeylen = 2;
                            }
                            //else if ((pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD) ||
                            //(pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_DEV))
                            //{
                            //    seedkeylen = 4;
                            //}
                            else
                            {
                                seedkeylen = 16;
                            }

                            if (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_BS6_A46)
                            {
                                var enums = string.Empty;
                                if (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.GREAVES_SEDEMAC_BS6)
                                {
                                    enums = "GREAVES_SEDEMAC_APP_BS6";
                                }
                                else
                                {
                                    enums = Enum.GetName(pidItem.seedkeyindex.GetType(), pidItem.seedkeyindex);
                                }
                                var seed_key = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), enums);

                                /*var result = */calculateSeedkey.CalculateSeedkey(seed_key, seedkeylen, ref numkeybytes, seedarray, ref actualKey); 
                            }
                            else
                            {
                                //SEEDKEYINDEXTYPE index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), pidItem.seedkeyindex);

                                /*var result = */
                                calculateSeedkey.CalculateSeedkey((ECUSeedkeyPCL.SEEDKEYINDEXTYPE)pidItem.seedkeyindex, seedkeylen, ref numkeybytes, seedarray, ref actualKey); 
                            }

                            if (status == "NOERROR")
                            {
                                byte[] newTxFrame = new byte[numkeybytes + 2];
                                /* Send calculated key to ECU */
                                newTxFrame[0] = 0x27;
                                newTxFrame[1] = Convert.ToByte(seedkeyindex + 1);

                                Array.Copy(actualKey, 0, newTxFrame, 2, numkeybytes);

                                tx_Frame_len = Rxsize;
                                frameLength = newTxFrame.Length;

                                //Send the parameter ID to the ECU
                                var pidResponse3 = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(newTxFrame));

                                var writeParaPID = HexStringToByteArray(pidItem.writeparapid);
                                var writeparaframesize = pidItem.writeparadata.Length;

                                if (pidResponse3.ECUResponseStatus == "NOERROR")
                                {
                                    byte[] writeFrame = new byte[1 + writeParaPID.Length + pidItem.totalBytes];
                                    /* write new value to ECU */
                                    writeFrame[0] = 0x2E;

                                    Array.Copy(writeParaPID, 0, writeFrame, 1, writeParaPID.Length);

                                    if (pidItem.ReadParameterPID_DataType == "IQA")
                                    {
                                        byte[] txdata = new byte[32];
                                        //KOEL
                                        Array.Resize(ref writeFrame, 1 + writeParaPID.Length + 32);
                                        tx_Frame_len = 1 + writeParaPID.Length + 32;
                                        char[] iqa_lookup_x = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8' };
                                        
                                       
                                     
                                        for (int noinj=0; noinj<1; noinj++)
                                        {
                                            UInt32 temp = 0;
                                            byte[] iqa_y = new byte[7];
                                            byte[] iqa_x = new byte[8];

                                           // Array.Copy(pidItem.writeparadata, 0+(noinj * 7), iqa_y, 0, 7);

                                            for (byte i = 0; i < 7; i++)
                                            {
                                                for (byte j = 0; j < 32; j++)
                                                {
                                                    if (iqa_lookup_x[j] == Char.ToUpper((char)pidItem.writeparadata[i+(noinj*7)]))
                                                    {
                                                        iqa_y[i] = j;
                                                        break;
                                                    }
                                                }
                                            }

                                            temp = (((UInt32)iqa_y[0] & 0x0000001F) << 27) |
                                                  (((UInt32)iqa_y[1] & 0x0000001F) << 22) |
                                                   (((UInt32)iqa_y[2] & 0x0000001F) << 17) |
                                                   (((UInt32)iqa_y[3] & 0x0000001F) << 12) |
                                                   (((UInt32)iqa_y[4] & 0x0000001F) << 7) |
                                                   (((UInt32)iqa_y[5] & 0x0000001F ) << 2);

                                            iqa_x[0] = (byte)((temp & 0xFF000000) >> 24);
                                            iqa_x[1] = (byte)((temp & 0x00FF0000) >> 16);
                                            iqa_x[2] = (byte)((temp & 0x0000FF00) >> 8);
                                            iqa_x[3] = (byte)((temp & 0x000000FF));
                                            iqa_x[4] = (byte)(iqa_y[6] << 3);
                                            iqa_x[5] = 0;
                                            iqa_x[6] = 0;
                                            iqa_x[7] = 0;

                                            Array.Copy(iqa_x, 0, writeFrame, writeParaPID.Length + pidItem.startByte + (noinj * 8), 8);
                                        }
                                        

                                       
                                    }
                                    else if (pidItem.ReadParameterPID_DataType == "GEARRATIO_IDM")
                                    {
                                        tx_Frame_len = 1 + writeParaPID.Length + pidItem.totalBytes;
                                        Array.Copy(pidItem.writeparadata, 0, writeFrame, writeParaPID.Length + pidItem.startByte, pidItem.writeparadata.Length);
                                    }
                                    else
                                    {
                                        tx_Frame_len = 1 + writeParaPID.Length + pidItem.totalBytes;
                                        Array.Copy(pidItem.writeparadata, 0, writeFrame, writeParaPID.Length + pidItem.startByte, pidItem.writeparadata.Length);
                                    }

                                    var pidResponse4 = await dongleComm.CAN_TxRx(writeFrame.Length, ByteArrayToString(writeFrame));

                                    status = pidResponse4.ECUResponseStatus;

                                    if (status == "NOERROR")
                                    {
                                        return_status = "NOERROR";

                                    }
                                    else
                                    {
                                        return_status = status;
                                    }
                                    var dataItem = new WriteParameterResponse
                                    {
                                        Status = pidResponse4.ECUResponseStatus,
                                        DataArray = pidResponse4.ActualDataBytes,
                                        pidName = pidItem.writeparapid,
                                        pidNumber = pidItem.writeparano,

                                        responseValue = pidResponse4.ECUResponseStatus

                                    };
                                    WriteParameterCollection.Add(dataItem);
                                }
                                else
                                {
                                    //key issue
                                    return_status = status;
                                    var dataItem = new WriteParameterResponse
                                    {
                                        Status = pidResponse3.ECUResponseStatus,
                                        DataArray = pidResponse3.ActualDataBytes,
                                        pidName = pidItem.writeparapid,
                                        pidNumber = pidItem.writeparano,

                                        responseValue = pidResponse3.ECUResponseStatus

                                    };
                                    WriteParameterCollection.Add(dataItem);
                                }
                            }
                            else
                            {
                                return_status = status;
                                var dataItem = new WriteParameterResponse
                                {
                                    Status = pidResponse.ECUResponseStatus,
                                    DataArray = pidResponse.ActualDataBytes,
                                    pidName = pidItem.writeparapid,
                                    pidNumber = pidItem.writeparano,

                                    responseValue = pidResponse.ECUResponseStatus

                                };
                                WriteParameterCollection.Add(dataItem);
                            }
                        }
                        else
                        {
                            return_status = status;
                            var dataItem = new WriteParameterResponse
                            {
                                Status = pidResponsebytes.ECUResponseStatus,
                                DataArray = pidResponsebytes.ActualDataBytes,
                                pidName = pidItem.writeparapid,
                                pidNumber = pidItem.writeparano,

                                responseValue = pidResponsebytes.ECUResponseStatus

                            };
                            WriteParameterCollection.Add(dataItem);
                        }
                    }
                    else
                    {
                        return_status = pidResponse.ECUResponseStatus;
                        var dataItem = new WriteParameterResponse
                        {
                            Status = pidResponse.ECUResponseStatus,
                            DataArray = pidResponse.ActualDataBytes,
                            pidName = pidItem.writeparapid,
                            pidNumber = pidItem.writeparano,

                            responseValue = pidResponse.ECUResponseStatus

                        };
                        WriteParameterCollection.Add(dataItem);

                    }
                }



                return WriteParameterCollection;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        #endregion


        #region WriteParameters Atuator Test
        public async Task<ObservableCollection<WriteParameterResponse>> AtuatorTestWriteParameters(int noOFParameters, WriteParameterIndex writeParameterIndex, ObservableCollection<WriteParameterPID> writeParameterCollection, bool IsPlay)
        {
            ObservableCollection<WriteParameterResponse> WriteParameterCollection = new ObservableCollection<WriteParameterResponse>();
            foreach (var pidItem in writeParameterCollection)
            {
                var writeParamIndex = writeParameterIndex;
                byte diagnosticsmode = 0x00;
                byte getseedindex = 0x00;
                if (writeParamIndex == WriteParameterIndex.UDS)
                {
                    /* send 10 03, followed by 27 09, 27 0A */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x01;

                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK090A)
                {
                    /* send 10 03, followed by 27 09, 27 0A */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x09;

                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0102)
                {
                    /* send 10 03, followed by 27 01, 27 02 */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x01;
                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0B0C)
                {
                    /* send 10 03, followed by 27 0B, 27 0C */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x0B;
                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0304)
                {
                    /* send 10 03, followed by 27 03, 27 04 */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x03;
                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0506)
                {
                    /* send 10 03, followed by 27 03, 27 04 */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x05;
                }


                byte[] TxFrame = new byte[2];
                /* Send start diagnostics mode command */
                TxFrame[0] = 0x10;
                TxFrame[1] = diagnosticsmode;
                int frameLength = 2;

                //Send the parameter ID to the ECU
                var pidResponse = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                string return_status = string.Empty;

                if (pidResponse.ECUResponseStatus == "NOERROR")
                {
                    /* Send get seed command to ECU */
                    TxFrame[0] = 0x27;
                    TxFrame[1] = getseedindex;
                    var tx_Frame_len = 2;
                    var pidResponsebytes = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                    var status = pidResponsebytes.ECUResponseStatus;

                    if (status == "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED")
                    {
                        Thread.Sleep(11000);

                        pidResponsebytes = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                        status = pidResponsebytes.ECUResponseStatus;
                    }

                    if (status == "NOERROR")
                    {
                        var datasArrays = pidResponsebytes.ActualDataBytes;
                        var Rxarray = pidResponsebytes.ActualDataBytes;
                        var Rxsize = Rxarray.Length;
                        int seedkeyindex = getseedindex;

                        var seedarray = new byte[Rxsize - 2];
                        /* get seed from the response and send it to the seedkey dll to get the key */
                        //for (int i = 0; i < Rxsize - 2; i++)
                        //{
                        //    seedarray[i] = Rxarray[i + 2];
                        //}
                        Array.Copy(Rxarray, 2, seedarray, 0, Rxsize - 2);

                        //status = getkeyfromseed(seedkeyindex, seedsize, seedarray, &keysize, keyarray);

                        byte numkeybytes = new byte();

                        byte[] actualKey = new byte[32];
                        calculateSeedkey = new ECUCalculateSeedkey();
                        Byte seedkeylen = 0;

                        if (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SML_BOSCH_BS6_PROD)
                        {
                            seedkeylen = 8;
                        }
                        else if ((pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_BS6) ||
                                (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_AMT_BS6))
                        {
                            seedkeylen = 4;
                        }
                        else if (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_TCIC)
                        {
                            seedkeylen = 2;
                        }
                        else
                        {
                            seedkeylen = 16;
                        }

                        //SEEDKEYINDEXTYPE index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), pidItem.seedkeyindex);

                        var result = calculateSeedkey.CalculateSeedkey((ECUSeedkeyPCL.SEEDKEYINDEXTYPE)pidItem.seedkeyindex, seedkeylen, ref numkeybytes, seedarray, ref actualKey);

                        if (status == "NOERROR")
                        {
                            byte[] newTxFrame = new byte[numkeybytes + 2];
                            /* Send calculated key to ECU */
                            newTxFrame[0] = 0x27;
                            newTxFrame[1] = Convert.ToByte(seedkeyindex + 1);

                            Array.Copy(actualKey, 0, newTxFrame, 2, numkeybytes);

                            tx_Frame_len = Rxsize;
                            frameLength = newTxFrame.Length;

                            //Send the parameter ID to the ECU
                            var pidResponse3 = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(newTxFrame));

                            var writeParaPID = HexStringToByteArray(pidItem.writeparapid);
                            var writeparaframesize = pidItem.writeparadata.Length;

                            if (pidResponse3.ECUResponseStatus == "NOERROR")
                            {
                                byte[] writeFrame = new byte[2 + writeParaPID.Length + pidItem.writeparadata.Length];
                                /* write new value to ECU */
                                // writeFrame[0] = 0x2E;
                                //writeParaPID = HexStringToByteArray(pidItem.writeparapid);
                                //var writeparaframesize = writeParaPID.Length;
                                //writeFrame = new byte[2 + writeParaPID.Length + pidItem.totalBytes];
                                writeFrame[0] = 0x2F;
                                Array.Copy(writeParaPID, 0, writeFrame, 1, writeParaPID.Length);


                                if (IsPlay)
                                {
                                    writeFrame[3] = 0x03;
                                }
                                else
                                {
                                    writeFrame[3] = 0x00;
                                }



                                //Array.Copy(writeParaPID, 0, writeFrame, 1, writeParaPID.Length);

                                if (pidItem.ReadParameterPID_DataType == "IQA")
                                {
                                    byte[] txdata = new byte[8];
                                    Array.Resize(ref writeFrame, 1 + writeParaPID.Length + 8);
                                    tx_Frame_len = 1 + writeParaPID.Length + 8;
                                    char[] iqa_lookup_x = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8' };
                                    byte[] iqa_y = new byte[7];
                                    byte[] iqa_x = new byte[8];
                                    UInt32 temp = 0;

                                    for (byte i = 0; i < 7; i++)
                                    {
                                        for (byte j = 0; j < 32; j++)
                                        {
                                            if (iqa_lookup_x[j] == Char.ToUpper((char)pidItem.writeparadata[i]))
                                            {
                                                iqa_y[i] = j;
                                                break;
                                            }
                                        }
                                    }

                                    temp = (((UInt32)iqa_y[0] & 0x0000001F) << 27) |
                                           (((UInt32)iqa_y[1] & 0x0000001F) << 22) |
                                           (((UInt32)iqa_y[2] & 0x0000001F) << 17) |
                                           (((UInt32)iqa_y[3] & 0x0000001F) << 12) |
                                           (((UInt32)iqa_y[4] & 0x0000001F) << 7) |
                                           (((UInt32)iqa_y[5] & 0x0000001F) << 2);

                                    iqa_x[0] = (byte)((temp & 0xFF000000) >> 24);
                                    iqa_x[1] = (byte)((temp & 0x00FF0000) >> 16);
                                    iqa_x[2] = (byte)((temp & 0x0000FF00) >> 8);
                                    iqa_x[3] = (byte)((temp & 0x000000FF));
                                    iqa_x[4] = (byte)(iqa_y[6] << 3);
                                    iqa_x[5] = 0;
                                    iqa_x[6] = 0;
                                    iqa_x[7] = 0;

                                    Array.Copy(iqa_x, 0, writeFrame, writeParaPID.Length + pidItem.startByte, 8);
                                }
                                else if (pidItem.ReadParameterPID_DataType == "GEARRATIO_IDM")
                                {
                                    tx_Frame_len = 1 + writeParaPID.Length + pidItem.totalBytes;
                                    Array.Copy(pidItem.writeparadata, 0, writeFrame, writeParaPID.Length + pidItem.startByte, pidItem.writeparadata.Length);
                                }
                                else
                                {
                                    if (IsPlay)
                                    {
                                        Array.Copy(pidItem.writeparadata, 0, writeFrame, writeParaPID.Length + pidItem.startByte + 1, pidItem.writeparadata.Length);
                                        //tx_Frame_len = 1 + writeParaPID.Length + pidItem.totalBytes;
                                        //Array.Copy(pidItem.writeparadata, 0, writeFrame, writeParaPID.Length + pidItem.startByte, pidItem.writeparadata.Length);
                                    }
                                }

                                //writeFrame[0] = 0x2F;
                                //writeFrame[1] = 0x00;
                                //writeFrame[2] = 0x39;
                                //writeFrame[3] = 0x03;
                                //writeFrame[4] = 0x00;
                                //writeFrame[5] = 0x05;

                                var pidResponse4 = await dongleComm.CAN_TxRx(writeFrame.Length, ByteArrayToString(writeFrame));

                                status = pidResponse4.ECUResponseStatus;

                                if (status == "NOERROR")
                                {
                                    return_status = "NOERROR";

                                }
                                else
                                {
                                    return_status = status;
                                }
                                var dataItem = new WriteParameterResponse
                                {
                                    Status = pidResponse4.ECUResponseStatus,
                                    DataArray = pidResponse4.ActualDataBytes,
                                    pidName = pidItem.writeparapid,
                                    pidNumber = pidItem.writeparano,

                                    responseValue = pidResponse4.ECUResponseStatus

                                };
                                WriteParameterCollection.Add(dataItem);
                            }
                            else
                            {
                                //key issue
                                return_status = status;
                                var dataItem = new WriteParameterResponse
                                {
                                    Status = pidResponse3.ECUResponseStatus,
                                    DataArray = pidResponse3.ActualDataBytes,
                                    pidName = pidItem.writeparapid,
                                    pidNumber = pidItem.writeparano,

                                    responseValue = pidResponse3.ECUResponseStatus

                                };
                                WriteParameterCollection.Add(dataItem);
                            }
                        }
                        else
                        {
                            return_status = status;
                            var dataItem = new WriteParameterResponse
                            {
                                Status = pidResponse.ECUResponseStatus,
                                DataArray = pidResponse.ActualDataBytes,
                                pidName = pidItem.writeparapid,
                                pidNumber = pidItem.writeparano,

                                responseValue = pidResponse.ECUResponseStatus

                            };
                            WriteParameterCollection.Add(dataItem);
                        }
                    }
                    else
                    {
                        return_status = status;
                        var dataItem = new WriteParameterResponse
                        {
                            Status = pidResponsebytes.ECUResponseStatus,
                            DataArray = pidResponsebytes.ActualDataBytes,
                            pidName = pidItem.writeparapid,
                            pidNumber = pidItem.writeparano,

                            responseValue = pidResponsebytes.ECUResponseStatus

                        };
                        WriteParameterCollection.Add(dataItem);
                    }
                }
                else
                {
                    return_status = pidResponse.ECUResponseStatus;
                    var dataItem = new WriteParameterResponse
                    {
                        Status = pidResponse.ECUResponseStatus,
                        DataArray = pidResponse.ActualDataBytes,
                        pidName = pidItem.writeparapid,
                        pidNumber = pidItem.writeparano,

                        responseValue = pidResponse.ECUResponseStatus

                    };
                    WriteParameterCollection.Add(dataItem);

                }
            }



            return WriteParameterCollection;

        }
        #endregion


        #region IOR Test
        bool test_condition;
        bool StopTimer = true;
        public async Task<ResponseArrayStatus> IORTestParameters1(Enums.SEEDKEYINDEXTYPE seedkeyindex1,
            WriteParameterIndex writeParameterIndex, string start_command, string request_command,
            string stop_command, bool TestCondition, int bit_position, string active_command,
            string complete_command, string fail_command, bool is_stop, int time_base)
        {
            Debug.WriteLine("-------START IOR TEST METHOD-------");
            ResponseArrayStatus responseArrayStatus = new ResponseArrayStatus();
            if (!is_stop)
            {
                Debug.WriteLine("-------Entered-------");
                ObservableCollection<WriteParameterResponse> WriteParameterCollection = new ObservableCollection<WriteParameterResponse>();
                test_condition = true;
                var writeParamIndex = writeParameterIndex;
                byte diagnosticsmode = 0x00;
                byte getseedindex = 0x00;
                if (writeParamIndex == WriteParameterIndex.UDS)
                {
                    /* send 10 03, followed by 27 09, 27 0A */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x01;

                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK090A)
                {
                    /* send 10 03, followed by 27 09, 27 0A */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x09;

                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0102)
                {
                    /* send 10 03, followed by 27 01, 27 02 */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x01;
                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0B0C)
                {
                    /* send 10 03, followed by 27 0B, 27 0C */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x0B;
                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0304)
                {
                    /* send 10 03, followed by 27 03, 27 04 */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x03;
                }
                else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0506)
                {
                    /* send 10 03, followed by 27 03, 27 04 */
                    diagnosticsmode = 0x03;
                    getseedindex = 0x05;
                }


                byte[] TxFrame = new byte[2];
                /* Send start diagnostics mode command */
                TxFrame[0] = 0x10;
                TxFrame[1] = diagnosticsmode;
                int frameLength = 2;

                //Send the parameter ID to the ECU
                Debug.WriteLine("-------Send the parameter ID to the ECU-------");
                responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));

                if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                {
                    /* Send get seed command to ECU */
                    TxFrame[0] = 0x27;
                    TxFrame[1] = getseedindex;
                    var tx_Frame_len = 2;
                    Debug.WriteLine("-------Send get seed command to ECU-------");
                    responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                    //var status = pidResponsebytes.ECUResponseStatus;

                    if (responseArrayStatus.ECUResponseStatus == "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED")
                    {
                        Thread.Sleep(11000);

                        responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                        //status = pidResponsebytes.ECUResponseStatus;
                    }

                    if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                    {
                        //var datasArrays = pidResponsebytes.ActualDataBytes;
                        //var Rxarray = pidResponsebytes.ActualDataBytes;
                        //var Rxsize = Rxarray.Length;
                        int seedkeyindex = getseedindex;

                        var seedarray = new byte[responseArrayStatus.ActualDataBytes.Length - 2];
                        /* get seed from the response and send it to the seedkey dll to get the key */
                        //for (int i = 0; i < Rxsize - 2; i++)
                        //{
                        //    seedarray[i] = Rxarray[i + 2];
                        //}
                        Array.Copy(responseArrayStatus.ActualDataBytes, 2, seedarray, 0, responseArrayStatus.ActualDataBytes.Length - 2);

                        //status = getkeyfromseed(seedkeyindex, seedsize, seedarray, &keysize, keyarray);

                        byte numkeybytes = new byte();

                        byte[] actualKey = new byte[32];
                        calculateSeedkey = new ECUCalculateSeedkey();
                        Byte seedkeylen = 0;

                        if (seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SML_BOSCH_BS6_PROD)
                        {
                            seedkeylen = 8;
                        }
                        else if ((seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_BS6) ||
                                (seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_AMT_BS6))
                        {
                            seedkeylen = 4;
                        }
                        else if (seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_TCIC)
                        {
                            seedkeylen = 2;
                        }
                        else
                        {
                            seedkeylen = 16;
                        }

                        //SEEDKEYINDEXTYPE index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), pidItem.seedkeyindex);

                        var result = calculateSeedkey.CalculateSeedkey((ECUSeedkeyPCL.SEEDKEYINDEXTYPE)seedkeyindex1, seedkeylen, ref numkeybytes, seedarray, ref actualKey);

                        if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                        {
                            byte[] newTxFrame = new byte[numkeybytes + 2];
                            /* Send calculated key to ECU */
                            newTxFrame[0] = 0x27;
                            newTxFrame[1] = Convert.ToByte(seedkeyindex + 1);

                            Array.Copy(actualKey, 0, newTxFrame, 2, numkeybytes);

                            tx_Frame_len = responseArrayStatus.ActualDataBytes.Length;
                            frameLength = newTxFrame.Length;

                            //Send the parameter ID to the ECU
                            responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(newTxFrame));

                            //var writeParaPID = HexStringToByteArray(command);
                            //var writeparaframesize = command.Length;

                            if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                            {
                                Debug.WriteLine("-------START COMMAND SENDING-------");
                                responseArrayStatus = await this.dongleComm.CAN_TxRx(start_command.Length / 2, start_command);
                                //bool Condition = TestCondition;
                                if (responseArrayStatus.ECUResponseStatus == "NOERROR")//"ECUERROR_CONDITIONSNOTCORRECT")//"NOERROR")
                                {
                                    if (time_base > 0)
                                    {
                                        StopTimer = test_condition = true;
                                        Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                                        {
                                            Xamarin.Forms.Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                                           {

                                               time_base -= 1;
                                               Console.WriteLine($"Count : {time_base}");
                                               if ((time_base < 1) && test_condition)
                                               {
                                                   Console.WriteLine($"Condition True : {time_base}");
                                                   StopTimer = test_condition = false;
                                                   Debug.WriteLine("-------STOP COMMAND SENDING-------");
                                                   responseArrayStatus = this.dongleComm.CAN_TxRx(stop_command.Length / 2, stop_command).Result;
                                                   if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                                                   {
                                                       responseArrayStatus.ECUResponseStatus = "Test Stopped";
                                                   }
                                               }
                                               return StopTimer;
                                           });
                                        });
                                    }

                                    while (test_condition)
                                    {
                                        //Debug.WriteLine("-------REQUEST COMMAND SENDING-------");
                                        responseArrayStatus = await this.dongleComm.CAN_TxRx(request_command.Length / 2, request_command);
                                        //Console.WriteLine($"Timer While Count : {time_base} = {DateTime.Now.ToString("hh:mm:ss:fff")}");
                                        //responseArrayStatus.ECUResponseStatus = "Running Test";

                                        if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                                        {

                                            if (responseArrayStatus.ActualDataBytes[bit_position - 1] != HexStringToByteArray(active_command)[0])
                                            {
                                                test_condition = false;
                                                if (responseArrayStatus.ActualDataBytes[bit_position - 1] == HexStringToByteArray(complete_command)[0])
                                                {
                                                    if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                                                    {
                                                        responseArrayStatus.ECUResponseStatus = "Test Completed";
                                                    }
                                                }

                                                else if (responseArrayStatus.ActualDataBytes[bit_position - 1] == HexStringToByteArray(fail_command)[0])
                                                {
                                                    if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                                                    {
                                                        responseArrayStatus.ECUResponseStatus = "Test Aborted";
                                                    }
                                                }
                                                else
                                                {

                                                }
                                                test_condition = StopTimer = false;
                                            }
                                        }
                                        else
                                        {
                                            test_condition = StopTimer = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //key issue
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            else
            {
                StopTimer = test_condition = false;
                Debug.WriteLine("-------STOP COMMAND SENDING------- Stop");
                responseArrayStatus = await this.dongleComm.CAN_TxRx(stop_command.Length / 2, stop_command);
                if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                {
                    responseArrayStatus.ECUResponseStatus = "Test Stopped";
                }
            }
            return responseArrayStatus;
        }

        public async Task<ResponseArrayStatus> IORTestParameters(Enums.SEEDKEYINDEXTYPE seedkeyindex1,
          WriteParameterIndex writeParameterIndex, string start_command, string request_command,
          string stop_command, bool TestCondition, int bit_position, List<string> active_command,
          string complete_command, string fail_command, bool is_stop, int time_base)
        {
            Debug.WriteLine("-------START IOR TEST METHOD-------");
            ResponseArrayStatus responseArrayStatus = new ResponseArrayStatus();
            Debug.WriteLine("-------Entered-------");
            ObservableCollection<WriteParameterResponse> WriteParameterCollection = new ObservableCollection<WriteParameterResponse>();
            test_condition = true;
            var writeParamIndex = writeParameterIndex;
            byte diagnosticsmode = 0x00;
            byte getseedindex = 0x00;
            if (writeParamIndex == WriteParameterIndex.UDS)
            {
                /* send 10 03, followed by 27 09, 27 0A */
                diagnosticsmode = 0x03;
                getseedindex = 0x01;

            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK090A)
            {
                /* send 10 03, followed by 27 09, 27 0A */
                diagnosticsmode = 0x03;
                getseedindex = 0x09;

            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0102)
            {
                /* send 10 03, followed by 27 01, 27 02 */
                diagnosticsmode = 0x03;
                getseedindex = 0x01;
            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0B0C)
            {
                /* send 10 03, followed by 27 0B, 27 0C */
                diagnosticsmode = 0x03;
                getseedindex = 0x0B;
            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0304)
            {
                /* send 10 03, followed by 27 03, 27 04 */
                diagnosticsmode = 0x03;
                getseedindex = 0x03;
            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0506)
            {
                /* send 10 03, followed by 27 03, 27 04 */
                diagnosticsmode = 0x03;
                getseedindex = 0x05;
            }


            byte[] TxFrame = new byte[2];
            /* Send start diagnostics mode command */
            TxFrame[0] = 0x10;
            TxFrame[1] = diagnosticsmode;
            int frameLength = 2;

            //Send the parameter ID to the ECU
            Debug.WriteLine("-------Send the parameter ID to the ECU-------");
            responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));

            if (responseArrayStatus.ECUResponseStatus == "NOERROR")
            {
                /* Send get seed command to ECU */
                TxFrame[0] = 0x27;
                TxFrame[1] = getseedindex;
                var tx_Frame_len = 2;
                Debug.WriteLine("-------Send get seed command to ECU-------");
                responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                //var status = pidResponsebytes.ECUResponseStatus;

                if (responseArrayStatus.ECUResponseStatus == "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED")
                {
                    Thread.Sleep(11000);

                    responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                    //status = pidResponsebytes.ECUResponseStatus;
                }

                if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                {
                    //var datasArrays = pidResponsebytes.ActualDataBytes;
                    //var Rxarray = pidResponsebytes.ActualDataBytes;
                    //var Rxsize = Rxarray.Length;
                    int seedkeyindex = getseedindex;

                    var seedarray = new byte[responseArrayStatus.ActualDataBytes.Length - 2];
                    /* get seed from the response and send it to the seedkey dll to get the key */
                    //for (int i = 0; i < Rxsize - 2; i++)
                    //{
                    //    seedarray[i] = Rxarray[i + 2];
                    //}
                    Array.Copy(responseArrayStatus.ActualDataBytes, 2, seedarray, 0, responseArrayStatus.ActualDataBytes.Length - 2);

                    //status = getkeyfromseed(seedkeyindex, seedsize, seedarray, &keysize, keyarray);

                    byte numkeybytes = new byte();

                    byte[] actualKey = new byte[32];
                    calculateSeedkey = new ECUCalculateSeedkey();
                    Byte seedkeylen = 0;

                    if (seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SML_BOSCH_BS6_PROD)
                    {
                        seedkeylen = 8;
                    }
                    else if ((seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_BS6) ||
                            (seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_AMT_BS6))
                    {
                        seedkeylen = 4;
                    }
                    else if (seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_TCIC)
                    {
                        seedkeylen = 2;
                    }
                    else
                    {
                        seedkeylen = 16;
                    }

                    //SEEDKEYINDEXTYPE index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), pidItem.seedkeyindex);

                    var result = calculateSeedkey.CalculateSeedkey((ECUSeedkeyPCL.SEEDKEYINDEXTYPE)seedkeyindex1, seedkeylen, ref numkeybytes, seedarray, ref actualKey);

                    if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                    {
                        byte[] newTxFrame = new byte[numkeybytes + 2];
                        /* Send calculated key to ECU */
                        newTxFrame[0] = 0x27;
                        newTxFrame[1] = Convert.ToByte(seedkeyindex + 1);

                        Array.Copy(actualKey, 0, newTxFrame, 2, numkeybytes);

                        tx_Frame_len = responseArrayStatus.ActualDataBytes.Length;
                        frameLength = newTxFrame.Length;

                        //Send the parameter ID to the ECU
                        responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(newTxFrame));

                        //var writeParaPID = HexStringToByteArray(command);
                        //var writeparaframesize = command.Length;

                        if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                        {
                            Debug.WriteLine("-------START COMMAND SENDING-------");
                            responseArrayStatus = await this.dongleComm.CAN_TxRx(start_command.Length / 2, start_command);
                        }
                        else
                        {
                            //key issue
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            else
            {
            }
            return responseArrayStatus;
        }

        bool active_con = false;

        public async Task<ResponseArrayStatus> IORTestParameters2(Enums.SEEDKEYINDEXTYPE seedkeyindex1,
       WriteParameterIndex writeParameterIndex, string start_command, string request_command,
       string stop_command, bool TestCondition, int bit_position, List<string> active_command,
       string complete_command, string fail_command, bool is_stop, int time_base, bool is_timebase)
        {
            Debug.WriteLine("-------START IOR TEST METHOD-------");
            ResponseArrayStatus responseArrayStatus = new ResponseArrayStatus();
            if (!is_stop)
            {
                if (is_timebase)
                {
                    if (time_base > 0)
                    {
                        StopTimer = test_condition = true;
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                        {
                            Xamarin.Forms.Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                            {

                                time_base -= 1;
                                Console.WriteLine($"Count : {time_base}");
                                if ((time_base < 1) && test_condition)
                                {
                                    Console.WriteLine($"Condition True : {time_base}");
                                    StopTimer = test_condition = false;
                                    Debug.WriteLine("-------STOP COMMAND SENDING-------");
                                    responseArrayStatus = this.dongleComm.CAN_TxRx(stop_command.Length / 2, stop_command).Result;
                                    if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                                    {
                                        responseArrayStatus.ECUResponseStatus = "Test Stopped";
                                    }
                                }
                                return StopTimer;
                            });
                        });
                    }
                }
                while (test_condition)
                {
                    //Debug.WriteLine("-------REQUEST COMMAND SENDING-------");
                    responseArrayStatus = await this.dongleComm.CAN_TxRx(request_command.Length / 2, request_command);
                    //Console.WriteLine($"Timer While Count : {time_base} = {DateTime.Now.ToString("hh:mm:ss:fff")}");
                    //responseArrayStatus.ECUResponseStatus = "Running Test";

                    if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                    {
                        var pos = responseArrayStatus.ActualDataBytes[bit_position - 1];
                        var com_byte = HexStringToByteArray(complete_command)[0];
                        active_con = false;
                        foreach (var active in active_command.ToList())
                        {
                            var active_command1 = HexStringToByteArray(active)[0];
                            if (responseArrayStatus.ActualDataBytes[bit_position - 1] == HexStringToByteArray(active)[0])
                            {
                                active_con = true;
                            }

                            Console.WriteLine($"Compare Byte = {pos} - {com_byte} - {active_command1}");
                        }
                        //var active_command1 = HexStringToByteArray(active_command)[0];

                        //Console.WriteLine($"Compare Byte = {pos} - {com_byte} - {active_command1}");


                        if (!active_con)
                        {
                            test_condition = false;

                          


                            if (responseArrayStatus.ActualDataBytes[bit_position - 1] == HexStringToByteArray(complete_command)[0])
                            {
                                if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                                {
                                    responseArrayStatus.ECUResponseStatus = "Test Completed";
                                }
                            }

                            else if (responseArrayStatus.ActualDataBytes[bit_position - 1] == HexStringToByteArray(fail_command)[0])
                            {
                                if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                                {
                                    responseArrayStatus.ECUResponseStatus = "Test Aborted";
                                }
                            }
                            else
                            {

                            }
                            test_condition = StopTimer = false;
                        }
                    }
                    else
                    {
                        test_condition = StopTimer = false;
                    }
                }
            }
            else
            {
                StopTimer = test_condition = false;
                Debug.WriteLine("-------STOP COMMAND SENDING------- Stop");
                responseArrayStatus = await this.dongleComm.CAN_TxRx(stop_command.Length / 2, stop_command);
                if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                {
                    responseArrayStatus.ECUResponseStatus = "Test Stopped";
                }
            }
            return responseArrayStatus;
        }


        public async Task<ResponseArrayStatus> StartIdIOR(Enums.SEEDKEYINDEXTYPE seedkeyindex1, WriteParameterIndex writeParameterIndex, string start_command)
        {
            Debug.WriteLine("-------START IOR TEST METHOD-------");
            ResponseArrayStatus responseArrayStatus = new ResponseArrayStatus();
            Debug.WriteLine("-------Entered-------");
            //ObservableCollection<WriteParameterResponse> WriteParameterCollection = new ObservableCollection<WriteParameterResponse>();
            test_condition = true;
            var writeParamIndex = writeParameterIndex;
            byte diagnosticsmode = 0x00;
            byte getseedindex = 0x00;
            if (writeParamIndex == WriteParameterIndex.UDS)
            {
                /* send 10 03, followed by 27 09, 27 0A */
                diagnosticsmode = 0x03;
                getseedindex = 0x01;

            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK090A)
            {
                /* send 10 03, followed by 27 09, 27 0A */
                diagnosticsmode = 0x03;
                getseedindex = 0x09;

            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0102)
            {
                /* send 10 03, followed by 27 01, 27 02 */
                diagnosticsmode = 0x03;
                getseedindex = 0x01;
            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0B0C)
            {
                /* send 10 03, followed by 27 0B, 27 0C */
                diagnosticsmode = 0x03;
                getseedindex = 0x0B;
            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0304)
            {
                /* send 10 03, followed by 27 03, 27 04 */
                diagnosticsmode = 0x03;
                getseedindex = 0x03;
            }
            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0506)
            {
                /* send 10 03, followed by 27 03, 27 04 */
                diagnosticsmode = 0x03;
                getseedindex = 0x05;
            }


            byte[] TxFrame = new byte[2];
            /* Send start diagnostics mode command */
            TxFrame[0] = 0x10;
            TxFrame[1] = diagnosticsmode;
            int frameLength = 2;

            //Send the parameter ID to the ECU
            Debug.WriteLine("-------Send the parameter ID to the ECU-------");
            responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));

            if (responseArrayStatus.ECUResponseStatus == "NOERROR")
            {
                /* Send get seed command to ECU */
                TxFrame[0] = 0x27;
                TxFrame[1] = getseedindex;
                var tx_Frame_len = 2;
                Debug.WriteLine("-------Send get seed command to ECU-------");
                responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                //var status = pidResponsebytes.ECUResponseStatus;

                if (responseArrayStatus.ECUResponseStatus == "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED")
                {
                    Thread.Sleep(11000);

                    responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
                    //status = pidResponsebytes.ECUResponseStatus;
                }

                if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                {
                    //var datasArrays = pidResponsebytes.ActualDataBytes;
                    //var Rxarray = pidResponsebytes.ActualDataBytes;
                    //var Rxsize = Rxarray.Length;
                    int seedkeyindex = getseedindex;

                    var seedarray = new byte[responseArrayStatus.ActualDataBytes.Length - 2];
                    /* get seed from the response and send it to the seedkey dll to get the key */
                    //for (int i = 0; i < Rxsize - 2; i++)
                    //{
                    //    seedarray[i] = Rxarray[i + 2];
                    //}
                    Array.Copy(responseArrayStatus.ActualDataBytes, 2, seedarray, 0, responseArrayStatus.ActualDataBytes.Length - 2);

                    //status = getkeyfromseed(seedkeyindex, seedsize, seedarray, &keysize, keyarray);

                    byte numkeybytes = new byte();

                    byte[] actualKey = new byte[32];
                    calculateSeedkey = new ECUCalculateSeedkey();
                    Byte seedkeylen = 0;

                    if (seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SML_BOSCH_BS6_PROD)
                    {
                        seedkeylen = 8;
                    }
                    else if ((seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_BS6) ||
                            (seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_AMT_BS6))
                    {
                        seedkeylen = 4;
                    }
                    else if (seedkeyindex1 == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_TCIC)
                    {
                        seedkeylen = 2;
                    }
                    else
                    {
                        seedkeylen = 16;
                    }

                    //SEEDKEYINDEXTYPE index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), pidItem.seedkeyindex);

                    var result = calculateSeedkey.CalculateSeedkey((ECUSeedkeyPCL.SEEDKEYINDEXTYPE)seedkeyindex1, seedkeylen, ref numkeybytes, seedarray, ref actualKey);

                    if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                    {
                        byte[] newTxFrame = new byte[numkeybytes + 2];
                        /* Send calculated key to ECU */
                        newTxFrame[0] = 0x27;
                        newTxFrame[1] = Convert.ToByte(seedkeyindex + 1);

                        Array.Copy(actualKey, 0, newTxFrame, 2, numkeybytes);

                        tx_Frame_len = responseArrayStatus.ActualDataBytes.Length;
                        frameLength = newTxFrame.Length;

                        //Send the parameter ID to the ECU
                        responseArrayStatus = await dongleComm.CAN_TxRx(frameLength, ByteArrayToString(newTxFrame));

                        //var writeParaPID = HexStringToByteArray(command);
                        //var writeparaframesize = command.Length;

                        if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                        {
                            Debug.WriteLine("-------START COMMAND SENDING-------");
                            responseArrayStatus = await this.dongleComm.CAN_TxRx(start_command.Length / 2, start_command);
                        }
                        else
                        {
                            //key issue
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            else
            {
            }
            return responseArrayStatus;
        }

        public async Task<ResponseArrayStatus> RequestIdIOR(string request_command)
        {
            Debug.WriteLine("-------START IOR TEST METHOD-------");
            ResponseArrayStatus responseArrayStatus = new ResponseArrayStatus();


            responseArrayStatus = await this.dongleComm.CAN_TxRx(request_command.Length / 2, request_command);

            return responseArrayStatus;
        }

        public async Task<ResponseArrayStatus> StopIdIOR(string Stop_command)
        {
            Debug.WriteLine("-------STOP IOR TEST METHOD-------");
            ResponseArrayStatus responseArrayStatus = new ResponseArrayStatus();
            Debug.WriteLine($"STOP COMMAND SENDING : {Stop_command}");

            responseArrayStatus = await this.dongleComm.CAN_TxRx(Stop_command.Length / 2, Stop_command);
            //Console.WriteLine($"Timer While Count : {time_base} = {DateTime.Now.ToString("hh:mm:ss:fff")}");
            //responseArrayStatus.ECUResponseStatus = "Running Test";

            if (responseArrayStatus.ECUResponseStatus == "NOERROR")
            {

            }
            else
            {
                test_condition = StopTimer = false;
            }
            return responseArrayStatus;
        }

        #endregion


        //public async Task<ObservableCollection<WriteParameterResponse>> WriteParameters1(int noOFParameters, WriteParameterIndex writeParameterIndex, ObservableCollection<WriteParameterPID> writeParameterCollection, bool IsActuatorTest)
        //{
        //    ObservableCollection<WriteParameterResponse> WriteParameterCollection = new ObservableCollection<WriteParameterResponse>();
        //    foreach (var pidItem in writeParameterCollection)
        //    {
        //        var writeParamIndex = writeParameterIndex;
        //        byte diagnosticsmode = 0x00;
        //        byte getseedindex = 0x00;

        //        if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK090A)
        //        {
        //            /* send 10 03, followed by 27 09, 27 0A */
        //            diagnosticsmode = 0x03;
        //            getseedindex = 0x09;

        //        }
        //        else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0102)
        //        {
        //            /* send 10 03, followed by 27 01, 27 02 */
        //            diagnosticsmode = 0x03;
        //            getseedindex = 0x01;
        //        }
        //        else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0B0C)
        //        {
        //            /* send 10 03, followed by 27 0B, 27 0C */
        //            diagnosticsmode = 0x03;
        //            getseedindex = 0x0B;
        //        }
        //        else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0304)
        //        {
        //            /* send 10 03, followed by 27 03, 27 04 */
        //            diagnosticsmode = 0x03;
        //            getseedindex = 0x03;
        //        }
        //        else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0506)
        //        {
        //            /* send 10 03, followed by 27 03, 27 04 */
        //            diagnosticsmode = 0x03;
        //            getseedindex = 0x05;
        //        }


        //        byte[] TxFrame = new byte[2];
        //        /* Send start diagnostics mode command */
        //        TxFrame[0] = 0x10;
        //        TxFrame[1] = diagnosticsmode;
        //        int frameLength = 2;

        //        //Send the parameter ID to the ECU
        //        var pidResponse = await dongleCommWin.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
        //        string return_status = string.Empty;

        //        if (pidResponse.ECUResponseStatus == "NOERROR")
        //        {
        //            /* Send get seed command to ECU */
        //            TxFrame[0] = 0x27;
        //            TxFrame[1] = getseedindex;
        //            var tx_Frame_len = 2;
        //            var pidResponsebytes = await dongleCommWin.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
        //            var status = pidResponsebytes.ECUResponseStatus;

        //            if (status == "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED")
        //            {
        //                Thread.Sleep(11000);

        //                pidResponsebytes = await dongleCommWin.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
        //                status = pidResponsebytes.ECUResponseStatus;
        //            }

        //            if (status == "NOERROR")
        //            {
        //                var datasArrays = pidResponsebytes.ActualDataBytes;
        //                var Rxarray = pidResponsebytes.ActualDataBytes;
        //                var Rxsize = Rxarray.Length;
        //                int seedkeyindex = getseedindex;

        //                var seedarray = new byte[Rxsize - 2];
        //                /* get seed from the response and send it to the seedkey dll to get the key */
        //                //for (int i = 0; i < Rxsize - 2; i++)
        //                //{
        //                //    seedarray[i] = Rxarray[i + 2];
        //                //}
        //                Array.Copy(Rxarray, 2, seedarray, 0, Rxsize - 2);

        //                //status = getkeyfromseed(seedkeyindex, seedsize, seedarray, &keysize, keyarray);

        //                byte numkeybytes = new byte();

        //                byte[] actualKey = new byte[32];
        //                calculateSeedkey = new ECUCalculateSeedkey();
        //                Byte seedkeylen = 0;

        //                if (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SML_BOSCH_BS6_PROD)
        //                {
        //                    seedkeylen = 8;
        //                }
        //                else if ((pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_BS6) ||
        //                        (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_AMT_BS6))
        //                {
        //                    seedkeylen = 4;
        //                }
        //                else if (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.SEEDKEYALGO_VECV_DELPHI_TCIC)
        //                {
        //                    seedkeylen = 2;
        //                }
        //                else
        //                {
        //                    seedkeylen = 16;
        //                }

        //                //SEEDKEYINDEXTYPE index = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), pidItem.seedkeyindex);

        //                var result = calculateSeedkey.CalculateSeedkey((ECUSeedkeyPCL.SEEDKEYINDEXTYPE)pidItem.seedkeyindex, seedkeylen, ref numkeybytes, seedarray, ref actualKey);

        //                if (status == "NOERROR")
        //                {
        //                    byte[] newTxFrame = new byte[numkeybytes + 2];
        //                    /* Send calculated key to ECU */
        //                    newTxFrame[0] = 0x27;
        //                    newTxFrame[1] = Convert.ToByte(seedkeyindex + 1);

        //                    Array.Copy(actualKey, 0, newTxFrame, 2, numkeybytes);

        //                    tx_Frame_len = Rxsize;
        //                    frameLength = newTxFrame.Length;

        //                    //Send the parameter ID to the ECU
        //                    var pidResponse3 = await dongleCommWin.CAN_TxRx(frameLength, ByteArrayToString(newTxFrame));

        //                    byte[] writeParaPID = null;

        //                    if (pidResponse3.ECUResponseStatus == "NOERROR")
        //                    {
        //                        responseBytes = await this.dongleCommWin.CAN_TxRx(command.Length / 2, command);
        //                        return responseBytes;
        //                    }
        //                    else
        //                    {
        //                        //key issue
        //                        return_status = status;
        //                        var dataItem = new WriteParameterResponse
        //                        {
        //                            Status = pidResponse3.ECUResponseStatus,
        //                            DataArray = pidResponse3.ActualDataBytes,
        //                            pidName = pidItem.writeparapid,
        //                            pidNumber = pidItem.writeparano,

        //                            responseValue = pidResponse3.ECUResponseStatus

        //                        };
        //                        WriteParameterCollection.Add(dataItem);
        //                    }
        //                }
        //                else
        //                {
        //                    return_status = status;
        //                    var dataItem = new WriteParameterResponse
        //                    {
        //                        Status = pidResponse.ECUResponseStatus,
        //                        DataArray = pidResponse.ActualDataBytes,
        //                        pidName = pidItem.writeparapid,
        //                        pidNumber = pidItem.writeparano,

        //                        responseValue = pidResponse.ECUResponseStatus

        //                    };
        //                    WriteParameterCollection.Add(dataItem);
        //                }
        //            }
        //            else
        //            {
        //                return_status = status;
        //                var dataItem = new WriteParameterResponse
        //                {
        //                    Status = pidResponsebytes.ECUResponseStatus,
        //                    DataArray = pidResponsebytes.ActualDataBytes,
        //                    pidName = pidItem.writeparapid,
        //                    pidNumber = pidItem.writeparano,

        //                    responseValue = pidResponsebytes.ECUResponseStatus

        //                };
        //                WriteParameterCollection.Add(dataItem);
        //            }
        //        }
        //        else
        //        {
        //            return_status = pidResponse.ECUResponseStatus;
        //            var dataItem = new WriteParameterResponse
        //            {
        //                Status = pidResponse.ECUResponseStatus,
        //                DataArray = pidResponse.ActualDataBytes,
        //                pidName = pidItem.writeparapid,
        //                pidNumber = pidItem.writeparano,

        //                responseValue = pidResponse.ECUResponseStatus

        //            };
        //            WriteParameterCollection.Add(dataItem);

        //        }
        //    }
        //    return WriteParameterCollection;

        //}

        #region Test Routine
        public async Task<object> TestRoutine(string command)
        {
            try
            {

                object responseBytes = null;
                //byte[] writeFrame = new byte[command.Length/2];
                //writeFrame = HexStringToByteArray(command);
                //var frameLength = 1;

                responseBytes = await this.dongleComm.CAN_TxRx(command.Length / 2, command);
                return responseBytes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion


        public async Task<string> StartEcuUnlocking(string tx_frame, string tx_frequency, string tx_totalTime)
        {
            try
            {
                int Frame_len = 0;
                Frame_len = tx_frame.Length/2;
                //byte[] TxFrame = new byte[2];
                ResponseArrayStatus response = new ResponseArrayStatus();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Device.StartTimer(TimeSpan.FromMilliseconds(int.Parse(tx_frequency)), () =>
                {
                    if (stopwatch.ElapsedMilliseconds < int.Parse(tx_totalTime))
                    {
                        Task.Run(async () =>
                        {
                            var response = await dongleComm.CAN_TxRx(Frame_len, tx_frame);
                        });

                        return true;
                    }
                    else
                        return false;

                });


                return response.ToString();
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        #region StartFlashBoschBS6
        uint totalbytestobeflashed;
        uint realtimebytesflashed;

        public async Task<float> GetRuntimeFlashPercent()
        {
            float runtimeflashpercent = (float)realtimebytesflashed / (float)totalbytestobeflashed;
            return runtimeflashpercent;
        }

        List<LoopModel> loopModelList = new List<LoopModel>();
        public async Task<string> FlashInterpreter(flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata, string interpreterFile)
        {
            ResponseArrayStatus reprogrammingResponse = new ResponseArrayStatus();
            try
            {
                string[] lineData = interpreterFile.Split('\n');

                /* Calculate the total number of bytes to be flashed in total and then calculated runtime flashing % */
                for (int i = 0; i < noofsectors; i++)
                {
                    var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;
                    totalbytestobeflashed += sector_num_bytes;
                    realtimebytesflashed = 0;
                }

                byte[] seedKey = new byte[flashconfig_data.seedkeynumbytes];

                for (int i = 0; i < lineData.Length; i++)
                {
                    string formattedLine = lineData[i].Replace("\r", string.Empty).Trim();

                    if (formattedLine.StartsWith("//") || formattedLine == "")
                    {
                        continue;
                    }

                    string command = formattedLine.Split(':')[0];
                    string data = formattedLine.Split(':')[1];

                    if (command == "send")
                    {
                        var splitData = data.Split('+');
                        int initialLength = HexStringToByteArray(splitData[0]).Length;
                        byte[] TxFrame = new byte[initialLength];

                        foreach (var item in splitData)
                        {
                            if (!item.Contains("<"))
                            {
                                var byteArray = HexStringToByteArray(item);
                                Array.Copy(byteArray, TxFrame, byteArray.Length);
                            }
                            else
                            {
                                int endIndex = item.IndexOf('>');

                                string bracketString = item.Substring(1, endIndex - 1);

                                string reference = bracketString.Split(',')[0];
                                int copyLength = Convert.ToInt32(bracketString.Split(',')[1]);

                                byte[] copyArray = new byte[] { };

                                switch (reference)
                                {
                                    case "key":
                                        copyArray = seedKey;
                                        break;
                                    case "json_strt_addr[i]":
                                        var sector_start_address = HexStringToByteArray(sectordata[loopModelList.Last().i].JsonStartAddress.PadLeft(copyLength * 2, '0'));
                                        copyArray = sector_start_address;
                                        break;
                                    case "json_sector_len[i]":
                                        var sector_num_bytes = Convert.ToUInt32(sectordata[loopModelList.Last().i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[loopModelList.Last().i].JsonStartAddress, 16) + 1;
                                        string sector_num_bytes_hex = sector_num_bytes.ToString("X");
                                        string formatted_sector_num_bytes = sector_num_bytes_hex.PadLeft(copyLength * 2, '0');
                                        var sectorLength = HexStringToByteArray(formatted_sector_num_bytes);
                                        copyArray = sectorLength;
                                        break;
                                    case "i":
                                        copyArray = new byte[1];
                                        copyArray[0] = (byte)loopModelList.Last().i;
                                        break;
                                    case "i^1":
                                        copyArray = new byte[1];
                                        copyArray[0] = (byte)(loopModelList.Last().i + 1);
                                        break;
                                }
                                Array.Resize(ref copyArray, copyLength);
                                int startCopyFrom = TxFrame.Length;
                                Array.Resize(ref TxFrame, TxFrame.Length + copyLength);
                                Array.Copy(copyArray, 0, TxFrame, startCopyFrom, copyLength);
                            }
                        }
                        Debug.WriteLine($"Sending Command {ByteArrayToString(TxFrame)}", "send : ");
                        reprogrammingResponse = await dongleComm.CAN_TxRx(TxFrame.Length, ByteArrayToString(TxFrame));
                        if (reprogrammingResponse?.ECUResponseStatus != "NOERROR")
                        {
                            Debug.WriteLine("--------- reprogrammingResponse ERROR------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
                            Debug.WriteLine("---------reprogrammingResponse LOOP END------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
                            return reprogrammingResponse.ECUResponseStatus;
                        }
                    }
                    else if (command == "sleep")
                    {
                        await Task.Delay(Convert.ToInt32(data));
                    }
                    //else if (command == "copy")
                    //{
                    //    string indexString = data.Substring(data.IndexOf('[') + 1, data.IndexOf(']') - data.IndexOf('[') - 1);

                    //    int index1 = indexString[0] - '0';
                    //    int index2 = indexString[indexString.Length - 1] - '0';

                    //    seedArray = new byte[index2 - index1 + 1];

                    //    Array.Copy(reprogrammingResponse.ActualDataBytes, index1, seedArray, 0, index2 - index1 + 1);
                    //    Debug.WriteLine($"-------seed array = {ByteArrayToString(seedArray)}-------");
                    //}
                    else if (command == "function")
                    {
                        if (data == "CalculateKeyFromSeed")
                        {
                            var seedarray = new byte[flashconfig_data.seedkeynumbytes];
                            byte numkeybytes = new byte();

                            Array.Copy(reprogrammingResponse.ActualDataBytes, 2, seedarray, 0, reprogrammingResponse.ActualDataBytes.Length - 2);
                            Debug.WriteLine($"-------seed array = {ByteArrayToString(seedarray)}-------");
                            calculateSeedkey = new ECUCalculateSeedkey();
                            Debug.WriteLine("-------get key-------");

                            var result = calculateSeedkey.CalculateSeedkey((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, ref numkeybytes, seedarray, ref seedKey);
                            Debug.WriteLine($"-------get key response = {ByteArrayToString(seedKey)}-------");
                        }
                    }
                    else if (command == "repeatstart")
                    {
                        var splitData = data.Split(',');
                        int maxIndex = splitData[2] == "noofsectors" ? noofsectors : int.Parse(splitData[2]);

                        loopModelList.Add(new LoopModel
                        {
                            i = 0,
                            loopId = int.Parse(splitData[0]),
                            maxIndex = maxIndex,
                            loopLocation = i
                        });
                    }
                    else if (command == "repeatend")
                    {
                        loopModelList.Last().i++;
                        if (loopModelList.Last().i == loopModelList.Last().maxIndex)
                        {
                            loopModelList.Remove(loopModelList.Last());
                        }
                        else
                        {
                            i = loopModelList.Last().loopLocation;
                        }
                    }
                    else if (command == "sendbulkdata")
                    {
                        int blkseqcnt = 1;
                        int Frame_len;
                        var SectorDataArray = HexStringToByteArray(sectordata[loopModelList.Last().i].JsonData);
                        var sector_num_bytes = Convert.ToUInt32(sectordata[loopModelList.Last().i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[loopModelList.Last().i].JsonStartAddress, 16) + 1;
                        for (int j = 0; j < sector_num_bytes;)
                        {
                            try
                            {
                                var NTxFrame = new byte[2];
                                //NTxFrame[0] = 0x36;
                                NTxFrame[0] = 0x36;
                                NTxFrame[1] = (byte)(blkseqcnt & 0xFF);
                                var currenttransferlen = Math.Min(sector_num_bytes - j, flashconfig_data.sectorframetransferlen);
                                //var currenttransferlen = Math.Min(sector_num_bytes - j, 252);

                                Debug.WriteLine("currenttransferlen==" + currenttransferlen + 2, "ELMZ");

                                Array.Resize(ref NTxFrame, Convert.ToInt32(currenttransferlen) + 2);

                                Array.Copy(SectorDataArray, j, NTxFrame, 2, currenttransferlen);
                                j += Convert.ToInt32(currenttransferlen);
                                Debug.WriteLine("J==" + j, "ELMZ");

                                Frame_len = Convert.ToInt32(currenttransferlen + 2);

                                var frame = ByteArrayToString(NTxFrame);
                                var bulkTransferResponse = await dongleComm.CAN_TxRx(Frame_len, frame);
                                blkseqcnt++;

                                if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
                                {
                                    Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                                    Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                                    return bulkTransferResponse.ECUResponseStatus;

                                }

                                realtimebytesflashed += (UInt32)currenttransferlen;

                                Debug.WriteLine("blkseqcnt==" + blkseqcnt, "ELMZ");
                            }
                            catch (Exception exception)
                            {
                                Debug.WriteLine("exception==" + exception.Message, "ELMZ");
                                var msg = exception;
                                return "exception";
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("exception==" + ex.Message, "ELMZ");
                var msg = ex;
                return "exception";
            }
            return reprogrammingResponse.ECUResponseStatus;
        }

        public async Task<string> StartFlashBoschBS6(flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata)
        {

            try
            {
                float runtimeflashpercent = 0;
                // switch to reprogramming mode - 10 02
                int Frame_len = 0;
                byte[] TxFrame = new byte[2];
                TxFrame[Frame_len++] = 0x10;
                TxFrame[Frame_len++] = 0x02;
                byte addressdataformat = 0x00;

                Debug.WriteLine("-------switch to reprogramming mode-------");
                var reprogrammingResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                if (reprogrammingResponse?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("--------- reprogrammingResponse ERROR------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
                    Debug.WriteLine("---------reprogrammingResponse LOOP END------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
                    return reprogrammingResponse.ECUResponseStatus;
                }

                // get Seed - 27 09
                Frame_len = 0;
                TxFrame[Frame_len++] = 0x27;
                TxFrame[Frame_len++] = flashconfig_data.sendseedbyte;

                Debug.WriteLine("-------get Seed-------");

                var getSeedResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                if (getSeedResponse?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("--------- getSeedResponse ERROR------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
                    Debug.WriteLine("---------getSeedResponse LOOP END------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
                    return getSeedResponse.ECUResponseStatus;
                }

                var seedarray = new byte[flashconfig_data.seedkeynumbytes];
                byte numkeybytes = new byte();

                Array.Copy(getSeedResponse.ActualDataBytes, 2, seedarray, 0, getSeedResponse.ActualDataBytes.Length - 2);

                //compute key for the seed received
                //status = getkeyfromseed(flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, Rxarray + 2, keyarray);

                byte[] actualKey = new byte[flashconfig_data.seedkeynumbytes];
                calculateSeedkey = new ECUCalculateSeedkey();
                Debug.WriteLine("-------get key-------");

                var result = calculateSeedkey.CalculateSeedkey((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, ref numkeybytes, seedarray, ref actualKey);

                Array.Resize(ref TxFrame, flashconfig_data.seedkeynumbytes + 2);//
                                                                                //send key 27 0A
                Frame_len = 0;
                TxFrame[Frame_len++] = 0x27;
                TxFrame[Frame_len++] = (byte)(flashconfig_data.sendseedbyte + 1);
                for (int i = 0; i < actualKey.Length; i++)
                {
                    TxFrame[Frame_len++] = actualKey[i];
                }
                //status = ISO15765_CANTxRx(channel, Frame_len, TxFrame, &Rxsize, Rxarray);
                var sendKeyResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                if (sendKeyResponse?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("--------- sendKeyResponse ERROR------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
                    Debug.WriteLine("---------sendKeyResponse LOOP END------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
                    return sendKeyResponse.ECUResponseStatus;
                }


                // Erase Memory
                if (flashconfig_data.erasesector == EraseSectorEnum.ERASEALLATONCE)
                {
                    Frame_len = 0;
                    TxFrame[Frame_len++] = 0x31;
                    TxFrame[Frame_len++] = 0x01;
                    TxFrame[Frame_len++] = 0xFF;
                    TxFrame[Frame_len++] = 0x00;

                    Array.Resize(ref TxFrame, 12);
                    // advantek erase memory map hard coded here...
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x02;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;

                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x0F;
                    TxFrame[Frame_len++] = 0xFF;
                    TxFrame[Frame_len++] = 0xFF;


                    var eraseMemoryResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                    if (eraseMemoryResponse?.ECUResponseStatus != "NOERROR")
                    {
                        Debug.WriteLine("--------- eraseMemoryResponse ERROR------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
                        Debug.WriteLine("---------eraseMemoryResponse LOOP END------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
                        return eraseMemoryResponse.ECUResponseStatus;
                    }
                }

                //*
                /* Calculate the total number of bytes to be flashed in total and then calculated runtime flashing % */
                for (int i = 0; i < noofsectors; i++)
                {
                    var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;
                    totalbytestobeflashed += sector_num_bytes;
                    realtimebytesflashed = 0;
                }


                var addrdataformat = flashconfig_data.addrdataformat;
                for (int i = 0; i < noofsectors; i++)
                {
                    var sector_start_address = Convert.ToUInt32(sectordata[i].JsonStartAddress, 16);
                    var sector_end_address = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16);
                    var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;

                    var ecu_memmap_start_address = Convert.ToUInt32(sectordata[i].ECUMemMapStartAddress, 16);
                    var ecu_memmap_end_address = Convert.ToUInt32(sectordata[i].ECUMemMapEndAddress, 16);


                    if (flashconfig_data.erasesector == EraseSectorEnum.ERASEBYSECTOR)
                    {
                        // Erase Memory
                        Frame_len = 0;
                        TxFrame[Frame_len++] = 0x31;
                        TxFrame[Frame_len++] = 0x01;
                        TxFrame[Frame_len++] = 0xFF;
                        TxFrame[Frame_len++] = 0x00;
                        TxFrame[Frame_len++] = 0x01;
                        TxFrame[Frame_len++] = (byte)i;
                        Array.Resize(ref TxFrame, 6);
                        var response2 = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                        if (response2?.ECUResponseStatus != "NOERROR")
                        {
                            Debug.WriteLine("--------- response2 ERROR------------==" + response2.ECUResponseStatus, "ELMZ");
                            Debug.WriteLine("---------response2 LOOP END------------==" + response2.ECUResponseStatus, "ELMZ");
                            return response2.ECUResponseStatus;
                        }
                    }


                    // Request download
                    Frame_len = 0;
                    TxFrame[Frame_len++] = 0x34;
                    TxFrame[Frame_len++] = 0x00;

                    if (addrdataformat == "33_00FFFFFF")
                    {
                        Array.Resize(ref TxFrame, 9);
                        TxFrame[Frame_len++] = addressdataformat = 0x33;
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

                    }
                    else if (addrdataformat == "33_FF00FFFF")
                    {
                        Array.Resize(ref TxFrame, 9);
                        TxFrame[Frame_len++] = addressdataformat = 0x33;
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0x0000FF00) >> 8);
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

                    }

                    else if (addrdataformat == "44") // addrdataformat == 0x44
                    {
                        Array.Resize(ref TxFrame, 11);
                        TxFrame[Frame_len++] = addressdataformat = 0x44;
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
                        TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0xFF000000) >> 24);
                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
                        TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

                    }


                    var memoryResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                    if (memoryResponse?.ECUResponseStatus != "NOERROR")
                    {
                        Debug.WriteLine("--------- memoryResponse ERROR------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
                        Debug.WriteLine("---------memoryResponse LOOP END------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
                        return memoryResponse.ECUResponseStatus;
                    }

                    // Transfer Data in this sector
                    int blkseqcnt = 1;
                    var SectorDataArray = HexStringToByteArray(sectordata[i].JsonData);
                    for (int j = 0; j < sector_num_bytes;)
                    {
                        try
                        {
                            var NTxFrame = new byte[2];
                            NTxFrame[0] = 0x36;
                            NTxFrame[1] = (byte)(blkseqcnt & 0xFF);
                            var currenttransferlen = Math.Min(sector_num_bytes - j, flashconfig_data.sectorframetransferlen);
                            //var currenttransferlen = Math.Min(sector_num_bytes - j, 252);

                            Debug.WriteLine("currenttransferlen==" + currenttransferlen + 2, "ELMZ");

                            Array.Resize(ref NTxFrame, Convert.ToInt32(currenttransferlen) + 2);

                            Array.Copy(SectorDataArray, j, NTxFrame, 2, currenttransferlen);
                            j += Convert.ToInt32(currenttransferlen);
                            Debug.WriteLine("J==" + j, "ELMZ");

                            Frame_len = Convert.ToInt32(currenttransferlen + 2);

                            var frame = ByteArrayToString(NTxFrame);
                            var bulkTransferResponse = await dongleComm.CAN_TxRx(Frame_len, frame);
                            blkseqcnt++;

                            if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
                            {
                                Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                                Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                                return bulkTransferResponse.ECUResponseStatus;

                            }
                            //*
                            realtimebytesflashed += (UInt32)currenttransferlen;

                            Debug.WriteLine("blkseqcnt==" + blkseqcnt, "ELMZ");
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine("exception==" + exception.Message, "ELMZ");
                            var msg = exception;
                            //return "exception";
                        }
                    }

                    // Transfer Exit
                    var BTxFrame = new byte[1];
                    BTxFrame[0] = 0x37;
                    Frame_len = 1;
                    var TransferResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(BTxFrame));
                    if (TransferResponse?.ECUResponseStatus != "NOERROR")
                    {
                        Debug.WriteLine("--------TransferResponse -ERROR------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
                        Debug.WriteLine("--------TransferResponse -LOOP END------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
                        return TransferResponse.ECUResponseStatus;
                    }

                    //Checksum test
                    var cTxFrame = new byte[6];
                    Frame_len = 0;
                    cTxFrame[Frame_len++] = 0x31;
                    cTxFrame[Frame_len++] = 0x01;
                    cTxFrame[Frame_len++] = 0x02;
                    cTxFrame[Frame_len++] = 0x02;
                    cTxFrame[Frame_len++] = 0x01;
                    cTxFrame[Frame_len++] = (byte)i;

                    var sendsectorchksumResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(cTxFrame));
                    if (TransferResponse?.ECUResponseStatus != "NOERROR")
                    {
                        Debug.WriteLine("--------sendsectorchksumResponse -ERROR------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
                        Debug.WriteLine("--------sendsectorchksumResponse -LOOP END------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
                        return sendsectorchksumResponse.ECUResponseStatus;
                    }

                } // end of all sectors

                string return_status = string.Empty;
                TxFrame = new byte[2];

                Frame_len = 0;
                TxFrame[Frame_len++] = 0x11;
                TxFrame[Frame_len++] = 0x01;

                var resetResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                if (resetResponse?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("--------resetResponse -ERROR------------==" + resetResponse.ECUResponseStatus, "ELMZ");
                    Debug.WriteLine("--------resetResponse -LOOP END------------==" + resetResponse.ECUResponseStatus, "ELMZ");
                    return resetResponse.ECUResponseStatus;
                }
                return_status = resetResponse.ECUResponseStatus;
                return (return_status);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        #endregion

        #region BIN FILE Flashing(BMS)
        public async Task<string> StartFlashBINfile(flashconfig flashconfig_data, int noofsectors, string data, int protocol)
        {
            object can_res = null;
            string return_status = "NOERROR";
            APDongleCommAnroid.Models.ResponseArrayStatus response = null;
            try
            {
                //can_res = await dongleComm.Dongle_SetProtocol(protocol);
                //can_res = await dongleComm.CAN_SetTxHeader("0803");
                //can_res = await dongleComm.CAN_SetRxHeaderMask("0801");
                //await Task.Delay(200);

                byte[] TxFrame = new byte[2];
                //UInt16 TxFrame[132];
                FlashCurtisModel sectordata = JsonConvert.DeserializeObject<FlashCurtisModel>(data);
                byte packetNumber = 0x01;
                byte[] binFile = HexStringToByteArray(sectordata.SectorData[0].JsonData); // this is the data part from json
                int totalSizeFlashed = 0;
                int TotalFileSize = HexStringToByteArray(sectordata.SectorData[0].JsonData).Length; ;
                int Frame_len = 0;// Get this from the json
                UInt16 crc = 0x00;
                byte[] refer_frame = new byte[128];
                int refer_index = 128;

                TxFrame[0] = (byte)'B';
                TxFrame[1] = (byte)'L';

                for (int i = 0; i < 3; i++)
                {
                    response = await dongleComm.CAN_TxRx(2, ByteArrayToString(TxFrame));
                }

                TxFrame = new byte[133];

                while (totalSizeFlashed < TotalFileSize)
                {
                    // Initialize TxFrame with 0x00;
                    //
                    TxFrame[0] = 0x01;
                    TxFrame[1] = packetNumber;
                    TxFrame[2] = (byte)~packetNumber;

                    Debug.WriteLine("@@@---" + totalbytestobeflashed + ", " + Math.Min(128, TotalFileSize - totalSizeFlashed), "ELMZ");
                    Array.Copy(binFile, totalSizeFlashed, TxFrame, 3, Math.Min(128, TotalFileSize - totalSizeFlashed));
                    Array.Copy(binFile, totalSizeFlashed, refer_frame, 0, Math.Min(128, TotalFileSize - totalSizeFlashed));
                    //calculate CRC
                    UInt32 chksum = 0;
                    //for (int i = 0; i < 131; i++)
                    //{
                    //    chksum += TxFrame[i];
                    //}

                    //TxFrame[131] = (byte)(chksum >> 8);
                    //TxFrame[132] = (byte)chksum;

                    crc = CalculateCRC(refer_frame);

                    //byte[] numberBytes = BitConverter.GetBytes(crc);

                    TxFrame[131] = (byte)(crc >> 8);
                    TxFrame[132] = (byte)crc;

                    totalSizeFlashed += 128;

                    //Frame_len++;
                    packetNumber++;

                    response = await dongleComm.CAN_TxRx(133, ByteArrayToString(TxFrame));
                    if (response?.ECUResponseStatus != "NOERROR")
                    {
                        Debug.WriteLine("--------resetResponse -ERROR------------==" + response.ECUResponseStatus, "ELMZ");
                        Debug.WriteLine("--------resetResponse -LOOP END------------==" + response.ECUResponseStatus, "ELMZ");
                        return response.ECUResponseStatus;
                    }


                }

                Array.Resize(ref TxFrame, 1);
                Frame_len = 1;
                TxFrame[0] = 0x04; // End of File
                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                if (response?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("--------resetResponse -ERROR------------==" + response.ECUResponseStatus, "ELMZ");
                    Debug.WriteLine("--------resetResponse -LOOP END------------==" + response.ECUResponseStatus, "ELMZ");
                    return response.ECUResponseStatus;
                }
                return (return_status);
            }
            catch (Exception ex)
            {
                return ex.Message + ex.StackTrace;
            }


        }

        UInt16 CalculateCRC(byte[] txFrame)
        {
            UInt16 poly = 0x1021;
            UInt16[] table = new UInt16[256];
            UInt16 initialValue = 0x0;
            UInt16 temp, a;
            UInt16 crc = initialValue;

            for (int i = 0; i < 256; ++i)
            {
                temp = 0;
                a = (UInt16)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                        temp = (UInt16)((temp << 1) ^ poly);
                    else
                        temp <<= 1;
                    a <<= 1;
                }
                table[i] = temp;
            }

            for (int i = 0; i < 128; ++i)
            {
                crc = (UInt16)((crc << 8) ^ table[((crc >> 8) ^ (0xff & txFrame[i]))]);
            }
            return crc;
            //printf("crc is %x", crc);
        }
        #endregion

        public async Task<string> StartFlashCurtis125KBPS(string data, string txHeader, string rxHeader, int protocol)
        {
            object can_res = null;

            try
            {
                var setProtocol = await dongleComm.Dongle_SetProtocol((int)0x14);

                await Task.Delay(200);

                APDongleCommAnroid.Models.ResponseArrayStatus response = null;
                byte[] TxFrame = new byte[8];

                int Frame_len = 8;
                FlashCurtisModel flashCurtisModel = JsonConvert.DeserializeObject<FlashCurtisModel>(data);
                TxFrame[0] = 0x23;
                TxFrame[1] = 0x29;
                TxFrame[2] = 0x33;
                TxFrame[3] = 0x00;
                TxFrame[4] = 0x1B;
                TxFrame[5] = 0x07;
                TxFrame[6] = 0x13;
                TxFrame[7] = 0x47;



                Frame_len = 8;
                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));



                setProtocol = await dongleComm.Dongle_SetProtocol((int)0x12);
                await Task.Delay(2000);

                can_res = await dongleComm.CAN_SetP2Max("2710");

                can_res = await dongleComm.CAN_SetP1Min("64");

                for (int i = 0; i < flashCurtisModel.NoOfSectors; i++)
                {
                    var sector_len = Convert.ToUInt32(flashCurtisModel.SectorData[i].SectorLength, 16);

                    Frame_len = 0;
                    TxFrame[Frame_len++] = 0xC0;
                    TxFrame[Frame_len++] = 0x29;
                    TxFrame[Frame_len++] = 0x33;
                    TxFrame[Frame_len++] = 0x00;

                    // pass length of sector
                    TxFrame[Frame_len++] = (byte)sector_len;
                    TxFrame[Frame_len++] = (byte)(sector_len >> 8);
                    TxFrame[Frame_len++] = (byte)(sector_len >> 16);
                    TxFrame[Frame_len++] = (byte)(sector_len >> 24);
                    Frame_len = 8;
                    response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                    var SectorDataArray = HexStringToByteArray(flashCurtisModel.SectorData[i].JsonData);

                    for (int j = 0; j < sector_len;)
                    {
                        var currenttransferlen = Math.Min(sector_len - j, 0x379);

                        byte[] NTxFrame = new byte[currenttransferlen];
                        Array.Copy(SectorDataArray, j, NTxFrame, 0, currenttransferlen);
                        j += Convert.ToInt32(currenttransferlen);
                        //Debug.WriteLine("J==" + j, "ELMZ");

                        Frame_len = Convert.ToInt32(currenttransferlen);

                        var frame = ByteArrayToString(NTxFrame);
                        var bulkTransferResponse = await dongleComm.CAN_TxRx(frame.Length / 2, frame);

                        if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
                        {
                            Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                            Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                            //await dongleComm.Dongle_SetProtocol((int)0x14);
                            //await Task.Delay(2000);
                            await dongleComm.CAN_SetP2Max("0000");
                            return bulkTransferResponse.ECUResponseStatus;
                        }

                    }

                    Frame_len = 0;
                    TxFrame[Frame_len++] = 0xC1;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    Frame_len = 8;
                    response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                }


                can_res = await dongleComm.CAN_SetTxHeader("0000");
                can_res = await dongleComm.CAN_SetRxHeaderMask("00A6");

                can_res = await dongleComm.CAN_StopPadding();

                Array.Resize(ref TxFrame, 2);
                Frame_len = 2;
                TxFrame[0] = 0x81;
                TxFrame[1] = 0x26;
                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                setProtocol = await dongleComm.Dongle_SetProtocol((int)0x14);
                //await Task.Delay(2000);

                await Task.Delay(1500);
                can_res = await dongleComm.CAN_ClearSocket();
                //await Task.Delay(500);

                can_res = await dongleComm.CAN_StartPadding("00");
                //await Task.Delay(200);
                can_res = await dongleComm.CAN_SetTxHeader(txHeader);
                //await Task.Delay(200);

                can_res = await dongleComm.CAN_SetRxHeaderMask(rxHeader);
                //await Task.Delay(200);


                Array.Resize(ref TxFrame, 8);
                Frame_len = 0;
                //TxFrame[Frame_len++] = 0x06;
                //TxFrame[Frame_len++] = 0x26;
                // Array.Resize(ref TxFrame, 8);

                TxFrame[Frame_len++] = 0x22;
                TxFrame[Frame_len++] = 0x2F;
                TxFrame[Frame_len++] = 0x33;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x01;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;

                //Frame_len = 10;

                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                Frame_len = 0;
                //TxFrame[Frame_len++] = 0x06;
                //TxFrame[Frame_len++] = 0x26;
                TxFrame[Frame_len++] = 0x22;
                TxFrame[Frame_len++] = 0x80;
                TxFrame[Frame_len++] = 0x31;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;

                //Frame_len = 10;

                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                //await Task.Delay(200);
                // Send Reset Command
                can_res = await dongleComm.CAN_SetTxHeader("0000");
                //await Task.Delay(200);
                can_res = await dongleComm.CAN_SetRxHeaderMask("00A6");

                can_res = await dongleComm.CAN_StopPadding();

                Array.Resize(ref TxFrame, 2);
                Frame_len = 2;
                TxFrame[0] = 0x81;
                TxFrame[1] = 0x26;
                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                await Task.Delay(1500);
                can_res = await dongleComm.CAN_ClearSocket();
                //await Task.Delay(500);


                return response.ECUResponseStatus;
            }
            catch (Exception ex)
            {
                //await dongleComm.Dongle_SetProtocol((int)0x14);
                //await Task.Delay(2000);
                //await dongleComm.CAN_SetP2Max("0000");
                return null;
            }
            finally
            {
                can_res = await dongleComm.CAN_StartPadding("00");
                //await Task.Delay(200);
                can_res = await dongleComm.CAN_SetTxHeader(txHeader);
                //await Task.Delay(200);

                can_res = await dongleComm.CAN_SetRxHeaderMask(rxHeader);
                //await Task.Delay(200);

                can_res = await dongleComm.CAN_SetP2Max("0000");
                //await Task.Delay(200);

                can_res = await dongleComm.Dongle_SetProtocol(protocol);
            }
        }

        public async Task<string> StartFlashCurtis500KBPS(string data, string txHeader, string rxHeader, int protocol)
        {
            object can_res = null;

            try
            {
                var setProtocol = await dongleComm.Dongle_SetProtocol((int)0x14);

                can_res = await dongleComm.CAN_SetP2Max("2710");//2 seconds delay

                can_res = await dongleComm.CAN_SetP1Min("64");

                await Task.Delay(200);

                APDongleCommAnroid.Models.ResponseArrayStatus response = null;
                byte[] TxFrame = new byte[8];

                int Frame_len = 8;
                FlashCurtisModel flashCurtisModel = JsonConvert.DeserializeObject<FlashCurtisModel>(data);
                TxFrame[0] = 0x23;
                TxFrame[1] = 0x29;
                TxFrame[2] = 0x33;
                TxFrame[3] = 0x00;
                TxFrame[4] = 0x1B;
                TxFrame[5] = 0x07;
                TxFrame[6] = 0x13;
                TxFrame[7] = 0x47;



                Frame_len = 8;
                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                if (response?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("-------- ERROR ------------==" + response.ECUResponseStatus, "ELMZ");
                    return response.ECUResponseStatus;
                }

                //setProtocol = await dongleComm.Dongle_SetProtocol((int)0x12);
                //await Task.Delay(2000);



                for (int i = 0; i < flashCurtisModel.NoOfSectors; i++)
                {
                    var sector_len = Convert.ToUInt32(flashCurtisModel.SectorData[i].SectorLength, 16);

                    Frame_len = 0;
                    TxFrame[Frame_len++] = 0xC0;
                    TxFrame[Frame_len++] = 0x29;
                    TxFrame[Frame_len++] = 0x33;
                    TxFrame[Frame_len++] = 0x00;

                    // pass length of sector
                    TxFrame[Frame_len++] = (byte)sector_len;
                    TxFrame[Frame_len++] = (byte)(sector_len >> 8);
                    TxFrame[Frame_len++] = (byte)(sector_len >> 16);
                    TxFrame[Frame_len++] = (byte)(sector_len >> 24);
                    Frame_len = 8;
                    response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                    if (response?.ECUResponseStatus != "NOERROR")
                    {
                        Debug.WriteLine("-------- ERROR ------------==" + response.ECUResponseStatus, "ELMZ");
                        await dongleComm.CAN_SetP2Max("0000");
                        return response.ECUResponseStatus;
                    }
                    var SectorDataArray = HexStringToByteArray(flashCurtisModel.SectorData[i].JsonData);

                    for (int j = 0; j < sector_len;)
                    {
                        var currenttransferlen = Math.Min(sector_len - j, 0x379);

                        byte[] NTxFrame = new byte[currenttransferlen];
                        Array.Copy(SectorDataArray, j, NTxFrame, 0, currenttransferlen);
                        j += Convert.ToInt32(currenttransferlen);
                        //Debug.WriteLine("J==" + j, "ELMZ");

                        Frame_len = Convert.ToInt32(currenttransferlen);

                        var frame = ByteArrayToString(NTxFrame);
                        //await Task.Delay(20);
                        var bulkTransferResponse = await dongleComm.CAN_TxRx(frame.Length / 2, frame);

                        if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
                        {
                            Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                            Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                            //await dongleComm.Dongle_SetProtocol((int)0x14);
                            //await Task.Delay(2000);
                            await dongleComm.CAN_SetP2Max("0000");
                            return bulkTransferResponse.ECUResponseStatus;
                        }

                    }

                    Frame_len = 0;
                    TxFrame[Frame_len++] = 0xC1;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x00;
                    Frame_len = 8;
                    response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                    if (response?.ECUResponseStatus != "NOERROR")
                    {
                        Debug.WriteLine("-------- ERROR ------------==" + response.ECUResponseStatus, "ELMZ");
                        await dongleComm.CAN_SetP2Max("0000");
                        return response.ECUResponseStatus;
                    }
                }

                //await dongleComm.Dongle_SetProtocol((int)0x14);
                //await Task.Delay(2000);

                can_res = await dongleComm.CAN_SetTxHeader("0000");
                can_res = await dongleComm.CAN_SetRxHeaderMask("00A6");

                can_res = await dongleComm.CAN_StopPadding();

                Array.Resize(ref TxFrame, 2);
                Frame_len = 2;
                TxFrame[0] = 0x81;
                TxFrame[1] = 0x26;
                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                if (response?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("-------- ERROR ------------==" + response.ECUResponseStatus, "ELMZ");
                    await dongleComm.CAN_SetP2Max("0000");
                    return response.ECUResponseStatus;
                }
                await Task.Delay(1500);
                can_res = await dongleComm.CAN_ClearSocket();
                //await Task.Delay(500);

                can_res = await dongleComm.CAN_StartPadding("00");
                //await Task.Delay(200);
                can_res = await dongleComm.CAN_SetTxHeader(txHeader);
                //await Task.Delay(200);

                can_res = await dongleComm.CAN_SetRxHeaderMask(rxHeader);
                //await Task.Delay(200);


                Array.Resize(ref TxFrame, 8);
                Frame_len = 0;
                //TxFrame[Frame_len++] = 0x06;
                //TxFrame[Frame_len++] = 0x26;
                // Array.Resize(ref TxFrame, 8);

                TxFrame[Frame_len++] = 0x22;
                TxFrame[Frame_len++] = 0x2F;
                TxFrame[Frame_len++] = 0x33;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x01;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;

                //Frame_len = 10;

                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                if (response?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("-------- ERROR ------------==" + response.ECUResponseStatus, "ELMZ");
                    await dongleComm.CAN_SetP2Max("0000");
                    return response.ECUResponseStatus;
                }
                Frame_len = 0;
                //TxFrame[Frame_len++] = 0x06;
                //TxFrame[Frame_len++] = 0x26;
                TxFrame[Frame_len++] = 0x22;
                TxFrame[Frame_len++] = 0x80;
                TxFrame[Frame_len++] = 0x31;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;

                //Frame_len = 10;

                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                if (response?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("-------- ERROR ------------==" + response.ECUResponseStatus, "ELMZ");
                    await dongleComm.CAN_SetP2Max("0000");
                    return response.ECUResponseStatus;
                }
                //await Task.Delay(200);
                // Send Reset Command
                can_res = await dongleComm.CAN_SetTxHeader("0000");
                //await Task.Delay(200);
                can_res = await dongleComm.CAN_SetRxHeaderMask("00A6");

                can_res = await dongleComm.CAN_StopPadding();

                Array.Resize(ref TxFrame, 2);
                Frame_len = 2;
                TxFrame[0] = 0x81;
                TxFrame[1] = 0x26;
                response = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                if (response?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("-------- ERROR ------------==" + response.ECUResponseStatus, "ELMZ");
                    await dongleComm.CAN_SetP2Max("0000");
                    return response.ECUResponseStatus;
                }
                await Task.Delay(1500);
                can_res = await dongleComm.CAN_ClearSocket();
                //await Task.Delay(500);


                return response.ECUResponseStatus;
            }
            catch (Exception ex)
            {
                //await dongleComm.Dongle_SetProtocol((int)0x14);
                //await Task.Delay(2000);
                //await dongleComm.CAN_SetP2Max("0000");
                return null;
            }
            finally
            {
                can_res = await dongleComm.CAN_StartPadding("00");
                //await Task.Delay(200);
                can_res = await dongleComm.CAN_SetTxHeader(txHeader);
                //await Task.Delay(200);

                can_res = await dongleComm.CAN_SetRxHeaderMask(rxHeader);
                //await Task.Delay(200);

                can_res = await dongleComm.CAN_SetP2Max("0000");
                //await Task.Delay(200);

                can_res = await dongleComm.Dongle_SetProtocol(protocol);
            }
        }

        //public async Task<string> StartFlashBosch(flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata)
        //{

        //    float runtimeflashpercent = 0;
        //    // switch to reprogramming mode - 10 02
        //    int Frame_len = 0;
        //    byte[] TxFrame = new byte[2];
        //    TxFrame[Frame_len++] = 0x10;
        //    TxFrame[Frame_len++] = 0x02;

        //    Debug.WriteLine("-------switch to reprogramming mode-------");
        //    var reprogrammingResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    if (reprogrammingResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- reprogrammingResponse ERROR------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------reprogrammingResponse LOOP END------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
        //        return reprogrammingResponse.ECUResponseStatus;
        //    }


        //    // get Seed - 27 09
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x27;
        //    TxFrame[Frame_len++] = flashconfig_data.sendseedbyte;

        //    Debug.WriteLine("-------get Seed-------");

        //    var getSeedResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    if (getSeedResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- getSeedResponse ERROR------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------getSeedResponse LOOP END------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
        //        return getSeedResponse.ECUResponseStatus;
        //    }

        //    var seedarray = new byte[flashconfig_data.seedkeynumbytes];
        //    byte numkeybytes = new byte();

        //    Array.Copy(getSeedResponse.ActualDataBytes, 2, seedarray, 0, getSeedResponse.ActualDataBytes.Length - 2);

        //    //compute key for the seed received
        //    //status = getkeyfromseed(flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, Rxarray + 2, keyarray);

        //    byte[] actualKey = new byte[flashconfig_data.seedkeynumbytes];
        //    calculateSeedkey = new ECUCalculateSeedkey();
        //    Debug.WriteLine("-------get key-------");
        //    //var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEX_TYPE.GREAVES_ADVANTEK_A46_BS6, 4, ref numkeybytes, seedarray, ref actualKey);
        //    //var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD, 4, ref numkeybytes, seedarray, ref actualKey);

        //    var enums = string.Empty;

        //    enums = Enum.GetName(flashconfig_data.seedkeyindex.GetType(), flashconfig_data.seedkeyindex);

        //    var seed_key = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), enums);
        //    //var seed_key = (SEEDKEYINDEXTYPE) flashconfig_data.seedkeyindex;
        //    var result = calculateSeedkey.CalculateSeedkey(seed_key, flashconfig_data.seedkeynumbytes, ref numkeybytes, seedarray, ref actualKey);

        //    Array.Resize(ref TxFrame, flashconfig_data.seedkeynumbytes + 2);//
        //                                                                    //send key 27 0A
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x27;
        //    TxFrame[Frame_len++] = (byte)(flashconfig_data.sendseedbyte + 1);
        //    for (int i = 0; i < actualKey.Length; i++)
        //    {
        //        TxFrame[Frame_len++] = actualKey[i];
        //    }
        //    //status = ISO15765_CANTxRx(channel, Frame_len, TxFrame, &Rxsize, Rxarray);
        //    var sendKeyResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    if (sendKeyResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- sendKeyResponse ERROR------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------sendKeyResponse LOOP END------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
        //        return sendKeyResponse.ECUResponseStatus;
        //    }

        //    /* ECU Mem Map                        */
        //    /* Sector 1 - 0x80004000 - 0x80013fff */
        //    /* Sector 2 - 0x80080000 - 0x8017ffff */
        //    /* Sector 3 - 0x80020000 - 0x8007ffff */


        //    // Erase Memory
        //    if (flashconfig_data.erasesector == EraseSectorEnum.ERASEALLATONCE)
        //    {
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x31;
        //        TxFrame[Frame_len++] = 0x01;
        //        TxFrame[Frame_len++] = 0xFF;
        //        TxFrame[Frame_len++] = 0x00;

        //        Array.Resize(ref TxFrame, 12);
        //        // advantek erase memory map hard coded here...
        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = 0x02;
        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = 0x00;

        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = 0x0F;
        //        TxFrame[Frame_len++] = 0xFF;
        //        TxFrame[Frame_len++] = 0xFF;
        //        var eraseMemoryResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //        if (eraseMemoryResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------- eraseMemoryResponse ERROR------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("---------eraseMemoryResponse LOOP END------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //            return eraseMemoryResponse.ECUResponseStatus;
        //        }

        //    }
        //    else if (flashconfig_data.erasesector == EraseSectorEnum.ERASEALLATONCE_WOADDR)
        //    {
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x31;
        //        TxFrame[Frame_len++] = 0x01;
        //        TxFrame[Frame_len++] = 0xFF;
        //        TxFrame[Frame_len++] = 0x00;

        //        Array.Resize(ref TxFrame, 6);
        //        // SEDEMAC erase memory map hard coded here...
        //        TxFrame[Frame_len++] = 0x01;
        //        TxFrame[Frame_len++] = 0x01;
        //        var eraseMemoryResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //        if (eraseMemoryResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------- eraseMemoryResponse ERROR------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("---------eraseMemoryResponse LOOP END------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //            return eraseMemoryResponse.ECUResponseStatus;
        //        }

        //    }


        //    /* Calculate the total number of bytes to be flashed in total and then calculated runtime flashing % */
        //    for (int i = 0; i < noofsectors; i++)
        //    {
        //        var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;
        //        totalbytestobeflashed += sector_num_bytes;
        //        realtimebytesflashed = 0;
        //    }


        //    var addrdataformat = Convert.ToByte(flashconfig_data.addrdataformat, 16);
        //    for (int i = 0; i < noofsectors; i++)
        //    {
        //        var sector_start_address = Convert.ToUInt32(sectordata[i].JsonStartAddress, 16);
        //        var sector_end_address = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16);
        //        var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;

        //        var ecu_memmap_start_address = Convert.ToUInt32(sectordata[i].ECUMemMapStartAddress, 16);
        //        var ecu_memmap_end_address = Convert.ToUInt32(sectordata[i].ECUMemMapEndAddress, 16);


        //        if (flashconfig_data.erasesector == EraseSectorEnum.ERASEBYSECTOR)
        //        {
        //            // Erase Memory
        //            Frame_len = 0;
        //            TxFrame[Frame_len++] = 0x31;
        //            TxFrame[Frame_len++] = 0x01;
        //            TxFrame[Frame_len++] = 0xFF;
        //            TxFrame[Frame_len++] = 0x00;

        //            if ((Convert.ToByte(flashconfig_data.addrdataformat, 16) & 0xF0) == 0x30)
        //            {
        //                Array.Resize(ref TxFrame, 10);

        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF0000) >> 16);
        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF00) >> 8);
        //                TxFrame[Frame_len++] = (byte)(ecu_memmap_start_address & 0x000000FF);

        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF0000) >> 16);
        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF00) >> 8);
        //                TxFrame[Frame_len++] = (byte)(ecu_memmap_end_address & 0x000000FF);

        //            }
        //            else
        //            {
        //                Array.Resize(ref TxFrame, 12);

        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0xFF000000) >> 24);
        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF0000) >> 16);
        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF00) >> 8);
        //                TxFrame[Frame_len++] = (byte)(ecu_memmap_start_address & 0x000000FF);

        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0xFF000000) >> 24);
        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF0000) >> 16);
        //                TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF00) >> 8);
        //                TxFrame[Frame_len++] = (byte)(ecu_memmap_end_address & 0x000000FF);

        //            }

        //            var response2 = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //            if (response2?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------- response2 ERROR------------==" + response2.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("---------response2 LOOP END------------==" + response2.ECUResponseStatus, "ELMZ");
        //                return response2.ECUResponseStatus;
        //            }

        //            Frame_len = 0;
        //            TxFrame = new byte[4];
        //            TxFrame[Frame_len++] = 0x31;
        //            TxFrame[Frame_len++] = 0x03;
        //            TxFrame[Frame_len++] = 0xFF;
        //            TxFrame[Frame_len++] = 0x00;

        //            var checkerase = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //            if (checkerase?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------- checkerase ERROR------------==" + checkerase.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("---------checkerase LOOP END------------==" + checkerase.ECUResponseStatus, "ELMZ");
        //                return response2.ECUResponseStatus;
        //            }

        //        }


        //        // Request download
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x34;
        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = Convert.ToByte(flashconfig_data.addrdataformat, 16);

        //        if (addrdataformat == 0x33)
        //        {
        //            Array.Resize(ref TxFrame, 9);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

        //        }
        //        else if (addrdataformat == 0x44) // addrdataformat == 0x44
        //        {
        //            Array.Resize(ref TxFrame, 11);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0xFF000000) >> 24);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

        //        }


        //        var memoryResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //        if (memoryResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------- memoryResponse ERROR------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("---------memoryResponse LOOP END------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
        //            return memoryResponse.ECUResponseStatus;
        //        }

        //        // Transfer Data in this sector
        //        int blkseqcnt = 1;
        //        var SectorDataArray = HexStringToByteArray(sectordata[i].JsonData);
        //        for (int j = 0; j < sector_num_bytes;)
        //        {
        //            try
        //            {
        //                var NTxFrame = new byte[2];
        //                NTxFrame[0] = 0x36;
        //                NTxFrame[1] = (byte)(blkseqcnt & 0xFF);
        //                var currenttransferlen = Math.Min(sector_num_bytes - j, flashconfig_data.sectorframetransferlen);
        //                //var currenttransferlen = Math.Min(sector_num_bytes - j, 252);

        //                Debug.WriteLine("currenttransferlen==" + currenttransferlen + 2, "ELMZ");

        //                Array.Resize(ref NTxFrame, Convert.ToInt32(currenttransferlen) + 2);

        //                Array.Copy(SectorDataArray, j, NTxFrame, 2, currenttransferlen);
        //                j += Convert.ToInt32(currenttransferlen);
        //                Debug.WriteLine("J==" + j, "ELMZ");

        //                Frame_len = Convert.ToInt32(currenttransferlen + 2);

        //                var frame = ByteArrayToString(NTxFrame);
        //                var bulkTransferResponse = await dongleComm.CAN_TxRx(Frame_len, frame);
        //                blkseqcnt++;

        //                if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
        //                {
        //                    Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
        //                    Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
        //                    return bulkTransferResponse.ECUResponseStatus;

        //                }
        //                realtimebytesflashed += (UInt32)currenttransferlen;

        //                Debug.WriteLine("blkseqcnt==" + blkseqcnt, "ELMZ");
        //            }
        //            catch (Exception exception)
        //            {
        //                Debug.WriteLine("exception==" + exception.Message, "ELMZ");
        //                var msg = exception;
        //                return "exception";
        //            }
        //        }

        //        // Transfer Exit
        //        var BTxFrame = new byte[1];
        //        BTxFrame[0] = 0x37;
        //        Frame_len = 1;
        //        var TransferResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(BTxFrame));
        //        if (TransferResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------TransferResponse -ERROR------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("--------TransferResponse -LOOP END------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
        //            return TransferResponse.ECUResponseStatus;
        //        }


        //        if (flashconfig_data.checksumsector == ChecksumSectorEnum.COMPAREBYSECTOR_WOADDR_CRCCCITT16)
        //        {
        //            TxFrame = new byte[8];
        //            Frame_len = 0;
        //            TxFrame[Frame_len++] = 0x31;
        //            TxFrame[Frame_len++] = 0x01;
        //            TxFrame[Frame_len++] = 0xFF;
        //            TxFrame[Frame_len++] = 0x01;
        //            TxFrame[Frame_len++] = 0x01;
        //            TxFrame[Frame_len++] = 0x01;

        //            UInt16 jsonCheckSum = Convert.ToUInt16(sectordata[i].JsonCheckSum, 16);
        //            TxFrame[Frame_len++] = (byte)((jsonCheckSum & 0xFF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)(jsonCheckSum & 0x00FF);

        //            var sendsectorchksumResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //            if (sendsectorchksumResponse?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------sendsectorchksumResponse -ERROR------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("--------sendsectorchksumResponse -LOOP END------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
        //                return sendsectorchksumResponse.ECUResponseStatus;
        //            }

        //        }
        //        else if ((flashconfig_data.checksumsector == ChecksumSectorEnum.COMPAREBYSECTOR) || (flashconfig_data.checksumsector == ChecksumSectorEnum.COMPUTEBYSECTOR))
        //        {
        //            //Checksum test
        //            TxFrame = new byte[10];
        //            Frame_len = 0;
        //            TxFrame[Frame_len++] = 0x31;
        //            TxFrame[Frame_len++] = 0x01;
        //            TxFrame[Frame_len++] = 0xFF;
        //            TxFrame[Frame_len++] = 0x01;

        //            if ((addrdataformat & 0xF0) == 0x30)
        //            {

        //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

        //                TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
        //                TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
        //                TxFrame[Frame_len++] = (byte)((sector_end_address & 0x000000FF));

        //            }
        //            else
        //            {
        //                Array.Resize(ref TxFrame, 12);
        //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
        //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //                TxFrame[Frame_len++] = (byte)(sector_start_address & 0x000000FF);

        //                TxFrame[Frame_len++] = (byte)((sector_end_address & 0xFF000000) >> 24);
        //                TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
        //                TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
        //                TxFrame[Frame_len++] = (byte)(sector_end_address & 0x000000FF);
        //            }

        //        }

        //        if (flashconfig_data.checksumsector == ChecksumSectorEnum.COMPAREBYSECTOR)
        //        {
        //            Array.Resize(ref TxFrame, Frame_len + 2);
        //            UInt16 jsonCheckSum = Convert.ToUInt16(sectordata[i].JsonCheckSum, 16);
        //            TxFrame[Frame_len++] = (byte)((jsonCheckSum & 0xFF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)(jsonCheckSum & 0x00FF);

        //            //TxFrame[Frame_len++] = (byte)((Convert.ToUInt16(sectordata[i].JsonCheckSum) & 0xFF00) >> 8);
        //            //TxFrame[Frame_len++] = (byte)( Convert.ToUInt16(sectordata[i].JsonCheckSum) & 0x00FF);
        //        }


        //        if ((flashconfig_data.checksumsector == ChecksumSectorEnum.COMPAREBYSECTOR) || (flashconfig_data.checksumsector == ChecksumSectorEnum.COMPUTEBYSECTOR))
        //        {
        //            var sendsectorchksumResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //            if (TransferResponse?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------sendsectorchksumResponse -ERROR------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("--------sendsectorchksumResponse -LOOP END------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
        //                return sendsectorchksumResponse.ECUResponseStatus;
        //            }

        //            TxFrame = new byte[4];
        //            Frame_len = 0;
        //            TxFrame[Frame_len++] = 0x31;
        //            TxFrame[Frame_len++] = 0x03;
        //            TxFrame[Frame_len++] = 0xFF;
        //            TxFrame[Frame_len++] = 0x01;

        //            var sendsectorchksumResponse2 = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //            if (sendsectorchksumResponse2?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------sendsectorchksumResponse2 -ERROR------------==" + sendsectorchksumResponse2.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("--------sendsectorchksumResponse2 -LOOP END------------==" + sendsectorchksumResponse2.ECUResponseStatus, "ELMZ");
        //                return sendsectorchksumResponse2.ECUResponseStatus;
        //            }
        //        }

        //    } // end of all sectors

        //    string return_status = string.Empty;
        //    TxFrame = new byte[2];

        //    if (((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_DEV) || ((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD) || ((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_SEDEMAC_BL_BS6))
        //    {
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x11;
        //        TxFrame[Frame_len++] = 0x01;
        //    }
        //    else
        //    {
        //        // Reset ECU

        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x11;
        //        TxFrame[Frame_len++] = 0x02;
        //    }
        //    var resetResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    if (resetResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------resetResponse -ERROR------------==" + resetResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("--------resetResponse -LOOP END------------==" + resetResponse.ECUResponseStatus, "ELMZ");
        //        return resetResponse.ECUResponseStatus;
        //    }
        //    return_status = resetResponse.ECUResponseStatus;
        //    return (return_status);

        //}


        #region Terminal
        public async Task<APDongleCommAnroid.Models.ResponseArrayStatus> SetDataData(string command)
        {
            var frame_lenght = command.Length / 2;
            var pidResponse = await dongleComm.CAN_TxRx(frame_lenght, command);
            return pidResponse;
        }
        #endregion

        #region StartFlashAdvantekBS6
        //public async Task<string> StartFlashAdvantekBS6(flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata)
        //{

        //    float runtimeflashpercent = 0;
        //    // switch to reprogramming mode - 10 02
        //    int Frame_len = 0;
        //    byte[] TxFrame = new byte[2];
        //    TxFrame[Frame_len++] = 0x10;
        //    TxFrame[Frame_len++] = 0x02;
        //    byte addressdataformat = 0x00;

        //    Debug.WriteLine("-------switch to reprogramming mode-------");
        //    var reprogrammingResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    if (reprogrammingResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- reprogrammingResponse ERROR------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------reprogrammingResponse LOOP END------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
        //        return reprogrammingResponse.ECUResponseStatus;
        //    }

        //    // get Seed - 27 09
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x27;
        //    TxFrame[Frame_len++] = flashconfig_data.sendseedbyte;

        //    Debug.WriteLine("-------get Seed-------");

        //    var getSeedResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    if (getSeedResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- getSeedResponse ERROR------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------getSeedResponse LOOP END------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
        //        return getSeedResponse.ECUResponseStatus;
        //    }

        //    var seedarray = new byte[flashconfig_data.seedkeynumbytes];
        //    byte numkeybytes = new byte();

        //    Array.Copy(getSeedResponse.ActualDataBytes, 2, seedarray, 0, getSeedResponse.ActualDataBytes.Length - 2);

        //    //compute key for the seed received
        //    //status = getkeyfromseed(flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, Rxarray + 2, keyarray);

        //    byte[] actualKey = new byte[flashconfig_data.seedkeynumbytes];
        //    calculateSeedkey = new ECUCalculateSeedkey();
        //    Debug.WriteLine("-------get key-------");
        //    //var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEX_TYPE.GREAVES_ADVANTEK_A46_BS6, 4, ref numkeybytes, seedarray, ref actualKey);
        //    //var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD, 4, ref numkeybytes, seedarray, ref actualKey);
        //    var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEXTYPE.SML_ADVANTEK_BS6_PROD, flashconfig_data.seedkeynumbytes, ref numkeybytes, seedarray, ref actualKey);

        //    Array.Resize(ref TxFrame, flashconfig_data.seedkeynumbytes + 2);//
        //    //send key 27 0A
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x27;
        //    TxFrame[Frame_len++] = (byte)(flashconfig_data.sendseedbyte + 1);
        //    for (int i = 0; i < actualKey.Length; i++)
        //    {
        //        TxFrame[Frame_len++] = actualKey[i];
        //    }
        //    //status = ISO15765_CANTxRx(channel, Frame_len, TxFrame, &Rxsize, Rxarray);
        //    var sendKeyResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    if (sendKeyResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- sendKeyResponse ERROR------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------sendKeyResponse LOOP END------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
        //        return sendKeyResponse.ECUResponseStatus;
        //    }

        //    /* ECU Mem Map                        */
        //    /* Sector 1 - 0x80004000 - 0x80013fff */
        //    /* Sector 2 - 0x80080000 - 0x8017ffff */
        //    /* Sector 3 - 0x80020000 - 0x8007ffff */


        //    // Erase Memory
        //    if (flashconfig_data.erasesector == EraseSectorEnum.ERASEALLATONCE)
        //    {
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x31;
        //        TxFrame[Frame_len++] = 0x01;
        //        TxFrame[Frame_len++] = 0xFF;
        //        TxFrame[Frame_len++] = 0x00;

        //        Array.Resize(ref TxFrame, 12);
        //        // advantek erase memory map hard coded here...
        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = 0x02;
        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = 0x00;

        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = 0x1F;
        //        TxFrame[Frame_len++] = 0xFF;
        //        TxFrame[Frame_len++] = 0xFF;


        //        var eraseMemoryResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //        if (eraseMemoryResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------- eraseMemoryResponse ERROR------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("---------eraseMemoryResponse LOOP END------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //            return eraseMemoryResponse.ECUResponseStatus;
        //        }
        //    }

        //    /* Calculate the total number of bytes to be flashed in total and then calculated runtime flashing % */
        //    for (int i = 0; i < noofsectors; i++)
        //    {
        //        var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;
        //        totalbytestobeflashed += sector_num_bytes;
        //        realtimebytesflashed = 0;
        //    }

        //    var addrdataformat = flashconfig_data.addrdataformat;
        //    for (int i = 0; i < noofsectors; i++)
        //    {
        //        var sector_start_address = Convert.ToUInt32(sectordata[i].JsonStartAddress, 16);
        //        var sector_end_address = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16);
        //        var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;

        //        var ecu_memmap_start_address = Convert.ToUInt32(sectordata[i].ECUMemMapStartAddress, 16);
        //        var ecu_memmap_end_address = Convert.ToUInt32(sectordata[i].ECUMemMapEndAddress, 16);


        //        if (flashconfig_data.erasesector == EraseSectorEnum.ERASEBYSECTOR)
        //        {
        //            // Erase Memory
        //            Frame_len = 0;
        //            TxFrame[Frame_len++] = 0x31;
        //            TxFrame[Frame_len++] = 0x01;
        //            TxFrame[Frame_len++] = 0xFF;
        //            TxFrame[Frame_len++] = 0x00;
        //            TxFrame[Frame_len++] = 0x01;
        //            TxFrame[Frame_len++] = (byte)i;
        //            Array.Resize(ref TxFrame, 6);
        //            var response2 = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //            if (response2?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------- response2 ERROR------------==" + response2.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("---------response2 LOOP END------------==" + response2.ECUResponseStatus, "ELMZ");
        //                return response2.ECUResponseStatus;
        //            }
        //        }


        //        // Request download
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x34;
        //        TxFrame[Frame_len++] = 0x00;

        //        if (addrdataformat == "33_00FFFFFF")
        //        {
        //            Array.Resize(ref TxFrame, 9);
        //            TxFrame[Frame_len++] = addressdataformat = 0x33;
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

        //        }
        //        else if (addrdataformat == "33_FF00FFFF")
        //        {
        //            Array.Resize(ref TxFrame, 9);
        //            TxFrame[Frame_len++] = addressdataformat = 0x33;
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x0000FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

        //        }

        //        else if (addrdataformat == "44") // addrdataformat == 0x44
        //        {
        //            Array.Resize(ref TxFrame, 11);
        //            TxFrame[Frame_len++] = addressdataformat = 0x44;
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0xFF000000) >> 24);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

        //        }


        //        var memoryResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //        if (memoryResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------- memoryResponse ERROR------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("---------memoryResponse LOOP END------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
        //            return memoryResponse.ECUResponseStatus;
        //        }

        //        // Transfer Data in this sector
        //        int blkseqcnt = 1;
        //        var SectorDataArray = HexStringToByteArray(sectordata[i].JsonData);
        //        for (int j = 0; j < sector_num_bytes;)
        //        {
        //            try
        //            {
        //                var NTxFrame = new byte[2];
        //                NTxFrame[0] = 0x36;
        //                NTxFrame[1] = (byte)(blkseqcnt & 0xFF);
        //                var currenttransferlen = Math.Min(sector_num_bytes - j, flashconfig_data.sectorframetransferlen);
        //                //var currenttransferlen = Math.Min(sector_num_bytes - j, 252);

        //                Debug.WriteLine("currenttransferlen==" + currenttransferlen + 2, "ELMZ");

        //                Array.Resize(ref NTxFrame, Convert.ToInt32(currenttransferlen) + 2);

        //                Array.Copy(SectorDataArray, j, NTxFrame, 2, currenttransferlen);
        //                j += Convert.ToInt32(currenttransferlen);
        //                Debug.WriteLine("J==" + j, "ELMZ");

        //                Frame_len = Convert.ToInt32(currenttransferlen + 2);

        //                var frame = ByteArrayToString(NTxFrame);
        //                var bulkTransferResponse = await dongleComm.CAN_TxRx(Frame_len, frame);
        //                blkseqcnt++;

        //                if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
        //                {
        //                    Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
        //                    Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
        //                    return bulkTransferResponse.ECUResponseStatus;

        //                }
        //                realtimebytesflashed += (UInt32)currenttransferlen;

        //                Debug.WriteLine("blkseqcnt==" + blkseqcnt, "ELMZ");
        //            }
        //            catch (Exception exception)
        //            {
        //                Debug.WriteLine("exception==" + exception.Message, "ELMZ");
        //                var msg = exception;
        //                return "exception";
        //            }
        //        }

        //        // Transfer Exit
        //        var BTxFrame = new byte[1];
        //        BTxFrame[0] = 0x37;
        //        Frame_len = 1;
        //        var TransferResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(BTxFrame));
        //        if (TransferResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------TransferResponse -ERROR------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("--------TransferResponse -LOOP END------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
        //            return TransferResponse.ECUResponseStatus;
        //        }

        //        ////Checksum test
        //        //var cTxFrame = new byte[6];
        //        //Frame_len = 0;
        //        //cTxFrame[Frame_len++] = 0x31;
        //        //cTxFrame[Frame_len++] = 0x01;
        //        //cTxFrame[Frame_len++] = 0x02;
        //        //cTxFrame[Frame_len++] = 0x02;
        //        //cTxFrame[Frame_len++] = 0x01;
        //        //cTxFrame[Frame_len++] = (byte)i;
        //        ////Array.Resize(ref cTxFrame, Frame_len);

        //        //var sendsectorchksumResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(cTxFrame));
        //        //if (TransferResponse?.ECUResponseStatus != "NOERROR")
        //        //{
        //        //    Debug.WriteLine("--------sendsectorchksumResponse -ERROR------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
        //        //    Debug.WriteLine("--------sendsectorchksumResponse -LOOP END------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
        //        //    return sendsectorchksumResponse.ECUResponseStatus;
        //        //}

        //    } // end of all sectors

        //    string return_status = string.Empty;
        //    TxFrame = new byte[2];

        //    //if (((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_DEV) || ((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD))
        //    {
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x11;
        //        TxFrame[Frame_len++] = 0x01;
        //    }
        //    //else 
        //    //{ 
        //    //    // Reset ECU

        //    //    Frame_len = 0;
        //    //    TxFrame[Frame_len++] = 0x11;
        //    //    TxFrame[Frame_len++] = 0x02;
        //    //}
        //    var resetResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    if (resetResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------resetResponse -ERROR------------==" + resetResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("--------resetResponse -LOOP END------------==" + resetResponse.ECUResponseStatus, "ELMZ");
        //        return resetResponse.ECUResponseStatus;
        //    }
        //    return_status = resetResponse.ECUResponseStatus;
        //    return (return_status);

        //}

        #endregion


        #region StartFlashBoschBS4
        public async Task<string> StartFlashBoschBS4(flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata)
        {

            float runtimeflashpercent = 0;
            // switch to reprogramming mode - 10 02
            int Frame_len = 0;
            byte[] TxFrame = new byte[2];
            TxFrame[Frame_len++] = 0x10;
            TxFrame[Frame_len++] = 0x02;
            byte addressdataformat = 0x00;

            Debug.WriteLine("-------switch to reprogramming mode-------");
            var reprogrammingResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
            if (reprogrammingResponse?.ECUResponseStatus != "NOERROR")
            {
                Debug.WriteLine("--------- reprogrammingResponse ERROR------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
                Debug.WriteLine("---------reprogrammingResponse LOOP END------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
                return reprogrammingResponse.ECUResponseStatus;
            }

            // get Seed - 27 09
            Frame_len = 0;
            TxFrame[Frame_len++] = 0x27;
            TxFrame[Frame_len++] = flashconfig_data.sendseedbyte;

            Debug.WriteLine("-------get Seed-------");

            var getSeedResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

            if (getSeedResponse?.ECUResponseStatus != "NOERROR")
            {
                Debug.WriteLine("--------- getSeedResponse ERROR------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
                Debug.WriteLine("---------getSeedResponse LOOP END------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
                return getSeedResponse.ECUResponseStatus;
            }

            var seedarray = new byte[flashconfig_data.seedkeynumbytes];
            byte numkeybytes = new byte();

            Array.Copy(getSeedResponse.ActualDataBytes, 2, seedarray, 0, getSeedResponse.ActualDataBytes.Length - 2);

            //compute key for the seed received
            //status = getkeyfromseed(flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, Rxarray + 2, keyarray);

            byte[] actualKey = new byte[flashconfig_data.seedkeynumbytes];
            calculateSeedkey = new ECUCalculateSeedkey();
            Debug.WriteLine("-------get key-------");
            //var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEX_TYPE.GREAVES_ADVANTEK_A46_BS6, 4, ref numkeybytes, seedarray, ref actualKey);
            //var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD, 4, ref numkeybytes, seedarray, ref actualKey);
            var result = calculateSeedkey.CalculateSeedkey((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, ref numkeybytes, seedarray, ref actualKey);

            Array.Resize(ref TxFrame, flashconfig_data.seedkeynumbytes + 2);//
            //send key 27 0A
            Frame_len = 0;
            TxFrame[Frame_len++] = 0x27;
            TxFrame[Frame_len++] = (byte)(flashconfig_data.sendseedbyte + 1);
            for (int i = 0; i < actualKey.Length; i++)
            {
                TxFrame[Frame_len++] = actualKey[i];
            }
            //status = ISO15765_CANTxRx(channel, Frame_len, TxFrame, &Rxsize, Rxarray);
            var sendKeyResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

            if (sendKeyResponse?.ECUResponseStatus != "NOERROR")
            {
                Debug.WriteLine("--------- sendKeyResponse ERROR------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
                Debug.WriteLine("---------sendKeyResponse LOOP END------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
                return sendKeyResponse.ECUResponseStatus;
            }

            /* ECU Mem Map                        */
            /* Sector 1 - 0x80004000 - 0x80013fff */
            /* Sector 2 - 0x80080000 - 0x8017ffff */
            /* Sector 3 - 0x80020000 - 0x8007ffff */


            // Erase Memory
            if (flashconfig_data.erasesector == EraseSectorEnum.ERASEALLATONCE)
            {
                Frame_len = 0;
                TxFrame[Frame_len++] = 0x31;
                TxFrame[Frame_len++] = 0x01;
                TxFrame[Frame_len++] = 0xFF;
                TxFrame[Frame_len++] = 0x00;

                Array.Resize(ref TxFrame, 12);
                // advantek erase memory map hard coded here...
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x02;
                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x00;

                TxFrame[Frame_len++] = 0x00;
                TxFrame[Frame_len++] = 0x0F;
                TxFrame[Frame_len++] = 0xFF;
                TxFrame[Frame_len++] = 0xFF;


                var eraseMemoryResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                if (eraseMemoryResponse?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("--------- eraseMemoryResponse ERROR------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
                    Debug.WriteLine("---------eraseMemoryResponse LOOP END------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
                    return eraseMemoryResponse.ECUResponseStatus;
                }
            }

            /* Calculate the total number of bytes to be flashed in total and then calculated runtime flashing % */
            for (int i = 0; i < noofsectors; i++)
            {
                var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;
                totalbytestobeflashed += sector_num_bytes;
                realtimebytesflashed = 0;
            }


            var addrdataformat = flashconfig_data.addrdataformat;
            for (int i = 0; i < noofsectors; i++)
            {
                var sector_start_address = Convert.ToUInt32(sectordata[i].JsonStartAddress, 16);
                var sector_end_address = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16);
                var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;

                var ecu_memmap_start_address = Convert.ToUInt32(sectordata[i].ECUMemMapStartAddress, 16);
                var ecu_memmap_end_address = Convert.ToUInt32(sectordata[i].ECUMemMapEndAddress, 16);


                if (flashconfig_data.erasesector == EraseSectorEnum.ERASEBYSECTOR)
                {
                    // Erase Memory
                    Frame_len = 0;
                    TxFrame[Frame_len++] = 0x31;
                    TxFrame[Frame_len++] = 0x01;
                    TxFrame[Frame_len++] = 0xFF;
                    TxFrame[Frame_len++] = 0x00;
                    TxFrame[Frame_len++] = 0x01;
                    TxFrame[Frame_len++] = (byte)i;
                    Array.Resize(ref TxFrame, 6);
                    var response2 = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

                    if (response2?.ECUResponseStatus != "NOERROR")
                    {
                        Debug.WriteLine("--------- response2 ERROR------------==" + response2.ECUResponseStatus, "ELMZ");
                        Debug.WriteLine("---------response2 LOOP END------------==" + response2.ECUResponseStatus, "ELMZ");
                        return response2.ECUResponseStatus;
                    }
                }


                // Request download
                Frame_len = 0;
                TxFrame[Frame_len++] = 0x34;
                TxFrame[Frame_len++] = 0x00;

                if (addrdataformat == "33_00FFFFFF")
                {
                    Array.Resize(ref TxFrame, 9);
                    TxFrame[Frame_len++] = addressdataformat = 0x33;
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

                }
                else if (addrdataformat == "33_FF00FFFF")
                {
                    Array.Resize(ref TxFrame, 9);
                    TxFrame[Frame_len++] = addressdataformat = 0x33;
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x0000FF00) >> 8);
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

                }

                else if (addrdataformat == "44") // addrdataformat == 0x44
                {
                    Array.Resize(ref TxFrame, 11);
                    TxFrame[Frame_len++] = addressdataformat = 0x44;
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0xFF000000) >> 24);
                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
                    TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

                }


                var memoryResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
                if (memoryResponse?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("--------- memoryResponse ERROR------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
                    Debug.WriteLine("---------memoryResponse LOOP END------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
                    return memoryResponse.ECUResponseStatus;
                }

                // Transfer Data in this sector
                int blkseqcnt = 1;
                var SectorDataArray = HexStringToByteArray(sectordata[i].JsonData);
                for (int j = 0; j < sector_num_bytes;)
                {
                    try
                    {
                        var NTxFrame = new byte[2];
                        NTxFrame[0] = 0x36;
                        NTxFrame[1] = (byte)(blkseqcnt & 0xFF);
                        var currenttransferlen = Math.Min(sector_num_bytes - j, flashconfig_data.sectorframetransferlen);
                        //var currenttransferlen = Math.Min(sector_num_bytes - j, 252);

                        Debug.WriteLine("currenttransferlen==" + currenttransferlen + 2, "ELMZ");

                        Array.Resize(ref NTxFrame, Convert.ToInt32(currenttransferlen) + 2);

                        Array.Copy(SectorDataArray, j, NTxFrame, 2, currenttransferlen);
                        j += Convert.ToInt32(currenttransferlen);
                        Debug.WriteLine("J==" + j, "ELMZ");

                        Frame_len = Convert.ToInt32(currenttransferlen + 2);

                        var frame = ByteArrayToString(NTxFrame);
                        var bulkTransferResponse = await dongleComm.CAN_TxRx(Frame_len, frame);
                        blkseqcnt++;

                        if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
                        {
                            Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                            Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
                            return bulkTransferResponse.ECUResponseStatus;

                        }
                        realtimebytesflashed += (UInt32)currenttransferlen;

                        Debug.WriteLine("blkseqcnt==" + blkseqcnt, "ELMZ");
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine("exception==" + exception.Message, "ELMZ");
                        var msg = exception;
                        return "exception";
                    }
                }

                // Transfer Exit
                var BTxFrame = new byte[1];
                BTxFrame[0] = 0x37;
                Frame_len = 1;
                var TransferResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(BTxFrame));
                if (TransferResponse?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("--------TransferResponse -ERROR------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
                    Debug.WriteLine("--------TransferResponse -LOOP END------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
                    return TransferResponse.ECUResponseStatus;
                }

                //Checksum test
                var cTxFrame = new byte[6];
                Frame_len = 0;
                cTxFrame[Frame_len++] = 0x31;
                cTxFrame[Frame_len++] = 0x01;
                cTxFrame[Frame_len++] = 0x02;
                cTxFrame[Frame_len++] = 0x02;
                cTxFrame[Frame_len++] = 0x01;
                cTxFrame[Frame_len++] = (byte)i;
                //Array.Resize(ref cTxFrame, Frame_len);

                var sendsectorchksumResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(cTxFrame));
                if (TransferResponse?.ECUResponseStatus != "NOERROR")
                {
                    Debug.WriteLine("--------sendsectorchksumResponse -ERROR------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
                    Debug.WriteLine("--------sendsectorchksumResponse -LOOP END------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
                    return sendsectorchksumResponse.ECUResponseStatus;
                }

            } // end of all sectors

            string return_status = string.Empty;
            TxFrame = new byte[2];

            //if (((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_DEV) || ((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD))
            {
                Frame_len = 0;
                TxFrame[Frame_len++] = 0x11;
                TxFrame[Frame_len++] = 0x01;
            }
            //else 
            //{ 
            //    // Reset ECU

            //    Frame_len = 0;
            //    TxFrame[Frame_len++] = 0x11;
            //    TxFrame[Frame_len++] = 0x02;
            //}
            var resetResponse = await dongleComm.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
            if (resetResponse?.ECUResponseStatus != "NOERROR")
            {
                Debug.WriteLine("--------resetResponse -ERROR------------==" + resetResponse.ECUResponseStatus, "ELMZ");
                Debug.WriteLine("--------resetResponse -LOOP END------------==" + resetResponse.ECUResponseStatus, "ELMZ");
                return resetResponse.ECUResponseStatus;
            }
            return_status = resetResponse.ECUResponseStatus;
            return (return_status);

        }

        #endregion

        #region
        public async Task TestBT(bool control)
        {
            try
            {
                int count = 0;
                //string command = "3601DEADBEEF0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000200005008FF4AD00900000008FD816801010601FAFAFAFAFAFAFAFAFAFAFAFAFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF08FF4AD831305357303739383034000408FD810008FF4FE70009000208FF4FE84A500424B5AFFBDB008A00040005000008FD81B000860004000700005281A63C000200040007000008FD8F68010100040005000008FD8200010200040005000008FD8208010000230003000008FD8E0008FFFFFF00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000060000003108FD837408FD87480300000008FD87600100000008FD877C0100000008FD87980100000008FD87A801000000000000010000000100000000000000000000000008FD87B80100000008FD87C00100000008FD87DC01000000000000000000000000000000000000000000001A0000000100000000000000000000000008FD87F80300000008FD88100100000008FD882C0100000008FD88480100000008FD885801000000000000020000000100000000000000000000000008FD88680300000008FD88800100000008FD889C0100000008FD88B80100000008FD88C8010000000000000E0000000100000000000000000000000008FD88D80300000008FD88F00100000008FD890C0100000008FD89280100000008FD8938010000000000000F0000000100000000000000000000000008FD89480200000008FD89580100000008FD89740100000000000000000000000000000000000000000000240000000100000000000000000000000000006014000000040000000108FD87440000433C000000100000001008FD873400000C99000000010000000108FD87300000A5F1000000010000000108FD872F000058A5000000010000000108FD872E0000661C000000010000000108FD872D0000C6C7000000010000000108FD872C0000ADFD000000040000000108FD87280000E45E000000040000000108FD87240000A362000000040000000108FD87200000DFEB000000040000000108FD871C00003183000000040000000108FD8718000095F9000000040000000108FD87140000A5D5000000040000000108FD871000000DB3000000040000000108FD870C000096F9000000040000000108FD8708000097CF000000040000000108FD870400001F38000000040000000108FD8700000097F9000000040000000108FD86FC00008646000000040000000108FD86F800001EB9000000040000000108FD86F4000090F9000000040000000108FD86F00000E372000000040000000108FD86EC00002A26000000040000000108FD86E8000091F90000000400000001";
                //int length = 1282;

                string command = "2EF1903132333132333132333132333132333132";
                int length = 20;

                while (control)
                {
                    Debug.WriteLine($"------METHOD RUN NO OF TIME : {count++}------");
                    var responseBytes = await this.dongleComm.CAN_TxRx(length, command);
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

    }

    #region Old Code
    //public class UDSDiagnostic
    //{
    //    #region Properties
    //    ReadDTCIndex DTCIndex = 0;
    //    private DongleCommWin dongleCommWin;
    //    private ECUCalculateSeedkey calculateSeedkey;
    //    #endregion

    //    #region CTOR
    //    public UDSDiagnostic(DongleCommWin dongleCommWin)
    //    {
    //        this.dongleCommWin = dongleCommWin;
    //    }
    //    #endregion

    //    #region ReadDTC
    //    public async Task<ReadDtcResponseModel> ReadDTC(ReadDTCIndex readDtcIndex)
    //    {
    //        DTCIndex = readDtcIndex;
    //        string status = string.Empty;
    //        string return_status = string.Empty;
    //        string[,] dtcarray = null;
    //        UInt16 dtcindex = 0;
    //        if (DTCIndex == ReadDTCIndex.KWP_2BYTE_DTC || DTCIndex == ReadDTCIndex.UDS_2BYTE12_DTC || DTCIndex == ReadDTCIndex.UDS_2BYTE_DTC || DTCIndex == ReadDTCIndex.UDS_2BYTE13_DTC || DTCIndex == ReadDTCIndex.UDS_3BYTE_DTC)
    //        {
    //            var frameLength = 3;
    //            var responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "1902AF");
    //            status = responseBytes.ECUResponseStatus;
    //            var actualData = responseBytes.ActualDataBytes;
    //            return_status = string.Empty;
    //            if (status == "NOERROR")
    //            {
    //                return_status = "NO_ERROR";
    //                var Rxsize = actualData.Length;
    //                var Rxarray = actualData;
    //                string dtc_type = string.Empty;


    //                /* 59 02 FF DTCHB DTCMB1 DTCLB1 DTCSTS1 DTCHB2 DTCMB2 DTCLB2 DTCSTS2 ..... DTCHBn DTCMBn DTCLBn DTCSTSn*/
    //                var dtc_start_byte_index = 3;
    //                var no_of_dtc = (Rxsize - 3) / 4;
    //                dtcarray = new string[no_of_dtc, 2];
    //                var i = 0;

    //                while (i < no_of_dtc)
    //                {

    //                    if ((Rxarray[dtc_start_byte_index + 3] == 0x40) || (Rxarray[dtc_start_byte_index + 3] == 0x50))
    //                    {
    //                        /* dont consider these dtcs */
    //                    }
    //                    else
    //                    {
    //                        var value = Rxarray[dtc_start_byte_index + 3] & 0x81; // status 
    //                        var dtctypebits = (Rxarray[dtc_start_byte_index] & 0xC0) >> 6;

    //                        if (dtctypebits == 0x00)
    //                        {
    //                            dtc_type = "P";

    //                        }
    //                        else if (dtctypebits == 0x01)
    //                        {
    //                            dtc_type = "C";
    //                        }
    //                        else if (dtctypebits == 0x02)
    //                        {
    //                            dtc_type = "B";
    //                        }
    //                        else if (dtctypebits == 0x03)
    //                        {
    //                            dtc_type = "U";
    //                        }


    //                        switch (value)
    //                        {
    //                            case 0x00:
    //                                dtcarray[dtcindex, 1] = "Inactive:LampOff";
    //                                break;
    //                            case 0x01:
    //                                dtcarray[dtcindex, 1] = "Active:LampOff";
    //                                break;
    //                            case 0x80:
    //                                dtcarray[dtcindex, 1] = "Inactive:LampOn";
    //                                break;
    //                            case 0x81:
    //                                dtcarray[dtcindex, 1] = "Active:LampOn";
    //                                break;
    //                        }

    //                        switch (DTCIndex)
    //                        {
    //                            case ReadDTCIndex.UDS_3BYTE_DTC:

    //                                dtcarray[dtcindex, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2") + "-" + (Rxarray[dtc_start_byte_index + 2]).ToString("X2"); // correct this syntax
    //                                break;
    //                            case ReadDTCIndex.UDS_2BYTE12_DTC:
    //                            case ReadDTCIndex.UDS_2BYTE_DTC:
    //                                dtcarray[dtcindex, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax
    //                                break;
    //                            case ReadDTCIndex.UDS_2BYTE13_DTC:
    //                                dtcarray[dtcindex, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 2]).ToString("X2"); // correct this syntax
    //                                break;
    //                            case ReadDTCIndex.KWP_2BYTE_DTC:

    //                                dtcarray[dtcindex, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax
    //                                break;
    //                        }
    //                        dtcindex++;
    //                    }
    //                    dtc_start_byte_index += 4;
    //                    i++;
    //                }
    //            }
    //            else
    //            {
    //                return_status = status;
    //            }
    //        }
    //        ReadDtcResponseModel readDtcResponseModel = new ReadDtcResponseModel
    //        {
    //            dtcs = dtcarray,
    //            noofdtc = dtcindex,
    //            status = return_status,
    //        };
    //        return readDtcResponseModel;
    //        //return dtcarray;
    //    }
    //    #endregion

    //    #region Helper
    //    private string ByteArrayToString(byte[] ba)
    //    {
    //        string hex = BitConverter.ToString(ba);
    //        return hex.Replace("-", "");
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

    //    static byte[] HexToBytes(string input)
    //    {
    //        byte[] result = new byte[input.Length / 2];
    //        for (int i = 0; i < result.Length; i++)
    //        {
    //            result[i] = Convert.ToByte(input.Substring(2 * i, 2), 16);
    //        }
    //        return result;
    //    }
    //    #endregion

    //    #region ClearDTC
    //    public async Task<object> ClearDTC(ReadDTCIndex readDtcIndex)
    //    {
    //        DTCIndex = readDtcIndex;
    //        object responseBytes = null;
    //        if (DTCIndex == ReadDTCIndex.UDS_4BYTES)
    //        {
    //            var frameLength = 4;
    //            responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "14FFFFFF");
    //        }
    //        else if (DTCIndex == ReadDTCIndex.UDS_3BYTES)
    //        {
    //            var frameLength = 3;
    //            responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "14FFFF");
    //        }
    //        else if (DTCIndex == ReadDTCIndex.GENERIC_OBD)
    //        {
    //            var frameLength = 1;
    //            responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "04");
    //        }
    //        return responseBytes;
    //    }
    //    #endregion

    //    #region ReadParameters
    //    public async Task<ObservableCollection<ReadParameterResponse>> ReadParameters(int noOFParameters, ObservableCollection<ReadParameterPID> readParameterCollection)
    //    {
    //        ObservableCollection<ReadParameterResponse> databyteArray = new ObservableCollection<ReadParameterResponse>();

    //        for (int i = 0; i < readParameterCollection.Count; i++)
    //        {
    //            var pidItem = readParameterCollection[i];
    //            //Total lenght of command
    //            var frameLength = pidItem.pid.Length / 2;
    //            string dataValue = string.Empty;
    //            //Convert PID TO Bytes
    //            byte[] pid = HexToBytes(pidItem.pid);
    //            Debug.WriteLine("---------------------START LOOP PID NAME-------------------------------------" + pidItem.pidName);

    //            //Send the parameter ID to the ECU
    //            var pidResponse = await dongleCommWin.CAN_TxRx(frameLength, pidItem.pid);
    //            try
    //            {

    //                if (pidResponse.ECUResponseStatus == "NOERROR")
    //                {
    //                    var pidBytesResponseString = ByteArrayToString(pidResponse.ActualDataBytes);
    //                    Debug.WriteLine("Response received = " + pidBytesResponseString);

    //                    var status = pidResponse.ECUResponseStatus;
    //                    var datasArray = pidResponse.ActualDataBytes;
    //                    var inputliveparaarray = readParameterCollection;

    //                    var Rxarray = datasArray;
    //                    double? float_pid_value = 0;


    //                    //var tx_Frame_len = pidResponse.ActualDataBytes.Length;
    //                    var tx_Frame_len = frameLength;
    //                    byte[] outputlivepararray = new byte[tx_Frame_len - 2];
    //                    if (status == "NOERROR")
    //                    {
    //                        Debug.WriteLine("---NOERROR LOOP");
    //                        if (inputliveparaarray[i].datatype == "CONTINUOUS")
    //                        {
    //                            Debug.WriteLine("---START CONTINUOUS--");
    //                            UInt32 unsignedpidintvalue = 0; // take double int data type
    //                            for (int j = 0; j < pidItem.noOfBytes; j++)
    //                            {
    //                                unsignedpidintvalue |= Convert.ToUInt32(Convert.ToUInt32(Rxarray[pidItem.startByte + tx_Frame_len - 1 + j]) << ((pidItem.noOfBytes - 1 - j) * 8));
    //                            }

    //                            if (pidItem.IsBitcoded == true)
    //                            {
    //                                var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
    //                                unsignedpidintvalue = Convert.ToUInt32((unsignedpidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask);
    //                            }


    //                            if (pidItem.readParameterIndex == ReadParameterIndex.UDS_2S_COMPLIMENT)
    //                            {
    //                                var signedpidintvalue = (uint)unsignedpidintvalue;
    //                                float_pid_value = (signedpidintvalue * pidItem.resolution) + pidItem.offset;
    //                            }
    //                            else
    //                            {
    //                                float_pid_value = (unsignedpidintvalue * pidItem.resolution) + pidItem.offset;
    //                            }


    //                            //outputlivepararray[i] = Convert.ToByte(float_pid_value);
    //                            //outputlivepararray[i] = Convert.ToByte(unsignedpidintvalue);

    //                            var TreePlace = Convert.ToDecimal(float_pid_value).ToString("0.000");

    //                            //dataValue = float_pid_value.ToString();
    //                            dataValue = TreePlace;
    //                            Debug.WriteLine("---END CONTINUOUS--");
    //                        }
    //                        else if (inputliveparaarray[i].datatype == "ASCII")
    //                        {
    //                            /* we dont encounter bit coded messages here */
    //                            //outputlivepararray[i] = Rxarray[pidItem.startByte + tx_Frame_len - 2] till inputliveparaarray[i].noofbytes + '\0';
    //                            //22f190 62f19036363636 - "6666"
    //                            //0906   4606363636     - "6666"
    //                            Debug.WriteLine("---START ASCII--");
    //                            Array.Resize(ref outputlivepararray, pidItem.noOfBytes);
    //                            Array.Copy(Rxarray, pidItem.startByte + tx_Frame_len - 1, outputlivepararray, 0, pidItem.noOfBytes);
    //                            dataValue = BytesConverter.HexToASCII(ByteArrayToString(outputlivepararray));
    //                            if (dataValue.Contains('\0') == true)
    //                            {
    //                                dataValue = "NULL";
    //                            }

    //                            Debug.WriteLine("---END ASCII--");
    //                        }
    //                        else if (inputliveparaarray[i].datatype == "BCD")

    //                        {
    //                            /* we dont encounter bitcoded messages here */
    //                            //outputlivepararray[i] = hex2str(Rxarray[startbyte + tx_Frame_len - 2] till inputliveparaarray[i].noofbytes) + '\0';

    //                            //22f190 62f19036363636 - "36363636"
    //                            //0906   4606363636     - ""

    //                            Debug.WriteLine("---START BCD--");
    //                            Array.Resize(ref outputlivepararray, pidItem.noOfBytes);
    //                            Array.Copy(Rxarray, pidItem.startByte + tx_Frame_len - 1, outputlivepararray, 0, pidItem.noOfBytes);
    //                            dataValue = ByteArrayToString(outputlivepararray);
    //                            Debug.WriteLine("---END BCD--");
    //                        }
    //                        else if (inputliveparaarray[i].datatype == "ENUMRATED")

    //                        {
    //                            UInt32 pidintvalue = 0; // take double int data type
    //                            for (int j = 0; j < pidItem.noOfBytes; j++)
    //                            {
    //                                pidintvalue |= Convert.ToUInt32(Convert.ToUInt32(Rxarray[pidItem.startByte + tx_Frame_len - 1 + j]) << ((pidItem.noOfBytes - 1 - j) * 8));
    //                            }

    //                            if (pidItem.IsBitcoded)
    //                            {
    //                                var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
    //                                pidintvalue = Convert.ToUInt32((pidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask);
    //                            }
    //                            dataValue = Convert.ToString(pidintvalue);
    //                            if (inputliveparaarray[i].messages.Count > 0)
    //                            {

    //                                foreach (var item in inputliveparaarray[i].messages)
    //                                {
    //                                    if (Convert.ToUInt32(item.code) == pidintvalue)
    //                                    {
    //                                        dataValue = item.message;
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        else if (inputliveparaarray[i].datatype == "IQA")
    //                        {
    //                            char[] iqa_lookup_x = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8' };
    //                            byte[] rxdata = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    //                            Array.Copy(Rxarray, 3, rxdata, 0, Rxarray.Length - 3);

    //                            byte[] iqa_y = new byte[7];
    //                            byte[] iqa_x = new byte[7];


    //                            iqa_y[0] = (byte)((rxdata[0] & 0xF8) >> 3);
    //                            iqa_y[1] = (byte)((((UInt16)(rxdata[0] & 0x07) << 8) | ((UInt16)(rxdata[1] & 0xC0))) >> 6);
    //                            iqa_y[2] = (byte)((rxdata[1] & 0x3E) >> 1);
    //                            iqa_y[3] = (byte)((((UInt16)(rxdata[1] & 0x01) << 8) | ((UInt16)(rxdata[2] & 0xF0))) >> 4);
    //                            iqa_y[4] = (byte)((((UInt16)(rxdata[2] & 0x0F) << 8) | ((UInt16)(rxdata[3] & 0x80))) >> 7);
    //                            iqa_y[5] = (byte)((rxdata[3] & 0x7C) >> 2);
    //                            iqa_y[6] = (byte)((rxdata[4] & 0xF8) >> 3);

    //                            //convert it to alphanumeric
    //                            iqa_x[0] = Convert.ToByte(iqa_lookup_x[iqa_y[0]]);
    //                            iqa_x[1] = Convert.ToByte(iqa_lookup_x[iqa_y[1]]);
    //                            iqa_x[2] = Convert.ToByte(iqa_lookup_x[iqa_y[2]]);
    //                            iqa_x[3] = Convert.ToByte(iqa_lookup_x[iqa_y[3]]);
    //                            iqa_x[4] = Convert.ToByte(iqa_lookup_x[iqa_y[4]]);
    //                            iqa_x[5] = Convert.ToByte(iqa_lookup_x[iqa_y[5]]);
    //                            iqa_x[6] = Convert.ToByte(iqa_lookup_x[iqa_y[6]]);

    //                            Debug.WriteLine("---START iqa--");
    //                            dataValue = BytesConverter.HexToASCII(ByteArrayToString(iqa_x));
    //                            Debug.WriteLine("---END iqa--");

    //                        }

    //                        var dataItem = new ReadParameterResponse
    //                        {
    //                            Status = pidResponse.ECUResponseStatus,
    //                            DataArray = outputlivepararray,
    //                            pidName = pidItem.pidName,
    //                            pidNumber = pidItem.pidNumber,
    //                            responseValue = dataValue
    //                        };
    //                        databyteArray.Add(dataItem);
    //                    }
    //                    else
    //                    {
    //                        outputlivepararray[i] = 0x00;
    //                        var dataItem = new ReadParameterResponse
    //                        {
    //                            Status = pidResponse.ECUResponseStatus,
    //                            DataArray = outputlivepararray,
    //                            pidName = pidItem.pidName,
    //                            pidNumber = pidItem.pidNumber,
    //                            responseValue = dataValue

    //                        };
    //                        databyteArray.Add(dataItem);
    //                    }
    //                }
    //                else
    //                {
    //                    //outputlivepararray[i] = 0x00;
    //                    var dataItem = new ReadParameterResponse
    //                    {
    //                        Status = pidResponse.ECUResponseStatus,
    //                        //DataArray = outputlivepararray,
    //                        pidName = pidItem.pidName,
    //                        pidNumber = pidItem.pidNumber,
    //                        responseValue = pidResponse.ECUResponseStatus

    //                    };
    //                    databyteArray.Add(dataItem);
    //                }
    //            }
    //            catch (Exception excepption)
    //            {
    //                var dataItem = new ReadParameterResponse
    //                {
    //                    Status = pidResponse.ECUResponseStatus,
    //                    //DataArray = outputlivepararray,
    //                    pidName = pidItem.pidName,
    //                    pidNumber = pidItem.pidNumber,
    //                    responseValue = excepption.Message

    //                };
    //                databyteArray.Add(dataItem);
    //            }
    //            Debug.WriteLine(" END LOOP PID NAME =" + pidItem.pidName);

    //            #region OldImplementation

    //            #endregion

    //        }
    //        return databyteArray;
    //    }

    //    //public async Task<ObservableCollection<ReadParameterResponse>> ReadParameters(int noOFParameters, ObservableCollection<ReadParameterPID> readParameterCollection)
    //    //{
    //    //    ObservableCollection<ReadParameterResponse> databyteArray = new ObservableCollection<ReadParameterResponse>();

    //    //    for (int i = 0; i < readParameterCollection.Count; i++)
    //    //    {
    //    //        var pidItem = readParameterCollection[i];
    //    //        //Total lenght of command
    //    //        var frameLength = pidItem.totalLen;
    //    //        string dataValue = string.Empty;
    //    //        //Convert PID TO Bytes
    //    //        byte[] pid = HexToBytes(pidItem.pid);
    //    //        Debug.WriteLine(" START LOOP PID NAME =" + pidItem.pidName);

    //    //        //Send the parameter ID to the ECU
    //    //        var pidResponse = await dongleCommWin.CAN_TxRx(frameLength, pidItem.pid);
    //    //        try
    //    //        {

    //    //            if (pidResponse.ECUResponseStatus == "NOERROR")
    //    //            {
    //    //                var pidBytesResponseString = ByteArrayToString(pidResponse.ActualDataBytes);
    //    //                Debug.WriteLine("Response received = " + pidBytesResponseString);

    //    //                var status = pidResponse.ECUResponseStatus;
    //    //                var datasArray = pidResponse.ActualDataBytes;
    //    //                var inputliveparaarray = readParameterCollection;

    //    //                var Rxarray = datasArray;
    //    //                double? float_pid_value = 0;


    //    //                //var tx_Frame_len = pidResponse.ActualDataBytes.Length;
    //    //                var tx_Frame_len = frameLength;
    //    //                byte[] outputlivepararray = new byte[tx_Frame_len - 2];
    //    //                if (status == "NOERROR")
    //    //                {
    //    //                    Debug.WriteLine("---NOERROR LOOP");
    //    //                    if (inputliveparaarray[i].datatype == "CONTINUOUS")
    //    //                    {
    //    //                        Debug.WriteLine("---START CONTINUOUS--");
    //    //                        int unsignedpidintvalue = 0; // take double int data type
    //    //                        for (int j = 0; j < pidItem.noOfBytes; j++)
    //    //                        {
    //    //                            unsignedpidintvalue |= Rxarray[pidItem.startByte + tx_Frame_len - j] << (j * 8);
    //    //                        }

    //    //                        if (pidItem.IsBitcoded == true)
    //    //                        {
    //    //                            var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
    //    //                            unsignedpidintvalue = (unsignedpidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask;
    //    //                        }


    //    //                        if (pidItem.readParameterIndex == ReadParameterIndex.UDS_2S_COMPLIMENT)
    //    //                        {
    //    //                            var signedpidintvalue = (uint)unsignedpidintvalue;
    //    //                            float_pid_value = (signedpidintvalue * pidItem.resolution) + pidItem.offset;
    //    //                        }
    //    //                        else
    //    //                        {
    //    //                            float_pid_value = (unsignedpidintvalue * pidItem.resolution) + pidItem.offset;
    //    //                        }


    //    //                        //outputlivepararray[i] = Convert.ToByte(float_pid_value);
    //    //                        //outputlivepararray[i] = Convert.ToByte(unsignedpidintvalue);
    //    //                        dataValue = float_pid_value.ToString();
    //    //                        Debug.WriteLine("---END CONTINUOUS--");
    //    //                    }
    //    //                    else if (inputliveparaarray[i].datatype == "ASCII")

    //    //                    {
    //    //                        /* we dont encounter bit coded messages here */
    //    //                        //outputlivepararray[i] = Rxarray[pidItem.startByte + tx_Frame_len - 2] till inputliveparaarray[i].noofbytes + '\0';
    //    //                        //22f190 62f19036363636 - "6666"
    //    //                        //0906   4606363636     - "6666"
    //    //                        Debug.WriteLine("---START ASCII--");
    //    //                        Array.Copy(Rxarray, pidItem.startByte, outputlivepararray, 0, tx_Frame_len - 2);
    //    //                        dataValue = BytesConverter.HexToASCII(ByteArrayToString(outputlivepararray));
    //    //                        Debug.WriteLine("---END ASCII--");
    //    //                    }
    //    //                    else if (inputliveparaarray[i].datatype == "BCD")

    //    //                    {
    //    //                        /* we dont encounter bitcoded messages here */
    //    //                        //outputlivepararray[i] = hex2str(Rxarray[startbyte + tx_Frame_len - 2] till inputliveparaarray[i].noofbytes) + '\0';

    //    //                        //22f190 62f19036363636 - "36363636"
    //    //                        //0906   4606363636     - ""

    //    //                        Debug.WriteLine("---START BCD--");
    //    //                        Array.Copy(Rxarray, pidItem.startByte, outputlivepararray, 0, tx_Frame_len - 2);
    //    //                        dataValue = ByteArrayToString(outputlivepararray);
    //    //                        Debug.WriteLine("---END BCD--");
    //    //                    }
    //    //                    else if (inputliveparaarray[i].datatype == "ENUMERATED")

    //    //                    {
    //    //                        var pidintvalue = 0; // take double int data type
    //    //                        for (int j = 0; j < pidItem.noOfBytes; j++)
    //    //                        {
    //    //                            pidintvalue |= Rxarray[pidItem.startByte + tx_Frame_len - 2] << ((pidItem.noOfBytes - 1) * 8);
    //    //                        }

    //    //                        if (pidItem.IsBitcoded)
    //    //                        {
    //    //                            var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
    //    //                            pidintvalue = (pidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask;
    //    //                        }

    //    //                        //outputlivepararray[i] = hex2str(pidintvalue); // If enumeration related to hex2str(pidintvalue) not found, then it will return the hex value itself
    //    //                        //for (int j = 0; j < noorrowsinenumstruct; j++) // u will have to compute the number of rows in enumoptions structure 
    //    //                        //{
    //    //                        //    if (outputlivepararray[i] == enumoptions[j][1])
    //    //                        //        outputlivepararray[i] = enumoptions[j][2]; // assign the enum value here
    //    //                        //}
    //    //                    }
    //    //                    else if (inputliveparaarray[i].datatype == "GREAVES_BOSCH_BS6_IQA")

    //    //                    {
    //    //                        /*SPECIAL LOGIC - WILL TAKE CARE OF THIS LATER */
    //    //                    }
    //    //                    else if (inputliveparaarray[i].datatype == "IQA")
    //    //                    {
    //    //                        char[] iqa_lookup_x = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8' };
    //    //                        byte[] rxdata = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    //    //                        Array.Copy(Rxarray, 3, rxdata, 0, Rxarray.Length - 3);

    //    //                        byte[] iqa_y = new byte[7];
    //    //                        byte[] iqa_x = new byte[7];

    //    //                        iqa_y[0] = Convert.ToByte(rxdata[0] >> 3);
    //    //                        iqa_y[1] = Convert.ToByte(rxdata[1] >> 3);
    //    //                        iqa_y[2] = Convert.ToByte(rxdata[2] >> 3);
    //    //                        iqa_y[3] = Convert.ToByte(rxdata[3] >> 3);
    //    //                        iqa_y[4] = Convert.ToByte(rxdata[4] >> 3);
    //    //                        iqa_y[5] = Convert.ToByte(rxdata[5] >> 3);
    //    //                        iqa_y[6] = Convert.ToByte(rxdata[7] >> 3);

    //    //                        //convert it to alphanumeric
    //    //                        iqa_x[0] = Convert.ToByte(iqa_lookup_x[iqa_y[0]]);
    //    //                        iqa_x[1] = Convert.ToByte(iqa_lookup_x[iqa_y[1]]);
    //    //                        iqa_x[2] = Convert.ToByte(iqa_lookup_x[iqa_y[2]]);
    //    //                        iqa_x[3] = Convert.ToByte(iqa_lookup_x[iqa_y[3]]);
    //    //                        iqa_x[4] = Convert.ToByte(iqa_lookup_x[iqa_y[4]]);
    //    //                        iqa_x[5] = Convert.ToByte(iqa_lookup_x[iqa_y[5]]);
    //    //                        iqa_x[6] = Convert.ToByte(iqa_lookup_x[iqa_y[6]]);
    //    //                        //iqa_x[7] = Convert.ToByte(iqa_lookup_x[iqa_y[7]]);
    //    //                        /*SPECIAL LOGIC - WILL TAKE CARE OF THIS LATER */

    //    //                        Debug.WriteLine("---START ASCII--");
    //    //                        outputlivepararray = Rxarray;
    //    //                        dataValue = BytesConverter.HexToASCII(ByteArrayToString(iqa_x));
    //    //                        Debug.WriteLine("---END ASCII--");


    //    //                        ////string iqainput = "DBG64N";
    //    //                        ////UInt32[] iqainputlookup = new UInt32[7];
    //    //                        ////UInt32 ecuinpuths = 0, ecuinputls = 0;

    //    //                        ////for (i = 0; i < 7; i++)
    //    //                        ////{
    //    //                        ////    for (int j = 0; j < 32; j++)
    //    //                        ////    {
    //    //                        ////        if (iqainput[i] == iqa_lookup_x[j])
    //    //                        ////        {
    //    //                        ////            iqainputlookup[i] = Convert.ToByte(j);
    //    //                        ////        }
    //    //                        ////    }


    //    //                        ////    ecuinpuths = ((iqainputlookup[0] & 0x0000001F) << 27) | ((iqainputlookup[1] & 0x0000001F) << 22) | ((iqainputlookup[2] & 0x0000001F) << 17) | ((iqainputlookup[3] & 0x0000001F) << 12) | ((iqainputlookup[4] & 0x0000001F) << 7) | ((iqainputlookup[5] & 0x0000001F) << 2);
    //    //                        ////    ecuinputls = ((iqainputlookup[6] & 0x0000001F) << 27);

    //    //                        ////    byte[] tx = new byte[10];
    //    //                        ////    tx[0] = 0x2E;
    //    //                        ////    tx[1] = 0x02;
    //    //                        ////    tx[2] = 0x50;
    //    //                        ////    tx[3] = Convert.ToByte((ecuinpuths & 0xFF000000) >> 24);
    //    //                        ////    tx[4] = Convert.ToByte((ecuinpuths & 0x00FF0000) >> 16);
    //    //                        ////    tx[5] = Convert.ToByte((ecuinpuths & 0x0000FF00) >> 8);
    //    //                        ////    tx[6] = Convert.ToByte((ecuinpuths & 0x000000FF));
    //    //                        ////    tx[7] = Convert.ToByte((ecuinputls & 0xFF000000) >> 24);
    //    //                        ////    tx[8] = Convert.ToByte((ecuinputls & 0x00FF0000) >> 16);
    //    //                        ////    tx[9] = Convert.ToByte((ecuinputls & 0x0000FF00) >> 8);
    //    //                        ////    tx[10] = Convert.ToByte((ecuinputls & 0x000000FF));




    //    //                    }

    //    //                    var dataItem = new ReadParameterResponse
    //    //                    {
    //    //                        Status = pidResponse.ECUResponseStatus,
    //    //                        DataArray = outputlivepararray,
    //    //                        pidName = pidItem.pidName,
    //    //                        pidNumber = pidItem.pidNumber,
    //    //                        responseValue = dataValue


    //    //                    };
    //    //                    databyteArray.Add(dataItem);
    //    //                }
    //    //                else
    //    //                {
    //    //                    outputlivepararray[i] = 0x00;
    //    //                    var dataItem = new ReadParameterResponse
    //    //                    {
    //    //                        Status = pidResponse.ECUResponseStatus,
    //    //                        DataArray = outputlivepararray,
    //    //                        pidName = pidItem.pidName,
    //    //                        pidNumber = pidItem.pidNumber,
    //    //                        responseValue = dataValue

    //    //                    };
    //    //                    databyteArray.Add(dataItem);
    //    //                }
    //    //            }
    //    //            else
    //    //            {
    //    //                //outputlivepararray[i] = 0x00;
    //    //                var dataItem = new ReadParameterResponse
    //    //                {
    //    //                    Status = pidResponse.ECUResponseStatus,
    //    //                    //DataArray = outputlivepararray,
    //    //                    pidName = pidItem.pidName,
    //    //                    pidNumber = pidItem.pidNumber,
    //    //                    responseValue = pidResponse.ECUResponseStatus

    //    //                };
    //    //                databyteArray.Add(dataItem);
    //    //            }
    //    //        }
    //    //        catch (Exception excepption)
    //    //        {
    //    //            var dataItem = new ReadParameterResponse
    //    //            {
    //    //                Status = pidResponse.ECUResponseStatus,
    //    //                //DataArray = outputlivepararray,
    //    //                pidName = pidItem.pidName,
    //    //                pidNumber = pidItem.pidNumber,
    //    //                responseValue = excepption.Message

    //    //            };
    //    //            databyteArray.Add(dataItem);
    //    //        }
    //    //        Debug.WriteLine(" END LOOP PID NAME =" + pidItem.pidName);

    //    //        #region OldImplementation
    //    //        //while (pidBytesResponse[7] == 0x7f)
    //    //        //{
    //    //        //    //pidResponse = await dongleCommWin.ReadData();
    //    //        //    //pidBytesResponse = (byte[])pidResponse;
    //    //        //}

    //    //        //Check if the 1st Byte is 62 and the total byte length of the response is total byes +3 

    //    //        //OLD func
    //    //        //if (dataArray != null)
    //    //        //{
    //    //        //    //Create an object of data array of the actual data in the response received from the ECU
    //    //        //    //var actualResponseDataByteArray = new byte[pidItem.totalBytes];

    //    //        //    //Copy those data bytes into the actual response data array
    //    //        //    //Array.Copy(pidBytesResponse, 7 + frameLength, actualResponseDataByteArray, 0, pidItem.noOfBytes);

    //    //        //    //Check if the PID is NOT BitEncoded
    //    //        //    if (!pidItem.IsBitcoded)
    //    //        //    {
    //    //        //        //Create a object for the data byte array
    //    //        //        //var dataByteArray = new byte[pidItem.noOfBytes];

    //    //        //        //Copy the data bytes to the data byte array
    //    //        //        //Array.Copy(actualResponseDataByteArray, pidItem.startByte, dataByteArray, 0, pidItem.noOfBytes);

    //    //        //        //Add the bytes array to the collection

    //    //        //        var dataItem = new ReadParameterResponse
    //    //        //        {
    //    //        //            Status = dataStatus,
    //    //        //            DataArray = dataArray
    //    //        //        };
    //    //        //        databyteArray.Add(dataItem);
    //    //        //    }
    //    //        //    //Check if the PID is BitEncoded
    //    //        //    else if (pidItem.IsBitcoded)
    //    //        //    {
    //    //        //        // Create an object for the bit array
    //    //        //        var dataBitArrayResponse = new byte[pidItem.noofBits];

    //    //        //        //Copy the designated byte into the dataBitArray
    //    //        //        Array.Copy(dataArray, pidItem.startByte, dataBitArrayResponse, 0, pidItem.noOfBytes);

    //    //        //        //Get the hex string of the byte
    //    //        //        var hexString = BitConverter.ToString(dataBitArrayResponse);

    //    //        //        //Get the eight bit binary string
    //    //        //        string binarystring = String.Join("0000", hexString.Select(
    //    //        //            number => Convert.ToString(Convert.ToInt32(number.ToString(), 16), 2).PadLeft(4, '0')
    //    //        //            ));


    //    //        //        var actualNumberArray = "00000000".ToCharArray();
    //    //        //        var binarystringArray = binarystring.ToCharArray();

    //    //        //        //Copy the initial array to the binary array
    //    //        //        Array.Copy(binarystringArray, pidItem.startBit, actualNumberArray, 0, pidItem.noofBits);

    //    //        //        string binaryStr = actualNumberArray.ToString();

    //    //        //        //convert the strings to byte
    //    //        //        var byteArray = Enumerable.Range(0, int.MaxValue / 8)
    //    //        //                                  .Select(i => i * 8)    // get the starting index of which char segment
    //    //        //                                  .TakeWhile(i => i < binaryStr.Length)
    //    //        //                                  .Select(i => binaryStr.Substring(i, 8)) // get the binary string segments
    //    //        //                                  .Select(s => Convert.ToByte(s, 2)) // convert to byte
    //    //        //                                  .ToArray();

    //    //        //        var dataItem = new ReadParameterResponse
    //    //        //        {
    //    //        //            Status = dataStatus,
    //    //        //            DataArray = byteArray
    //    //        //        };
    //    //        //        databyteArray.Add(dataItem);
    //    //        //    }
    //    //        //}
    //    //        //else
    //    //        //{
    //    //        //    var dataItem = new ReadParameterResponse
    //    //        //    {
    //    //        //        Status = dataStatus,
    //    //        //        DataArray = pidBytesResponse
    //    //        //    };
    //    //        //    databyteArray.Add(dataItem);
    //    //        //}
    //    //        #endregion

    //    //    }
    //    //    return databyteArray;
    //    //}

    //    #endregion

    //    #region WriteParameters
    //    public async Task<ObservableCollection<WriteParameterResponse>> WriteParameters(int noOFParameters, WriteParameterIndex writeParameterIndex, ObservableCollection<WriteParameterPID> writeParameterCollection)
    //    {
    //        ObservableCollection<WriteParameterResponse> WriteParameterCollection = new ObservableCollection<WriteParameterResponse>();
    //        foreach (var pidItem in writeParameterCollection)
    //        {
    //            var writeParamIndex = writeParameterIndex;
    //            byte diagnosticsmode = 0x00;
    //            byte getseedindex = 0x00;

    //            if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK090A)
    //            {
    //                /* send 10 03, followed by 27 09, 27 0A */
    //                diagnosticsmode = 0x03;
    //                getseedindex = 0x09;

    //            }
    //            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0102)
    //            {
    //                /* send 10 03, followed by 27 01, 27 02 */
    //                diagnosticsmode = 0x03;
    //                getseedindex = 0x01;
    //            }
    //            else if (writeParamIndex == WriteParameterIndex.UDS_DS1003_SK0B0C)
    //            {
    //                /* send 10 03, followed by 27 0B, 27 0C */
    //                diagnosticsmode = 0x03;
    //                getseedindex = 0x0B;
    //            }

    //            byte[] TxFrame = new byte[2];
    //            /* Send start diagnostics mode command */
    //            TxFrame[0] = 0x10;
    //            TxFrame[1] = diagnosticsmode;
    //            int frameLength = 2;

    //            //Send the parameter ID to the ECU
    //            var pidResponse = await dongleCommWin.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
    //            string return_status = string.Empty;

    //            if (pidResponse.ECUResponseStatus == "NOERROR")
    //            {
    //                /* Send get seed command to ECU */
    //                TxFrame[0] = 0x27;
    //                TxFrame[1] = getseedindex;
    //                var tx_Frame_len = 2;
    //                var pidResponsebytes = await dongleCommWin.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
    //                var status = pidResponsebytes.ECUResponseStatus;

    //                if (status == "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED")
    //                {
    //                    Thread.Sleep(11000);

    //                    pidResponsebytes = await dongleCommWin.CAN_TxRx(frameLength, ByteArrayToString(TxFrame));
    //                    status = pidResponsebytes.ECUResponseStatus;
    //                }

    //                if (status == "NOERROR")
    //                {
    //                    var datasArrays = pidResponsebytes.ActualDataBytes;
    //                    var Rxarray = pidResponsebytes.ActualDataBytes;
    //                    var Rxsize = Rxarray.Length;
    //                    int seedkeyindex = getseedindex;

    //                    var seedarray = new byte[Rxsize - 2];
    //                    /* get seed from the response and send it to the seedkey dll to get the key */
    //                    //for (int i = 0; i < Rxsize - 2; i++)
    //                    //{
    //                    //    seedarray[i] = Rxarray[i + 2];
    //                    //}
    //                    Array.Copy(Rxarray, 2, seedarray, 0, Rxsize - 2);

    //                    //status = getkeyfromseed(seedkeyindex, seedsize, seedarray, &keysize, keyarray);

    //                    byte numkeybytes = new byte();

    //                    byte[] actualKey = new byte[4];
    //                    calculateSeedkey = new ECUCalculateSeedkey();
    //                    Byte seedkeylen = 0;
    //                    if ((pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD) ||
    //                        (pidItem.seedkeyindex == Enums.SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_DEV))
    //                    {
    //                        seedkeylen = 4;
    //                    }
    //                    else
    //                    {
    //                        seedkeylen = 16;
    //                    }

    //                    var result = calculateSeedkey.CalculateSeedkey((SEEDKEYINDEXTYPE)pidItem.seedkeyindex, seedkeylen, ref numkeybytes, seedarray, ref actualKey);

    //                    if (status == "NOERROR")
    //                    {
    //                        byte[] newTxFrame = new byte[actualKey.Length + 2];
    //                        /* Send calculated key to ECU */
    //                        newTxFrame[0] = 0x27;
    //                        newTxFrame[1] = Convert.ToByte(seedkeyindex + 1);

    //                        Array.Copy(actualKey, 0, newTxFrame, 2, actualKey.Length);

    //                        tx_Frame_len = Rxsize;
    //                        frameLength = newTxFrame.Length;

    //                        //Send the parameter ID to the ECU
    //                        var pidResponse3 = await dongleCommWin.CAN_TxRx(frameLength, ByteArrayToString(newTxFrame));

    //                        var writeParaPID = HexStringToByteArray(pidItem.writeparapid);
    //                        var writeparaframesize = pidItem.writeparadata.Length;

    //                        if (pidResponse3.ECUResponseStatus == "NOERROR")
    //                        {
    //                            byte[] writeFrame = new byte[1 + writeParaPID.Length + pidItem.totalBytes];
    //                            /* write new value to ECU */
    //                            writeFrame[0] = 0x2E;

    //                            Array.Copy(writeParaPID, 0, writeFrame, 1, writeParaPID.Length);

    //                            if (pidItem.ReadParameterPID_DataType == "IQA")
    //                            {
    //                                byte[] txdata = new byte[8];
    //                                Array.Resize(ref writeFrame, 1 + writeParaPID.Length + 8);
    //                                tx_Frame_len = 1 + writeParaPID.Length + 8;
    //                                char[] iqa_lookup_x = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8' };
    //                                byte[] iqa_y = new byte[7];
    //                                byte[] iqa_x = new byte[8];
    //                                UInt32 temp = 0;

    //                                for (byte i = 0; i < 7; i++)
    //                                {
    //                                    for (byte j = 0; j < 32; j++)
    //                                    {
    //                                        if (iqa_lookup_x[j] == Char.ToUpper((char)pidItem.writeparadata[i]))
    //                                        {
    //                                            iqa_y[i] = j;
    //                                            break;
    //                                        }
    //                                    }
    //                                }

    //                                temp = (((UInt32)iqa_y[0] & 0x0000001F) << 27) |
    //                                       (((UInt32)iqa_y[1] & 0x0000001F) << 22) |
    //                                       (((UInt32)iqa_y[2] & 0x0000001F) << 17) |
    //                                       (((UInt32)iqa_y[3] & 0x0000001F) << 12) |
    //                                       (((UInt32)iqa_y[4] & 0x0000001F) << 7) |
    //                                       (((UInt32)iqa_y[5] & 0x0000001F) << 2);

    //                                iqa_x[0] = (byte)((temp & 0xFF000000) >> 24);
    //                                iqa_x[1] = (byte)((temp & 0x00FF0000) >> 16);
    //                                iqa_x[2] = (byte)((temp & 0x0000FF00) >> 8);
    //                                iqa_x[3] = (byte)((temp & 0x000000FF));
    //                                iqa_x[4] = (byte)(iqa_y[6] << 3);
    //                                iqa_x[5] = 0;
    //                                iqa_x[6] = 0;
    //                                iqa_x[7] = 0;

    //                                Array.Copy(iqa_x, 0, writeFrame, writeParaPID.Length + pidItem.startByte, 8);
    //                            }
    //                            else
    //                            {
    //                                tx_Frame_len = 1 + writeParaPID.Length + pidItem.totalBytes;
    //                                Array.Copy(pidItem.writeparadata, 0, writeFrame, writeParaPID.Length + pidItem.startByte, pidItem.writeparadata.Length);
    //                            }

    //                            var pidResponse4 = await dongleCommWin.CAN_TxRx(writeFrame.Length, ByteArrayToString(writeFrame));

    //                            status = pidResponse4.ECUResponseStatus;

    //                            if (status == "NOERROR")
    //                            {
    //                                return_status = "NOERROR";

    //                            }
    //                            else
    //                            {
    //                                return_status = status;
    //                            }
    //                            var dataItem = new WriteParameterResponse
    //                            {
    //                                Status = pidResponse4.ECUResponseStatus,
    //                                DataArray = pidResponse4.ActualDataBytes,
    //                                pidName = pidItem.writeparapid,
    //                                pidNumber = pidItem.writeparano,

    //                                responseValue = pidResponse4.ECUResponseStatus

    //                            };
    //                            WriteParameterCollection.Add(dataItem);
    //                        }
    //                        else
    //                        {
    //                            //key issue
    //                            return_status = status;
    //                            var dataItem = new WriteParameterResponse
    //                            {
    //                                Status = pidResponse3.ECUResponseStatus,
    //                                DataArray = pidResponse3.ActualDataBytes,
    //                                pidName = pidItem.writeparapid,
    //                                pidNumber = pidItem.writeparano,

    //                                responseValue = pidResponse3.ECUResponseStatus

    //                            };
    //                            WriteParameterCollection.Add(dataItem);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        return_status = status;
    //                        var dataItem = new WriteParameterResponse
    //                        {
    //                            Status = pidResponse.ECUResponseStatus,
    //                            DataArray = pidResponse.ActualDataBytes,
    //                            pidName = pidItem.writeparapid,
    //                            pidNumber = pidItem.writeparano,

    //                            responseValue = pidResponse.ECUResponseStatus

    //                        };
    //                        WriteParameterCollection.Add(dataItem);
    //                    }
    //                }
    //                else
    //                {
    //                    return_status = status;
    //                    var dataItem = new WriteParameterResponse
    //                    {
    //                        Status = pidResponsebytes.ECUResponseStatus,
    //                        DataArray = pidResponsebytes.ActualDataBytes,
    //                        pidName = pidItem.writeparapid,
    //                        pidNumber = pidItem.writeparano,

    //                        responseValue = pidResponsebytes.ECUResponseStatus

    //                    };
    //                    WriteParameterCollection.Add(dataItem);
    //                }
    //            }
    //            else
    //            {
    //                return_status = pidResponse.ECUResponseStatus;
    //                var dataItem = new WriteParameterResponse
    //                {
    //                    Status = pidResponse.ECUResponseStatus,
    //                    DataArray = pidResponse.ActualDataBytes,
    //                    pidName = pidItem.writeparapid,
    //                    pidNumber = pidItem.writeparano,

    //                    responseValue = pidResponse.ECUResponseStatus

    //                };
    //                WriteParameterCollection.Add(dataItem);

    //            }
    //        }



    //        return WriteParameterCollection;

    //    }

    //    public async Task<string> StartFlashSSS(string abcd)
    //    {
    //        return "";
    //    }

    //    #endregion

    //    #region StartFlash
    //    public static UInt32 realtimebytesflashed = 0;
    //    public static UInt32 totalbytestobeflashed = 0;
    //    public async Task<float> GetRuntimeFlashPercent()
    //    {
    //        float runtimeflashpercent = (float)realtimebytesflashed / (float)totalbytestobeflashed;
    //        return runtimeflashpercent;
    //    }

    //    //public async Task<string> StartFlash(flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata)
    //    //{

    //    //    int Frame_len = 0;
    //    //    byte[] TxFrame = new byte[2];
    //    //    TxFrame[Frame_len++] = 0x10;
    //    //    TxFrame[Frame_len++] = 0x02;

    //    //    Debug.WriteLine("-------switch to reprogramming mode-------");
    //    //    var reprogrammingResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
    //    //    if (reprogrammingResponse?.ECUResponseStatus != "NOERROR")
    //    //    {
    //    //        Debug.WriteLine("--------- reprogrammingResponse ERROR------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
    //    //        Debug.WriteLine("---------reprogrammingResponse LOOP END------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
    //    //        return "reprogrammingResponse" + reprogrammingResponse.ECUResponseStatus;
    //    //    }


    //    //    // get Seed - 27 09
    //    //    Frame_len = 0;
    //    //    TxFrame[Frame_len++] = 0x27;
    //    //    TxFrame[Frame_len++] = flashconfig_data.sendseedbyte;

    //    //    Debug.WriteLine("-------get Seed-------");

    //    //    var getSeedResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

    //    //    if (getSeedResponse?.ECUResponseStatus != "NOERROR")
    //    //    {
    //    //        Debug.WriteLine("--------- getSeedResponse ERROR------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
    //    //        Debug.WriteLine("---------getSeedResponse LOOP END------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
    //    //        return "getSeedResponse" + getSeedResponse.ECUResponseStatus;
    //    //    }

    //    //    var seedarray = new byte[flashconfig_data.seedkeynumbytes];
    //    //    byte numkeybytes = new byte();

    //    //    Array.Copy(getSeedResponse.ActualDataBytes, 2, seedarray, 0, getSeedResponse.ActualDataBytes.Length - 2);

    //    //    //compute key for the seed received
    //    //    //status = getkeyfromseed(flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, Rxarray + 2, keyarray);

    //    //    byte[] actualKey = new byte[flashconfig_data.seedkeynumbytes];
    //    //    calculateSeedkey = new ECUCalculateSeedkey();
    //    //    Debug.WriteLine("-------get key-------");
    //    //    //var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEX_TYPE.GREAVES_ADVANTEK_A46_BS6, 4, ref numkeybytes, seedarray, ref actualKey);
    //    //    var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_BS6_A46, 4, ref numkeybytes, seedarray, ref actualKey);

    //    //    Array.Resize(ref TxFrame, flashconfig_data.seedkeynumbytes + 2);//
    //    //    //send key 27 0A
    //    //    Frame_len = 0;
    //    //    TxFrame[Frame_len++] = 0x27;
    //    //    TxFrame[Frame_len++] = (byte)(flashconfig_data.sendseedbyte + 1);
    //    //    for (int i = 0; i < actualKey.Length; i++)
    //    //    {
    //    //        TxFrame[Frame_len++] = actualKey[i];
    //    //    }
    //    //    //status = ISO15765_CANTxRx(channel, Frame_len, TxFrame, &Rxsize, Rxarray);
    //    //    var sendKeyResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

    //    //    if (sendKeyResponse?.ECUResponseStatus != "NOERROR")
    //    //    {
    //    //        Debug.WriteLine("--------- sendKeyResponse ERROR------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
    //    //        Debug.WriteLine("---------sendKeyResponse LOOP END------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
    //    //        return "sendKeyResponse " + sendKeyResponse.ECUResponseStatus;
    //    //    }

    //    //    // Erase Memory
    //    //    Frame_len = 0;
    //    //    TxFrame[Frame_len++] = 0x31;
    //    //    TxFrame[Frame_len++] = 0x01;
    //    //    TxFrame[Frame_len++] = 0xFF;
    //    //    TxFrame[Frame_len++] = 0x00;

    //    //    Array.Resize(ref TxFrame, 12);

    //    //    TxFrame[Frame_len++] = 0x00;
    //    //    TxFrame[Frame_len++] = 0x02;
    //    //    TxFrame[Frame_len++] = 0x00;
    //    //    TxFrame[Frame_len++] = 0x00;

    //    //    TxFrame[Frame_len++] = 0x00;
    //    //    TxFrame[Frame_len++] = 0x0F;
    //    //    TxFrame[Frame_len++] = 0xFF;
    //    //    TxFrame[Frame_len++] = 0xFF;


    //    //    var eraseMemoryResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

    //    //    if (eraseMemoryResponse?.ECUResponseStatus != "NOERROR")
    //    //    {
    //    //        Debug.WriteLine("--------- eraseMemoryResponse ERROR------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
    //    //        Debug.WriteLine("---------eraseMemoryResponse LOOP END------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
    //    //        return "eraseMemoryResponse " + eraseMemoryResponse.ECUResponseStatus;
    //    //    }

    //    //    //for (int m = 0; m <= 1000; m++)
    //    //    //{
    //    //    //    var ftt = HexStringToByteArray("3601000000000000000000000000000000005555AAAA00000000000000000000000000000000000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF000000000000000020FE60FFA000E001200360040005A0054006800700400040004000400040004000400040004000402003B00440060807D00798086009F00A800C100EF100830113025F02A602ED023303CB034704BE040008000A000CCC0D001000140018001C001E00207805780578057805780578057805780578057805EE02EE02EE02EE02EE02EE02EE02EE02EE02EE02C201C201C201C201C201C201C201C201C201C20100000000000000000000000000000000000000006AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF4006D0076009F00A800CA00FC012E0157017401F20FE60FFA000E001200360040005A00540068007C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C8002C012C012C012C012C012C012C012C012C012C01DC01DC01DC01DC01DC01DC01DC01DC01DC01DC0158025802580258025802580258025802580258029C029C029C029C029C029C029C029C029C029C02250325032503250325032503250325032503250384038403840384038403840384038403840384038403840384038403840384038403840384038403002000240028002C003000340038003A003C00400040083F083B2937CD340E2F73279822971C0800000090012003B0044006D0076009F00A800C480DD80E6810C0125014E0157017E803780508076009F00A800CA00F941188137C1570176419581B401F1027E02E004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040");
    //    //    //    var nsbyts = new byte[944];

    //    //    //    var pidResponses = await dongleCommWin.CAN_TxRx(ftt.Length, ByteArrayToString(ftt));
    //    //    //}

    //    //    var addrdataformat = flashconfig_data.addrdataformat;
    //    //    for (int i = 0; i < noofsectors; i++)
    //    //    {
    //    //        var sector_start_address = Convert.ToUInt32(sectordata[i].JsonStartAddress, 16);
    //    //        var sector_end_address = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16);
    //    //        var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;

    //    //        var ecu_memmap_start_address = Convert.ToUInt32(sectordata[i].ECUMemMapStartAddress, 16);
    //    //        var ecu_memmap_end_address = Convert.ToUInt32(sectordata[i].ECUMemMapEndAddress, 16);

    //    //        //// Erase Memory
    //    //        //Frame_len = 0;
    //    //        //TxFrame[Frame_len++] = 0x31;
    //    //        //TxFrame[Frame_len++] = 0x01;
    //    //        //TxFrame[Frame_len++] = 0xFF;
    //    //        //TxFrame[Frame_len++] = 0x00;

    //    //        //if ((flashconfig_data.addrdataformat & 0xF0) == 0x30)
    //    //        //{
    //    //        //    Array.Resize(ref TxFrame, 10);

    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF0000) >> 16);
    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF00) >> 8);
    //    //        //    TxFrame[Frame_len++] = (byte)(ecu_memmap_start_address & 0x000000FF);

    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF0000) >> 16);
    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF00) >> 8);
    //    //        //    TxFrame[Frame_len++] = (byte)(ecu_memmap_end_address & 0x000000FF);

    //    //        //}
    //    //        //else
    //    //        //{
    //    //        //    Array.Resize(ref TxFrame, 12);

    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0xFF000000) >> 24);
    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF0000) >> 16);
    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF00) >> 8);
    //    //        //    TxFrame[Frame_len++] = (byte)(ecu_memmap_start_address & 0x000000FF);

    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0xFF000000) >> 24);
    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF0000) >> 16);
    //    //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF00) >> 8);
    //    //        //    TxFrame[Frame_len++] = (byte)(ecu_memmap_end_address & 0x000000FF);

    //    //        //}

    //    //        //pidResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
    //    //        // Request download
    //    //        Frame_len = 0;
    //    //        TxFrame[Frame_len++] = 0x34;
    //    //        TxFrame[Frame_len++] = 0x00;
    //    //        TxFrame[Frame_len++] = flashconfig_data.addrdataformat;

    //    //        if (addrdataformat == 0x33)
    //    //        {
    //    //            Array.Resize(ref TxFrame, 9);
    //    //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
    //    //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
    //    //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

    //    //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
    //    //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
    //    //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

    //    //        }
    //    //        else if (addrdataformat == 0x44) // addrdataformat == 0x44
    //    //        {
    //    //            Array.Resize(ref TxFrame, 11);
    //    //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
    //    //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
    //    //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
    //    //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

    //    //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0xFF000000) >> 24);
    //    //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
    //    //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
    //    //            TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

    //    //        }


    //    //        var memoryResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
    //    //        if (memoryResponse?.ECUResponseStatus != "NOERROR")
    //    //        {
    //    //            Debug.WriteLine("--------- memoryResponse ERROR------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
    //    //            Debug.WriteLine("---------memoryResponse LOOP END------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
    //    //            return "memoryResponse " + memoryResponse.ECUResponseStatus;
    //    //        }

    //    //        UInt32 bytestranferred = 0;
    //    //        // Transfer Data in this sector
    //    //        int blkseqcnt = 1;
    //    //        var SectorDataArray = HexStringToByteArray(sectordata[i].JsonData);
    //    //        for (int j = 0; j < sector_num_bytes;)
    //    //        {
    //    //            try
    //    //            {
    //    //                if (j == 41076)
    //    //                {

    //    //                }
    //    //                var NTxFrame = new byte[2];
    //    //                NTxFrame[0] = 0x36;
    //    //                NTxFrame[1] = (byte)(blkseqcnt & 0xFF);
    //    //                //var currenttransferlen = Math.Min(sector_num_bytes - bytestranferred, flashconfig_data.sectorframetransferlen );
    //    //                var currenttransferlen = Math.Min(sector_num_bytes - j, 944);

    //    //                Debug.WriteLine("currenttransferlen==" + currenttransferlen + 2, "ELMZ");

    //    //                Array.Resize(ref NTxFrame, Convert.ToInt32(currenttransferlen) + 2);

    //    //                Array.Copy(SectorDataArray, j, NTxFrame, 2, currenttransferlen);
    //    //                j += Convert.ToInt32(currenttransferlen);
    //    //                Debug.WriteLine("J==" + j, "ELMZ");

    //    //                //for (temp = 0; temp < currenttransferlen; temp++)
    //    //                //{

    //    //                //}
    //    //                //TB//Array.Copy(SectorDataArray[bytestranferred], 2, TxFrame, 2, Convert.ToInt32(currenttransferlen));

    //    //                //copyarray(sectordata.json[bytestranferred], TxFrame, 2, currenttransferlen)
    //    //                //j += Convert.ToInt32( currenttransferlen);
    //    //                //bytestranferred += Convert.ToUInt32(currenttransferlen);
    //    //                Frame_len = Convert.ToInt32(currenttransferlen + 2);


    //    //                //var ftt = HexStringToByteArray("43B43601000000000000000000000000000000005555AAAA00000000000000000000000000000000000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF000000000000000020FE60FFA000E001200360040005A0054006800700400040004000400040004000400040004000402003B00440060807D00798086009F00A800C100EF100830113025F02A602ED023303CB034704BE040008000A000CCC0D001000140018001C001E00207805780578057805780578057805780578057805EE02EE02EE02EE02EE02EE02EE02EE02EE02EE02C201C201C201C201C201C201C201C201C201C20100000000000000000000000000000000000000006AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF4006D0076009F00A800CA00FC012E0157017401F20FE60FFA000E001200360040005A00540068007C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C8002C012C012C012C012C012C012C012C012C012C01DC01DC01DC01DC01DC01DC01DC01DC01DC01DC0158025802580258025802580258025802580258029C029C029C029C029C029C029C029C029C029C02250325032503250325032503250325032503250384038403840384038403840384038403840384038403840384038403840384038403840384038403002000240028002C003000340038003A003C00400040083F083B2937CD340E2F73279822971C0800000090012003B0044006D0076009F00A800C480DD80E6810C0125014E0157017E803780508076009F00A800CA00F941188137C1570176419581B401F1027E02E004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040A809");
    //    //                var frame = ByteArrayToString(NTxFrame);
    //    //                var bulkTransferResponse = await dongleCommWin.CAN_TxRx(Frame_len, frame);
    //    //                blkseqcnt++;

    //    //                if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
    //    //                {
    //    //                    Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
    //    //                    Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
    //    //                    return "bulkTransferResponse" + bulkTransferResponse.ECUResponseStatus;
    //    //                }
    //    //                //if(blkseqcnt==200)
    //    //                //{

    //    //                //}
    //    //                Debug.WriteLine("blkseqcnt==" + blkseqcnt, "ELMZ");
    //    //            }
    //    //            catch (Exception exception)
    //    //            {
    //    //                Debug.WriteLine("exception==" + exception.Message, "ELMZ");
    //    //                var msg = exception;
    //    //                return "exception";
    //    //            }
    //    //        }

    //    //        // Transfer Exit
    //    //        var BTxFrame = new byte[1];
    //    //        BTxFrame[0] = 0x37;
    //    //        Frame_len = 1;
    //    //        var TransferResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(BTxFrame));
    //    //        if (TransferResponse?.ECUResponseStatus != "NOERROR")
    //    //        {
    //    //            Debug.WriteLine("--------TransferResponse -ERROR------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
    //    //            Debug.WriteLine("--------TransferResponse -LOOP END------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
    //    //            return "TransferResponse" + TransferResponse.ECUResponseStatus;
    //    //        }

    //    //        // Checksum test
    //    //        //TxFrame = new byte[10];
    //    //        //Frame_len = 0;
    //    //        //TxFrame[Frame_len++] = 0x31;
    //    //        //TxFrame[Frame_len++] = 0x01;
    //    //        //TxFrame[Frame_len++] = 0xFF;
    //    //        //TxFrame[Frame_len++] = 0x01;

    //    //        //if ((addrdataformat & 0xF0) == 0x30)
    //    //        //{

    //    //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
    //    //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
    //    //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

    //    //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
    //    //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
    //    //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x000000FF));

    //    //        //}
    //    //        //else
    //    //        //{
    //    //        //    Array.Resize(ref TxFrame, 12);
    //    //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
    //    //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
    //    //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
    //    //        //    TxFrame[Frame_len++] = (byte)(sector_start_address & 0x000000FF);

    //    //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0xFF000000) >> 24);
    //    //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
    //    //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
    //    //        //    TxFrame[Frame_len++] = (byte)(sector_end_address & 0x000000FF);
    //    //        //}

    //    //        //if (flashconfig_data.sendsectorchksum == true)
    //    //        //{
    //    //        //    //TxFrame[Frame_len++] = (byte)((sectordata[i].sectorchecksum & 0xFF00) >> 8);
    //    //        //    //TxFrame[Frame_len++] = (byte)(sectordata[i].sectorchecksum & 0x00FF);

    //    //        //    Frame_len += 2;
    //    //        //}

    //    //        //var sendsectorchksumResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

    //    //    } // end of all sectors

    //    //    // Reset ECU
    //    //    TxFrame = new byte[2];
    //    //    Frame_len = 0;
    //    //    TxFrame[Frame_len++] = 0x11;
    //    //    TxFrame[Frame_len++] = 0x02;

    //    //    var resetResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
    //    //    if (resetResponse?.ECUResponseStatus != "NOERROR")
    //    //    {
    //    //        Debug.WriteLine("--------resetResponse -ERROR------------==" + resetResponse.ECUResponseStatus, "ELMZ");
    //    //        Debug.WriteLine("--------resetResponse -LOOP END------------==" + resetResponse.ECUResponseStatus, "ELMZ");
    //    //        return "resetResponse" + resetResponse.ECUResponseStatus;
    //    //    }

    //    //    string return_status = resetResponse.ECUResponseStatus;
    //    //    return ("ECU FLASHED" + return_status);
    //    //}

    //    #endregion

    //    #region StartFlashBosch
    //    public async Task<string> StartFlashBosch(flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata)
    //    {

    //        float runtimeflashpercent = 0;
    //        // switch to reprogramming mode - 10 02
    //        int Frame_len = 0;
    //        byte[] TxFrame = new byte[2];
    //        TxFrame[Frame_len++] = 0x10;
    //        TxFrame[Frame_len++] = 0x02;

    //        Debug.WriteLine("-------switch to reprogramming mode-------");
    //        var reprogrammingResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
    //        if (reprogrammingResponse?.ECUResponseStatus != "NOERROR")
    //        {
    //            Debug.WriteLine("--------- reprogrammingResponse ERROR------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
    //            Debug.WriteLine("---------reprogrammingResponse LOOP END------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
    //            return reprogrammingResponse.ECUResponseStatus;
    //        }


    //        // get Seed - 27 09
    //        Frame_len = 0;
    //        TxFrame[Frame_len++] = 0x27;
    //        TxFrame[Frame_len++] = flashconfig_data.sendseedbyte;

    //        Debug.WriteLine("-------get Seed-------");

    //        var getSeedResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

    //        if (getSeedResponse?.ECUResponseStatus != "NOERROR")
    //        {
    //            Debug.WriteLine("--------- getSeedResponse ERROR------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
    //            Debug.WriteLine("---------getSeedResponse LOOP END------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
    //            return getSeedResponse.ECUResponseStatus;
    //        }

    //        var seedarray = new byte[flashconfig_data.seedkeynumbytes];
    //        byte numkeybytes = new byte();

    //        Array.Copy(getSeedResponse.ActualDataBytes, 2, seedarray, 0, getSeedResponse.ActualDataBytes.Length - 2);

    //        //compute key for the seed received
    //        //status = getkeyfromseed(flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, Rxarray + 2, keyarray);

    //        byte[] actualKey = new byte[flashconfig_data.seedkeynumbytes];
    //        calculateSeedkey = new ECUCalculateSeedkey();
    //        Debug.WriteLine("-------get key-------");
    //        //var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEX_TYPE.GREAVES_ADVANTEK_A46_BS6, 4, ref numkeybytes, seedarray, ref actualKey);
    //        //var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD, 4, ref numkeybytes, seedarray, ref actualKey);
    //        var result = calculateSeedkey.CalculateSeedkey((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, ref numkeybytes, seedarray, ref actualKey);

    //        Array.Resize(ref TxFrame, flashconfig_data.seedkeynumbytes + 2);//
    //        //send key 27 0A
    //        Frame_len = 0;
    //        TxFrame[Frame_len++] = 0x27;
    //        TxFrame[Frame_len++] = (byte)(flashconfig_data.sendseedbyte + 1);
    //        for (int i = 0; i < actualKey.Length; i++)
    //        {
    //            TxFrame[Frame_len++] = actualKey[i];
    //        }
    //        //status = ISO15765_CANTxRx(channel, Frame_len, TxFrame, &Rxsize, Rxarray);
    //        var sendKeyResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

    //        if (sendKeyResponse?.ECUResponseStatus != "NOERROR")
    //        {
    //            Debug.WriteLine("--------- sendKeyResponse ERROR------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
    //            Debug.WriteLine("---------sendKeyResponse LOOP END------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
    //            return sendKeyResponse.ECUResponseStatus;
    //        }

    //        /* ECU Mem Map                        */
    //        /* Sector 1 - 0x80004000 - 0x80013fff */
    //        /* Sector 2 - 0x80080000 - 0x8017ffff */
    //        /* Sector 3 - 0x80020000 - 0x8007ffff */


    //        // Erase Memory
    //        if (flashconfig_data.erasesector == EraseSectorEnum.ERASEALLATONCE)
    //        {
    //            Frame_len = 0;
    //            TxFrame[Frame_len++] = 0x31;
    //            TxFrame[Frame_len++] = 0x01;
    //            TxFrame[Frame_len++] = 0xFF;
    //            TxFrame[Frame_len++] = 0x00;

    //            Array.Resize(ref TxFrame, 12);
    //            // advantek erase memory map hard coded here...
    //            TxFrame[Frame_len++] = 0x00;
    //            TxFrame[Frame_len++] = 0x02;
    //            TxFrame[Frame_len++] = 0x00;
    //            TxFrame[Frame_len++] = 0x00;

    //            TxFrame[Frame_len++] = 0x00;
    //            TxFrame[Frame_len++] = 0x0F;
    //            TxFrame[Frame_len++] = 0xFF;
    //            TxFrame[Frame_len++] = 0xFF;


    //            var eraseMemoryResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

    //            if (eraseMemoryResponse?.ECUResponseStatus != "NOERROR")
    //            {
    //                Debug.WriteLine("--------- eraseMemoryResponse ERROR------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
    //                Debug.WriteLine("---------eraseMemoryResponse LOOP END------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
    //                return eraseMemoryResponse.ECUResponseStatus;
    //            }
    //        }

    //        /* Calculate the total number of bytes to be flashed in total and then calculated runtime flashing % */
    //        for (int i = 0; i < noofsectors; i++)
    //        {
    //            var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;
    //            totalbytestobeflashed += sector_num_bytes;
    //            realtimebytesflashed = 0;
    //        }


    //        var addrdataformat = flashconfig_data.addrdataformat;
    //        for (int i = 0; i < noofsectors; i++)
    //        {
    //            var sector_start_address = Convert.ToUInt32(sectordata[i].JsonStartAddress, 16);
    //            var sector_end_address = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16);
    //            var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;

    //            var ecu_memmap_start_address = Convert.ToUInt32(sectordata[i].ECUMemMapStartAddress, 16);
    //            var ecu_memmap_end_address = Convert.ToUInt32(sectordata[i].ECUMemMapEndAddress, 16);


    //            if (flashconfig_data.erasesector == EraseSectorEnum.ERASEBYSECTOR)
    //            {
    //                // Erase Memory
    //                Frame_len = 0;
    //                TxFrame[Frame_len++] = 0x31;
    //                TxFrame[Frame_len++] = 0x01;
    //                TxFrame[Frame_len++] = 0xFF;
    //                TxFrame[Frame_len++] = 0x00;

    //                if ((flashconfig_data.addrdataformat & 0xF0) == 0x30)
    //                {
    //                    Array.Resize(ref TxFrame, 10);

    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF0000) >> 16);
    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF00) >> 8);
    //                    TxFrame[Frame_len++] = (byte)(ecu_memmap_start_address & 0x000000FF);

    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF0000) >> 16);
    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF00) >> 8);
    //                    TxFrame[Frame_len++] = (byte)(ecu_memmap_end_address & 0x000000FF);

    //                }
    //                else
    //                {
    //                    Array.Resize(ref TxFrame, 12);

    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0xFF000000) >> 24);
    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF0000) >> 16);
    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF00) >> 8);
    //                    TxFrame[Frame_len++] = (byte)(ecu_memmap_start_address & 0x000000FF);

    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0xFF000000) >> 24);
    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF0000) >> 16);
    //                    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF00) >> 8);
    //                    TxFrame[Frame_len++] = (byte)(ecu_memmap_end_address & 0x000000FF);

    //                }

    //                var response2 = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

    //                if (response2?.ECUResponseStatus != "NOERROR")
    //                {
    //                    Debug.WriteLine("--------- response2 ERROR------------==" + response2.ECUResponseStatus, "ELMZ");
    //                    Debug.WriteLine("---------response2 LOOP END------------==" + response2.ECUResponseStatus, "ELMZ");
    //                    return response2.ECUResponseStatus;
    //                }

    //                Frame_len = 0;
    //                TxFrame = new byte[4];
    //                TxFrame[Frame_len++] = 0x31;
    //                TxFrame[Frame_len++] = 0x03;
    //                TxFrame[Frame_len++] = 0xFF;
    //                TxFrame[Frame_len++] = 0x00;

    //                var checkerase = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

    //                if (checkerase?.ECUResponseStatus != "NOERROR")
    //                {
    //                    Debug.WriteLine("--------- checkerase ERROR------------==" + checkerase.ECUResponseStatus, "ELMZ");
    //                    Debug.WriteLine("---------checkerase LOOP END------------==" + checkerase.ECUResponseStatus, "ELMZ");
    //                    return response2.ECUResponseStatus;
    //                }

    //            }


    //            // Request download
    //            Frame_len = 0;
    //            TxFrame[Frame_len++] = 0x34;
    //            TxFrame[Frame_len++] = 0x00;
    //            TxFrame[Frame_len++] = flashconfig_data.addrdataformat;

    //            if (addrdataformat == 0x33)
    //            {
    //                Array.Resize(ref TxFrame, 9);
    //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
    //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
    //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

    //                TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
    //                TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
    //                TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

    //            }
    //            else if (addrdataformat == 0x44) // addrdataformat == 0x44
    //            {
    //                Array.Resize(ref TxFrame, 11);
    //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
    //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
    //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
    //                TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

    //                TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0xFF000000) >> 24);
    //                TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
    //                TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
    //                TxFrame[Frame_len++] = (byte)((sector_num_bytes & 0x000000FF));

    //            }


    //            var memoryResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
    //            if (memoryResponse?.ECUResponseStatus != "NOERROR")
    //            {
    //                Debug.WriteLine("--------- memoryResponse ERROR------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
    //                Debug.WriteLine("---------memoryResponse LOOP END------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
    //                return memoryResponse.ECUResponseStatus;
    //            }

    //            // Transfer Data in this sector
    //            int blkseqcnt = 1;
    //            var SectorDataArray = HexStringToByteArray(sectordata[i].JsonData);
    //            for (int j = 0; j < sector_num_bytes;)
    //            {
    //                try
    //                {
    //                    var NTxFrame = new byte[2];
    //                    NTxFrame[0] = 0x36;
    //                    NTxFrame[1] = (byte)(blkseqcnt & 0xFF);
    //                    var currenttransferlen = Math.Min(sector_num_bytes - j, flashconfig_data.sectorframetransferlen);
    //                    //var currenttransferlen = Math.Min(sector_num_bytes - j, 252);

    //                    Debug.WriteLine("currenttransferlen==" + currenttransferlen + 2, "ELMZ");

    //                    Array.Resize(ref NTxFrame, Convert.ToInt32(currenttransferlen) + 2);

    //                    Array.Copy(SectorDataArray, j, NTxFrame, 2, currenttransferlen);
    //                    j += Convert.ToInt32(currenttransferlen);
    //                    Debug.WriteLine("J==" + j, "ELMZ");

    //                    Frame_len = Convert.ToInt32(currenttransferlen + 2);

    //                    var frame = ByteArrayToString(NTxFrame);
    //                    var bulkTransferResponse = await dongleCommWin.CAN_TxRx(Frame_len, frame);
    //                    blkseqcnt++;

    //                    if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
    //                    {
    //                        Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
    //                        Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
    //                        return bulkTransferResponse.ECUResponseStatus;

    //                    }
    //                    realtimebytesflashed += (UInt32)currenttransferlen;

    //                    Debug.WriteLine("blkseqcnt==" + blkseqcnt, "ELMZ");
    //                }
    //                catch (Exception exception)
    //                {
    //                    Debug.WriteLine("exception==" + exception.Message, "ELMZ");
    //                    var msg = exception;
    //                    return "exception";
    //                }
    //            }

    //            // Transfer Exit
    //            var BTxFrame = new byte[1];
    //            BTxFrame[0] = 0x37;
    //            Frame_len = 1;
    //            var TransferResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(BTxFrame));
    //            if (TransferResponse?.ECUResponseStatus != "NOERROR")
    //            {
    //                Debug.WriteLine("--------TransferResponse -ERROR------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
    //                Debug.WriteLine("--------TransferResponse -LOOP END------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
    //                return TransferResponse.ECUResponseStatus;
    //            }


    //            if ((flashconfig_data.checksumsector == ChecksumSectorEnum.COMPAREBYSECTOR) || (flashconfig_data.checksumsector == ChecksumSectorEnum.COMPUTEBYSECTOR))
    //            {
    //                //Checksum test
    //                TxFrame = new byte[10];
    //                Frame_len = 0;
    //                TxFrame[Frame_len++] = 0x31;
    //                TxFrame[Frame_len++] = 0x01;
    //                TxFrame[Frame_len++] = 0xFF;
    //                TxFrame[Frame_len++] = 0x01;

    //                if ((addrdataformat & 0xF0) == 0x30)
    //                {

    //                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
    //                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
    //                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

    //                    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
    //                    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
    //                    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x000000FF));

    //                }
    //                else
    //                {
    //                    Array.Resize(ref TxFrame, 12);
    //                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
    //                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
    //                    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
    //                    TxFrame[Frame_len++] = (byte)(sector_start_address & 0x000000FF);

    //                    TxFrame[Frame_len++] = (byte)((sector_end_address & 0xFF000000) >> 24);
    //                    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
    //                    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
    //                    TxFrame[Frame_len++] = (byte)(sector_end_address & 0x000000FF);
    //                }

    //            }

    //            if (flashconfig_data.checksumsector == ChecksumSectorEnum.COMPAREBYSECTOR)
    //            {
    //                Array.Resize(ref TxFrame, Frame_len + 2);
    //                UInt16 jsonCheckSum = Convert.ToUInt16(sectordata[i].JsonCheckSum, 16);
    //                TxFrame[Frame_len++] = (byte)((jsonCheckSum & 0xFF00) >> 8);
    //                TxFrame[Frame_len++] = (byte)(jsonCheckSum & 0x00FF);

    //                //TxFrame[Frame_len++] = (byte)((Convert.ToUInt16(sectordata[i].JsonCheckSum) & 0xFF00) >> 8);
    //                //TxFrame[Frame_len++] = (byte)( Convert.ToUInt16(sectordata[i].JsonCheckSum) & 0x00FF);
    //            }

    //            if ((flashconfig_data.checksumsector == ChecksumSectorEnum.COMPAREBYSECTOR) || (flashconfig_data.checksumsector == ChecksumSectorEnum.COMPUTEBYSECTOR))
    //            {
    //                var sendsectorchksumResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
    //                if (TransferResponse?.ECUResponseStatus != "NOERROR")
    //                {
    //                    Debug.WriteLine("--------sendsectorchksumResponse -ERROR------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
    //                    Debug.WriteLine("--------sendsectorchksumResponse -LOOP END------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
    //                    return sendsectorchksumResponse.ECUResponseStatus;
    //                }

    //                TxFrame = new byte[4];
    //                Frame_len = 0;
    //                TxFrame[Frame_len++] = 0x31;
    //                TxFrame[Frame_len++] = 0x03;
    //                TxFrame[Frame_len++] = 0xFF;
    //                TxFrame[Frame_len++] = 0x01;

    //                var sendsectorchksumResponse2 = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
    //                if (sendsectorchksumResponse2?.ECUResponseStatus != "NOERROR")
    //                {
    //                    Debug.WriteLine("--------sendsectorchksumResponse2 -ERROR------------==" + sendsectorchksumResponse2.ECUResponseStatus, "ELMZ");
    //                    Debug.WriteLine("--------sendsectorchksumResponse2 -LOOP END------------==" + sendsectorchksumResponse2.ECUResponseStatus, "ELMZ");
    //                    return sendsectorchksumResponse2.ECUResponseStatus;
    //                }
    //            }

    //        } // end of all sectors

    //        string return_status = string.Empty;
    //        TxFrame = new byte[2];

    //        if (((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_DEV) || ((SEEDKEYINDEXTYPE)flashconfig_data.seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD))
    //        {
    //            Frame_len = 0;
    //            TxFrame[Frame_len++] = 0x11;
    //            TxFrame[Frame_len++] = 0x01;
    //        }
    //        else
    //        {
    //            // Reset ECU

    //            Frame_len = 0;
    //            TxFrame[Frame_len++] = 0x11;
    //            TxFrame[Frame_len++] = 0x02;
    //        }
    //        var resetResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
    //        if (resetResponse?.ECUResponseStatus != "NOERROR")
    //        {
    //            Debug.WriteLine("--------resetResponse -ERROR------------==" + resetResponse.ECUResponseStatus, "ELMZ");
    //            Debug.WriteLine("--------resetResponse -LOOP END------------==" + resetResponse.ECUResponseStatus, "ELMZ");
    //            return resetResponse.ECUResponseStatus;
    //        }
    //        return_status = resetResponse.ECUResponseStatus;
    //        return (return_status);

    //    }

    //    #endregion


    //}
    #endregion
}
