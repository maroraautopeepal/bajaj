using Bajaj.Model;
using Bajaj.Services;
using System;
using System.Collections.Generic;

namespace Bajaj.ViewModel
{
    public class RemoteDiagnosticViewModel : BaseViewModel
    {
        ApiServices services;
        public RemoteDiagnosticViewModel()
        {
            try
            {
                services = new ApiServices();
                OnlineExpertList = new List<ExpertModel>();


                if (!CurrentUserEvent.Instance.IsExpert)
                {
                    GetOnlineExpertList();
                }

                ////OnlineExpertList = new List<ExpertModel>
                ////{
                ////    new ExpertModel
                ////    {
                ////        first_name = "Diagnostic Expert North1",
                ////        mobile = "0000000000",
                ////        workshop = "VECV Plant",
                ////        //auto_session_internal_id = "Request Remote Support?",
                ////        status = "Offline",
                ////        status_color = "#FF0000",
                ////    },
                ////  new ExpertModel
                ////  {
                ////      first_name = "Diagnostic Expert North2",
                ////      mobile = "0000000000",
                ////      workshop= "VECV Plant",
                ////      //RequestType = "Request Remote Support?",
                ////      status= "Online",
                ////      status_color= "#008000",
                ////  },
                ////  new ExpertModel
                ////  {
                ////      first_name= "Diagnostic Suppert",
                ////      mobile= "0000000000",
                ////      workshop= "VECV Plant",
                ////      //RequestType = "Request Remote Support?",
                ////      status= "Busy",
                ////      status_color= "#FFA500",
                ////  },
                ////};
                //////GetJobCardList();
            }
            catch (Exception ex)
            {
            }
        }

        private List<ExpertModel> onlineExpertList;
        public List<ExpertModel> OnlineExpertList
        {
            get => onlineExpertList;
            set
            {
                onlineExpertList = value;
                OnPropertyChanged("JobCardList");
            }
        }

        public void GetOnlineExpertList()
        {
            try
            {
                var result = services.GetOnlineExpert(App.JwtToken);
                //OnlineExpertList = result.Result.results;

                foreach (var item in result.Result.results)
                {
                    ExpertModel expertModel = new ExpertModel();
                    expertModel.auto_session_id = item.auto_session_id;
                    expertModel.auto_session_internal_id = item.auto_session_internal_id;
                    expertModel.email = item.email;
                    expertModel.first_name = item.first_name;
                    expertModel.id = item.id;
                    expertModel.last_name = item.last_name;
                    expertModel.mobile = item.mobile;
                    expertModel.remote_session_id = item.remote_session_id;
                    expertModel.remote_session_internal_id = item.remote_session_internal_id;
                    if (item.status == "free")
                    {
                        expertModel.status = "Online";
                        //expertModel.status_color = "#00FF00";
                        expertModel.status_color = "#4CBB17";
                        expertModel.btnIsActive = true;
                    }
                    else if (item.status == "busy")
                    {
                        expertModel.status = "Busy";
                        //expertModel.status_color = "#FF5733";
                        expertModel.status_color = "#FF7800";
                        expertModel.btnIsActive = true;
                    }

                    expertModel.workshop = item.workshop;
                    expertModel.workshop_city = item.workshop_city;


                    OnlineExpertList.Add(expertModel);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
