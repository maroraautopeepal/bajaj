using APDiagnosticAndroid.Enums;
using System.Collections.Generic;

namespace APDiagnosticAndroid.Models
{
    public class ReadParameterPID
    {
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
        public int pidNumber { get; set; }
        public string pidName { get; set; }

        public ReadParameterIndex readParameterIndex { get; set; }

        public List<SelectedParameterMessage> messages { get; set; }
    }

    public class SelectedParameterMessage
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class PIDFrameId
    {
        public string FramID { get; set; }
        public string pid_description { get; set; }
        public string start_byte { get; set; }
        public string @byte { get; set; }
        public string bit_coded { get; set; }
        public string start_bit { get; set; }
        public string no_of_bits { get; set; }
        public string resolution { get; set; }
        public string offset { get; set; }
        public string unit { get; set; }
        public object message_type { get; set; }
        public List<FrameOfPidMessage> frame_of_pid_message { get; set; }
        public string endian { get; set; }
        public string num_type { get; set; }
    }

    public class FrameOfPidMessage
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class IVN_SelectedPID
    {
        public string frame_id { get; set; }
        public List<PIDFrameId> frame_ids { get; set; }
    }
}
