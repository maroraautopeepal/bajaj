using System.Collections.Generic;

namespace APDiagnosticAndroid.Models
{
    public class FlashingMatrix
    {
        public string JsonStartAddress { get; set; }
        public string JsonEndAddress { get; set; }
        public string JsonData { get; set; }

        public string ECUMemMapStartAddress { get; set; }
        public string ECUMemMapEndAddress { get; set; }

        public string JsonCheckSum { get; set; }

    }
    public class FlashingMatrixData
    {
        public int NoOfSectors { get; set; }
        public List<FlashingMatrix> SectorData { get; set; }

    }
}
