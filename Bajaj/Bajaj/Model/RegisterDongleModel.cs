using Newtonsoft.Json;

namespace Bajaj.Model
{
    public class RegisterDongleModel
    {
        [JsonProperty("mac_id")]
        public object MacId { get; set; }
        public string device_type { get; set; }

        //[JsonProperty("device_type")]
        //public object DeviceType { get; set; }
    }

    public class RegisterDongleRespons
    {
        //public ErrorRes errorRes { get; set; }
        //public UserResModel userRes { get; set; }

        public RegDongleRespons errorRes { get; set; }
    }

    public class ErrorRes
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
    }
    public class RegDongleRespons
    {
        public string oem { get; set; }
        public string device_type { get; set; }
        public string message { get; set; }
        public string mac_id { get; set; }
        public string user { get; set; }
        public bool is_active { get; set; }
    }
}
