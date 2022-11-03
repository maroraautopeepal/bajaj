using Bajaj.Model;
using Bajaj.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bajaj.ViewModel
{
    public class JobcardViewModel : BaseViewModel
    {
        ApiServices services;
        public JobcardViewModel()
        {
            try
            {
                App.is_login = true;
                services = new ApiServices();
                JobCardList = new ObservableCollection<JobCardListModel>();

                GetJobCardList();
                //get_all_models(0);
                //get_dtc(6);
                //get_pid(6);
            }
            catch (Exception ex)
            {
            }
        }

        private ObservableCollection<JobCardListModel> jobCardList;
        public ObservableCollection<JobCardListModel> JobCardList
        {
            get => jobCardList;
            set
            {
                jobCardList = value;
                OnPropertyChanged("JobCardList");
            }
        }

        public async Task MoveToRemotePage()
        {
            try
            {
                var token = App.Current.Properties["token"].ToString();
                if (string.IsNullOrEmpty(token))
                {
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            //top = 20;
                            break;
                        case Device.Android:
                            App.IsSameResponce.Clear();
                            App.Current.Properties["MasterLoginUserRoleBY"] = null;
                            App.Current.Properties["LoginUserId"] = null;
                            App.Current.MainPage = new NavigationPage(new View.LoginPage());
                            //MainPage = new NavigationPage(new ConnectionPage(null));
                            break;
                        case Device.UWP:
                            //MainPage = new View.LoginPage();
                            App.Current.MainPage = new NavigationPage(new View.LoginPage());
                            break;
                        default:
                            //top = 0;
                            break;
                    }
                }
                else
                {
                    App.Current.MainPage = new NavigationPage(new View.RemoteRequestPage());
                }
            }
            catch (Exception ex)
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        //top = 20;
                        break;
                    case Device.Android:
                        App.IsSameResponce.Clear();
                        App.Current.Properties["MasterLoginUserRoleBY"] = null;
                        App.Current.Properties["LoginUserId"] = null;
                        App.Current.MainPage = new NavigationPage(new View.LoginPage());
                        //MainPage = new NavigationPage(new ConnectionPage(null));
                        break;
                    case Device.UWP:
                        //MainPage = new View.LoginPage();
                        App.Current.MainPage = new NavigationPage(new View.LoginPage());
                        break;
                    default:
                        //top = 0;
                        break;
                }
            }
        }
        public async void GetJobCardList()
        {
            try
            {
                var result = await services.GetJobCard(App.JwtToken, "JbCardJson.txt");

                if (result != null)
                {
                    JobCardList.Clear();
                    foreach (var item in result)
                    {
                        //var card = item.job_card_session.Where(x => x.vehicle_model.parent.name.ToLower().Contains("mDe".ToLower()) || x.vehicle_model.parent.name.ToLower().Contains("VO".ToLower()) || x.vehicle_model.parent.name.ToLower().Contains("VDE".ToLower()));
                        //foreach (var jobCard in item.job_card_session)
                        {
                            var date = item.job_card_session[0].start_date?.ToString("dd-MM-yyyy");
                            if (item.job_card_session[0].status != "closed")
                            {
                                try
                                {
                                    string model_with_submodel = string.Empty;

                                    if (!string.IsNullOrEmpty(item.job_card_session[0].vehicle_model.name) && item.job_card_session[0].vehicle_model.name != "NA")
                                    {
                                        model_with_submodel = item.job_card_session[0].vehicle_model.parent.name + "-" + item.job_card_session[0].vehicle_model.name;
                                    }
                                    else
                                    {
                                        model_with_submodel = item.job_card_session[0].vehicle_model.parent.name;
                                    }
                                    JobCardListModel jobCardListModel = new JobCardListModel
                                    {
                                        jobcard_name = item.job_card_name,
                                        chasis_id = item.chasis_id,
                                        registration_no = item.registration_no,
                                        complaints = item.complaints,
                                        fert_code = item.fert_code,
                                        jobcard_status = item.status,
                                        vehicle_segment = item.vehicle_segment,
                                        modified = item.modified,
                                        km_covered = item.km_covered,
                                        session_id = item.job_card_session[0].session_id,
                                        status = item.job_card_session[0].status,
                                        session_type = item.job_card_session[0].session_type,
                                        source = item.job_card_session[0].source,
                                        end_date = item.job_card_session[0].end_date,
                                        start_date = item.job_card_session[0].start_date,
                                        job_card = item.job_card_session[0].job_card,
                                        id = item.job_card_session[0].id,
                                        vehicle_model = item.job_card_session[0].vehicle_model.parent.name,
                                        model_year = item.job_card_session[0].vehicle_model.model_year,
                                        sub_model = item.job_card_session[0].vehicle_model.name,
                                        show_start_date = date,
                                        model_id = 0,
                                        sub_model_id = item.job_card_session[0].vehicle_model.id,
                                        model_with_submodel = model_with_submodel,
                                        workshop = item.created_by.us_user.workshop.name,
                                        city = item.created_by.us_user.workshop.city,
                                        state = item.created_by.us_user.workshop.state
                                    };
                                    JobCardList.Add(jobCardListModel);
                                }   
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
            }
        }

        public void get_all_models(int id)
        {
            try
            {
                //var res = services.Login(model);
                var model = services.get_all_models(App.JwtToken, id);
                //return res.Result;
            }
            catch (Exception ex)
            {
                //return null;
            }
        }

        public void get_pid(int id)
        {
            try
            {
                //var res = services.Login(model);
                var pids = services.get_pid(App.JwtToken, id);
                //return res.Result;
            }
            catch (Exception ex)
            {
                //return null;
            }
        }

        public void get_dtc(int id)
        {
            try
            {
                //var res = services.Login(model);
                var dtcs = services.get_dtc(App.JwtToken, id);
                //return res.Result;
            }
            catch (Exception ex)
            {
                //return null;
            }
        }
    }
}
