using Bajaj.ViewModel;
using System.Collections.Generic;

namespace Bajaj.Model
{
    public class AllModelsModel
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<ModelResult> results { get; set; }
    }

    public class FlashInfo
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<Ecu2> results { get; set; }
    }

    public class Dataset
    {
        public int id { get; set; }
        public string code { get; set; }
    }

    public class PidDataset
    {
        public int id { get; set; }
        public string code { get; set; }
    }


    public class IvnDtcDataset
    {
        public int id { get; set; }
        public string code { get; set; }
    }

    public class IvnPidDataset
    {
        public int id { get; set; }
        public string code { get; set; }
    }

    public class Protocol
    {
        public string name { get; set; }
        public string elm { get; set; }
        public string autopeepal { get; set; }
    }

    public class ReadDtcFnIndex
    {
        public string value { get; set; }
    }

    public class ClearDtcFnIndex
    {
        public string value { get; set; }
    }

    public class ReadDataFnIndex
    {
        public string value { get; set; }
    }

    public class WriteDataFnIndex
    {
        public string value { get; set; }
    }

    public class SeedkeyalgoFnIndex
    {
        public string value { get; set; }
    }

    public class IORTestFnIndexModel
    {
        public string value { get; set; }
    }

    public class File
    {
        public int id { get; set; }
        public string data_file_name { get; set; }
        public string data_file { get; 
            set; }
    }

    public class EcuMapFile
    {
        public int id { get; set; }
        public string end_address { get; set; }
        public string sector_name { get; set; }
        public string start_address { get; set; }
        public int priority { get; set; }
    }



    public class Ecu2
    {
        public int id { get; set; }
        public int ecu { get; set; }
        public string tx_header { get; set; }
        public string rx_header { get; set; }
        public Protocol protocol { get; set; }
        public string sequence_file_name { get; set; }
        //public string flashsep_time { get; set; }
        //public string flash_address_data_format { get; set; }
        //public string flash_check_sum_type { get; set; }
        //public string flash_diagnostic_mode { get; set; }
        //public string flash_erase_type { get; set; }
        //public string flash_frase_byte { get; set; }
        //public string flash_nax_blkseqcntr { get; set; }
        public string flash_seed_key_length { get; set; }
        public string flash_status { get; set; }
        public string sequence_file { get; set; }
        public List<File> file { get; set; }

        public string sectorframetransferlen { get; set; }
        //public string sendseedbyte { get; set; }
        public List<EcuMapFile> ecu_map_file { get; set; }
    }

    public class Ecu : BaseViewModel
    {
        public int id { get; set; }
        
        private string _name;
        public string name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("name");
            }
        }
        public string tx_header { get; set; }
        public string rx_header { get; set; }
        public List<Dataset> datasets { get; set; }
        public List<DtcDataset> dtc_datasets { get; set; }
        public List<PidDataset> pid_datasets { get; set; }
        public List<PidPidDataset> pid_pid_datasets { get; set; }
        public List<IvnDtcDataset> ivn_dtc_datasets { get; set; }
        public List<IvnPidDataset> ivn_pid_datasets { get; set; }
        public Protocol protocol { get; set; }
        public ReadDtcFnIndex read_dtc_fn_index { get; set; }
        public ClearDtcFnIndex clear_dtc_fn_index { get; set; }
        public ReadDataFnIndex read_data_fn_index { get; set; }
        public WriteDataFnIndex write_data_fn_index { get; set; }
        public SeedkeyalgoFnIndex seedkeyalgo_fn_index { get; set; }
        public IORTestFnIndexModel ior_test_fn_index { get; set; }
        public List<Ecu2> ecu { get; set; }
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

    }

    public class DtcDataset
    {
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
    }

    public class PidPidDataset
    {
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
    }

    public class SubModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string model_year { get; set; }
        public List<Ecu> ecus { get; set; }
        public ConnectorType connector_type { get; set; }
        public SubModelFile sub_model_file { get; set; }
        public List<EcuSubmodel> ecu_submodel { get; set; }
        public bool is_active { get; set; }
    }

    public class SubModelFile
    {
        public string media_type { get; set; }
        public string attachment { get; set; }
        public string name { get; set; }
        public bool status { get; set; }
    }

    public class EcuSubmodel
    {
        public int id { get; set; }
        public int ecu { get; set; }
        public List<Dataset> datasets { get; set; }
        public List<PidDataset> pid_datasets { get; set; }
        public List<object> ivn_dtc_datasets { get; set; }
        public List<object> ivn_pid_datasets { get; set; }
    }

    public class ConnectorType
    {
        public string media_type { get; set; }
        public string attachment { get; set; }
        public string name { get; set; }
        public bool status { get; set; }
    }

    public class ModelResult :BaseViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        //public string model_name { get; set; }

        private string _model_name;
        public string model_name
        {
            get => _model_name;
            set
            {
                _model_name = value;
                OnPropertyChanged("model_name");
            }
        }
        public List<SubModel> sub_models { get; set; }
    }

    public class FlashEcusModel : BaseViewModel
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
        public List<File> flash_file_list { get; set; }
        public List<EcuMapFile> ecu_map_file { get; set; }
        public SeedkeyalgoFnIndex SeedkeyalgoFnIndex_Values { get; set; }
        
        public Ecu2 ecu2 { get; set; }
        //public List<Ecu> ecus { get; set; }
        public string seqFileUrl { get; set; }
    }

    public class UnlockModel : BaseViewModel
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

        public string tx_header { get; set; }
        public string rx_header { get; set; }

        public Protocol protocol { get; set; }

        public Ecu2 ecu2 { get; set; }
        //public List<Ecu> ecus { get; set; }
    }
}
