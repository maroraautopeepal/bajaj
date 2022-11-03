using Acr.UserDialogs;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using MultiEventController;
using MultiEventController.Models;
using Plugin.Connectivity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Bajaj.Interfaces;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RemoteDiagnosticPage : DisplayAlertPage
    {
        JobCardListModel jobCardSession;
        RemoteDiagnosticViewModel viewModel;
        ApiServices services;
        string ConnectedDeviceVersion = string.Empty;
        public RemoteDiagnosticPage(JobCardListModel jobCardSession, string version)
        {
            try
            {
                InitializeComponent();
                services = new ApiServices();
                //this.jobCardSession = jobCardSession;
                //this.jobCardSession = jobCardSession;
                BindingContext = viewModel = new RemoteDiagnosticViewModel();

                this.jobCardSession = jobCardSession;
                ConnectedDeviceVersion = version;
            }
            catch (Exception ex)
            {
            }
        }


        protected async override void OnAppearing()
        {
            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //controlEventManager.OnRecievedData -= ControlEventManager_OnRecieved;
            //if (controlEventManager != null)
            //{
            //    // controlEventManager = null;
            //}
        }
        //private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
        //{
        //    var elementEventHandler = (sender as ElementEventHandler);
        //    this.ReceiveValue = elementEventHandler.ElementValue;
        //    controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        //}

        private async void ButtonYes_Clicked(object sender, EventArgs e)
        {
            try
            {
                // var selectedItem = (ExpertModel)((Button)sender).BindingContext;

                //await Task.Delay(200);
                Device.BeginInvokeOnMainThread(async () =>
                {
                    using (UserDialogs.Instance.Loading("Sending Request To Expert...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(200);

                        var selectedItem = (ExpertModel)((Button)sender).BindingContext;

                        if (selectedItem == null)
                            return;

                        App.notify_expert_id = Convert.ToString(selectedItem.id);
                        App.notify_expert_first_name = Convert.ToString(selectedItem.first_name);
                        App.SessionId = jobCardSession.id;

                        RemoteJobCardModel RequestModel = new RemoteJobCardModel
                        {
                            expert_user = selectedItem.id,
                            job_card_session = jobCardSession.id,
                            remote_session_status = jobCardSession.status,
                            //status = jobCardSession.status,
                            status = "new",
                            request_status = false,
                        };
                        var _isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                        if (_isReachable)
                        {
                            var ResultValue = await services.GetRemoteSession(App.SessionId);
                            if (ResultValue != null)
                            {
                                if (ResultValue.results != null)
                                {
                                    if (ResultValue.results.Count == 0)
                                    {
                                        var result = await services.CreateRemoteJobCard(RequestModel, App.SessionId);

                                        if (result != null)
                                        {
                                            if (result.badRequestResponseModel.non_field_errors.Count > 0)
                                            {
                                                var remoteSessionId = result.badRequestResponseModel.non_field_errors.FirstOrDefault().remote_session_id;
                                                var Res = services.UpdateRemoteJobCard(RequestModel, App.SessionId, remoteSessionId);
                                                var GotData = Res.Result;
                                                App.RemoteSessionId = Res.Result.job_card_session;
                                                alert.IsVisible = true;
                                                txt.Text = "Your request has been submitted sucessfully.\nKindly wait for call from Uptime center.";
                                                App.InitTechnician(remoteSessionId, App.SessionId);
                                            }
                                            else
                                            {
                                                App.RemoteSessionId = result.NewRequestResponseModel.job_card_session;
                                                alert.IsVisible = true;
                                                txt.Text = "Your request has been submitted sucessfully !!!.\n\nKindly wait for call from Uptime center.";
                                                App.InitTechnician(result.NewRequestResponseModel.remote_session_id, App.SessionId);
                                            }
                                            var resul = await App.controlEventManager.Init();
                                            DependencyService.Get<IToastMessage>().Show(resul);
                                            //controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
                                        }
                                    }
                                    else
                                    {
                                        alert.IsVisible = true;
                                        txt.Text = "Your Already Sent request to this Expert '" + selectedItem.first_name + " " + selectedItem.last_name + "'!!!.\n\nPlease Wait for Expert request Acceptance.";
                                    }
                                }
                            }
                            else
                            {
                                var result = services.CreateRemoteJobCard(RequestModel, App.SessionId);
                                if (result.Result.status != "Already Exist")
                                {
                                    alert.IsVisible = true;
                                    txt.Text = "Your request has been submitted sucessfully.\nKindly wait for call from Uptime center.";
                                    App.InitTechnician(result.Result.NewRequestResponseModel.id, result.Result.NewRequestResponseModel.job_card_session);
                                }
                                else
                                {
                                    alert.IsVisible = true;
                                    txt.Text = "Request has Already Exist !!!.\n\nKindly wait for call from Uptime center.";
                                }
                            }
                        }
                        else
                        {
                            alert.IsVisible = true;
                            txt.Text = "Internet Connection Issue \nPlease check Internet Connection !!!";
                        }

                    }

                    //    #region Old Code
                    //    //RemoteJobCardModel model = new RemoteJobCardModel
                    //    //{
                    //    //    expert_user = selectedItem.id,
                    //    //    job_card_session = jobCardSession.id,
                    //    //    remote_session_status = jobCardSession.status,
                    //    //    status = jobCardSession.status,
                    //    //    request_status = false,
                    //    //};
                    //    //var result = services.CreateRemoteJobCard(model, App.SessionId);
                    //    //if (result.Result.status == "Already Exist")
                    //    //{
                    //    //    var remoteSessionId = result.Result.badRequestResponseModel.non_field_errors[0].remote_session_id;
                    //    //    var Res = services.UpdateRemoteJobCard(model, App.SessionId, remoteSessionId);
                    //    //}
                    //    //else
                    //    //{
                    //    //    App.Technician(App.JwtToken, Convert.ToString(result.Result.NewRequestResponseModel.expert_user));
                    //    //    RemoteRequestDiagnostic RRD = new RemoteRequestDiagnostic
                    //    //    {
                    //    //        ForExpertSessionID = App.SessionId,
                    //    //        IDForRemote = Convert.ToString(result.Result.NewRequestResponseModel.id),
                    //    //        status = result.Result.NewRequestResponseModel.status,
                    //    //        expert_user = result.Result.NewRequestResponseModel.expert_user,
                    //    //        request_status = result.Result.NewRequestResponseModel.request_status
                    //    //    };
                    //    //    var json = JsonConvert.SerializeObject(RRD);
                    //    //    controlEventManager.SendRequestData(json);
                    //    //}
                    //    //Working = false;   
                    //    #endregion

                });
            }
            catch (Exception ex)
            {
                //Working = false;
            }
        }
        private async void BtnOk_Clicked(object sender, EventArgs e)
        {
            alert.IsVisible = false;

            NotificationModel model = new NotificationModel
            {
                title = "Remote Diagnostics Request",
                body = App.notify_jobcard_id,
                technician = App.notify_technitian_name,
                model = App.notify_vehicle_model,
                workshop = App.notify_workshop_name,
                address = App.notify_workshop_add
            };

            NotificationM model1 = new NotificationM
            {
                title = "Remote Diagnostics Request",
                body = $"{App.notify_jobcard_id}",
            };

            await services.ExperNotfy(model, model1, Convert.ToString(App.notify_expert_first_name) + App.notify_expert_id);
            await Navigation.PopAsync();
        }

        private void BackClicked(object sender, EventArgs e)
        {
            this.Navigation.PopAsync();
        }

        private void BtnExitClicked(object sender, EventArgs e)
        {
            this.Navigation.PopAsync();
            //DependencyService.Get<ICloseApplication>().closeApplication();
        }

        public void show_alert(string title, string message, bool btnCancel, bool btnOk)
        {
            Working = true;
            TitleText = title;
            MessageText = message;
            OkVisible = btnOk;
            CancelVisible = btnCancel;
            CancelCommand = new Command(() =>
            {
                Working = false;
            });
        }
    }
}