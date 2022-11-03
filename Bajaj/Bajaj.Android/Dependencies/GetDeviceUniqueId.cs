using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using System;
using System.Net.NetworkInformation;
using Xamarin.Forms;

[assembly: Dependency(typeof(GetDeviceUniqueId))]
namespace Bajaj.Droid.Dependencies
{
    public class GetDeviceUniqueId : IGetDeviceUniqueId
    {
        public string GetId()
        {
            try
            {
                //NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                //String sMacAddress = string.Empty;
                //foreach (NetworkInterface adapter in nics)
                //{
                //    if (adapter.Description == "wlan0")
                //    {
                //        IPInterfaceProperties properties = adapter.GetIPProperties();
                //        sMacAddress = adapter.GetPhysicalAddress().ToString();
                //    }
                //    if (adapter.Description == "ccmni1")
                //    {

                //    }
                //    if (sMacAddress == String.Empty)// only return MAC Address from first card
                //    {
                //        IPInterfaceProperties properties = adapter.GetIPProperties();
                //        sMacAddress = adapter.GetPhysicalAddress().ToString();
                //    }
                //}

                //Android.Telephony.TelephonyManager mTelephonyMgr;
                ////if(Android.OS.Build.VERSION.SdkInt >=29)
                ////{

                ////}
                //mTelephonyMgr = (Android.Telephony.TelephonyManager)Forms.Context.GetSystemService(Android.Content.Context.TelephonyService);
                //// return mTelephonyMgr.DeviceId;

                //return "1234567890";
                string id = string.Empty;
                id = Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                return id;
            }
            catch (Exception ex)
            {
                return "Invalid Device Id";
            }
        }
    }
}