using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IvnLiveParameterSelectedPage : DisplayAlertPage
    {
        Grid ems_grid;
        Grid iecu_grid;
        Grid acm_grid;
        Grid tgw2_grid;
        bool ems_grid_permitted = false;
        bool iecu_grid_permitted = false;
        bool acm_grid_permitted = false;
        bool tgw2_grid_permitted = false;
        TapGestureRecognizer tab_gesture;
        IvnLiveParameterSelectedViewModel viewModel;
        public ObservableCollection<ReadParameterPID> ParameterList;
        public ObservableCollection<IVN_SelectedPID> SelectedIVN_ParameterList;
        string EcuName = string.Empty;
        ApiServices services;
        Color color;

        public IvnLiveParameterSelectedPage(ObservableCollection<ReadParameterPID> SelectedParameterList, ObservableCollection<IVN_SelectedPID> SelectedPid, string SelectedEcu)
        {
            InitializeComponent();
            BindingContext = viewModel = new IvnLiveParameterSelectedViewModel();

            PlayPauseIMG.Source = "play.png";
            RecordingIMG.Source = "ic_recording.png";

            services = new ApiServices();
            //ems_List.ItemsSource = viewModel.EMSParameterList;
            tab_gesture = new TapGestureRecognizer();
            tab_gesture.Tapped += Tab_gesture_Tapped;
            //create_ems_tab(1);
            //create_iecu_tab(.5);
            tab_gesture = new TapGestureRecognizer();
            EcuName = SelectedEcu;

            if (SelectedPid != null)
            {
                if (SelectedPid.Count > 0)
                {
                    create_ems_tab(1);
                    EmsView.IsVisible = true;
                }

                Console.WriteLine("Constructor Send" + System.DateTime.Now);
                Task.Run(async () => { await IVN_read_pid(SelectedPid); });

                SelectedIVN_ParameterList = SelectedPid;
            }
            else if (SelectedParameterList != null)
            {
                if (SelectedParameterList.Count > 0)
                {
                    create_ems_tab(1);
                    EmsView.IsVisible = true;
                }
                //read_pid(SelectedParameterList);
                //Loading();
                //Task.Delay(100);

                Console.WriteLine("Constructor Send" + System.DateTime.Now);

                Task.Run(async () => { await read_pid(SelectedParameterList); });

                ParameterList = SelectedParameterList;

                //RecordingItem.BackgroundColor = Color.Gray;
                //RecordingItem.IsEnabled = false;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var data = DependencyService.Get<ISaveLocalData>().GetData("selctedOemModel");
            if (data != null)
            {
                App.selectedOem = JsonConvert.DeserializeObject<AllOemModel>(data);
                color = ThemeManager.GetColorFromHexValue(App.selectedOem.color.ToString());
                //App.Current.Resources["theme_color"] = color;
            }
        }

        public ObservableCollection<ReadPidPresponseModel> read_pid_android;
        public async Task read_pid(ObservableCollection<ReadParameterPID> SelectedParameterList)
        {
            read_pid_android = new ObservableCollection<ReadPidPresponseModel>();
            try
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        //top = 20;
                        break;
                    case Device.Android:
                        if (App.ConnectedVia == "USB")
                        {
                            Console.WriteLine("Read Responces from PID" + System.DateTime.Now);
                            await Task.Run(async () =>
                            {
                                if (PlayPauseItem.IsEnabled != true && Snapshot != true && RecordingStart != true)
                                {
                                    popupLoadingView.IsVisible = true;
                                }

                                Thread.Sleep(100);
                                read_pid_android.Clear();
                                read_pid_android = await Task.Run(async () =>
                                {
                                    var data = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadPid(SelectedParameterList);
                                    return data;
                                });
                            });
                            Debug.WriteLine("------Got Responces from PID USB------" + System.DateTime.Now, "");
                        }
                        else
                        {
                            Console.WriteLine("Read Responces from PID" + System.DateTime.Now);
                            await Task.Run(async () =>
                            {
                                if (PlayPauseItem.IsEnabled != true && Snapshot != true && RecordingStart != true)
                                {
                                    popupLoadingView.IsVisible = true;
                                }
                                Thread.Sleep(100);
                                read_pid_android.Clear();
                                //read_pid_android = await DependencyService.Get<Interfaces.IBth>().ReadPid(SelectedParameterList);
                                read_pid_android = await Task.Run(async () =>
                                {
                                    var data = await DependencyService.Get<Interfaces.IBth>().ReadPid(SelectedParameterList);
                                    return data;
                                });
                            });
                            Debug.WriteLine("------Got Responces from PID BT------" + System.DateTime.Now, "");
                        }
                        break;
                    case Device.UWP:
                        var read_pid_uwp = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadPid(SelectedParameterList);
                        break;
                    default:
                        //top = 0;
                        break;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (read_pid_android != null)
                    {
                        foreach (var pid in read_pid_android)
                        {
                            foreach (var item in SelectedParameterList)
                            {
                                if (item.pidNumber == pid.pidNumber)
                                {
                                    if (pid.Status == "NOERROR")
                                    {
                                        if (pid.responseValue == "")
                                        {
                                            item.show_resolution = "ERR";
                                        }
                                        else
                                        {
                                            item.show_resolution = pid.responseValue;
                                        }
                                    }
                                    else
                                    {
                                        item.show_resolution = "ERR";
                                        item.unit = "";
                                    }
                                    break;
                                }
                            }
                        }
                        if (SelectedParameterList != null)
                        {
                            viewModel.EMSParameterList = SelectedParameterList;
                            ParameterList = SelectedParameterList;
                            ems_List.ItemsSource = viewModel.EMSParameterList;
                        }
                    }
                    popupLoadingView.IsVisible = false;
                });
            }
            catch (Exception ex)
            {
                popupLoadingView.IsVisible = false;
            }
        }

        public async Task IVN_read_pid(ObservableCollection<IVN_SelectedPID> PidFRead)
        {
            #region Old Code
            read_pid_android = new ObservableCollection<ReadPidPresponseModel>();
            try
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        //top = 20;
                        break;
                    case Device.Android:
                        if (App.ConnectedVia == "USB")
                        {
                            Console.WriteLine("Read Responces from PID" + System.DateTime.Now);

                            popupLoadingView.IsVisible = false;

                            Thread.Sleep(1000);
                            //read_pid_android.Clear();
                            await Task.Run(async () =>
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionUSB>().IVN_ReadPid(PidFRead);
                            });
                            //read_pid_android = await DependencyService.Get<Interfaces.IConnectionUSB>().IVN_ReadPid(PidFRead);
                            Debug.WriteLine("------Got Responces from PID USB------" + System.DateTime.Now, "");
                        }
                        else
                        {
                            Console.WriteLine("Read Responces from PID" + System.DateTime.Now);

                            //popupLoadingView.IsVisible = false;

                            Thread.Sleep(100);
                            //read_pid_android.Clear();

                            await Task.Run(async () =>
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IBth>().IVN_ReadPid(PidFRead);
                            });

                            //read_pid_android = await DependencyService.Get<Interfaces.IBth>().IVN_ReadPid(PidFRead);
                            Debug.WriteLine("------Got Responces from PID BT------" + System.DateTime.Now, "");
                        }
                        break;
                    case Device.UWP:
                        //var read_pid_uwp = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadPid(SelectedParameterList);
                        break;
                    default:
                        //top = 0;
                        break;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (read_pid_android != null)
                    {
                        var OldValueList = viewModel.EMSParameterList;

                        ems_List.ItemsSource = null;
                        viewModel.IVNEMSParameterList = SelectedIVN_ParameterList;
                        SelectedIVN_ParameterList = PidFRead;
                        ems_List.ItemsSource = viewModel.IVNEMSParameterList;

                        ems_List.ItemsSource = null;

                        var List = new ObservableCollection<ReadParameterPID>();

                        //viewModel.EMSParameterList.Clear();

                        if (read_pid_android != null)
                        {
                            foreach (var item in read_pid_android)
                            {
                                if (item.responseValue == "ERR")
                                {
                                    foreach (var item2 in OldValueList)
                                    {
                                        if (item2.pidName == item.pidName)
                                        {
                                            item.responseValue = item2.show_resolution;
                                            item.Unit = item2.unit;
                                            if (item2.show_resolution == "")
                                            {
                                                item.responseValue = "";
                                            }
                                            break;
                                        }
                                    }
                                    //item.responseValue = OldValueList.FirstOrDefault(x => x.pidName == item.pidName)?.show_resolution;
                                    if (item.responseValue == "ERR")
                                    {
                                        item.responseValue = " ";
                                    }
                                }
                                //viewModel.EMSParameterList.Add(new ReadParameterPID { pidName = item.pidName, show_resolution = item.responseValue, unit = item.Unit });                                
                                List.Add(new ReadParameterPID { pidName = item.pidName, show_resolution = item.responseValue, unit = item.Unit });
                            }

                            viewModel.EMSParameterList.Clear();
                            viewModel.EMSParameterList = List;
                            ems_List.ItemsSource = viewModel.EMSParameterList;
                        }
                    }
                    popupLoadingView.IsVisible = false;
                });
            }
            catch (Exception ex)
            {
                popupLoadingView.IsVisible = false;
            }
            #endregion
        }

        public void create_ems_tab(double opacity)
        {
            try
            {
                grd_tab.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Label lbl = new Label
                {
                    Text = EcuName,
                    Style = (Style)this.Resources["txt_tab"]
                };
                ems_grid = new Grid
                {
                    BackgroundColor = (Color)Application.Current.Resources["theme_color"],
                    Opacity = opacity,
                    ClassId = "EMS",
                };
                ems_grid.Children.Add(lbl);
                var count = grd_tab.ColumnDefinitions.Count - 1;
                grd_tab.Children.Add(ems_grid, count, 0);
                ems_grid.GestureRecognizers.Add(tab_gesture);
                ems_grid_permitted = true;
            }
            catch (Exception ex)
            {
            }
        }

        public void create_iecu_tab(double opacity)
        {
            try
            {
                grd_tab.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Label lbl = new Label
                {
                    Text = "IECU",
                    Style = (Style)this.Resources["txt_tab"]
                };
                iecu_grid = new Grid
                {
                    BackgroundColor = (Color)Application.Current.Resources["theme_color"],
                    Opacity = opacity,
                    ClassId = "IECU",
                };
                iecu_grid.Children.Add(lbl);
                var count = grd_tab.ColumnDefinitions.Count - 1;
                grd_tab.Children.Add(iecu_grid, count, 0);
                iecu_grid.GestureRecognizers.Add(tab_gesture);
                iecu_grid_permitted = true;
            }
            catch (Exception ex)
            {
            }
        }

        public void create_acm_tab(double opacity)
        {
            try
            {
                grd_tab.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Label lbl = new Label
                {
                    Text = "ACM",
                    Style = (Style)this.Resources["txt_tab"]
                };
                acm_grid = new Grid
                {
                    BackgroundColor = (Color)Application.Current.Resources["theme_color"],
                    Opacity = opacity,
                    ClassId = "ACM",
                };
                acm_grid.Children.Add(lbl);
                var count = grd_tab.ColumnDefinitions.Count - 1;
                grd_tab.Children.Add(acm_grid, count, 0);
                acm_grid.GestureRecognizers.Add(tab_gesture);
                acm_grid_permitted = true;
            }
            catch (Exception ex)
            {
            }
        }

        public void create_tgw2_tab(double opacity)
        {
            try
            {
                grd_tab.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Label lbl = new Label
                {
                    Text = "TGW2",
                    Style = (Style)this.Resources["txt_tab"]
                };
                tgw2_grid = new Grid
                {
                    BackgroundColor = (Color)Application.Current.Resources["theme_color"],
                    Opacity = opacity,
                    ClassId = "TGW2",
                };
                tgw2_grid.Children.Add(lbl);
                var count = grd_tab.ColumnDefinitions.Count - 1;
                grd_tab.Children.Add(tgw2_grid, count, 0);
                tgw2_grid.GestureRecognizers.Add(tab_gesture);
                tgw2_grid_permitted = true;
            }
            catch (Exception ex)
            {
            }
        }
        private void Tab_gesture_Tapped(object sender, EventArgs e)
        {
            try
            {
                var sen = sender as Grid;
                if (sen == null) { return; }
                if (ems_grid_permitted)
                {
                    ems_grid.Opacity = 0.5;
                }

                if (iecu_grid_permitted)
                {
                    iecu_grid.Opacity = 0.5;
                }

                if (acm_grid_permitted)
                {
                    acm_grid.Opacity = 0.5;
                }

                if (tgw2_grid_permitted)
                {
                    tgw2_grid.Opacity = 0.5;
                }

                EmsView.IsVisible = false;
                IecuView.IsVisible = false;
                AcmView.IsVisible = false;
                TgwView.IsVisible = false;

                switch (sen.ClassId)
                {
                    case "EMS":
                        ems_grid.Opacity = 1;
                        EmsView.IsVisible = true;
                        //SelectedECU = "(1.1.0)";
                        break;

                    case "IECU":
                        iecu_grid.Opacity = 1;
                        IecuView.IsVisible = true;
                        //SelectedECU = "(6.2.0)";
                        break;

                    case "ACM":
                        acm_grid.Opacity = 1;
                        AcmView.IsVisible = true;
                        //SelectedECU = "(2.1.0)";
                        break;

                    case "TGW2":
                        tgw2_grid.Opacity = 1;
                        TgwView.IsVisible = true;
                        //SelectedECU = "(12.1.0)";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
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
        public async Task TValue()
        {
            read_pid_android = new ObservableCollection<ReadPidPresponseModel>();
            try
            {
                read_pid_android.Clear();
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        //top = 20;
                        break;
                    case Device.Android:
                        if (App.ConnectedVia == "USB")
                        {
                            Console.WriteLine("Read Responces from PID" + System.DateTime.Now);
                            await Task.Run(async () =>
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadPid(ParameterList);
                            });
                            Debug.WriteLine("------Got Responces from PID USB------" + System.DateTime.Now, "");
                        }
                        else
                        {
                            Console.WriteLine("Read Responces from PID" + System.DateTime.Now);
                            await Task.Run(async () =>
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionWifi>().ReadPid(ParameterList);
                            });
                            Debug.WriteLine("------Got Responces from PID BT------" + System.DateTime.Now, "");
                        }
                        break;
                    case Device.UWP:
                        var read_pid_uwp = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadPid(ParameterList);
                        break;
                    default:
                        //top = 0;
                        break;
                }

                foreach (var pid in read_pid_android)
                {
                    var item = ParameterList.FirstOrDefault(x => x.pidNumber == pid.pidNumber);
                    if (item != null)
                    {
                        if (pid.Status == "NOERROR")
                        {
                            item.show_resolution = pid.responseValue;
                        }
                        else
                        {
                            item.show_resolution = "ERR";
                            item.unit = "";
                        }
                        //item.resolution= Convert.ToDouble(pid.responseValue);
                    }
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    ems_List.ItemsSource = null;
                    viewModel.EMSParameterList = ParameterList;
                    ems_List.ItemsSource = viewModel.EMSParameterList;
                });
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        protected override async void OnDisappearing()
        {
            StartTime = false;
            RecordingStart = false;
        }

        public bool StartTime = false;
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                if (StartTime == false)
                {
                    StartTime = true;
                    PlayPauseIMG.Source = "ic_pause.png";

                    PlayPauseItem.BackgroundColor = Color.FromHex("#3e4095"); ;
                    SnapshotItem.BackgroundColor = Color.Gray;
                    RecordingItem.BackgroundColor = Color.Gray;

                    PlayPauseItem.IsEnabled = true;
                    SnapshotItem.IsEnabled = false;
                    RecordingItem.IsEnabled = false;

                    read_pid_android = new ObservableCollection<ReadPidPresponseModel>();
                    while (StartTime)
                    {
                        //await Task.Run(async () => { await TValue(); });

                        if (SelectedIVN_ParameterList != null)
                        {
                            //Console.WriteLine("Start IVN Send" + System.DateTime.Now);
                            await Task.Run(async () => { await IVN_read_pid(SelectedIVN_ParameterList); });
                        }
                        else if (ParameterList != null)
                        {
                            //Console.WriteLine("Constructor Send" + System.DateTime.Now);                            
                            await Task.Run(async () => { await read_pid(ParameterList); });
                        }
                    }
                }
                else
                {
                    StartTime = false;
                    PlayPauseIMG.Source = "play.png";

                    PlayPauseItem.IsEnabled = true;
                    SnapshotItem.IsEnabled = true;
                    RecordingItem.IsEnabled = true;

                    PlayPauseItem.BackgroundColor = Color.FromHex("#2196F3");
                    SnapshotItem.BackgroundColor = Color.FromHex("#2196F3");
                    RecordingItem.BackgroundColor = Color.FromHex("#2196F3");
                }
            }
            catch (Exception ex)
            {

            }
        }
        protected override bool OnBackButtonPressed()
        {
            return false;
        }
        public bool RecordingStart = false;
        private async void TapGestureRecognizer_Recording(object sender, EventArgs e)
        {
            try
            {
                StartTime = false;
                await Task.Delay(200);

                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
                if (isReachable)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    if (RecordingStart == false)
                    {
                        stopwatch.Start();

                        PlayPauseItem.BackgroundColor = Color.Gray;
                        SnapshotItem.BackgroundColor = Color.Gray;
                        RecordingItem.BackgroundColor = Color.FromHex("#EE204D");

                        PlayPauseItem.IsEnabled = false;
                        SnapshotItem.IsEnabled = false;
                        RecordingItem.IsEnabled = true;

                        RecordingStart = true;

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                            {
                                LabRecordingTimer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");
                                return RecordingStart;
                            });
                        });

                        RecordingIMG.Source = "ic_recording.png";
                    }
                    else
                    {
                        PlayPauseItem.BackgroundColor = Color.FromHex("#2196F3");
                        SnapshotItem.BackgroundColor = Color.FromHex("#2196F3");
                        RecordingItem.BackgroundColor = Color.FromHex("#2196F3");

                        PlayPauseItem.IsEnabled = true;
                        SnapshotItem.IsEnabled = true;
                        RecordingItem.IsEnabled = true;

                        RecordingStart = false;
                    }

                    var LiveRecord = new List<PIDLiveRecord>(); // 1
                    var Live = new List<PidLive>(); // 2
                    var YAxisPoint = new List<YAxisPointName>(); // 3

                    int j = 0;
                    while (RecordingStart)
                    {
                        j++;
                        await Task.Run(async () =>
                        {
                            //await TValue();

                            if (SelectedIVN_ParameterList != null)
                            {
                                //Console.WriteLine("Start IVN Send" + System.DateTime.Now);
                                await Task.Run(async () => { await IVN_read_pid(SelectedIVN_ParameterList); });
                            }
                            else if (ParameterList != null)
                            {
                                //Console.WriteLine("Constructor Send" + System.DateTime.Now);
                                await Task.Run(async () => { await read_pid(ParameterList); });
                            }

                            #region Live
                            foreach (var item1 in viewModel.EMSParameterList)
                            {
                                var isValue = YAxisPoint.Where(x => x.pid_name == item1.pidName);
                                if (isValue != null && isValue.Count() > 0)
                                {
                                    if (isValue.FirstOrDefault().value != null)
                                    {
                                        isValue.FirstOrDefault().value.Add(item1.show_resolution);
                                    }
                                    else
                                    {
                                        isValue.FirstOrDefault().value = new List<string>();
                                        isValue.FirstOrDefault().value.Add(item1.show_resolution);
                                    }
                                }
                                else
                                {
                                    List<string> lst = new List<string>();
                                    lst.Add(item1.show_resolution);
                                    YAxisPoint.Add(new YAxisPointName()
                                    {
                                        pid_name = item1.pidName,
                                        value = lst
                                    });
                                }
                            }
                            #endregion
                        });
                    }

                    if (RecordingStart == false)
                    {
                        using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                        {
                            await Task.Delay(200);
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
                                    await this.DisplayAlert("NO ERROR", "Recording are Saved Successfully", "OK");
                                    stopwatch.Reset();
                                    stopwatch.Stop();

                                    LabRecordingTimer.Text = "00:00";
                                }
                                else
                                {
                                    await this.DisplayAlert("ERROR", "Recording are not Saved Successfully !!!", "OK");
                                }
                            }
                        }
                    }
                }
                else
                {
                    show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                    Snapshot = true;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public bool Snapshot = false;
        private List<IVN_LiveParameterSelectModel> iVN_PidList;

        private async void TapGestureRecognizer_snapshot(object sender, EventArgs e)
        {
            try
            {
                StartTime = false;
                RecordingStart = false;
                await Task.Delay(200);

                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
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

                        await Task.Run(async () =>
                        {
                            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                            {
                                await Task.Delay(200);

                                //await TValue();

                                if (SelectedIVN_ParameterList != null)
                                {
                                    //Console.WriteLine("Start IVN Send" + System.DateTime.Now);
                                    await Task.Run(async () => { await IVN_read_pid(SelectedIVN_ParameterList); });
                                }
                                else if (ParameterList != null)
                                {
                                    //Console.WriteLine("Constructor Send" + System.DateTime.Now);
                                    await Task.Run(async () => { await read_pid(ParameterList); });
                                }

                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    var Values = new List<snapshot_record>();
                                    foreach (var item in read_pid_android)
                                    {
                                        if (item.responseValue == "")
                                        {
                                            Values.Add(new snapshot_record { code = item.pidName, value = "ERR" });
                                        }
                                        else
                                        {
                                            Values.Add(new snapshot_record { code = item.pidName, value = item.responseValue });
                                        }
                                    }
                                    bool GotReturnValues = services.pid_snapshot_record(Values, App.JwtToken, App.SessionId);
                                    if (GotReturnValues == true)
                                    {
                                        await ShowMessage("NO ERROR", "Snapshot Saved Successfully", "OK", async () =>
                                        {
                                            PlayPauseItem.BackgroundColor = Color.FromHex("#2196F3");
                                            SnapshotItem.BackgroundColor = Color.FromHex("#2196F3");
                                            RecordingItem.BackgroundColor = Color.FromHex("#2196F3");

                                            PlayPauseItem.IsEnabled = true;
                                            SnapshotItem.IsEnabled = true;
                                            RecordingItem.IsEnabled = true;

                                            Snapshot = false;
                                        });
                                    }
                                    else
                                    {
                                        await ShowMessage("ERROR", "Snapshot Not Saved Successfully", "OK", async () =>
                                        {
                                            PlayPauseItem.BackgroundColor = Color.FromHex("#2196F3");
                                            SnapshotItem.BackgroundColor = Color.FromHex("#2196F3");
                                            RecordingItem.BackgroundColor = Color.FromHex("#2196F3");

                                            PlayPauseItem.IsEnabled = true;
                                            SnapshotItem.IsEnabled = true;
                                            RecordingItem.IsEnabled = true;

                                            Snapshot = false;
                                        });
                                    }
                                });
                            }
                        });
                    }
                    else
                    {
                        //PlayPauseItem.BackgroundColor = Color.FromHex("#3e4095");
                        //SnapshotItem.BackgroundColor = Color.FromHex("#3e4095");
                        //RecordingItem.BackgroundColor = Color.FromHex("#3e4095");

                        //PlayPauseItem.IsEnabled = true;
                        //SnapshotItem.IsEnabled = true;
                        //RecordingItem.IsEnabled = true;

                        //Snapshot = false;
                    }
                }
                else
                {
                    show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                    Snapshot = false;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task ShowMessage(string message,
            string title,
            string buttonText,
            Action afterHideCallback)
        {
            await DisplayAlert(
                title,
                message,
                buttonText);

            afterHideCallback?.Invoke();
        }
    }
}