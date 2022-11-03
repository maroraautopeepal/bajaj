namespace Bajaj.Model
{
    public class WriteParameterModel
    {
        public string code { get; set; }
        public string unit { get; set; }
        public string new_value { get; set; }
        public string old_value { get; set; }
        public string description { get; set; }
    }
    public class WriteParameter_Status
    {
        public string Status { get; set; }
    }
}
