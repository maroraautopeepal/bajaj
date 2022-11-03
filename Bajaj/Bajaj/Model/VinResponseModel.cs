using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.Model
{
    public class VinResponseModel
    {
        public int error { get; set; }
        public string message { get; set; }
        public VinResponseData data { get; set; }
    }

    public class VinResponseData
    {
        public List<Controller> controllers { get; set; }
    }

    public class Controller
    {
        public List<Hexfile> hexfiles { get; set; }
        public string name { get; set; }
        public string part_no { get; set; }
        public string calibration_version { get; set; }
        public string uds_request_id { get; set; }
        public string uds_response_id { get; set; }
        public object balnet_base_id { get; set; }
        public object balnet_checkbyte { get; set; }
        public object balnet_program_constat { get; set; }
        public object uds_ext_session { get; set; }
        public string calibration_hexfile { get; set; }
        public string uds_did_list { get; set; }
        public string uds_fault_list { get; set; }
        public Diagnostics_Trouble_Codes diagnostic_Trouble_Codes { get; set; }
    }

    public class Hexfile
    {
        public object vendor_code { get; set; }
        public string hex_file { get; set; }
        public object software_version { get; set; }
        public int software_revision { get; set; }
        public string app_hex { get; set; }
    }
}
