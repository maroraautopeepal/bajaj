namespace APDongleCommWin.Models
{
    public class ResponseArrayStatus
    {
        public string ECUResponseStatus { get; set; }
        public byte[] ECUResponse { get; set; }
        public byte[] ActualDataBytes { get; set; }
    }
}
