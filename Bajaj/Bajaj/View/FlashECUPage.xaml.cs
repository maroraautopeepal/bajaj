using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using MultiEventController;
using MultiEventController.Models;
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
using Bajaj.View.ViewModel;
using Bajaj.Popup;
using Newtonsoft.Json;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FlashECUPage : DisplayAlertPage
    {
        JobCardListModel jobCard;
        FlashEcuViewModel viewModel;
        ApiServices services;
        string read_json_file = string.Empty;
        public string read_interpreter_file = string.Empty;
        public FlashECUPage(JobCardListModel jobCardSession)
        {
            try
            {
                InitializeComponent();
                jobCard = jobCardSession;

                BindingContext = viewModel = new FlashEcuViewModel();

                services = new ApiServices();
                GetFlashFileList();
            }
            catch (Exception ex)
            {

            }
        }


        protected async override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
            }
            catch (Exception ex)
            {

            }
        }


        FlashData flash_data;
        ModelResult model;
        SubModel sub_model;
        Ecu ecu1;
        Ecu2 Ecu2;
        int count1 = 0;
        public async void GetFlashFileList()
        {
            try
            {
                viewModel.ecus_list = new ObservableCollection<FlashEcusModel>();
                viewModel.flash_file_list = new ObservableCollection<File>();
                viewModel.static_flash_file_list = new ObservableCollection<File>();

                if (!CurrentUserEvent.Instance.IsExpert)
                {
                    UserDialogs.Instance.ShowLoading("Loading...");
                    await Task.Delay(100);
                    try
                    {
                        count1 = 0;
                        viewModel.all_models_list = await services.get_all_models(App.JwtToken, 0);
                        foreach (var ecu in StaticData.ecu_info)
                        {
                            count1++;
                            int pid_dataset = ecu.pid_dataset_id;
                            //var pid_li = await services.get_pid(App.JwtToken, pid_dataset);
                            //viewModel.flash_file_list= viewModel.all_models_list.results.FirstOrDefault(x=>x.id== jobCard.model_id),
                            model = viewModel.all_models_list.results.FirstOrDefault(x => x.name == jobCard.vehicle_model);
                            if (model != null && model.sub_models.Any())
                            {
                                sub_model = model.sub_models.FirstOrDefault(x => x.id == jobCard.sub_model_id);
                                if (sub_model != null && sub_model.ecu_submodel != null)
                                {
                                    //var ecu2 = sub_model.ecu_submodel.FirstOrDefault(x => x.ecu == ecu.ecu_ID);
                                    Ecu2 = await services.getFlashRecords(App.JwtToken, ecu.ecu_ID);
                                }
                            }

                            if (Ecu2 != null)
                            {
                                viewModel.ecus_list.Add(
                                                       new FlashEcusModel
                                                       {
                                                           ecu_name = ecu.ecu_name,
                                                           opacity = count1 == 1 ? 1 : .5,
                                                           flash_file_list = Ecu2.file,
                                                           seqFileUrl = Ecu2.sequence_file,
                                                           ecu_map_file = Ecu2.ecu_map_file,
                                                           SeedkeyalgoFnIndex_Values = ecu.seed_key_index,
                                                           ecu2 = Ecu2
                                                       }); 
                            }
                            else
                            {
                                viewModel.ecus_list.Add(
                                                       new FlashEcusModel
                                                       {
                                                           ecu_name = ecu.ecu_name,
                                                           opacity = count1 == 1 ? 1 : .5,
                                                           SeedkeyalgoFnIndex_Values = ecu.seed_key_index,
                                                       });
                            }
                        }
                        viewModel.static_flash_file_list = viewModel.flash_file_list = new ObservableCollection<File>();

                        //foreach (var item in viewModel.ecus_list)
                        //{
                        //viewModel.static_flash_file_list.Add(flash_file_list);
                        //}
                        viewModel.static_flash_file_list = viewModel.flash_file_list = new ObservableCollection<File>(viewModel.ecus_list.FirstOrDefault().flash_file_list);
                        viewModel.selected_ecu = viewModel.ecus_list[0];
                        Ecu2 = viewModel.selected_ecu.ecu2;

                        if (App.ConnectedVia == "USB")
                        {
                            DependencyService.Get<Interfaces.IConnectionUSB>().SetDongleProperties(viewModel.selected_ecu.ecu2.protocol.autopeepal,
                            viewModel.selected_ecu.ecu2.tx_header,
                            viewModel.selected_ecu.ecu2.rx_header);
                        }
                        else if (App.ConnectedVia == "BT")
                        {
                            await DependencyService.Get<Interfaces.IBth>().SetDongleProperties(viewModel.selected_ecu.ecu2.protocol.autopeepal,
                                                    viewModel.selected_ecu.ecu2.tx_header,
                                                    viewModel.selected_ecu.ecu2.rx_header);
                        }

                    }
                    catch (Exception ex)
                    {
                        App.controlEventManager.SendRequestData("Exception*#" + ex.Message);
                        var Alert = new Popup.DisplayAlertPage("Error", ex.Message, "Ok");
                        await PopupNavigation.Instance.PushAsync(Alert);
                    }
                    UserDialogs.Instance.HideLoading();
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

        private void OpenPopupClicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"OpenPopupFrame",
                        ElementValue = "OpenPopupClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                viewModel.flash_file_view_visible = true;
            }
            catch (Exception ex)
            {

            }
        }


        private async void SelectFlashFileClicked(object sender, EventArgs e)
        {
            try
            {
                viewModel.selected_flash_file = (File)((Grid)sender).BindingContext;

                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = $"{viewModel.selected_flash_file.id}",
                    ElementValue = $"SelectFlashFileClicked",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });

                SelectFlashFileClicked();
            }
            catch (Exception ex)
            {
            }
        }

        private async void SelectFlashFileClicked()
        {
            try
            {
                //viewModel.selected_flash_file = (File)((Grid)sender).BindingContext;

                viewModel.flash_file_view_visible = false;

                var Alert = new Popup.DisplayAlertPage("Alert", $"Are you sure you want to Flash the dataset", "Ok", "Cancel");
                await PopupNavigation.Instance.PushAsync(Alert);
                bool alertResult = await Alert.PopupClosedTask;

                if (alertResult)
                {

                    await PopupNavigation.Instance.PopAllAsync();
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(200);
                        flash_data = new FlashData
                        {
                            ecu2 = viewModel.selected_ecu.ecu2,
                            file = viewModel.selected_flash_file,
                            seqFileUrl = viewModel.selected_ecu.seqFileUrl,
                            SeedkeyalgoFnIndex_Values = viewModel.selected_ecu.SeedkeyalgoFnIndex_Values,
                            ecu_map_file = viewModel.selected_ecu.ecu_map_file
                        };
                        viewModel.select_dataset = flash_data.file.data_file_name;
                        if (CurrentUserEvent.Instance.IsExpert)
                        {
                            DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                        }
                        if (!CurrentUserEvent.Instance.IsExpert)
                        {
                            read_json_file = await ReadJson();
                            read_interpreter_file = await ReadInterpreterFile();
                            Timerr();
                        }

                        if (CurrentUserEvent.Instance.IsExpert)
                        {
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }


                        DependencyService.Get<IToastMessage>().Show("------Started ECU Flashing------");
                        if (!CurrentUserEvent.Instance.IsExpert)
                        {
                            StartFlashing();
                        }
                    }
                }
                else
                {
                    await PopupNavigation.Instance.PopAllAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }


        string flashing_result;
        public async Task<string> ReadJson()
        {
            try
            {
                using (UserDialogs.Instance.Loading("Downloading File...", null, null, true, MaskType.Black))
                {
                    return await services.read_json_file(viewModel.selected_flash_file.data_file);
                }
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public async Task<string> ReadInterpreterFile()
        {
            try
            {
                using (UserDialogs.Instance.Loading("Downloading Sequence File...", null, null, true, MaskType.Black))
                {
                    return await services.read_json_file(viewModel.selected_ecu.seqFileUrl);
                }
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }
        


        Stopwatch stopwatch;
        bool timer_status = false;
        public async void Timerr()
        {
            try
            {
                stopwatch = new Stopwatch();
                stopwatch.Reset();
                stopwatch.Start();
                timer_status = true;
                viewModel.timer_visible = true;
                //viewModel.progress_visible = true;
                viewModel.flash_timer = "00 : 00";
                viewModel.flashProgress_visible = true;
                //await Task.Delay(1000);
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    viewModel.flash_timer = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");
                    return timer_status;
                });

                float flashPercent = 0;

                Device.StartTimer(TimeSpan.FromSeconds(5), () =>
                {
                    if (App.ConnectedVia == "USB")
                        flashPercent = DependencyService.Get<IConnectionUSB>().FlashingData().Result;
                    else if (App.ConnectedVia == "BT")
                        flashPercent = DependencyService.Get<IBth>().FlashingData().Result;
                    else
                        flashPercent = DependencyService.Get<IConnectionWifi>().FlashingData().Result;
                    viewModel.flash_percent = String.Format("{0:0.0}%", flashPercent * 100);
                    progressBar.ProgressTo(flashPercent, 500, Easing.Linear);
                    return timer_status;
                });
            }
            catch (Exception ex)
            {

            }
        }


        List<flash_record> FlashRecord;
        string ServerMessage = string.Empty;
        string MessageTitle = string.Empty;
        string Message = string.Empty;
        string Status = string.Empty;

        string cvn_after_flash = string.Empty;
        string cvn_before_flash = string.Empty;
        public async void StartFlashing()
        {
            try
            {
                FlashRecord = new List<flash_record>();

                ServerMessage = string.Empty;
                MessageTitle = string.Empty;
                Message = string.Empty;
                Status = string.Empty;

                App.controlEventManager.SendRequestData("StartTimer*#");
                App.controlEventManager.SendRequestData("ExtraViewVisibleTrue*#");
                viewModel.extra_view_visible = true;
                cvn_before_flash = "NA";

                await Task.Delay(1000);

                NavigationPage.SetHasBackButton(this, false);

                await Task.Run(async () =>
                {
                    if (App.ConnectedVia == "USB")
                    {
                        flashing_result = await DependencyService.Get<IConnectionUSB>()
                        .StartECUFlashing(
                        read_json_file,
                        read_interpreter_file,
                        flash_data.ecu2,
                        flash_data.SeedkeyalgoFnIndex_Values,
                        viewModel.selected_ecu.ecu_map_file);
                    }
                    else if (App.ConnectedVia == "BT")
                    {
                        flashing_result = await DependencyService.Get<IBth>()
                        .StartECUFlashing(
                        read_json_file,
                        read_interpreter_file,
                        flash_data.ecu2,
                        flash_data.SeedkeyalgoFnIndex_Values,
                        viewModel.selected_ecu.ecu_map_file);
                    }
                    else if (App.ConnectedVia == "WIFI")
                    {
                        flashing_result = await DependencyService.Get<IConnectionWifi>()
                        .StartECUFlashing(
                        read_json_file,
                        read_interpreter_file,
                        flash_data.ecu2,
                        flash_data.SeedkeyalgoFnIndex_Values,
                        viewModel.selected_ecu.ecu_map_file);
                    }
                });


                cvn_after_flash = viewModel.selected_flash_file.data_file_name;
                stopwatch.Stop();
                timer_status = false;

                App.controlEventManager.SendRequestData("StopTimer*#");
                NavigationPage.SetHasBackButton(this, false);
                //Device.BeginInvokeOnMainThread(async () =>
                //{
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    //await Task.Delay(100);
                    if (string.IsNullOrEmpty(flashing_result))
                    {
                        MessageTitle = "ERROR";
                        Message = "Flashing Stopped with following error";
                        Status = string.Empty;
                        FlashRecord.Add(new flash_record { flash_duration = viewModel.flash_timer, status = "fail", cvn_after_flash = cvn_after_flash, cvn_before_flash = cvn_before_flash });
                        //App.controlEventManager.SendRequestData("ERROR*#" + "Flashing Stopped with following error");
                        //var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Flashing Stopped with following error", "OK");
                        //await PopupNavigation.Instance.PushAsync(RecordingSaved);
                    }
                    else if (flashing_result != "NOERROR")
                    {
                        MessageTitle = "ERROR";
                        Message = "Flashing Stopped with following error";
                        Status = flashing_result;
                        FlashRecord.Add(new flash_record { flash_duration = viewModel.flash_timer, status = "fail", cvn_after_flash = cvn_after_flash, cvn_before_flash = cvn_before_flash });
                        //App.controlEventManager.SendRequestData("ERROR*#" + flashing_result);
                        //var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Flashing Stopped with following error \n" + flashing_result, "OK");
                        //await PopupNavigation.Instance.PushAsync(RecordingSaved);
                    }
                    else
                    {
                        MessageTitle = "Successful";
                        Message = "Flashing Successful";
                        Status = string.Empty;
                        FlashRecord.Add(new flash_record { flash_duration = viewModel.flash_timer, status = "pass", cvn_after_flash = cvn_after_flash, cvn_before_flash = cvn_before_flash });
                        //App.controlEventManager.SendRequestData("SUCCESS*#" + "Flashing Successful !!");
                        //var RecordingSaved = new Popup.DisplayAlertPage("Successful", "Flashing Successful !!", "OK");
                        //await PopupNavigation.Instance.PushAsync(RecordingSaved);
                    }
                }
                //});

                if (FlashRecord.Count > 0)
                {
                    var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                    if (isReachable)
                    {

                        var server_response = await services.flash_record(FlashRecord, App.JwtToken, App.SessionId);
                        if (server_response)
                        {
                            ServerMessage = "Parameter data saved";
                        }
                        else
                        {
                            ServerMessage = "Parameter data not saved";
                        }
                        //App.controlEventManager.SendRequestData($"SeverMessage*#{ServerMessage}");
                    }
                    else
                    {
                        var RecordingSaved1 = new Popup.DisplayAlertPage("Please check Internet Connection", "Internet Connection Problem", "OK");
                        await PopupNavigation.Instance.PushAsync(RecordingSaved1);
                        //show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                    }

                    //SelectedPidValue = "";
                    //NewPidValue = "";
                }

                App.controlEventManager.SendRequestData("Navigate#" + $"{MessageTitle}#{Message}#{Status}#{ServerMessage}");
                var RecordingSaved2 = new Popup.DisplayAlertPage(MessageTitle, $"{Message}\n{Status}\n{ServerMessage}", "OK");
                await PopupNavigation.Instance.PushAsync(RecordingSaved2);

                //App.controlEventManager.SendRequestData("ExtraViewVisibleFalse*#");
                viewModel.extra_view_visible = false;
                await Navigation.PushAsync(new AppFeaturePage());
            }
            catch (Exception ex)
            {

            }
        }

        //public async Task read_json()
        //{
        //    try
        //    {
        //        using (UserDialogs.Instance.Loading("Checking Internet Connection...", null, null, true, MaskType.Black))
        //        {
        //            await Task.Delay(200);
        //            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
        //            if (isReachable)
        //            {
        //                using (UserDialogs.Instance.Loading("Downloading File...", null, null, true, MaskType.Black))
        //                {
        //                    var dataFile = model.data_file;
        //                    await Task.Delay(200);
        //                    read_json_file = await services.read_json_file(model.data_file);
        //                    await Task.Delay(200);
        //                    if (file != "")
        //                    {
        //                        if (App.ConnectedVia == "USB")
        //                        {
        //                            Device.BeginInvokeOnMainThread(async () =>
        //                            {
        //                                        //var result = await Application.Current.MainPage.DisplayAlert("Alert", txt_ecu.Text, "Yes", "No");
        //                                        //if (result)
        //                                        App.controlEventManager.SendRequestData("StarterFlashingAlertOKCancel_" + txt_ecu.Text);
        //                                var Alert = new Popup.DisplayAlertPage("Alert", txt_ecu.Text, "Ok", "Cancel");
        //                                await PopupNavigation.Instance.PushAsync(Alert);
        //                                bool alertResult = await Alert.PopupClosedTask;
        //                                if (alertResult)
        //                                {
        //                                    using (UserDialogs.Instance.Loading("ECU Flashing Started...", null, null, true, MaskType.Black))
        //                                    {
        //                                        await Task.Delay(1000);

        //                                        Device.BeginInvokeOnMainThread(async () =>
        //                                        {
        //                                            Stopwatch stopwatch = new Stopwatch();
        //                                            stopwatch.Reset();
        //                                            stopwatch.Start();

        //                                            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        //                                            {
        //                                                Timer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");
        //                                                Task.Run(async () =>
        //                                                {
        //                                                    App.controlEventManager.SendRequestData("SendTimer_" + Timer.Text);
        //                                                    return "";
        //                                                });
        //                                                return TimerStopOrNot;
        //                                            });

        //                                            DependencyService.Get<IToastMessage>().Show("------Started ECU Flashing------");

        //                                            object USB_startECUFlashing;
        //                                            string CvnBeforeFlash = string.Empty;
        //                                            string CvnAfterFlash = string.Empty;
        //                                            using (UserDialogs.Instance.Loading("Flashing...", null, null, true, MaskType.Black))
        //                                            {
        //                                                await Task.Delay(200);

        //                                                        //var BeforeValue = await ReadCVN();
        //                                                        //CvnBeforeFlash = Convert.ToString(BeforeValue.FirstOrDefault()?.responseValue);
        //                                                        //IConnectionWifi
        //                                                        USB_startECUFlashing = await DependencyService.
        //                                                Get<IConnectionUSB>().StartECUFlashing(file, ecu2, SeedkeyalgoFnIndex_Flash, ecu_map_file);
        //                                                        //USB_startECUFlashing = await DependencyService.Get<IConnectionWifi>().StartECUFlashing(file, ecu2, SeedkeyalgoFnIndex_Flash, ecu_map_file);
        //                                                    }
        //                                            if (USB_startECUFlashing == "")
        //                                            {
        //                                                stopwatch.Stop();
        //                                                TimerStopOrNot = false;
        //                                                        //await this.DisplayAlert("ERROR", "Unknown ERROR!! \n Try Flashing Again", "OK");
        //                                                        App.controlEventManager.SendRequestData("ERROR_Unknown");
        //                                                var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Unknown ERROR!! \n Try Flashing Again", "OK");
        //                                                await PopupNavigation.Instance.PushAsync(RecordingSaved);
        //                                                stopwatch.Reset();
        //                                            }
        //                                            else if (USB_startECUFlashing != "NOERROR")
        //                                            {
        //                                                stopwatch.Stop();
        //                                                TimerStopOrNot = false;
        //                                                        //await Application.Current.MainPage.DisplayAlert("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(USB_startECUFlashing), "OK");
        //                                                        //controlEventManager.SendRequestData("ERROR_Flashing_Stopped" + "Flashing Stopped with following error \n" + Convert.ToString(USB_startECUFlashing));
        //                                                        //var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(USB_startECUFlashing), "OK");
        //                                                        //await PopupNavigation.Instance.PushAsync(RecordingSaved);
        //                                                        stopwatch.Reset();

        //                                                var isInternet = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
        //                                                if (isInternet)
        //                                                {
        //                                                    await Task.Delay(200);
        //                                                            //var AfterValue = await ReadCVN();
        //                                                            //CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                                                            var FlashRecord = new List<flash_record>();
        //                                                    FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "fail", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                                                    services.flash_record(FlashRecord, App.JwtToken, App.SessionId);
        //                                                }

        //                                                stopwatch.Reset();
        //                                                stopwatch.Stop();

        //                                                Device.BeginInvokeOnMainThread(async () =>
        //                                                {
        //                                                    App.controlEventManager.SendRequestData("ERROR_Flashing_Stopped" + "Flashing Stopped with following error \n" + Convert.ToString(USB_startECUFlashing));
        //                                                    await Task.Delay(200);
        //                                                    var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(USB_startECUFlashing), "OK");
        //                                                    await PopupNavigation.Instance.PushAsync(RecordingSaved);

        //                                                    await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion, null, null, string.Empty));
        //                                                });
        //                                            }
        //                                            else
        //                                            {
        //                                                using (UserDialogs.Instance.Loading("Reading CVN...", null, null, true, MaskType.Black))
        //                                                {
        //                                                    await Task.Delay(10000);

        //                                                            //var AfterValue = await ReadCVN();
        //                                                            //CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                                                            var FlashRecord = new List<flash_record>();
        //                                                    FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "pass", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                                                    services.flash_record(FlashRecord, App.JwtToken, App.SessionId);

        //                                                    stopwatch.Stop();
        //                                                    TimerStopOrNot = false;
        //                                                    stopwatch.Reset();

        //                                                    Timer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");

        //                                                    Device.BeginInvokeOnMainThread(async () =>
        //                                                    {
        //                                                                //await Application.Current.MainPage.DisplayAlert("Successful", "Flashing Successful !!", "OK");

        //                                                                App.controlEventManager.SendRequestData("Flashing_Successful");
        //                                                        await Task.Delay(200);
        //                                                        var RecordingSaved = new Popup.DisplayAlertPage("Successful", "Flashing Successful !!", "OK");
        //                                                        await PopupNavigation.Instance.PushAsync(RecordingSaved);
        //                                                        await Task.Delay(200);

        //                                                        await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion, null, null, string.Empty));
        //                                                    });
        //                                                }
        //                                            }
        //                                        });
        //                                    }
        //                                }
        //                            });
        //                        }
        //                        else if (App.ConnectedVia == "BT")
        //                        {
        //                            Device.BeginInvokeOnMainThread(async () =>
        //                            {
        //                                        //var result = await Application.Current.MainPage.DisplayAlert("Alert", txt_ecu.Text, "Yes", "No");
        //                                        //if (result)
        //                                        App.controlEventManager.SendRequestData("StarterFlashingAlertOKCancel_" + txt_ecu.Text);
        //                                var Alert = new Popup.DisplayAlertPage("Alert", txt_ecu.Text, "Ok", "Cancel");
        //                                await PopupNavigation.Instance.PushAsync(Alert);
        //                                bool alertResult = await Alert.PopupClosedTask;
        //                                if (alertResult)
        //                                {
        //                                    using (UserDialogs.Instance.Loading("ECU Flashing Started...", null, null, true, MaskType.Black))
        //                                    {
        //                                        await Task.Delay(1000);

        //                                        Device.BeginInvokeOnMainThread(async () =>
        //                                        {
        //                                            Stopwatch stopwatch = new Stopwatch();
        //                                            stopwatch.Reset();
        //                                            stopwatch.Start();

        //                                            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        //                                            {
        //                                                Timer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");
        //                                                        //Task.Run(async () =>
        //                                                        //{
        //                                                        //    App.controlEventManager.SendRequestData("SendTimer_" + Timer.Text);
        //                                                        //    return "";
        //                                                        //});
        //                                                        return TimerStopOrNot;
        //                                            });

        //                                            DependencyService.Get<IToastMessage>().Show("------Started ECU Flashing------");

        //                                            object startECUFlashing;
        //                                            string CvnBeforeFlash = string.Empty;
        //                                            string CvnAfterFlash = string.Empty;
        //                                            using (UserDialogs.Instance.Loading("Flashing...", null, null, true, MaskType.Black))
        //                                            {
        //                                                await Task.Delay(200);

        //                                                        //var BeforeValue = await ReadCVN();
        //                                                        //CvnBeforeFlash = Convert.ToString(BeforeValue.FirstOrDefault()?.responseValue);

        //                                                        startECUFlashing = await DependencyService.Get<IBth>().StartECUFlashing(file, ecu2, SeedkeyalgoFnIndex_Flash, ecu_map_file);
        //                                            }
        //                                            if (startECUFlashing == "")
        //                                            {
        //                                                stopwatch.Stop();
        //                                                TimerStopOrNot = false;
        //                                                        //await this.DisplayAlert("ERROR", "Unknown ERROR!! \n Try Flashing Again", "OK");
        //                                                        App.controlEventManager.SendRequestData("ERROR_Unknown");
        //                                                var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Unknown ERROR!! \n Try Flashing Again", "OK");
        //                                                await PopupNavigation.Instance.PushAsync(RecordingSaved);
        //                                                stopwatch.Reset();
        //                                            }
        //                                            else if (startECUFlashing != "NOERROR")
        //                                            {
        //                                                stopwatch.Stop();
        //                                                TimerStopOrNot = false;
        //                                                        //await Application.Current.MainPage.DisplayAlert("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(USB_startECUFlashing), "OK");
        //                                                        //controlEventManager.SendRequestData("ERROR_Flashing_Stopped" + "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing));
        //                                                        //var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing), "OK");
        //                                                        //await PopupNavigation.Instance.PushAsync(RecordingSaved);
        //                                                        stopwatch.Reset();

        //                                                var isInternet = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
        //                                                if (isInternet)
        //                                                {
        //                                                    await Task.Delay(200);
        //                                                            //var AfterValue = await ReadCVN();
        //                                                            //CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                                                            var FlashRecord = new List<flash_record>();
        //                                                    FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "fail", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                                                    services.flash_record(FlashRecord, App.JwtToken, App.SessionId);
        //                                                }
        //                                                        //Timer.Text = "00:00";
        //                                                        stopwatch.Reset();
        //                                                stopwatch.Stop();

        //                                                Device.BeginInvokeOnMainThread(async () =>
        //                                                {
        //                                                    App.controlEventManager.SendRequestData("ERROR_Flashing_Stopped" + "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing));
        //                                                    var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing), "OK");
        //                                                    await PopupNavigation.Instance.PushAsync(RecordingSaved);

        //                                                    await Task.Delay(200);
        //                                                    await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion, null, null, string.Empty));
        //                                                });
        //                                            }
        //                                            else
        //                                            {
        //                                                using (UserDialogs.Instance.Loading("Reading CVN...", null, null, true, MaskType.Black))
        //                                                {
        //                                                    await Task.Delay(10000);

        //                                                            //var AfterValue = await ReadCVN();
        //                                                            //CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                                                            var FlashRecord = new List<flash_record>();
        //                                                    FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "pass", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                                                    services.flash_record(FlashRecord, App.JwtToken, App.SessionId);

        //                                                    stopwatch.Stop();
        //                                                    TimerStopOrNot = false;
        //                                                    stopwatch.Reset();

        //                                                    Timer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");

        //                                                    Device.BeginInvokeOnMainThread(async () =>
        //                                                    {
        //                                                                //await Application.Current.MainPage.DisplayAlert("Successful", "Flashing Successful !!", "OK");

        //                                                                App.controlEventManager.SendRequestData("Flashing_Successful");
        //                                                        var RecordingSaved = new Popup.DisplayAlertPage("Successful", "Flashing Successful !!", "OK");
        //                                                        await PopupNavigation.Instance.PushAsync(RecordingSaved);

        //                                                        await Task.Delay(200);
        //                                                        await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion, null, null, string.Empty));
        //                                                    });
        //                                                }
        //                                            }
        //                                        });
        //                                    }
        //                                }
        //                            });

        //                            #region Old Code
        //                            //Device.BeginInvokeOnMainThread(async () =>
        //                            //{
        //                            //    var result = await DisplayAlert("Alert", txt_ecu.Text, "Ok", "Cancel");
        //                            //    if (result)
        //                            //    {
        //                            //        await Task.Delay(1000);
        //                            //        Device.BeginInvokeOnMainThread(async () =>
        //                            //        {
        //                            //            Stopwatch stopwatch = new Stopwatch();
        //                            //            stopwatch.Reset();
        //                            //            stopwatch.Start();

        //                            //            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        //                            //            {
        //                            //                Timer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");
        //                            //                return true;
        //                            //            });

        //                            //            object startECUFlashing;
        //                            //            string CvnBeforeFlash = string.Empty;
        //                            //            string CvnAfterFlash = string.Empty;
        //                            //            using (UserDialogs.Instance.Loading("Flashing...", null, null, true, MaskType.Black))
        //                            //            {
        //                            //                await Task.Delay(200);
        //                            //                var BeforeValue = await ReadCVN();
        //                            //                CvnBeforeFlash = Convert.ToString(BeforeValue.FirstOrDefault()?.responseValue);

        //                            //                startECUFlashing = await DependencyService.Get<IBth>().StartECUFlashing(file, ecu2, SeedkeyalgoFnIndex_Flash, ecu_map_file);
        //                            //            }
        //                            //            if (startECUFlashing == "")
        //                            //            {
        //                            //                stopwatch.Stop();
        //                            //                await this.DisplayAlert("ERROR", "Unknown ERROR!! \n Try Flashing Again", "OK");

        //                            //                stopwatch.Reset();
        //                            //            }
        //                            //            else if (startECUFlashing != "NOERROR")
        //                            //            {
        //                            //                stopwatch.Stop();
        //                            //                stopwatch.Reset();
        //                            //                await this.DisplayAlert("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing), "OK");

        //                            //                await Task.Delay(200);
        //                            //                var AfterValue = await ReadCVN();
        //                            //                CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                            //                var FlashRecord = new List<flash_record>();
        //                            //                FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "fail", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                            //                services.flash_record(FlashRecord, App.JwtToken, App.SessionId);

        //                            //                //Timer.Text = "00:00";
        //                            //                await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion));
        //                            //            }
        //                            //            else
        //                            //            {
        //                            //                stopwatch.Stop();
        //                            //                await this.DisplayAlert("Successful", "Flashing Successful !!", "OK");
        //                            //                stopwatch.Reset();

        //                            //                await Task.Delay(200);
        //                            //                var AfterValue = await ReadCVN();
        //                            //                CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                            //                var FlashRecord = new List<flash_record>();
        //                            //                FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "pass", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                            //                services.flash_record(FlashRecord, App.JwtToken, App.SessionId);

        //                            //                await Task.Delay(200);
        //                            //                await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion));
        //                            //            }
        //                            //        });

        //                            //        //var updateRate = 100 / 30f; // 30Hz
        //                            //        //double step = updateRate / (2 * 30 * 100f);
        //                            //        //Device.StartTimer(TimeSpan.FromMilliseconds(updateRate), () =>
        //                            //        //{
        //                            //        //    if (ProcessBar.Progress < 100)
        //                            //        //    {
        //                            //        //        Device.BeginInvokeOnMainThread(() => ProcessBar.Progress += step);
        //                            //        //        return true;
        //                            //        //    }
        //                            //        //    return false;
        //                            //        //});
        //                            //    }
        //                            //    // UserDialogs.Instance.HideLoading();
        //                            //    //stop loader
        //                            //});
        //                            #endregion
        //                        }
        //                        else
        //                        {
        //                            Device.BeginInvokeOnMainThread(async () =>
        //                            {
        //                                        //var result = await Application.Current.MainPage.DisplayAlert("Alert", txt_ecu.Text, "Yes", "No");
        //                                        //if (result)
        //                                        App.controlEventManager.SendRequestData("StarterFlashingAlertOKCancel_" + txt_ecu.Text);
        //                                var Alert = new Popup.DisplayAlertPage("Alert", txt_ecu.Text, "Ok", "Cancel");
        //                                await PopupNavigation.Instance.PushAsync(Alert);
        //                                bool alertResult = await Alert.PopupClosedTask;
        //                                if (alertResult)
        //                                {
        //                                    using (UserDialogs.Instance.Loading("ECU Flashing Started...", null, null, true, MaskType.Black))
        //                                    {
        //                                        await Task.Delay(1000);

        //                                        Device.BeginInvokeOnMainThread(async () =>
        //                                        {
        //                                            Stopwatch stopwatch = new Stopwatch();
        //                                            stopwatch.Reset();
        //                                            stopwatch.Start();

        //                                            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        //                                            {
        //                                                Timer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");
        //                                                return TimerStopOrNot;
        //                                            });

        //                                            DependencyService.Get<IToastMessage>().Show("------Started ECU Flashing------");

        //                                            object startECUFlashing;
        //                                            string CvnBeforeFlash = string.Empty;
        //                                            string CvnAfterFlash = string.Empty;
        //                                            using (UserDialogs.Instance.Loading("Flashing...", null, null, true, MaskType.Black))
        //                                            {
        //                                                await Task.Delay(200);

        //                                                        //var BeforeValue = await ReadCVN();
        //                                                        //CvnBeforeFlash = Convert.ToString(BeforeValue.FirstOrDefault()?.responseValue);

        //                                                        startECUFlashing = await DependencyService.Get<IConnectionWifi>()
        //                                                .StartECUFlashing(file, ecu2, SeedkeyalgoFnIndex_Flash, ecu_map_file);
        //                                            }
        //                                            if (startECUFlashing == "")
        //                                            {
        //                                                stopwatch.Stop();
        //                                                TimerStopOrNot = false;
        //                                                        //await this.DisplayAlert("ERROR", "Unknown ERROR!! \n Try Flashing Again", "OK");
        //                                                        App.controlEventManager.SendRequestData("ERROR_Unknown");
        //                                                var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Unknown ERROR!! \n Try Flashing Again", "OK");
        //                                                await PopupNavigation.Instance.PushAsync(RecordingSaved);
        //                                                stopwatch.Reset();
        //                                            }
        //                                            else if (startECUFlashing != "NOERROR")
        //                                            {
        //                                                stopwatch.Stop();
        //                                                TimerStopOrNot = false;
        //                                                        //await Application.Current.MainPage.DisplayAlert("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(USB_startECUFlashing), "OK");
        //                                                        //controlEventManager.SendRequestData("ERROR_Flashing_Stopped" + "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing));
        //                                                        //var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing), "OK");
        //                                                        //await PopupNavigation.Instance.PushAsync(RecordingSaved);
        //                                                        stopwatch.Reset();

        //                                                var isInternet = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
        //                                                if (isInternet)
        //                                                {
        //                                                    await Task.Delay(200);
        //                                                            //var AfterValue = await ReadCVN();
        //                                                            //CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                                                            var FlashRecord = new List<flash_record>();
        //                                                    FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "fail", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                                                    services.flash_record(FlashRecord, App.JwtToken, App.SessionId);
        //                                                }
        //                                                        //Timer.Text = "00:00";
        //                                                        stopwatch.Reset();
        //                                                stopwatch.Stop();
        //                                                Device.BeginInvokeOnMainThread(async () =>
        //                                                {
        //                                                    App.controlEventManager.SendRequestData("ERROR_Flashing_Stopped" + "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing));
        //                                                    var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing), "OK");
        //                                                    await PopupNavigation.Instance.PushAsync(RecordingSaved);

        //                                                    await Task.Delay(200);
        //                                                    await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion, null, null, string.Empty));
        //                                                });
        //                                            }
        //                                            else
        //                                            {
        //                                                using (UserDialogs.Instance.Loading("Reading CVN...", null, null, true, MaskType.Black))
        //                                                {
        //                                                    await Task.Delay(10000);

        //                                                            //var AfterValue = await ReadCVN();
        //                                                            //CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                                                            var FlashRecord = new List<flash_record>();
        //                                                    FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "pass", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                                                    services.flash_record(FlashRecord, App.JwtToken, App.SessionId);

        //                                                    stopwatch.Stop();
        //                                                    TimerStopOrNot = false;
        //                                                    stopwatch.Reset();

        //                                                    Timer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");

        //                                                    Device.BeginInvokeOnMainThread(async () =>
        //                                                    {
        //                                                                //await Application.Current.MainPage.DisplayAlert("Successful", "Flashing Successful !!", "OK");

        //                                                                App.controlEventManager.SendRequestData("Flashing_Successful");
        //                                                        var RecordingSaved = new Popup.DisplayAlertPage("Successful", "Flashing Successful !!", "OK");
        //                                                        await PopupNavigation.Instance.PushAsync(RecordingSaved);

        //                                                        await Task.Delay(200);
        //                                                        await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion, null, null, string.Empty));
        //                                                    });
        //                                                }
        //                                            }
        //                                        });
        //                                    }
        //                                }
        //                            });

        //                            #region Old Code
        //                            //Device.BeginInvokeOnMainThread(async () =>
        //                            //{
        //                            //    var result = await DisplayAlert("Alert", txt_ecu.Text, "Ok", "Cancel");
        //                            //    if (result)
        //                            //    {
        //                            //        await Task.Delay(1000);
        //                            //        Device.BeginInvokeOnMainThread(async () =>
        //                            //        {
        //                            //            Stopwatch stopwatch = new Stopwatch();
        //                            //            stopwatch.Reset();
        //                            //            stopwatch.Start();

        //                            //            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        //                            //            {
        //                            //                Timer.Text = stopwatch.Elapsed.Minutes.ToString("00") + " : " + stopwatch.Elapsed.Seconds.ToString("00");
        //                            //                return true;
        //                            //            });

        //                            //            object startECUFlashing;
        //                            //            string CvnBeforeFlash = string.Empty;
        //                            //            string CvnAfterFlash = string.Empty;
        //                            //            using (UserDialogs.Instance.Loading("Flashing...", null, null, true, MaskType.Black))
        //                            //            {
        //                            //                await Task.Delay(200);
        //                            //                var BeforeValue = await ReadCVN();
        //                            //                CvnBeforeFlash = Convert.ToString(BeforeValue.FirstOrDefault()?.responseValue);

        //                            //                startECUFlashing = await DependencyService.Get<IConnectionWifi>().StartECUFlashing(file, ecu2, SeedkeyalgoFnIndex_Flash, ecu_map_file);
        //                            //            }
        //                            //            if (startECUFlashing == "")
        //                            //            {
        //                            //                stopwatch.Stop();
        //                            //                await this.DisplayAlert("ERROR", "Unknown ERROR!! \n Try Flashing Again", "OK");

        //                            //                stopwatch.Reset();
        //                            //            }
        //                            //            else if (startECUFlashing != "NOERROR")
        //                            //            {
        //                            //                stopwatch.Stop();
        //                            //                stopwatch.Reset();
        //                            //                await this.DisplayAlert("ERROR", "Flashing Stopped with following error \n" + Convert.ToString(startECUFlashing), "OK");

        //                            //                await Task.Delay(200);
        //                            //                var AfterValue = await ReadCVN();
        //                            //                CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                            //                var FlashRecord = new List<flash_record>();
        //                            //                FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "fail", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                            //                services.flash_record(FlashRecord, App.JwtToken, App.SessionId);

        //                            //                //Timer.Text = "00:00";
        //                            //                await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion));
        //                            //            }
        //                            //            else
        //                            //            {
        //                            //                stopwatch.Stop();
        //                            //                await this.DisplayAlert("Successful", "Flashing Successful !!", "OK");
        //                            //                stopwatch.Reset();

        //                            //                await Task.Delay(200);
        //                            //                var AfterValue = await ReadCVN();
        //                            //                CvnAfterFlash = Convert.ToString(AfterValue.FirstOrDefault()?.responseValue);

        //                            //                var FlashRecord = new List<flash_record>();
        //                            //                FlashRecord.Add(new flash_record { flash_duration = Timer.Text, status = "pass", cvn_after_flash = CvnAfterFlash, cvn_before_flash = CvnBeforeFlash });

        //                            //                services.flash_record(FlashRecord, App.JwtToken, App.SessionId);

        //                            //                await Task.Delay(200);
        //                            //                await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion));
        //                            //            }
        //                            //        });
        //                            //    }
        //                            //});
        //                            #endregion
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Device.BeginInvokeOnMainThread(async () =>
        //                        {
        //                            App.controlEventManager.SendRequestData("ERROR_FlashFile_NotFound");
        //                            await Task.Delay(200);
        //                            var RecordingSaved = new Popup.DisplayAlertPage("ERROR", "Flash File Not Found !!", "OK");
        //                            await PopupNavigation.Instance.PushAsync(RecordingSaved);

        //                            await Navigation.PushAsync(new AppFeaturePage(jobCard, App.firmwareVersion, null, null, string.Empty));
        //                        });
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        private void FlashFileListScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = Convert.ToString(e.FirstVisibleItemIndex),
                        ElementValue = "FlashFileListScrolled",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ClosePopupClicked(object sender, EventArgs e)
        {
            try
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = $"ClosePopupFrame",
                    ElementValue = "ClosePopupClicked",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
                viewModel.flash_file_view_visible = false;
            }
            catch (Exception ex)
            {

            }
        }

        private void Tab_Clicked(object sender, EventArgs e)
        {
            try
            {
                viewModel.selected_ecu = (FlashEcusModel)((Button)sender).BindingContext;
                //App.controlEventManager.SendRequestData("SelectedEcu*#" + JsonConvert.SerializeObject(viewModel.selected_ecu));
                Device.BeginInvokeOnMainThread(async () =>
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        try
                        {
                            if (CurrentUserEvent.Instance.IsExpert)
                            {
                                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                                {
                                    ElementName = JsonConvert.SerializeObject(viewModel.selected_ecu),
                                    ElementValue = "Tab_Clicked",
                                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                                    IsExpert = CurrentUserEvent.Instance.IsExpert
                                });

                                DependencyService.Get<ILodingPageService>().HideLoadingPage();
                                DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                                await Task.Delay(200);
                            }
                            await ChangeEcuTab();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                });
            }
            catch (Exception ex)
            {

            }
        }

        public async Task ChangeEcuTab()
        {
            try
            {
                if (viewModel.selected_ecu != null)
                {
                    viewModel.selected_ecu.opacity = 1;

                    foreach (var ecu in viewModel.ecus_list)
                    {
                        if (viewModel.selected_ecu.ecu_name != ecu.ecu_name)
                        {
                            ecu.opacity = .5;
                        }
                    }
                    viewModel.ecus_list.FirstOrDefault(x => x.ecu_name == viewModel.selected_ecu.ecu_name).opacity = 1;
                }

                viewModel.flash_file_list = new ObservableCollection<File>(viewModel.selected_ecu.flash_file_list);

                if (!CurrentUserEvent.Instance.IsExpert)
                {
                    if (App.ConnectedVia == "USB")
                    {
                        DependencyService.Get<Interfaces.IConnectionUSB>().SetDongleProperties(viewModel.selected_ecu.ecu2.protocol.autopeepal,
                                                viewModel.selected_ecu.ecu2.tx_header,
                                                viewModel.selected_ecu.ecu2.rx_header);
                    }
                    else if (App.ConnectedVia == "BT")
                    {
                        DependencyService.Get<Interfaces.IBth>().SetDongleProperties(viewModel.selected_ecu.ecu2.protocol.autopeepal,
                                                viewModel.selected_ecu.ecu2.tx_header,
                                                viewModel.selected_ecu.ecu2.rx_header);
                    }
                    else if (App.ConnectedVia == "WIFI")
                    {
                        DependencyService.Get<Interfaces.IConnectionWifi>().SetDongleProperties(viewModel.selected_ecu.ecu2.protocol.autopeepal,
                                                viewModel.selected_ecu.ecu2.tx_header,
                                                viewModel.selected_ecu.ecu2.rx_header);
                    }
                }
                DependencyService.Get<ILodingPageService>().HideLoadingPage();
            }
            catch (Exception ex)
            {

            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}