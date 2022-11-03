using Bajaj.Model;
using System.Collections.Generic;

namespace Bajaj
{
    public static class StaticData
    {

        public static List<EcuDataSet> ecu_info = new List<EcuDataSet>();
    }

    public class EcuDataSet
    {
        //public string ecu_name { get; set; }//public static int pid_dataset_id = 0;
        //public int dtc_dataset_id { get; set; }
        //public int pid_dataset_id { get; set; }
        //public string clear_dtc_index { get; set; }
        //public string read_dtc_index { get; set; }
        //public string write_pid_index { get; set; }
        //public string seed_key_index { get; set; }

        public int ecu_ID { get; set; }
        public string ecu_name { get; set; }//public static int pid_dataset_id = 0;        
        public string chasis_id { get; set; }
        public int dtc_dataset_id { get; set; }
        public int pid_dataset_id { get; set; }
        public int ivn_pid_dataset_id { get; set; }
        public int ivn_dtc_dataset_id { get; set; }
        public string clear_dtc_index { get; set; }
        public string read_dtc_index { get; set; }
        public string write_pid_index { get; set; }
        public string ior_test_fn_index { get; set; }
        public SeedkeyalgoFnIndex seed_key_index { get; set; }
        public string tx_header { get; set; }
        public string rx_header { get; set; }
        public Protocol protocol { get; set; }
    }
    public static class SessionStaticData
    {

        public static List<SessionResponseJobCardModel> SessionResponse = new List<SessionResponseJobCardModel>();
    }
    public class SessionResponseJobCardModel
    {
        public string id { get; set; }
        public string remote_session_id { get; set; }
        public string job_card_session { get; set; }
        public object case_id { get; set; }
        public string status { get; set; }
        public int expert_user { get; set; }
        public object expert_device { get; set; }
        public string expert_email { get; set; }
        public bool request_status { get; set; }
    }


    //public static class StaticData
    //{

    //    public static List<EcuDataSet> ecu_info = new List<EcuDataSet>();
    //}

    //public class EcuDataSet
    //{
    //    public string ecu_name { get; set; }//public static int pid_dataset_id = 0;
    //    public int dtc_dataset_id { get; set; }
    //    public int pid_dataset_id { get; set; }
    //    public string clear_dtc_index { get; set; }
    //    public string read_dtc_index { get; set; }
    //    public string write_pid_index { get; set; }
    //    public string seed_key_index { get; set; }
    //}

    //public static class SessionStaticData
    //{

    //    public static List<SessionResponseJobCardModel> SessionResponse = new List<SessionResponseJobCardModel>();
    //}
    //public class SessionResponseJobCardModel
    //{
    //    public string id { get; set; }
    //    public string remote_session_id { get; set; }
    //    public string job_card_session { get; set; }
    //    public object case_id { get; set; }
    //    public string status { get; set; }
    //    public int expert_user { get; set; }
    //    public object expert_device { get; set; }
    //    public string expert_email { get; set; }
    //    public bool request_status { get; set; }
    //}
}
