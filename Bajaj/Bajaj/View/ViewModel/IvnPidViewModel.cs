using Bajaj.Model;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class IvnPidViewModel : BaseViewModel
    {
        public IvnPidViewModel()
        {
            try
            {
                ecus_list = new ObservableCollection<IvnEcusModel>();
                pid_list = new ObservableCollection<PIDFrameId>();
                static_pid_list = new ObservableCollection<PIDFrameId>();

                AllPIDDetail = new List<PID>();
            }
            catch (Exception ex)
            {
            }
        }


        public ICommand EcuTabCommand => new Command(async (obj) =>
        {
            try
            {
                var selected = (IvnEcusModel)obj;
                selected.opacity = 1;
                selected_ecu = selected;

                foreach (var ecu in ecus_list)
                {
                    if (selected != ecu)
                    {
                        ecu.opacity = .5;
                    }
                }
                pid_list = new ObservableCollection<PIDFrameId>(selected_ecu.pid_list);
                static_pid_list = pid_list;
            }
            catch (Exception ex)
            {

            }
        });

        private IvnEcusModel _selected_ecu;
        public IvnEcusModel selected_ecu
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

        private ObservableCollection<IvnEcusModel> _ecus_list;
        public ObservableCollection<IvnEcusModel> ecus_list
        {
            get => _ecus_list;
            set
            {
                _ecus_list = value;
                OnPropertyChanged("ecus_list");
            }
        }

        private ObservableCollection<PIDFrameId> _static_pid_list;
        public ObservableCollection<PIDFrameId> static_pid_list
        {
            get => _static_pid_list;
            set
            {
                _static_pid_list = value;
                OnPropertyChanged("static_pid_list");
            }
        }

        private ObservableCollection<PIDFrameId> _pid_list;
        public ObservableCollection<PIDFrameId> pid_list
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

        private PIDFrameId _check_changed_pid;
        public PIDFrameId check_changed_pid
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

        private List<PID> _AllPIDDetail;
        public List<PID> AllPIDDetail
        {
            get => _AllPIDDetail;
            set
            {
                _AllPIDDetail = value;
                OnPropertyChanged("AllPIDDetail");
            }
        }

        private List<Results> allPid;
        public List<Results> AllPid
        {
            get => allPid;
            set
            {
                allPid = value;
                OnPropertyChanged("AllPid");
            }
        }
        private List<PIDResult> _IVNAllPIDDetail;
        public List<PIDResult> IVNAllPIDDetail
        {
            get => _IVNAllPIDDetail;
            set
            {
                _IVNAllPIDDetail = value;
                OnPropertyChanged("IVNAllPIDDetail");
            }
        }

        private List<IVN_LiveParameterSelectModel> _IVN_pidList;
        public List<IVN_LiveParameterSelectModel> IVN_PidList
        {
            get => _IVN_pidList;
            set
            {
                _IVN_pidList = value;
                OnPropertyChanged("IVN_PidList");
            }
        }
    }
}
