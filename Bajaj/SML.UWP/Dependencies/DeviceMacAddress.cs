
using SML.Interfaces;
using SML.UWP.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: Dependency(typeof(DeviceMacAddress))]
namespace SML.UWP.Dependencies
{
    public class DeviceMacAddress : IDeviceMacAddress
    {
        //const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        //const int ERROR_BUFFER_OVERFLOW = 111;
        //const int MAX_ADAPTER_NAME_LENGTH = 256;
        //const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        //const int MIB_IF_TYPE_OTHER = 1;
        //const int MIB_IF_TYPE_ETHERNET = 6;
        //const int MIB_IF_TYPE_TOKENRING = 9;
        //const int MIB_IF_TYPE_FDDI = 15;
        //const int MIB_IF_TYPE_PPP = 23;
        //const int MIB_IF_TYPE_LOOPBACK = 24;
        //const int MIB_IF_TYPE_SLIP = 28;

        //[DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        //public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);
        public string GetMacAddress()
        {
            try
            {




                var ni = NetworkInterface.GetAllNetworkInterfaces().OrderBy(intf => intf.NetworkInterfaceType)
          .FirstOrDefault(intf => intf.OperationalStatus == OperationalStatus.Up
           && (intf.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
           || intf.NetworkInterfaceType == NetworkInterfaceType.Ethernet));
                var hw = ni.GetPhysicalAddress();
                return string.Join(":", (from ma in hw.GetAddressBytes() select ma.ToString("X2")).ToArray());

                #region Old
                ////IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
                ////NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                ////Console.WriteLine("Interface information for {0}.{1}     ",
                ////        computerProperties.HostName, computerProperties.DomainName);
                //////if (nics == null || nics.Length < 1)
                //////{
                //////    Console.WriteLine("  No network interfaces found.");
                //////    return;
                //////}

                ////Console.WriteLine("  Number of interfaces .................... : {0}", nics.Length);
                ////foreach (NetworkInterface adapter in nics)
                ////{
                ////    IPInterfaceProperties properties = adapter.GetIPProperties(); //  .GetIPInterfaceProperties();
                ////    Console.WriteLine();
                ////    Console.WriteLine(adapter.Description);
                ////    Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
                ////    Console.WriteLine("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
                ////    Console.Write("  Physical address ........................ : ");
                ////    PhysicalAddress address = adapter.GetPhysicalAddress();
                ////    byte[] bytes = address.GetAddressBytes();
                ////    for (int i = 0; i < bytes.Length; i++)
                ////    {
                ////        // Display the physical address in hexadecimal.
                ////        Console.Write("{0}", bytes[i].ToString("X2"));
                ////        // Insert a hyphen after each byte, unless we are at the end of the
                ////        // address.
                ////        if (i != bytes.Length - 1)
                ////        {
                ////            Console.Write("-");
                ////        }
                ////    }
                ////    Console.WriteLine();
                ////}

                ////return "";

                //var adapters = new List<AdapterInfo>();

                //long structSize = Marshal.SizeOf(typeof(IP_ADAPTER_INFO));
                //IntPtr pArray = Marshal.AllocHGlobal(new IntPtr(structSize));

                //int ret = GetAdaptersInfo(pArray, ref structSize);

                //if (ret == ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
                //{
                //    // Buffer was too small, reallocate the correct size for the buffer.
                //    pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));

                //    ret = GetAdaptersInfo(pArray, ref structSize);
                //}

                //if (ret == 0)
                //{
                //    // Call Succeeded
                //    IntPtr pEntry = pArray;

                //    do
                //    {
                //        var adapter = new AdapterInfo();

                //        // Retrieve the adapter info from the memory address
                //        var entry = (IP_ADAPTER_INFO)Marshal.PtrToStructure(pEntry, typeof(IP_ADAPTER_INFO));

                //        // Adapter Type
                //        switch (entry.Type)
                //        {
                //            case MIB_IF_TYPE_ETHERNET:
                //                adapter.Type = "Ethernet";
                //                break;
                //            case MIB_IF_TYPE_TOKENRING:
                //                adapter.Type = "Token Ring";
                //                break;
                //            case MIB_IF_TYPE_FDDI:
                //                adapter.Type = "FDDI";
                //                break;
                //            case MIB_IF_TYPE_PPP:
                //                adapter.Type = "PPP";
                //                break;
                //            case MIB_IF_TYPE_LOOPBACK:
                //                adapter.Type = "Loopback";
                //                break;
                //            case MIB_IF_TYPE_SLIP:
                //                adapter.Type = "Slip";
                //                break;
                //            default:
                //                adapter.Type = "Other/Unknown";
                //                break;
                //        } // switch

                //        adapter.Name = entry.AdapterName;
                //        adapter.Description = entry.AdapterDescription;

                //        // MAC Address (data is in a byte[])
                //        adapter.MAC = string.Join("-", Enumerable.Range(0, (int)entry.AddressLength).Select(s => string.Format("{0:X2}", entry.Address[s])));

                //        // Get next adapter (if any)

                //        adapters.Add(adapter);

                //        pEntry = entry.Next;
                //    }
                //    while (pEntry != IntPtr.Zero);

                //    Marshal.FreeHGlobal(pArray);
                //}
                //else
                //{
                //    Marshal.FreeHGlobal(pArray);
                //    throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
                //}

                //return "";
                //////////string macAddresses = "";
                //////////foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                //////////{
                //////////    // Only consider Ethernet network interfaces, thereby ignoring any
                //////////    // loopback devices etc.
                //////////    if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;
                //////////    if (nic.OperationalStatus == OperationalStatus.Up)
                //////////    {
                //////////        macAddresses += nic.GetPhysicalAddress().ToString();
                //////////        break;
                //////////    }
                //////////}
                //////////return macAddresses;

                #endregion
            }
            catch (Exception ex)
            {

                return "";
            }
        }
        private BoxView _nativeView;
        public void InitLoadingPage(ContentPage loadingIndicatorPage)
        {
            

            if (loadingIndicatorPage != null)
            {
                loadingIndicatorPage.Parent = Xamarin.Forms.Application.Current.MainPage;

                loadingIndicatorPage.Layout(new Rectangle(0, 0,
                    Xamarin.Forms.Application.Current.MainPage.Width,
                    Xamarin.Forms.Application.Current.MainPage.Height));
                var renderer = loadingIndicatorPage.GetOrCreateRenderer();
            }
        }


        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //public struct IP_ADAPTER_INFO
        //{
        //    public IntPtr Next;
        //    public Int32 ComboIndex;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
        //    public string AdapterName;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
        //    public string AdapterDescription;
        //    public UInt32 AddressLength;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
        //    public byte[] Address;
        //    public Int32 Index;
        //    public UInt32 Type;
        //    public UInt32 DhcpEnabled;
        //    public IntPtr CurrentIpAddress;
        //    public IP_ADDR_STRING IpAddressList;
        //    public IP_ADDR_STRING GatewayList;
        //    public IP_ADDR_STRING DhcpServer;
        //    public bool HaveWins;
        //    public IP_ADDR_STRING PrimaryWinsServer;
        //    public IP_ADDR_STRING SecondaryWinsServer;
        //    public Int32 LeaseObtained;
        //    public Int32 LeaseExpires;
        //}

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //public struct IP_ADDR_STRING
        //{
        //    public IntPtr Next;
        //    public IP_ADDRESS_STRING IpAddress;
        //    public IP_ADDRESS_STRING IpMask;
        //    public Int32 Context;
        //}

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //public struct IP_ADDRESS_STRING
        //{
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        //    public string Address;
        //}
    }
    public class AdapterInfo
    {
        public string Type { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string MAC { get; set; }
    }
}
