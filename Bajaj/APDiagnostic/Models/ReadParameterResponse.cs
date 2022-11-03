namespace APDiagnostic.Models
{
    public class ReadParameterResponse
    {
        public string Status { get; set; }
        public byte[] DataArray { get; set; }
        public int pidNumber { get; set; }
        public string pidName { get; set; }
        public string responseValue { get; set; }
    }

    public class ReadParameterResponseIvn
    {
        public string Status { get; set; }
        public string pidName { get; set; }
        public string responseValue { get; set; }
        public string Unit { get; set; }
    }
}
