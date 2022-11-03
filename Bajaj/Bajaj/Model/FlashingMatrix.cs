using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.Model
{
    public class FlashingMatrix
    {
        public string JsonStartAddress { get; set; }
        public string JsonEndAddress { get; set; }
        public string ECUMemMapStartAddress { get; set; }
        public string ECUMemMapEndAddress { get; set; }

        public string JsonCheckSum { get; set; }
        public string JsonData { get; set; }

    }
    public class FlashingMatrixData
    {
        public int NoOfSectors { get; set; }
        public List<FlashingMatrix> SectorData { get; set; }

    }
    public class SREC_FlashingMatrixData
    {
        public int NoOfSectors { get; set; }
        public string file_start_addr { get; set; }
        public List<FlashingMatrix> SectorData { get; set; }
    }
}
