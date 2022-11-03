using System;
using System.Diagnostics;

namespace APDongleCommAnroid.Helper
{
    public static class ResponseArrayDecoding
    {
        public static bool firstposdongleackreceived = false;
        public static string val = string.Empty;
        public static string HalfActualRespons = string.Empty;
        public static int ActualLenth = 0;
        public static void CheckResponse(byte[] pidBytesResponse, byte[] requestBytes, out byte[] dataArray, out string status)
        {
            try
            {
                var responeBytes = pidBytesResponse;
                var request = requestBytes;
                string expectedframe = string.Empty;
                string reqtype = string.Empty;
                int nextpacketindex = 0;
                dataArray = pidBytesResponse;
                status = "NOERROR";
                bool endofpacket = false;
                int framelen = 0;


                if (request[0] == 0x20)
                {
                    reqtype = "CONFIGREQUEST";
                    switch (request[2])
                    {
                        case 0x01: // Reset Dongle
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x02: // Set Protocol
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x03: // Get Protocol
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x04: // Set Transmit Header
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x05: // Get Transmit Header
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x06: // Set Receive Header
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x07: // Get Receive Header
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x08: // Set Block length in flow control frame
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x09: // Get Block length in flow control frame
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x0A: // Set separation time between two consecutive frame
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x0B: // Get separation time between two consecutive frame
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x0C: // Set Min time between two transmit frame
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x0D: // Get Min time between two transmit frame
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x0E: // Set Max wait time between req and resp
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x0F: // Get Max wait time between req and resp
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x10: // Transmit Test present message periodically after every p1 (only in case of diagnstics protocols)
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x11: // Stop periodic tester present message being sent
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x12: // Pad transmit messsage with hh to maintain standard byte length
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x13: // Stop message padding 
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x14: // Get firmware version 
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x15: // Bluetooth name change
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x17: // Set multiple filters for rxheader (max 15)
                            expectedframe = "DONGLECONFIGACK";
                            break;
                    };
                }


                else if (request[0] == 0x40)
                {
                    reqtype = "DATAREQUEST";
                    expectedframe = "DONGLECONFIGACK";
                }
                /* Initialize parser index and run parser */

                nextpacketindex = 0;
                endofpacket = false;
                /* start of the parser */

                while (endofpacket == false)
                {
                    //if (nextpacketindex > responeBytes.Length) // check - originally was (nextpacketindex > responeBytes.Length - 1)
                    //{
                    //    status = "READAGAIN";
                    //    endofpacket = true;
                    //    break;
                    //}
                    if (responeBytes[nextpacketindex] == 0x20)
                    {
                        framelen = responeBytes[nextpacketindex + 1];
                        /* valid response from dongle */
                        /* look at second byte of the response */
                        if (reqtype == "CONFIGREQUEST")
                        {
                            endofpacket = true;
                            if (expectedframe == "DONGLECONFIGRESPONSE")
                            {
                                /* send expected response bytes */

                                //reponsearray[0 to framelen - 2] = byte[nextpacketindex + 3 to framelen - 2]
                                int length = framelen - 2;
                                byte[] responseArray = new byte[length];


                                Array.Copy(responeBytes, nextpacketindex + 3, responseArray, 0, framelen - 2);
                                var actualRespone = responseArray;
                                dataArray = actualRespone;
                                endofpacket = true;
                            }
                        }
                        else if (responeBytes[nextpacketindex + 1] == 0x03) // request type is a DATAREQUEST and dongle positive acknowledgement
                        {
                            if (firstposdongleackreceived == true)
                            {
                                firstposdongleackreceived = false;
                                endofpacket = true;
                                status = "NOERROR";

                                Debug.WriteLine("------ Jugaad - Getting positive dongle ack frame from dongle for second time -------");
                                break;
                                /* Jugaad - this is bypassing the wrong second response from the dongle. which should have been a 4xxx frame */
                                /* once hte dongle response is corrected, we can remove this if and also the variable references of firstposdongleackreceived */
                            }

                            nextpacketindex += 5;
                            // endofpacket = false; --- Test
                            firstposdongleackreceived = true;
                            /* postive acknowledgement from dongle */



                            /* ********************************* Test */
                            if (nextpacketindex < responeBytes.Length)
                            {
                                endofpacket = false;
                            }
                            else
                            {
                                status = "READAGAIN";
                                endofpacket = true;
                                break;
                            }

                            /* ********************************* Test */

                        }
                        else // request type is a DATAREQUEST and dongle negative acknowledgement
                        {
                            endofpacket = true;
                            firstposdongleackreceived = false;
                            /* Negative acknowledgement from dongle */
                            switch (responeBytes[3])
                            {
                                case 0x10:
                                    status = "DONGLEERROR_COMMANDNOTSUPPORTED";
                                    break;
                                case 0x12:
                                    status = "DONGLEERROR_INPUTNOTSUPPORTED";
                                    break;
                                case 0x13:
                                    status = "DONGLEERROR_INVALIDFORMAT";
                                    status = "SENDAGAIN";
                                    break;
                                case 0x14:
                                    status = "DONGLEERROR_INVALIDOPERATION";
                                    break;
                                case 0x15:
                                    status = "DONGLEERROR_CRCFAILURE";
                                    /* send the same request again for 3 times. if we still get the same error, then get out of the function with this same error code */

                                    status = "SENDAGAIN";
                                    break;
                                case 0x16:
                                    status = "DONGLEERROR_PROTOCOLNOTSET";
                                    break;
                                case 0x33:
                                    status = "DONGLEERROR_SECURITYACCESSDENIED";
                                    break;
                            }

                            status = "SENDAGAIN";
                        }
                    }
                    /* IF there is no response from the ECU for P2MAX time */
                    else if (((responeBytes[nextpacketindex] & 0xF0) == 0x40) && (responeBytes[nextpacketindex + 1] == 0x02))
                    {

                        status = "ECUERROR_NORESPONSEFROMECU";
                        endofpacket = true;
                        firstposdongleackreceived = false;
                        break;
                    }


                    /* if the request was a data frame starting with 4x, (not a config frame that starts with 2x), hten we have to expect data response from ecu */
                    else if (((responeBytes[nextpacketindex] & 0xF0) == 0x40) || !string.IsNullOrEmpty(HalfActualRespons))
                    {
                        firstposdongleackreceived = false;
                        /* compute data message len xxx in 4x xx*/

                        var msglen = ((responeBytes[nextpacketindex] & 0x0F) << 8) + responeBytes[nextpacketindex + 1];
                        framelen = msglen;
                        if (string.IsNullOrEmpty(HalfActualRespons))
                        {
                            ActualLenth = msglen;
                            //msglen = ((HexStringToByteArray(HalfActualRespons)[nextpacketindex] & 0x0F) << 8) + HexStringToByteArray(HalfActualRespons)[nextpacketindex + 1];
                        }
                        //else
                        //{
                        //    //framelen =
                        //}
                        if (responeBytes[nextpacketindex + 2] == 0x7F)
                        {
                            if (responeBytes[nextpacketindex + 4] == 0x78)
                            {
                                /* read next packet */
                                nextpacketindex += 7;
                                endofpacket = false;
                                if (nextpacketindex > responeBytes.Length - 1)
                                {
                                    status = "READAGAIN";
                                    endofpacket = true;

                                    break;
                                }

                            }
                            else
                            {
                                endofpacket = true;
                                /* this is ecu negative repsonse return to the parent function with appropriate status */
                                switch (responeBytes[nextpacketindex + 4])
                                {
                                    case 0x10:
                                        status = "ECUERROR_GENERALREJECT";
                                        break;
                                    case 0x11:
                                        status = "ECUERROR_SERVICENOTSUPPORTED";
                                        break;
                                    case 0x12:
                                        status = "ECUERROR_SUBFUNCTIONNOTSUPPORTED";

                                        break;
                                    case 0x13:
                                        status = "ECUERROR_INVALIDFORMAT";
                                        break;
                                    case 0x14:
                                        status = "ECUERROR_RESPONSETOOLONG";
                                        break;
                                    case 0x21:
                                        status = "ECUERROR_BUSYREPEATREQUEST";
                                        break;
                                    case 0x22:
                                        status = "ECUERROR_CONDITIONSNOTCORRECT";
                                        break;
                                    case 0x24:
                                        status = "ECUERROR_REQUESTSEQUENCEERROR";
                                        break;
                                    case 0x31:
                                        status = "ECUERROR_REQUESTOUTOFRANGE";
                                        break;
                                    case 0x33:
                                        status = "ECUERROR_SECURITYACCESSDENIED";
                                        break;
                                    case 0x35:
                                        status = "ECUERROR_INVALIDKEY";
                                        break;
                                    case 0x36:
                                        status = "ECUERROR_EXCEEDEDNUMBEROFATTEMPTS";
                                        break;
                                    case 0x37:
                                        status = "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED";
                                        break;
                                    case 0x70:
                                        status = "ECUERROR_UPLOADDOWNLOADNOTACCEPTED";
                                        break;
                                    case 0x71:
                                        status = "ECUERROR_TRANSFERDATASUSPENDED";
                                        break;
                                    case 0x72:
                                        status = "ECUERROR_GENERALPROGRAMMINGFAILURE";
                                        break;
                                    case 0x73:
                                        status = "ECUERROR_WRONGBLOCKSEQCOUNTER";
                                        break;
                                    case 0x7E:
                                        status = "ECUERROR_SUBFNNOTSUPPORTEDINACTIVESESSION";
                                        break;
                                    case 0x7F:
                                        status = "ECUERROR_SERVICENOTSUPPORTEDINACTIVESESSION";
                                        break;
                                    case 0x81:
                                        status = "ECUERROR_RPMTOOHIGH";
                                        break;
                                    case 0x82:
                                        status = "ECUERROR_RPMTOOLOW";
                                        break;
                                    case 0x83:
                                        status = "ECUERROR_ENGINEISRUNNING";
                                        break;
                                    case 0x84:
                                        status = "ECUERROR_ENGINEISNOTRUNNING";
                                        break;
                                    case 0x85:
                                        status = "ECUERROR_ENGINERUNTIMETOOLOW";
                                        break;
                                    case 0x86:
                                        status = "ECUERROR_TEMPTOOHIGH";
                                        break;
                                    case 0x87:
                                        status = "ECUERROR_TEMPTOOLOW";
                                        break;
                                    case 0x88:
                                        status = "ECUERROR_VEHSPEEDTOOHIGH";
                                        break;
                                    case 0x89:
                                        status = "ECUERROR_VEHSPEEDTOOLOW";
                                        break;
                                    case 0x8A:
                                        status = "ECUERROR_THROTTLETOOHIGH";
                                        break;
                                    case 0x8B:
                                        status = "ECUERROR_THROTTLETOOLOW";
                                        break;
                                    case 0x8C:
                                        status = "ECUERROR_TRANSMISSIONRANGENOTINNEUTRAL";
                                        break;
                                    case 0x8D:
                                        status = "ECUERROR_TRANSMISSIONRANGENOTINGEAR";
                                        break;
                                    case 0x8F:
                                        status = "ECUERROR_BRKPEDALNOTPRESSED";
                                        break;
                                    case 0x90:
                                        status = "ECUERROR_SHIFTERLEVERNOTINPARK";
                                        break;
                                    case 0x91:
                                        status = "ECUERROR_TRQCONVERTERCLUTCHLOCKED";
                                        break;
                                    case 0x92:
                                        status = "ECUERROR_VOLTAGETOOHIGH";
                                        break;
                                    case 0x93:
                                        status = "ECUERROR_VOLTAGETOOLOW";
                                        break;

                                }


                            }
                        }
                        else /* positive response from the dongle */
                        {
                            /* check if we have received the entire packet of data as per */
                            //if()
                            //{
                            //    nextpacketindex += framelen;
                            //    endofpacket = false;
                            //    if (nextpacketindex < responeBytes.Length - 1)
                            //    {
                            //        status = "READAGAIN";
                            //        endofpacket = true;
                            //        Array.Copy(responeBytes, nextpacketindex + 2, responseArray, 0, framelen - 2);
                            //        var actualRespone = responseArray;
                            //        dataArray = actualRespone;

                            //        break;
                            //    }
                            //}
                            //else
                            //{

                            endofpacket = true;
                            //reponsearray[0 to framelen - 2] = byte[nextpacketindex + 3 to framelen - 2]
                            int length = framelen - 2;
                            byte[] responseArray = new byte[length];
                            //Console.WriteLine($"responeBytes : { ByteArrayToString(responeBytes) }");
                            //Console.WriteLine($"nextpacketindex : {nextpacketindex}");
                            //Console.WriteLine($"responseArray : {ByteArrayToString(responseArray)}");
                            //Console.WriteLine($"framelen  : {framelen }");
                            val = $"{ByteArrayToString(responeBytes)}, {nextpacketindex + 2}, {ByteArrayToString(responseArray)}, 0 ,{framelen - 2}";

                            Debug.Write($"Array Copy Detail\n{val}", "Checksum Method : ");



                            if (!string.IsNullOrEmpty(HalfActualRespons))
                            {
                                Debug.Write("Add Remaining Half Packat", "Checksum Method : ");
                                HalfActualRespons += ByteArrayToString(responeBytes);
                                Debug.Write($"Added Remaining Half Packat {HalfActualRespons}", "Checksum Method : ");
                                responeBytes = HexStringToByteArray(HalfActualRespons);
                                Array.Resize(ref responseArray, ActualLenth);
                                framelen = ActualLenth;//responeBytes.Length;
                                //HalfActualRespons = string.Empty;
                            }
                            val = $"{ByteArrayToString(responeBytes)}, {nextpacketindex + 2}, {ByteArrayToString(responseArray)}, 0 ,{framelen - 2}";
                            if ((responeBytes.Length - 2) < (int)responeBytes[1])
                            {
                                status = "READAGAIN";
                                Debug.Write("Set Remaining Half Packat", "Checksum Method : ");
                                HalfActualRespons = ByteArrayToString(responeBytes);

                                Debug.Write($"1 Set Remaining Half Packat {HalfActualRespons}", "Checksum Method : ");
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(HalfActualRespons))
                                {
                                    HalfActualRespons = string.Empty;
                                    ActualLenth = 0;
                                }

                                status = "NOERROR";
                                Debug.Write("Copy Remaining Response Array", "Checksum Method : ");
                                //Array.Resize(ref responeBytes, 2);
                                Array.Copy(responeBytes, nextpacketindex + 2, responseArray, 0, framelen - 2);
                                Debug.Write("Copied Remaining Response Array", "Checksum Method : ");
                                var actualRespone = responseArray;
                                dataArray = actualRespone;
                            }


                            //}
                        }
                    }


                    //else if (responeBytes[])
                    else
                    {
                        /* unexpected dongle response*/
                        status = "GENERALERROR_INVALIDRESPFROMDONGLE";
                        firstposdongleackreceived = false;
                    }
                } /* while (endofpacket == true) */

                string ByteArrayToString(byte[] ba)
                {
                    string hex = BitConverter.ToString(ba);
                    return hex.Replace("-", "");
                }

                byte[] HexStringToByteArray(String hex)
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
            catch (Exception ex)
            {
                dataArray = null;
                status = val + "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
            }

        }

