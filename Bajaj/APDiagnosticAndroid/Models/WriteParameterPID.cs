using APDiagnosticAndroid.Enums;

namespace APDiagnosticAndroid.Models
{
    public class WriteParameterPID
    {
        public WriteParameterIndex writepamindex { get; set; }
        public SEEDKEYINDEXTYPE seedkeyindex { get; set; }
        public string writeparapid { get; set; }
        public int writeparadatasize { get; set; }

        public int writeparano { get; set; }

        public int writeparaName { get; set; }
        public byte[] writeparadata { get; set; }
        public string ReadParameterPID_DataType { get; set; }
        public string pid { get; set; }
        public int totalLen { get; set; }
        public int totalBytes { get; set; }
        public int startByte { get; set; }
        public int noOfBytes { get; set; }
        public bool IsBitcoded { get; set; }
        public int startBit { get; set; }
        public int noofBits { get; set; }

        public string datatype { get; set; }
        public double? resolution { get; set; }
        public double? offset { get; set; }
        public string unit { get; set; }
        public string pidName { get; set; }


        //    public byte[] dataByteArray { get; set; }
    }
}
