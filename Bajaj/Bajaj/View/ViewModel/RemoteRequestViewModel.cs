using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class RemoteRequestViewModel : BaseViewModel
    {
        ApiServices services;
        public RemoteRequestViewModel()
        {
            services = new ApiServices();
            RJCM = new ObservableCollection<ResponseJobCardModel>();
            LoadRequestList();
        }
        private ObservableCollection<ResponseJobCardModel> _RJCM;
        public ObservableCollection<ResponseJobCardModel> RJCM
        {
            get { return _RJCM; }
            set
            {
                _RJCM = value;
                OnPropertyChanged("RJCM");
            }
        }
        public async Task LoadRequestList()
        {
            try
            {
                var RequestList = new List<ResponseJobCardModel>();
                Device.BeginInvokeOnMainThread(async() =>
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        string UserID = App.Current.Properties["LoginUserId"].ToString();
                        var ExpertNotificationCount = await services.GetExpertRequestList(UserID);
                        if (ExpertNotificationCount != null)
                        {
                            try
                            {
                                RJCM = new ObservableCollection<ResponseJobCardModel>();
                                if (ExpertNotificationCount != null)
                                {
                                    if (ExpertNotificationCount.count > 0)
                                    {
                                        RJCM.Add(new ResponseJobCardModel
                                        {
                                            case_id = ExpertNotificationCount.results.FirstOrDefault()?.case_id,
                                            expert_device = ExpertNotificationCount.results.FirstOrDefault()?.expert_device,
                                            expert_email = ExpertNotificationCount.results.FirstOrDefault()?.expert_email,
                                            expert_user = ExpertNotificationCount.results.FirstOrDefault().expert_user,
                                            id = ExpertNotificationCount.results.FirstOrDefault()?.id,
                                            job_card_session = ExpertNotificationCount.results.FirstOrDefault()?.job_card_session,
                                            remote_session_id = ExpertNotificationCount.results.FirstOrDefault()?.remote_session_id,
                                            request_status = ExpertNotificationCount.results.FirstOrDefault().request_status,
                                            status = ExpertNotificationCount.results.FirstOrDefault()?.status
                                        });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                });
            }
            catch (Exception)
            {

            }
        }
        public ICommand Accept
        {
            get
            {
                return new Command((e) =>
                {
                    var ItemResult = (e as ResponseJobCardModel);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                        {
                            await Task.Delay(100);
                            try
                            {
                                if (ItemResult.request_status != true)
                                {
                                    ResponseJobCardModel Value = new ResponseJobCardModel
                                    {
                                        case_id = ItemResult.case_id,
                                        expert_device = ItemResult.expert_device,
                                        expert_email = ItemResult.expert_email,
                                        expert_user = ItemResult.expert_user,
                                        id = ItemResult.id,
                                        job_card_session = ItemResult.job_card_session,
                                        remote_session_id = ItemResult.remote_session_id,
                                        request_status = true,
                                        status = "open"
                                    };
                                    var Rec = await services.AcceptRemoteRequest(Value, ItemResult.job_card_session, ItemResult.id);
                                    App.RemoteSessionId = Rec.job_card_session;
                                    SessionStaticData.SessionResponse = new List<SessionResponseJobCardModel>();
                                    SessionStaticData.SessionResponse.Add(new SessionResponseJobCardModel
                                    {
                                        case_id = Value.case_id,
                                        expert_device = Value.expert_device,
                                        expert_email = Value.expert_email,
                                        expert_user = Value.expert_user,
                                        id = Value.id,
                                        job_card_session = Value.job_card_session,
                                        remote_session_id = Value.remote_session_id,
                                        request_status = Value.request_status,
                                        status = Value.status
                                    });

                                    //App.OnitExpert(Rec.id, Rec.job_card_session);
                                    var resul = await App.controlEventManager.Init();
                                    DependencyService.Get<IToastMessage>().Show(resul);
                                    App.Current.MainPage = new NavigationPage(new AppFeaturePage());


                                    if (App.IsSameResponce != null)
                                    {
                                        App.IsSameResponce.Clear();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    });
                });
            }
        }
        public ICommand Decline
        {
            get
            {
                return new Command((e) =>
                {
                    var ItemResult = (e as ResponseJobCardModel);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (ItemResult.request_status == true)
                        {
                            ResponseJobCardModel Value = new ResponseJobCardModel
                            {
                                case_id = ItemResult.case_id,
                                expert_device = ItemResult.expert_device,
                                expert_email = ItemResult.expert_email,
                                expert_user = ItemResult.expert_user,
                                id = ItemResult.id,
                                job_card_session = ItemResult.job_card_session,
                                remote_session_id = ItemResult.remote_session_id,
                                request_status = false,
                                status = "closed_by_expert"
                            };
                            var Rec = await services.AcceptRemoteRequest(Value, ItemResult.job_card_session, ItemResult.id);
                            await LoadRequestList();
                        }
                    });
                });
            }
        }
    }
}
