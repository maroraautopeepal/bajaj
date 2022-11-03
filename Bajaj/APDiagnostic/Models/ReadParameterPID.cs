using APDiagnostic.Enums;
using System.Collections.Generic;

namespace APDiagnostic.Models
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
        public string pid { get; set; }
        public string code { get; set; }
        public string message { get; set; }
    }


    #region Old Code
    //public class ReadParameterPID
    //{
    //    public string pid { get; set; }
    //    public int totalLen { get; set; }
    //    //public int totalBytes { get; set; }
    //    public int startByte { get; set; }
    //    public int noOfBytes { get; set; }
    //    public bool IsBitcoded { get; set; }
    //    public int startBit { get; set; }
    //    public int noofBits { get; set; }
    //    public string datatype { get; set; }
    //    public double? resolution { get; set; }
    //    public double? offset { get; set; }
    //    public int pidNumber { get; set; }
    //    public string pidName { get; set; }

    //    public ReadParameterIndex readParameterIndex { get; set; }

    //    public List<SelectedParameterMessage> messages { get; set; }
    //}
    //public class SelectedParameterMessage
    //{
    //    public string code { get; set; }
    //    public string message { get; set; }
    //}
    #endregion
}
