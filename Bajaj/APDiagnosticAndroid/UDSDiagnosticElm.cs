using APDiagnosticAndroid.Enums;
using APDiagnosticAndroid.Helper;
using APDiagnosticAndroid.Models;
using AutoELM327;
using ECUSeedkeyPCL;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace APDiagnosticAndroid
{
    public class UDSDiagnosticElm
    {
        #region Properties
        ReadDTCIndex DTCIndex = 0;
        private APELMDongleComm dongleCommWin;
        private ECUCalculateSeedkey calculateSeedkey;
        #endregion

        #region CTOR
        public UDSDiagnosticElm(APELMDongleComm dongleCommWin)
        {
            this.dongleCommWin = dongleCommWin;
        }

        #endregion

        #region ReadDTC
        public async Task<ReadDtcResponseModel> ReadDTC(ReadDTCIndex readDtcIndex)
        {
            try
            {
                DTCIndex = readDtcIndex;
                string status = string.Empty;
                string return_status = string.Empty;
                string[,] dtcarray = null;

                if (DTCIndex == ReadDTCIndex.UDS_2BYTE12_DTC || DTCIndex == ReadDTCIndex.UDS_2BYTE13_DTC || DTCIndex == ReadDTCIndex.UDS_3BYTE_DTC)
                {
                    var frameLength = 3;
                    AutoELM327.Models.ResponseArrayStatus responseBytes = new AutoELM327.Models.ResponseArrayStatus();
                    //responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "1902AF");
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
                     responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, DTCFunction);
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
                        var dtc_start_byte_index = 3;
                        var no_of_dtc = (Rxsize - 3) / 4;
                        dtcarray = new string[no_of_dtc, 2];
                        var i = 0;
                        while (i < no_of_dtc)
                        {
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

                            var value = Rxarray[dtc_start_byte_index + 3] & 0x81;
                            switch (value)
                            {
                                case 0x00:
                                    dtcarray[i, 2 - 1] = "Inactive:LampOff";
                                    break;
                                case 0x01:
                                    dtcarray[i, 2 - 1] = "Active:LampOff";
                                    break;
                                case 0x80:
                                    dtcarray[i, 2 - 1] = "Inactive:LampOn";
                                    break;
                                case 0x81:
                                    dtcarray[i, 2 - 1] = "Active:LampOn";
                                    break;
                            }

                            switch (DTCIndex)
                            {
                                case ReadDTCIndex.UDS_3BYTE_DTC:

                                    dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2") + "-" + (Rxarray[dtc_start_byte_index + 2]).ToString("X2"); // correct this syntax
                                    break;
                                case ReadDTCIndex.UDS_2BYTE12_DTC:
                                    dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax
                                    break;
                                case ReadDTCIndex.UDS_2BYTE13_DTC:
                                    dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 2]).ToString("X2"); // correct this syntax
                                    break;
                                case ReadDTCIndex.KWP_2BYTE_DTC:

                                    dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax
                                    break;
                            }
                            dtc_start_byte_index += 4;
                            i++;
                        }
                    }
                    else
                    {
                        return_status = status;
                    }
                }
                else if (DTCIndex == ReadDTCIndex.KWP_2BYTE_DTC)
                {
                    var frameLength = 4;
                    AutoELM327.Models.ResponseArrayStatus responseBytes = new AutoELM327.Models.ResponseArrayStatus();
                    responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "1800FF00");

                    status = responseBytes.ECUResponseStatus;
                    var actualData = responseBytes.ActualDataBytes;
                    return_status = string.Empty;
                    if (status == "NOERROR")
                    {
                        return_status = "NO_ERROR";
                        var Rxsize = actualData.Length;
                        var Rxarray = actualData;
                        string dtc_type = string.Empty;


                        /* 58 nodtc DTCHB1 DTCLB1 DTCSTS1 DTCHB2 DTCLB2 DTCSTS2 ..... DTCHBn DTCLBn DTCSTSn*/
                        var dtc_start_byte_index = 2;
                        var no_of_dtc = Rxarray[1];
                        dtcarray = new string[no_of_dtc, 2];
                        var i = 0;
                        while (i < no_of_dtc)
                        {
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

                            var value = Rxarray[dtc_start_byte_index + 2] & 0x81;
                            switch (value)
                            {
                                case 0x00:
                                    dtcarray[i, 2 - 1] = "Inactive:LampOff";
                                    break;
                                case 0x01:
                                    dtcarray[i, 2 - 1] = "Active:LampOff";
                                    break;
                                case 0x80:
                                    dtcarray[i, 2 - 1] = "Inactive:LampOn";
                                    break;
                                case 0x81:
                                    dtcarray[i, 2 - 1] = "Active:LampOn";
                                    break;
                            }



                            dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax

                            dtc_start_byte_index += 3;
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
                    var responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "03");

                    status = responseBytes.ECUResponseStatus;
                    var actualData03 = responseBytes.ActualDataBytes;

                    if (status == "NOERROR")
                    {
                        var Rxarray03 = actualData03;
                        //var Rxsize03 = Rxarray03[1];

                        frameLength = 1;
                        responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "07");

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
                //else if (DTCIndex == ReadDTCIndex.KWP_2BYTE_DTC)
                //{
                //    var frameLength = 4;
                //    AutoELM327.Models.ResponseArrayStatus responseBytes = new AutoELM327.Models.ResponseArrayStatus();
                //    responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "1802FFFF");

                //    status = responseBytes.ECUResponseStatus;
                //    var actualData = responseBytes.ActualDataBytes;
                //    return_status = string.Empty;
                //    if (status == "NOERROR")
                //    {
                //        return_status = "NO_ERROR";
                //        var Rxsize = actualData.Length;
                //        var Rxarray = actualData;
                //        string dtc_type = string.Empty;


                //        /* 58 nodtc DTCHB1 DTCLB1 DTCSTS1 DTCHB2 DTCLB2 DTCSTS2 ..... DTCHBn DTCLBn DTCSTSn*/
                //        var dtc_start_byte_index = 2;
                //        var no_of_dtc = Rxarray[1];
                //        dtcarray = new string[no_of_dtc, 2];
                //        var i = 0;
                //        while (i < no_of_dtc)
                //        {
                //            var dtctypebits = (Rxarray[dtc_start_byte_index] & 0xC0) >> 6;

                //            if (dtctypebits == 0x00)
                //            {
                //                dtc_type = "P";

                //            }
                //            else if (dtctypebits == 0x01)
                //            {
                //                dtc_type = "C";
                //            }
                //            else if (dtctypebits == 0x02)
                //            {
                //                dtc_type = "B";
                //            }
                //            else if (dtctypebits == 0x03)
                //            {
                //                dtc_type = "U";
                //            }

                //            var value = Rxarray[dtc_start_byte_index + 2] & 0x81;
                //            switch (value)
                //            {
                //                case 0x00:
                //                    dtcarray[i, 2 - 1] = "Inactive:LampOff";
                //                    break;
                //                case 0x01:
                //                    dtcarray[i, 2 - 1] = "Active:LampOff";
                //                    break;
                //                case 0x80:
                //                    dtcarray[i, 2 - 1] = "Inactive:LampOn";
                //                    break;
                //                case 0x81:
                //                    dtcarray[i, 2 - 1] = "Active:LampOn";
                //                    break;
                //            }



                //            dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax

                //            dtc_start_byte_index += 3;
                //            i++;
                //        }
                //    }
                //    else
                //    {
                //        return_status = status;
                //    }
                //}

                #region Old Code
                //else if (DTCIndex == ReadDTCIndex.GENERIC_OBD)
                //{
                //    var frameLength = 1;
                //    var responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "03");

                //    status = responseBytes.ECUResponseStatus;
                //    var actualData03 = responseBytes.ActualDataBytes;

                //    if (status == "NOERROR")
                //    {
                //        var Rxarray03 = actualData03;
                //        //var Rxsize03 = Rxarray03[1];

                //        frameLength = 1;
                //        responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "07");

                //        status = responseBytes.ECUResponseStatus;
                //        var actualData07 = responseBytes.ActualDataBytes;

                //        return_status = string.Empty;
                //        if (status == "NOERROR")
                //        {
                //            return_status = "NO_ERROR";
                //            var Rxarray07 = actualData07;
                //            //var Rxsize07 = Rxarray07[1];

                //            string dtc_type = string.Empty;


                //            /* All DTCs - 43 LEN DTCHB DTCLB1 DTCHB2 DTCLB2  ..... DTCHBn DTCLBn*/
                //            /* Pending DTCs - 47 LEN DTCHB DTCLB1 DTCHB2 DTCLB2  ..... DTCHBn DTCLBn*/

                //            //var dtc_start_byte_index = 2;
                //            var no_of_dtc03 = Rxarray03[1];
                //            var no_of_dtc07 = Rxarray07[1];

                //            var dtcarray03 = new string[no_of_dtc03, 2];
                //            var dtcarray07 = new string[no_of_dtc07, 2];

                //            //int index03 = 0;
                //            //int index07 = 0;
                //            dtcarray = new string[no_of_dtc03, 2];
                //            for (int i = 0; i < no_of_dtc03; i++)
                //            {
                //                var dtctypebits = (Rxarray03[(i * 2) + 2] & 0xC0) >> 6;

                //                if (dtctypebits == 0x00)
                //                {
                //                    dtc_type = "P";

                //                }
                //                else if (dtctypebits == 0x01)
                //                {
                //                    dtc_type = "C";
                //                }
                //                else if (dtctypebits == 0x02)
                //                {
                //                    dtc_type = "B";
                //                }
                //                else if (dtctypebits == 0x03)
                //                {
                //                    dtc_type = "U";
                //                }

                //                dtcarray[i, 0] = dtc_type + (Rxarray03[(i * 2) + 2] & 0x3F).ToString("X2") + (Rxarray03[(i * 2) + 3]).ToString("X2");
                //                dtcarray[i, 1] = "Current";

                //                for (int j = 0; j < no_of_dtc07; j++)
                //                {
                //                    if ((Rxarray03[(i * 2) + 2] == Rxarray07[(j * 2) + 2]) && (Rxarray03[(i * 2) + 3] == Rxarray07[(j * 2) + 3]))
                //                    {
                //                        dtcarray[i, 1] = "Pending";
                //                        break;
                //                    }
                //                }
                //            }

                //        }
                //        else
                //        {
                //            return_status = status;
                //        }
                //    }
                //    else
                //    {
                //        return_status = status;
                //    }
                //}
                #endregion
                ReadDtcResponseModel readDtcResponseModel = new ReadDtcResponseModel
                {
                    dtcs = dtcarray,
                    status = return_status,
                };
                return readDtcResponseModel;
            }
            catch (Exception ex)
            {
                return null;
            }
            //return dtcarray;

            #region Old Code
            //try
            //{
            //    DTCIndex = readDtcIndex;
            //    string status = string.Empty;
            //    string return_status = string.Empty;
            //    string[,] dtcarray = null;
            //    if (DTCIndex == ReadDTCIndex.KWP_2BYTE_DTC || DTCIndex == ReadDTCIndex.UDS_2BYTE12_DTC || DTCIndex == ReadDTCIndex.UDS_2BYTE13_DTC || DTCIndex == ReadDTCIndex.UDS_3BYTE_DTC)
            //    {
            //        var frameLength = 3;
            //        var responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "1902AF");

            //        status = responseBytes.ECUResponseStatus;
            //        var actualData = responseBytes.ActualDataBytes;
            //        return_status = string.Empty;
            //        if (status == "NOERROR")
            //        {
            //            return_status = "NO_ERROR";
            //            var Rxsize = actualData.Length;
            //            var Rxarray = actualData;
            //            string dtc_type = string.Empty;


            //            /* 59 02 FF DTCHB DTCMB1 DTCLB1 DTCSTS1 DTCHB2 DTCMB2 DTCLB2 DTCSTS2 ..... DTCHBn DTCMBn DTCLBn DTCSTSn*/
            //            var dtc_start_byte_index = 3;
            //            var no_of_dtc = (Rxsize - 3) / 4;
            //            dtcarray = new string[no_of_dtc, 2];
            //            var i = 0;
            //            while (i < no_of_dtc)
            //            {
            //                var dtctypebits = (Rxarray[dtc_start_byte_index] & 0xC0) >> 6;

            //                if (dtctypebits == 0x00)
            //                {
            //                    dtc_type = "P";

            //                }
            //                else if (dtctypebits == 0x01)
            //                {
            //                    dtc_type = "C";
            //                }
            //                else if (dtctypebits == 0x02)
            //                {
            //                    dtc_type = "B";
            //                }
            //                else if (dtctypebits == 0x03)
            //                {
            //                    dtc_type = "U";
            //                }

            //                var value = Rxarray[dtc_start_byte_index + 3] & 0x81;
            //                switch (value)
            //                {
            //                    case 0x00:
            //                        dtcarray[i, 2 - 1] = "Inactive:LampOff";
            //                        break;
            //                    case 0x01:
            //                        dtcarray[i, 2 - 1] = "Active:LampOff";
            //                        break;
            //                    case 0x80:
            //                        dtcarray[i, 2 - 1] = "Inactive:LampOn";
            //                        break;
            //                    case 0x81:
            //                        dtcarray[i, 2 - 1] = "Active:LampOn";
            //                        break;
            //                }

            //                switch (DTCIndex)
            //                {
            //                    case ReadDTCIndex.UDS_3BYTE_DTC:

            //                        dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2") + "-" + (Rxarray[dtc_start_byte_index + 2]).ToString("X2"); // correct this syntax
            //                        break;
            //                    case ReadDTCIndex.UDS_2BYTE12_DTC:
            //                        dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax
            //                        break;
            //                    case ReadDTCIndex.UDS_2BYTE13_DTC:
            //                        dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 2]).ToString("X2"); // correct this syntax
            //                        break;
            //                    case ReadDTCIndex.KWP_2BYTE_DTC:

            //                        dtcarray[i, 0] = dtc_type + (Rxarray[dtc_start_byte_index] & 0x3F).ToString("X2") + (Rxarray[dtc_start_byte_index + 1]).ToString("X2"); // correct this syntax
            //                        break;
            //                }
            //                dtc_start_byte_index += 4;
            //                i++;
            //            }
            //        }
            //        else
            //        {
            //            return_status = status;
            //        }
            //    }
            //    else if (DTCIndex == ReadDTCIndex.GENERIC_OBD)
            //    {
            //        var frameLength = 1;
            //        var responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "03");

            //        status = responseBytes.ECUResponseStatus;
            //        var actualData03 = responseBytes.ActualDataBytes;

            //        if (status == "NOERROR")
            //        {

            //            var Rxsize03 = actualData03.Length;
            //            var Rxarray03 = actualData03;

            //            frameLength = 1;
            //            responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "07");

            //            status = responseBytes.ECUResponseStatus;
            //            var actualData07 = responseBytes.ActualDataBytes;

            //            return_status = string.Empty;
            //            if (status == "NOERROR")
            //            {
            //                return_status = "NO_ERROR";
            //                var Rxsize07 = actualData07.Length;
            //                var Rxarray07 = actualData07;

            //                string dtc_type = string.Empty;


            //                /* All DTCs - 43 DTCHB DTCLB1 DTCHB2 DTCLB2  ..... DTCHBn DTCLBn*/
            //                /* Pending DTCs - 47 DTCHB DTCLB1 DTCHB2 DTCLB2  ..... DTCHBn DTCLBn*/

            //                //var dtc_start_byte_index = 2;
            //                var no_of_dtc03 = (Rxsize03 - 1) / 2;
            //                var no_of_dtc07 = (Rxsize07 - 1) / 2;

            //                var dtcarray03 = new string[no_of_dtc03, 2];
            //                var dtcarray07 = new string[no_of_dtc07, 2];

            //                //int index03 = 0;
            //                //int index07 = 0;
            //                dtcarray = new string[no_of_dtc03, 2];
            //                for (int i = 0; i < no_of_dtc03; i++)
            //                {
            //                    var dtctypebits = (Rxarray03[i + 2] & 0xC0) >> 6;

            //                    if (dtctypebits == 0x00)
            //                    {
            //                        dtc_type = "P";

            //                    }
            //                    else if (dtctypebits == 0x01)
            //                    {
            //                        dtc_type = "C";
            //                    }
            //                    else if (dtctypebits == 0x02)
            //                    {
            //                        dtc_type = "B";
            //                    }
            //                    else if (dtctypebits == 0x03)
            //                    {
            //                        dtc_type = "U";
            //                    }

            //                    dtcarray[i, 0] = dtc_type + (Rxarray03[i + 2] & 0x3F).ToString("X2") + (Rxarray03[i + 3]).ToString("X2");
            //                    dtcarray[i, 1] = "Current";

            //                    for (int j = 0; j < no_of_dtc07; j++)
            //                    {
            //                        if (Rxarray03[(i + 1) * 2] == Rxarray07[(j + 1) * 2])
            //                        {
            //                            dtcarray[i, 1] = "Pending";
            //                            break;
            //                        }

            //                    }

            //                }
            //            }
            //            else
            //            {
            //                return_status = status;
            //            }
            //        }
            //        else
            //        {
            //            return_status = status;
            //        }
            //    }
            //    ReadDtcResponseModel readDtcResponseModel = new ReadDtcResponseModel
            //    {
            //        dtcs = dtcarray,
            //        status = return_status,
            //    };
            //    return readDtcResponseModel;
            //}
            //catch (Exception ex)
            //{
            //    return null;
            //}
            ////return dtcarray;
            #endregion
        }
        #endregion

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
                var frameLength = 5;
                responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "14FFFFFF");
            }
            else if (ClearDTCIndex == ClearDTCIndex.UDS_3BYTES)
            {
                var frameLength = 4;
                responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "14FFFF");
            }
            else if (ClearDTCIndex == ClearDTCIndex.GENERIC_OBD)
            {
                var frameLength = 1;
                responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "04");
            }
            else if (ClearDTCIndex == ClearDTCIndex.KWP)
            {
                var frameLength = 3;
                responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "14FF00");
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

                for (int i = 0; i < 7; i++)
                {
                    var frameLength = 1;
                    var response = await this.dongleCommWin.CAN_TxRx(frameLength, "01" + (i * 0x20).ToString("X2"));

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
                                supportedpid[noofsupportedpid - 1] = "01" + ((i * 0x20) + (32 - j)).ToString("X2");
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

                var pidResponse = await dongleCommWin.CAN_TxRx(frameLength, pidItem.pid);
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


                                if (pidItem.readParameterIndex == ReadParameterIndex.UDS_2S_COMPLIMENT)
                                {
                                    var signedpidintvalue = (uint)unsignedpidintvalue;
                                    float_pid_value = (signedpidintvalue * pidItem.resolution) + pidItem.offset;
                                }
                                else
                                {
                                    float_pid_value = (unsignedpidintvalue * pidItem.resolution) + pidItem.offset;
                                }


                                //outputlivepararray[i] = Convert.ToByte(float_pid_value);
                                //outputlivepararray[i] = Convert.ToByte(unsignedpidintvalue);
                                dataValue = float_pid_value.ToString();
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
                                if (dataValue.Contains('\0') == true)
                                {
                                    dataValue = "NULL";
                                }

                                Debug.WriteLine("---END ASCII--");
                            }
                            else if (inputliveparaarray[i].datatype == "BCD")

                            {
                                /* we dont encounter bitcoded messages here */
                                //outputlivepararray[i] = hex2str(Rxarray[startbyte + tx_Frame_len - 2] till inputliveparaarray[i].noofbytes) + '\0';

                                //22f190 62f19036363636 - "36363636"
                                //0906   4606363636     - ""

                                Debug.WriteLine("---START BCD--");
                                Array.Resize(ref outputlivepararray, pidItem.noOfBytes);
                                Array.Copy(Rxarray, pidItem.startByte + tx_Frame_len - 1, outputlivepararray, 0, pidItem.noOfBytes);
                                dataValue = ByteArrayToString(outputlivepararray);
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
                                byte[] rxdata = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                                Array.Copy(Rxarray, 3, rxdata, 0, Rxarray.Length - 3);

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
                                dataValue = BytesConverter.HexToASCII(ByteArrayToString(iqa_x));
                                Debug.WriteLine("---END iqa--");

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
        //        var frameLength = pidItem.pid.Length / 2;
        //        string dataValue = string.Empty;
        //        //Convert PID TO Bytes
        //        byte[] pid = HexToBytes(pidItem.pid);
        //        Debug.WriteLine("---------------------START LOOP PID NAME-------------------------------------" + pidItem.pidName);

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
        //                        UInt32 unsignedpidintvalue = 0; // take double int data type
        //                        for (int j = 0; j < pidItem.noOfBytes; j++)
        //                        {
        //                            unsignedpidintvalue |= Convert.ToUInt32(Convert.ToUInt32(Rxarray[pidItem.startByte + tx_Frame_len - 1 + j]) << ((pidItem.noOfBytes - 1 - j) * 8));
        //                        }

        //                        if (pidItem.IsBitcoded == true)
        //                        {
        //                            var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
        //                            unsignedpidintvalue = Convert.ToUInt32((unsignedpidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask);
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
        //                        Array.Resize(ref outputlivepararray, pidItem.noOfBytes);
        //                        Array.Copy(Rxarray, pidItem.startByte + tx_Frame_len - 1, outputlivepararray, 0, pidItem.noOfBytes);
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
        //                        Array.Resize(ref outputlivepararray, pidItem.noOfBytes);
        //                        Array.Copy(Rxarray, pidItem.startByte + tx_Frame_len - 1, outputlivepararray, 0, pidItem.noOfBytes);
        //                        dataValue = ByteArrayToString(outputlivepararray);
        //                        Debug.WriteLine("---END BCD--");
        //                    }
        //                    else if (inputliveparaarray[i].datatype == "ENUMRATED")

        //                    {
        //                        UInt32 pidintvalue = 0; // take double int data type
        //                        for (int j = 0; j < pidItem.noOfBytes; j++)
        //                        {
        //                            pidintvalue |= Convert.ToUInt32(Convert.ToUInt32(Rxarray[pidItem.startByte + tx_Frame_len - 1 + j]) << ((pidItem.noOfBytes - 1 - j) * 8));
        //                        }

        //                        if (pidItem.IsBitcoded)
        //                        {
        //                            var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
        //                            pidintvalue = Convert.ToUInt32((pidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask);
        //                        }
        //                        dataValue = Convert.ToString(pidintvalue);
        //                        if (inputliveparaarray[i].messages.Count > 0)
        //                        {

        //                            foreach (var item in inputliveparaarray[i].messages)
        //                            {
        //                                if (Convert.ToUInt32(item.code) == pidintvalue)
        //                                {
        //                                    dataValue = item.message;
        //                                }
        //                            }
        //                        }
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

        //                    }
        //                    //else if (inputliveparaarray[i].datatype == "IQA")
        //                    //{
        //                    //    char[] iqa_lookup_x = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8' };
        //                    //    byte[] rxdata = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        //                    //    Array.Copy(Rxarray, 3, rxdata, 0, Rxarray.Length - 3);

        //                    //    byte[] iqa_y = new byte[7];
        //                    //    byte[] iqa_x = new byte[7];

        //                    //    iqa_y[0] = Convert.ToByte(rxdata[0] >> 3);
        //                    //    iqa_y[1] = Convert.ToByte(rxdata[1] >> 3);
        //                    //    iqa_y[2] = Convert.ToByte(rxdata[2] >> 3);
        //                    //    iqa_y[3] = Convert.ToByte(rxdata[3] >> 3);
        //                    //    iqa_y[4] = Convert.ToByte(rxdata[4] >> 3);
        //                    //    iqa_y[5] = Convert.ToByte(rxdata[5] >> 3);
        //                    //    iqa_y[6] = Convert.ToByte(rxdata[7] >> 3);

        //                    //    //convert it to alphanumeric
        //                    //    iqa_x[0] = Convert.ToByte(iqa_lookup_x[iqa_y[0]]);
        //                    //    iqa_x[1] = Convert.ToByte(iqa_lookup_x[iqa_y[1]]);
        //                    //    iqa_x[2] = Convert.ToByte(iqa_lookup_x[iqa_y[2]]);
        //                    //    iqa_x[3] = Convert.ToByte(iqa_lookup_x[iqa_y[3]]);
        //                    //    iqa_x[4] = Convert.ToByte(iqa_lookup_x[iqa_y[4]]);
        //                    //    iqa_x[5] = Convert.ToByte(iqa_lookup_x[iqa_y[5]]);
        //                    //    iqa_x[6] = Convert.ToByte(iqa_lookup_x[iqa_y[6]]);
        //                    //    //iqa_x[7] = Convert.ToByte(iqa_lookup_x[iqa_y[7]]);
        //                    //    /*SPECIAL LOGIC - WILL TAKE CARE OF THIS LATER */

        //                    //    Debug.WriteLine("---START ASCII--");
        //                    //    outputlivepararray = Rxarray;
        //                    //    dataValue = BytesConverter.HexToASCII(ByteArrayToString(iqa_x));
        //                    //    Debug.WriteLine("---END ASCII--");

        //                    //}

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

        //        #endregion

        //    }
        //    return databyteArray;
        //}



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
        //        Debug.WriteLine("---------------------START LOOP PID NAME-------------------------------------" + pidItem.pidName);

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
        //                        //for (int j = 0; j < pidItem.noOfBytes; j++)
        //                        //{
        //                        //    unsignedpidintvalue |= Rxarray[pidItem.startByte + tx_Frame_len - j] << (j * 8);
        //                        //}

        //                        for (int j = 0; j < pidItem.noOfBytes; j++)
        //                        {
        //                            unsignedpidintvalue |= Convert.ToUInt32(Convert.ToUInt32(Rxarray[pidItem.startByte + tx_Frame_len - 1 + j]) << ((pidItem.noOfBytes - 1 - j) * 8));
        //                        }

        //                        if (pidItem.IsBitcoded == true)
        //                        {
        //                            var mask = (Convert.ToInt32((Math.Pow(16, pidItem.noOfBytes * 2) - 1))) >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1);
        //                            unsignedpidintvalue = Convert.ToUInt32((unsignedpidintvalue >> ((pidItem.noOfBytes * 8) - pidItem.noofBits - pidItem.startBit + 1)) & mask);
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
        //                        Array.Resize(ref outputlivepararray, pidItem.noOfBytes);
        //                        Array.Copy(Rxarray, pidItem.startByte + tx_Frame_len - 1, outputlivepararray, 0, pidItem.noOfBytes);
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
        //                        Array.Resize(ref outputlivepararray, pidItem.noOfBytes);
        //                        Array.Copy(Rxarray, pidItem.startByte + tx_Frame_len - 1, outputlivepararray, 0, pidItem.noOfBytes);
        //                        dataValue = ByteArrayToString(outputlivepararray);
        //                        Debug.WriteLine("---END BCD--");
        //                    }
        //                    else if (inputliveparaarray[i].datatype == "ENUMRATED")

        //                    {
        //                        var pidintvalue = 0; // take double int data type
        //                        //for (int j = 0; j < pidItem.noOfBytes; j++)
        //                        //{
        //                        //    pidintvalue |= Rxarray[pidItem.startByte + tx_Frame_len - 2] << ((pidItem.noOfBytes - 1) * 8);
        //                        //}
        //                        for (int j = 0; j < pidItem.noOfBytes; j++)
        //                        {
        //                            pidintvalue |= Rxarray[pidItem.startByte + tx_Frame_len - j] << (j * 8);
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

        //        #endregion

        //    }
        //    return databyteArray;
        //}

        #endregion

        #region WriteParameters
        //public async Task<ObservableCollection<WriteParameterResponse>> WriteParameters(int noOFParameters, WriteParameterIndex writeParameterIndex, ObservableCollection<WriteParameterPID> writeParameterCollection)
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
        //            var datasArrays = pidResponsebytes.ActualDataBytes;
        //            var Rxarray = pidResponsebytes.ActualDataBytes;
        //            var Rxsize = Rxarray.Length;
        //            int seedkeyindex = getseedindex;
        //            var status = pidResponsebytes.ECUResponseStatus;

        //            if (status == "NOERROR")
        //            {
        //                var seedarray = new byte[Rxsize - 2];
        //                /* get seed from the response and send it to the seedkey dll to get the key */
        //                //for (int i = 0; i < Rxsize - 2; i++)
        //                //{
        //                //    seedarray[i] = Rxarray[i + 2];
        //                //}
        //                Array.Copy(Rxarray, 2, seedarray, 0, Rxsize - 2);

        //                //status = getkeyfromseed(seedkeyindex, seedsize, seedarray, &keysize, keyarray);

        //                byte numkeybytes = new byte();

        //                byte[] actualKey = new byte[16];
        //                calculateSeedkey = new ECUCalculateSeedkey();
        //                var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEXTYPE.ATULAUTO_ADVANTEK_A46_BS6, 16, ref numkeybytes, seedarray, ref actualKey);

        //                if (status == "NOERROR")
        //                {
        //                    byte[] newTxFrame = new byte[actualKey.Length + 2];
        //                    /* Send calculated key to ECU */
        //                    newTxFrame[0] = 0x27;
        //                    newTxFrame[1] = Convert.ToByte(seedkeyindex + 1);

        //                    Array.Copy(actualKey, 0, newTxFrame, 2, actualKey.Length);

        //                    tx_Frame_len = Rxsize;
        //                    frameLength = newTxFrame.Length;

        //                    //Send the parameter ID to the ECU
        //                    var pidResponse3 = await dongleCommWin.CAN_TxRx(frameLength, ByteArrayToString(newTxFrame));

        //                    var writeParaPID = HexStringToByteArray(pidItem.writeparapid);
        //                    var writeparaframesize = pidItem.writeparadata.Length;
        //                    if (pidResponse3.ECUResponseStatus == "NOERROR")
        //                    {
        //                        byte[] writeFrame = new byte[1 + writeParaPID.Length + pidItem.writeparadata.Length];
        //                        /* write new value to ECU */
        //                        writeFrame[0] = 0x2E;

        //                        Array.Copy(writeParaPID, 0, writeFrame, 1, writeParaPID.Length);

        //                        Array.Copy(pidItem.writeparadata, 0, writeFrame, writeParaPID.Length + 1, pidItem.writeparadata.Length);

        //                        var pidResponse4 = await dongleCommWin.CAN_TxRx(writeFrame.Length, ByteArrayToString(writeFrame));

        //                        status = pidResponse4.ECUResponseStatus;

        //                        if (status == "NOERROR")
        //                        {
        //                            return_status = "NOERROR";

        //                        }
        //                        else
        //                        {
        //                            return_status = status;
        //                        }
        //                        var dataItem = new WriteParameterResponse
        //                        {
        //                            Status = pidResponse4.ECUResponseStatus,
        //                            DataArray = pidResponse4.ActualDataBytes,
        //                            pidName = pidItem.writeparapid,
        //                            pidNumber = pidItem.writeparano,

        //                            responseValue = pidResponse4.ECUResponseStatus

        //                        };
        //                        WriteParameterCollection.Add(dataItem);
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

        //    //#region OldImplementation
        //    //byte[] SendByteArray = null;
        //    //foreach (var pidItem in readParameterCollection)
        //    //{
        //    //    //Set Session
        //    //    var setSession = await dongleCommWin.SendCommand("1003");
        //    //    var sessionBytesResponse = (byte[])setSession;

        //    //    if (sessionBytesResponse[0] == 0x50)
        //    //    {
        //    //        //positive response
        //    //    }
        //    //    else
        //    //    {
        //    //        //negative respone
        //    //    }

        //    //    byte[] pid = HexToBytes(pidItem.pid);

        //    //    //Request for the seed
        //    //    var requestSeed = await dongleCommWin.SendCommand("27" + pid[1].ToString("D2"));
        //    //    var requestSeedBytesResponse = (byte[])requestSeed;

        //    //    //Copy the seed bytes
        //    //    var copySeedBytes = new byte[requestSeedBytesResponse.Length - 2];
        //    //    Array.Copy(requestSeedBytesResponse, 2, copySeedBytes, 0, requestSeedBytesResponse.Length - 2);

        //    //    //Call Api to calulate the key
        //    //    //Receive the Key from the server

        //    //    //send key to ECU
        //    //    var sendKeytoECU = await dongleCommWin.SendCommand("27yy+1+key");
        //    //    var keysFromECU = (byte[])sendKeytoECU;
        //    //    if (keysFromECU[0] == 0x67)
        //    //    {
        //    //        //positive response
        //    //    }
        //    //    else
        //    //    {
        //    //        //negative response}
        //    //    }

        //    //    if (!pidItem.IsBitcoded)
        //    //    {
        //    //        var totalBytes = pidItem.totalBytes;
        //    //        var startBytes = pidItem.startByte;
        //    //        var noOfBytes = pidItem.noOfBytes;
        //    //        var dataByteArray = pidItem.dataByteArray;

        //    //        var byteArray = new byte[pidItem.totalBytes];


        //    //        Array.Copy(dataByteArray, startBytes, byteArray, 0, pidItem.noOfBytes);
        //    //        SendByteArray = byteArray;


        //    //    }
        //    //    else if (pidItem.IsBitcoded)
        //    //    {
        //    //        var totalBytes = pidItem.totalBytes;
        //    //        var startBytes = pidItem.startByte;
        //    //        var noOfBytes = pidItem.noOfBytes;

        //    //        var startBit = pidItem.startBit;
        //    //        var noOFBit = pidItem.noofBits;

        //    //        var dataByteArray = pidItem.dataByteArray;


        //    //        var byteArray = new byte[pidItem.totalBytes];
        //    //        Array.Copy(dataByteArray, startBytes, byteArray, 0, pidItem.noOfBytes);


        //    //        var actualDataByte = byteArray[startBytes];

        //    //        //Get the hex string of the byte
        //    //        var hexString = actualDataByte.ToString();

        //    //        //Get the eight bit binary string
        //    //        string binarystring = String.Join("0000", hexString.Select(
        //    //            number => Convert.ToString(Convert.ToInt32(number.ToString(), 16), 2).PadLeft(4, '0')
        //    //            ));


        //    //        var actualNumberArray = "00000000".ToCharArray();
        //    //        var binarystringArray = binarystring.ToCharArray();

        //    //        //Copy the initial array to the binary array
        //    //        Array.Copy(binarystringArray, pidItem.startBit, actualNumberArray, 0, pidItem.noofBits);

        //    //        string binaryStr = actualNumberArray.ToString();

        //    //        //convert the strings to byte
        //    //        var bytesArray = Enumerable.Range(0, int.MaxValue / 8)
        //    //                                  .Select(i => i * 8)    // get the starting index of which char segment
        //    //                                  .TakeWhile(i => i < binaryStr.Length)
        //    //                                  .Select(i => binaryStr.Substring(i, 8)) // get the binary string segments
        //    //                                  .Select(s => Convert.ToByte(s, 2)) // convert to byte
        //    //                                  .ToString();

        //    //        byteArray[pidItem.startByte] = HexToBytes(bytesArray)[0];

        //    //        SendByteArray = byteArray;


        //    //    }
        //    //    var frameLength = pidItem.totalLen;
        //    //    var pidResponse = await dongleCommWin.CAN_TxRx(frameLength, "2E" + pidItem.pid + SendByteArray);

        //    //    return null;
        //    //}


        //    //return null; 
        //    //#endregion
        //}

        #endregion

        #region StartFlash
        //public async Task<string> StartFlash(flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata)
        //{

        //    int Frame_len = 0;
        //    byte[] TxFrame = new byte[2];
        //    TxFrame[Frame_len++] = 0x10;
        //    TxFrame[Frame_len++] = 0x02;

        //    Debug.WriteLine("-------switch to reprogramming mode-------");
        //    var reprogrammingResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    if (reprogrammingResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- reprogrammingResponse ERROR------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------reprogrammingResponse LOOP END------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
        //        return "reprogrammingResponse" + reprogrammingResponse.ECUResponseStatus;
        //    }


        //    // get Seed - 27 09
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x27;
        //    TxFrame[Frame_len++] = flashconfig_data.sendseedbyte;

        //    Debug.WriteLine("-------get Seed-------");

        //    var getSeedResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    if (getSeedResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- getSeedResponse ERROR------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------getSeedResponse LOOP END------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
        //        return "getSeedResponse" + getSeedResponse.ECUResponseStatus;
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
        //    var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEX_TYPE.GREAVES_ADVANTEK_A46_BS6, 4, ref numkeybytes, seedarray, ref actualKey);

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
        //    var sendKeyResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    if (sendKeyResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- sendKeyResponse ERROR------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------sendKeyResponse LOOP END------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
        //        return "sendKeyResponse " + sendKeyResponse.ECUResponseStatus;
        //    }

        //    // Erase Memory
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x31;
        //    TxFrame[Frame_len++] = 0x01;
        //    TxFrame[Frame_len++] = 0xFF;
        //    TxFrame[Frame_len++] = 0x00;

        //    Array.Resize(ref TxFrame, 12);

        //    TxFrame[Frame_len++] = 0x00;
        //    TxFrame[Frame_len++] = 0x02;
        //    TxFrame[Frame_len++] = 0x00;
        //    TxFrame[Frame_len++] = 0x00;

        //    TxFrame[Frame_len++] = 0x00;
        //    TxFrame[Frame_len++] = 0x0F;
        //    TxFrame[Frame_len++] = 0xFF;
        //    TxFrame[Frame_len++] = 0xFF;


        //    var eraseMemoryResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    if (eraseMemoryResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- eraseMemoryResponse ERROR------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------eraseMemoryResponse LOOP END------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //        return "eraseMemoryResponse " + eraseMemoryResponse.ECUResponseStatus;
        //    }

        //    //for (int m = 0; m <= 1000; m++)
        //    //{
        //    //    var ftt = HexStringToByteArray("3601000000000000000000000000000000005555AAAA00000000000000000000000000000000000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF000000000000000020FE60FFA000E001200360040005A0054006800700400040004000400040004000400040004000402003B00440060807D00798086009F00A800C100EF100830113025F02A602ED023303CB034704BE040008000A000CCC0D001000140018001C001E00207805780578057805780578057805780578057805EE02EE02EE02EE02EE02EE02EE02EE02EE02EE02C201C201C201C201C201C201C201C201C201C20100000000000000000000000000000000000000006AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF4006D0076009F00A800CA00FC012E0157017401F20FE60FFA000E001200360040005A00540068007C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C8002C012C012C012C012C012C012C012C012C012C01DC01DC01DC01DC01DC01DC01DC01DC01DC01DC0158025802580258025802580258025802580258029C029C029C029C029C029C029C029C029C029C02250325032503250325032503250325032503250384038403840384038403840384038403840384038403840384038403840384038403840384038403002000240028002C003000340038003A003C00400040083F083B2937CD340E2F73279822971C0800000090012003B0044006D0076009F00A800C480DD80E6810C0125014E0157017E803780508076009F00A800CA00F941188137C1570176419581B401F1027E02E004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040");
        //    //    var nsbyts = new byte[944];

        //    //    var pidResponses = await dongleCommWin.CAN_TxRx(ftt.Length, ByteArrayToString(ftt));
        //    //}

        //    var addrdataformat = flashconfig_data.addrdataformat;
        //    for (int i = 0; i < noofsectors; i++)
        //    {
        //        var sector_start_address = Convert.ToUInt32(sectordata[i].JsonStartAddress, 16);
        //        var sector_end_address = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16);
        //        var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress, 16) - Convert.ToUInt32(sectordata[i].JsonStartAddress, 16) + 1;

        //        var ecu_memmap_start_address = Convert.ToUInt32(sectordata[i].ECUMemMapStartAddress, 16);
        //        var ecu_memmap_end_address = Convert.ToUInt32(sectordata[i].ECUMemMapEndAddress, 16);

        //        //// Erase Memory
        //        //Frame_len = 0;
        //        //TxFrame[Frame_len++] = 0x31;
        //        //TxFrame[Frame_len++] = 0x01;
        //        //TxFrame[Frame_len++] = 0xFF;
        //        //TxFrame[Frame_len++] = 0x00;

        //        //if ((flashconfig_data.addrdataformat & 0xF0) == 0x30)
        //        //{
        //        //    Array.Resize(ref TxFrame, 10);

        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF0000) >> 16);
        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF00) >> 8);
        //        //    TxFrame[Frame_len++] = (byte)(ecu_memmap_start_address & 0x000000FF);

        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF0000) >> 16);
        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF00) >> 8);
        //        //    TxFrame[Frame_len++] = (byte)(ecu_memmap_end_address & 0x000000FF);

        //        //}
        //        //else
        //        //{
        //        //    Array.Resize(ref TxFrame, 12);

        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0xFF000000) >> 24);
        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF0000) >> 16);
        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_start_address & 0x00FF00) >> 8);
        //        //    TxFrame[Frame_len++] = (byte)(ecu_memmap_start_address & 0x000000FF);

        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0xFF000000) >> 24);
        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF0000) >> 16);
        //        //    TxFrame[Frame_len++] = (byte)((ecu_memmap_end_address & 0x00FF00) >> 8);
        //        //    TxFrame[Frame_len++] = (byte)(ecu_memmap_end_address & 0x000000FF);

        //        //}

        //        //pidResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //        // Request download
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x34;
        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = flashconfig_data.addrdataformat;

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


        //        var memoryResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //        if (memoryResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------- memoryResponse ERROR------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("---------memoryResponse LOOP END------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
        //            return "memoryResponse " + memoryResponse.ECUResponseStatus;
        //        }

        //        UInt32 bytestranferred = 0;
        //        // Transfer Data in this sector
        //        int blkseqcnt = 1;
        //        var SectorDataArray = HexStringToByteArray(sectordata[i].JsonData);
        //        for (int j = 0; j < sector_num_bytes;)
        //        {
        //            try
        //            {
        //                if (j == 41076)
        //                {

        //                }
        //                var NTxFrame = new byte[2];
        //                NTxFrame[0] = 0x36;
        //                NTxFrame[1] = (byte)(blkseqcnt & 0xFF);
        //                //var currenttransferlen = Math.Min(sector_num_bytes - bytestranferred, flashconfig_data.sectorframetransferlen );
        //                var currenttransferlen = Math.Min(sector_num_bytes - j, 944);

        //                Debug.WriteLine("currenttransferlen==" + currenttransferlen + 2, "ELMZ");

        //                Array.Resize(ref NTxFrame, Convert.ToInt32(currenttransferlen) + 2);

        //                Array.Copy(SectorDataArray, j, NTxFrame, 2, currenttransferlen);
        //                j += Convert.ToInt32(currenttransferlen);
        //                Debug.WriteLine("J==" + j, "ELMZ");

        //                //for (temp = 0; temp < currenttransferlen; temp++)
        //                //{

        //                //}
        //                //TB//Array.Copy(SectorDataArray[bytestranferred], 2, TxFrame, 2, Convert.ToInt32(currenttransferlen));

        //                //copyarray(sectordata.json[bytestranferred], TxFrame, 2, currenttransferlen)
        //                //j += Convert.ToInt32( currenttransferlen);
        //                //bytestranferred += Convert.ToUInt32(currenttransferlen);
        //                Frame_len = Convert.ToInt32(currenttransferlen + 2);


        //                //var ftt = HexStringToByteArray("43B43601000000000000000000000000000000005555AAAA00000000000000000000000000000000000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF000000000000000020FE60FFA000E001200360040005A0054006800700400040004000400040004000400040004000402003B00440060807D00798086009F00A800C100EF100830113025F02A602ED023303CB034704BE040008000A000CCC0D001000140018001C001E00207805780578057805780578057805780578057805EE02EE02EE02EE02EE02EE02EE02EE02EE02EE02C201C201C201C201C201C201C201C201C201C20100000000000000000000000000000000000000006AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF6AFF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF38FF4006D0076009F00A800CA00FC012E0157017401F20FE60FFA000E001200360040005A00540068007C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C800C8002C012C012C012C012C012C012C012C012C012C01DC01DC01DC01DC01DC01DC01DC01DC01DC01DC0158025802580258025802580258025802580258029C029C029C029C029C029C029C029C029C029C02250325032503250325032503250325032503250384038403840384038403840384038403840384038403840384038403840384038403840384038403002000240028002C003000340038003A003C00400040083F083B2937CD340E2F73279822971C0800000090012003B0044006D0076009F00A800C480DD80E6810C0125014E0157017E803780508076009F00A800CA00F941188137C1570176419581B401F1027E02E004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040004000400040A809");
        //                var frame = ByteArrayToString(NTxFrame);
        //                var bulkTransferResponse = await dongleCommWin.CAN_TxRx(Frame_len, frame);
        //                blkseqcnt++;

        //                if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
        //                {
        //                    Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
        //                    Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
        //                    return "bulkTransferResponse" + bulkTransferResponse.ECUResponseStatus;
        //                }
        //                //if(blkseqcnt==200)
        //                //{

        //                //}
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
        //        var TransferResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(BTxFrame));
        //        if (TransferResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------TransferResponse -ERROR------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("--------TransferResponse -LOOP END------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
        //            return "TransferResponse" + TransferResponse.ECUResponseStatus;
        //        }

        //        // Checksum test
        //        //TxFrame = new byte[10];
        //        //Frame_len = 0;
        //        //TxFrame[Frame_len++] = 0x31;
        //        //TxFrame[Frame_len++] = 0x01;
        //        //TxFrame[Frame_len++] = 0xFF;
        //        //TxFrame[Frame_len++] = 0x01;

        //        //if ((addrdataformat & 0xF0) == 0x30)
        //        //{

        //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

        //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
        //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
        //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x000000FF));

        //        //}
        //        //else
        //        //{
        //        //    Array.Resize(ref TxFrame, 12);
        //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
        //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //        //    TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //        //    TxFrame[Frame_len++] = (byte)(sector_start_address & 0x000000FF);

        //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0xFF000000) >> 24);
        //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
        //        //    TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
        //        //    TxFrame[Frame_len++] = (byte)(sector_end_address & 0x000000FF);
        //        //}

        //        //if (flashconfig_data.sendsectorchksum == true)
        //        //{
        //        //    //TxFrame[Frame_len++] = (byte)((sectordata[i].sectorchecksum & 0xFF00) >> 8);
        //        //    //TxFrame[Frame_len++] = (byte)(sectordata[i].sectorchecksum & 0x00FF);

        //        //    Frame_len += 2;
        //        //}

        //        //var sendsectorchksumResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    } // end of all sectors

        //    // Reset ECU
        //    TxFrame = new byte[2];
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x11;
        //    TxFrame[Frame_len++] = 0x02;

        //    var resetResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    if (resetResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------resetResponse -ERROR------------==" + resetResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("--------resetResponse -LOOP END------------==" + resetResponse.ECUResponseStatus, "ELMZ");
        //        return "resetResponse" + resetResponse.ECUResponseStatus;
        //    }

        //    string return_status = resetResponse.ECUResponseStatus;
        //    return ("ECU FLASHED" + return_status);
        //}

        #endregion

        #region StartFlashBosch
        //public async Task<string> StartFlashBosch(flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata)
        //{

        //    // switch to reprogramming mode - 10 02
        //    int Frame_len = 0;
        //    byte[] TxFrame = new byte[2];
        //    TxFrame[Frame_len++] = 0x10;
        //    TxFrame[Frame_len++] = 0x02;

        //    Debug.WriteLine("-------switch to reprogramming mode-------");
        //    var reprogrammingResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    if (reprogrammingResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- reprogrammingResponse ERROR------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------reprogrammingResponse LOOP END------------==" + reprogrammingResponse.ECUResponseStatus, "ELMZ");
        //        return "reprogrammingResponse" + reprogrammingResponse.ECUResponseStatus;
        //    }


        //    // get Seed - 27 09
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x27;
        //    TxFrame[Frame_len++] = flashconfig_data.sendseedbyte;

        //    Debug.WriteLine("-------get Seed-------");

        //    var getSeedResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    if (getSeedResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- getSeedResponse ERROR------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------getSeedResponse LOOP END------------==" + getSeedResponse.ECUResponseStatus, "ELMZ");
        //        return "getSeedResponse" + getSeedResponse.ECUResponseStatus;
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
        //    var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEX_TYPE.GREAVES_BOSCHBS6_PROD, 4, ref numkeybytes, seedarray, ref actualKey);

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
        //    var sendKeyResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    if (sendKeyResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------- sendKeyResponse ERROR------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("---------sendKeyResponse LOOP END------------==" + sendKeyResponse.ECUResponseStatus, "ELMZ");
        //        return "sendKeyResponse " + sendKeyResponse.ECUResponseStatus;
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

        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = 0x02;
        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = 0x00;

        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = 0x0F;
        //        TxFrame[Frame_len++] = 0xFF;
        //        TxFrame[Frame_len++] = 0xFF;


        //        var eraseMemoryResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //        if (eraseMemoryResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------- eraseMemoryResponse ERROR------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("---------eraseMemoryResponse LOOP END------------==" + eraseMemoryResponse.ECUResponseStatus, "ELMZ");
        //            return "eraseMemoryResponse " + eraseMemoryResponse.ECUResponseStatus;
        //        }
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

        //            if ((flashconfig_data.addrdataformat & 0xF0) == 0x30)
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

        //            var response2 = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //            if (response2?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------- response2 ERROR------------==" + response2.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("---------response2 LOOP END------------==" + response2.ECUResponseStatus, "ELMZ");
        //                return "response2 " + response2.ECUResponseStatus;
        //            }

        //            Frame_len = 0;
        //            TxFrame = new byte[4];
        //            TxFrame[Frame_len++] = 0x31;
        //            TxFrame[Frame_len++] = 0x03;
        //            TxFrame[Frame_len++] = 0xFF;
        //            TxFrame[Frame_len++] = 0x00;

        //            var checkerase = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //            if (checkerase?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------- checkerase ERROR------------==" + checkerase.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("---------checkerase LOOP END------------==" + checkerase.ECUResponseStatus, "ELMZ");
        //                return "checkerase " + response2.ECUResponseStatus;
        //            }

        //        }


        //        // Request download
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x34;
        //        TxFrame[Frame_len++] = 0x00;
        //        TxFrame[Frame_len++] = flashconfig_data.addrdataformat;

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


        //        var memoryResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //        if (memoryResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------- memoryResponse ERROR------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("---------memoryResponse LOOP END------------==" + memoryResponse.ECUResponseStatus, "ELMZ");
        //            return "memoryResponse " + memoryResponse.ECUResponseStatus;
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
        //                //var currenttransferlen = Math.Min(sector_num_bytes - bytestranferred, flashconfig_data.sectorframetransferlen );
        //                var currenttransferlen = Math.Min(sector_num_bytes - j, 252);

        //                Debug.WriteLine("currenttransferlen==" + currenttransferlen + 2, "ELMZ");

        //                Array.Resize(ref NTxFrame, Convert.ToInt32(currenttransferlen) + 2);

        //                Array.Copy(SectorDataArray, j, NTxFrame, 2, currenttransferlen);
        //                j += Convert.ToInt32(currenttransferlen);
        //                Debug.WriteLine("J==" + j, "ELMZ");

        //                Frame_len = Convert.ToInt32(currenttransferlen + 2);

        //                var frame = ByteArrayToString(NTxFrame);
        //                var bulkTransferResponse = await dongleCommWin.CAN_TxRx(Frame_len, frame);
        //                blkseqcnt++;

        //                if (bulkTransferResponse?.ECUResponseStatus != "NOERROR")
        //                {
        //                    Debug.WriteLine("--------bulkTransferResponse -ERROR------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
        //                    Debug.WriteLine("--------bulkTransferResponse -LOOP END------------==" + bulkTransferResponse.ECUResponseStatus, "ELMZ");
        //                    return "bulkTransferResponse" + bulkTransferResponse.ECUResponseStatus;
        //                }
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
        //        var TransferResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(BTxFrame));
        //        if (TransferResponse?.ECUResponseStatus != "NOERROR")
        //        {
        //            Debug.WriteLine("--------TransferResponse -ERROR------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
        //            Debug.WriteLine("--------TransferResponse -LOOP END------------==" + TransferResponse.ECUResponseStatus, "ELMZ");
        //            return "TransferResponse" + TransferResponse.ECUResponseStatus;
        //        }


        //        if ((flashconfig_data.checksumsector == ChecksumSectorEnum.COMPAREBYSECTOR) || (flashconfig_data.checksumsector == ChecksumSectorEnum.COMPUTEBYSECTOR))
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
        //            TxFrame[Frame_len++] = (byte)((sectordata[i].sectorchecksum & 0xFF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)(sectordata[i].sectorchecksum & 0x00FF);
        //        }

        //        if ((flashconfig_data.checksumsector == ChecksumSectorEnum.COMPAREBYSECTOR) || (flashconfig_data.checksumsector == ChecksumSectorEnum.COMPUTEBYSECTOR))
        //        {
        //            var sendsectorchksumResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //            if (TransferResponse?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------sendsectorchksumResponse -ERROR------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("--------sendsectorchksumResponse -LOOP END------------==" + sendsectorchksumResponse.ECUResponseStatus, "ELMZ");
        //                return "sendsectorchksumResponse" + sendsectorchksumResponse.ECUResponseStatus;
        //            }

        //            TxFrame = new byte[4];
        //            Frame_len = 0;
        //            TxFrame[Frame_len++] = 0x31;
        //            TxFrame[Frame_len++] = 0x03;
        //            TxFrame[Frame_len++] = 0xFF;
        //            TxFrame[Frame_len++] = 0x01;

        //            var sendsectorchksumResponse2 = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //            if (sendsectorchksumResponse2?.ECUResponseStatus != "NOERROR")
        //            {
        //                Debug.WriteLine("--------sendsectorchksumResponse2 -ERROR------------==" + sendsectorchksumResponse2.ECUResponseStatus, "ELMZ");
        //                Debug.WriteLine("--------sendsectorchksumResponse2 -LOOP END------------==" + sendsectorchksumResponse2.ECUResponseStatus, "ELMZ");
        //                return "sendsectorchksumResponse2" + sendsectorchksumResponse2.ECUResponseStatus;
        //            }
        //        }

        //    } // end of all sectors

        //    // Reset ECU
        //    TxFrame = new byte[2];
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x11;
        //    TxFrame[Frame_len++] = 0x02;

        //    var resetResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    if (resetResponse?.ECUResponseStatus != "NOERROR")
        //    {
        //        Debug.WriteLine("--------resetResponse -ERROR------------==" + resetResponse.ECUResponseStatus, "ELMZ");
        //        Debug.WriteLine("--------resetResponse -LOOP END------------==" + resetResponse.ECUResponseStatus, "ELMZ");
        //        return "resetResponse" + resetResponse.ECUResponseStatus;
        //    }

        //    string return_status = resetResponse.ECUResponseStatus;
        //    return ("ECU FLASHED" + return_status);
        //}

        #endregion


        //public async Task<string> StartFlash( flashconfig flashconfig_data, int noofsectors, FlashingMatrix[] sectordata)
        //{

        //    // switch to reprogramming mode - 10 02
        //    int Frame_len = 0;
        //    byte[] TxFrame = new byte[2];
        //    TxFrame[Frame_len++] = 0x10;
        //    TxFrame[Frame_len++] = 0x02;

        //    var flashResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    // get Seed - 27 09
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x27;
        //    TxFrame[Frame_len++] = flashconfig_data.sendseedbyte;

        //    flashResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    var seedarray = new byte[flashconfig_data.seedkeynumbytes] ;
        //    byte numkeybytes = new byte();

        //    Array.Copy(flashResponse.ActualDataBytes,2, seedarray,0,flashResponse.ActualDataBytes.Length-2 );

        //    //compute key for the seed received
        //    //status = getkeyfromseed(flashconfig_data.seedkeyindex, flashconfig_data.seedkeynumbytes, Rxarray + 2, keyarray);

        //    byte[] actualKey = new byte[flashconfig_data.seedkeynumbytes];
        //    calculateSeedkey = new ECUCalculateSeedkey();
        //    var result = calculateSeedkey.CalculateSeedkey(SEEDKEYINDEX_TYPE.GREAVES_ADVANTEK_A46_BS6, 4, ref numkeybytes, seedarray, ref actualKey);

        //    Array.Resize(ref TxFrame, flashconfig_data.seedkeynumbytes+2);//
        //    //send key 27 0A
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x27;
        //    TxFrame[Frame_len++] = (byte)(flashconfig_data.sendseedbyte + 1);
        //    for (int i = 0; i < actualKey.Length; i++)
        //    {
        //        TxFrame[Frame_len++] = actualKey[i];
        //    }
        //    //status = ISO15765_CANTxRx(channel, Frame_len, TxFrame, &Rxsize, Rxarray);
        //    var pidResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //    var addrdataformat = flashconfig_data.addrdataformat;
        //    for (int i = 0; i < noofsectors; i++)
        //    {
        //        var sector_start_address = Convert.ToUInt32(sectordata[i].JsonStartAddress,16);
        //        var sector_end_address = Convert.ToUInt32(sectordata[i].JsonEndAddress,16);
        //        var sector_num_bytes = Convert.ToUInt32(sectordata[i].JsonEndAddress,16) - Convert.ToUInt32(sectordata[i].JsonStartAddress,16)+1;

        //        // Erase Memory
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x31;
        //        TxFrame[Frame_len++] = 0x01;
        //        TxFrame[Frame_len++] = 0xFF;
        //        TxFrame[Frame_len++] = 0x00;

        //        if ((flashconfig_data.addrdataformat & 0xF0) == 0x30)
        //        {
        //            Array.Resize(ref TxFrame, 10);

        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)(sector_start_address & 0x000000FF);

        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)(sector_end_address & 0x000000FF);

        //        }
        //        else
        //        {
        //            Array.Resize(ref TxFrame, 12);

        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)(sector_start_address & 0x000000FF);

        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0xFF000000) >> 24);
        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)(sector_end_address & 0x000000FF);

        //        }

        //        pidResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //        // Request download
        //        TxFrame[0] = 0x34;
        //        TxFrame[1] = 0x00;
        //        TxFrame[2] = flashconfig_data.addrdataformat ;

        //        if (addrdataformat == 0x33)
        //        {
        //            Array.Resize(ref TxFrame, 9);
        //            TxFrame[3] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[4] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[5] = (byte)((sector_start_address & 0x000000FF));

        //            TxFrame[6] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
        //            TxFrame[7] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
        //            TxFrame[8] = (byte)((sector_num_bytes & 0x000000FF));

        //            Frame_len = 9;
        //        }
        //        else if (addrdataformat == 0x44) // addrdataformat == 0x44
        //        {
        //            Array.Resize(ref TxFrame, 13);
        //            TxFrame[5] = (byte)((sector_start_address & 0xFF000000) >> 24);
        //            TxFrame[6] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[7] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[8] = (byte)((sector_start_address & 0x000000FF));

        //            TxFrame[9] = (byte)((sector_num_bytes & 0xFF000000) >> 24);
        //            TxFrame[10] = (byte)((sector_num_bytes & 0x00FF0000) >> 16);
        //            TxFrame[11] = (byte)((sector_num_bytes & 0x00FF00) >> 8);
        //            TxFrame[12] = (byte)((sector_num_bytes & 0x000000FF));

        //            Frame_len = 13;
        //        }

        //        pidResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //        UInt32 bytestranferred = 0x000000;
        //        // Transfer Data in this sector
        //        for (int j = 0; j < sector_num_bytes; j++)
        //        {
        //            int temp = 0;
        //            int blkseqcnt = 0;


        //            TxFrame[0] = 36;
        //            TxFrame[1] = (byte)(blkseqcnt & 0xFF);
        //            var currenttransferlen = Math.Min(sector_num_bytes - bytestranferred, flashconfig_data.sectorframetransferlen );

        //            //for (temp = 0; temp < currenttransferlen; temp++)
        //            //{

        //            //}
        //            var SectorDataArray = HexStringToByteArray(sectordata[i].JsonData);
        //            //TB//Array.Copy(SectorDataArray[bytestranferred], 2, TxFrame, 2, Convert.ToInt32(currenttransferlen));

        //            //copyarray(sectordata.json[bytestranferred], TxFrame, 2, currenttransferlen)
        //            //j += Convert.ToInt32( currenttransferlen);
        //            bytestranferred += Convert.ToUInt32(currenttransferlen);
        //            Frame_len = Convert.ToInt32( currenttransferlen + 2);
        //            pidResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));
        //        }

        //        // Transfer Exit
        //        TxFrame[0] = 37;
        //        Frame_len = 1;
        //        pidResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //        // Checksum test
        //        Frame_len = 0;
        //        TxFrame[Frame_len++] = 0x31;
        //        TxFrame[Frame_len++] = 0x01;
        //        TxFrame[Frame_len++] = 0xFF;
        //        TxFrame[Frame_len++] = 0x01;

        //        if ((addrdataformat & 0xF0) == 0x30)
        //        {
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x000000FF));

        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0x000000FF));

        //        }
        //        else
        //        {
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0xFF000000) >> 24);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_start_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)(sector_start_address & 0x000000FF);

        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0xFF000000) >> 24);
        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF0000) >> 16);
        //            TxFrame[Frame_len++] = (byte)((sector_end_address & 0x00FF00) >> 8);
        //            TxFrame[Frame_len++] = (byte)(sector_end_address & 0x000000FF);
        //        }

        //        ////Fash if condition comment

        //        //if (flashconfig_data.sendsectorchksum == true)
        //        {
        //            //TxFrame[Frame_len++] = (byte)((sectordata[i].sectorchecksum & 0xFF00) >> 8);
        //            //TxFrame[Frame_len++] = (byte)(sectordata[i].sectorchecksum & 0x00FF);

        //            Frame_len += 2;
        //        }

        //        pidResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    } // end of all sectors

        //    // Reset ECU
        //    Frame_len = 0;
        //    TxFrame[Frame_len++] = 0x11;
        //    TxFrame[Frame_len++] = 0x01;

        //    pidResponse = await dongleCommWin.CAN_TxRx(Frame_len, ByteArrayToString(TxFrame));

        //    string return_status = pidResponse.ECUResponseStatus;
        //    return (return_status);
        //}

        //public async Task<ObservableCollection<byte[]>> WriteParameters(int noOFParameters, ObservableCollection<WriteParameterPID> readParameterCollection)
        //{
        //    foreach (var pidItem in readParameterCollection)
        //    {
        //        //Set Session
        //        var setSession = await dongleCommWin.SendCommand("1003");
        //        var sessionBytesResponse = (byte[])setSession;

        //        if (sessionBytesResponse[0] == 0x50)
        //        {
        //            //positive response
        //        }
        //        else
        //        {
        //            //negative respone
        //        }

        //        byte[] pid = ConvertToByteArray(pidItem.pid, Encoding.UTF8);

        //        //Request for the seed
        //        var requestSeed = await dongleCommWin.SendCommand("27" + pid[1]);
        //        var requestSeedBytesResponse = (byte[])requestSeed;

        //        //Copy the seed bytes
        //        var copySeedBytes = new byte[requestSeedBytesResponse.Length - 2];
        //        Array.Copy(requestSeedBytesResponse, 2, copySeedBytes, 0, requestSeedBytesResponse.Length - 2);

        //        //Call Api to calulate the key
        //        //Receive the Key from the server

        //        //send key to ECU
        //        var sendKeytoECU = await dongleCommWin.SendCommand("27yy+1+key");
        //        var keysFromECU = (byte[])sendKeytoECU;
        //        if (keysFromECU[0] == 0x67)
        //        {
        //            //positive response
        //        }
        //        else
        //        {
        //            //negative response}
        //        }
        //        var frameLength = pidItem.totalLen;
        //        var pidResponse = await dongleCommWin.CAN_TxRx(frameLength, "2E" + pidItem.pid);
        //        if (!pidItem.IsBitcoded)
        //        {
        //            var totalBytes = pidItem.totalBytes;
        //            var startBytes = pidItem.startByte;
        //            var noOfBytes = pidItem.noOfBytes;
        //            var dataByteArray = pidItem.dataByteArray;

        //            var byteArray = new byte[pidItem.totalBytes];
        //            Array.Copy(dataByteArray, startBytes, byteArray, 0, pidItem.noOfBytes);
        //        }
        //        else if (pidItem.IsBitcoded)
        //        {
        //            var totalBytes = pidItem.totalBytes;
        //            var startBytes = pidItem.startByte;
        //            var noOfBytes = pidItem.noOfBytes;

        //            var startBit = pidItem.startBit;
        //            var noOFBit = pidItem.noofBits;

        //            var dataByteArray = pidItem.dataByteArray;


        //            var byteArray = new byte[pidItem.totalBytes];
        //            Array.Copy(dataByteArray, startBytes, byteArray, 0, pidItem.noOfBytes);

        //            var bits = new BitArray(byteArray[startBytes]);

        //            string[] b = byteArray.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')).ToArray();
        //            //var bits = new BitArray(BitConverter.GetBytes(byteArray[startBytes]).ToArray());
        //            var replacedBits = new BitArray[noOFBit];



        //            //bits.CopyTo(replacedBits, 3);
        //            //string yourByteString = Convert.ToString(byteArray[startBytes], 2).PadLeft(8, '0');
        //            //// produces "00111111"

        //            //byte[] array = BitConverter.GetBytes(value);

        //        }

        //        return null;
        //    }


        //    return null;
        //}
    }

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
    //            var responseBytes = await this.dongleCommWin.CAN_TxRx(frameLength, "1902FF");
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
    //                            dataValue = float_pid_value.ToString();
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
    //                            if(dataValue.Contains('\0') == true)
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

    //                                for(byte i = 0; i< 7; i++)
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
    //                    var currenttransferlen = Math.Min(sector_num_bytes - j, flashconfig_data.sectorframetransferlen );
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
}
