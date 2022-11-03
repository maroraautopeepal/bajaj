using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class IvnDtcViewModel : BaseViewModel
    {
        ApiServices services;
        List<string> FrameIdList;

        public IvnDtcViewModel()
        {

            try
            {
                services = new ApiServices();
                FrameIdList = new List<string>();
                ServerDtcList = new List<DtcModel>();
                EcuDtcList = new List<DtcModel>();
                DtcList = new List<DtcListModel>();

                ecus_list = new ObservableCollection<DtcEcusModel>();
                dtc_list = new ObservableCollection<DtcCode>();
                //dtc_list = new ObservableCollection<DtcListModel>();
                dtc_server_list = new List<DtcCode>();
                selected_ecu = new DtcEcusModel();

                //AllDtc = get_dtc_from_local().Result;


                AllDtc = new List<DtcResults>();// get_dtc_All_Recoed().Result.FirstOrDefault().DTC_R;
                //IVNAllDTCDetail = get_dtc_All_Recoed().Result.FirstOrDefault().IDCT_R;

                DTC_Error = "NoValue";
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.StackTrace}", "Alert", "Ok");
            }
        }

        //public ICommand EcuTabCommand => new Command(async (obj) =>
        //{
        //    try
        //    {
        //        var selected = (DtcEcusModel)obj;
        //        selected.opacity = 1;
        //        selected_ecu = selected;

        //        foreach (var ecu in ecus_list)
        //        {
        //            if (selected != ecu)
        //            {
        //                ecu.opacity = .5;
        //            }
        //        }
        //        dtc_list = new ObservableCollection<DtcCode> (selected_ecu.dtc_list);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //});


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

        //private ObservableCollection<DtcListModel> _dtc_list;
        //public ObservableCollection<DtcListModel> dtc_list
        //{
        //    get => _dtc_list;
        //    set
        //    {
        //        _dtc_list = value;
        //        OnPropertyChanged("dtc_list");
        //    }
        //}

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

        private List<IVN_DtcModel> serverIVNDtcList;
        public List<IVN_DtcModel> ServerIVNDtcList
        {
            get => serverIVNDtcList;
            set
            {
                serverIVNDtcList = value;
                OnPropertyChanged("ServerIVNDtcList");
            }
        }

        private string _DTCFoundOrNotMessage;
        public string DTCFoundOrNotMessage
        {
            get { return _DTCFoundOrNotMessage; }
            set { _DTCFoundOrNotMessage = value; 
                OnPropertyChanged("DTCFoundOrNotMessage"); }
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


        private List<DtcModel> serverDtcList;
        public List<DtcModel> ServerDtcList
        {
            get => serverDtcList;
            set
            {
                serverDtcList = value;
                OnPropertyChanged("ServerDtcList");
            }
        }

        private List<DtcModel> ecuDtcList;
        public List<DtcModel> EcuDtcList
        {
            get => ecuDtcList;
            set
            {
                ecuDtcList = value;
                OnPropertyChanged("EcuDtcList");
            }
        }

        private List<DtcListModel> dtcList;
        public List<DtcListModel> DtcList
        {
            get => dtcList;
            set
            {
                dtcList = value;
                OnPropertyChanged("DtcList");
            }
        }

        private List<DtcResults> allDtc;
        public List<DtcResults> AllDtc
        {
            get => allDtc;
            set
            {
                allDtc = value;
                OnPropertyChanged("AllDtc");
            }
        }

        private List<IVN_Result> _IVNAllDTCDetail;
        public List<IVN_Result> IVNAllDTCDetail
        {
            get => _IVNAllDTCDetail;
            set
            {
                _IVNAllDTCDetail = value;
                OnPropertyChanged("IVNAllDTCDetail");
            }
        }

        public async Task<List<IVNdtc_AND_dtc>> get_dtc_from_local()
        {
            try
            {
                //var DTC = await services.get_dtc(App.JwtToken, 0);
                //var IVNDTC = await services.get_ivn_dtc(App.JwtToken, 0);

                //DTC
                string Data = string.Empty;
                List<DtcResults> DTC = new List<DtcResults>();
                Data = DependencyService.Get<ISaveLocalData>().GetData("dtcjson");
                if (Data != null)
                {
                    DTC = JsonConvert.DeserializeObject<List<DtcResults>>(Data);
                }

                //IVN DTC
                string IVNData = string.Empty;
                List<IVN_Result> IVNDTC = new List<IVN_Result>();
                IVNData = DependencyService.Get<ISaveLocalData>().GetData("IVN_DtcJson");
                if (IVNData != null)
                {
                    IVNDTC = JsonConvert.DeserializeObject<List<IVN_Result>>(IVNData);
                }

                var Result = new List<IVNdtc_AND_dtc>();
                Result.Add(new IVNdtc_AND_dtc { DTC_R = DTC, IDCT_R = IVNDTC });
                return Result;
            }
            catch (Exception ex)
            {
                return null;
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
