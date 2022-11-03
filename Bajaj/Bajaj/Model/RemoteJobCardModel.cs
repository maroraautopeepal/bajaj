using System.Collections.Generic;

namespace Bajaj.Model
{
    public class RemoteJobCardModel
    {
        public string job_card_session { get; set; }
        public string remote_session_status { get; set; }
        public string status { get; set; }
        public bool request_status { get; set; }
        public int expert_user { get; set; }
    }

    public class RemoteRequestDiagnostic
    {
        public string ForExpertSessionID { get; set; }
        public string IDForRemote { get; set; }
        public string status { get; set; }
        public int expert_user { get; set; }
        public bool request_status { get; set; }
    }
    public class RemoteRequestAccept_OR_Decline
    {
        public string status { get; set; }
        public int expert_user { get; set; }
        public bool request_status { get; set; }
    }
    public class NonFieldError
    {
        public string remote_session_id { get; set; }
        public string error { get; set; }
    }

    public class BadRequestResponseModel
    {
        public List<NonFieldError> non_field_errors { get; set; } = new List<NonFieldError>();
    }

    public class MainResponseModel
    {
        public BadRequestResponseModel badRequestResponseModel { get; set; } = new BadRequestResponseModel();
        public ResponseJobCardModel NewRequestResponseModel { get; set; }
        public string status { get; set; }
    }

    public class ResponseJobCardModel
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

    public class ResponseRoot
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<ResponseJobCardModel> results { get; set; }
    }
}
