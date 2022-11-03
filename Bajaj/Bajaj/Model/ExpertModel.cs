using System.Collections.Generic;

namespace Bajaj.Model
{
    public class ExpertModel
    {
        public int id { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string status { get; set; }
        public string workshop { get; set; }
        public string workshop_city { get; set; }
        public string remote_session_id { get; set; }
        public string remote_session_internal_id { get; set; }
        public object auto_session_internal_id { get; set; }
        public object auto_session_id { get; set; }
        public string status_color { get; set; }
        public bool btnIsActive { get; set; }
    }

    public class OnlineExpertModel
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<ExpertModel> results { get; set; }
    }

    public class DTCMaskRoot
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<DTCMaskResult> results { get; set; }
    }

    public class DTCMaskResult
    {
        public int id { get; set; }
        public string mask_name { get; set; }
        public string mask_description { get; set; }
        public string is_active { get; set; }
        public List<DTCMaskMaskId> mask_ids { get; set; }
    }

    public class DTCMaskMaskId
    {
        public int vehicle_model { get; set; }
        public int sub_model { get; set; }
        public int model_year { get; set; }
        public List<string> dtc_code { get; set; }
    }
}
