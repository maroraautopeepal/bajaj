using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Bajaj.Model
{
    public class DtcModel
    {
        public string ecu_name { get; set; }
        public List<DtcListModel> dtc_list { get; set; }
    }

    public class IVN_DtcModel
    {
        public string frame_name { get; set; }
        public string frame_id { get; set; }
        public List<FrameId> dtc_list { get; set; }
    }

    public class IVNdtc_AND_dtc
    {
        public List<IVN_Result> IDCT_R { get; set; } = new List<IVN_Result>();
        public List<DtcResults> DTC_R { get; set; } = new List<DtcResults>();
    }

    public class DtcCode
    {
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }

        public string is_active { get; set; }
        public string status_activation { get; set; }
        public string lamp_activation { get; set; }
        public Color status_activation_color { get; set; }
        public Color lamp_activation_color { get; set; }
    }

    public class DtcListModel
    {
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public string status_color { get; set; }
    }

    public class DtcResults
    {
        public int id { get; set; }
        public string code { get; set; }
        public object description { get; set; }
        public string is_active { get; set; }
        public List<DtcCode> dtc_code { get; set; }
        public List<DtcListModel> dtc_code1 { get; set; }
    }

    public class DtcMainModel
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<DtcResults> results { get; set; }
    }

    

    public class DtcEcusModel : BaseViewModel
    {
        public string ecu_name { get; set; }

        private double _opacity;
        public double opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                OnPropertyChanged("opacity");
            }
        }
        public List<DtcCode> dtc_list { get; set; }
        public string emptyViewText { get; set; }
    }
    public class ReadDtcResponseModel
    {
        public string status { get; set; }
        public string[,] dtcs { get; set; }
        public UInt16 noofdtc { get; set; }
    }

    public class ClearDtcResponseModel
    {
        public string ECUResponseStatus { get; set; }
        public string ECUResponse { get; set; }
        public string ActualDataBytes { get; set; }
    }

    public class IvnReadDtcResponseModel
    {
        public int ecu_id { get; set; }
        public string Frame { get; set; }
        public string ECUResponseStatus { get; set; }
        public string ECUResponse { get; set; }
        public byte[] ActualDataBytes { get; set; }
    }

    public class IVN_Result
    {
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public List<FrameDataset> frame_datasets { get; set; }
    }

    public class IvnDtc
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<IVN_Result> results { get; set; }
    }

    public class FrameDataset
    {
        public int id { get; set; }
        public string frame_name { get; set; }
        public string frame_id { get; set; }
        public object frame_description { get; set; }
        public List<FrameId> frame_ids { get; set; }
        public List<FrameEnum> frame_enum { get; set; }
        public List<FrameStatu> frame_status { get; set; }
    }

    public class FrameEnum
    {
        public int id { get; set; }
        public string digit { get; set; }
        public string @enum { get; set; }
    }

    public class FrameStatu
    {
        public int id { get; set; }
        public string digit { get; set; }
        public string @enum { get; set; }
    }

    public class FrameId
    {
        public int id { get; set; }
        public int frame_name { get; set; }
        public string dtc_code { get; set; }
        public string dtc_description { get; set; }
        public string @byte { get; set; }
        public string bit { get; set; }
        public string no_of_bits { get; set; }
        public string status_byte { get; set; }
        public string status_bit { get; set; }
        public string status_no_of_bits { get; set; }
    }
}