        public static void CheckResponseRP1210(byte[] pidBytesResponse, byte[] requestBytes, out byte[] dataArray, out string status)
        {
            dataArray = pidBytesResponse;
            status = "NOERROR";
            try
            {
                var responeBytes = pidBytesResponse;
                var request = requestBytes;
                int nextpacketindex = 0;
                bool endofpacket = false;
                int framelen = 0;

                while (endofpacket == false)
                {
                    if (responeBytes[nextpacketindex] == 0x7F)
                    {
                        if (responeBytes[nextpacketindex + 2] == 0x78)
                        {
                            /* read next packet */
                            nextpacketindex += 7;
                            endofpacket = false;
                            if (nextpacketindex > responeBytes.Length - 1)
                            {
                                status = "READAGAIN";
                                endofpacket = true;

                                break;
                            }
                        }
                        else
                        {
                            endofpacket = true;
                            /* this is ecu negative repsonse return to the parent function with appropriate status */
                            switch (responeBytes[nextpacketindex + 2])
                            {
                                case 0x10:
                                    status = "ECUERROR_GENERALREJECT";
                                    break;
                                case 0x11:
                                    status = "ECUERROR_SERVICENOTSUPPORTED";
                                    break;
                                case 0x12:
                                    status = "ECUERROR_SUBFUNCTIONNOTSUPPORTED";

                                    break;
                                case 0x13:
                                    status = "ECUERROR_INVALIDFORMAT";
                                    break;
                                case 0x14:
                                    status = "ECUERROR_RESPONSETOOLONG";
                                    break;
                                case 0x21:
                                    status = "ECUERROR_BUSYREPEATREQUEST";
                                    break;
                                case 0x22:
                                    status = "ECUERROR_CONDITIONSNOTCORRECT";
                                    break;
                                case 0x24:
                                    status = "ECUERROR_REQUESTSEQUENCEERROR";
                                    break;
                                case 0x31:
                                    status = "ECUERROR_REQUESTOUTOFRANGE";
                                    break;
                                case 0x33:
                                    status = "ECUERROR_SECURITYACCESSDENIED";
                                    break;
                                case 0x35:
                                    status = "ECUERROR_INVALIDKEY";
                                    break;
                                case 0x36:
                                    status = "ECUERROR_EXCEEDEDNUMBEROFATTEMPTS";
                                    break;
                                case 0x37:
                                    status = "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED";
                                    break;
                                case 0x70:
                                    status = "ECUERROR_UPLOADDOWNLOADNOTACCEPTED";
                                    break;
                                case 0x71:
                                    status = "ECUERROR_TRANSFERDATASUSPENDED";
                                    break;
                                case 0x72:
                                    status = "ECUERROR_GENERALPROGRAMMINGFAILURE";
                                    break;
                                case 0x73:
                                    status = "ECUERROR_WRONGBLOCKSEQCOUNTER";
                                    break;
                                case 0x7E:
                                    status = "ECUERROR_SUBFNNOTSUPPORTEDINACTIVESESSION";
                                    break;
                                case 0x7F:
                                    status = "ECUERROR_SERVICENOTSUPPORTEDINACTIVESESSION";
                                    break;
                                case 0x81:
                                    status = "ECUERROR_RPMTOOHIGH";
                                    break;
                                case 0x82:
                                    status = "ECUERROR_RPMTOOLOW";
                                    break;
                                case 0x83:
                                    status = "ECUERROR_ENGINEISRUNNING";
                                    break;
                                case 0x84:
                                    status = "ECUERROR_ENGINEISNOTRUNNING";
                                    break;
                                case 0x85:
                                    status = "ECUERROR_ENGINERUNTIMETOOLOW";
                                    break;
                                case 0x86:
                                    status = "ECUERROR_TEMPTOOHIGH";
                                    break;
                                case 0x87:
                                    status = "ECUERROR_TEMPTOOLOW";
                                    break;
                                case 0x88:
                                    status = "ECUERROR_VEHSPEEDTOOHIGH";
                                    break;
                                case 0x89:
                                    status = "ECUERROR_VEHSPEEDTOOLOW";
                                    break;
                                case 0x8A:
                                    status = "ECUERROR_THROTTLETOOHIGH";
                                    break;
                                case 0x8B:
                                    status = "ECUERROR_THROTTLETOOLOW";
                                    break;
                                case 0x8C:
                                    status = "ECUERROR_TRANSMISSIONRANGENOTINNEUTRAL";
                                    break;
                                case 0x8D:
                                    status = "ECUERROR_TRANSMISSIONRANGENOTINGEAR";
                                    break;
                                case 0x8F:
                                    status = "ECUERROR_BRKPEDALNOTPRESSED";
                                    break;
                                case 0x90:
                                    status = "ECUERROR_SHIFTERLEVERNOTINPARK";
                                    break;
                                case 0x91:
                                    status = "ECUERROR_TRQCONVERTERCLUTCHLOCKED";
                                    break;
                                case 0x92:
                                    status = "ECUERROR_VOLTAGETOOHIGH";
                                    break;
                                case 0x93:
                                    status = "ECUERROR_VOLTAGETOOLOW";
                                    break;

                            }
                        }
                    }
                    else
                    {
                        status = "NOERROR";
                        endofpacket = true;
                    }

                }
            }
            catch (Exception ex)
            {
                dataArray = null;
                status = "\n" + ex.Message + "\n\n" + ex.StackTrace;
            }
        }


