using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.View.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Bajaj.Popup;
using MultiEventController.Models;
using Acr.UserDialogs;

namespace Bajaj.View.IorTest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PreConditionPage : ContentPage
    {
        PreConditionViewModel viewModel;
        double deci;
        bool IsReadingPid;
        IorResult iorResult;
        string SeedIndex;
        string WriteFnIndex;
        ObservableCollection<ReadPidPresponseModel> read_pid_android;
        int ecu_id = 0;
        //ObservableCollection<IorTestRoutine> RoutineList;
        //ObservableCollection<TestMonitor> MonitorList;
        public PreConditionPage(IorResult iorResult, string SeedIndex, string WriteFnIndex, int ecu_id)
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new PreConditionViewModel(iorResult.pre_conditions);
                this.ecu_id = ecu_id;
                this.iorResult = iorResult;
                this.SeedIndex = SeedIndex;
                this.WriteFnIndex = WriteFnIndex;
                IsReadingPid = true;
                //this.RoutineList = RoutineList;
                //this.MonitorList = MonitorList;
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
            IsReadingPid = true;

            //App.controlEventManager.SendRequestData("PreconditionList*#" + JsonConvert.SerializeObject(viewModel.PreConditionList));


            GetPidValues();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
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
            IsReadingPid = false;
        }




        string[] PairedData = new string[2];
        private async void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {
            //var status = App.controlEventManager.hubConnection;
            //DependencyService.Get<Interfaces.Toasts>().Show("Enter 1");
            #region Check Internet Connection
            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                //DependencyService.Get<Interfaces.Toasts>().Show("Enter 2");
                await Task.Delay(100);
                bool InsternetActive = true;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    //DependencyService.Get<Interfaces.Toasts>().Show("Enter 3");
                    string data = (string)sender; //sender as string;
                    //DependencyService.Get<Interfaces.Toasts>().Show(data);
                    //await DisplayAlert("DATA", data, "Ok");
                    if (data.Contains("PreconditionList*#"))
                    {
                        PairedData = data.Split('#');
                        viewModel.PreConditionList = JsonConvert.DeserializeObject<ObservableCollection<PreCondition>>(PairedData[1]); ;
                        DependencyService.Get<ILodingPageService>().HideLoadingPage();
                    }
                    else if (data.Contains("PidValue*#"))
                    {

                        //DependencyService.Get<Interfaces.Toasts>().Show("Enter 4");
                        PairedData = data.Split('#');
                        read_pid_android = JsonConvert.DeserializeObject<ObservableCollection<ReadPidPresponseModel>>(PairedData[1]); ;
                        //DependencyService.Get<Interfaces.Toasts>().Show("Enter 5");
                        SetPidValue();
                        //DependencyService.Get<Interfaces.Toasts>().Show("Enter 6");
                        DependencyService.Get<ILodingPageService>().HideLoadingPage();
                    }
                    else if (data.Contains("BtnEnable*#"))
                    {

                        viewModel.BtnContinueEnable = true;
                    }
                    else if (data.Contains("BtnDisable*#"))
                    {
                        viewModel.BtnContinueEnable = false;
                    }
                    InsternetActive = false;
                });
            }
            //DependencyService.Get<Interfaces.Toasts>().Show("END");
            #endregion
        }

        public string ReceiveValue = string.Empty;
        private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
        {
            var elementEventHandler = (sender as ElementEventHandler);
            this.ReceiveValue = elementEventHandler.ElementValue;
            //if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectParameterClicked"))
            //{
            //    viewModel.PidListVisible = true;
            //    viewModel.NewValue = string.Empty;
            //    viewModel.ShortNameVisible = viewModel.EnumrateDropDownVisible = viewModel.ManualEntryVisible = viewModel.BtnVisible = false;
            //}
            //else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectEcuClicked"))
            //{
            //    //App.controlEventManager.SendRequestData("SelectIorTest*#" + elementEventHandler.ElementName);
            //    viewModel.SelectedEcu = JsonConvert.DeserializeObject<EcuTestRoutine>(elementEventHandler.ElementName);

            //    SelectEcuClicked();
            //}
            if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("CheckBox_CheckedChanged"))
            {
                try
                {
                    PairedData = ReceiveValue.Split('*');
                    viewModel.SelectPreCondition = viewModel.PreConditionList.FirstOrDefault(x => x.id == elementEventHandler.ElementName);

                    if (PairedData[1].Contains("rue"))
                    {
                        viewModel.SelectPreCondition.is_check = true;
                    }
                    else
                    {
                        viewModel.SelectPreCondition.is_check = false;
                    }

                    //var static_pid = viewModel.static_pid_list.FirstOrDefault(x => x.id == viewModel.check_changed_pid.id);
                    //static_pid.Selected = viewModel.check_changed_pid.Selected;
                }
                catch (Exception ex)
                {
                }
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("ContinueClicked"))
            {
                try
                {
                    ContinueClicked();
                }
                catch (Exception ex)
                {
                }
            }

            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }


        bool AllConditionMatched = false;
        List<PidCode> codes = new List<PidCode>();
        PidCode pidCode = null;
        public async void GetPidValues()
        {
            if (!CurrentUserEvent.Instance.IsExpert)
            {
                await Task.Run(async() =>
                {
                    //Device.BeginInvokeOnMainThread(async () =>
                    //{
                        await Task.Delay(100);
                        using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                        {
                            try
                            {
                                var PidData = DependencyService.Get<ISaveLocalData>().GetData("pidjson");
                                var PidInfo = JsonConvert.DeserializeObject<List<Results>>(PidData);
                                viewModel.PidPrecondition = new ObservableCollection<PidCode>();

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
                            var PreConditionList = viewModel.PreConditionList.Where(x => x.pre_condition_type == "pid").ToList();

                                foreach (var item2 in PreConditionList.ToList())
                                {
                                    pidCode = null;
                                    pidCode = codes.FirstOrDefault(x => x.id == item2.pid);
                                    if (pidCode != null)
                                    {
                                        viewModel.BtnContinueEnable = false;
                                        viewModel.PidPrecondition.Add(pidCode);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    //});
                });
                if (viewModel.PidPrecondition != null && viewModel.PidPrecondition.Any())
                {
                    while (IsReadingPid)
                    {
                        await GetPidsValue();
                        //IsReadingPid = false;

                        if (AllConditionMatched)
                        {
                            viewModel.BtnContinueEnable = true;
                            App.controlEventManager.SendRequestData("BtnEnable*#");
                        }
                        else
                        {
                            viewModel.BtnContinueEnable = false;
                            App.controlEventManager.SendRequestData("BtnDisable*#");
                        }
                    }
                }
            }
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
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadPid(viewModel.PidPrecondition);
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IBth>().ReadPid(viewModel.PidPrecondition);
                            }
                            else
                            {
                                read_pid_android = await DependencyService.Get<Interfaces.IConnectionWifi>().ReadPid(viewModel.PidPrecondition);
                            }
                            break;
                        default:
                            //top = 0;
                            break;
                    }
                    SetPidValue();
                    if (!CurrentUserEvent.Instance.IsExpert)
                    {
                        //var status = App.controlEventManager.hubConnection;
                        //DependencyService.Get<Interfaces.Toasts>().Show(status.State.ToString());
                        App.controlEventManager.SendRequestData("PidValue*#" + JsonConvert.SerializeObject(read_pid_android));
                    }
                }
                else
                {
                    //DependencyService.Get<ILodingPageService>().HideLoadingPage();
                    //DependencyService.Get<ILodingPageService>().InitLoadingPage(new LoadingIndicatorPage());
                    //DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                    //await Task.Delay(200);
                }
            }
            catch (Exception ex)
            {
            }
        }
        List<PreCondition> manual_precondition;
        public void SetPidValue()
        {
            if (read_pid_android != null)
            {
                AllConditionMatched = true;
                manual_precondition = new List<PreCondition>(viewModel.PreConditionList.Where(y => y.pre_condition_type == "manual_confirm").ToList());
                {

                    foreach (var read_pid in read_pid_android)
                    {
                        var pidlist = viewModel.PreConditionList.FirstOrDefault(x => x.pid == read_pid.pidNumber);
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

                                if (Convert.ToDouble(pidlist.current_value) >= Convert.ToDouble(pidlist.lower_limit)
                                    && Convert.ToDouble(pidlist.current_value) <= Convert.ToDouble(pidlist.upper_limit))
                                {
                                    //AllConditionMatched = true;
                                    AllConditionMatched = AllConditionMatched ? true : false;
                                    //Console.WriteLine($"{pidlist.description} : True");
                                    if (manual_precondition.FirstOrDefault(x => !x.is_check) != null)
                                    {
                                        AllConditionMatched = false;
                                        //Console.WriteLine($"{pidlist.description} : Manual List True");
                                    }
                                }
                                else
                                {
                                    AllConditionMatched = false;
                                    //Console.WriteLine($"{pidlist.description} : False");
                                }
                            }
                            else
                            {
                                pidlist.current_value = "ERR";
                                AllConditionMatched = false;
                                //pidlist.unit = "";
                            }
                        }
                    }
                }
            }
        }

        private void CheckBox_CheckedChanged1(object sender, CheckedChangedEventArgs e)
        {
            try
            {
                var item = viewModel.PreConditionList.FirstOrDefault(x => x.pre_condition_type == "manual_confirm" && x.is_check == false);
                if (item != null)
                {
                    viewModel.BtnContinueEnable = false;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                viewModel.SelectPreCondition = (PreCondition)((ImageButton)sender).BindingContext;
                if (viewModel.SelectPreCondition == null)
                    return;

                viewModel.SelectPreCondition.is_check = !viewModel.SelectPreCondition.is_check;
                //var sele = viewModel.pid_list.Where(x => x.Selected).ToList();
                var status = App.controlEventManager.hubConnection;
                DependencyService.Get<Interfaces.Toasts>().Show(status.State.ToString());
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = $"{viewModel.SelectPreCondition.id}",
                    ElementValue = $"CheckBox_CheckedChanged*{viewModel.SelectPreCondition.is_check}",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });

            }
            catch (Exception ex)
            {
            }
        }

        private async void ContinueClicked(object sender, EventArgs e)
        {
            try
            {


                if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
                {
                    #region Check Internet Connection
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                        {
                            ElementName = "ContinueFrame",
                            ElementValue = "ContinueClicked",
                            ToUserId = CurrentUserEvent.Instance.ToUserId,
                            IsExpert = CurrentUserEvent.Instance.IsExpert
                        });
                    }
                    #endregion
                }
                ContinueClicked();
            }
            catch (Exception ex)
            {
            }
        }

        private void ContinueClicked()
        {
            try
            {
                IsReadingPid = false;
                Navigation.PushAsync(new IorTestPlayPage(iorResult, SeedIndex, WriteFnIndex, ecu_id));
                this.Navigation.RemovePage(this);
            }
            catch (Exception ex)
            {
            }
        }
    }
}