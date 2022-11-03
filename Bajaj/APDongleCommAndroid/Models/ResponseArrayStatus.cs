using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APDongleCommAnroid.Models
{
    public class ResponseArrayStatus
    {
        public string ECUResponseStatus { get; set; }
        public byte[] ECUResponse { get; set; }
        public byte[] ActualDataBytes { get; set; }
    }

    public class IvnResponseArrayStatus
    {
        public string Frame { get; set; }
        public string ECUResponseStatus { get; set; }
        public byte[] ECUResponse { get; set; }
        public byte[] ActualDataBytes { get; set; }
    }

    public class ResponseArrayStatusivn
    {
        public string ECUResponseStatus { get; set; }
        public byte[] ActualFrameBytes { get; set; }
    }


}