        public static void CheckResponseIVN(byte[] pidBytesResponse, byte[] requestBytes, string reqtype1, out byte[] dataArray, out string status)
        {
            try
            {
                var responeBytes = pidBytesResponse;
                var request = requestBytes;
                string expectedframe = string.Empty;
                string reqtype = string.Empty;
                int nextpacketindex = 0;
                dataArray = null;
                status = "NOERROR";
                bool endofpacket = false;
                int framelen = 0;

                if (request[0] == 0x20)
                {
                    //reqtype = "CONFIGREQUEST";
                    switch (request[2])
                    {
                        case 0x01: // Reset Dongle
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x02: // Set Protocol
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x03: // Get Protocol
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x04: // Set Transmit Header
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x05: // Get Transmit Header
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x06: // Set Receive Header
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x07: // Get Receive Header
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x08: // Set Block length in flow control frame
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x09: // Get Block length in flow control frame
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x0A: // Set separation time between two consecutive frame
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x0B: // Get separation time between two consecutive frame
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x0C: // Set Min time between two transmit frame
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x0D: // Get Min time between two transmit frame
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x0E: // Set Max wait time between req and resp
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x0F: // Get Max wait time between req and resp
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x10: // Transmit Test present message periodically after every p1 (only in case of diagnstics protocols)
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x11: // Stop periodic tester present message being sent
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x12: // Pad transmit messsage with hh to maintain standard byte length
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x13: // Stop message padding 
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x14: // Get firmware version 
                            expectedframe = "DONGLECONFIGRESPONSE";
                            break;
                        case 0x15: // Bluetooth name change
                            expectedframe = "DONGLECONFIGACK";
                            break;
                        case 0x17: // Set multiple filters for rxheader (max 15)
                            expectedframe = "DONGLECONFIGACK";
                            break;
                    };
                }


                else if (request[0] == 0x40)
                {
                    reqtype = "DATAREQUEST";
                    expectedframe = "DONGLECONFIGACK";
                }
                /* Initialize parser index and run parser */

                nextpacketindex = 0;
                endofpacket = false;
                /* start of the parser */
                while (endofpacket == false)
                {
                    //if (nextpacketindex > responeBytes.Length) // check - originally was (nextpacketindex > responeBytes.Length - 1)
                    //{
                    //    status = "READAGAIN";
                    //    endofpacket = true;
                    //    break;
                    //}
                    if (responeBytes[nextpacketindex] == 0x20)
                    {
                        framelen = responeBytes[nextpacketindex + 1];
                        /* valid response from dongle */
                        /* look at second byte of the response */
                        var count = responeBytes.Length;

                        //if (count > 5)
                        //{
                        //    if (responeBytes[5] == 0x40)
                        //    {
                        //        reqtype = "DATAREQUEST";
                        //        expectedframe = "DONGLECONFIGACK";
                        //    }
                        //}

                        //reqtype
                        if (reqtype == "CONFIGREQUEST")
                        {
                            endofpacket = true;
                            if (expectedframe == "DONGLECONFIGRESPONSE")
                            {
                                /* send expected response bytes */

                                //reponsearray[0 to framelen - 2] = byte[nextpacketindex + 3 to framelen - 2]
                                int length = framelen - 2;
                                byte[] responseArray = new byte[length];

                                Array.Copy(responeBytes, nextpacketindex + 3, responseArray, 0, framelen - 2);
                                var actualRespone = responseArray;
                                dataArray = actualRespone;
                                endofpacket = true;
                            }
                        }
                        else if (responeBytes[nextpacketindex + 1] == 0x03) // request type is a DATAREQUEST and dongle positive acknowledgement
                        {
                            if (firstposdongleackreceived == true)
                            {
                                firstposdongleackreceived = false;
                                endofpacket = true;
                                status = "NOERROR";

                                Debug.WriteLine("------ Jugaad - Getting positive dongle ack frame from dongle for second time -------");
                                break;
                                /* Jugaad - this is bypassing the wrong second response from the dongle. which should have been a 4xxx frame */
                                /* once hte dongle response is corrected, we can remove this if and also the variable references of firstposdongleackreceived */
                            }

                            nextpacketindex += 5;
                            // endofpacket = false; --- Test
                            firstposdongleackreceived = true;
                            /* postive acknowledgement from dongle */



                            /* ********************************* Test */
                            if (nextpacketindex < responeBytes.Length)
                            {
                                endofpacket = false;
                            }
                            else
                            {
                                status = "READAGAIN";
                                endofpacket = true;
                                break;
                            }

                            /* ********************************* Test */

                        }

                        else // request type is a DATAREQUEST and dongle negative acknowledgement
                        {
                            endofpacket = true;
                            firstposdongleackreceived = false;
                            /* Negative acknowledgement from dongle */
                            switch (responeBytes[3])
                            {
                                case 0x10:
                                    status = "DONGLEERROR_COMMANDNOTSUPPORTED";
                                    break;
                                case 0x12:
                                    status = "DONGLEERROR_INPUTNOTSUPPORTED";
                                    break;
                                case 0x13:
                                    status = "DONGLEERROR_INVALIDFORMAT";
                                    break;
                                case 0x14:
                                    status = "DONGLEERROR_INVALIDOPERATION";
                                    break;
                                case 0x15:
                                    status = "DONGLEERROR_CRCFAILURE";
                                    /* send the same request again for 3 times. if we still get the same error, then get out of the function with this same error code */

                                    status = "SENDAGAIN";
                                    break;
                                case 0x16:
                                    status = "DONGLEERROR_PROTOCOLNOTSET";
                                    break;
                                case 0x33:
                                    status = "DONGLEERROR_SECURITYACCESSDENIED";
                                    break;
                            }
                            status = "SENDAGAIN";


                        }
                    }
                    /* IF there is no response from the ECU for P2MAX time */
                    else if (((responeBytes[nextpacketindex] & 0xF0) == 0x40) && (responeBytes[nextpacketindex + 1] == 0x02))
                    {

                        status = "ECUERROR_NORESPONSEFROMECU";
                        endofpacket = true;
                        firstposdongleackreceived = false;
                        break;
                    }


                    /* if the request was a data frame starting with 4x, (not a config frame that starts with 2x), hten we have to expect data response from ecu */
                    else if ((responeBytes[nextpacketindex] & 0xF0) == 0x40)
                    {
                        firstposdongleackreceived = false;
                        /* compute data message len xxx in 4x xx*/
                        var msglen = ((responeBytes[nextpacketindex] & 0x0F) << 8) + responeBytes[nextpacketindex + 1];
                        framelen = msglen;
                        if (responeBytes[nextpacketindex + 2] == 0x7F)
                        {
                            if (responeBytes[nextpacketindex + 4] == 0x78)
                            {
                                /* read next packet */
                                nextpacketindex += 7;
                                endofpacket = false;
                                if (nextpacketindex > responeBytes.Length - 1)
                                {
                                    status = "READAGAIN";
                                    endofpacket = true;

                                    break;
                                }

                            }
                            else
                            {
                                endofpacket = true;
                                /* this is ecu negative repsonse return to the parent function with appropriate status */
                                switch (responeBytes[nextpacketindex + 4])
                                {
                                    case 0x10:
                                        status = "ECUERROR_GENERALREJECT";
                                        break;
                                    case 0x11:
                                        status = "ECUERROR_SERVICENOTSUPPO0RTED";
                                        break;
                                    case 0x12:
                                        status = "ECUERROR_SUBFUNCTIONNOTSUPPORTED";

                                        break;
                                    case 0x13:
                                        status = "ECUERROR_INVALIDFORMAT";
                                        break;
                                    case 0x14:
                                        status = "ECUERROR_RESPONSETOOLONG";
                                        break;
                                    case 0x21:
                                        status = "ECUERROR_BUSYREPEATREQUEST";
                                        break;
                                    case 0x22:
                                        status = "ECUERROR_CONDITIONSNOTCORRECT";
                                        break;
                                    case 0x24:
                                        status = "ECUERROR_REQUESTSEQUENCEERROR";
                                        break;
                                    case 0x31:
                                        status = "ECUERROR_REQUESTOUTOFRANGE";
                                        break;
                                    case 0x33:
                                        status = "ECUERROR_SECURITYACCESSDENIED";
                                        break;
                                    case 0x35:
                                        status = "ECUERROR_INVALIDKEY";
                                        break;
                                    case 0x36:
                                        status = "ECUERROR_EXCEEDEDNUMBEROFATTEMPTS";
                                        break;
                                    case 0x37:
                                        status = "ECUERROR_REQUIREDTIMEDELAYNOTEXPIRED";
                                        break;
                                    case 0x70:
                                        status = "ECUERROR_UPLOADDOWNLOADNOTACCEPTED";
                                        break;
                                    case 0x71:
                                        status = "ECUERROR_TRANSFERDATASUSPENDED";
                                        break;
                                    case 0x72:
                                        status = "ECUERROR_GENERALPROGRAMMINGFAILURE";
                                        break;
                                    case 0x73:
                                        status = "ECUERROR_WRONGBLOCKSEQCOUNTER";
                                        break;
                                    case 0x7E:
                                        status = "ECUERROR_SUBFNNOTSUPPORTEDINACTIVESESSION";
                                        break;
                                    case 0x7F:
                                        status = "ECUERROR_SERVICENOTSUPPORTEDINACTIVESESSION";
                                        break;
                                    case 0x81:
                                        status = "ECUERROR_RPMTOOHIGH";
                                        break;
                                    case 0x82:
                                        status = "ECUERROR_RPMTOOLOW";
                                        break;
                                    case 0x83:
                                        status = "ECUERROR_ENGINEISRUNNING";
                                        break;
                                    case 0x84:
                                        status = "ECUERROR_ENGINEISNOTRUNNING";
                                        break;
                                    case 0x85:
                                        status = "ECUERROR_ENGINERUNTIMETOOLOW";
                                        break;
                                    case 0x86:
                                        status = "ECUERROR_TEMPTOOHIGH";
                                        break;
                                    case 0x87:
                                        status = "ECUERROR_TEMPTOOLOW";
                                        break;
                                    case 0x88:
                                        status = "ECUERROR_VEHSPEEDTOOHIGH";
                                        break;
                                    case 0x89:
                                        status = "ECUERROR_VEHSPEEDTOOLOW";
                                        break;
                                    case 0x8A:
                                        status = "ECUERROR_THROTTLETOOHIGH";
                                        break;
                                    case 0x8B:
                                        status = "ECUERROR_THROTTLETOOLOW";
                                        break;
                                    case 0x8C:
                                        status = "ECUERROR_TRANSMISSIONRANGENOTINNEUTRAL";
                                        break;
                                    case 0x8D:
                                        status = "ECUERROR_TRANSMISSIONRANGENOTINGEAR";
                                        break;
                                    case 0x8F:
                                        status = "ECUERROR_BRKPEDALNOTPRESSED";
                                        break;
                                    case 0x90:
                                        status = "ECUERROR_SHIFTERLEVERNOTINPARK";
                                        break;
                                    case 0x91:
                                        status = "ECUERROR_TRQCONVERTERCLUTCHLOCKED";
                                        break;
                                    case 0x92:
                                        status = "ECUERROR_VOLTAGETOOHIGH";
                                        break;
                                    case 0x93:
                                        status = "ECUERROR_VOLTAGETOOLOW";
                                        break;

                                }


                            }
                        }
                        else /* positive response from the dongle */
                        {
                            endofpacket = true;
                            //reponsearray[0 to framelen - 2] = byte[nextpacketindex + 3 to framelen - 2]
                            int length = framelen - 2;
                            byte[] responseArray = new byte[length];

                            val = $"{ByteArrayToString(responeBytes)}, {nextpacketindex}, {ByteArrayToString(responseArray)}, {framelen}";
                            Array.Copy(responeBytes, nextpacketindex + 2, responseArray, 0, framelen - 2);

                            var actualRespone = responseArray;
                            dataArray = actualRespone;
                        }
                    }
                    else
                    {
                        /* unexpected dongle response*/
                        status = "GENERALERROR_INVALIDRESPFROMDONGLE";
                        firstposdongleackreceived = false;
                    }
                } /* while (endofpacket == true) */
            }
            catch (Exception ex)
            {
                dataArray = null;
                status = val + "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
            }

        }

        private static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }


    }
}
