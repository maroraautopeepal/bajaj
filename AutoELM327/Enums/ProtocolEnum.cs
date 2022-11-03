using System;
using System.Collections.Generic;
using System.Text;

namespace AutoELM327.Enums
{
    public enum ProtocolEnum
    {
        //Automatic = 0x00,
        //SAE_J1850_PWM = 0x01,
        //SAE_J1850_VPW = 0x02,
        //ISO_9141_2 = 0x03,
        //ISO14230_4KWP_5BAUD_INIT = 0x04,
        //ISO14230_4KWP_FASTINIT = 0x05,
        //ISO15765_500KB_11BIT_CAN = 0x06,
        //ISO15765_500KB_29BIT_CAN = 0x07,
        //ISO15765_250KB_11BIT_CAN = 0x08,
        //ISO15765_250Kb_29BIT_CAN = 0x09,
        //SAEJ1939CAN_29BIT_ID_250KBAUD = 0x0A,

        ISO15765_250KB_11BIT_CAN = 0x00,
        ISO15765_250Kb_29BIT_CAN = 0x01,
        ISO15765_500KB_11BIT_CAN = 0x02,
        ISO15765_500KB_29BIT_CAN = 0x03,
        ISO15765_1MB_11BIT_CAN = 0x04,
        ISO15765_1MB_29BIT_CAN = 0x05,
        I250KB_11BIT_CAN = 0x06,
        I250Kb_29BIT_CAN = 0x07,
        I500KB_11BIT_CAN = 0x08,
        I500KB_29BIT_CAN = 0x09,
        I1MB_11BIT_CAN = (int)0x0A,
        I1MB_29BIT_CAN = (int)0x0B,
        OE_IVN_250KBPS_11BIT_CAN = (int)0x0C,
        OE_IVN_250KBPS_29BIT_CAN = (int)0x0D,
        OE_IVN_500KBPS_11BIT_CAN = (int)0x0E,
        OE_IVN_500KBPS_29BIT_CAN = (int)0x0F,
        OE_IVN_1MBPS_11BIT_CAN = (int)0x10,
        OE_IVN_1MBPS_29BIT_CAN = (int)0x11,
        ISO14230_4KWP_FASTINIT = (int)0x12,
    }
    public enum Connectivity
    {
        None,
        Bluetooth,
        WiFi,
        USB
    }
    public enum Platform
    {
        None,
        Android,
        UWP,
        iOS
    }
}
