using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.Model
{
    public class FirmwareUpdateModel
    {
        public string type { get; set; }
    }

    public class FirmwareUpdateResponseModel
    {
        public string error { get; set; }
        public bool success { get; set; }
        public FirmwareData data { get; set; }
    }

    public class FirmwareData
    {
        public int id { get; set; }
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
        public string device_type { get; set; }
        public string fotax_version { get; set; }
        public string fotax_firmware { get; set; }
        public string features { get; set; }
        public string bug_fixes { get; set; }
        public bool is_active { get; set; }
        public bool is_latest { get; set; }
    }
}
