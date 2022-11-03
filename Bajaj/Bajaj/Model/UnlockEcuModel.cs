using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.Model
{
    public class UnlockEcuModel
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<ResultUnlock> results { get; set; }
    }

    public class EcuUnlock
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class ResultUnlock
    {
        public int id { get; set; }
        public int? oem { get; set; }
        public int model { get; set; }
        public int submodel { get; set; }
        public int year { get; set; }
        public EcuUnlock ecu { get; set; }
        public Protocol protocol { get; set; }
        public string tx_id { get; set; }
        public string tx_frame { get; set; }
        public string tx_frequency { get; set; }
        public string tx_total_time { get; set; }
        public string rx_id { get; set; }
        public string rx_frame { get; set; }
        public string is_active { get; set; }
    }
}
