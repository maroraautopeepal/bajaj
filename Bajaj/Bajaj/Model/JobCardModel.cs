using Bajaj.ViewModel;
using System;
using System.Collections.Generic;

namespace Bajaj.Model
{
    public class JobCardModel
    {
        public string id { get; set; }
        public string job_card_name { get; set; }
        public string status { get; set; }
        public string fert_code { get; set; }
        public string vehicle_segment { get; set; }
        public VehicleModel vehicle_model { get; set; }
        public string chasis_id { get; set; }
        public string registration_no { get; set; }
        public int km_covered { get; set; }
        public string complaints { get; set; }
        public CreatedBy created_by { get; set; }
        public DateTime? created { get; set; }
        public DateTime? modified { get; set; }
        public int job_card_age { get; set; }
        public List<JobCardSession> job_card_session { get; set; }
    }

    public class JobCardSession
    {
        public string id { get; set; }
        public string session_id { get; set; }
        public string job_card { get; set; }
        public DateTime? start_date { get; set; }
        public string show_start_date { get; set; }
        public DateTime? end_date { get; set; }
        public object device_dongle { get; set; }
        public VehicleModel vehicle_model { get; set; }
        public object device { get; set; }
        public User user { get; set; }
        public string source { get; set; }
        public string session_type { get; set; }
        public string status { get; set; }
        public JobCardRemoteSession job_card_remote_session { get; set; }
        public object automated_session { get; set; }
    }

    public class User
    {
        public string email { get; set; }
        public object workshop { get; set; }
        public object role { get; set; }
    }
    public class JobCardRemoteSession
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

    public class Parent
    {
        public string name { get; set; }
        public object parent { get; set; }
        public object model_year { get; set; }
    }

