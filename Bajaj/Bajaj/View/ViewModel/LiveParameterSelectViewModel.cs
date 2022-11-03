using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;

namespace Bajaj.ViewModel
{
    public class LiveParameterSelectViewModel : BaseViewModel
    {
        public LiveParameterSelectViewModel()
        {
            try
            {
                ecus_list = new ObservableCollection<EcusModel>();
            }
            catch (Exception ex)
            {
            }
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
            }
            catch (Exception ex)
            {
            }
        });



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

        private string _search_key;
        public string search_key
        {
            get => _search_key;
            set
            {
                _search_key = value;
                OnPropertyChanged("search_key");
            }
        }

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

        private ObservableCollection<PidCode> _selected_pid_list;
        public ObservableCollection<PidCode> selected_pid_list
        {
            get => _selected_pid_list;
            set
            {
                _selected_pid_list = value;
                OnPropertyChanged("selected_pid_list");
            }
        }

        private PidCode _check_changed_pid;
        public PidCode check_changed_pid
        {
            get => _check_changed_pid;
            set
            {
                _check_changed_pid = value;
                OnPropertyChanged("check_changed_pid");
            }
        }

        private string selectPidImage = "ic_select_all_pid.png";
        public string SelectPidImage
        {
            get => selectPidImage;
            set
            {
                selectPidImage = value;
                OnPropertyChanged("SelectPidImage");
            }
        }
    }
}
