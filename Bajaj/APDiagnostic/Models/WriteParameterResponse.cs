﻿namespace APDiagnostic.Models
{
    public class WriteParameterResponse
    {
        public string Status { get; set; }
        public byte[] DataArray { get; set; }
        public int pidNumber { get; set; }
        public string pidName { get; set; }
        public string responseValue { get; set; }
    }
}
