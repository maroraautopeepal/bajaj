using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Popup;
using Bajaj.ViewModel;
using MultiEventController;
using MultiEventController.Models;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Bajaj.Services;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LiveParameterSelectPage : DisplayAlertPage
    {
        LiveParameterSelectViewModel viewModel;
        ApiServices apiServices;
        bool IsPageRemove = false;

        public LiveParameterSelectPage()
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new LiveParameterSelectViewModel();
                apiServices = new ApiServices();
                GetPidList();

            }
            catch (Exception ex)
            {
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            IsPageRemove = true;
            
            
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsPageRemove)
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
                //await Task.Delay(100);
                bool InsternetActive = true;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    string data = (string)sender; //sender as string;
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (data.Contains("GetEcuPidList*#"))
                        {
                            try
                            {
                                //int pid_count = 0;
                                count1 = 0;
                                foreach (var ecu in StaticData.ecu_info)
                                {
                                    count1++;
                                    int pid_dataset = ecu.pid_dataset_id;
                                    var pid_li = await apiServices.get_pid(App.JwtToken, pid_dataset);
                                    viewModel.ecus_list.Add(
                                    new EcusModel
                                    {
                                        ecu_name = ecu.ecu_name,
                                        opacity = count1 == 1 ? 1 : .5,
                                        //////
                                        //pid_list = pid_li.FirstOrDefault(x => x.id == pid_dataset).codes.OrderBy(x => x.short_name).ToList(),
                                    });
                                }
                                viewModel.static_pid_list = viewModel.pid_list = new ObservableCollection<PidCode>(viewModel.ecus_list.FirstOrDefault().pid_list);
                                //App.controlEventManager.SendRequestData("GetEcuPidList*#");
                                //var ecu1 = JsonConvert.SerializeObject(viewModel.ecus_list);
                                //var pid = JsonConvert.SerializeObject(viewModel.pid_list);
                                //App.controlEventManager.SendRequestData("EcuList*#" + ecu1);
                                //App.controlEventManager.SendRequestData("PidList*#" + pid);
                            }
                            catch (Exception ex)
                            {
                                //msg = "Something went wrong...";
                                //loader_visible = false;
                            }
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("PidList*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.static_pid_list = viewModel.pid_list = JsonConvert.DeserializeObject<ObservableCollection<PidCode>>(PairedData[1]); ;
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("SelectAnyParameter"))
                        {
                            var errorpage = new Popup.DisplayAlertPage("Alert", "Please select any parameter", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
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
            if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SelectEcuClicked"))
            {
                SelectPidClicked();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("PidListScrolled"))
            {
                collectionView.ScrollTo(Convert.ToInt32(elementEventHandler.ElementName), 0, ScrollToPosition.Start);
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("CheckBox_CheckedChanged"))
            {
                try
                {
                    PairedData = ReceiveValue.Split('*');
                    viewModel.check_changed_pid = viewModel.pid_list.FirstOrDefault(x => x.id == Convert.ToInt32(elementEventHandler.ElementName));

                    if (PairedData[1].Contains("rue"))
                    {
                        viewModel.check_changed_pid.Selected = true;
                    }
                    else
                    {
                        viewModel.check_changed_pid.Selected = false;
                    }

                    var static_pid = viewModel.static_pid_list.FirstOrDefault(x => x.id == viewModel.check_changed_pid.id);
                    static_pid.Selected = viewModel.check_changed_pid.Selected;
                }
                catch (Exception ex)
                {
                }
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("MaximumPidSelectionAlert"))
            {
                var errorpage = new Popup.DisplayAlertPage("Alert", "You can select maximum 20 parameters", "OK");
                await PopupNavigation.Instance.PushAsync(errorpage);
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("SearchParameter"))
            {
                viewModel.search_key = elementEventHandler.ElementName;
                SearchParameter();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("ContinueClicked"))
            {
                ContinueClicked();
            }

            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }

        int count1 = 0;
        List<PidCode> pidCodes = new List<PidCode>();
        public async void GetPidList()
        {
            if (!CurrentUserEvent.Instance.IsExpert)
            {
                
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        UserDialogs.Instance.ShowLoading("Loading...");
                        await Task.Delay(100);
                        try
                        {
                            //int pid_count = 0;
                            count1 = 0;
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
                                            message_type = item1.message_type,
                                            unit = item1.unit,
                                            messages = item1.messages
                                        });
                                    }
                                }

                                //pid_li.FirstOrDefault(x => x.id == pid_dataset).codes.Sort(x=>x.short_name);

                                viewModel.ecus_list.Add(
                                new EcusModel
                                {
                                    ecu_name = ecu.ecu_name,
                                    opacity = count1 == 1 ? 1 : .5,
                                //pid_list = pid_li.FirstOrDefault(x => x.id == pid_dataset).codes.OrderBy(x => x.short_name).ToList(),
                                    pid_list = pidCode.OrderBy(x => x.short_name).ToList(),
                                    protocol = ecu.protocol.autopeepal,
                                    txHeader = ecu.tx_header,
                                    rxHeader = ecu.rx_header
                                });
                            }
                            viewModel.static_pid_list = viewModel.pid_list = new ObservableCollection<PidCode>(viewModel.ecus_list.FirstOrDefault().pid_list);
                            viewModel.selected_ecu = viewModel.ecus_list.FirstOrDefault();
                            App.controlEventManager.SendRequestData("GetEcuPidList*#");
                            //var ecu1 = JsonConvert.SerializeObject(viewModel.ecus_list);
                            //var pid = JsonConvert.SerializeObject(viewModel.pid_list);
                            //App.controlEventManager.SendRequestData("EcuList*#" + ecu1);
                            //App.controlEventManager.SendRequestData("PidList*#" + pid);
                        }
                        catch (Exception ex)
                        {
                            //msg = "Something went wrong...";
                            //loader_visible = false;
                            UserDialogs.Instance.HideLoading();
                        }
                        UserDialogs.Instance.HideLoading();
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

        private void ContinueClicked(object sender, EventArgs e)
        {
            IsPageRemove = false;
            if (CurrentUserEvent.Instance.IsExpert)
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = $"ContinueFrame",
                    ElementValue = "ContinueClicked",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }
            ContinueClicked();
        }

        private async void ContinueClicked()
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                if (viewModel.pid_list != null)
                {
                    if (viewModel.pid_list.Count > 0)
                    {
                        var selected_pid = viewModel.static_pid_list.Where(x => x.Selected).ToList();

                        if (selected_pid.Count > 0)
                        {
                            if (App.ConnectedVia == "USB")
                            {
                                DependencyService.Get<Interfaces.IConnectionUSB>().SetDongleProperties(viewModel.selected_ecu.protocol, viewModel.selected_ecu.txHeader, viewModel.selected_ecu.rxHeader);
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                await DependencyService.Get<Interfaces.IBth>().SetDongleProperties(viewModel.selected_ecu.protocol, viewModel.selected_ecu.txHeader, viewModel.selected_ecu.rxHeader);
                            }

                            await page.Navigation.PushAsync(new LiveParameterSelectedPage(new ObservableCollection<PidCode>(selected_pid)));
                        }
                        else
                        {
                            App.controlEventManager.SendRequestData("SelectAnyParameter");
                            var errorpage = new Popup.DisplayAlertPage("Alert", "Please select any parameter", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                    }
                }
            }
                
        }


        int count = 0;
        private async void SelectPidClicked(object sender, EventArgs e)
        {
            try
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "SelectEcuFrame",
                        ElementValue = "SelectEcuClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                SelectPidClicked();

            }
            catch (Exception ex)
            {
            }
        }

        private void SelectPidClicked()
        {
            try
            {

                count = 0;
                if (viewModel.SelectPidImage.Contains("ic_select_all_pid"))
                {
                    viewModel.SelectPidImage = "ic_unselect_all_pid.png";
                    foreach (var pid in viewModel.pid_list)
                    {
                        count++;
                        if (count < 20)
                        {
                            pid.Selected = true;
                        }
                        else
                        {
                            pid.Selected = false;
                        }

                        var static_pid = viewModel.static_pid_list.FirstOrDefault(x => x.id == pid.id);
                        static_pid.Selected = pid.Selected;
                    }
                    //var SelectedList = viewModel.pid_list.Take(20).Select(y => y.Selected = true);
                }
                else
                {
                    viewModel.SelectPidImage = "ic_select_all_pid.png";
                    foreach (var pid in viewModel.pid_list)
                    {
                        pid.Selected = false;
                    }

                    foreach (var pid1 in viewModel.static_pid_list)
                    {
                        pid1.Selected = false;
                    }
                    //var SelectedList = viewModel.pid_list.Take(20).Select(y => y.Selected = false);
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

        private async void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                viewModel.check_changed_pid = (PidCode)((ImageButton)sender).BindingContext;
                if (viewModel.check_changed_pid == null)
                    return;
                if (viewModel.pid_list.Where(x => x.Selected).ToList().Count < 20)
                {

                    viewModel.check_changed_pid.Selected = !viewModel.check_changed_pid.Selected;
                    var sele = viewModel.pid_list.Where(x => x.Selected).ToList();

                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"{viewModel.check_changed_pid.id}",
                        ElementValue = $"CheckBox_CheckedChanged*{viewModel.check_changed_pid.Selected}",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                else
                {
                    if (viewModel.check_changed_pid.Selected)
                    {
                        viewModel.check_changed_pid.Selected = !viewModel.check_changed_pid.Selected;
                        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                        {
                            ElementName = $"{viewModel.check_changed_pid.id}",
                            ElementValue = $"CheckBox_CheckedChanged*{viewModel.check_changed_pid.Selected}",
                            ToUserId = CurrentUserEvent.Instance.ToUserId,
                            IsExpert = CurrentUserEvent.Instance.IsExpert
                        });
                    }
                    else
                    {
                        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                        {
                            ElementName = $"MaximumPidSelectionAlertFrame",
                            ElementValue = $"MaximumPidSelectionAlert",
                            ToUserId = CurrentUserEvent.Instance.ToUserId,
                            IsExpert = CurrentUserEvent.Instance.IsExpert
                        });
                        var errorpage = new Popup.DisplayAlertPage("Alert", "You can select maximum 20 parameters", "OK");
                        await PopupNavigation.Instance.PushAsync(errorpage);
                    }
                }

                var static_pid = viewModel.static_pid_list.FirstOrDefault(x => x.id == viewModel.check_changed_pid.id);
                static_pid.Selected = viewModel.check_changed_pid.Selected;

            }
            catch (Exception ex)
            {
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
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

        private void SearchParameter()
        {
            if (viewModel.static_pid_list != null)
            {
                var selected = viewModel.static_pid_list.Where(x => x.Selected);
                if (!string.IsNullOrEmpty(viewModel.search_key))
                {
                    viewModel.pid_list = new ObservableCollection<PidCode>(viewModel.static_pid_list.Where(x => x.short_name.ToLower().Contains(viewModel.search_key.ToLower())).ToList());
                }
                else
                {
                    viewModel.pid_list = new ObservableCollection<PidCode>(viewModel.static_pid_list.ToList());
                }
                var selected1 = viewModel.static_pid_list.Where(x => x.Selected);
            }
        }
    }
}