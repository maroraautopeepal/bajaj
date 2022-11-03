using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Bajaj.Model
{
    public class UserModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string mac_id { get; set; }
        public string device_type { get; set; }
    }
    public class ChangePassword
    {
        public string new_password1 { get; set; }
        public string new_password2 { get; set; }
    }
    public class LoginRespons
    {
        //[JsonProperty("login")]
        public LoginRes LoginRes { get; set; }
        public UserResModel userRes { get; set; }
    }

    public class LoginRes
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
    }

    public class Licences
    {
        public bool live_parameter { get; set; }
        public bool iotest { get; set; }
        public bool rdtechnician { get; set; }
        public bool dtc_read_clear { get; set; }
        public bool flash { get; set; }
        public bool write_parameter { get; set; }
        public bool rdexpert { get; set; }
        public bool terminal { get; set; }
        public bool dms_job_card { get; set; }
        public bool guided_diagnostics { get; set; }
    }

    public class UserResModel
    {
        public int user_id { get; set; }
        public Licences licences { get; set; }
        public string last_name { get; set; }
        public string role { get; set; }
        public DateTime expires { get; set; }
        public Token token { get; set; }
        public List<VehicleModel> vehicle_models { get; set; }
        public string user { get; set; }
        public string first_name { get; set; }
        public Profile profile { get; set; }
    }

    public class Profile
    {
        public int id { get; set; }
        public Oem1 oem { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string role { get; set; }
        public bool status { get; set; }
        public object parent { get; set; }
        public int user { get; set; }
        public int workshop { get; set; }
        public object report_to { get; set; }
        public List<object> run_time_licenses { get; set; }
    }

    public class Oem1
    {
        public int id { get; set; }
        public OemFile oem_file { get; set; }
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public object created_by { get; set; }
        public bool is_active { get; set; }
        public string color { get; set; }
        public string app_name { get; set; }
        public int admin { get; set; }
        public object manager { get; set; }
    }

    public class OemFile
    {
        public string media_type { get; set; }
        public string attachment { get; set; }
        public string name { get; set; }
        public bool status { get; set; }
    }

    public class Token
    {
        public string refresh { get; set; }
        public string access { get; set; }
    }
}