    public class VehicleModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public Parent parent { get; set; }
        public string model_year { get; set; }
        public List<SubModelClass> sub_models { get; set; }
    }


    public class CreatedBy
    {
        public string email { get; set; }
        public UsUser us_user { get; set; }
    }

    public class Workshop
    {
        public string name { get; set; }
        public int oem { get; set; }
        public string address { get; set; }
        public string pincode { get; set; }
        public bool is_active { get; set; }
        public int id { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string state { get; set; }
    }

    public class UsUser
    {
        public int oem { get; set; }
        public string role { get; set; }
        public Workshop workshop { get; set; }
        public bool status { get; set; }
        public List<object> run_time_licenses { get; set; }
    }


    public class SubModelClass
    {
        public int id { get; set; }
        public string model_year { get; set; }
        public string name { get; set; }
    }

    public class SendJobcardData
    {
        public string status { get; set; }
        public string vehicle_segment { get; set; }
        public string source { get; set; }
        public string chasis_id { get; set; }
        public string session_type { get; set; }
        public string registration_no { get; set; }
        public string vehicle_model_id { get; set; }
        public string submodel { get; set; }
        public string date { get; set; }
        public string fert_code { get; set; }
        public string device_mac_id { get; set; }
        public string complaints { get; set; }
        public string km_covered { get; set; }
        public string VehModDes { get; set; }
        public string job_card_name { get; set; }
        public string model { get; set; }
        public string job_card_status { get; set; }
        public string engine_no { get; set; }
    }

    public class LocalJobcardData
    {
        public string session_id { get; set; }
        public DateTime? start_date { get; set; }
        public string status { get; set; }
        public string job_card { get; set; }
        public DateTime? end_date { get; set; }
        public DateTime? modified { get; set; }
        public string session_type { get; set; }
        public string id { get; set; }
        public string vehicle_model { get; set; }
        public int model_id { get; set; }
        public string sub_model { get; set; }
        public int sub_model_id { get; set; }
        public string model_year { get; set; }
        public string chasis_id { get; set; }
        public string complaints { get; set; }
        public string fert_code { get; set; }
        public int km_covered { get; set; }
        public string registration_no { get; set; }
        public string vehicle_segment { get; set; }
        public string jobcard_status { get; set; }
        public string show_start_date { get; set; }
        public string source { get; set; }
    }

    public class PostJobCardSession
    {
        public string status { get; set; }
        public string job_card { get; set; }
        public string source { get; set; }
        public string vehicle_model_id { get; set; }
        public string session_type { get; set; }
        public string device_mac_id { get; set; }
        public string job_card_name { get; set; }
    }

    public class ResCreateSessionModel
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<Result> results { get; set; }
    }

    public class Result
    {
        public string id { get; set; }
        public string session_id { get; set; }
        public string job_card { get; set; }
        public string source { get; set; }
        public VehicleModel vehicle_model { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public object device_dongle { get; set; }
        public int? device { get; set; }
        public int dtc_record_count { get; set; }
        public int clear_record_count { get; set; }
        public int pid_snapshot_record_count { get; set; }
        public int pid_live_record_count { get; set; }
        public int pid_write_record_count { get; set; }
        public int flash_record_count { get; set; }
        public User user { get; set; }
        public string session_type { get; set; }
        public string status { get; set; }
        public JobCardRemoteSession job_card_remote_session { get; set; }
        public object automated_session { get; set; }
    }

    public class ExistJobCard
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<ExistJobCardResult> results { get; set; }
    }

    public class ExistJobCardResult
    {
        public string id { get; set; }
        public string job_card_name { get; set; }
        public string status { get; set; }
        public string fert_code { get; set; }
        public string vehicle_segment { get; set; }
        public VehicleModel vehicle_model { get; set; }
        public int vehicle_model_id { get; set; }
        public string chasis_id { get; set; }
        public string registration_no { get; set; }
        public int km_covered { get; set; }
        public string complaints { get; set; }
        public CreatedBy created_by { get; set; }
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
    }

    public class JobCatrdResModel
    {
        public string last_name { get; set; }
        public string token { get; set; }
        public List<object> vehicle_models { get; set; }
        public string first_name { get; set; }
        public string role { get; set; }
        public string user { get; set; }
        public DateTime expires { get; set; }
        public Licences licences { get; set; }
    }


    public class SameJobcard
    {
        public List<string> job_card_name { get; set; }
    }

    public class MainResultClass
    {
        public SameJobcard SameJobcard { get; set; }
        public JobCardModel CreateJobcard { get; set; }
    }

    public class CloseSession
    {
        public string status { get; set; }
    }

    public class ResCloseSession
    {
        public string id { get; set; }
        public string source { get; set; }
        public string session_id { get; set; }
        public string job_card { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public List<User> user { get; set; }
        public string session_type { get; set; }
        public string status { get; set; }
    }

    public class JobCardListModel : BaseViewModel
    {
        public string jobcard_name { get; set; }
        public string session_id { get; set; }
        public DateTime? start_date { get; set; }
        public string status { get; set; }
        public string job_card { get; set; }
        public DateTime? end_date { get; set; }
        public DateTime? modified { get; set; }
        public string session_type { get; set; }
        public string id { get; set; }
        public string vehicle_model { get; set; }
        public int model_id { get; set; }
        public string sub_model { get; set; }
        public int sub_model_id { get; set; }
        public string model_year { get; set; }
        public string chasis_id { get; set; }
        public string complaints { get; set; }
        public string fert_code { get; set; }
        public int km_covered { get; set; }
        public string registration_no { get; set; }
        public string vehicle_segment { get; set; }
        public string jobcard_status { get; set; }
        public string show_start_date { get; set; }
        public string source { get; set; }
        public string workshop { get; set; }
        public string city{ get; set; }
        public string state { get; set; }
        private string _model_with_submodel;
        public string model_with_submodel
        {
            get => _model_with_submodel;
            set
            {
                _model_with_submodel = value;
                OnPropertyChanged("model_with_submodel");
            }
        }

    }
    public class PostDtcRecord
    {
        public string status { get; set; }
        public string value { get; set; }
    }
    public class DtcR
    {
        public List<PostDtcRecord> dtc { get; set; }
    }
    public class ClearDtcRecord
    {
        public string session { get; set; }
        public string status { get; set; }
    }
    public class pid_write_record
    {
        public string pid_code { get; set; }
        public string value_before { get; set; }
        public string value_after { get; set; }
        public string status { get; set; }
    }
    public class PidWriteRecord
    {
        public List<pid_write_record> pid_write_records { get; set; }
    }

    public class flash_record
    {
        public string flash_duration { get; set; }
        public string status { get; set; }
        public string cvn_before_flash { get; set; }
        public string cvn_after_flash { get; set; }
    }
    public class pid_live_record
    {
        public string pid_live { get; set; }
    }
    public class pid_snapshot_record
    {
        public List<snapshot_record> pid_snapshot { get; set; }
    }
    public class snapshot_record
    {
        public string code { get; set; }
        public string value { get; set; }
    }
    public class AutoNewJobCard
    {
        public bool success { get; set; }
        public string name { get; set; }
    }

    public class JobcardNumber
    {
        public string name { get; set; }
        public bool success { get; set; }
        public string error { get; set; }

    }
}
