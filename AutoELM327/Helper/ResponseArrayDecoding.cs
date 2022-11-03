using System;
using System.Collections.Generic;
using System.Text;

namespace AutoELM327.Helper
{
    public static class ResponseArrayDecoding
    {
        public static void CheckResponse(byte[] pidBytesResponse, out byte[] dataArray, out string status)
        {
          
                var responeBytes = pidBytesResponse;
                var responselen = pidBytesResponse.Length;

                string expectedframe = string.Empty;
                string reqtype = string.Empty;
                int nextpacketindex = 0;
                dataArray = null;
                status = "NOERROR";
                bool endofpacket = false;
                int framelen = 0;
                /* Initialize parser index and run parser */

                nextpacketindex = 0;
                endofpacket = false;
                /* start of the parser */

                while (endofpacket == false)
                {
                    if (responeBytes[nextpacketindex] == 0x7F)
                    {
                        if (responeBytes[nextpacketindex + 2] == 0x78)
                        {
                            /* read next packet */

                            nextpacketindex += 3;
                            if (nextpacketindex >= responeBytes.Length)
                            {
                                status = "READAGAIN";
                                endofpacket = true;
                            }
                            else
                            {

                                endofpacket = false;
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
                    else /* positive response from the dongle */
                    {
                        endofpacket = true;
                        var length = responselen - nextpacketindex;
                        byte[] responseArray = new byte[length];
                        Array.Copy(responeBytes, nextpacketindex, responseArray, 0, length);
                        var actualRespone = responseArray;
                        dataArray = actualRespone;
                    }
                }/* while (endofpacket == true) */
           
        } 
    }
}

