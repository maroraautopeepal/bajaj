using APDiagnosticAndroid.Enums;
using System;

namespace APDiagnosticAndroid.Structures
{
    public struct flashconfig
    {
        public FLASHINDEXTYPE flash_index;
        public byte diag_mode;
        public byte sendseedbyte;
        public byte seedkeynumbytes;
        public SEEDKEYINDEXTYPE seedkeyindex;
        public string addrdataformat;
        public UInt16 sectorframetransferlen;
        public byte septime;
        public EraseSectorEnum erasesector;
        public ChecksumSectorEnum checksumsector;
        public string FlashStatus;

        //public FLASHINDEXTYPE flash_index;
        //public byte diag_mode;
        //public byte sendseedbyte;
        //public byte seedkeynumbytes;
        //public SEEDKEYINDEXTYPE seedkeyindex;
        //public byte addrdataformat;
        //public UInt16 sectorframetransferlen;
        //public byte septime;
        //public EraseSectorEnum erasesector;
        //public ChecksumSectorEnum checksumsector;
    }

    public enum EraseSectorEnum
    {
        None,
        ERASEALLATONCE,
        ERASEBYSECTOR,
        ERASEALLATONCE_WOADDR,
    }

    public enum ChecksumSectorEnum
    {
        None,
        COMPARE_2BYTESIMPLEADD_BYSECTOR,
        COMPARE_CRC16CCITT_BYSECTOR,
        COMPUTEBYSECTOR,
        NOCHECKSUM_BYSECTOR,
        //None,
        COMPAREBYSECTOR,
        //COMPUTEBYSECTOR,
        NOCHECKSUM,
        COMPAREBYSECTOR_WOADDR_CRCCCITT16,
    }
}
