using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.Model
{
    public class WhoAmIModel
    {
        public int error { get; set; }
        public string message { get; set; }
        public string token { get; set; }
        public string expiry { get; set; }
    }

    public class DealerLoginModel
    {
        public int error { get; set; }
        public string message { get; set; }
    }

    public class ValidateLoginOtpModel
    {
        public string serial_number { get; set; }
        public string otp { get; set; }
    }

    public class ValidateVinOtpModel
    {
        public string otp { get; set; }
        public string vin { get; set; }
        public string serial_number { get; set; }
    }

    public class ValidateLoginOtpResp
    {
        public int error { get; set; }
        public string message { get; set; }
        public string token { get; set; }
    }

    public class VinOtpModel
    {
        public int error { get; set; }
        public string message { get; set; }
        public VinOtpModelData data { get; set; }
    }

    public class VinOtpModelData
    {
        public List<string> serial_number { get; set; }
        public List<string> vin { get; set; }
    }
}
