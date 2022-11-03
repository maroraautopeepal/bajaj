namespace APDongleCommAnroid
{

    internal interface IWifiUSBHandler
    {
        //object Wifi_GetDevices();

        //object Wifi_ConnectAP();

        //object Wifi_WriteSSIDPW();

        //object Wifi_ConnectStation();

        bool Wifi_Disconnect();

        //bool USB_Connect();

        bool USB_Disconnect();

    }

    public enum Protocol : byte
    {
        ISO15765_250KB_11BIT_CAN = 00,
        ISO15765_250Kb_29BIT_CAN = 01,
        ISO15765_500KB_11BIT_CAN = 02,
        ISO15765_500KB_29BIT_CAN = 03,
        ISO15765_1MB_11BIT_CAN = 04,
        ISO15765_1MB_29BIT_CAN = 05,
        I250KB_11BIT_CAN = 06,
        I250Kb_29BIT_CAN = 07,
        I500KB_11BIT_CAN = 08,
        I500KB_29BIT_CAN = 09,
        I1MB_11BIT_CAN = 0x0A,
        I1MB_29BIT_CAN = 0x0B,
        OE_IVN_250KBPS_11BIT_CAN = 0x0C,
        OE_IVN_250KBPS_29BIT_CAN = 0x0D,
        OE_IVN_500KBPS_11BIT_CAN = 0x0E,
        OE_IVN_500KBPS_29BIT_CAN = 0x0F,

        OE_IVN_1MBPS_11BIT_CAN = 0x10,
        OE_IVN_1MBPS_29BIT_CAN = 0x11,
        CANOPEN_125KBPS_11BIT_CAN = 0x12,
        CANOPEN_500KBPS_11BIT_CAN = 0x13,
        XMODEM_125KBPS_11BIT_CAN = 0x18,
        XMODEM_500KBPS_11BIT_CAN = 0x1a,
        XMODEM_500KBPS_29BIT_CAN = 0x1b,
        XMODEM_125KBPS_29BIT_CAN = 0x19




    }
}
