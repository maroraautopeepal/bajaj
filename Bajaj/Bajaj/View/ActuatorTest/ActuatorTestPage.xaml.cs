using Acr.UserDialogs;
using MultiEventController.Models;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Services;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Popup;
using Bajaj.Services;
using Bajaj.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.ActuatorTest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActuatorTestPage : ContentPage
    {
        ApiServices services;
        ActuatorTestViewModel viewModel;
        bool TestIsRunning = false;
        bool StopTimer = false;
        JobCardListModel jobCardSession;
        public ActuatorTestPage(JobCardListModel jobCardSession)
        {
            InitializeComponent();
            this.jobCardSession = jobCardSession;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send<ActuatorTestPage>(this, "StopMasterDetailSwip");

            App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;


            BindingContext = viewModel = new ActuatorTestViewModel();
            services = new ApiServices();
            //if (!CurrentUserEvent.Instance.IsExpert)
            //{

            GetActuatorTest(jobCardSession.model_id);

            StopTimer = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                viewModel.TimerVisible = !viewModel.TimerVisible;
                return StopTimer; // True = Repeat again, False = Stop the timer
            });
            //}
        }

        protected async override void OnDisappearing()
        {

            if (TestIsRunning)
            {
                MessagingCenter.Send<ActuatorTestPage>(this, "StartMasterDetailSwip");
                StopTimer = false;
                Console.WriteLine("Back button Press Started : Test Stopped");
                await btnWrite_Clicked(false);
                Console.WriteLine("Back button Pressed : Test Stopped");
                base.OnDisappearing();
            }

            if (CurrentUserEvent.Instance.IsExpert)
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = "GoBack",
                    ElementValue = "GoBack",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }

            App.controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
        }


        string[] PairedData = new string[2];
        private async void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {
            #region Check Internet Connection
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                await Task.Delay(100);
                bool InsternetActive = true;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    string data = (string)sender; //sender as string;
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (data.Contains("EcuList*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.EcuList = JsonConvert.DeserializeObject<ObservableCollection<EcuTestRoutine>>(PairedData[1]); ;
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("SelectedEcu*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.SelectedEcu = JsonConvert.DeserializeObject<EcuTestRoutine>(PairedData[1]); ;
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("SelectedNewEcu*#"))
                        {
                            PairedData = data.Split('#');

                            viewModel.SelectedEcu = JsonConvert.DeserializeObject<EcuTestRoutine>(PairedData[1]);

                            viewModel.PidList = new ObservableCollection<PidCode>();
                            foreach (var ecu in viewModel.EcuList.ToList())
                            {
                                if (viewModel.SelectedEcu.id == ecu.id)
                                {
                                    ecu.opacity = 1;
                                    viewModel.PidList = new ObservableCollection<PidCode>(ecu.pid_list.ToList());
                                }
                                else
                                {
                                    ecu.opacity = .5;
                                }
                            }

                        }
                        else if (data.Contains("PidList*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.PidList = JsonConvert.DeserializeObject<ObservableCollection<PidCode>>(PairedData[1]); ;
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("NoParameterSelect"))
                        {
                            var errorpage = new Popup.DisplayAlertPage("Alert", "Please select parameter", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                        else if (data.Contains("NoParameterSelect"))
                        {
                            var errorpage = new Popup.DisplayAlertPage("Alert", "Please select parameter", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }

                        else if (data.Contains("StopTest1"))
                        {
                            viewModel.BtnText = "Play";
                            viewModel.TxtAction = "Click play button to start the test";
                        }

                        else if (data.Contains("StopTest"))
                        {
                            //App.controlEventManager.SendRequestData("StopTest");
                            viewModel.LayoutVisible = false;
                            NavigationPage.SetHasBackButton(this, true);
                            //await btnWrite_Clicked(false);
                            //viewModel.BtnText = "Play1";
                            //viewModel.TxtAction = "Click play button to start the test";
                        }
                        else if (data.Contains("ERROR*#"))
                        {
                            PairedData = data.Split('#');
                            var errorpage = new Popup.DisplayAlertPage("Alert", PairedData[1], "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                        else if (data.Contains("RunningOnTest"))
                        {
                            //await DisplayAlert("Success", "Test is running on", "Ok");
                            var errorpage = new Popup.DisplayAlertPage("Success", "Test is running on", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                            viewModel.BtnText = "Stop";
                            viewModel.TxtAction = "Click stop button to stop the test";
                            viewModel.LayoutVisible = true;
                            NavigationPage.SetHasBackButton(this, false);
                            TestIsRunning = true;
                        }
                        else if (data.Contains("SuccessTest"))
                        {
                            var errorpage = new Popup.DisplayAlertPage("Success", "Test Completed", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                            viewModel.BtnText = "Play";
                            viewModel.TxtAction = "Click play button to start the test";
                            viewModel.LayoutVisible = false;
                            NavigationPage.SetHasBackButton(this, true);
                            TestIsRunning = false;
                        }
                        else if (data.Contains("FailledTest*#"))
                        {
                            PairedData = data.Split('#');
                            var errorpage = new Popup.DisplayAlertPage("Failed", $"Test Incompleted\n\nError : {PairedData[1]}", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                            viewModel.BtnText = "Play";
                            viewModel.TxtAction = "Click play button to start the test";
                            viewModel.LayoutVisible = false;
                            NavigationPage.SetHasBackButton(this, true);
                            TestIsRunning = false;
                        }
                        else if (data.Contains("FailledTest1*#"))
                        {
                            PairedData = data.Split('#');
                            var errorpage = new Popup.DisplayAlertPage("Failed", $"{PairedData[1]}", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                            //await DisplayAlert("Failed", $"Test Incompleted\n\nError : {item.Status}", "Ok");
                            viewModel.BtnText = "Play";
                            viewModel.TxtAction = "Click play button to start the test";
                            viewModel.LayoutVisible = false;
                            NavigationPage.SetHasBackButton(this, true);
                            TestIsRunning = false;
                        }
                        else if (data.Contains("Exception*#"))
                        {
                            PairedData = data.Split('#');
                            var errorpage = new Popup.DisplayAlertPage("Failed", PairedData[1], "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                        else if (data.Contains("ListNotFound*#"))
                        {
                            viewModel.ViewVisible = true;
                            viewModel.ShowError = false;
                            viewModel.ErrorText = string.Empty;
                        }
                        else if (data.Contains("ListFound*#"))
                        {
                            viewModel.ErrorText = "Actuator test not found.";
                            viewModel.ViewVisible = false;
                            viewModel.ShowError = true;
                        }
                        else if (data.Contains("Exception*#"))
                        {
                            PairedData = data.Split('#');
                            var errorpage = new Popup.DisplayAlertPage("Failed", PairedData[1], "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                        //else if (data.Contains("OnWriteFrame"))
                        //{
                        //    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                        //    {
                        //        ElementName = $"OnWriteFrame",
                        //        ElementValue = "OnWriteClicked",
                        //        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        //        IsExpert = CurrentUserEvent.Instance.IsExpert
                        //    });
                        //}
                    }
                    InsternetActive = false;
                });
            }
            #endregion
        }

        public string ReceiveValue = string.Empty;
        private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
        {
            var elementEventHandler = (sender as ElementEventHandler);
            this.ReceiveValue = elementEventHandler.ElementValue;
            if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectParameterClicked"))
            {
                viewModel.PidListVisible = true;
                viewModel.NewValue = string.Empty;
                viewModel.ShortNameVisible = viewModel.EnumrateDropDownVisible = viewModel.ManualEntryVisible = viewModel.BtnVisible = false;
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectEcuClicked"))
            {
                //App.controlEventManager.SendRequestData("SelectIorTest*#" + elementEventHandler.ElementName);
                viewModel.SelectedEcu = JsonConvert.DeserializeObject<EcuTestRoutine>(elementEventHandler.ElementName);

                SelectEcuClicked();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("HidePIdViewClicked"))
            {
                viewModel.EnumrateListVisible = viewModel.PidListVisible = false;
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectPidClicked"))
            {
                viewModel.SelectedPid = viewModel.PidList.FirstOrDefault(x => x.id == (Convert.ToInt32(elementEventHandler.ElementName)));
                if (viewModel.SelectedPid != null)
                {
                    SelectPidClicked();
                }
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("ShowEnumrateViewClicked"))
            {
                viewModel.PidListVisible = false;
                viewModel.EnumrateListVisible = true;
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectEnumrateClicked"))
            {
                viewModel.SelectedEnumrate = viewModel.EnumrateList.FirstOrDefault(x => x.message.Equals(elementEventHandler.ElementName));
                if (viewModel.SelectedEnumrate != null)
                {
                    SelectEnumrateClicked();
                }
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("NewTextValueChanged"))
            {
                viewModel.NewValue = elementEventHandler.ElementName;
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("PlayClicked"))
            {
                PlayClicked();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("StopClicked"))
            {
                await btnWrite_Clicked(false);
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("OnWriteClicked"))
            {
                await btnWrite_Clicked(true);
            }

            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }

        public async void GetActuatorTest(int model_id)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        if (!CurrentUserEvent.Instance.IsExpert)
                        {
                            int count = 0;
                            foreach (var ecu in StaticData.ecu_info.ToList())
                            {
                                count++;
                                var pid = await services.get_pid(App.JwtToken, ecu.pid_dataset_id);
                                var json = JsonConvert.SerializeObject(pid);

                                List<PidCode> pidCode = new List<PidCode>();
                                foreach (var item in pid.FirstOrDefault().codes)
                                {
                                    foreach (var item1 in item.pi_code_variable)
                                    {
                                        pidCode.Add(new PidCode
                                        {
                                            id = item1.id,
                                            code = item.code,
                                            short_name = item1.short_name,
                                            long_name = item1.long_name,
                                            total_len = item.total_len,
                                            byte_position = item1.byte_position,
                                            length = item1.length,
                                            bitcoded = item1.bitcoded,
                                            start_bit_position = item1.start_bit_position,
                                            end_bit_position = item1.end_bit_position,
                                            resolution = item1.resolution,
                                            offset = item1.offset,
                                            min = item1.min,
                                            max = item1.max,
                                            read = item.read,
                                            write = item.write,
                                            message_type = item1.message_type,
                                            unit = item1.unit,
                                            write_pid = item.write_pid,
                                            reset = item.reset,
                                            reset_value = item.reset_value,
                                            messages = item1.messages,
                                            io_ctrl = item.io_ctrl,
                                            io_ctrl_pid = item.io_ctrl_pid
                                        });
                                    }
                                }

                                if (count < 2)
                                {
                                    viewModel.EcuList.Add(
                                    new EcuTestRoutine
                                    {
                                        ecu_name = ecu.ecu_name,
                                        id = ecu.ecu_ID,
                                        opacity = 1,
                                        pid_list = pidCode.Where(x => x.io_ctrl).ToList()
                                    });
                                    viewModel.SelectedEcu = new EcuTestRoutine
                                    {
                                        ecu_name = ecu.ecu_name,
                                        id = ecu.ecu_ID,
                                        opacity = 1,
                                        pid_list = pidCode.Where(x => x.io_ctrl).ToList()
                                    };
                                    viewModel.PidList = new ObservableCollection<PidCode>(pidCode.Where(x => x.io_ctrl).ToList());
                                }
                                else
                                {
                                    viewModel.EcuList.Add(
                                    new EcuTestRoutine
                                    {
                                        ecu_name = ecu.ecu_name,
                                        id = ecu.ecu_ID,
                                        opacity = .5,
                                        pid_list = pidCode.Where(x => x.io_ctrl).ToList()
                                    });

                                    viewModel.SelectedEcu = viewModel.EcuList.FirstOrDefault();
                                    viewModel.SelectedEcu.opacity = 1;
                                    viewModel.PidList = new ObservableCollection<PidCode>(viewModel.SelectedEcu.pid_list.ToList());
                                }
                            }

                            if (viewModel.PidList == null || (!viewModel.PidList.Any()))
                            {
                                viewModel.ViewVisible = false;
                                viewModel.ShowError = true;
                                viewModel.ErrorText = "Actuator test not found.";
                                App.controlEventManager.SendRequestData("ListFound*#" );
                            }
                            else
                            {
                                viewModel.ViewVisible = true;
                                viewModel.ShowError = false;
                                viewModel.ErrorText = string.Empty;
                                App.controlEventManager.SendRequestData("ListNotFound*#");
                            }

                            App.controlEventManager.SendRequestData("EcuList*#" + JsonConvert.SerializeObject(viewModel.EcuList));
                            App.controlEventManager.SendRequestData("SelectedEcu*#" + JsonConvert.SerializeObject(viewModel.SelectedEcu));
                            App.controlEventManager.SendRequestData("PidList*#" + JsonConvert.SerializeObject(viewModel.PidList));
                        }
                        else
                        {
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();

                            DependencyService.Get<ILodingPageService>().InitLoadingPage(new LoadingIndicatorPage());
                            DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                            await Task.Delay(200);
                        }
                    }
                });
            }
            catch (System.Exception ex)
            {
                viewModel.ViewVisible = true;
                viewModel.ShowError = false;
                viewModel.ErrorText = ex.Message;
            }
        }

        private async void SelectEcuClicked(object sender, System.EventArgs e)
        {
            try
            {
                viewModel.SelectedEcu = (EcuTestRoutine)((Button)sender).BindingContext;

                if (viewModel.SelectedEcu == null)
                    return;

                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = JsonConvert.SerializeObject(viewModel.SelectedEcu),
                        ElementValue = "SelectEcuClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                SelectEcuClicked();
            }
            catch (System.Exception ex)
            {
            }
        }

        private void SelectEcuClicked()
        {
            try
            {
                viewModel.PidList = new ObservableCollection<PidCode>();
                foreach (var ecu in viewModel.EcuList.ToList())
                {
                    if (viewModel.SelectedEcu.id == ecu.id)
                    {
                        ecu.opacity = 1;
                        viewModel.PidList = new ObservableCollection<PidCode>(ecu.pid_list.ToList());
                    }
                    else
                    {
                        ecu.opacity = .5;
                    }
                }
            }
            catch (System.Exception ex)
            {
            }
        }

        private async void HidePIdViewClicked(object sender, System.EventArgs e)
        {
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                #region Check Internet Connection
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "HidePIdViewFrame",
                        ElementValue = "HidePIdViewClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                #endregion
            }
            viewModel.EnumrateListVisible = viewModel.PidListVisible = false;
        }

        protected override bool OnBackButtonPressed()
        {
            if (viewModel.LayoutVisible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void ShowPidViewClicked(object sender, System.EventArgs e)
        {

            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                #region Check Internet Connection
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "SelectParameterFrame",
                        ElementValue = "SelectParameterClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                #endregion
            }

            viewModel.PidListVisible = true;
            viewModel.NewValue = string.Empty;
            viewModel.ShortNameVisible = viewModel.EnumrateDropDownVisible = viewModel.ManualEntryVisible = viewModel.BtnVisible = false;
        }

        private async void SelectPidClicked(object sender, System.EventArgs e)
        {
            try
            {
                viewModel.SelectedPid = (PidCode)((Grid)sender).BindingContext;
                if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
                {
                    #region Check Internet Connection
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                        {
                            ElementName = $"{viewModel.SelectedPid.id}",
                            ElementValue = "SelectPidClicked",
                            ToUserId = CurrentUserEvent.Instance.ToUserId,
                            IsExpert = CurrentUserEvent.Instance.IsExpert
                        });
                    }
                    #endregion
                }

                SelectPidClicked();
            }
            catch (Exception ex)
            {
            }
        }

        public void SelectPidClicked()
        {
            try
            {


                if (viewModel.SelectedPid == null)
                    return;

                if (viewModel.SelectedPid.message_type == "ENUMRATED")
                {
                    viewModel.EnumrateList = new ObservableCollection<Message>();
                    viewModel.EnumrateDropDownVisible = true;
                    viewModel.ManualEntryVisible = false;
                    viewModel.EnumrateList = new ObservableCollection<Message>(viewModel.SelectedPid.messages.ToList());
                    viewModel.BtnVisible = false;
                }
                else
                {
                    viewModel.EnumrateDropDownVisible = false;
                    viewModel.ManualEntryVisible = true;
                    viewModel.BtnVisible = true;
                    viewModel.TxtPlaceholder = $"Enter the value in {viewModel.SelectedPid.unit} between {viewModel.SelectedPid.min} to {viewModel.SelectedPid.max}";
                }
                viewModel.ShortNameVisible = true;
                viewModel.PidListVisible = false;
                viewModel.NewValue = string.Empty;
            }
            catch (System.Exception ex)
            {
            }
        }

        private async void SelectEnumrateClicked(object sender, System.EventArgs e)
        {
            try
            {
                viewModel.SelectedEnumrate = (Message)((Grid)sender).BindingContext;

                if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
                {
                    #region Check Internet Connection
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                        {
                            ElementName = $"{viewModel.SelectedEnumrate.message}",
                            ElementValue = "SelectEnumrateClicked",
                            ToUserId = CurrentUserEvent.Instance.ToUserId,
                            IsExpert = CurrentUserEvent.Instance.IsExpert
                        });
                    }
                    #endregion
                }

                SelectEnumrateClicked();
            }
            catch (System.Exception ex)
            {
            }
        }

        public void SelectEnumrateClicked()
        {
            if (viewModel.SelectedEnumrate == null)
                return;

            viewModel.BtnVisible = true;
            viewModel.NewValue = viewModel.SelectedEnumrate.code;
            viewModel.EnumrateListVisible = false;
        }

        private async void ShowEnumrateViewClicked(object sender, System.EventArgs e)
        {
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                #region Check Internet Connection
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"ShowEnumrateViewFrame",
                        ElementValue = "ShowEnumrateViewClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                #endregion
            }

            viewModel.PidListVisible = false;
            viewModel.EnumrateListVisible = true;
        }

        private async void PlayClicked(object sender, System.EventArgs e)
        {
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                #region Check Internet Connection
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"PlayFrame",
                        ElementValue = "PlayClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                #endregion
            }
            else
            {
                PlayClicked();
            }
        }

        private async void PlayClicked()
        {
            if (viewModel.NewValue == string.Empty)
            {
                App.controlEventManager.SendRequestData("NoParameterSelect");
                var errorpage = new Popup.DisplayAlertPage("Alert", "Please select parameter", "OK");
                await PopupNavigation.Instance.PushAsync(errorpage);
            }
            else
            {
                if (viewModel.BtnText == "Play")
                {
                    if (!CurrentUserEvent.Instance.IsExpert)
                    {
                        await btnWrite_Clicked(true);
                    }
                }
                else
                {
                    if (!CurrentUserEvent.Instance.IsExpert)
                    {
                        App.controlEventManager.SendRequestData("StopTest");
                        viewModel.LayoutVisible = false;
                        NavigationPage.SetHasBackButton(this, true);
                        await btnWrite_Clicked(false);
                        App.controlEventManager.SendRequestData("StopTest1");
                        viewModel.BtnText = "Play";
                        viewModel.TxtAction = "Click play button to start the test";
                    }

                }
            }
        }

        private async Task btnWrite_Clicked(bool IsPlay)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(200);
                        ObservableCollection<WriteParameterPID> pidList = new ObservableCollection<WriteParameterPID>();
                        ObservableCollection<WriteParameter_Status> Parameter_Status = new ObservableCollection<WriteParameter_Status>();

                        byte[] data_bytes = null;

                        if (IsPlay)
                        {

                            if (viewModel.SelectedPid.min != null && viewModel.SelectedPid.max != null)
                            {
                                if (viewModel.SelectedPid.min > Convert.ToDouble(viewModel.NewValue)
                                    || viewModel.SelectedPid.max < Convert.ToDouble(viewModel.NewValue))
                                {

                                    App.controlEventManager.SendRequestData($"ERROR*#Enter the value between {viewModel.SelectedPid.min} to {viewModel.SelectedPid.max}");

                                    var errorpage = new Popup.DisplayAlertPage("Alert", $"Enter the value between {viewModel.SelectedPid.min} to {viewModel.SelectedPid.max}", "OK");
                                    await PopupNavigation.Instance.PushAsync(errorpage);
                                    return;
                                }
                            }

                            string outputHex = int.Parse(viewModel.NewValue.ToUpper()).ToString("X");
                            byte[] writeinput = HexStringToByteArray(outputHex);

                            if (writeinput.Length < 2)
                            {
                                data_bytes = new byte[2];
                                data_bytes[0] = 0x00;
                                data_bytes[1] = writeinput[0];
                            }
                            else
                            {
                                data_bytes = new byte[writeinput.Length];
                                Array.Copy(writeinput, 0, data_bytes, 1 - 1, writeinput.Length);
                            }
                        }
                        else
                        {
                            data_bytes = new byte[0];
                        }

                        var model_detail = StaticData.ecu_info.FirstOrDefault(x => x.ecu_ID == viewModel.SelectedEcu.id);

                        pidList.Add(
                               new WriteParameterPID
                               {
                                   seedkeyindex = model_detail.seed_key_index.value,
                                   writepamindex = model_detail.ior_test_fn_index,
                                   writeparadata = data_bytes,
                                   writeparadatasize = viewModel.SelectedPid.total_len,
                                   writeparapid = viewModel.SelectedPid.io_ctrl_pid,
                                   ReadParameterPID_DataType = viewModel.SelectedPid.message_type,

                                   pid = viewModel.SelectedPid.write_pid,
                                   startByte = viewModel.SelectedPid.byte_position,
                                   totalBytes = viewModel.SelectedPid.total_len
                               }
                               );

                        switch (Device.RuntimePlatform)
                        {
                            case Device.iOS:
                                break;
                            case Device.Android:
                                if (App.ConnectedVia == "USB")
                                {
                                    Parameter_Status = await DependencyService.Get<Interfaces.IConnectionUSB>().WriteAtuatorTest(model_detail.ior_test_fn_index, pidList, IsPlay);
                                }
                                else if (App.ConnectedVia == "BT")
                                {
                                    Parameter_Status = await DependencyService.Get<Interfaces.IBth>().WriteAtuatorTest(model_detail.ior_test_fn_index, pidList, IsPlay);
                                }
                                else
                                {
                                    Parameter_Status = await DependencyService.Get<Interfaces.IConnectionWifi>().WriteAtuatorTest(model_detail.ior_test_fn_index, pidList, IsPlay);
                                }
                                break;
                            case Device.UWP:
                                Parameter_Status = await DependencyService.Get<Interfaces.IConnectionUSB>().WritePid(model_detail.write_pid_index, pidList);
                                break;
                            default:
                                break;
                        }

                        var PidWriteRecord = new List<pid_write_record>();

                        if (Parameter_Status.FirstOrDefault()?.Status == "NOERROR")
                        {
                            foreach (var item in Parameter_Status)
                            {
                                if (item.Status == "NOERROR")
                                {
                                    if (IsPlay)
                                    {

                                        App.controlEventManager.SendRequestData($"RunningOnTest");

                                        var errorpage = new Popup.DisplayAlertPage("Success", "Test is running on", "OK");
                                        await PopupNavigation.Instance.PushAsync(errorpage);
                                        viewModel.BtnText = "Stop";
                                        viewModel.TxtAction = "Click stop button to stop the test";
                                        viewModel.LayoutVisible = true;
                                        NavigationPage.SetHasBackButton(this, false);
                                        TestIsRunning = true;
                                    }
                                    else
                                    {

                                        App.controlEventManager.SendRequestData($"SuccessTest");

                                        var errorpage = new Popup.DisplayAlertPage("Success", "Test Completed", "OK");
                                        await PopupNavigation.Instance.PushAsync(errorpage);
                                        viewModel.BtnText = "Play";
                                        viewModel.TxtAction = "Click play button to start the test";
                                        viewModel.LayoutVisible = false;
                                        NavigationPage.SetHasBackButton(this, true);
                                        TestIsRunning = false;
                                    }
                                }
                                else
                                {
                                    App.controlEventManager.SendRequestData($"FailledTest*#{item.Status}");

                                    var errorpage = new Popup.DisplayAlertPage("Failed", $"Test Incompleted\n\nError : {item.Status}", "OK");
                                    await PopupNavigation.Instance.PushAsync(errorpage);
                                    viewModel.BtnText = "Play";
                                    viewModel.TxtAction = "Click play button to start the test";
                                    viewModel.LayoutVisible = false;
                                    NavigationPage.SetHasBackButton(this, true);
                                    TestIsRunning = false;
                                }
                            }
                            if (PidWriteRecord.Count != 0)
                            {
                            }
                        }
                        else
                        {
                            if (!CurrentUserEvent.Instance.IsExpert)
                            {
                                App.controlEventManager.SendRequestData($"FailledTest1*#Test Inompleted\n\nError : {Parameter_Status.FirstOrDefault()?.Status}");
                                var errorpage = new Popup.DisplayAlertPage("Failed", $"Test Inompleted\n\nError : {Parameter_Status.FirstOrDefault()?.Status}", "OK");
                                await PopupNavigation.Instance.PushAsync(errorpage);
                                //await DisplayAlert("Failed", $"Test Incompleted\n\nError : {item.Status}", "Ok");
                                viewModel.BtnText = "Play";
                                viewModel.TxtAction = "Click play button to start the test";
                                viewModel.LayoutVisible = false;
                                NavigationPage.SetHasBackButton(this, true);
                                TestIsRunning = false;
                            }
                        }
                    }
                });
            }

            catch (Exception ex)
            {

                App.controlEventManager.SendRequestData($"Exception*#{ex.Message}");

                var errorpage = new Popup.DisplayAlertPage("Failed", ex.Message, "OK");
                await PopupNavigation.Instance.PushAsync(errorpage);
            }
        }

        private byte[] HexStringToByteArray(String hex)
        {
            hex = hex.Replace(" ", "");
            int numberChars = hex.Length;
            if (numberChars % 2 != 0)
            {
                hex = "0" + hex;
                numberChars++;
            }
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private void NewValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = viewModel.NewValue,
                    ElementValue = "NewTextValueChanged",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }
        }
    }
}