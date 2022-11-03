using Bajaj.ViewModel;
using System.Collections.Generic;

namespace Bajaj.Model
{
    public class LiveParameterSelectModel //: INotifyPropertyChanged
    {
        public string ecu_name { get; set; }
        public List<PidCode> roots { get; set; }

        //    //public event PropertyChangedEventHandler PropertyChanged;
        //    public string code { get; set; }
        //    public string long_name { get; set; }
        //    public string resolution { get; set; }
        //    public string unit { get; set; }
        //    //public bool IsChecked { get; set; }

        //    private bool selected = false;
        //    public bool Selected
        //    {
        //        get => selected;
        //        set
        //        {
        //            selected = value;
        //            NotifyPropertyChanged();
        //        }
        //    }


        //    public event PropertyChangedEventHandler PropertyChanged;

        //    protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        //    {
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //    }
    }

    public class Message
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class PidCode : BaseViewModel
    {
        public int id { get; set; }
        public string code { get; set; }
        public string short_name { get; set; }
        public string long_name { get; set; }
        public int total_len { get; set; }
        public int byte_position { get; set; }
        public int length { get; set; }
        public bool bitcoded { get; set; }
        public int? start_bit_position { get; set; }
        public int? end_bit_position { get; set; }
        public double? resolution { get; set; }
        public double? offset { get; set; }
        public double? min { get; set; }
        public double? max { get; set; }
        public bool read { get; set; }
        public bool write { get; set; }
        public string message_type { get; set; }
        public string unit { get; set; }
        public string write_pid { get; set; }
        public bool reset { get; set; }
        public string reset_value { get; set; }
        public List<Message> messages { get; set; }
        public bool io_ctrl { get; set; }
        public string io_ctrl_pid { get; set; }

        private bool selected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged("Selected");
            }
        }

        private string _show_resolution;
        public string show_resolution
        {
            get => _show_resolution;
            set
            {
                _show_resolution = value;
                OnPropertyChanged("show_resolution");
            }
        }
    }

    public class Results
    {
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public List<PidCodeNew> codes { get; set; }
    }

    public class PIDResult
    {
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public List<PIDFrameDataset> frame_datasets { get; set; }
    }

    public class PIDFrameDataset
    {
        public int id { get; set; }
        public string frame_name { get; set; }
        public string frame_id { get; set; }
        public List<PIDFrameId> frame_ids { get; set; }
    }

    public class Root
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<Results> results { get; set; }
    }

    public class ReadParameterPID : BaseViewModel
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

        private string _unit;
        public string unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged("unit");
            }
        }
        public int pidNumber { get; set; }

        private string _pidName;
        public string pidName
        {
            get => _pidName;
            set
            {
                _pidName = value;
                OnPropertyChanged("pidName");
            }
        }

        private string _show_resolution;
        public string show_resolution
        {
            get => _show_resolution;
            set
            {
                _show_resolution = value;
                OnPropertyChanged("show_resolution");
            }
        }

        //message
        public List<SelectedParameterMessage> messages { get; set; }
    }

    public class SelectedParameterMessage
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class PID
    {
        public List<Results> NormalPID { get; set; }
        public List<PIDResult> IVNPID { get; set; }
    }

    public class IVN_LiveParameterSelectModel
    {
        public string frame_name { get; set; }
        public string frame_id { get; set; }
        public List<PIDFrameId> frame_ids { get; set; }
    }
    public class IVN_LiveParameterFrame_name
    {
        public string frame_name { get; set; }
        public List<PIDFrameId> frame_idsAndPid { get; set; }
    }

    public class IVN_frame_idsAndPID
    {
        public string frame_id { get; set; }
        public List<PIDFrameId> frame_ids { get; set; }
    }

    public class WriteParameterPID
    {
        public string writepamindex { get; set; }
        public string seedkeyindex { get; set; }
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

    }

    public class ReadPidPresponseModel
    {
        public string Status { get; set; }
        public string DataArray { get; set; }
        public int pidNumber { get; set; }
        public string pidName { get; set; }
        public string responseValue { get; set; }
        public string Unit { get; set; }
    }

    public class TestRoutineResponseModel
    {
        public string ECUResponseStatus { get; set; }
        public string ECUResponse { get; set; }
        public byte[] ActualDataBytes { get; set; }
    }

    public class IVN_SelectedPID
    {
        public string frame_id { get; set; }
        public List<PIDFrameId> frame_ids { get; set; }
    }

    public class PIDModel
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<PIDResult> results { get; set; }
    }

    public class PIDFrameId : BaseViewModel
    {
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
        private bool selected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged("Selected");
            }
        }
        public string FramID { get; set; }
        public string endian { get; set; }
        public string num_type { get; set; }
    }

    public class FrameOfPidMessage
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    #region New Model
    public class EcusModel : BaseViewModel
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
        public List<PidCode> pid_list { get; set; }
        public string protocol { get; set; }
        public string txHeader { get; set; }
        public string rxHeader { get; set; }
    }
    #endregion

    public class IvnEcusModel : BaseViewModel
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
        public List<PIDFrameId> pid_list { get; set; }
    }
}
