using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bajaj.Model
{
    public class DongleModel
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public List<DongleResult> Results { get; set; } = new List<DongleResult>();
    }

    public partial class DongleResult
    {
        [JsonProperty("mac_id")]
        public string MacId { get; set; }

        [JsonProperty("parent")]
        public object Parent { get; set; }

        [JsonProperty("oem")]
        public Oem Oem { get; set; } = new Oem();

        [JsonProperty("user")]
        public List<DongleUser> User { get; set; } = new List<DongleUser>();

        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
    }

    public partial class Oem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("admin")]
        public string Admin { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
    }

    public partial class DongleUser
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("workshop")]
        public string Workshop { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }

    }
}
