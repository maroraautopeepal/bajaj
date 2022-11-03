using AutoELM327.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AutoELM327
{
    //----------------------------------------------------------------------------
    // Namespace        : AutoELM327
    // Class Name       : IBluetoothHandler
    // Description      : This interface is implemented by Bluetooth Handler class for platform specific BT operations
    // Author           : Autopeepal  
    // Date             : 20-08-20
    // Notes            : 
    // Revision History : 
    //----------------------------------------------------------------------------
    internal interface IBluetoothHandler
    {

        //----------------------------------------------------------------------------
        // Method Name   : IsCommModeEnabled
        // Input         : NA
        // Output        : status of IsCommModeEnabledcin form of bool
        // Purpose       : To check if the communication mode is enabled 
        // Date          : 20-08-20
        //---------------------------------------------------------------------------

        bool BT_ConnectDevice(string deviceName);

        //----------------------------------------------------------------------------
        // Method Name   : EnableCommMode
        // Input         : NA
        // Purpose       : To enable the communication mode on device
        // Output        : Status of EnableCommMode in form of bool
        // Date          : 20-08-20
        //----------------------------------------------------------------------------
        bool BT_Disconnect();

        //----------------------------------------------------------------------------
        // Method Name   : GetDevicesList
        // Input         : NA
        // Output        : List of devices in form of ObservableCollection<BluetoothDevices>
        // Purpose       : To get list of paired bluetooth devices 
        // Date          : 20-08-20
        //----------------------------------------------------------------------------
        ObservableCollection<BluetoothDevices> BT_GetDevices();//called in DeviceList()

    }
}
