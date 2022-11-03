using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.Model
{
    public class IorTestModel
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public string error { get; set; }
        public List<IorResult> results { get; set; }
    }

    public class IorResult
    {
        public string id { get; set; }
        public string routine_name { get; set; }
        public string notice { get; set; }
        public string vehicle_model { get; set; }
        public string sub_model { get; set; }
        public string model_year { get; set; }
        public ecu_model ecu { get; set; }
        public string status { get; set; }
        public List<PreCondition> pre_conditions { get; set; }
        public List<IorTestRoutine> ior_test_routine { get; set; }
        public IorTestRoutineType ior_test_routine_type { get; set; }
        public List<TestMonitor> test_monitors { get; set; }
        public List<object> routine_result { get; set; }
    }

    public class ecu_model
    {
        public string name { get; set; }
        public int id { get; set; }
    }

    public class TestMonitor : BaseViewModel
    {
        public string routine_id { get; set; }
        public string id { get; set; }
        public string test_monitor_type { get; set; }
        public int pid { get; set; }
        public string description { get; set; }
        public string lower_limit { get; set; }
        public string upper_limit { get; set; }

        private string _current_value;
        public string current_value
        {
            get => _current_value;
            set
            {
                _current_value = value;
                OnPropertyChanged("current_value");
            }
        }

        private string _unit;
        public string unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged("unit");
            }
        }
    }

    public class IorTestRoutineType
    {
        public string id { get; set; }
        public string test_routine_type { get; set; }
        public bool send_tester_present { get; set; }
        public string activation_time { get; set; }
        public string status_byte_loc { get; set; }
        public List<StatusByteDefinition> status_byte_definition { get; set; }
    }

    public class StatusByteDefinition
    {
        public string routine_id { get; set; }
        public string id { get; set; }
        public string @byte { get; set; }
        public string byte_definations { get; set; }
    }

    public class IorTestRoutine : BaseViewModel
    {
        public string routine_id { get; set; }
        public string id { get; set; }
        public string start_routine_id { get; set; }
        public string req_routine_id { get; set; }
        public string stop_followup_routine_id { get; set; }
        public string description { get; set; }
        public bool is_play { get; set; }

        private string _test_status = "Stop";
        public string test_status
        {
            get => _test_status;
            set
            {
                _test_status = value;
                OnPropertyChanged("test_status");
            }
        }

        private string _image_source = "ic_play.png";
        public string image_source
        {
            get => _image_source;
            set
            {
                _image_source = value;
                OnPropertyChanged("image_source");
            }
        }

        private bool _btn_activation_status = true;
        public bool btn_activation_status
        {
            get => _btn_activation_status;
            set
            {
                _btn_activation_status = value;
                OnPropertyChanged("btn_activation_status");
            }
        }

        private bool _btn_visible = true;
        public bool btn_visible
        {
            get => _btn_visible;
            set
            {
                _btn_visible = value;
                OnPropertyChanged("btn_visible");
            }
        }

        private string _btn_background_color = "e62f31";
        public string btn_background_color
        {
            get => _btn_background_color;
            set
            {
                _btn_background_color = value;
                OnPropertyChanged("btn_background_color");
            }
        }
    }

    public class PreCondition : BaseViewModel
    {
        public string routine_id { get; set; }
        public string id { get; set; }
        public string pre_condition_type { get; set; }
        public int? pid { get; set; }
        public string description { get; set; }
        public string lower_limit { get; set; }
        public string upper_limit { get; set; }

        private string _current_value;
        public string current_value
        {
            get => _current_value;
            set
            {
                _current_value = value;
                OnPropertyChanged("current_value");
            }
        }

        private bool _is_check;
        public bool is_check
        {
            get => _is_check;
            set
            {
                _is_check = value;
                OnPropertyChanged("is_check");
            }
        }
    }

    public class EcuTestRoutine : BaseViewModel
    {
        public int id { get; set; }
        public string ecu_name { get; set; }

        private double _opacity;
        public double opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                OnPropertyChanged("opacity");
            }
        }

        public List<IorResult> results { get; set; }
        public List<PidCode> pid_list { get; set; }
    }
}
