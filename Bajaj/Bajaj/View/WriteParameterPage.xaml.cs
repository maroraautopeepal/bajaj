using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Popup;
using Bajaj.Services;
using Bajaj.View.PopupPages;
using MultiEventController;
using MultiEventController.Models;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Bajaj.ViewModel;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WriteParameterPage : DisplayAlertPage
    {
        ApiServices apiServices;
        ObservableCollection<ReadPidPresponseModel> read_pid_android;
        ObservableCollection<WriteParameterPID> pidList;
        ObservableCollection<WriteParameter_Status> Parameter_Status;
        List<pid_write_record> PidWriteRecord;
        ObservableCollection<PidCode> pid_list;
        public string SelectedPidValue = string.Empty;
        double deci;

        WriteParameterViewModel viewModel;
        public WriteParameterPage()
        {

            try
            {
                InitializeComponent();

                //GetPidList();
            }
            catch (Exception)
            {

            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
            BindingContext = viewModel = new WriteParameterViewModel();
            //SelectedParameterList = new ObservableCollection<ReadParameterPID>();
            read_pid_android = new ObservableCollection<ReadPidPresponseModel>();
            apiServices = new ApiServices();
            await GetPidList();
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
        }

        string[] PairedData = new string[2];
        private void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {
            #region Check Internet Connection
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                //await Task.Delay(100);
                Device.BeginInvokeOnMainThread(async () =>
                {
                    string data = (string)sender; //sender as string;
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (data.Contains("EcuList*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.ecus_list = JsonConvert.DeserializeObject<ObservableCollection<EcusModel>>(PairedData[1]);
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("PidList*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.static_pid_list = viewModel.pid_list = JsonConvert.DeserializeObject<ObservableCollection<PidCode>>(PairedData[1]);
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("ShowPidPopupClicked*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.static_pid_list = viewModel.pid_list = JsonConvert.DeserializeObject<ObservableCollection<PidCode>>(PairedData[1]);
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("ClosePidPopupClicked*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.static_pid_list = viewModel.pid_list = JsonConvert.DeserializeObject<ObservableCollection<PidCode>>(PairedData[1]);
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("PidValue*#"))
                        {
                            PairedData = data.Split('#');
                            read_pid_android = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(PairedData[1]); ;
                            SetPidValue();
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("InValidLength"))
                        {
                            var errorpage = new Popup.DisplayAlertPage("ERROR", "Invalid Length", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                        else if (data.Contains("AskToWritePid"))
                        {
                            var Alert = new Popup.DisplayAlertPage("Alert!! " + viewModel.new_value.ToUpper(), "Do Want to write the value of - " + txt_parameter.Text + " to " + viewModel.new_value, "Ok", "Cancel");
                            await PopupNavigation.Instance.PushAsync(Alert);
                        }
                        else if (data.Contains("PidToWrite*#"))
                        {
                            PairedData = data.Split('#');
                            pidList = new ObservableCollection<WriteParameterPID>();
                            pidList = JsonConvert.DeserializeObject<ObservableCollection<WriteParameterPID>>(PairedData[1]);
                        }
                        else if (data.Contains("DllResult*#"))
                        {
                            PairedData = data.Split('#');
                            Parameter_Status = new ObservableCollection<WriteParameter_Status>();
                            Parameter_Status = JsonConvert.DeserializeObject<ObservableCollection<WriteParameter_Status>>(PairedData[1]);
                        }
                        else if (data.Contains("ServerMessage"))
                        {
                            PairedData = data.Split('#');
                            ServerMessage = $"{PairedData[1]}";
                        }
                        else if (data.Contains("WritingCompletedMessage"))
                        {
                            var WritingCompleted = new Popup.DisplayAlertPage("Writing Completed", $"\n\n{ServerMessage}", "OK");
                            await PopupNavigation.Instance.PushAsync(WritingCompleted);
                        }
                        else if (data.Contains("WritingFailledMessage"))
                        {
                            var WritingIncomplete = new Popup.DisplayAlertPage("Writing Incomplete", $"{Parameter_Status.FirstOrDefault()?.Status}\n\n{ServerMessage}", "OK");
                            await PopupNavigation.Instance.PushAsync(WritingIncomplete);
                        }
                        else if (data.Contains("ServerWriteRecord*#"))
                        {
                            PairedData = data.Split('#');
                            PidWriteRecord = new List<pid_write_record>();
                            PidWriteRecord = JsonConvert.DeserializeObject<List<pid_write_record>>(PairedData[1]);
                        }
                        else if (data.Contains("ServerWriteRecord*#"))
                        {
                            PairedData = data.Split('#');
                            PidWriteRecord = new List<pid_write_record>();
                            PidWriteRecord = JsonConvert.DeserializeObject<List<pid_write_record>>(PairedData[1]);
                        }
                        else if (data.Contains("SeverMessage#"))
                        {
                            PairedData = data.Split('#');
                            var WritingIncomplete = new Popup.DisplayAlertPage(PairedData[1], $"{PairedData[2]}\n{PairedData[3]}", "OK");
                            await PopupNavigation.Instance.PushAsync(WritingIncomplete);
                        }
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
            if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("RefreshPidClicked"))
            {
                await GetPidsValue();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("ShowPidPopupClicked"))
            {
                viewModel.pid_view_visible = true;
                viewModel.title = "Select a parameter to write";
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("ClosePidPopupClicked"))
            {
                viewModel.pid_view_visible = false;
                viewModel.title = "Select a parameter to write";
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("PidListScrolled"))
            {
                collectionView.ScrollTo(Convert.ToInt32(elementEventHandler.ElementName), 0, ScrollToPosition.Start);
            }

            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SearchParameter"))
            {
                viewModel.search_key = elementEventHandler.ElementName;
                SearchParameter();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectPidClicked"))
            {
                viewModel.selected_pid = viewModel.pid_list.FirstOrDefault(x => x.id == Convert.ToInt32(elementEventHandler.ElementName));
                SelectPidClicked();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("PidNewValve_TextChanged"))
            {
                viewModel.new_value = elementEventHandler.ElementName;
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("btnWriteClicked"))
            {
                btnWrite_Clicked();
            }

            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }

        int count1 = 0;
        public async Task GetPidList()
        {
            if (!CurrentUserEvent.Instance.IsExpert)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        try
                        {

                            //int pid_count = 0;
                            count1 = 0;
                            viewModel.ecus_list = new ObservableCollection<EcusModel>();
                            foreach (var ecu in StaticData.ecu_info)
                            {
                                count1++;
                                int pid_dataset = ecu.pid_dataset_id;
                                var pid_li = await apiServices.get_pid(App.JwtToken, pid_dataset);

                                List<PidCode> pidCode = new List<PidCode>();
                                foreach (var item in pid_li.FirstOrDefault().codes)
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
                                            messages = item1.messages
                                        });
                                    }
                                }

                                viewModel.ecus_list.Add(
                                new EcusModel
                                {
                                    ecu_name = ecu.ecu_name,
                                    opacity = count1 == 1 ? 1 : .5,
                                    
                                    pid_list = pidCode.Where(y => y.write).ToList(),
                                    protocol = ecu.protocol.autopeepal,
                                    txHeader = ecu.tx_header,
                                    rxHeader = ecu.rx_header
                                });
                            }
                            viewModel.static_pid_list = viewModel.pid_list = new ObservableCollection<PidCode>(viewModel.ecus_list.FirstOrDefault().pid_list);
                            viewModel.selected_ecu = viewModel.ecus_list.FirstOrDefault();

                            if (App.ConnectedVia == "USB")
                            {
                                DependencyService.Get<Interfaces.IConnectionUSB>().SetDongleProperties(viewModel.selected_ecu.protocol, viewModel.selected_ecu.txHeader, viewModel.selected_ecu.rxHeader);
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                await DependencyService.Get<Interfaces.IBth>().SetDongleProperties(viewModel.selected_ecu.protocol, viewModel.selected_ecu.txHeader, viewModel.selected_ecu.rxHeader);
                            }

                            //App.controlEventManager.SendRequestData("GetEcuPidList*#");

                            //var ecu1 = JsonConvert.SerializeObject(viewModel.ecus_list);
                            //var pid = JsonConvert.SerializeObject(viewModel.pid_list);
                            App.controlEventManager.SendRequestData("EcuList*#" + JsonConvert.SerializeObject(viewModel.ecus_list));
                            App.controlEventManager.SendRequestData("PidList*#" + JsonConvert.SerializeObject(viewModel.pid_list));
                        }
                        catch (Exception ex)
                        {
                            //msg = "Something went wrong...";
                            //loader_visible = false;
                        }
                    }
                });
            }
            else
            {
                DependencyService.Get<ILodingPageService>().HideLoadingPage();
                DependencyService.Get<ILodingPageService>().InitLoadingPage(new LoadingIndicatorPage());
                DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                await Task.Delay(200);
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


        private async void RefreshPidClicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"RefreshPidFrame",
                        ElementValue = "RefreshPidClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                await GetPidsValue();
            }
            catch (Exception)
            {

            }
        }

        private void SearchPid_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"{viewModel.search_key}",
                        ElementValue = "SearchParameter",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                SearchParameter();
            }
            catch (Exception ex)
            {
            }
        }

        private void SearchParameter()
        {
            try
            {
                if (!string.IsNullOrEmpty(viewModel.search_key))
                {
                    viewModel.pid_list = new ObservableCollection<PidCode>(viewModel.static_pid_list.Where(x => x.short_name.ToLower().Contains(viewModel.search_key.ToLower())).ToList());
                }
                else
                {
                    viewModel.pid_list = new ObservableCollection<PidCode>(viewModel.static_pid_list.ToList());
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void PidNewValve_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"{viewModel.new_value}",
                        ElementValue = "PidNewValve_TextChanged",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                //SearchParameter();
            }
            catch (Exception ex)
            {
            }
        }

        private void ShowPidPopup(object sender, EventArgs e)
        {
            if (CurrentUserEvent.Instance.IsExpert)
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = $"ShowPidPopupFrame",
                    ElementValue = "ShowPidPopupClicked",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }
            viewModel.pid_view_visible = true;
            viewModel.title = "Select a parameter to write";
        }

        private void ClosePopupClicked(object sender, EventArgs e)
        {
            if (CurrentUserEvent.Instance.IsExpert)
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = $"ClosePidPopupFrame",
                    ElementValue = "ClosePidPopupClicked",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }
            viewModel.pid_view_visible = false;
            viewModel.title = "Select a parameter to write";
        }

        private void SelectPidClicked(object sender, EventArgs e)
        {
            viewModel.selected_pid = (PidCode)((Grid)sender).BindingContext;
            App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
            {
                ElementName = $"{viewModel.selected_pid.id}",
                ElementValue = $"SelectPidClicked",
                ToUserId = CurrentUserEvent.Instance.ToUserId,
                IsExpert = CurrentUserEvent.Instance.IsExpert
            });
            SelectPidClicked();
        }

        private void SelectPidClicked()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(50);
                    try
                    {
                        if (viewModel.selected_pid != null)
                        {
                            viewModel.iqa_view_visible = viewModel.default_view_visible = false;

                            await GetPidsValue();
                            viewModel.title = viewModel.selected_pid.short_name;
                            //viewModel.pid_view_visible = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        viewModel.new_value = string.Empty;
                        viewModel.pid_view_visible = false;
                        viewModel.title = viewModel.selected_pid.short_name;
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
                    pid_list = new ObservableCollection<PidCode>();
                    pid_list.Add(viewModel.selected_pid);
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            //top = 20;
                            break;
                        case Device.Android:
                            if (App.ConnectedVia == "USB")
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadPid(pid_list);
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IBth>().ReadPid(pid_list);
                            }
                            else if (App.ConnectedVia == "WIFI")
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionWifi>().ReadPid(pid_list);
                            }
                            else
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionRP>().ReadPid(pid_list);
                            }
                            break;
                        default:
                            //top = 0;
                            break;
                    }
                    viewModel.new_value = string.Empty;
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
                if (viewModel.selected_pid.message_type.Contains("IQA"))
                {
                    viewModel.iqa_view_visible = true;
                }
                else
                {
                    viewModel.default_view_visible = true;
                }

                viewModel.pid_view_visible = false;
                viewModel.title = viewModel.selected_pid.short_name;
            }
        }
        public void SetPidValue()
        {
            if (read_pid_android != null)
            {
                if (viewModel.selected_pid.message_type.Contains("IQA"))
                {
                    viewModel.iqa_view_visible = true;
                    viewModel.default_view_visible = false;
                }
                else
                {
                    viewModel.iqa_view_visible = false;
                    viewModel.default_view_visible = true;
                }

                foreach (var pid in read_pid_android)
                {

                    if (pid.Status == "NOERROR")
                    {

                        if (viewModel.selected_pid.message_type.Contains("IQA"))
                        {
                            viewModel.cylinder_one = $"{pid.responseValue.Split(',')[0]}";
                            viewModel.cylinder_two = $"{pid.responseValue.Split(',')[1]}";
                            viewModel.cylinder_three = $"{pid.responseValue.Split(',')[2]}";
                            viewModel.cylinder_four = $"{pid.responseValue.Split(',')[3]}";
                        }
                        else
                        {
                            if (pid.responseValue.Contains("."))
                            {
                                double.TryParse(pid.responseValue, out deci);
                                viewModel.selected_pid.show_resolution = deci.ToString("0.00");
                            }
                            else
                            {
                                viewModel.selected_pid.show_resolution = pid.responseValue;
                            }
                        }
                    }
                    else
                    {
                        if (viewModel.selected_pid.message_type.Contains("IQA"))
                        {
                            viewModel.cylinder_one = $"ERR";
                            viewModel.cylinder_two = $"ERR";
                            viewModel.cylinder_three = $"ERR";
                            viewModel.cylinder_four = $"ERR";
                        }
                        else
                        {
                            viewModel.selected_pid.show_resolution = "ERR";
                            viewModel.selected_pid.unit = "";
                        }
                    }
                }
            }
            viewModel.pid_view_visible = false;
            viewModel.title = viewModel.selected_pid.short_name;
        }

        private async void btnWrite_Clicked(object sender, EventArgs e)
        {
            try
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = $"btnWriteFrame",
                    ElementValue = $"btnWriteClicked",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
                btnWrite_Clicked();
            }
            catch (Exception ex)
            {
            }
        }

        private async void btnWrite_Clicked()
        {
            try
            {
                SelectedPidValue = viewModel.selected_pid.show_resolution;

                if (!CurrentUserEvent.Instance.IsExpert)
                {
                    byte[] writeinput = null;

                    if (viewModel.selected_pid.message_type.Contains("IQA"))
                    {
                        //writeinput = Encoding.ASCII.GetBytes(viewModel.new_cylinder_one.ToUpper()
                        //    +viewModel.new_cylinder_two
                        //    + viewModel.new_cylinder_three
                        //    + viewModel.new_cylinder_four);
                        writeinput = new byte[28];
                        byte[] rxdata = new byte[7];
                        byte[] final_rxdata = new byte[8];

                        rxdata = Encoding.ASCII.GetBytes(viewModel.cylinder_one.ToUpper());
                        Array.Copy(rxdata, 0, writeinput, 0, 7);

                        rxdata = Encoding.ASCII.GetBytes(viewModel.cylinder_two.ToUpper());
                        Array.Copy(rxdata, 0, writeinput, 7, 7);

                        rxdata = Encoding.ASCII.GetBytes(viewModel.cylinder_three.ToUpper());
                        Array.Copy(rxdata, 0, writeinput, 14, 7);

                        rxdata = Encoding.ASCII.GetBytes(viewModel.cylinder_four.ToUpper());
                        Array.Copy(rxdata, 0, writeinput, 21, 7);

                        viewModel.new_value = "IQA";



                    }
                    else
                    {
                        writeinput = Encoding.ASCII.GetBytes(viewModel.new_value.ToUpper());
                    }



                    var length = writeinput.Length;
                    double? Continues_Value = 0;

                    if (viewModel.selected_pid.message_type == "CONTINUOUS")
                    {
                        if (Convert.ToDouble(viewModel.new_value) >= viewModel.selected_pid.min && Convert.ToDouble(viewModel.new_value) <= viewModel.selected_pid.max)
                        {
                            Continues_Value = Convert.ToDouble(viewModel.new_value) - viewModel.selected_pid.offset;

                            Continues_Value = Continues_Value / viewModel.selected_pid.resolution;

                            byte[] val = new byte[viewModel.selected_pid.length];

                            for (int i = 0; i < viewModel.selected_pid.length; i++)
                            {
                                val[viewModel.selected_pid.length - i - 1] = (byte)(Convert.ToUInt32(Continues_Value) >> (i * 8));
                            }
                            WriteParameter(val);
                        }
                        else
                        {
                            App.controlEventManager.SendRequestData("InValidLength");
                            var errorpage = new Popup.DisplayAlertPage("ERROR", "Invalid Length", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                            return;
                        }
                    }
                    else if (viewModel.selected_pid.message_type == "IQA")
                    {
                        WriteParameter(writeinput);
                        return;
                    }
                    else if (writeinput.Length != viewModel.selected_pid.length)
                    {
                        App.controlEventManager.SendRequestData("InValidLength");
                        var errorpage = new Popup.DisplayAlertPage("ERROR", "Invalid Length", "OK");
                        await PopupNavigation.Instance.PushAsync(errorpage);
                        return;
                    }
                    else
                    {
                        WriteParameter(writeinput);
                    }
                }

            }
            catch (Exception ex)
            {
            }
        }

        string ServerMessage = string.Empty;
        string MessageTitle = string.Empty;
        string Message = string.Empty;
        string Status = string.Empty;
        public async void WriteParameter(byte[] writeinput)
        {
            try
            {
                pidList = new ObservableCollection<WriteParameterPID>();
                ServerMessage = string.Empty;
                Message = string.Empty;
                MessageTitle = string.Empty;
                App.controlEventManager.SendRequestData("AskToWritePid");
                var Alert = new Popup.DisplayAlertPage("Alert!! " + viewModel.new_value.ToUpper(), "Do Want to write the value of - " + txt_parameter.Text + " to " + viewModel.new_value, "Ok", "Cancel");
                await PopupNavigation.Instance.PushAsync(Alert);
                bool alertResult = await Alert.PopupClosedTask;
                if (alertResult)
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(200);

                        Parameter_Status = new ObservableCollection<WriteParameter_Status>();

                        var model_detail = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == viewModel.selected_ecu.ecu_name);
                        pidList.Add(
                                               new WriteParameterPID
                                               {
                                                   seedkeyindex = model_detail.seed_key_index.value,
                                                   writepamindex = model_detail.write_pid_index,
                                                   writeparadata = writeinput,
                                                   writeparadatasize = viewModel.selected_pid.total_len,
                                                   writeparapid = viewModel.selected_pid.write_pid,
                                                   ReadParameterPID_DataType = viewModel.selected_pid.message_type,
                                                   pid = viewModel.selected_pid.code,
                                                   startByte = viewModel.selected_pid.byte_position,
                                                   totalBytes = viewModel.selected_pid.total_len
                                               }
                                              );
                        App.controlEventManager.SendRequestData("PidToWrite*#" + JsonConvert.SerializeObject(pidList));

                        if (App.ConnectedVia == "USB")
                        {
                            Parameter_Status = await DependencyService.Get<Interfaces.IConnectionUSB>().WritePid(model_detail.write_pid_index, pidList);
                        }
                        else if (App.ConnectedVia == "BT")
                        {
                            Parameter_Status = await DependencyService.Get<Interfaces.IBth>().WritePid(model_detail.write_pid_index, pidList);
                        }
                        else if (App.ConnectedVia == "WIFI")
                        {
                            Parameter_Status = await DependencyService.Get<Interfaces.IConnectionWifi>().WritePid(model_detail.write_pid_index, pidList);
                        }
                        else
                        {
                            Parameter_Status = await DependencyService.Get<Interfaces.IConnectionRP>().WritePid(model_detail.write_pid_index, pidList);
                        }

                        App.controlEventManager.SendRequestData("DllResult*#" + JsonConvert.SerializeObject(Parameter_Status));





                        PidWriteRecord = new List<pid_write_record>();

                        foreach (var item in Parameter_Status)
                        {
                            if (item.Status == "NOERROR")
                            {
                                await GetPidsValue();
                                viewModel.new_value = string.Empty;
                                //ServerMessage = string.Empty;

                                MessageTitle = "Writing Completed";
                                Message = $"\n\n{ServerMessage}";
                                Status = string.Empty;
                                //App.controlEventManager.SendRequestData("WritingStatus");
                                //var WritingCompleted = new Popup.DisplayAlertPage("Writing Completed", $"\n\n{ServerMessage}", "OK");

                                //await PopupNavigation.Instance.PushAsync(WritingCompleted);

                                PidWriteRecord.Add(new pid_write_record { pid_code = viewModel.selected_pid.code, value_before = SelectedPidValue, value_after = viewModel.selected_pid.show_resolution, status = "pass" });
                            }
                            else
                            {
                                MessageTitle = "Writing Incomplete";
                                Message = $"{Parameter_Status.FirstOrDefault()?.Status}\n\n{ServerMessage}";
                                Status = item.Status;
                                PidWriteRecord.Add(new pid_write_record { pid_code = viewModel.selected_pid.code, value_before = SelectedPidValue, value_after = viewModel.selected_pid.show_resolution, status = "fail" });

                                //App.controlEventManager.SendRequestData("WritingFailledMessage");
                                //var WritingIncomplete = new Popup.DisplayAlertPage("Writing Incomplete", $"{Parameter_Status.FirstOrDefault()?.Status}\n\n{ServerMessage}", "OK");
                                //await PopupNavigation.Instance.PushAsync(WritingIncomplete);
                            }

                            if (PidWriteRecord.Count > 0)
                            {
                                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                                if (isReachable)
                                {

                                    var server_response = await apiServices.Pid_Write_Record(PidWriteRecord, App.JwtToken, App.SessionId);
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
                                    show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                                }

                                //SelectedPidValue = "";
                                //NewPidValue = "";
                            }

                            if (Parameter_Status.FirstOrDefault().Status.Contains("NOERROR"))
                            {
                                Status = string.Empty;
                            }

                            App.controlEventManager.SendRequestData($"SeverMessage#{MessageTitle}#{Status}#{ServerMessage}");
                            var WritingIncomplete = new Popup.DisplayAlertPage(MessageTitle, $"{Status}\n{ServerMessage}", "OK");
                            await PopupNavigation.Instance.PushAsync(WritingIncomplete);

                            //App.controlEventManager.SendRequestData("ServerWriteRecord*#" + JsonConvert.SerializeObject(PidWriteRecord));
                        }
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
    }
}