using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Popup;
using Bajaj.Services;
using Bajaj.ViewModel;
using Java.Lang;
using MultiEventController;
using MultiEventController.Models;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Exception = System.Exception;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LiveParameterSelectedPage : DisplayAlertPage
    {
        LiveParameterSelectedViewModel viewModel;
        ObservableCollection<ReadPidPresponseModel> read_pid_android;
        ApiServices services;
        List<PIDLiveRecord> LiveRecord;
        List<PidLive> Live;
        List<YAxisPointName> YAxisPoint;
        List<snapshot_record> Values;
        int j = 0;
        double deci;
        string value = string.Empty;
        string code = string.Empty;
        public LiveParameterSelectedPage(ObservableCollection<PidCode> SelectedParameterList)
        {
            try
            {
                InitializeComponent();

                BindingContext = viewModel = new LiveParameterSelectedViewModel();
                services = new ApiServices();
                read_pid_android = new ObservableCollection<ReadPidPresponseModel>();
                viewModel.SelectedParameterList = new ObservableCollection<PidCode>(SelectedParameterList);
            }
            catch (Exception ex)
            {
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
            Task.Run(async () =>
            {
                await Task.Delay(100);
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await GetPidsValue();
                }
            });
        }
        protected async override void OnDisappearing()
        {
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

            StartTime = false;
            RecordingStart = false;
        }


        string[] PairedData = new string[2];
        private async void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {
            #region Check Internet Connection
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                //await Task.Delay(100);
                bool InsternetActive = true;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    string data = (string)sender; //sender as string;
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (data.Contains("PidValue*#"))
                        {
                            PairedData = data.Split('#');
                            read_pid_android = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(PairedData[1]); ;
                            SetPidValue();
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("SnapshotStart*#"))
                        {
                            PlayPauseItem.BackgroundColor = Color.Gray;
                            SnapshotItem.BackgroundColor = Color.FromHex("#3e4095");
                            RecordingItem.BackgroundColor = Color.Gray;

                            PlayPauseItem.IsEnabled = false;
                            SnapshotItem.IsEnabled = true;
                            RecordingItem.IsEnabled = false;

                            Snapshot = true;

                            value = string.Empty;
                            code = string.Empty;
                        }
                        else if (data.Contains("SnapShotData*#"))
                        {
                            PairedData = data.Split('#');
                            Values = JsonConvert.DeserializeObject<List<snapshot_record>>(PairedData[1]);
                        }
                        else if (data.Contains("SnapShotSaved"))
                        {
                            var RecordingSaved = new Popup.DisplayAlertPage("SUCCESS", "Snapshot Saved Successfully", "OK");
                            await PopupNavigation.Instance.PushAsync(RecordingSaved);
                            PlayPauseItem.BackgroundColor = Color.FromHex("#3e4095");
                            SnapshotItem.BackgroundColor = Color.FromHex("#3e4095");
                            RecordingItem.BackgroundColor = Color.FromHex("#3e4095");

                            PlayPauseItem.IsEnabled = true;
                            SnapshotItem.IsEnabled = true;
                            RecordingItem.IsEnabled = true;

                            Snapshot = false;
                        }
                        else if (data.Contains("SnapShotFailled"))
                        {
                            var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Snapshot Not Saved Successfully", "OK");
                            await PopupNavigation.Instance.PushAsync(RecordingSaved);
                            PlayPauseItem.BackgroundColor = Color.FromHex("#3e4095");
                            SnapshotItem.BackgroundColor = Color.FromHex("#3e4095");
                            RecordingItem.BackgroundColor = Color.FromHex("#3e4095");

                            PlayPauseItem.IsEnabled = true;
                            SnapshotItem.IsEnabled = true;
                            RecordingItem.IsEnabled = true;

                            Snapshot = false;
                        }
                        else if (data.Contains("InternetIssue"))
                        {
                            Snapshot = false;
                        }
                        else if (data.Contains("RecordingStartFalse*#"))
                        {
                            PlayPauseItem.BackgroundColor = Color.Gray;
                            SnapshotItem.BackgroundColor = Color.Gray;
                            RecordingItem.BackgroundColor = Color.FromHex("#EE204D");

                            PlayPauseItem.IsEnabled = false;
                            SnapshotItem.IsEnabled = false;
                            RecordingItem.IsEnabled = true;

                            RecordingStart = true;
                            RecordingItem.Source = "ic_recording.png";
                        }
                        else if (data.Contains("RecordingStartTrue*#"))
                        {
                            PlayPauseItem.BackgroundColor = Color.FromHex("#3e4095");
                            SnapshotItem.BackgroundColor = Color.FromHex("#3e4095");
                            RecordingItem.BackgroundColor = Color.FromHex("#3e4095");

                            PlayPauseItem.IsEnabled = true;
                            SnapshotItem.IsEnabled = true;
                            RecordingItem.IsEnabled = true;

                            RecordingStart = false;
                        }
                        else if (data.Contains("InitializeList*#"))
                        {
                            LiveRecord = new List<PIDLiveRecord>(); // 1
                            Live = new List<PidLive>(); // 2
                            YAxisPoint = new List<YAxisPointName>(); // 3

                            //j = 0;
                            NavigationPage.SetHasBackButton(this, false);
                        }
                        else if (data.Contains("SetHasBackButtonTrue*#"))
                        {
                            NavigationPage.SetHasBackButton(this, true);
                        }
                        else if (data.Contains("ShowLoader*#"))
                        {
                            NavigationPage.SetHasBackButton(this, true);
                        }
                        else if (data.Contains("HideLoader*#"))
                        {
                            NavigationPage.SetHasBackButton(this, true);
                        }
                        else if (data.Contains("SendTimer*#"))
                        {
                            PairedData = data.Split('#');
                            LabRecordingTimer.Text = PairedData[1];
                        }
                        else if (data.Contains("ResetRecordingTimer*#"))
                        {
                            PairedData = data.Split('#');
                            LabRecordingTimer.Text = PairedData[1];
                        }
                        else if (data.Contains("RecodingSaved"))
                        {
                            var RecordingSaved = new Popup.DisplayAlertPage("SUCCESS", "Recording Saved Successfully", "OK");
                            await PopupNavigation.Instance.PushAsync(RecordingSaved);
                        }
                        else if (data.Contains("RecordingFailled"))
                        {
                            var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Recording Not Saved Successfully", "OK");
                            await PopupNavigation.Instance.PushAsync(RecordingSaved);
                        }
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
            if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("PidPlayPauseClicked"))
            {
                PidPlayPauseClicked();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("PidListScrolled"))
            {
                collectionView.ScrollTo(Convert.ToInt32(elementEventHandler.ElementName), 0, ScrollToPosition.Start);
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("PidRecordingClicked"))
            {
                PidRecordingClicked();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("PidSnapshotClicked"))
            {
                PidSnapshotClicked();
            }
            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
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
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadPid(viewModel.SelectedParameterList);
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IBth>().ReadPid(viewModel.SelectedParameterList);
                            }
                            else if (App.ConnectedVia == "WIFI")
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionWifi>().ReadPid(viewModel.SelectedParameterList);
                            }
                            else
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionRP>().ReadPid(viewModel.SelectedParameterList);
                                //ReadPid_RP1210();
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
        public void SetPidValue()
        {
            if (read_pid_android != null)
            {
                foreach (var pid in read_pid_android)
                {
                    var pidlist = viewModel.SelectedParameterList.FirstOrDefault(x => x.id == pid.pidNumber);
                    if (pidlist != null)
                    {
                        if (pid.Status == "NOERROR")
                        {
                            if (pid.responseValue.Contains("."))
                            {
                                double.TryParse(pid.responseValue, out deci);
                                pidlist.show_resolution = deci.ToString("0.00");
                            }
                            else
                            {
                                pidlist.show_resolution = pid.responseValue;
                            }
                        }
                        else
                        {
                            pidlist.show_resolution = "ERR";
                            pidlist.unit = "";
                        }
                    }
                }
            }
        }

        public bool StartTime = false;
        public bool RecordingStart = false;
        public Stopwatch stopwatch;
        bool ReadCompleted = true;
        private void PidPlayPauseClicked(object sender, EventArgs e)
        {
            try
            {

                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"PidPlayPauseFrame",
                        ElementValue = "PidPlayPauseClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                PidPlayPauseClicked();
            }
            catch (Exception ex)
            {

            }
        }

        private async void PidPlayPauseClicked()
        {
            try
            {

                if (StartTime == false)
                {
                    StartTime = true;
                    txtPlayStop.Text = "Stop";
                    PlayPauseItem.Source = "ic_pause.png";

                    PlayPauseItem.BackgroundColor = Color.FromHex("#3e4095");
                    RecordingItem.BackgroundColor = SnapshotItem.BackgroundColor = Color.Gray;

                    PlayPauseItem.IsEnabled = true;
                    RecordingItem.IsEnabled = SnapshotItem.IsEnabled = false;
                    NavigationPage.SetHasBackButton(this, false);

                    read_pid_android = new ObservableCollection<ReadPidPresponseModel>();
                    while (StartTime)
                    {
                        await Task.Run(async () =>
                        {
                            await GetPidsValue();
                        });
                    }
                    NavigationPage.SetHasBackButton(this, true);
                    PlayPauseItem.Source = "play.png";
                    RecordingItem.IsEnabled = SnapshotItem.IsEnabled = true;
                    SnapshotItem.BackgroundColor = RecordingItem.BackgroundColor = Color.FromHex("#3e4095");
                }
                else
                {
                    StartTime = false;
                    PlayPauseItem.Source = "play.png";
                    txtPlayStop.Text = "Play";
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void PidRecordingClicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"PidRecordingFrame",
                        ElementValue = "PidRecordingClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                PidRecordingClicked();
            }
            catch (Exception ex)
            {
            }
        }

        private async void PidRecordingClicked()
        {
            try
            {

                string code = string.Empty;
                StartTime = false;
                //await Task.Delay(200);

                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (isReachable)
                {
                    stopwatch = new Stopwatch();
                    if (RecordingStart == false)
                    {
                        stopwatch.Start();

                        PlayPauseItem.BackgroundColor = Color.Gray;
                        SnapshotItem.BackgroundColor = Color.Gray;
                        RecordingItem.BackgroundColor = Color.FromHex("#EE204D");

                        PlayPauseItem.IsEnabled = false;
                        SnapshotItem.IsEnabled = false;
                        RecordingItem.IsEnabled = true;
                        j = 0;
                        RecordingStart = true;

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                            {
                                LabRecordingTimer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");
                                if (!CurrentUserEvent.Instance.IsExpert)
                                {
                                    App.controlEventManager.SendRequestData("SendTimer*#" + LabRecordingTimer.Text);
                                }
                                return RecordingStart;
                            });
                        });

                        RecordingItem.Source = "ic_recording.png";
                        recording();
                        if (!CurrentUserEvent.Instance.IsExpert)
                        {
                            App.controlEventManager.SendRequestData("RecordingStartFalse*#");
                        }
                    }
                    else
                    {
                        PlayPauseItem.BackgroundColor = Color.FromHex("#3e4095");
                        SnapshotItem.BackgroundColor = Color.FromHex("#3e4095");
                        RecordingItem.BackgroundColor = Color.FromHex("#3e4095");

                        PlayPauseItem.IsEnabled = true;
                        SnapshotItem.IsEnabled = true;
                        RecordingItem.IsEnabled = true;

                        RecordingStart = false;
                        if (!CurrentUserEvent.Instance.IsExpert)
                        {
                            App.controlEventManager.SendRequestData("RecordingStartTrue*#");
                        }
                    }


                }
                else
                {
                    //show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                    var RecordingSaved = new Popup.DisplayAlertPage("Please check Internet Connection", "Internet Connection Problem", "OK");
                    await PopupNavigation.Instance.PushAsync(RecordingSaved);
                    //Snapshot = true;
                }

            }
            catch (Exception ex)
            {

            }
        }

        public async void recording()
        {
            string code = string.Empty;
            LiveRecord = new List<PIDLiveRecord>(); // 1
            Live = new List<PidLive>(); // 2
            YAxisPoint = new List<YAxisPointName>(); // 3



            NavigationPage.SetHasBackButton(this, false);
            if (!CurrentUserEvent.Instance.IsExpert)
            {
                App.controlEventManager.SendRequestData("InitializeList*#");
            }
            while (RecordingStart)
            {
                j++;
                Console.WriteLine($"Read No Of Times: {j}");
                await Task.Run(async () =>
                {
                    ReadCompleted = false;
                    await GetPidsValue();
                    ReadCompleted = true;
                    Console.WriteLine($"After Read : {ReadCompleted}");
                    //await Task.Delay(500);
                    #region Live

                    foreach (var item1 in viewModel.SelectedParameterList)
                    {
                        var isValue = YAxisPoint.Where(x => x.pid_name == item1.code);
                        if (isValue != null && isValue.Count() > 0)
                        {
                            if (isValue.FirstOrDefault().value != null)
                            {
                                if (string.IsNullOrEmpty(item1.show_resolution))
                                {
                                    isValue.FirstOrDefault().value.Add("NOT FOUND");
                                }
                                else
                                {
                                    isValue.FirstOrDefault().value.Add(item1.show_resolution);
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(item1.show_resolution))
                                {
                                    isValue.FirstOrDefault().value = new List<string>();
                                    isValue.FirstOrDefault().value.Add("NOT FOUND");
                                }
                                else
                                {
                                    isValue.FirstOrDefault().value = new List<string>();
                                    isValue.FirstOrDefault().value.Add(item1.show_resolution);
                                }

                            }
                        }
                        else
                        {
                            code = string.Empty;
                            List<string> lst = new List<string>();
                            if (string.IsNullOrEmpty(item1.show_resolution))
                            {
                                lst.Add("NOT FOUND");
                            }
                            else
                            {
                                lst.Add(item1.show_resolution);
                            }

                            if (string.IsNullOrEmpty(item1.code))
                            {
                                lst.Add("NOT FOUND");
                            }
                            code = item1.code;
                            if (string.IsNullOrEmpty(item1.code))
                            {
                                code = "NOT FOUND";
                            }

                            YAxisPoint.Add(new YAxisPointName()
                            {

                                pid_name = code,
                                value = lst
                            });
                        }
                    }
                    #endregion
                });
            }
            NavigationPage.SetHasBackButton(this, true);
            if (!CurrentUserEvent.Instance.IsExpert)
            {
                App.controlEventManager.SendRequestData("SetHasBackButtonTrue*#");
            }
            if (RecordingStart == false)
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    if (!CurrentUserEvent.Instance.IsExpert)
                    {
                        App.controlEventManager.SendRequestData("ShowLoader*#");
                    }
                    await Task.Delay(200);
                    Console.WriteLine($"Completed Read : {ReadCompleted}");
                    while (ReadCompleted == false)
                    {
                        Console.Write("######$$$$$$$$$$$$$$%%%%%%%%%%%%%--Data Reading--######$$$$$$$$$$$$$$%%%%%%%%%%%%%");
                    }
                    stopwatch.Stop();

                    List<string> val1 = new List<string>();
                    for (int i = 0; i < j; i++)
                    {
                        val1.Add(Convert.ToString(i));
                    }
                    Live.Add(new PidLive() { x_axis_point = val1, y_axis_point_name = YAxisPoint });

                    LiveRecord.Add(new PIDLiveRecord() { pid_live = Live });

                    if (LiveRecord.FirstOrDefault().pid_live.FirstOrDefault().x_axis_point.Count() != 0)
                    {
                        bool GotReturnValues = services.pid_live_record(LiveRecord, App.JwtToken, App.SessionId);
                        if (GotReturnValues == true)
                        {
                            //await this.DisplayAlert("NO ERROR", "Recording Saved Successfully", "OK");
                            if (PopupNavigation.Instance.PopupStack.Any())
                            {
                                await PopupNavigation.Instance.PopAllAsync();
                            }
                            if (!CurrentUserEvent.Instance.IsExpert)
                            {
                                var RecordingSaved = new Popup.DisplayAlertPage("SUCCESS", "Recording Saved Successfully", "OK");
                                await PopupNavigation.Instance.PushAsync(RecordingSaved);
                                App.controlEventManager.SendRequestData("RecodingSaved");
                            }
                            stopwatch.Reset();
                            stopwatch.Stop();


                        }
                        else
                        {
                            if (PopupNavigation.Instance.PopupStack.Any())
                            {
                                await PopupNavigation.Instance.PopAllAsync();
                            }
                            //await this.DisplayAlert("ERROR", "Recording Not Saved Successfully", "OK");
                            if (!CurrentUserEvent.Instance.IsExpert)
                            {
                                var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Recording Not Saved Successfully", "OK");

                                await PopupNavigation.Instance.PushAsync(RecordingSaved);
                                App.controlEventManager.SendRequestData("RecordingFailled");
                            }
                        }

                        LabRecordingTimer.Text = "00:00";
                        if (!CurrentUserEvent.Instance.IsExpert)
                        {
                            App.controlEventManager.SendRequestData("ResetRecordingTimer*#" + LabRecordingTimer.Text);
                        }
                    }
                }
            }
        }

        public bool Snapshot = false;
        private void PidSnapshotClicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"PidSnapshotFrame",
                        ElementValue = "PidSnapshotClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                PidSnapshotClicked();
            }
            catch (Exception ex)
            {

            }
        }

        private async void PidSnapshotClicked()
        {
            try
            {
                StartTime = false;
                RecordingStart = false;

                if (!CurrentUserEvent.Instance.IsExpert)
                {
                    var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                    if (isReachable)
                    {
                        if (Snapshot == false)
                        {
                            PlayPauseItem.BackgroundColor = Color.Gray;
                            SnapshotItem.BackgroundColor = Color.FromHex("#3e4095");
                            RecordingItem.BackgroundColor = Color.Gray;

                            PlayPauseItem.IsEnabled = false;
                            SnapshotItem.IsEnabled = true;
                            RecordingItem.IsEnabled = false;

                            Snapshot = true;

                            value = string.Empty;
                            code = string.Empty;
                            App.controlEventManager.SendRequestData("SnapshotStart*#");
                            await Task.Run(async () =>
                            {
                                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                                {
                                    await Task.Delay(200);
                                    await GetPidsValue();

                                    Device.BeginInvokeOnMainThread(async () =>
                                    {
                                        Values = new List<snapshot_record>();
                                        foreach (var item in read_pid_android)
                                        {
                                            code = item.pidName;
                                            value = item.responseValue;

                                            if (string.IsNullOrEmpty(item.pidName))
                                            {
                                                code = "NOT FOUND";
                                            }

                                            if (string.IsNullOrEmpty(value))
                                            {
                                                value = "NOT FOUND";
                                            }
                                            Values.Add(new snapshot_record { code = code, value = value });
                                        }

                                        App.controlEventManager.SendRequestData($"SnapShotData*#{JsonConvert.SerializeObject(Values)}");
                                        bool GotReturnValues = services.pid_snapshot_record(Values, App.JwtToken, App.SessionId);
                                        if (GotReturnValues == true)
                                        {
                                            if (PopupNavigation.Instance.PopupStack.Any())
                                            {
                                                await PopupNavigation.Instance.PopAllAsync();
                                            }
                                            UserDialogs.Instance.HideLoading();
                                            var SnapshotSaved = new Popup.DisplayAlertPage("SUCCESS", "Snapshot Saved Successfully", "OK");
                                            await PopupNavigation.Instance.PushAsync(SnapshotSaved);
                                            App.controlEventManager.SendRequestData("SnapShotSaved");



                                            PlayPauseItem.BackgroundColor = Color.FromHex("#3e4095");
                                            SnapshotItem.BackgroundColor = Color.FromHex("#3e4095");
                                            RecordingItem.BackgroundColor = Color.FromHex("#3e4095");

                                            PlayPauseItem.IsEnabled = true;
                                            SnapshotItem.IsEnabled = true;
                                            RecordingItem.IsEnabled = true;

                                            Snapshot = false;

                                        }
                                        else
                                        {
                                            if (PopupNavigation.Instance.PopupStack.Any())
                                            {
                                                await PopupNavigation.Instance.PopAllAsync();
                                            }
                                            UserDialogs.Instance.HideLoading();


                                            var SnapshotSaved = new Popup.DisplayAlertPage("ERROR", "Snapshot Not Saved Successfully", "OK");
                                            await PopupNavigation.Instance.PushAsync(SnapshotSaved);
                                            App.controlEventManager.SendRequestData("SnapShotFailled");

                                            PlayPauseItem.BackgroundColor = Color.FromHex("#3e4095");
                                            SnapshotItem.BackgroundColor = Color.FromHex("#3e4095");
                                            RecordingItem.BackgroundColor = Color.FromHex("#3e4095");
                                            PlayPauseItem.IsEnabled = true;
                                            SnapshotItem.IsEnabled = true;
                                            RecordingItem.IsEnabled = true;
                                            Snapshot = false;
                                        }
                                    });
                                }
                            });
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                        App.controlEventManager.SendRequestData("InternetIssue");
                        Snapshot = false;
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void PidListScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (CurrentUserEvent.Instance.IsExpert)
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = Convert.ToString(e.FirstVisibleItemIndex),
                    ElementValue = "PidListScrolled",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }
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


        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        //private async Task ObservableCollection<ReadPidPresponseModel> ReadPid_RP1210()
        //{
        //    try
        //    {
        //        object result = new object();
        //        functionClass = new FunctionClass();
        //        ObservableCollection<ReadParameterPID> list = new ObservableCollection<ReadParameterPID>();
        //        foreach (var item in viewModel.SelectedParameterList)
        //        {
        //            var MessageValueList = new List<SelectedParameterMessage>();
        //            if (item.messages != null)
        //            {
        //                foreach (var MessageItem in item.messages)
        //                {
        //                    MessageValueList.Add(new SelectedParameterMessage { code = MessageItem.code, message = MessageItem.message });
        //                }
        //            }

        //            list.Add(
        //                new ReadParameterPID
        //                {
        //                    datatype = item.message_type,
        //                    IsBitcoded = item.bitcoded,
        //                    noofBits = item.end_bit_position.GetValueOrDefault() - item.start_bit_position.GetValueOrDefault() + 1,
        //                    noOfBytes = item.length,
        //                    offset = item.offset,
        //                    pid = item.code,
        //                    resolution = item.resolution,
        //                    startBit = Convert.ToInt32(item.start_bit_position),
        //                    startByte = item.byte_position,
        //                    //totalBytes= item.totalBytes,
        //                    totalLen = item.code.Length / 2,
        //                    pidNumber = item.id,
        //                    pidName = item.short_name,
        //                    messages = MessageValueList

        //                });
        //        }
        //        var respo = await Task.Run(async () =>
        //        {

        //            //result = await functionClass.ReadParameters(list.Count, list);
        //            //result = await dSDiagnostic.ReadParameters(pidList.Count, list);
        //            var res = JsonConvert.SerializeObject(result);
        //            var res_list = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(res);
        //            return res_list;
        //            //status = Response.ECUResponseStatus;
        //        });

        //        return respo;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
    }
}