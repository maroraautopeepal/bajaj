using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.Runtime;
using APDiagnosticAndroid;
using APDiagnosticAndroid.Enums;
using APDiagnosticAndroid.Models;
using APDiagnosticAndroid.Structures;
using APDongleCommAndroid.Helper;
//using APDongleCommWin;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using Bajaj.Model;
using Java.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Zeroconf;
using Console = System.Console;
using APDongleCommAndroid;
using APDongleCommAnroid.Models;
//using ECUSeedkeyPCL;

[assembly: Dependency(typeof(WifiConnector))]
namespace Bajaj.Droid.Dependencies
{
    public class WifiConnector : IConnectionWifi
    {
        OutputStreamInvoker outStream = null;
        InputStreamInvoker inStream = null;
        InputStreamReader inputStreamReader = null;
        BufferedReader bufferedReader = null;
        DongleCommWin dongleCommWin;
        DongleCommWin dongleComm;
        UDSDiagnostic dSDiagnostic;
        NetworkStream stream;
        TcpClient clients;
        private APDiagnosticAndroid.Models.ReadDtcResponseModel readDTCResponse;
        public WifiConnector()
        {

        }
        public async void Connect_dongle()
        {
            Xamarin.Forms.Forms.Context.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionWifiSettings));
        }

        public Task<ObservableCollection<BluetoothDevicesModel>> enable_hotspots()
        {
            string str = string.Empty;
            return null;
        }

        // WifiManager wifiManager;
        //ConnectivityManager _conn;
        //public async Task<ObservableCollection<BluetoothDevicesModel>> get_device_list()
        //{
        //    try
        //    {
        //        ObservableCollection<BluetoothDevicesModel> available_dogles = new ObservableCollection<BluetoothDevicesModel>();

        //        return available_dogles = await Task.Run(async () =>
        //        {
        //            WifiManager wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);

        //            if (!wifi.IsWifiEnabled)
        //            {
        //                wifi.SetWifiEnabled(true);
        //            }

        //            //ZeroconfResolver.
        //            IReadOnlyList<IZeroconfHost> responses = null;

        //            ILookup<string, string> domains = await ZeroconfResolver.BrowseDomainsAsync();
        //            responses = await Task.Run(async () =>
        //            {
        //                return await ZeroconfResolver.ResolveAsync(domains.Select(g => g.Key));
        //            });
        //            if (responses.Count == 0 || responses.FirstOrDefault().DisplayName.Contains("obd") != true || responses.FirstOrDefault().DisplayName.Contains("vocom") != true)
        //            {
        //                foreach (var service in domains)
        //                {
        //                    var vvv = service.Key;
        //                    foreach (var host in service)
        //                    {
        //                        if (host.Contains("obd") || host.Contains("vocom"))
        //                        {
        //                            string[] SplitHost = host.Split(new char[] { ':' });

        //                            available_dogles.Add(
        //                        new BluetoothDevicesModel
        //                        {
        //                            Name = SplitHost[1],
        //                            Ip = SplitHost[0]
        //                        });
        //                        }
        //                    }
        //                }
        //            }

        //            foreach (var resp in responses)
        //            {
        //                if (resp.DisplayName.Contains("obd") || (resp.DisplayName.Contains("vocom")))
        //                {
        //                    available_dogles.Add(
        //                new BluetoothDevicesModel
        //                {
        //                    Name = resp.DisplayName,
        //                    Ip = resp.IPAddress
        //                });
        //                }
        //            }
        //            //foreach (var network in wifi.ConfiguredNetworks)
        //            //{
        //            //    available_dogles.Add(
        //            //   new BluetoothDevicesModel
        //            //   {
        //            //       Name = network.Ssid
        //            //       Mac_Address = network.Bssid
        //            //   });
        //            //}

        //            return available_dogles;
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //    // true or false to activate/deactivate wifi

        //    //try
        //    //{
        //    //    // OBD-2EF4 
        //    //    // password1
        //    //    const string formattedSsid = "\"OBD2-2EF4\"";
        //    //    const string formattedPassword = "\"password1\"";

        //    //    var wifiConfig = new WifiConfiguration
        //    //    {
        //    //        Ssid = formattedSsid,
        //    //        PreSharedKey = formattedPassword,
        //    //        Priority = 0,
        //    //    };

        //    //    wifiManager = (WifiManager)Android.App.Application.Context
        //    //                   .GetSystemService(Context.WifiService);
        //    //    var addNetwork = wifiManager.AddNetwork(wifiConfig);
        //    //    System.Diagnostics.Debug.WriteLine("addNetwork = " + addNetwork);

        //    //    var network = wifiManager.ConfiguredNetworks.FirstOrDefault((n) => n.Ssid == "\"OBD2-2EF4\"");
        //    //    if (network == null)
        //    //    {
        //    //        System.Diagnostics.Debug.WriteLine("Didn't find the network, not connecting.");
        //    //        //return;
        //    //    }


        //    //   // wifiManager.Disconnect();

        //    //    var enableNetwork = wifiManager.EnableNetwork(network.NetworkId, true);
        //    //    System.Diagnostics.Debug.WriteLine("enableNetwork = " + enableNetwork);
        //    //    var isConncted = wifiManager.r();
        //    //    if (isConncted)
        //    //    {
        //    //        var builder = new AlertDialog.Builder(Android.App.Application.Context);
        //    //        builder.SetMessage("Connected");
        //    //        var alert = builder.Create();
        //    //        alert.Show();

        //    //    }
        //    //    else
        //    //    {
        //    //        var builder = new AlertDialog.Builder(Android.App.Application.Context);
        //    //        builder.SetMessage("Nope");
        //    //        var alert = builder.Create();
        //    //        alert.Show();
        //    //    }

        //    //    // var wifiSpecifier = new WifiNetworkSpecifier.Builder()
        //    //    //.SetSsid(ssid)
        //    //    //.SetWpa2Passphrase(preSharedKey)
        //    //    //.Build();

        //    //    // var request = new NetworkRequest.Builder()
        //    //    //     .AddTransportType(TransportType.Wifi) // we want WiFi
        //    //    //     .RemoveCapability(NetCapability.Internet) // Internet not required
        //    //    //     .SetNetworkSpecifier(wifiSpecifier) // we want _our_ network
        //    //    //     .Build();

        //    //    // _wifiManager.RequestNetwork(request, _networkCallback);
        //    //    return true;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    return false;
        //    //}

        //}

        public async Task<ObservableCollection<BluetoothDevicesModel>> get_device_list()
        {
            try
            {
                ObservableCollection<BluetoothDevicesModel> available_dogles = new ObservableCollection<BluetoothDevicesModel>();

                return available_dogles = await Task.Run(async () =>
                {
                    WifiManager wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);

                    if (!wifi.IsWifiEnabled)
                    {
                        wifi.SetWifiEnabled(true);
                    }

                    //ZeroconfResolver.
                    IReadOnlyList<IZeroconfHost> responses = null;
                    bool is_Found = false;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    while (sw.Elapsed.TotalSeconds < 15)
                    {
                        ILookup<string, string> domains = await ZeroconfResolver.BrowseDomainsAsync();
                        responses = await Task.Run(async () =>
                        {
                            return await ZeroconfResolver.ResolveAsync(domains.Select(g => g.Key));
                        });

                        if (responses != null)
                        {
                            foreach (var resp in responses)
                            {
                                if (resp.DisplayName.Contains("obd"))
                                {
                                    is_Found = true;
                                    bool is_double = false;
                                    foreach (var item in available_dogles)
                                    {
                                        if (item.Ip == resp.IPAddress)
                                        {
                                            is_double = true;
                                        }
                                    }
                                    if (!is_double)
                                    {
                                        available_dogles.Add(
                                            new BluetoothDevicesModel
                                            {
                                                Name = resp.DisplayName,
                                                Ip = resp.IPAddress
                                            });

                                    }

                                }
                            }
                        }
                    }
                    if (is_Found)
                    {
                        return available_dogles;
                    }
                    else
                        return null;



                    //foreach (var network in wifi.ConfiguredNetworks)
                    //{
                    //    available_dogles.Add(
                    //   new BluetoothDevicesModel
                    //   {
                    //       Name = network.Ssid
                    //       Mac_Address = network.Bssid
                    //   });
                    //}


                });
            }
            catch (Exception ex)
            {
                return null;
            }
            // true or false to activate/deactivate wifi

            //try
            //{
            //    // OBD-2EF4 
            //    // password1
            //    const string formattedSsid = "\"OBD2-2EF4\"";
            //    const string formattedPassword = "\"password1\"";

            //    var wifiConfig = new WifiConfiguration
            //    {
            //        Ssid = formattedSsid,
            //        PreSharedKey = formattedPassword,
            //        Priority = 0,
            //    };

            //    wifiManager = (WifiManager)Android.App.Application.Context
            //                   .GetSystemService(Context.WifiService);
            //    var addNetwork = wifiManager.AddNetwork(wifiConfig);
            //    System.Diagnostics.Debug.WriteLine("addNetwork = " + addNetwork);

            //    var network = wifiManager.ConfiguredNetworks.FirstOrDefault((n) => n.Ssid == "\"OBD2-2EF4\"");
            //    if (network == null)
            //    {
            //        System.Diagnostics.Debug.WriteLine("Didn't find the network, not connecting.");
            //        //return;
            //    }


            //   // wifiManager.Disconnect();

            //    var enableNetwork = wifiManager.EnableNetwork(network.NetworkId, true);
            //    System.Diagnostics.Debug.WriteLine("enableNetwork = " + enableNetwork);
            //    var isConncted = wifiManager.r();
            //    if (isConncted)
            //    {
            //        var builder = new AlertDialog.Builder(Android.App.Application.Context);
            //        builder.SetMessage("Connected");
            //        var alert = builder.Create();
            //        alert.Show();

            //    }
            //    else
            //    {
            //        var builder = new AlertDialog.Builder(Android.App.Application.Context);
            //        builder.SetMessage("Nope");
            //        var alert = builder.Create();
            //        alert.Show();
            //    }

            //    // var wifiSpecifier = new WifiNetworkSpecifier.Builder()
            //    //.SetSsid(ssid)
            //    //.SetWpa2Passphrase(preSharedKey)
            //    //.Build();

            //    // var request = new NetworkRequest.Builder()
            //    //     .AddTransportType(TransportType.Wifi) // we want WiFi
            //    //     .RemoveCapability(NetCapability.Internet) // Internet not required
            //    //     .SetNetworkSpecifier(wifiSpecifier) // we want _our_ network
            //    //     .Build();

            //    // _wifiManager.RequestNetwork(request, _networkCallback);
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}

        }


        private async Task<IReadOnlyList<IZeroconfHost>> EnumerateAllServicesFromAllHosts()
        {
            // get all domains
            var domains = await ZeroconfResolver.BrowseDomainsAsync();

            // get all hosts
            return await ZeroconfResolver.ResolveAsync(domains.Select(g => g.Key));
        }

        public async Task<bool> CheckConnection()
        {
            if (clients == null)
                return false;

            return true;
        }

        public async Task<string> get_ipaddress()
        {
            try
            {
                string ipAddress = "";
                List<string> ip_List = new List<string>();

                //foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
                //{
                //    if (f.OperationalStatus == OperationalStatus.Up)
                //    {
                //        var ip = f.GetIPProperties().GatewayAddresses;
                //        foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                //        {
                //            ipAddress = d.Address.ToString();
                //        }
                //    }
                //}

                //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

                //if (adresses != null && adresses[0] != null)
                //{
                //    var data = adresses[0].ToString();
                //}
                //else
                //{
                //    ;
                //}


                //return ipAddress




                //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

                //if (adresses != null && adresses[0] != null)
                //{
                //    var data = adresses[0].ToString();
                //}
                //else
                //{
                //    ;
                //}

                //DhcpInfo d;

                //d = wifi.DhcpInfo;
                ////ipAddress =  d.Gateway.ToString();
                //int ip = d.Gateway;
                //ipAddress = Formatter.FormatIpAddress(ip);

                var bytes = HexStringToByteArray("05000b0000000000000000500c47568afe56214e238000ffc315e4");
                TcpClient client = new TcpClient("192.168.4.1", 6888);
                byte[] seq_command = new byte[] { 0x50, 0x0C, 0x47, 0x56, 0x8A, 0xFE, 0x56, 0x21, 0x4E, 0x23, 0x80, 0x00, 0xFF, 0xC3 };

                NetworkStream stream = client.GetStream();
                stream.Write(bytes, 0, bytes.Length);

                var response = await ReadData(stream);

                var SSID = HexStringToByteArray("0600110000000000000000201216014155544f2050454550414c203200732cd49b");
                stream.Write(SSID, 0, SSID.Length);
                var response1 = await ReadData(stream);


                var PASSWORD = HexStringToByteArray("0700110000000000000000201217016175746f70656570616c31323300b7565ab2");
                stream.Write(PASSWORD, 0, PASSWORD.Length);
                var response2 = await ReadData(stream);




                string ssid = "";
                string password = "";
                var wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);

                var formattedSsid = $"\"AUTO PEEPAL 2\"";
                var formattedPassword = $"\"autopeepal123\"";

                var wifiConfig = new WifiConfiguration
                {
                    Ssid = formattedSsid,
                    PreSharedKey = formattedPassword
                };

                var addNetwork = wifi.AddNetwork(wifiConfig);

                var network = wifi.ConfiguredNetworks.FirstOrDefault(n => n.Ssid == "\"AUTO PEEPAL 2\"");

                if (network == null)
                {
                    Console.WriteLine($"Cannot connect to network: {ssid}");
                    //return;
                }

                // wifiManager.Disconnect();

                var enableNetwork = wifi.EnableNetwork(network.NetworkId, true);
                System.Diagnostics.Debug.WriteLine("enableNetwork = " + enableNetwork);
                var isConncted = wifi.Reconnect();
                if (isConncted)
                {
                    //var builder = new AlertDialog.Builder(Android.App.Application.Context);
                    //builder.SetMessage("Connected");
                    //var alert = builder.Create();
                    //alert.Show();
                    Dialog dialog = new Dialog(Android.App.Application.Context);
                    dialog.Create();


                }
                else
                {
                    var builder = new AlertDialog.Builder(Android.App.Application.Context);
                    builder.SetMessage("Nope");
                    var alert = builder.Create();
                    alert.Show();
                }


                var domains = await ZeroconfResolver.BrowseDomainsAsync();
                //var domains = await ZeroconfResolver.BrowseDomainsAsync();

                //Console.WriteLine(Array2Text(RetArray, false) + "\n" + "[" + RetArray.Length.ToString() + "]");

                //Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);



                //while (true)
                //{
                //    byte[] receivedBuffer = new byte[1024];

                //    await stream.ReadAsync(receivedBuffer, 0, receivedBuffer.Length);

                //    StringBuilder msg = new StringBuilder();
                //    foreach (byte b in receivedBuffer)
                //    {
                //        if (b.Equals(00))
                //        {
                //            break;
                //        }
                //        else
                //        {
                //            msg.Append(Convert.ToChar(b).ToString());
                //        }

                //    }
                //    var sss = msg;
                //}


                //---------------------------------------------------
                //string ssid = "";
                //string password = "";
                //var wifiManager = (WifiManager)Android.App.Application.Context
                //       .GetSystemService(Context.WifiService);

                //var formattedSsid = $"\"{ssid}\"";
                //var formattedPassword = $"\"{password}\"";

                //var wifiConfig = new WifiConfiguration
                //{
                //    Ssid = formattedSsid,
                //    PreSharedKey = formattedPassword
                //};

                //var addNetwork = wifiManager.AddNetwork(wifiConfig);

                //var network = wifiManager.ConfiguredNetworks
                //     .FirstOrDefault(n => n.Ssid == ssid);

                //if (network == null)
                //{
                //    Console.WriteLine($"Cannot connect to network: {ssid}");
                //    //return;
                //}

                //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

                //if (adresses != null && adresses[0] != null)
                //{
                //    var data = adresses[0].ToString();
                //}
                //else
                //{
                //    ;
                //}
                return null;
                //-------------------------------------------------------------------------


                //WifiManager wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
                //WifiInfo wifiInfo = wifi.ConnectionInfo;
                //int ip = wifiInfo.IpAddress;
                //ipAddress = Formatter.FormatIpAddress(ip);


                //var res = Dns.GetHostName();
                //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

                //if (adresses != null && adresses[0] != null)
                //{
                //    return adresses[0].ToString();
                //}
                //else
                //{
                //    return null;
                //}
                //return ipAddress;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        TcpClient TcpClient = null;
        private NetworkStream Stream = null;

        public async Task<string> Write_SSIDPassword(string RouterSSID, string RouterPassword)
        {
            try
            {
                string ipAddress = "";
                List<string> ip_List = new List<string>();

                var bytes = HexStringToByteArray("05000b0000000000000000500c47568afe56214e238000ffc315e4");

                Debug.WriteLine("-------------Tcp Start-------------");
                TcpClient client = new TcpClient("192.168.4.1", 6888);
                Debug.WriteLine("-------------Tcp END-------------");

                byte[] seq_command = new byte[] { 0x50, 0x0C, 0x47, 0x56, 0x8A, 0xFE, 0x56, 0x21, 0x4E, 0x23, 0x80, 0x00, 0xFF, 0xC3 };

                NetworkStream stream = client.GetStream();
                stream.Write(bytes, 0, bytes.Length);

                Debug.WriteLine("-------------First ReadData Start-------------");
                var response = await ReadData(stream);
                Debug.WriteLine("-------------First ReadData End" + BitConverter.ToString(response) + "-------------");

                //// SSID
                string SSIDCommand = "20" + (RouterSSID.Length + 5).ToString("X2") + "1601" + ToHex(RouterSSID) + "00";

                string wificmd = ReadChecksum(RouterSSID, SSIDCommand);
                Debug.WriteLine("SSID Send Start -------------" + wificmd + "-------------");
                var byteData = HexStringToByteArray(wificmd);
                stream.Write(byteData, 0, byteData.Length);
                var SSIDResponse = await ReadData(stream);
                Debug.WriteLine("SSID Send End Response -------------" + BitConverter.ToString(SSIDResponse) + "-------------");


                //// SSID PASSWORD
                string pw = "20" + (RouterPassword.Length + 5).ToString("X2") + "1701" + ToHex(RouterPassword) + "00";
                string wifipwcmd = ReadChecksum(RouterPassword, pw);
                Debug.WriteLine("Password Send Start -------------" + wifipwcmd + "-------------");
                var byteDatapw = HexStringToByteArray(wifipwcmd);
                stream.Write(byteDatapw, 0, byteDatapw.Length);
                var SSIDpwResponse = await ReadData(stream);
                Debug.WriteLine("Password Send End Response -------------" + BitConverter.ToString(SSIDpwResponse) + "-------------");


                string ssid = "";
                string password = "";
                var wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);

                var formattedSsid = RouterSSID;
                var formattedPassword = RouterPassword;

                var wifiConfig = new WifiConfiguration
                {
                    Ssid = formattedSsid,
                    PreSharedKey = formattedPassword
                };

                var addNetwork = wifi.AddNetwork(wifiConfig);

                var network = wifi.ConfiguredNetworks.FirstOrDefault(n => n.Ssid == RouterSSID);

                if (network == null)
                {
                    Console.WriteLine($"Cannot connect to network: {ssid}");
                    //return;
                }

                // wifiManager.Disconnect();

                var enableNetwork = wifi.EnableNetwork(network.NetworkId, true);
                System.Diagnostics.Debug.WriteLine("enableNetwork = " + enableNetwork);
                var isConncted = wifi.Reconnect();
                if (isConncted)
                {
                    //var builder = new AlertDialog.Builder(Android.App.Application.Context);
                    //builder.SetMessage("Connected");
                    //var alert = builder.Create();
                    //alert.Show();
                    Dialog dialog = new Dialog(Android.App.Application.Context);
                    dialog.Create();
                }
                else
                {
                    var builder = new AlertDialog.Builder(Android.App.Application.Context);
                    builder.SetMessage("Nope");
                    var alert = builder.Create();
                    alert.Show();
                }


                var domains = await ZeroconfResolver.BrowseDomainsAsync();
                //var domains = await ZeroconfResolver.BrowseDomainsAsync();

                //Console.WriteLine(Array2Text(RetArray, false) + "\n" + "[" + RetArray.Length.ToString() + "]");

                //Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);



                //while (true)
                //{
                //    byte[] receivedBuffer = new byte[1024];

                //    await stream.ReadAsync(receivedBuffer, 0, receivedBuffer.Length);

                //    StringBuilder msg = new StringBuilder();
                //    foreach (byte b in receivedBuffer)
                //    {
                //        if (b.Equals(00))
                //        {
                //            break;
                //        }
                //        else
                //        {
                //            msg.Append(Convert.ToChar(b).ToString());
                //        }

                //    }
                //    var sss = msg;
                //}


                //---------------------------------------------------
                //string ssid = "";
                //string password = "";
                //var wifiManager = (WifiManager)Android.App.Application.Context
                //       .GetSystemService(Context.WifiService);

                //var formattedSsid = $"\"{ssid}\"";
                //var formattedPassword = $"\"{password}\"";

                //var wifiConfig = new WifiConfiguration
                //{
                //    Ssid = formattedSsid,
                //    PreSharedKey = formattedPassword
                //};

                //var addNetwork = wifiManager.AddNetwork(wifiConfig);

                //var network = wifiManager.ConfiguredNetworks
                //     .FirstOrDefault(n => n.Ssid == ssid);

                //if (network == null)
                //{
                //    Console.WriteLine($"Cannot connect to network: {ssid}");
                //    //return;
                //}

                //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

                //if (adresses != null && adresses[0] != null)
                //{
                //    var data = adresses[0].ToString();
                //}
                //else
                //{
                //    ;
                //}
                return null;
                //-------------------------------------------------------------------------


                //WifiManager wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
                //WifiInfo wifiInfo = wifi.ConnectionInfo;
                //int ip = wifiInfo.IpAddress;
                //ipAddress = Formatter.FormatIpAddress(ip);


                //var res = Dns.GetHostName();
                //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

                //if (adresses != null && adresses[0] != null)
                //{
                //    return adresses[0].ToString();
                //}
                //else
                //{
                //    return null;
                //}
                //return ipAddress;
            }
            catch (Exception ex)
            {
                return "";
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
        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        private string ToHex(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
                sb.AppendFormat("{0:X2}", (int)c);
            return sb.ToString().Trim();
        }

        private async Task<byte[]> ReadData(NetworkStream stream)
        {
            byte[] rbuffer = new byte[1024];
            byte[] RetArray = new byte[] { };

            //Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
            // Read data from the device
            //while (!stream.CanRead)
            //{
            //    //Console.WriteLine("------------------------------------------------");
            //}
            int readByte = await stream.ReadAsync(rbuffer, 0, rbuffer.Length);

            RetArray = new byte[readByte];
            Array.Copy(rbuffer, 0, RetArray, 0, readByte);
            return RetArray;
        }

        //public async Task<string[]> Connect(string IP)
        //{
        //    if (IP == "")
        //    {
        //        clients.Close();
        //    }

        //    var firmware_version = await SendMessage("500C47568AFE56214E238000FFC3", IP);
        //    return firmware_version;

        //    //TcpClient clients = new TcpClient(IP, 6888);
        //    //if (clients.Connected == true)
        //    //{               
        //    //    var bytes = HexStringToByteArray("05000b0000000000000000500c47568afe56214e238000ffc315e4");

        //    //    NetworkStream streamm = clients.GetStream();
        //    //    streamm.Write(bytes, 0, bytes.Length);
        //    //    byte[] finalData = new byte[8];
        //    //    var responseee = await ReadData(streamm);
        //    //}
        //}

        public async void SetDongleProperties(string ProtocolName, string tx_header_temp, string rx_header_temp)
        {
            //var setProtocol = await dongleCommWin.Dongle_SetProtocol(ProtocolValue);
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
            //string[] returns = new string[2];
            try
            {
                //clients = new TcpClient(IP, 6888);

                //var ProtocolValue = (dynamic)null;

                //string ProtocolNameValue = StaticData.ecu_info.FirstOrDefault().protocol.name;//.protocol.name;               
                //string value = ProtocolNameValue.Replace("-", "_");
                //string tx_header_temp = string.Empty;
                //string rx_header_temp = string.Empty;


                //tx_header_temp = StaticData.ecu_info.FirstOrDefault().tx_header;
                //rx_header_temp = StaticData.ecu_info.FirstOrDefault().rx_header;

                //switch (value)
                //{
                //    case "ISO15765_250KB_11BIT_CAN":
                //        ProtocolValue = 00;

                //        break;
                //    case "ISO15765_250Kb_29BIT_CAN":
                //        ProtocolValue = 01;
                //        break;
                //    case "ISO15765_500KB_11BIT_CAN":
                //        ProtocolValue = 02;
                //        break;
                //    case "ISO15765_500KB_29BIT_CAN":
                //        ProtocolValue = 03;
                //        break;
                //    case "ISO15765_1MB_11BIT_CAN":
                //        ProtocolValue = 04;
                //        break;
                //    case "ISO15765_1MB_29BIT_CAN":
                //        ProtocolValue = 05;
                //        break;
                //    case "I250KB_11BIT_CAN":
                //        ProtocolValue = 06;
                //        break;
                //    case "I250Kb_29BIT_CAN":
                //        ProtocolValue = 07;
                //        break;
                //    case "I500KB_11BIT_CAN":
                //        ProtocolValue = 08;
                //        break;
                //    case "I500KB_29BIT_CAN":
                //        ProtocolValue = 09;
                //        break;
                //    case "I1MB_11BIT_CAN":
                //        ProtocolValue = (int)0x0A;
                //        break;
                //    case "I1MB_29BIT_CAN":
                //        ProtocolValue = (int)0x0B;
                //        break;
                //    case "OE_IVN_250KBPS_11BIT_CAN":
                //        ProtocolValue = (int)0x0C;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_250KBPS_29BIT_CAN":
                //        ProtocolValue = (int)0x0D;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_500KBPS_11BIT_CAN":
                //        ProtocolValue = (int)0x0E;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_500KBPS_29BIT_CAN":
                //        ProtocolValue = (int)0x0F;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_1MBPS_11BIT_CAN":
                //        ProtocolValue = (int)0x10;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_1MBPS_29BIT_CAN":
                //        ProtocolValue = (int)0x11;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    default:
                //        break;
                //}

                //dongleCommWin = new DongleCommWin(clients, stream, ProtocolValue, Convert.ToUInt32(tx_header_temp, 16), Convert.ToUInt32(rx_header_temp, 16), 0x00, 0x10, 0x10, 0x10);

                //dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x07E0, 0x07E8, 0x00, 0x10, 0x10, 0x10);
                //dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.WiFi);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
                //dSDiagnostic = new UDSDiagnostic(dongleCommWin);
                //var ab = new byte[] { };
                //var securityAccess = await dongleCommWin.SecurityAccess();
                var setProtocol = await dongleCommWin.Dongle_SetProtocol(ProtocolValue);
                var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header_temp);
                var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(rx_header_temp);
                var setPadding = await dongleCommWin.CAN_StartPadding("00");
                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                var firmwareResult = (byte[])firmwareVersion;
                var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                //returns[0] = ver;
                //var version = ByteArrayToString(firmwareResult);


                //var GetMacId = await dongleCommWin.GetWifiMacId();
                //var GetMacId1 = (byte[])GetMacId;
                //var MacId = GetMacId1[3].ToString("X2") + ":" +
                //    GetMacId1[4].ToString("X2") + ":" +
                //    GetMacId1[5].ToString("X2") + ":" +
                //    GetMacId1[6].ToString("X2") + ":" +
                //    GetMacId1[7].ToString("X2") + ":" +
                //    GetMacId1[8].ToString("X2");
                //returns[1] = MacId;
                #region Old Code
                return ver;
                //var ProtocolValue = (dynamic)null;

                //string ProtocolNameValue = StaticData.ecu_info.FirstOrDefault().protocol.name;//.protocol.name;
                //string value = ProtocolNameValue.Replace("-", "_");
                //string tx_header_temp = string.Empty;
                //string rx_header_temp = string.Empty;

                //tx_header_temp = StaticData.ecu_info.FirstOrDefault().tx_header;
                //rx_header_temp = StaticData.ecu_info.FirstOrDefault().rx_header;

                //switch (value)
                //{
                //    case "ISO15765_250KB_11BIT_CAN":
                //        ProtocolValue = 00;
                //        break;
                //    case "ISO15765_250Kb_29BIT_CAN":
                //        ProtocolValue = 01;
                //        break;
                //    case "ISO15765_500KB_11BIT_CAN":
                //        ProtocolValue = 02;
                //        break;
                //    case "ISO15765_500KB_29BIT_CAN":
                //        ProtocolValue = 03;
                //        break;
                //    case "ISO15765_1MB_11BIT_CAN":
                //        ProtocolValue = 04;
                //        break;
                //    case "ISO15765_1MB_29BIT_CAN":
                //        ProtocolValue = 05;
                //        break;
                //    case "I250KB_11BIT_CAN":
                //        ProtocolValue = 06;
                //        break;
                //    case "I250Kb_29BIT_CAN":
                //        ProtocolValue = 07;
                //        break;
                //    case "I500KB_11BIT_CAN":
                //        ProtocolValue = 08;
                //        break;
                //    case "I500KB_29BIT_CAN":
                //        ProtocolValue = 09;
                //        break;
                //    case "I1MB_11BIT_CAN":
                //        ProtocolValue = (int)0x0A;
                //        break;
                //    case "I1MB_29BIT_CAN":
                //        ProtocolValue = (int)0x0B;
                //        break;
                //    case "OE_IVN_250KBPS_11BIT_CAN":
                //        ProtocolValue = (int)0x0C;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_250KBPS_29BIT_CAN":
                //        ProtocolValue = (int)0x0D;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_500KBPS_11BIT_CAN":
                //        ProtocolValue = (int)0x0E;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_500KBPS_29BIT_CAN":
                //        ProtocolValue = (int)0x0F;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_1MBPS_11BIT_CAN":
                //        ProtocolValue = (int)0x10;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    case "OE_IVN_1MBPS_29BIT_CAN":
                //        ProtocolValue = (int)0x11;
                //        tx_header_temp = "07E0";
                //        rx_header_temp = "07E8";
                //        break;
                //    default:
                //        break;
                //}

                //dongleCommWin = new DongleCommWin(clients, stream, value, Convert.ToUInt32(tx_header_temp, 16), Convert.ToUInt32(rx_header_temp, 16), 0x00, 0x10, 0x10, 0x10);
                ////dongleCommWin = new DongleCommWin(clients, stream, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x7E0, 0x7E8, 0x00, 0x10, 0x10, 0x10);
                //dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.WiFi);
                //dSDiagnostic = new UDSDiagnostic(dongleCommWin);

                //var securityAccess = await dongleCommWin.SecurityAccess();
                //var securityResult = (byte[])securityAccess;
                //var securityResponse = ByteArrayToString(securityResult);
                //var setProtocol = await dongleCommWin.Dongle_SetProtocol(2);
                //var ProtocolResult = (byte[])setProtocol;
                //var ProtocolResponse = ByteArrayToString(ProtocolResult);
                //var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header_temp);
                //var HeaderResult = (byte[])setHeader;
                //var HeaderResponse = ByteArrayToString(HeaderResult);
                //var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(rx_header_temp);
                //var HeaderMarkResult = (byte[])setHeaderMask;
                //var HeaderMarkResponse = ByteArrayToString(HeaderMarkResult);

                //var setp2max = await dongleCommWin.CAN_SetP2Max("2710");
                //var setp2maxresult = (byte[])setp2max;
                //var setp2maxresultstr = ByteArrayToString(setp2maxresult);

                //var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                //var firmwareResult = (byte[])firmwareVersion;
                //var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");

                //var version = ByteArrayToString(firmwareResult);
                #endregion



                //byte[] byteArray = Encoding.ASCII.GetBytes(message);
                //await socket.OutputStream.WriteAsync(byteArray, 0, byteArray.Length);
                //await socket.OutputStream.FlushAsync();

                //byte[] Rbuffer = new byte[1024];
                //byte[] RArray = new byte[] { };
                //try
                //{
                //    int readByte = socket.InputStream.Read(Rbuffer, 0, Rbuffer.Length);
                //    RArray = new byte[readByte];
                //    Array.Copy(Rbuffer.ToArray(), 0, RArray, 0, readByte);
                //    string ReceivedMessage = ConvertedByteToString(RArray);
                //}
                //catch (Exception ex)
                //{

                //    throw;
                //}



                //uint messageLength = (uint)message.Length;
                //byte[] countBuffer = BitConverter.GetBytes(messageLength);
                //byte[] buffer = HexStringToByteArray(message);

                //await socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                //await socket.OutputStream.FlushAsync();
                //await outStream.WriteAsync(buffer, 0, buffer.Length);
                //await outStream.WriteAsync(buffer, 0, buffer.Length);
                //ReadData(out string data);

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        string tx_header_temp = string.Empty;
        string rx_header_temp = string.Empty;
        int ProtocolValue = 0;
        public async Task<string> GetDongleMacID(string IP, bool is_disconnct)
        {
            try
            {
                string MacId = string.Empty;
                //clients.Close();
                if (IP == "")
                {
                    clients.Close();
                    return "";
                }
                if (clients != null)
                {
                    clients.Close();
                }

                if (!is_disconnct)
                {
                    clients = new TcpClient(IP, 6888);

                    //var ProtocolValue = (dynamic)null;

                    string ProtocolNameValue = StaticData.ecu_info.FirstOrDefault().protocol.name;//.protocol.name;               
                    string value = ProtocolNameValue.Replace("-", "_");
                    //string tx_header_temp = string.Empty;
                    //string rx_header_temp = string.Empty;


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
                            //tx_header_temp = "07E0";
                            //rx_header_temp = "07E8";
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

                    dongleCommWin = new DongleCommWin(clients, stream, ProtocolValue, Convert.ToUInt32(tx_header_temp, 16), Convert.ToUInt32(rx_header_temp, 16), 0x00, 0x10, 0x10, 0x10);

                    //dongleCommWin = new DongleCommWin(serialInputOutputManager: inputOutputManager, port, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x07E0, 0x07E8, 0x00, 0x10, 0x10, 0x10);
                    dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.WiFi);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
                    dSDiagnostic = new UDSDiagnostic(dongleCommWin);
                    var securityAccess = await dongleCommWin.SecurityAccess();
                    var GetMacId = await dongleCommWin.GetWifiMacId();
                    var GetMacId1 = (byte[])GetMacId;
                    MacId = GetMacId1[3].ToString("X2") + ":" +
                        GetMacId1[4].ToString("X2") + ":" +
                        GetMacId1[5].ToString("X2") + ":" +
                        GetMacId1[6].ToString("X2") + ":" +
                        GetMacId1[7].ToString("X2") + ":" +
                        GetMacId1[8].ToString("X2");
                }
                return MacId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                clients.Close();
                return "";
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

        public async Task<string> GetFirmware1()
        {
            try
            {
                //var securityAccess = await dongleCommWin.SecurityAccess();

                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                var firmwareResult = (byte[])firmwareVersion;
                var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                return ver;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> GetFirmware(string IP)
        {
            try
            {
                if (IP == "")
                {
                    clients.Close();
                }
                clients = new TcpClient(IP, 6888);

                dongleCommWin = new DongleCommWin(clients, stream, 0x00, 0x00, 0x00, 0x00, 0x10, 0x10, 0x10);
                dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.WiFi);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
                dSDiagnostic = new UDSDiagnostic(dongleCommWin);

                var securityAccess = await dongleCommWin.SecurityAccess();

                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                var firmwareResult = (byte[])firmwareVersion;
                var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");
                return ver;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private class NetworkCallback : ConnectivityManager.NetworkCallback
        {
            private ConnectivityManager _conn;
            public Action<Network> NetworkAvailable { get; set; }
            public Action NetworkUnavailable { get; set; }

            public NetworkCallback(ConnectivityManager connectivityManager)
            {
                _conn = connectivityManager;
            }

            public override void OnAvailable(Network network)
            {
                base.OnAvailable(network);
                // Need this to bind to network otherwise it is connected to wifi 
                // but traffic is not routed to the wifi specified
                _conn.BindProcessToNetwork(network);
                NetworkAvailable?.Invoke(network);
            }

            public override void OnUnavailable()
            {
                base.OnUnavailable();

                NetworkUnavailable?.Invoke();
            }
        }

        public async Task<Model.ReadDtcResponseModel> ReadDtc(string dtc_index)
        {
            try
            {
                if (dtc_index == "UDS-2BYTE-DTC")
                {
                    dtc_index = "UDS_2BYTE12_DTC";
                }
                dtc_index = dtc_index.Replace('-', '_');

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

        //static string ConvertedByteToString(byte[] bytes)
        //{
        //    using (var stream = new MemoryStream(bytes))
        //    {
        //        using (var stremReader = new StreamReader(stream))
        //        {
        //            return stremReader.ReadToEnd();
        //        }
        //    }
        //}
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

        #region Old Working Code
        //public async Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<Model.ReadParameterPID> pidList)
        //{
        //    try
        //    {
        //        object result = new object();
        //        ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID>();
        //        foreach (var item in pidList)
        //        {
        //            var MessageValueList = new List<APDiagnosticAndroid.Models.SelectedParameterMessage>();
        //            if (item.messages != null)
        //            {
        //                foreach (var MessageItem in item.messages)
        //                {
        //                    MessageValueList.Add(new APDiagnosticAndroid.Models.SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
        //                }
        //            }

        //            list.Add(
        //                new APDiagnosticAndroid.Models.ReadParameterPID
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
        //                    totalBytes = item.totalBytes,
        //                    totalLen = item.totalLen,
        //                    pidNumber = item.pidNumber,
        //                    pidName = item.pidName,
        //                    messages = MessageValueList
        //                });
        //        }

        //        var respo = await Task.Run(async () =>
        //        {
        //            result = await dSDiagnostic.ReadParameters(pidList.Count, list);
        //            var res = JsonConvert.SerializeObject(result);
        //            var res_list = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(res);
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
        #endregion
        // Write Para
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

        //Flashing
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

                //ECUCalculateSeedkey ecuCalculateSeedkey;

                string response = string.Empty;

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                StartTesterPresent();
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
                StopTesterPresent();
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

        public async Task TXheader(string tx_header)
        {
            var setHeader = await dongleCommWin.CAN_SetTxHeader(tx_header);
            var HeaderResult = (byte[])setHeader;
            var HeaderResponse = ByteArrayToString(HeaderResult);
            Debug.WriteLine("------DTC TX Header Set------" + tx_header + " ", "\n--Header Response--" + HeaderResponse);
        }
        public async Task RXheader(string rx_header)
        {
            var setHeader = await dongleCommWin.CAN_SetRxHeaderMask(rx_header);
            var HeaderResult = (byte[])setHeader;
            var HeaderResponse = ByteArrayToString(HeaderResult);
            Debug.WriteLine("------DTC RX Header Set------" + rx_header + " ", "RX Header Response--" + HeaderResponse);
        }


        public async Task<string[]> SendTerminalCommands(string IP, string[] commands)
        {
            try
            {
                string[] vs_res = new string[7];
                if (IP == "")
                {
                    clients.Close();
                }
                clients = new TcpClient(IP, 6888);


                dongleCommWin = new DongleCommWin(clients, stream, Convert.ToInt32(commands[0]), Convert.ToUInt32(commands[1], 16), Convert.ToUInt32(commands[2], 16), 0x00, 0x10, 0x10, 0x10);
                dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.WiFi);//InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android);
                dSDiagnostic = new UDSDiagnostic(dongleCommWin);

                var securityAccess = await dongleCommWin.SecurityAccess();


                var setProtocol = await dongleCommWin.Dongle_SetProtocol(Convert.ToInt32(commands[0]));
                vs_res[0] = commands[0];
                vs_res[5] = commands[5];
                var ProtocolResult = (byte[])setProtocol;

                var setHeader = await dongleCommWin.CAN_SetTxHeader(commands[1]);
                vs_res[1] = commands[1];
                var setHeaderMask = await dongleCommWin.CAN_SetRxHeaderMask(commands[2]);
                vs_res[2] = commands[2];
                if (!string.IsNullOrEmpty(commands[3]))
                {
                    var setpadding = await dongleCommWin.CAN_StartPadding("00");
                    vs_res[3] = "Enabled";
                }
                else
                {
                    vs_res[3] = "Disabled";
                }

                if (!string.IsNullOrEmpty(commands[4]))
                {
                    var setpadding = await dongleCommWin.CAN_StartPadding("00");
                    vs_res[4] = "Enabled";
                }
                else
                {
                    vs_res[4] = "Disabled";
                }

                var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
                var firmwareResult = (byte[])firmwareVersion;
                var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");

                vs_res[6] = ver;

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
            var setHeader = await dSDiagnostic.SetDataData(commands);

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

        public async Task<string[]> GetSsidPassword()
        {
            string[] Response = new string[4];
            try
            {


                int lenth = 0;

                var Default_SSID = await dongleCommWin.CAN_Get_Default_SSID();
                var defaultSSID = (byte[])Default_SSID;

                if (defaultSSID != null)
                {
                    if (defaultSSID.Length > 0)
                    {
                        string defaultSSID_decoded = Encoding.ASCII.GetString(defaultSSID);
                        if (!defaultSSID_decoded.Contains("NULL"))
                        {
                            lenth = defaultSSID.Length - 2;
                            for (int i = 0; i < defaultSSID.Length; i++)
                            {
                                if (i > 3 && i < lenth)
                                {
                                    Response[0] = Response[0] + defaultSSID[i];
                                }

                            }
                            //Response[0] = defaultSSID[4].ToString("X2") +
                            //        defaultSSID[5].ToString("X2") +
                            //        defaultSSID[6].ToString("X2") +
                            //        defaultSSID[7].ToString("X2") +
                            //        defaultSSID[8].ToString("X2") +
                            //        defaultSSID[9].ToString("X2");
                        }
                        else
                        {
                            Response[0] = "NULL";
                        }
                    }
                    Console.WriteLine($"DEFAULT SSID: {Response[0]}");
                }





                var Default_Password = await dongleCommWin.CAN_Get_Default_Password();
                var defaultPassword = (byte[])Default_Password;

                if (defaultPassword != null)
                {
                    if (defaultPassword.Length > 0)
                    {
                        string defaultPassword_decoded = Encoding.ASCII.GetString(defaultPassword);
                        if (!defaultPassword_decoded.Contains("NULL"))
                        {
                            lenth = defaultPassword.Length - 2;
                            for (int i = 0; i < defaultPassword.Length; i++)
                            {
                                if (i > 3 && i < lenth)
                                {
                                    Response[1] = Response[1] + defaultPassword[i];
                                }
                            }
                            //Response[1] = defaultPassword[4].ToString("X2") +
                            //defaultPassword[5].ToString("X2") +
                            //defaultPassword[6].ToString("X2") +
                            //defaultPassword[7].ToString("X2") +
                            //defaultPassword[8].ToString("X2") +
                            //defaultPassword[9].ToString("X2") +
                            //defaultPassword[10].ToString("X2") +
                            //defaultPassword[11].ToString("X2");
                        }
                        else
                        {
                            Response[1] = "NULL";
                        }
                    }
                    Console.WriteLine($"DEFAULT PASSWORD : {Response[1]}");
                }


                var User_SSID = await dongleCommWin.CAN_Get_User_SSID();
                var userSSID = (byte[])User_SSID;

                if (userSSID != null)
                {
                    if (userSSID.Length > 0)
                    {
                        string userSSID_decoded = Encoding.ASCII.GetString(userSSID);
                        if (!userSSID_decoded.Contains("NULL"))
                        {
                            lenth = userSSID.Length - 2;
                            for (int i = 0; i < userSSID.Length; i++)
                            {
                                if (i > 3 && i < lenth)
                                {
                                    Response[2] = Response[2] + userSSID[i];
                                }
                            }
                            //Response[2] = userSSID[4].ToString("X2") +
                            //userSSID[5].ToString("X2") +
                            //userSSID[6].ToString("X2") +
                            //userSSID[7].ToString("X2");
                        }
                        else
                        {
                            Response[2] = "NULL";
                        }
                    }
                    Console.WriteLine($"USER SSID : {Response[2]}");
                }





                var User_Password = await dongleCommWin.CAN_Get_User_Password();
                var userPassword = (byte[])User_Password;
                if (userPassword != null)
                {
                    if (userPassword.Length > 0)
                    {
                        string userPassword_decoded = Encoding.ASCII.GetString(userPassword);
                        if (!userPassword_decoded.Contains("NULL"))
                        {
                            lenth = userPassword.Length - 2;
                            for (int i = 0; i < userPassword.Length; i++)
                            {
                                if (i > 3 && i < lenth)
                                {
                                    Response[3] = Response[3] + userPassword[i];
                                }
                            }
                            //Response[3] = userPassword[4].ToString("X2") +
                            //userPassword[5].ToString("X2") +
                            //userPassword[6].ToString("X2") +
                            //userPassword[7].ToString("X2");
                        }
                        else
                        {
                            Response[3] = "NULL";
                        }
                    }
                    Console.WriteLine($"USER PASSWORD : {Response[3]}");
                }

                return Response;
            }
            catch (Exception ex)
            {
                return Response;
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

        public async Task<string> SendFotaCommand(string command)
        {
            try
            {
                var response = await dongleCommWin.Dongle_SetFota(command);
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        //public async Task<string> GetWifiMacIdCommand(string command)
        //{
        //    try
        //    {
        //        var response = await dongleCommWin.Dongle_SetFota(command);
        //        return "";
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";
        //    }
        //}

        //public class BindNetworkCallBack : ConnectivityManager.NetworkCallback
        //{
        //    public override void OnAvailable(Network network)
        //    {
        //        try
        //        {
        //            _conn.ConnectivityManager.BindProcessToNetwork(network);
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine(@"\tERROR Unable to Bind process to network {0}", ex.Message);
        //        }
        //        WifiConnect.ConnectivityManager.UnregisterNetworkCallback(this);
        //    }
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

        public async void StartTesterPresent()
        {
            var result = await dongleCommWin.CAN_StartTP();
        }

        public async void StopTesterPresent()
        {
            var result = await dongleCommWin.CAN_StopTP();
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
    //public class WifiConnector : IConnectionWifi
    //{
    //    OutputStreamInvoker outStream = null;
    //    InputStreamInvoker inStream = null;
    //    InputStreamReader inputStreamReader = null;
    //    BufferedReader bufferedReader = null;
    //    DongleCommWin dongleCommWin;
    //    UDSDiagnostic dSDiagnostic;
    //    NetworkStream stream;
    //    TcpClient clients;
    //    private APDiagnosticAndroid.Models.ReadDtcResponseModel readDTCResponse;
    //    public WifiConnector()
    //    {

    //    }
    //    public async void Connect_dongle()
    //    {
    //        Xamarin.Forms.Forms.Context.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionWifiSettings));
    //    }

    //    public Task<ObservableCollection<BluetoothDevicesModel>> enable_hotspots()
    //    {
    //        string str = string.Empty;
    //        return null;
    //    }

    //    // WifiManager wifiManager;
    //    //ConnectivityManager _conn;
    //    public async Task<ObservableCollection<BluetoothDevicesModel>> get_device_list()
    //    {
    //        try
    //        {
    //            ObservableCollection<BluetoothDevicesModel> available_dogles = new ObservableCollection<BluetoothDevicesModel>();
    //            WifiManager wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);

    //            if (!wifi.IsWifiEnabled)
    //            {
    //                wifi.SetWifiEnabled(true);
    //            }


    //            IReadOnlyList<IZeroconfHost> responses = null;
    //            string output;

    //            var domains = await ZeroconfResolver.BrowseDomainsAsync();

    //            responses = await ZeroconfResolver.ResolveAsync(domains.Select(g => g.Key));

    //            foreach (var resp in responses)
    //            {
    //                if (resp.DisplayName.Contains("obd"))
    //                {
    //                    available_dogles.Add(
    //                new BluetoothDevicesModel
    //                {
    //                    Name = resp.DisplayName,
    //                    Ip = resp.IPAddress
    //                });
    //                }
    //            }
    //            //foreach (var network in wifi.ConfiguredNetworks)
    //            //{
    //            //    available_dogles.Add(
    //            //   new BluetoothDevicesModel
    //            //   {
    //            //       Name = network.Ssid
    //            //       Mac_Address = network.Bssid
    //            //   });
    //            //}

    //            return available_dogles;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //        // true or false to activate/deactivate wifi

    //        //try
    //        //{
    //        //    // OBD-2EF4 
    //        //    // password1
    //        //    const string formattedSsid = "\"OBD2-2EF4\"";
    //        //    const string formattedPassword = "\"password1\"";

    //        //    var wifiConfig = new WifiConfiguration
    //        //    {
    //        //        Ssid = formattedSsid,
    //        //        PreSharedKey = formattedPassword,
    //        //        Priority = 0,
    //        //    };

    //        //    wifiManager = (WifiManager)Android.App.Application.Context
    //        //                   .GetSystemService(Context.WifiService);
    //        //    var addNetwork = wifiManager.AddNetwork(wifiConfig);
    //        //    System.Diagnostics.Debug.WriteLine("addNetwork = " + addNetwork);

    //        //    var network = wifiManager.ConfiguredNetworks.FirstOrDefault((n) => n.Ssid == "\"OBD2-2EF4\"");
    //        //    if (network == null)
    //        //    {
    //        //        System.Diagnostics.Debug.WriteLine("Didn't find the network, not connecting.");
    //        //        //return;
    //        //    }


    //        //   // wifiManager.Disconnect();

    //        //    var enableNetwork = wifiManager.EnableNetwork(network.NetworkId, true);
    //        //    System.Diagnostics.Debug.WriteLine("enableNetwork = " + enableNetwork);
    //        //    var isConncted = wifiManager.r();
    //        //    if (isConncted)
    //        //    {
    //        //        var builder = new AlertDialog.Builder(Android.App.Application.Context);
    //        //        builder.SetMessage("Connected");
    //        //        var alert = builder.Create();
    //        //        alert.Show();

    //        //    }
    //        //    else
    //        //    {
    //        //        var builder = new AlertDialog.Builder(Android.App.Application.Context);
    //        //        builder.SetMessage("Nope");
    //        //        var alert = builder.Create();
    //        //        alert.Show();
    //        //    }

    //        //    // var wifiSpecifier = new WifiNetworkSpecifier.Builder()
    //        //    //.SetSsid(ssid)
    //        //    //.SetWpa2Passphrase(preSharedKey)
    //        //    //.Build();

    //        //    // var request = new NetworkRequest.Builder()
    //        //    //     .AddTransportType(TransportType.Wifi) // we want WiFi
    //        //    //     .RemoveCapability(NetCapability.Internet) // Internet not required
    //        //    //     .SetNetworkSpecifier(wifiSpecifier) // we want _our_ network
    //        //    //     .Build();

    //        //    // _wifiManager.RequestNetwork(request, _networkCallback);
    //        //    return true;
    //        //}
    //        //catch (Exception ex)
    //        //{
    //        //    return false;
    //        //}

    //    }
    //    public async Task<string> get_ipaddress()
    //    {
    //        try
    //        {
    //            string ipAddress = "";
    //            List<string> ip_List = new List<string>();

    //            //foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
    //            //{
    //            //    if (f.OperationalStatus == OperationalStatus.Up)
    //            //    {
    //            //        var ip = f.GetIPProperties().GatewayAddresses;
    //            //        foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
    //            //        {
    //            //            ipAddress = d.Address.ToString();
    //            //        }
    //            //    }
    //            //}

    //            //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

    //            //if (adresses != null && adresses[0] != null)
    //            //{
    //            //    var data = adresses[0].ToString();
    //            //}
    //            //else
    //            //{
    //            //    ;
    //            //}


    //            //return ipAddress




    //            //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

    //            //if (adresses != null && adresses[0] != null)
    //            //{
    //            //    var data = adresses[0].ToString();
    //            //}
    //            //else
    //            //{
    //            //    ;
    //            //}

    //            //DhcpInfo d;

    //            //d = wifi.DhcpInfo;
    //            ////ipAddress =  d.Gateway.ToString();
    //            //int ip = d.Gateway;
    //            //ipAddress = Formatter.FormatIpAddress(ip);

    //            var bytes = HexStringToByteArray("05000b0000000000000000500c47568afe56214e238000ffc315e4");
    //            TcpClient client = new TcpClient("192.168.4.1", 6888);
    //            byte[] seq_command = new byte[] { 0x50, 0x0C, 0x47, 0x56, 0x8A, 0xFE, 0x56, 0x21, 0x4E, 0x23, 0x80, 0x00, 0xFF, 0xC3 };

    //            NetworkStream stream = client.GetStream();
    //            stream.Write(bytes, 0, bytes.Length);

    //            var response = await ReadData(stream);

    //            var SSID = HexStringToByteArray("0600110000000000000000201216014155544f2050454550414c203200732cd49b");
    //            stream.Write(SSID, 0, SSID.Length);
    //            var response1 = await ReadData(stream);


    //            var PASSWORD = HexStringToByteArray("0700110000000000000000201217016175746f70656570616c31323300b7565ab2");
    //            stream.Write(PASSWORD, 0, PASSWORD.Length);
    //            var response2 = await ReadData(stream);




    //            string ssid = "";
    //            string password = "";
    //            var wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);

    //            var formattedSsid = $"\"AUTO PEEPAL 2\"";
    //            var formattedPassword = $"\"autopeepal123\"";

    //            var wifiConfig = new WifiConfiguration
    //            {
    //                Ssid = formattedSsid,
    //                PreSharedKey = formattedPassword
    //            };

    //            var addNetwork = wifi.AddNetwork(wifiConfig);

    //            var network = wifi.ConfiguredNetworks.FirstOrDefault(n => n.Ssid == "\"AUTO PEEPAL 2\"");

    //            if (network == null)
    //            {
    //                Console.WriteLine($"Cannot connect to network: {ssid}");
    //                //return;
    //            }

    //            // wifiManager.Disconnect();

    //            var enableNetwork = wifi.EnableNetwork(network.NetworkId, true);
    //            System.Diagnostics.Debug.WriteLine("enableNetwork = " + enableNetwork);
    //            var isConncted = wifi.Reconnect();
    //            if (isConncted)
    //            {
    //                //var builder = new AlertDialog.Builder(Android.App.Application.Context);
    //                //builder.SetMessage("Connected");
    //                //var alert = builder.Create();
    //                //alert.Show();
    //                Dialog dialog = new Dialog(Android.App.Application.Context);
    //                dialog.Create();


    //            }
    //            else
    //            {
    //                var builder = new AlertDialog.Builder(Android.App.Application.Context);
    //                builder.SetMessage("Nope");
    //                var alert = builder.Create();
    //                alert.Show();
    //            }


    //            var domains = await ZeroconfResolver.BrowseDomainsAsync();
    //            //var domains = await ZeroconfResolver.BrowseDomainsAsync();

    //            //Console.WriteLine(Array2Text(RetArray, false) + "\n" + "[" + RetArray.Length.ToString() + "]");

    //            //Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);



    //            //while (true)
    //            //{
    //            //    byte[] receivedBuffer = new byte[1024];

    //            //    await stream.ReadAsync(receivedBuffer, 0, receivedBuffer.Length);

    //            //    StringBuilder msg = new StringBuilder();
    //            //    foreach (byte b in receivedBuffer)
    //            //    {
    //            //        if (b.Equals(00))
    //            //        {
    //            //            break;
    //            //        }
    //            //        else
    //            //        {
    //            //            msg.Append(Convert.ToChar(b).ToString());
    //            //        }

    //            //    }
    //            //    var sss = msg;
    //            //}


    //            //---------------------------------------------------
    //            //string ssid = "";
    //            //string password = "";
    //            //var wifiManager = (WifiManager)Android.App.Application.Context
    //            //       .GetSystemService(Context.WifiService);

    //            //var formattedSsid = $"\"{ssid}\"";
    //            //var formattedPassword = $"\"{password}\"";

    //            //var wifiConfig = new WifiConfiguration
    //            //{
    //            //    Ssid = formattedSsid,
    //            //    PreSharedKey = formattedPassword
    //            //};

    //            //var addNetwork = wifiManager.AddNetwork(wifiConfig);

    //            //var network = wifiManager.ConfiguredNetworks
    //            //     .FirstOrDefault(n => n.Ssid == ssid);

    //            //if (network == null)
    //            //{
    //            //    Console.WriteLine($"Cannot connect to network: {ssid}");
    //            //    //return;
    //            //}

    //            //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

    //            //if (adresses != null && adresses[0] != null)
    //            //{
    //            //    var data = adresses[0].ToString();
    //            //}
    //            //else
    //            //{
    //            //    ;
    //            //}
    //            return null;
    //            //-------------------------------------------------------------------------


    //            //WifiManager wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
    //            //WifiInfo wifiInfo = wifi.ConnectionInfo;
    //            //int ip = wifiInfo.IpAddress;
    //            //ipAddress = Formatter.FormatIpAddress(ip);


    //            //var res = Dns.GetHostName();
    //            //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

    //            //if (adresses != null && adresses[0] != null)
    //            //{
    //            //    return adresses[0].ToString();
    //            //}
    //            //else
    //            //{
    //            //    return null;
    //            //}
    //            //return ipAddress;
    //        }
    //        catch (Exception ex)
    //        {
    //            return "";
    //        }
    //    }

    //    TcpClient TcpClient = null;
    //    private NetworkStream Stream = null;

    //    public async Task<string> Write_SSIDPassword(string RouterSSID, string RouterPassword)
    //    {
    //        try
    //        {
    //            string ipAddress = "";
    //            List<string> ip_List = new List<string>();

    //            var bytes = HexStringToByteArray("05000b0000000000000000500c47568afe56214e238000ffc315e4");

    //            Debug.WriteLine("-------------Tcp Start-------------");
    //            TcpClient client = new TcpClient("192.168.4.1", 6888);
    //            Debug.WriteLine("-------------Tcp END-------------");

    //            byte[] seq_command = new byte[] { 0x50, 0x0C, 0x47, 0x56, 0x8A, 0xFE, 0x56, 0x21, 0x4E, 0x23, 0x80, 0x00, 0xFF, 0xC3 };

    //            NetworkStream stream = client.GetStream();
    //            stream.Write(bytes, 0, bytes.Length);

    //            Debug.WriteLine("-------------First ReadData Start-------------");
    //            var response = await ReadData(stream);
    //            Debug.WriteLine("-------------First ReadData End" + BitConverter.ToString(response) + "-------------");

    //            //// SSID
    //            string SSIDCommand = "20" + (RouterSSID.Length + 5).ToString("X2") + "1601" + ToHex(RouterSSID) + "00";

    //            string wificmd = ReadChecksum(RouterSSID, SSIDCommand);
    //            Debug.WriteLine("SSID Send Start -------------" + wificmd + "-------------");
    //            var byteData = HexStringToByteArray(wificmd);
    //            stream.Write(byteData, 0, byteData.Length);
    //            var SSIDResponse = await ReadData(stream);
    //            Debug.WriteLine("SSID Send End Response -------------" + BitConverter.ToString(SSIDResponse) + "-------------");


    //            //// SSID PASSWORD
    //            string pw = "20" + (RouterPassword.Length + 5).ToString("X2") + "1701" + ToHex(RouterPassword) + "00";
    //            string wifipwcmd = ReadChecksum(RouterPassword, pw);
    //            Debug.WriteLine("Password Send Start -------------" + wifipwcmd + "-------------");
    //            var byteDatapw = HexStringToByteArray(wifipwcmd);
    //            stream.Write(byteDatapw, 0, byteDatapw.Length);
    //            var SSIDpwResponse = await ReadData(stream);
    //            Debug.WriteLine("Password Send End Response -------------" + BitConverter.ToString(SSIDpwResponse) + "-------------");


    //            string ssid = "";
    //            string password = "";
    //            var wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);

    //            var formattedSsid = RouterSSID;
    //            var formattedPassword = RouterPassword;

    //            var wifiConfig = new WifiConfiguration
    //            {
    //                Ssid = formattedSsid,
    //                PreSharedKey = formattedPassword
    //            };

    //            var addNetwork = wifi.AddNetwork(wifiConfig);

    //            var network = wifi.ConfiguredNetworks.FirstOrDefault(n => n.Ssid == RouterSSID);

    //            if (network == null)
    //            {
    //                Console.WriteLine($"Cannot connect to network: {ssid}");
    //                //return;
    //            }

    //            // wifiManager.Disconnect();

    //            var enableNetwork = wifi.EnableNetwork(network.NetworkId, true);
    //            System.Diagnostics.Debug.WriteLine("enableNetwork = " + enableNetwork);
    //            var isConncted = wifi.Reconnect();
    //            if (isConncted)
    //            {
    //                //var builder = new AlertDialog.Builder(Android.App.Application.Context);
    //                //builder.SetMessage("Connected");
    //                //var alert = builder.Create();
    //                //alert.Show();
    //                Dialog dialog = new Dialog(Android.App.Application.Context);
    //                dialog.Create();
    //            }
    //            else
    //            {
    //                var builder = new AlertDialog.Builder(Android.App.Application.Context);
    //                builder.SetMessage("Nope");
    //                var alert = builder.Create();
    //                alert.Show();
    //            }


    //            var domains = await ZeroconfResolver.BrowseDomainsAsync();
    //            //var domains = await ZeroconfResolver.BrowseDomainsAsync();

    //            //Console.WriteLine(Array2Text(RetArray, false) + "\n" + "[" + RetArray.Length.ToString() + "]");

    //            //Debug.WriteLine("--------- READ DATA RESPONSE-------" + ByteArrayToString(RetArray), DebugTag);



    //            //while (true)
    //            //{
    //            //    byte[] receivedBuffer = new byte[1024];

    //            //    await stream.ReadAsync(receivedBuffer, 0, receivedBuffer.Length);

    //            //    StringBuilder msg = new StringBuilder();
    //            //    foreach (byte b in receivedBuffer)
    //            //    {
    //            //        if (b.Equals(00))
    //            //        {
    //            //            break;
    //            //        }
    //            //        else
    //            //        {
    //            //            msg.Append(Convert.ToChar(b).ToString());
    //            //        }

    //            //    }
    //            //    var sss = msg;
    //            //}


    //            //---------------------------------------------------
    //            //string ssid = "";
    //            //string password = "";
    //            //var wifiManager = (WifiManager)Android.App.Application.Context
    //            //       .GetSystemService(Context.WifiService);

    //            //var formattedSsid = $"\"{ssid}\"";
    //            //var formattedPassword = $"\"{password}\"";

    //            //var wifiConfig = new WifiConfiguration
    //            //{
    //            //    Ssid = formattedSsid,
    //            //    PreSharedKey = formattedPassword
    //            //};

    //            //var addNetwork = wifiManager.AddNetwork(wifiConfig);

    //            //var network = wifiManager.ConfiguredNetworks
    //            //     .FirstOrDefault(n => n.Ssid == ssid);

    //            //if (network == null)
    //            //{
    //            //    Console.WriteLine($"Cannot connect to network: {ssid}");
    //            //    //return;
    //            //}

    //            //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

    //            //if (adresses != null && adresses[0] != null)
    //            //{
    //            //    var data = adresses[0].ToString();
    //            //}
    //            //else
    //            //{
    //            //    ;
    //            //}
    //            return null;
    //            //-------------------------------------------------------------------------


    //            //WifiManager wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
    //            //WifiInfo wifiInfo = wifi.ConnectionInfo;
    //            //int ip = wifiInfo.IpAddress;
    //            //ipAddress = Formatter.FormatIpAddress(ip);


    //            //var res = Dns.GetHostName();
    //            //IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

    //            //if (adresses != null && adresses[0] != null)
    //            //{
    //            //    return adresses[0].ToString();
    //            //}
    //            //else
    //            //{
    //            //    return null;
    //            //}
    //            //return ipAddress;
    //        }
    //        catch (Exception ex)
    //        {
    //            return "";
    //        }
    //    }
    //    private string ReadChecksum(string input, string command)
    //    {
    //        var dataBytes = HexStringToByteArray(command.Substring(4));
    //        var Checksum = Crc16CcittKermit.ComputeChecksum(dataBytes);
    //        command = command + Checksum.ToString("X4");

    //        var byte1 = "00";
    //        var byte2 = (command.Length / 2).ToString("X4");
    //        var byte3 = DateTime.Now.ToString("hhmmssff");
    //        var byte4 = DateTime.Now.ToString("00000000");
    //        //var byte5 = ByteArrayToString(Encoding.ASCII.GetBytes(command));
    //        var dataBytes_ = HexStringToByteArray(byte1 + byte2 + byte3 + byte4 + command);
    //        var byte6Checksum = Crc16CcittKermit.ComputeChecksum(dataBytes_);
    //        var returnstr = byte1 + byte2 + byte3 + byte4 + command + byte6Checksum.ToString("X4");

    //        return returnstr;
    //    }
    //    private string ByteArrayToString(byte[] ba)
    //    {
    //        string hex = BitConverter.ToString(ba);
    //        return hex.Replace("-", "");
    //    }

    //    private string ToHex(string input)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        foreach (char c in input)
    //            sb.AppendFormat("{0:X2}", (int)c);
    //        return sb.ToString().Trim();
    //    }

    //    private async Task<byte[]> ReadData(NetworkStream stream)
    //    {
    //        byte[] rbuffer = new byte[1024];
    //        byte[] RetArray = new byte[] { };

    //        //Debug.WriteLine("---------INSIDE READ DATA-------" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:ffff"), DebugTag);
    //        // Read data from the device
    //        //while (!stream.CanRead)
    //        //{
    //        //    //Console.WriteLine("------------------------------------------------");
    //        //}
    //        int readByte = await stream.ReadAsync(rbuffer, 0, rbuffer.Length);

    //        RetArray = new byte[readByte];
    //        Array.Copy(rbuffer, 0, RetArray, 0, readByte);
    //        return RetArray;
    //    }

    //    public async Task<string> Connect(string IP)
    //    {
    //        if (IP == "")
    //        {
    //            clients.Close();
    //        }

    //        var firmware_version = await SendMessage("500C47568AFE56214E238000FFC3", IP);
    //        return firmware_version;

    //        //TcpClient clients = new TcpClient(IP, 6888);
    //        //if (clients.Connected == true)
    //        //{               
    //        //    var bytes = HexStringToByteArray("05000b0000000000000000500c47568afe56214e238000ffc315e4");

    //        //    NetworkStream streamm = clients.GetStream();
    //        //    streamm.Write(bytes, 0, bytes.Length);
    //        //    byte[] finalData = new byte[8];
    //        //    var responseee = await ReadData(streamm);
    //        //}
    //    }

    //    private async Task<string> SendMessage(string message, string IP)
    //    {
    //        try
    //        {
    //            clients = new TcpClient(IP, 6888);

    //            //dongleCommWin = new DongleCommWin(clients, stream, APDongleCommAnroid.Protocol.ISO15765_500KB_11BIT_CAN, 0x7E0, 0x7E8, 0x00, 0x10, 0x10, 0x10);
    //            dongleCommWin.InitializePlatform(APDongleCommAnroid.ENUMS.Platform.Android, APDongleCommAnroid.ENUMS.Connectivity.WiFi);
    //            dSDiagnostic = new UDSDiagnostic(dongleCommWin);

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

    //            var setp2max = await dongleCommWin.CAN_SetP2Max("2710");
    //            var setp2maxresult = (byte[])setp2max;
    //            var setp2maxresultstr = ByteArrayToString(setp2maxresult);

    //            var firmwareVersion = await dongleCommWin.Dongle_GetFimrwareVersion();
    //            var firmwareResult = (byte[])firmwareVersion;
    //            var ver = firmwareResult[3].ToString("D2") + "." + firmwareResult[4].ToString("D2") + "." + firmwareResult[5].ToString("D2");

    //            var version = ByteArrayToString(firmwareResult);

    //            //byte[] byteArray = Encoding.ASCII.GetBytes(message);
    //            //await socket.OutputStream.WriteAsync(byteArray, 0, byteArray.Length);
    //            //await socket.OutputStream.FlushAsync();

    //            //byte[] Rbuffer = new byte[1024];
    //            //byte[] RArray = new byte[] { };
    //            //try
    //            //{
    //            //    int readByte = socket.InputStream.Read(Rbuffer, 0, Rbuffer.Length);
    //            //    RArray = new byte[readByte];
    //            //    Array.Copy(Rbuffer.ToArray(), 0, RArray, 0, readByte);
    //            //    string ReceivedMessage = ConvertedByteToString(RArray);
    //            //}
    //            //catch (Exception ex)
    //            //{

    //            //    throw;
    //            //}

    //            return ver;

    //            //uint messageLength = (uint)message.Length;
    //            //byte[] countBuffer = BitConverter.GetBytes(messageLength);
    //            //byte[] buffer = HexStringToByteArray(message);

    //            //await socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    //            //await socket.OutputStream.FlushAsync();
    //            //await outStream.WriteAsync(buffer, 0, buffer.Length);
    //            //await outStream.WriteAsync(buffer, 0, buffer.Length);
    //            //ReadData(out string data);

    //        }
    //        catch (Exception ex)
    //        {
    //            return "";
    //        }
    //    }
    //    static string ConvertedByteToString(byte[] bytes)
    //    {
    //        using (var stream = new MemoryStream(bytes))
    //        {
    //            using (var stremReader = new StreamReader(stream))
    //            {
    //                return stremReader.ReadToEnd();
    //            }
    //        }
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

    //    private class NetworkCallback : ConnectivityManager.NetworkCallback
    //    {
    //        private ConnectivityManager _conn;
    //        public Action<Network> NetworkAvailable { get; set; }
    //        public Action NetworkUnavailable { get; set; }

    //        public NetworkCallback(ConnectivityManager connectivityManager)
    //        {
    //            _conn = connectivityManager;
    //        }

    //        public override void OnAvailable(Network network)
    //        {
    //            base.OnAvailable(network);
    //            // Need this to bind to network otherwise it is connected to wifi 
    //            // but traffic is not routed to the wifi specified
    //            _conn.BindProcessToNetwork(network);
    //            NetworkAvailable?.Invoke(network);
    //        }

    //        public override void OnUnavailable()
    //        {
    //            base.OnUnavailable();

    //            NetworkUnavailable?.Invoke();
    //        }
    //    }

    //    public async Task<Model.ReadDtcResponseModel> ReadDtc(string dtc_index)
    //    {
    //        try
    //        {
    //            if (dtc_index == "UDS-2BYTE-DTC")
    //            {
    //                dtc_index = "UDS_2BYTE12_DTC";
    //            }

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
    //    public async Task<string> ClearDtc(string dtc_index)
    //    {
    //        try
    //        {
    //            if (dtc_index == "UDS-4BYTES")
    //            {
    //                dtc_index = "UDS_4BYTES";
    //            }

    //            string status = string.Empty;
    //            object result = new object();
    //            ReadDTCIndex index = (ReadDTCIndex)Enum.Parse(typeof(ReadDTCIndex), dtc_index);
    //            await Task.Run(async () =>
    //            {

    //                result = await dSDiagnostic.ClearDTC(index);
    //                var res = JsonConvert.SerializeObject(result);
    //                var Response = JsonConvert.DeserializeObject<ClearDtcResponseModel>(res);
    //                status = Response.ECUResponseStatus;
    //            });

    //            return status;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }


    //    //PID
    //    public async Task<ObservableCollection<ReadPidPresponseModel>> ReadPid(ObservableCollection<Model.ReadParameterPID> pidList)
    //    {
    //        try
    //        {
    //            object result = new object();
    //            ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID> list = new ObservableCollection<APDiagnosticAndroid.Models.ReadParameterPID>();
    //            foreach (var item in pidList)
    //            {
    //                var MessageValueList = new List<APDiagnosticAndroid.Models.SelectedParameterMessage>();
    //                if (item.messages != null)
    //                {
    //                    foreach (var MessageItem in item.messages)
    //                    {
    //                        MessageValueList.Add(new APDiagnosticAndroid.Models.SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
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
    //                        messages = MessageValueList
    //                    });
    //            }

    //            var respo = await Task.Run(async () =>
    //            {
    //                result = await dSDiagnostic.ReadParameters(pidList.Count, list);
    //                var res = JsonConvert.SerializeObject(result);
    //                var res_list = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(res);
    //                return res_list;
    //                //status = Response.ECUResponseStatus;
    //            });

    //            return respo;
    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }

    //    // Write Para
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

    //    //Flashing
    //    public async Task<object> StartECUFlashing(string flashJson, Ecu2 ecu2, SeedkeyalgoFnIndex sklFN, List<EcuMapFile> ecu_map_file)
    //    {
    //        try
    //        {
    //            var jsonData = JsonConvert.DeserializeObject<FlashingMatrixData>(flashJson);

    //            EraseSectorEnum erase_type = (EraseSectorEnum)Enum.Parse(typeof(EraseSectorEnum), ecu2.flash_erase_type);
    //            ChecksumSectorEnum check_sum_type = (ChecksumSectorEnum)Enum.Parse(typeof(ChecksumSectorEnum), ecu2.flash_check_sum_type);
    //            SEEDKEYINDEXTYPE seedkeyindx = (SEEDKEYINDEXTYPE)Enum.Parse(typeof(SEEDKEYINDEXTYPE), sklFN.value);

    //            //byte asdb = Convert.ToByte(ecu2.flash_address_data_format);
    //            //UInt16 acc = Convert.ToUInt16(ecu2.sectorframetransferlen,16);

    //            var flashConfig = new flashconfig
    //            {
    //                //addrdataformat = Convert.ToByte(ecu2.flash_address_data_format, 16),
    //                checksumsector = check_sum_type,
    //                diag_mode = Convert.ToByte(ecu2.flash_diagnostic_mode, 16),
    //                erasesector = erase_type,
    //                //flash_index = FLASHINDEXTYPE.GREAVES_BOSCH_BS6,
    //                sectorframetransferlen = Convert.ToUInt16(ecu2.sectorframetransferlen, 16),
    //                seedkeyindex = seedkeyindx,
    //                seedkeynumbytes = Convert.ToByte(ecu2.flash_seed_key_length, 16),
    //                sendseedbyte = Convert.ToByte(ecu2.sendseedbyte, 16),
    //                septime = Convert.ToByte(ecu2.flashsep_time, 16),
    //            };


    //            string response = string.Empty;

    //            Stopwatch stopWatch = new Stopwatch();
    //            stopWatch.Start();

    //            await Task.Run(async () =>
    //            {
    //                //response = await dSDiagnostic.StartFlashBosch(flashConfig, jsonData.NoOfSectors, jsonData.SectorData.ToArray());
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

    //    //public class BindNetworkCallBack : ConnectivityManager.NetworkCallback
    //    //{
    //    //    public override void OnAvailable(Network network)
    //    //    {
    //    //        try
    //    //        {
    //    //            _conn.ConnectivityManager.BindProcessToNetwork(network);
    //    //        }
    //    //        catch (Exception ex)
    //    //        {
    //    //            Debug.WriteLine(@"\tERROR Unable to Bind process to network {0}", ex.Message);
    //    //        }
    //    //        WifiConnect.ConnectivityManager.UnregisterNetworkCallback(this);
    //    //    }
    //    //}
    //}
    #endregion
}