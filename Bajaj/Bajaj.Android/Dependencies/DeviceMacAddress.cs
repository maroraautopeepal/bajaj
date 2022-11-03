using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using Xamarin.Forms;

[assembly: Dependency(typeof(DeviceMacAddress))]
namespace Bajaj.Droid.Dependencies
{
    public class DeviceMacAddress : IDeviceMacAddress
    {
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

            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}