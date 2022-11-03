using Newtonsoft.Json;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class IvnLiveParameterSelectViewModel : BaseViewModel
    {
        ApiServices services;
        public IvnLiveParameterSelectViewModel()
        {
            try
            {
                services = new ApiServices();
                PidList = new List<LiveParameterSelectModel>();
                LiveParameterList = new List<PidCode>();
                //AllPid = get_pid_from_local().Result;

                AllPIDDetail = new List<PID>();
                AllPIDDetail = get_All_pid_from_local().Result;

                AllPid = AllPIDDetail.FirstOrDefault().NormalPID;
                IVNAllPIDDetail = AllPIDDetail.FirstOrDefault().IVNPID;
            }
            catch (Exception ex)
            {
            }
        }

        private List<LiveParameterSelectModel> pidList;
        public List<LiveParameterSelectModel> PidList
        {
            get => pidList;
            set
            {
                pidList = value;
                OnPropertyChanged("PidList");
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

        private List<PidCode> liveParameterList;
        public List<PidCode> LiveParameterList
        {
            get => liveParameterList;
            set
            {
                liveParameterList = value;
                OnPropertyChanged("LiveParameterList");
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
        public async Task<List<Results>> get_pid_from_local()
        {
            var res = await services.get_pid(App.JwtToken, 0);
            return res;
        }

        public async Task<List<PID>> get_All_pid_from_local()
        {
            // var res = await services.get_pid(App.JwtToken, 0);
            //return res;

            /* comment */
            //PID
            string Data = string.Empty;
            List<Results> PIDR = new List<Results>();
            Data = DependencyService.Get<ISaveLocalData>().GetData("pidjson");
            if (Data != null)
            {
                PIDR = JsonConvert.DeserializeObject<List<Results>>(Data);
            }

            //IVN PID
            string IVNData = string.Empty;
            List<PIDResult> IVNPID = new List<PIDResult>();
            IVNData = DependencyService.Get<ISaveLocalData>().GetData("IVN_PidJson");
            if (IVNData != null)
            {
                IVNPID = JsonConvert.DeserializeObject<List<PIDResult>>(IVNData);
            }

            var Result = new List<PID>();
            /* comment */
            //Result.Add(new Model.PID { NormalPID = PIDR, IVNPID = IVNPID });            
            Result.Add(new Model.PID { NormalPID = new List<Results>(), IVNPID = IVNPID });
            return Result;
        }

        public async Task<List<Results>> get_pidValues(int ID)
        {
            var res = await services.get_pid(App.JwtToken, ID);
            return res;
        }

        private List<SelectedParameterMessage> _SelectedParameterMessage;
        public List<SelectedParameterMessage> SelectedParameter_Messages
        {
            get => _SelectedParameterMessage;
            set
            {
                _SelectedParameterMessage = value;
                OnPropertyChanged("SelectedParameter_Messages");
            }
        }

        //public async Task<List<PidCode>> get_pid(int id)
        //{
        //    try
        //    {
        //        var res = await services.get_pid(App.JwtToken,id);
        //        var pid_list = res.codes.;

        //      return pid_list;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
    }

}
