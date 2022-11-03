using Acr.UserDialogs;
using Bajaj.Model;
using Bajaj.Services;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Bajaj.ViewModel
{
    public class DtcViewModel : BaseViewModel
    {
        public ReadDtcResponseModel read_dtc;
        public DtcViewModel()
        {
            try
            {
                read_dtc = new ReadDtcResponseModel();
                ecus_list = new ObservableCollection<DtcEcusModel>();
                dtc_list = new ObservableCollection<DtcCode>();
                dtc_server_list = new List<DtcCode>();
                DTC_Error = "NoValue";
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.StackTrace}", "Alert", "Ok");
            }
        }

        public ICommand EcuTabCommand => new Command(async (obj) =>
        {
            try
            {
                var selected = (DtcEcusModel)obj;
                selected.opacity = 1;
                selected_ecu = selected;

                foreach (var ecu in ecus_list)
                {
                    if (selected != ecu)
                    {
                        ecu.opacity = .5;
                    }
                }

                dtc_list = new ObservableCollection<DtcCode>(ecus_list.FirstOrDefault(x => x.ecu_name == selected_ecu.ecu_name).dtc_list);
                empty_view_text = selected_ecu.emptyViewText;
            }
            catch (Exception ex)
            {
            }
        });


        #region properties
        private string _DTCFoundOrNotMessage;
        public string DTCFoundOrNotMessage
        {
            get { return _DTCFoundOrNotMessage; }
            set { _DTCFoundOrNotMessage = value; OnPropertyChanged("DTCFoundOrNotMessage"); }
        }

        private ObservableCollection<DtcEcusModel> _ecus_list;
        public ObservableCollection<DtcEcusModel> ecus_list
        {
            get => _ecus_list;
            set
            {
                _ecus_list = value;
                OnPropertyChanged("ecus_list");
            }
        }

        private DtcEcusModel _selected_ecu;
        public DtcEcusModel selected_ecu
        {
            get => _selected_ecu;
            set
            {
                _selected_ecu = value;
                OnPropertyChanged("selected_ecu");
            }
        }

        private ObservableCollection<DtcCode> _dtc_list;
        public ObservableCollection<DtcCode> dtc_list
        {
            get => _dtc_list;
            set
            {
                _dtc_list = value;
                OnPropertyChanged("dtc_list");
            }
        }

        private List<DtcCode> _dtc_server_list;
        public List<DtcCode> dtc_server_list
        {
            get => _dtc_server_list;
            set
            {
                _dtc_server_list = value;
                OnPropertyChanged("dtc_server_list");
            }
        }

        private bool _is_running = true;
        public bool is_running
        {
            get => _is_running;
            set
            {
                _is_running = value;
                OnPropertyChanged("is_running");
            }
        }

        private string _empty_view_text = "Loading...";
        public string empty_view_text
        {
            get => _empty_view_text;
            set
            {
                _empty_view_text = value;
                OnPropertyChanged("empty_view_text");
            }
        }
        #endregion

        private string errorMsg;
        public string ErrorMsg
        {
            get => errorMsg;
            set
            {
                errorMsg = value;
                OnPropertyChanged("ErrorMsg");
            }
        }

        private string _DTC_Error;
        public string DTC_Error
        {
            get => _DTC_Error;
            set
            {
                _DTC_Error = value;
                OnPropertyChanged("DTC_Error");
            }
        }
    }
}
