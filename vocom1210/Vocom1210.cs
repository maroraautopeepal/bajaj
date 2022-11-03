using DotNetSta.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Zeroconf;

namespace DotNetSta
{
    public class Vocom1210
    {
        TcpClient client = new TcpClient();

        //static byte[] MessageLength = new byte[4];
        byte[] ClientId = new byte[2];
        //static byte[] DWCommand = new byte[4];
        //static byte[] Message = new byte[7];
        //static byte[] Command;
        //static int length = 0;
        bool connected = false;
        string protocol_name = string.Empty;
        byte[] IsAppPacktizingIncomingMsgsByt = new byte[2];
        //byte[] TxCanId = new byte[2];
        byte[] MsgType = new byte[1];
        byte[] TxId = new byte[4];
        byte[] RxId = new byte[4];
        byte[] TxExtendedAddress = new byte[1];
        byte[] RxExtendedAddress = new byte[1];
        string DebugTag = "Wifi Communication";

        public Vocom1210()
        {
        }

        public byte[] SetHearder(int DWCommandInt, byte[] Message)
        {
            //client = new TcpClient();
            try
            {

                byte[] return_arr = new byte[0];
                byte[] dwCommand = new byte[4];
                byte[] cmdLenght = new byte[4];
                //byte[] cmdMessage = new byte[];

                ClientId[0] = 0x00;
                ClientId[1] = 0x00;

                dwCommand[0] = (byte)(DWCommandInt >> 24);
                dwCommand[1] = (byte)(DWCommandInt >> 16);
                dwCommand[2] = (byte)(DWCommandInt >> 8);
                dwCommand[3] = (byte)DWCommandInt;

                //DWCommand[0] = 0x03;
                //DWCommand[1] = 0x04;
                //DWCommand[2] = 0x05;
                //DWCommand[3] = 0x06;

                //Message[0] = 0x07;
                //Message[1] = 0x07;
                //Message[2] = 0x07;
                //Message[3] = 0x07;
                //Message[4] = 0x07;
                //Message[5] = 0x07;
                //Message[6] = 0x07;

                int Lenght = 4 + ClientId.Length + dwCommand.Length + Message.Length;



                cmdLenght[0] = (byte)(Lenght >> 24);
                cmdLenght[1] = (byte)(Lenght >> 16);
                cmdLenght[2] = (byte)(Lenght >> 8);
                cmdLenght[3] = (byte)Lenght;

                Array.Resize(ref return_arr, Lenght);

                Array.Copy(cmdLenght, 0, return_arr, 0, cmdLenght.Length);
                Array.Copy(ClientId, 0, return_arr, cmdLenght.Length, ClientId.Length);
                Array.Copy(dwCommand, 0, return_arr, cmdLenght.Length + ClientId.Length, dwCommand.Length);
                Array.Copy(Message, 0, return_arr, cmdLenght.Length + ClientId.Length + dwCommand.Length, Message.Length);

                return return_arr;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void InitEcu(string Protocol, byte[] tx_id, byte[] rx_id, byte[] tx_extended_address, byte[] rx_extended_address)
        {

            if (Protocol.Contains("11BIT_CAN"))
                MsgType[0] = (byte)CanMassageTypeEnum.Standard_CAN_ISO15765_EXTENDED;
            else if (Protocol.Contains("29BIT_CAN"))
                MsgType[0] = (byte)CanMassageTypeEnum.Extended_CAN_ISO15765_EXTENDED;

            TxId = tx_id;
            RxId = rx_id;
            TxExtendedAddress[0] = tx_extended_address[0];
            RxExtendedAddress[0] = rx_extended_address[0];
        }

        public void DecodeHeader(ref byte[] Command, out byte[] lenth, out byte[] client_id, out byte[] dw_command, out byte[] message_command)
        {
            //Command = new byte[] { 0x00, 0x00, 0x00, 0x11, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07 };

            try
            {
                lenth = new byte[4];
                client_id = new byte[2];
                dw_command = new byte[4];
                message_command = new byte[0];

                Array.Resize(ref message_command, Command.Length - (lenth.Length + client_id.Length + dw_command.Length));

                Array.Copy(Command, 0, lenth, 0, lenth.Length);
                Array.Copy(Command, lenth.Length, client_id, 0, client_id.Length);
                Array.Copy(Command, lenth.Length + client_id.Length, dw_command, 0, dw_command.Length);
                Array.Copy(Command, lenth.Length + client_id.Length + dw_command.Length, message_command, 0, message_command.Length);

                //lenth = MessageLength;
                //client_id = ClientId;
                //dw_command = DWCommand;
                //message_command = Message;

            }
            catch (Exception ex)
            {
                lenth = null;
                client_id = null;
                dw_command = null;
                message_command = null;
            }
        }

        public bool ConnectDevice(string ip, int port = 27015)
        {
            client = new TcpClient(ip, port);

            if (client.Connected)
            {
                client.ReceiveTimeout = 15000;

                return true;
            }

            return false;
        }

        public bool ClientConnect(string protocol_str, int IsAppPacktizingIncomingMsgs = 0)
        {
            try
            {
                byte[] res_Mode = new byte[4];
                byte[] err_Code = new byte[2];
                byte[] result = new byte[4];
                //client = new TcpClient("192.168.43.179", 27015);
                //protocol_str = "ISO15765:Baud=500000,Channel=1";
                protocol_name = protocol_str.Split(':')[0];
                IsAppPacktizingIncomingMsgsByt[0] = (byte)(IsAppPacktizingIncomingMsgs >> 8);
                IsAppPacktizingIncomingMsgsByt[1] = (byte)IsAppPacktizingIncomingMsgs;
                byte[] bytes = Encoding.ASCII.GetBytes(protocol_str);
                byte[] protocol_arr = new byte[bytes.Length + 2];
                protocol_arr[0] = IsAppPacktizingIncomingMsgsByt[0];
                protocol_arr[1] = IsAppPacktizingIncomingMsgsByt[1];

                Array.Copy(bytes, 0, protocol_arr, 2, bytes.Length);



                byte[] command = SetHearder(1, protocol_arr);

                _SendCommand(command);

                byte[] response = _ReadMessage();

                //byte[] response = ConvHesStrToByteArr("000000100000000000000000000102bd");

                Decode_ClientConnect_Response(response, out res_Mode, out err_Code, out result);
                int int_res_Mode = BitConverter.ToInt32(res_Mode, 0);

                while (int_res_Mode != 0)
                {
                    response = _ReadMessage();

                    //response = ConvHesStrToByteArr("0000000e00000000000100000000");

                    Decode_ClientConnect_Response(response, out res_Mode, out err_Code, out result);
                    int_res_Mode = BitConverter.ToInt32(res_Mode, 0);
                }

                if (int_res_Mode == 0 && BitConverter.ToInt32(result, 0) == 0)
                {
                    connected = true;
                }
                else
                {
                    connected = false;
                }

            }
            catch (Exception ex)
            {
            }
            return connected;
        }

        public void _SendCommand(byte[] send_com)
        {
            Console.WriteLine("Wifi Communication : --- Send Response --- " + ByteArrayToString(send_com));
            client.GetStream().Write(send_com, 0, send_com.Length);
        }

        public byte[] _ReadMessage()
        {
            byte[] RetArray = new byte[] { };
            try
            {
                byte[] trgtlen = new byte[4];
                int readByte;
                try
                {
                    NetworkStream networkStream = client.GetStream();
                    //networkStream.ReadTimeout = 20000;
                    readByte = networkStream.Read(trgtlen, 0, trgtlen.Length);
                }
                catch (Exception ex)
                {
                    
                        //client.Client.Blocking = true;
                        //readByte = client.GetStream().Read(trgtlen, 0, trgtlen.Length);
                        //client.Client.Blocking = false;
                    
                }
                //int readByte = client.GetStream().Read(trgtlen, 0, trgtlen.Length);
                byte[] rev_trgtlen = new byte[trgtlen.Length];
                Array.Copy(trgtlen, rev_trgtlen, trgtlen.Length);
                Array.Reverse(rev_trgtlen);
                int unsignedpidintvalue = BitConverter.ToInt32(rev_trgtlen, 0);

                byte[] rbuffer = new byte[unsignedpidintvalue];
                Array.Copy(trgtlen, 0, rbuffer, 0, trgtlen.Length);
                readByte = client.GetStream().Read(rbuffer, 4, rbuffer.Length - 4);


                if (readByte > 0)
                {
                    RetArray = new byte[rbuffer.Length];
                    Array.Copy(rbuffer, 0, RetArray, 0, RetArray.Length);
                }
                else
                {

                }

                Console.WriteLine("Wifi Communication : --- Read Response --- " + ByteArrayToString(RetArray));

                return RetArray;
            }
            catch (System.IO.IOException tex)
            {

                return RetArray = Encoding.ASCII.GetBytes(tex.Message);
            }
            catch (Exception ex)
            {
                return RetArray;
            }
        }

        public byte[] ConvHesStrToByteArr(string hex_str)
        {
            try
            {
                return Enumerable.Range(0, hex_str.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(hex_str.Substring(x, 2), 16))
                                 .ToArray();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public void Decode_ClientConnect_Response(byte[] response, out byte[] res_Mode, out byte[] err_Code, out byte[] result)
        {
            byte[] lenth = new byte[4];
            byte[] client_id = new byte[2];
            byte[] dw_command = new byte[4];
            byte[] message_command = new byte[0];
            res_Mode = new byte[4];
            err_Code = new byte[2];
            result = new byte[4];

            DecodeHeader(ref response, out lenth, out client_id, out dw_command, out message_command);

            if (dw_command[3] == 0x00)
            {
                Array.Copy(message_command, 0, res_Mode, 0, res_Mode.Length);
                Array.Copy(message_command, res_Mode.Length, err_Code, 0, err_Code.Length);

                int int_res_Mode = BitConverter.ToInt32(res_Mode, 0);

            }
            else if (dw_command[3] == 0x01)
            {
                Array.Copy(message_command, 0, result, 0, result.Length);

            }
            else
            {

            }

        }

        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        public async Task<List<DeviceModel>> FindDongle()
        {
            try
            {
                int count = 0;
                bool ctrl_while = true;

                List<DeviceModel> list = new List<DeviceModel>();
                Stopwatch sw = Stopwatch.StartNew();
                while (ctrl_while && sw.Elapsed.TotalSeconds < 15)
                {
                    IReadOnlyList<IZeroconfHost> responses = null;

                    ILookup<string, string> domains = await ZeroconfResolver.BrowseDomainsAsync();
                    var response = await ZeroconfResolver.ResolveAsync(domains.Select(g => g.Key));

                    if (response != null && response.Any())
                    {
                        foreach (IZeroconfHost host in response)
                        {
                            if (host.DisplayName.Contains("vocom"))
                            {
                                list.Add(new DeviceModel { id = host.Id, ip = host.IPAddress, name = host.DisplayName });
                                ctrl_while = false;
                            }
                            Debug.WriteLine($"{host.DisplayName}---{host.IPAddress}", DebugTag);
                        }
                    }
                }
                if(list.Count > 0)
                    return list;
                else return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetFirmwareVersion()
        {
            string err_Code = string.Empty;
            byte[] result = new byte[4];
            byte[] command = SetHearder(5, new byte[0]);
            byte[] actual_result = null;
            string firmware_version = string.Empty;

            _SendCommand(command);

            byte[] response = _ReadMessage();

            //byte[] response = ConvHesStrToByteArr("000000100000000000000000000102bd");

            Decode_GetVersion_Response(response, out err_Code, out result);
            actual_result = new byte[result.Length - 4];
            Array.Copy(result, 4, actual_result, 0, actual_result.Length);
            firmware_version = Encoding.ASCII.GetString(actual_result);
            return firmware_version;
        }

        public void Decode_GetVersion_Response(byte[] response, out string err_Code, out byte[] result)
        {
            byte[] lenth = new byte[4];
            byte[] client_id = new byte[2];
            byte[] dw_command = new byte[4];
            byte[] message_command = new byte[0];

            DecodeHeader(ref response, out lenth, out client_id, out dw_command, out message_command);

            result = new byte[message_command.Length];

            if (dw_command[3] == 0x00)
            {
                //Array.Copy(message_command, 0, res_Mode, 0, res_Mode.Length);
                //Array.Copy(message_command, res_Mode.Length, err_Code, 0, err_Code.Length);
                //int int_res_Mode = BitConverter.ToInt32(res_Mode, 0);

            }
            else if (dw_command[3] == 0x05)
            {
                Array.Copy(message_command, 0, result, 0, result.Length);
                err_Code = "NO_ERROR";
            }
            else
            {

            }
            err_Code = "NO_ERROR";
        }

        public string SendCommand(byte[] cmd)
        {
            try
            {
                string err_Code = string.Empty;
                byte[] result = new byte[4];
                byte[] command = SetHearder(6, cmd);
                byte[] actual_result = new byte[4];
                _SendCommand(command);

                byte[] response = _ReadMessage();

                //byte[] response = ConvHesStrToByteArr("000000100000000000000000000102bd");

                Decode_SendCommand_Response(response, out err_Code, out result);
                Array.Copy(result, 0, actual_result, 0, actual_result.Length);
                return err_Code;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Decode_SendCommand_Response(byte[] response, out string err_Code, out byte[] result)
        {
            byte[] lenth = new byte[4];
            byte[] client_id = new byte[2];
            byte[] dw_command = new byte[4];
            byte[] message_command = new byte[0];

            DecodeHeader(ref response, out lenth, out client_id, out dw_command, out message_command);

            result = new byte[message_command.Length];

            if (dw_command[3] == 0x00)
            {
                //Array.Copy(message_command, 0, res_Mode, 0, res_Mode.Length);
                //Array.Copy(message_command, res_Mode.Length, err_Code, 0, err_Code.Length);
                //int int_res_Mode = BitConverter.ToInt32(res_Mode, 0);

            }
            else if (dw_command[3] == 0x06)
            {
                Array.Copy(message_command, 0, result, 0, result.Length);
                err_Code = "NO_ERROR";
            }
            else
            {

            }
            err_Code = "NO_ERROR";
        }



        #region READ DTC

        public string SendMessaage(byte[] message)
        {
            byte[] format_message = null;
            string err_Code = string.Empty;
            byte[] result = new byte[4];
            //
            if (protocol_name == "ISO15765")
            {
                format_message = format_msg_ISO15765(message);
            }
            byte[] command = SetHearder(3, format_message);
            byte[] actual_result = new byte[4];
            _SendCommand(command);

            byte[] response = _ReadMessage();

            //byte[] response = ConvHesStrToByteArr("000000100000000000000000000102bd");

            Decode_SendMessaage_Response(response, out err_Code, out result);

            if (BitConverter.ToInt32(result, 0) == 0)
            {
                response = _ReadMessage();
                Decode_ReadMessage_Response(response, out err_Code, out result, true);
            }
            else
            {
                err_Code = $"Send Message Error! ({ByteArrayToString(result)})";
            }

            Array.Copy(result, 0, actual_result, 0, result.Length);
            return err_Code;
        }

        public byte[] format_msg_ISO15765(byte[] message)
        {
            try
            {
                int _length = 0;
                TxId = HexStringToByteArray("000007e0");
                
                // 1 byte is Message type code and 2, 3, 4, 5 byte is  Tx Can ID ond 6 byte is tx extended address
                byte[] format_message = { MsgType[0], TxId[0], TxId[1], TxId[2], TxId[3], TxExtendedAddress[0] };
                _length = format_message.Length;
                Array.Resize(ref format_message, format_message.Length + message.Length);
                Array.Copy(message, 0, format_message, _length, message.Length);

                return format_message;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void Decode_SendMessaage_Response(byte[] response, out string err_Code, out byte[] result)
        {
            byte[] lenth = new byte[4];
            byte[] client_id = new byte[2];
            byte[] dw_command = new byte[4];
            byte[] message_command = new byte[0];

            DecodeHeader(ref response, out lenth, out client_id, out dw_command, out message_command);

            result = new byte[message_command.Length];

            if (dw_command[3] == 0x00)
            {
                //Array.Copy(message_command, 0, res_Mode, 0, res_Mode.Length);
                //Array.Copy(message_command, res_Mode.Length, err_Code, 0, err_Code.Length);
                //int int_res_Mode = BitConverter.ToInt32(res_Mode, 0);

            }
            else if (dw_command[3] == 0x03)
            {
                Array.Copy(message_command, 0, result, 0, result.Length);
                err_Code = "NO_ERROR";
            }
            else
            {

            }
            err_Code = "NO_ERROR";
        }

        public void Decode_ReadMessage_Response(byte[] response, out string err_Code, out byte[] result, bool can_acc = false)
        {
            byte[] lenth = new byte[4];
            byte[] client_id = new byte[2];
            byte[] dw_command = new byte[4];
            byte[] message_command = new byte[0];
            byte[] resp = null;
            byte[] result_res = new byte[0];
            string err_Code_resp = string.Empty;

            DecodeHeader(ref response, out lenth, out client_id, out dw_command, out message_command);

            resp = new byte[message_command.Length];
            Array.Reverse(dw_command);
            int dw_command_int = BitConverter.ToInt32(dw_command, 0);

            if (dw_command_int == 0)
            {
                err_Code_resp = "NEED_TO_IMPLEMENT";
                //Array.Copy(message_command, 0, res_Mode, 0, res_Mode.Length);
                //Array.Copy(message_command, res_Mode.Length, err_Code, 0, err_Code.Length);
                //int int_res_Mode = BitConverter.ToInt32(res_Mode, 0);

            }
            else if (dw_command_int == 4)
            {
                Array.Copy(message_command, 0, resp, 0, resp.Length);
                result_res = decode_msg_ISO15765(resp, can_acc);

                if (can_acc)
                {
                    Array.Reverse(result_res);
                    int resp_int = BitConverter.ToInt32(resp, 0);
                    ///////////////////////////////////////////////////////////////////////////////////////////////
                    int resp_int1 = BitConverter.ToInt16(result_res, 0);
                    if (resp_int1 == 0)
                    {
                        err_Code_resp = "NO_ERROR";
                    }
                    else
                    {
                        err_Code_resp = "FAILLED_TO_SEND_CAN_MESSAGE";
                    }

                }
                //err_Code = "NO_ERROR";
            }
            else
            {
                err_Code_resp = "WRONG_RESPONSE_DW_COMMAND";
            }

            err_Code = err_Code_resp;
            result = result_res;
        }

        public byte[] decode_msg_ISO15765(byte[] message, bool can_acc = false)
        {
            try
            {
                int _length = 0;
                byte[] return_value = null;
                bool echo_enable = false;
                byte[] time_stamp = new byte[4];
                byte[] remain_bytes = null;
                byte[] echo_byte = new byte[1];
                byte[] res_msg_type_code = new byte[1];
                byte[] indication_status = new byte[1];
                byte[] can_id = new byte[4];
                byte[] extended_addr = new byte[1];
                Array.Copy(message, 0, time_stamp, 0, time_stamp.Length);


                if (echo_enable)
                {
                    echo_byte[0] = message[4];
                    remain_bytes = new byte[message.Length - 5];
                    Array.Copy(message, 5, remain_bytes, 0, remain_bytes.Length);
                }
                else
                {
                    remain_bytes = new byte[message.Length - 4];
                    Array.Copy(message, 4, remain_bytes, 0, remain_bytes.Length);
                }
                int index = 0;

                indication_status[0] = remain_bytes[index++];
                res_msg_type_code[0] = remain_bytes[index++];

                can_id[0] = remain_bytes[index++];
                can_id[1] = remain_bytes[index++];
                can_id[2] = remain_bytes[index++];
                can_id[3] = remain_bytes[index++];

                if (!can_acc)
                {
                    extended_addr[0] = remain_bytes[index++];
                }

                Array.Resize(ref return_value, remain_bytes.Length - index);
                Array.Copy(remain_bytes, index, return_value, 0, return_value.Length);
                return return_value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public byte[] ReadMessage()
        {
            string err_Code = string.Empty;
            byte[] result = new byte[4];
            byte[] response = _ReadMessage();

            Decode_ReadMessage_Response(response, out err_Code, out result);

            return result;
        }

        public byte[] ReadAgainMessage()
        {
            byte[] ActualBytes = new byte[] { };
            try
            {
                byte[] rbuffer = new byte[4500];
                byte[] RetArray = new byte[] { };
                string err_Code = string.Empty;
                byte[] result = new byte[4];


                Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
                //client.GetStream().ReadTimeout = 5000;
                int readByte = 0;
                try
                {
                    readByte = client.GetStream().Read(rbuffer, 0, rbuffer.Length);
                }
                catch (Exception ex)
                {

                }
                Debug.WriteLine("--------- Read Byte Lenth-------" + readByte, DebugTag);
                RetArray = new byte[readByte];
                Array.Copy(rbuffer, 0, RetArray, 0, readByte);

                Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);

                Decode_ReadMessage_Response(RetArray, out err_Code, out result);

                return result;
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
        }
        #endregion


            #region Disconnect 
        public bool ClientDisconnect()
        {
            string err_Code = string.Empty;
            byte[] result = new byte[4];
            byte[] command = SetHearder(2, new byte[0]);
            byte[] actual_result = null;
            string firmware_version = string.Empty;

            _SendCommand(command);

            byte[] response = _ReadMessage();

            //byte[] response = ConvHesStrToByteArr("000000100000000000000000000102bd");

            Decode_ClientDisconnect_Response(response, out err_Code, out result);

            if (BitConverter.ToInt32(result, 0) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Decode_ClientDisconnect_Response(byte[] response, out string err_Code, out byte[] result)
        {
            byte[] lenth = new byte[4];
            byte[] client_id = new byte[2];
            byte[] dw_command = new byte[4];
            byte[] message_command = new byte[0];

            DecodeHeader(ref response, out lenth, out client_id, out dw_command, out message_command);

            result = new byte[message_command.Length];

            if (dw_command[3] == 0x00)
            {
                //Array.Copy(message_command, 0, res_Mode, 0, res_Mode.Length);
                //Array.Copy(message_command, res_Mode.Length, err_Code, 0, err_Code.Length);
                //int int_res_Mode = BitConverter.ToInt32(res_Mode, 0);

            }
            else if (dw_command[3] == 0x02)
            {
                Array.Copy(message_command, 0, result, 0, result.Length);
                err_Code = "NO_ERROR";
            }
            else
            {

            }
            err_Code = "NO_ERROR";
        }
        #endregion

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

    public class DeviceModel
    {
        public string ip { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public enum CanMassageTypeEnum
    {
        Standart_CAN = 0x00,
        Extended_CAN = 0x01,
        Standard_CAN_ISO15765_EXTENDED = 0x02,
        Extended_CAN_ISO15765_EXTENDED = 0x03,
        STANDARD_MIXED_CAN_ISO15765 = 0x04,
    }
}
