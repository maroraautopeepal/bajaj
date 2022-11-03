using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.Model
{
    public class PidCodeNew
    {
        public int id { get; set; }
        public string code { get; set; }
        public int total_len { get; set; }
        public bool read { get; set; }
        public bool write { get; set; }
        public string write_pid { get; set; }
        public bool reset { get; set; }
        public string reset_value { get; set; }
        public bool io_ctrl { get; set; }
        public string io_ctrl_pid { get; set; }
        public List<PiCodeVariable> pi_code_variable { get; set; }
    }

    public class PiCodeVariable : BaseViewModel
    {
        public int id { get; set; }
        public string short_name { get; set; }
        public string long_name { get; set; }
        public int byte_position { get; set; }
        public int length { get; set; }
        public bool bitcoded { get; set; }
        public int? start_bit_position { get; set; }
        public int? end_bit_position { get; set; }
        public double? resolution { get; set; }
        public double? offset { get; set; }
        public double? min { get; set; }
        public double? max { get; set; }
        public string message_type { get; set; }
        public string unit { get; set; }
        public string endian { get; set; }
        public string num_type { get; set; }
        public List<Message> messages { get; set; }
        private bool selected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged("Selected");
            }
        }

        private string _show_resolution;
        public string show_resolution
        {
            get => _show_resolution;
            set
            {
                _show_resolution = value;
                OnPropertyChanged("show_resolution");
            }
        }
    }
}
