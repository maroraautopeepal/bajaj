using Acr.UserDialogs;
using MultiEventController;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RemoteRequestPage : ContentPage
    {
        ApiServices services;
        RemoteRequestViewModel viewModel;
        public RemoteRequestPage()
        {
            InitializeComponent();
            services = new ApiServices();
            BindingContext = viewModel = new RemoteRequestViewModel();
        }

        //ControlEventManager controlEventManager;
        //public string ReceiveValue = string.Empty;
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            // if (controlEventManager == null)
            //{
            //    controlEventManager = new ControlEventManager();
            //}
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
                        try
                        {

                            string UserID = App.Current.Properties["LoginUserId"].ToString();
                            var ExpertNotificationCount = await services.GetExpertRequestList(UserID);
                            if (ExpertNotificationCount != null)
                            {

                                try
                                {
                                    viewModel.RJCM = new ObservableCollection<ResponseJobCardModel>();
                                    if (ExpertNotificationCount != null)
                                    {
                                        if (ExpertNotificationCount.count > 0)
                                        {
                                            viewModel.RJCM.Add(new ResponseJobCardModel
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
                                //if (ExpertNotificationCount!= null)
                                //{
                                //    RequestList.Clear();

                                //    if (ExpertNotificationCount.count != 0)
                                //    {
                                //        RequestList.Add(new ResponseJobCardModel
                                //        {
                                //            case_id = ExpertNotificationCount.results.FirstOrDefault().case_id,
                                //            expert_device = ExpertNotificationCount.results.FirstOrDefault().expert_device,
                                //            expert_email = ExpertNotificationCount.results.FirstOrDefault().expert_email,
                                //            expert_user = ExpertNotificationCount.results.FirstOrDefault().expert_user,
                                //            id = ExpertNotificationCount.results.FirstOrDefault().id,
                                //            job_card_session = ExpertNotificationCount.results.FirstOrDefault().job_card_session,
                                //            remote_session_id = ExpertNotificationCount.results.FirstOrDefault().remote_session_id,
                                //            request_status = ExpertNotificationCount.results.FirstOrDefault().request_status,
                                //            status = ExpertNotificationCount.results.FirstOrDefault().status
                                //        });
                                //        RemoteRequest.ItemsSource = RequestList;
                                //    }
                                //}
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                });
            }
            catch (Exception)
            {

            }
        }
        //private async void BTNAccept_Clicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Button btn = (Button)sender;
        //        Grid gd = (Grid)btn.Parent;
        //        Label lbl = (Label)gd.Parent;
        //        var ll = lbl.Text;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}
        private async void BTNDecline_Clicked(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception)
            {

            }
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await LoadRequestList();
        }
    }
}