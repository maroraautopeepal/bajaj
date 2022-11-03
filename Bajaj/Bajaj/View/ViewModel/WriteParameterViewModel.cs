using Bajaj.Model;
using Bajaj.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bajaj.ViewModel
{
    public class WriteParameterViewModel : BaseViewModel
    {
        public WriteParameterViewModel()
        {
        }

        public ICommand EcuTabCommand => new Command(async (obj) =>
        {
            try
            {
                var selected = (EcusModel)obj;
                selected.opacity = 1;
                selected_ecu = selected;

                foreach (var ecu in ecus_list)
                {
                    if (selected != ecu)
                    {
                        ecu.opacity = .5;
                    }
                }

                static_pid_list = pid_list = new ObservableCollection<PidCode>(
                                                    ecus_list.FirstOrDefault(x => x.ecu_name == selected_ecu.ecu_name).pid_list);

                if (App.ConnectedVia == "USB")
                {
                    DependencyService.Get<Interfaces.IConnectionUSB>().SetDongleProperties(selected_ecu.protocol, selected_ecu.txHeader, selected_ecu.rxHeader);
                }
                else if (App.ConnectedVia == "BT")
                {
                    await DependencyService.Get<Interfaces.IBth>().SetDongleProperties(selected_ecu.protocol, selected_ecu.txHeader, selected_ecu.rxHeader);
                }
            }
            catch (Exception ex)
            {
            }
        });

        private ObservableCollection<EcusModel> _ecus_list;
        public ObservableCollection<EcusModel> ecus_list
        {
            get => _ecus_list;
            set
            {
                _ecus_list = value;
                OnPropertyChanged("ecus_list");
            }
        }

        private EcusModel _selected_ecu;
        public EcusModel selected_ecu
        {
            get => _selected_ecu;
            set
            {
                _selected_ecu = value;
                OnPropertyChanged("selected_ecu");
            }
        }

        private ObservableCollection<PidCode> _static_pid_list;
        public ObservableCollection<PidCode> static_pid_list
        {
            get => _static_pid_list;
            set
            {
                _static_pid_list = value;
                OnPropertyChanged("static_pid_list");
            }
        }

        private ObservableCollection<PidCode> _pid_list;
        public ObservableCollection<PidCode> pid_list
        {
            get => _pid_list;
            set
            {
                _pid_list = value;
                OnPropertyChanged("pid_list");
            }
        }

        //private ObservableCollection<PidCode> _selected_pid_list;
        //public ObservableCollection<PidCode> selected_pid_list
        //{
        //    get => _selected_pid_list;
        //    set
        //    {
        //        _selected_pid_list = value;
        //        OnPropertyChanged("selected_pid_list");
        //    }
        //}

        private PidCode _selected_pid;
        public PidCode selected_pid
        {
            get => _selected_pid;
            set
            {
                _selected_pid = value;
                OnPropertyChanged("selected_pid");
            }
        }

        private bool _pid_view_visible = false;
        public bool pid_view_visible
        {
            get => _pid_view_visible;
            set
            {
                _pid_view_visible = value;
                OnPropertyChanged("pid_view_visible");
            }
        }

        private bool _default_view_visible = false;
        public bool default_view_visible
        {
            get => _default_view_visible;
            set
            {
                _default_view_visible = value;
                OnPropertyChanged("default_view_visible");
            }
        }

        private bool _iqa_view_visible = false;
        public bool iqa_view_visible
        {
            get => _iqa_view_visible;
            set
            {
                _iqa_view_visible = value;
                OnPropertyChanged("iqa_view_visible");
            }
        }


        private string _new_value;
        public string new_value
        {
            get => _new_value;
            set
            {
                _new_value = value;
                OnPropertyChanged("new_value");
            }
        }

        private string _cylinder_one;
        public string cylinder_one
        {
            get => _cylinder_one;
            set
            {
                _cylinder_one = value;
                OnPropertyChanged("cylinder_one");
            }
        }

        private string _cylinder_two;
        public string cylinder_two
        {
            get => _cylinder_two;
            set
            {
                _cylinder_two = value;
                OnPropertyChanged("cylinder_two");
            }
        }

        private string _cylinder_three;
        public string cylinder_three
        {
            get => _cylinder_three;
            set
            {
                _cylinder_three = value;
                OnPropertyChanged("cylinder_three");
            }
        }

        private string _cylinder_four;
        public string cylinder_four
        {
            get => _cylinder_four;
            set
            {
                _cylinder_four = value;
                OnPropertyChanged("cylinder_four");
            }
        }

        private string _new_cylinder_one = "DBG63S2";//"NIAHUEL";
        public string new_cylinder_one
        {
            get => _new_cylinder_one;
            set
            {
                _new_cylinder_one = value;
                OnPropertyChanged("new_cylinder_one");
            }
        }

        private string _new_cylinder_two = "DBG63S2";//"AAAAB6D";
        public string new_cylinder_two
        {
            get => _new_cylinder_two;
            set
            {
                _new_cylinder_two = value;
                OnPropertyChanged("new_cylinder_two");
            }
        }

        private string _new_cylinder_three = "DBG63S2";//"AAAAB7I";
        public string new_cylinder_three
        {
            get => _new_cylinder_three;
            set
            {
                _new_cylinder_three = value;
                OnPropertyChanged("new_cylinder_three");
            }
        }

        private string _new_cylinder_four = "DBG63S2";//"AAAAABF";
        public string new_cylinder_four
        {
            get => _new_cylinder_four;
            set
            {
                _new_cylinder_four = value;
                OnPropertyChanged("new_cylinder_four");
            }
        }

        private string _title = "Select a parameter to write";
        public string title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged("title");
            }
        }

        private string _search_key = string.Empty;
        public string search_key
        {
            get => _search_key;
            set
            {
                _search_key = value;
                OnPropertyChanged("search_key");
            }
        }
    }
}
