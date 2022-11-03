using System;
using System.Collections.Generic;
using System.Text;

namespace AutoELM327.Models
{
    public class ResponseArrayStatus
    {
        public string ECUResponseStatus { get; set; }
        public byte[] ECUResponse { get; set; }
        public byte[] ActualDataBytes { get; set; }
    }
}
