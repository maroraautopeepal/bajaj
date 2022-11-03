using Bajaj.ViewModel;
using MultiEventController.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bajaj.View.GdSection
{
    public class TreeListModel : BaseViewModel
    {
        //public long id { get; set; }
        //public string topic { get; set; }
        //public string description { get; set; }
        //public string group_name { get; set; }
        //public double view_height { get; set; }
        //public long ok_page_node_id { get; set; }
        //public long not_ok_page_node_id { get; set; }
        //public bool page_visible { get; set; }

        private long _id;
        public long id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private long _ok_page_node_id;
        public long ok_page_node_id
        {
            get => _ok_page_node_id;
            set
            {
                _ok_page_node_id = value;
                OnPropertyChanged();
            }
        }

        private long _not_ok_page_node_id;
        public long not_ok_page_node_id
        {
            get => _not_ok_page_node_id;
            set
            {
                _not_ok_page_node_id = value;
                OnPropertyChanged();
            }
        }

        private string _topic;
        public string topic
        {
            get => _topic;
            set
            {
                _topic = value;
                OnPropertyChanged();
            }
        }

        private string _description;
        public string description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private string _group_name;
        public string group_name
        {
            get => _group_name;
            set
            {
                _group_name = value;
                OnPropertyChanged();
            }
        }

        private double _view_height;
        public double view_height
        {
            get => _view_height;
            set
            {
                _view_height = value;
                OnPropertyChanged();
            }
        }

        private string _description_text_color;
        public string description_text_color
        {
            get => _description_text_color;
            set
            {
                _description_text_color = value;
                OnPropertyChanged();
            }
        }

        private string _description_background_color;
        public string description_background_color
        {
            get => _description_background_color;
            set
            {
                _description_background_color = value;
                OnPropertyChanged();
            }
        }

        //private double _scroll_height;
        //public double scroll_height
        //{
        //    get => _scroll_height;
        //    set
        //    {
        //        _scroll_height = value;
        //        OnPropertyChanged();
        //    }
        //}

        private bool _page_visible;
        public bool page_visible
        {
            get => _page_visible;
            set
            {
                _page_visible = value;
                OnPropertyChanged();
            }
        }

        //private double _traslate_y_exist;
        //public double traslate_y_exist
        //{
        //    get => _traslate_y_exist;
        //    set
        //    {
        //        _traslate_y_exist = value;
        //        OnPropertyChanged();
        //    }
        //}

        private ObservableCollection<GroupListModel> _group_list = new ObservableCollection<GroupListModel>();
        public ObservableCollection<GroupListModel> group_list
        {
            get => _group_list;
            set
            {
                _group_list = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DecissionModel> _decission_list = new ObservableCollection<DecissionModel>();
        public ObservableCollection<DecissionModel> decission_list
        {
            get => _decission_list;
            set
            {
                _decission_list = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<LastQueCheckModel> _last_question_list = new ObservableCollection<LastQueCheckModel>();
        public ObservableCollection<LastQueCheckModel> last_question_list
        {
            get => _last_question_list;
            set
            {
                _last_question_list = value;
                OnPropertyChanged();
            }
        }

        //private double _view_height;
        //public double view_height
        //{
        //    get => _view_height;
        //    set
        //    {
        //        _view_height = value;
        //        OnPropertyChanged();
        //    }
        //}
    }

    public class GroupListModel : BaseViewModel
    {
        private string _upper_limit;
        public string upper_limit
        {
            get => _upper_limit;
            set
            {
                _upper_limit = value;
                OnPropertyChanged();
            }
        }

        private string _lower_limit;
        public string lower_limit
        {
            get => _lower_limit;
            set
            {
                _lower_limit = value;
                OnPropertyChanged();
            }
        }

        private bool _upper_lower_value_visible = false;
        public bool upper_lower_value_visible
        {
            get => _upper_lower_value_visible;
            set
            {
                _upper_lower_value_visible = value;
                OnPropertyChanged();
            }
        }

        private string _unit;
        public string unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged();
            }
        }

        private string _group_name;
        public string group_name
        {
            get => _group_name;
            set
            {
                _group_name = value;
                OnPropertyChanged();
            }
        }

        private string _current_limit = string.Empty;
        public string current_limit
        {
            get => _current_limit;
            set
            {
                _current_limit = value;
                if (!IsDigitsOnly(_current_limit))
                {
                    _current_limit = "";
                }
                OnPropertyChanged();
            }
        }
        private bool IsDigitsOnly(string str)
        {
            return str.All(c => c >= '0' && c <= '9');
        }
        private string _status_color = "#000000";
        public string status_color
        {
            get => _status_color;
            set
            {
                _status_color = value;
                OnPropertyChanged();
            }
        }


        private string _entry_description;
        public string entry_description
        {
            get => _entry_description;
            set
            {
                _entry_description = value;
                OnPropertyChanged();
            }
        }
    }

    public class LastQueCheckModel : BaseViewModel
    {
        private bool _isCheck;
        public bool isCheck
        {
            get => _isCheck;
            set
            {
                _isCheck = value;
                OnPropertyChanged();
            }
        }

        private string _describe;
        public string describe
        {
            get => _describe;
            set
            {
                _describe = value;
                OnPropertyChanged();
            }
        }

        private long _id;
        public long id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
    }

    public class DecissionModel : BaseViewModel
    {
        private bool _isCheck;
        public bool isCheck
        {
            get => _isCheck;
            set
            {
                _isCheck = value;
                OnPropertyChanged();
            }
        }

        private string _text_value;
        public string text_value
        {
            get => _text_value;
            set
            {
                _text_value = value;
                OnPropertyChanged();
            }
        }

        private long _next_node;
        public long next_node
        {
            get => _next_node;
            set
            {
                _next_node = value;
                OnPropertyChanged();
            }
        }

        private string _type;
        public string type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        //private string _not_ok;
        //public string not_ok
        //{
        //    get => _not_ok;
        //    set
        //    {
        //        _not_ok = value;
        //        OnPropertyChanged();
        //    }
        //}

        private long _id;
        public long id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
    }

}
