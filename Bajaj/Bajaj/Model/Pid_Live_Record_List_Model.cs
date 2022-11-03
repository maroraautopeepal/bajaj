using System.Collections.Generic;

namespace Bajaj.Model
{
    public class YAxisPointName
    {
        public string pid_name { get; set; }
        public List<string> value { get; set; }
    }

    public class PidLive
    {
        public List<string> x_axis_point { get; set; }
        public List<YAxisPointName> y_axis_point_name { get; set; }
    }

    public class PIDLiveRecord
    {
        public List<PidLive> pid_live { get; set; }
    }

}
