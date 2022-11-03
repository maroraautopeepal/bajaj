using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.Model
{
    public class EcuModelNew
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<EcuModelResult> results { get; set; }
    }

    public class EcuModelResult
    {
        public int id { get; set; }
        public string name { get; set; }
        public string tx_header { get; set; }
        public string rx_header { get; set; }
        public Protocol protocol { get; set; }
        public ReadDtcFnIndex read_dtc_fn_index { get; set; }
        public ClearDtcFnIndex clear_dtc_fn_index { get; set; }
        public ReadDataFnIndex read_data_fn_index { get; set; }
        public WriteDataFnIndex write_data_fn_index { get; set; }
        public SeedkeyalgoFnIndex seedkeyalgo_fn_index { get; set; }
        public IorTestFnIndex ior_test_fn_index { get; set; }
    }

    public class IorTestFnIndex
    {
        public string value { get; set; }
    }
}
