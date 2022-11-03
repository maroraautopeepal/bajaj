using System;

namespace APDiagnostic.Structures
{
    struct sectordata
    {
        public UInt32 startaddress;
        public UInt32 noofbytes;
        public byte[] bytearray;
        public UInt16 sectorchecksum;
    }

}
