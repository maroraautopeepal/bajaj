using Bajaj.Model;
using Bajaj.View.ViewModel;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Bajaj.Interfaces;
using Newtonsoft.Json;
using System.Diagnostics;
using Acr.UserDialogs;
using MultiEventController.Models;
using Bajaj.Popup;

namespace Bajaj.View.IorTest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IorTestPlayPage : ContentPage
    {
        ObservableCollection<ReadPidPresponseModel> read_pid_android;
        TestRoutineResponseModel routineResponse;
        IorTestPlayViewModel viewModel;
        IorResult iorResult;
        Stopwatch stopwatch;
        double deci;
        int ecu_id;
        string SeedIndex;
        string WriteFnIndex;
        bool StopTimer1 = false;

        bool IsRequestId = false;
        public IorTestPlayPage(IorResult iorResult, string SeedIndex, string WriteFnIndex, int ecu_id)
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new IorTestPlayViewModel(iorResult);
                routineResponse = new TestRoutineResponseModel();
                read_pid_android = new ObservableCollection<ReadPidPresponseModel>();
                this.ecu_id = ecu_id;
                this.iorResult = iorResult;
                this.SeedIndex = SeedIndex;
                this.WriteFnIndex = WriteFnIndex;
                stopwatch = new Stopwatch();

                StopTimer1 = true;
                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    viewModel.Timer_Visible = !viewModel.Timer_Visible;
                    return StopTimer1; // True = Repeat again, False = Stop the timer
                });
            }
            catch (Exception ex)
            {
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                MessagingCenter.Send<IorTestPlayPage>(this, "StopMasterDetailSwip");
                App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
                App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;

                if (!CurrentUserEvent.Instance.IsExpert)
                {
                    
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            break;
                        case Device.Android:
                            if (App.ConnectedVia == "USB")
                            {
                                DependencyService.Get<IConnectionUSB>().StartTesterPresent();
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                DependencyService.Get<Interfaces.IBth>().StartTesterPresent();
                                //Parameter_Status = await DependencyService.Get<Interfaces.IBth>().WriteAtuatorTest(model_detail.write_pid_index, pidList);
                            }
                            else
                            {
                                DependencyService.Get<Interfaces.IConnectionWifi>().StartTesterPresent();
                            }
                            break;
                        default:
                            break;
                    }
                    GetPidValues();

                }
            }
            catch (Exception ex)
            {
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Send<IorTestPlayPage>(this, "StartMasterDetailSwip");
            StopTimer1 = false;
            IsRequestId = false;
            viewModel.SelectedIorTest.btn_activation_status = true;
            viewModel.SelectedIorTest.btn_visible = true;
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

            if (!CurrentUserEvent.Instance.IsExpert)
            {
                if (!CurrentUserEvent.Instance.IsExpert)
                {
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            break;
                        case Device.Android:
                            if (App.ConnectedVia == "USB")
                            {
                                DependencyService.Get<IConnectionUSB>().StopTesterPresent();
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                DependencyService.Get<Interfaces.IBth>().StopTesterPresent();
                                //Parameter_Status = await DependencyService.Get<Interfaces.IBth>().WriteAtuatorTest(model_detail.write_pid_index, pidList);
                            }
                            else
                            {
                                DependencyService.Get<Interfaces.IConnectionWifi>().StopTesterPresent();
                            }
                            break;
                        default:
                            break;
                    }

                }

            }
        }

        string[] PairedData = new string[2];
        private async void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {
            #region Check Internet Connection
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                await Task.Delay(100);
                // bool InsternetActive = true;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        string data = (string)sender; //sender as string;
                        if (!string.IsNullOrEmpty(data))
                        {
                            if (data.Contains("btn_activation_status"))
                            {
                                viewModel.SelectedIorTest.btn_activation_status = false;
                            }
                            else if (data.Contains("StopTest"))
                            {
                                viewModel.SelectedIorTest.is_play = StopTimer = false;
                                viewModel.SelectedIorTest.image_source = "ic_play.png";
                                viewModel.SelectedIorTest.test_status = "Test Status : Stopped";
                                viewModel.TxtAction = "Click play button to start the test";
                                viewModel.LayoutVisible = viewModel.TimerVisible = viewModel.Timer_Visible = false;
                                NavigationPage.SetHasBackButton(this, true);
                            }
                            else if (data.Contains("SetHasBackButton"))
                            {
                                NavigationPage.SetHasBackButton(this, false);
                            }
                            else if (data.Contains("TimerVisibleTrue"))
                            {
                                viewModel.TimerVisible = true;
                            }
                            else if (data.Contains("ButtonStatus"))
                            {
                                viewModel.SelectedIorTest.btn_activation_status = false;
                                viewModel.SelectedIorTest.btn_background_color = "FEC0C0";
                            }
                            else if (data.Contains("BtnPlayFalse"))
                            {
                                viewModel.SelectedIorTest.btn_visible = false;
                            }
                            else if (data.Contains("TimerValue*#"))
                            {
                                PairedData = data.Split('#');
                                viewModel.Timer = PairedData[1];
                            }
                            else if (data.Contains("TimerVisibleFalse"))
                            {
                                viewModel.TimerVisible = false;
                            }
                            else if (data.Contains("NOERROR"))
                            {
                                viewModel.SelectedIorTest.btn_activation_status = viewModel.SelectedIorTest.is_play = true;
                                viewModel.SelectedIorTest.image_source = "ic_test_stop.png";
                                viewModel.SelectedIorTest.test_status = "Test Status : Running";
                                viewModel.TxtAction = "Click  stop button to stop the test";
                                viewModel.LayoutVisible = viewModel.TimerVisible = viewModel.Timer_Visible = true;
                            }
                            else if (data.Contains("StoppedStatus*#"))
                            {
                                PairedData = data.Split('#');
                                viewModel.SelectedIorTest.btn_activation_status = true;
                                viewModel.SelectedIorTest.btn_background_color = "e62f31";
                                viewModel.SelectedIorTest.is_play = false;
                                viewModel.SelectedIorTest.image_source = "ic_play.png";
                                viewModel.SelectedIorTest.test_status = "Test Status : Stopped";
                                StopTimer = false;
                                viewModel.LayoutVisible = viewModel.TimerVisible = viewModel.Timer_Visible = false;
                                NavigationPage.SetHasBackButton(this, true);
                                viewModel.TxtAction = "Click play button to start the test";
                                if (PairedData[1] == "NOERROR")
                                {
                                    await UserDialogs.Instance.AlertAsync("Test Stopped", "", "Ok");
                                }
                                else
                                {
                                    await UserDialogs.Instance.AlertAsync(PairedData[1], "", "Ok");
                                }
                            }
                            else if (data.Contains("TestNotRunning"))
                            {
                                await UserDialogs.Instance.AlertAsync("Test not running", "ERROR", "Ok");
                            }
                            else if (data.Contains("Test_Stopped"))
                            {
                                viewModel.SelectedIorTest.btn_activation_status = true;
                                viewModel.SelectedIorTest.btn_background_color = "e62f31";
                                viewModel.SelectedIorTest.is_play = false;
                                viewModel.SelectedIorTest.image_source = "ic_play.png";
                                viewModel.SelectedIorTest.test_status = "Test Status : Stopped";
                                viewModel.TxtAction = "Click play button to start the test";
                                StopTimer = false;
                                viewModel.LayoutVisible = viewModel.TimerVisible = viewModel.Timer_Visible = false;
                                NavigationPage.SetHasBackButton(this, true);
                                await UserDialogs.Instance.AlertAsync("Test Stopped", "", "Ok");
                            }
                            else if (data.Contains("TestStopped*#"))
                            {
                                await UserDialogs.Instance.AlertAsync("Test Stopped", "ERROR", "Ok");
                            }
                            else if (data.Contains("Exception*#"))
                            {
                                PairedData = data.Split('#');
                                await DisplayAlert("Alert", PairedData[1], "Ok");
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        //InsternetActive = false;
                    }
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
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("TestActionClicked"))
            {
                viewModel.SelectedIorTest = JsonConvert.DeserializeObject<IorTestRoutine>(elementEventHandler.ElementName);
                TestActionClicked();
            }

            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }

        bool StopTimer = false;
        int Seconds = 0;
        int TimeDuration = 0;
        bool test_condition = false;
        List<string> active_byte_list = new List<string>();

        private async void TestActionClicked(object sender, EventArgs e)
        {
            try
            {
                viewModel.SelectedIorTest = (IorTestRoutine)((ImageButton)sender).BindingContext;

                if (viewModel.SelectedIorTest == null)
                    return;

                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = JsonConvert.SerializeObject(viewModel.SelectedIorTest),
                        ElementValue = "TestActionClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }

                TestActionClicked();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", ex.Message + "\n\n" + ex.StackTrace, "Ok");
            }
        }

        //int time_base =0;
        private async void TestActionClicked()
        {
            try
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                //await  Task.Run(async () =>
                {
                    try
                    {
                        TimeDuration = 0;

                        if (!CurrentUserEvent.Instance.IsExpert)
                        {
                            App.controlEventManager.SendRequestData("btn_activation_status");
                        }
                        viewModel.SelectedIorTest.btn_activation_status = false;


                        foreach (var active_byte in iorResult.ior_test_routine_type.status_byte_definition)
                        {
                            if (active_byte.byte_definations == "TestActive")
                            {
                                active_byte_list.Add(active_byte.@byte);
                            }
                        }

                        if (viewModel.SelectedIorTest.is_play)
                        {
                            if (!CurrentUserEvent.Instance.IsExpert)
                            {
                                App.controlEventManager.SendRequestData("StopTest");
                            }

                            viewModel.SelectedIorTest.is_play = StopTimer = false;
                            //viewModel.SelectedIorTest.image_source = "ic_play.png";
                            //viewModel.SelectedIorTest.test_status = "Test Status : Stopped";
                            //viewModel.TxtAction = "Click play button to start the test";
                            //viewModel.LayoutVisible = viewModel.TimerVisible = viewModel.Timer_Visible = false;
                            //NavigationPage.SetHasBackButton(this, true);




                            #region Stop Routine Method
                            IsRequestId = false;

                            await Task.Run(async () =>
                           {
                               routineResponse = await StopRoutine(viewModel.SelectedIorTest.stop_followup_routine_id);
                           });
                            
                            if (!CurrentUserEvent.Instance.IsExpert)
                            {
                                App.controlEventManager.SendRequestData("Test_Stopped");
                            }
                            viewModel.SelectedIorTest.btn_activation_status = true;
                            viewModel.SelectedIorTest.btn_background_color = "e62f31";
                            viewModel.SelectedIorTest.is_play = false;
                            viewModel.SelectedIorTest.image_source = "ic_play.png";
                            viewModel.SelectedIorTest.test_status = "Test Status : Stopped";
                            viewModel.TxtAction = "Click play button to start the test";
                            StopTimer = false;
                            viewModel.LayoutVisible = viewModel.TimerVisible = viewModel.Timer_Visible = false;
                            NavigationPage.SetHasBackButton(this, true);
                            await UserDialogs.Instance.AlertAsync("Test Stopped", "", "Ok");

                            #endregion

                        }
                        else
                        {
                            if (!CurrentUserEvent.Instance.IsExpert)
                            {
                                App.controlEventManager.SendRequestData("SetHasBackButton");
                            }
                            NavigationPage.SetHasBackButton(this, false);

                            if (!string.IsNullOrEmpty(iorResult.ior_test_routine_type.test_routine_type))
                            {

                                if (iorResult.ior_test_routine_type.test_routine_type.Contains("TimeBased"))
                                {
                                    if (!CurrentUserEvent.Instance.IsExpert)
                                    {
                                        App.controlEventManager.SendRequestData("TimerVisibleTrue");
                                    }
                                    viewModel.TimerVisible = true;
                                    StopTimer = true;

                                    stopwatch.Reset();
                                    stopwatch.Start();
                                    Seconds = 0;
                                    TimeDuration = Convert.ToInt32(iorResult.ior_test_routine_type.activation_time) + 1;
                                    //if (string.IsNullOrEmpty(viewModel.SelectedIorTest.stop_followup_routine_id))
                                    if (string.IsNullOrEmpty(viewModel.SelectedIorTest.stop_followup_routine_id))
                                    {
                                        if (!CurrentUserEvent.Instance.IsExpert)
                                        {
                                            App.controlEventManager.SendRequestData("BtnPlayFalse");
                                        }
                                        viewModel.SelectedIorTest.btn_visible = false;
                                    }

                                    #region Routine Timer
                                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                                    {
                                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                                        {
                                            viewModel.Timer = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch
                                           .Elapsed.Seconds.ToString("00");
                                            if (!CurrentUserEvent.Instance.IsExpert)
                                            {
                                                App.controlEventManager.SendRequestData($"TimerValue*#{viewModel.Timer}");
                                            }
                                            return StopTimer;
                                        });
                                    });
                                    #endregion
                                }
                                else
                                {
                                    if (!CurrentUserEvent.Instance.IsExpert)
                                    {
                                        App.controlEventManager.SendRequestData("TimerVisibleFalse");
                                    }
                                    viewModel.TimerVisible = false;
                                }

                                #region Start Routine Method
                                using (UserDialogs.Instance.Loading("Test Starting...", null, null, true, MaskType.Black))
                                {
                                    await Task.Delay(200);

                                    if (string.IsNullOrEmpty(viewModel.SelectedIorTest.stop_followup_routine_id))
                                    {
                                        if (!CurrentUserEvent.Instance.IsExpert)
                                        {
                                            App.controlEventManager.SendRequestData("BtnPlayFalse");
                                        }
                                        viewModel.SelectedIorTest.btn_visible = false;
                                    }

                                    routineResponse = await StartRoutine(viewModel.SelectedIorTest.start_routine_id);
                                }
                                #endregion

                                if (!string.IsNullOrEmpty(routineResponse.ECUResponseStatus))
                                {
                                    if (routineResponse.ECUResponseStatus == "NOERROR" || routineResponse.ECUResponseStatus != "NOERROR")
                                    {
                                        if (!CurrentUserEvent.Instance.IsExpert)
                                        {
                                            App.controlEventManager.SendRequestData("NOERROR");
                                        }
                                        viewModel.SelectedIorTest.btn_activation_status = viewModel.SelectedIorTest.is_play = true;
                                        viewModel.SelectedIorTest.image_source = "ic_test_stop.png";
                                        viewModel.SelectedIorTest.test_status = "Test Status : Running";
                                        viewModel.TxtAction = "Click  stop button to stop the test";
                                        viewModel.LayoutVisible = viewModel.TimerVisible = viewModel.Timer_Visible = true;
                                        IsRequestId = true;
                                        if (!string.IsNullOrEmpty(viewModel.SelectedIorTest.req_routine_id))
                                        {
                                            Device.BeginInvokeOnMainThread(() =>
                                           {
                                               Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                                               {

                                                   TimeDuration -= 1;
                                                   //Console.WriteLine($"Count : {TimeDuration}");
                                                   if ((TimeDuration < 1) && test_condition)
                                                   {
                                                       //Console.WriteLine($"Condition True : {time_base}");
                                                       IsRequestId = StopTimer = test_condition = false;
                                                       Debug.WriteLine("-------STOP COMMAND SENDING-------");

                                                       var re = StopRoutine(viewModel.SelectedIorTest.stop_followup_routine_id);
                                                       if (re.Result.ECUResponseStatus == "NOERROR")
                                                       {
                                                           re.Result.ECUResponseStatus = "Test Stopped";
                                                       }
                                                       //responseArrayStatus = this.dongleCommWin.CAN_TxRx(stop_command.Length / 2, stop_command).Result;
                                                       //if (responseArrayStatus.ECUResponseStatus == "NOERROR")
                                                       //{
                                                       //    responseArrayStatus.ECUResponseStatus = "Test Stopped";
                                                       //}
                                                   }
                                                   return StopTimer;
                                               });
                                           });
                                            while (IsRequestId)
                                            {
                                                routineResponse = await CountinueRoutine(viewModel.SelectedIorTest.req_routine_id);
                                                await GetPidsValue();
                                            }
                                        }
                                        else
                                        {
                                            while (IsRequestId)
                                            {
                                                await GetPidsValue();
                                            }
                                        }
                                    }
                                    if (routineResponse != null)
                                    {
                                        if (!string.IsNullOrEmpty(routineResponse.ECUResponseStatus))
                                        {
                                            if (!string.IsNullOrEmpty(viewModel.SelectedIorTest.req_routine_id))
                                            {
                                                if (!CurrentUserEvent.Instance.IsExpert)
                                                {
                                                    App.controlEventManager.SendRequestData($"StoppedStatus*#{routineResponse.ECUResponseStatus}");
                                                }
                                                viewModel.SelectedIorTest.btn_activation_status = true;
                                                viewModel.SelectedIorTest.btn_background_color = "e62f31";
                                                viewModel.SelectedIorTest.is_play = false;
                                                viewModel.SelectedIorTest.image_source = "ic_play.png";
                                                viewModel.SelectedIorTest.test_status = "Test Status : Stopped";
                                                StopTimer = false;
                                                viewModel.LayoutVisible = viewModel.TimerVisible = viewModel.Timer_Visible = false;
                                                NavigationPage.SetHasBackButton(this, true);
                                                viewModel.TxtAction = "Click play button to start the test";
                                                if (routineResponse.ECUResponseStatus == "NOERROR")
                                                {
                                                    //await UserDialogs.Instance.AlertAsync("Test Stopped", "", "Ok");
                                                }
                                                else
                                                {
                                                    await UserDialogs.Instance.AlertAsync(routineResponse.ECUResponseStatus, "", "Ok");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!CurrentUserEvent.Instance.IsExpert)
                                            {
                                                App.controlEventManager.SendRequestData($"TestNotRunning");
                                            }
                                            await UserDialogs.Instance.AlertAsync("Test not running", "ERROR", "Ok");
                                        }
                                    }
                                    else
                                    {
                                        if (!CurrentUserEvent.Instance.IsExpert)
                                        {
                                            App.controlEventManager.SendRequestData("Test_Stopped");
                                        }
                                        viewModel.SelectedIorTest.btn_activation_status = true;
                                        viewModel.SelectedIorTest.btn_background_color = "e62f31";
                                        viewModel.SelectedIorTest.is_play = false;
                                        viewModel.SelectedIorTest.image_source = "ic_play.png";
                                        viewModel.SelectedIorTest.test_status = "Test Status : Stopped";
                                        viewModel.TxtAction = "Click play button to start the test";
                                        StopTimer = false;
                                        viewModel.LayoutVisible = viewModel.TimerVisible = viewModel.Timer_Visible = false;
                                        NavigationPage.SetHasBackButton(this, true);
                                        await UserDialogs.Instance.AlertAsync("Test Stopped", "", "Ok");
                                    }
                                }
                                else
                                {
                                    if (!CurrentUserEvent.Instance.IsExpert)
                                    {
                                        App.controlEventManager.SendRequestData("TestStopped*#");
                                    }
                                    await UserDialogs.Instance.AlertAsync("Test Stopped", "ERROR", "Ok");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                });
            }
            catch (Exception ex)
            {
                if (!CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestData($"Exception*#{ex.Message}");
                }
                await DisplayAlert("Alert", ex.Message, "Ok");
            }
        }


        public async Task<TestRoutineResponseModel> StartRoutine(string start_command)
        {
            var response = new TestRoutineResponseModel();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    break;
                case Device.Android:
                    if (App.ConnectedVia == "USB")
                    {
                        response = await DependencyService.Get<IConnectionUSB>().SetTestRoutineCommand(SeedIndex, WriteFnIndex, start_command);
                    }
                    else if (App.ConnectedVia == "BT")
                    {
                        response = await DependencyService.Get<IBth>().SetTestRoutineCommand(SeedIndex, WriteFnIndex, start_command);
                        //Parameter_Status = await DependencyService.Get<Interfaces.IBth>().WriteAtuatorTest(model_detail.write_pid_index, pidList);
                    }
                    else
                    {
                        response = await DependencyService.Get<IConnectionWifi>().SetTestRoutineCommand(SeedIndex, WriteFnIndex, start_command);
                    }
                    break;
                default:
                    break;
            }

            //var response = await DependencyService.Get<IConnectionWifi>().SetTestRoutineCommand(SeedIndex, WriteFnIndex, start_command);
            Console.WriteLine("Routine Started");
            return response;
        }

        int bit_position = 0;
        string complete_command = string.Empty;
        string fail_command = string.Empty;
        bool active_con = false;
        public void WhileMethod(TestRoutineResponseModel testRoutineResponseModel)
        {
            bit_position = Convert.ToInt32(iorResult.ior_test_routine_type.status_byte_loc);
            fail_command = iorResult.ior_test_routine_type.status_byte_definition.FirstOrDefault(x => x.byte_definations == "TestAborted").@byte;
            complete_command = iorResult.ior_test_routine_type.status_byte_definition.FirstOrDefault(x => x.byte_definations == "TestComplete").@byte;
            if (testRoutineResponseModel.ECUResponseStatus == "NOERROR")
            {
                var pos = (byte)(testRoutineResponseModel.ActualDataBytes[bit_position - 1]);
                var com_byte = HexStringToByteArray(complete_command)[0];
                active_con = false;
                foreach (var active in active_byte_list.ToList())
                {
                    var active_command1 = HexStringToByteArray(active)[0];
                    if (testRoutineResponseModel.ActualDataBytes[bit_position - 1] == HexStringToByteArray(active)[0])
                    {
                        active_con = true;
                    }

                    Console.WriteLine($"Compare Byte = {pos} - {com_byte} - {active_command1}");
                }
                //var active_command1 = HexStringToByteArray(active_command)[0];

                //Console.WriteLine($"Compare Byte = {pos} - {com_byte} - {active_command1}");


                if (!active_con)
                {
                    //test_condition = false;
                    if (testRoutineResponseModel.ActualDataBytes[bit_position - 1] == HexStringToByteArray(complete_command)[0])
                    {
                        if (testRoutineResponseModel.ECUResponseStatus == "NOERROR")
                        {
                            testRoutineResponseModel.ECUResponseStatus = "Test Completed";
                        }
                    }

                    else if (testRoutineResponseModel.ActualDataBytes[bit_position - 1] == HexStringToByteArray(fail_command)[0])
                    {
                        if (testRoutineResponseModel.ECUResponseStatus == "NOERROR")
                        {
                            testRoutineResponseModel.ECUResponseStatus = "Test Aborted";
                        }
                    }
                    else
                    {

                    }
                    //test_condition = StopTimer = false;
                }
            }
            else
            {
                //test_condition = StopTimer = false;
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

        public async Task<TestRoutineResponseModel> CountinueRoutine(string request_command)
        {
            var response = new TestRoutineResponseModel();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    break;
                case Device.Android:
                    if (App.ConnectedVia == "USB")
                    {
                        response = await DependencyService.Get<IConnectionUSB>().RequestIorTest(request_command);
                    }
                    else if (App.ConnectedVia == "BT")
                    {
                        response = await DependencyService.Get<IBth>().RequestIorTest(request_command);
                        //Parameter_Status = await DependencyService.Get<Interfaces.IBth>().WriteAtuatorTest(model_detail.write_pid_index, pidList);
                    }
                    else
                    {
                        response = await DependencyService.Get<IConnectionWifi>().RequestIorTest(request_command);
                    }
                    break;
                default:
                    break;
            }

            //var response = await DependencyService.Get<IConnectionWifi>().RequestIorTest(request_command);
            Console.WriteLine("Routine Continue");
            return response;
        }

        //public async Task<TestRoutineResponseModel> RequestRoutine(string command)
        //{
        //    //var response = await DependencyService.Get<Interfaces.IConnectionWifi>().SetTestRoutineCommand(command);
        //    Console.WriteLine("Routine Request");
        //    return null;
        //}

        public async Task<TestRoutineResponseModel> StopRoutine(string stop_command)
        {
            Console.WriteLine("Routine Stopped");

            var response = new TestRoutineResponseModel();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    break;
                case Device.Android:
                    if (App.ConnectedVia == "USB")
                    {
                        response = await DependencyService.Get<Interfaces.IConnectionUSB>().StopIorTest(stop_command);
                    }
                    else if (App.ConnectedVia == "BT")
                    {
                        response = await DependencyService.Get<Interfaces.IBth>().StopIorTest(stop_command);
                        //Parameter_Status = await DependencyService.Get<Interfaces.IBth>().WriteAtuatorTest(model_detail.write_pid_index, pidList);
                    }
                    else
                    {
                        response = await DependencyService.Get<Interfaces.IConnectionWifi>().StopIorTest(stop_command);
                    }
                    break;
                default:
                    break;
            }

            //var response = await DependencyService.Get<Interfaces.IConnectionWifi>().StopIorTest(stop_command);
            //Console.WriteLine("Routine Stopped");
            return response;
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

        #region TEST MONITORS
        //bool AllConditionMatched = false;
        List<PidCode> codes;
        ObservableCollection<PidCode> listedCodes;
        PidCode pidCode = null;
        public async void GetPidValues()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    try
                   {
                       var PidData = DependencyService.Get<ISaveLocalData>().GetData("pidjson");
                       var PidInfo = JsonConvert.DeserializeObject<List<Results>>(PidData);
                       listedCodes = new ObservableCollection<PidCode>();

                       var pid_datset = StaticData.ecu_info.FirstOrDefault(x => x.ecu_ID == ecu_id).pid_dataset_id;
                        List<PidCode> pidCodeList = new List<PidCode>();
                        foreach (var item in PidInfo.FirstOrDefault(x => x.id == pid_datset).codes)
                        {
                            foreach (var item1 in item.pi_code_variable)
                            {
                                pidCodeList.Add(new PidCode
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
                                    message_type = item1.message_type,
                                    unit = item1.unit,
                                    messages = item1.messages
                                });
                            }
                        }


                        codes = new List<PidCode>(pidCodeList);

                        foreach (var item2 in viewModel.MonitorList.ToList())
                       {
                           pidCode = null;
                           pidCode = codes.FirstOrDefault(x => x.id == item2.pid);
                           if (pidCode != null)
                           {
                               listedCodes.Add(pidCode);
                           }
                           //foreach (var item in codes)
                           //{
                           //    if (item.id == item2.pid)
                           //    {
                           //        viewModel.BtnContinueEnable = false;
                           //        viewModel.PidPrecondition.Add(item);
                           //    }
                           //    //foreach (var item1 in item.codes.ToList())
                           //    //{
                           //    //    if (item1.id == item2.pid)
                           //    //    {
                           //    //        viewModel.BtnContinueEnable = false;
                           //    //        viewModel.PidPrecondition.Add(item1);
                           //    //    }
                           //    //}
                           //}
                       }

                       //if (listedCodes != null && viewModel.listedCodes.Any())
                       //{
                       //    while (IsReadingPid)
                       //    {
                       //        await GetPidsValue();

                       //        //if (AllConditionMatched)
                       //        //{
                       //        //    viewModel.BtnContinueEnable = true;
                       //        //}
                       //        //else
                       //        //{
                       //        //    viewModel.BtnContinueEnable = false;
                       //        //}
                       //    }
                       //}
                   }
                   catch (Exception ex)
                   {
                   }
                }
            });
        }

        public async Task GetPidsValue()
        {
            try
            {
                if (!CurrentUserEvent.Instance.IsExpert)
                {
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            //top = 20;
                            break;
                        case Device.Android:
                            if (App.ConnectedVia == "USB")
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadPid(listedCodes);
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IBth>().ReadPid(listedCodes);
                            }
                            else
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionWifi>().ReadPid(listedCodes);
                            }
                            break;
                        default:
                            //top = 0;
                            break;
                    }
                    SetPidValue();
                    if (!CurrentUserEvent.Instance.IsExpert)
                    {
                        App.controlEventManager.SendRequestData("PidValue*#" + JsonConvert.SerializeObject(read_pid_android));
                    }
                }
                else
                {
                    DependencyService.Get<ILodingPageService>().HideLoadingPage();
                    DependencyService.Get<ILodingPageService>().InitLoadingPage(new LoadingIndicatorPage());
                    DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                    await Task.Delay(200);
                }
            }
            catch (Exception ex)
            {
            }
        }
        List<PreCondition> manual_precondition;
        public async void SetPidValue()
        {
            if (read_pid_android != null)
            {
                foreach (var read_pid in read_pid_android)
                {
                    var pidlist = viewModel.MonitorList.FirstOrDefault(x => x.pid == read_pid.pidNumber);
                    if (pidlist != null)
                    {
                        if (read_pid.Status == "NOERROR")
                        {
                            if (read_pid.responseValue.Contains("."))
                            {
                                double.TryParse(read_pid.responseValue, out deci);
                                pidlist.current_value = deci.ToString("0.00");
                            }
                            else
                            {
                                pidlist.current_value = read_pid.responseValue;
                            }

                            //if (Convert.ToDouble(pidlist.current_value) >= Convert.ToDouble(pidlist.lower_limit)
                            //    && Convert.ToDouble(pidlist.current_value) <= Convert.ToDouble(pidlist.upper_limit))
                            //{
                            //    AllConditionMatched = true;
                            //    if (manual_precondition.FirstOrDefault(x => !x.is_check) != null)
                            //    {
                            //        AllConditionMatched = false;
                            //    }
                            //}
                            //else
                            //{
                            //    AllConditionMatched = false;
                            //}
                        }
                        else
                        {
                            pidlist.current_value = "ERR";
                            //AllConditionMatched = false;
                            //pidlist.unit = "";
                        }
                    }
                }
            }
        }
        #endregion

        #region Commented Code
        //public async Task GetPidValues()
        //{
        //    try
        //    {
        //        var PidData = DependencyService.Get<ISaveLocalData>().GetData("pidjson");
        //        var PidInfo = JsonConvert.DeserializeObject<List<Results>>(PidData);

        //        foreach (var item2 in viewModel.MonitorList.Where(x => x.test_monitor_type == "pid").ToList())
        //        {
        //            foreach (var item in PidInfo.ToList())
        //            {
        //                foreach (var item1 in viewModel.SelectedIorTest.codes.ToList())
        //                {

        //                    if (item1.id == item2.pid)
        //                    {
        //                        viewModel.ServerPid.Add(item1);
        //                    }
        //                }
        //            }
        //        }

        //        ObservableCollection<ReadParameterPID> pidList = new ObservableCollection<ReadParameterPID>();

        //        foreach (var item in viewModel.ServerPid)
        //        {
        //            var MessageModels = new List<SelectedParameterMessage>();
        //            foreach (var MessageItem in viewModel.SelectedIorTest.messages)
        //            {
        //                MessageModels.Add(new SelectedParameterMessage { code = MessageviewModel.SelectedIorTest.code, message = MessageviewModel.SelectedIorTest.message });
        //            }
        //            pidList.Add(
        //                new ReadParameterPID
        //                {
        //                    pid = viewModel.SelectedIorTest.code,
        //                    totalLen = viewModel.SelectedIorTest.code.Length / 2,
        //                    //totalbyte -
        //                    startByte = viewModel.SelectedIorTest.byte_position,
        //                    noOfBytes = viewModel.SelectedIorTest.length,
        //                    IsBitcoded = viewModel.SelectedIorTest.bitcoded,
        //                    //noofBits = (int?)viewModel.SelectedIorTest.start_bit_position - (int?)viewModel.SelectedIorTest.end_bit_position + 1,
        //                    startBit = Convert.ToInt32(viewModel.SelectedIorTest.start_bit_position),
        //                    noofBits = viewModel.SelectedIorTest.end_bit_position.GetValueOrDefault() - viewModel.SelectedIorTest.start_bit_position.GetValueOrDefault() + 1,
        //                    resolution = viewModel.SelectedIorTest.resolution,
        //                    offset = viewModel.SelectedIorTest.offset,
        //                    datatype = viewModel.SelectedIorTest.message_type,
        //                    //totalBytes = viewModel.SelectedIorTest.length,
        //                    pidNumber = viewModel.SelectedIorTest.id,
        //                    pidName = viewModel.SelectedIorTest.short_name,
        //                    unit = viewModel.SelectedIorTest.unit,
        //                    messages = MessageModels,
        //                });
        //        }

        //        var read_pid_android = await DependencyService.Get<Interfaces.IBth>().ReadPid(pidList);

        //        foreach (var pid in read_pid_android)
        //        {
        //            var item = viewModel.ServerPid.FirstOrDefault(x => x.id == pid.pidNumber);
        //            if (item != null)
        //            {
        //                //if (pid.Status == "NOERROR")
        //                //{
        //                //    viewModel.SelectedIorTest.show_resolution = pid.responseValue;
        //                //}
        //                //else
        //                //{
        //                //    viewModel.SelectedIorTest.show_resolution = "ERR";
        //                //    viewModel.SelectedIorTest.unit = "";
        //                //}
        //                //viewModel.SelectedIorTest.resolution= Convert.ToDouble(pid.responseValue);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        //public void WritePid()
        //{
        //    long count = 0;
        //    bool is_active = true;
        //    while (is_active)
        //    {
        //        count++;
        //        Console.WriteLine($"Count : {count}");

        //        if (count >= 10000)
        //        {
        //            is_active = false;
        //        }
        //    }
        //}
        #endregion
    }
}