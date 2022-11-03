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
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IvnPidSelectPage : DisplayAlertPage
    {
        IvnPidViewModel viewModel;
        ApiServices apiServices;
        bool IsPageRemove = false;


        public IvnPidSelectPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            IsPageRemove = true;
            App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
            BindingContext = viewModel = new IvnPidViewModel();
            apiServices = new ApiServices();
            await GetPidList();
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
                            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                            {
                                await Task.Delay(100);
                                try
                                {
                                    //int pid_count = 0;
                                    int count2 = 0;
                                    var IVNPidList = new List<IVN_LiveParameterSelectModel>();
                                    var FrameId = new List<PIDFrameId>();

                                    foreach (var ecu in StaticData.ecu_info)
                                    {
                                        if (ecu.read_dtc_index == "IVN")
                                        {
                                            count2++;
                                            int pid_dataset = ecu.pid_dataset_id;

                                            int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).pid_dataset_id;

                                            if (viewModel.IVNAllPIDDetail.Count != 0)
                                            {
                                                foreach (var item in viewModel.IVNAllPIDDetail.FirstOrDefault(x => x.id == id).frame_datasets)
                                                {
                                                    var frame_id = item.frame_id;
                                                    var IVN_pid = item.frame_ids;
                                                    var check_ecu_name = IVNPidList.FirstOrDefault(x => x.frame_name == ecu.ecu_name);
                                                    if (check_ecu_name != null)
                                                    {
                                                        foreach (var frame_ids_item in item.frame_ids)
                                                        {
                                                            FrameId.Add(new PIDFrameId
                                                            {
                                                                FramID = item.frame_id,
                                                                bit_coded = frame_ids_item.bit_coded,
                                                                @byte = frame_ids_item.@byte,
                                                                frame_of_pid_message = frame_ids_item.frame_of_pid_message,
                                                                message_type = frame_ids_item.message_type,
                                                                no_of_bits = frame_ids_item.no_of_bits,
                                                                offset = frame_ids_item.offset,
                                                                pid_description = frame_ids_item.pid_description,
                                                                resolution = frame_ids_item.resolution,
                                                                Selected = frame_ids_item.Selected,
                                                                start_bit = frame_ids_item.start_bit,
                                                                start_byte = frame_ids_item.start_byte,
                                                                unit = frame_ids_item.unit,
                                                                endian = frame_ids_item.endian,
                                                                num_type = frame_ids_item.num_type,
                                                            });
                                                            check_ecu_name.frame_ids.Add(FrameId.FirstOrDefault());
                                                            FrameId.Clear();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var Fram = new List<PIDFrameId>();
                                                        foreach (var frame_ids_item in item.frame_ids)
                                                        {
                                                            FrameId.Add(new PIDFrameId
                                                            {
                                                                FramID = item.frame_id,
                                                                bit_coded = frame_ids_item.bit_coded,
                                                                @byte = frame_ids_item.@byte,
                                                                frame_of_pid_message = frame_ids_item.frame_of_pid_message,
                                                                message_type = frame_ids_item.message_type,
                                                                no_of_bits = frame_ids_item.no_of_bits,
                                                                offset = frame_ids_item.offset,
                                                                pid_description = frame_ids_item.pid_description,
                                                                resolution = frame_ids_item.resolution,
                                                                Selected = frame_ids_item.Selected,
                                                                start_bit = frame_ids_item.start_bit,
                                                                start_byte = frame_ids_item.start_byte,
                                                                unit = frame_ids_item.unit,
                                                                endian = frame_ids_item.endian,
                                                                num_type = frame_ids_item.num_type,
                                                            });
                                                            Fram.Add(FrameId.FirstOrDefault());
                                                            FrameId.Clear();
                                                        }

                                                        IVNPidList.Add(new IVN_LiveParameterSelectModel
                                                        {
                                                            frame_name = ecu.ecu_name,
                                                            frame_id = item.frame_id,
                                                            frame_ids = Fram
                                                        });
                                                    }
                                                }

                                                bool isEcuPresent = false;
                                                foreach (var ecuItem in viewModel.ecus_list)
                                                {
                                                    if(ecuItem.ecu_name == ecu.ecu_name)
                                                    {
                                                        isEcuPresent = true;
                                                    }
                                                }

                                                if (!isEcuPresent)
                                                {
                                                    viewModel.ecus_list.Add(
                                                    new IvnEcusModel
                                                    {
                                                        ecu_name = ecu.ecu_name,
                                                        opacity = count2 == 1 ? 1 : .5,
                                                        pid_list = IVNPidList.FirstOrDefault(x => x.frame_name == ecu.ecu_name).frame_ids.OrderBy(x => x.pid_description).ToList()
                                                    });
                                                }
                                                
                                                viewModel.IVN_PidList = IVNPidList;
                                            }
                                        }
                                    }
                                    viewModel.selected_ecu = viewModel.ecus_list.FirstOrDefault();
                                    viewModel.pid_list = new ObservableCollection<PIDFrameId>(viewModel.selected_ecu.pid_list);
                                    viewModel.static_pid_list = viewModel.pid_list;
                                }
                                catch (Exception ex)
                                {

                                }
                            }

                        }
                        else if (data.Contains("IVNAllPIDDetail*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.IVNAllPIDDetail = JsonConvert.DeserializeObject<List<PIDResult>>(PairedData[1]);
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        //if (data.Contains("EcuList*#"))
                        //{
                        //    PairedData = data.Split('#');
                        //    viewModel.ecus_list = JsonConvert.DeserializeObject<ObservableCollection<IvnEcusModel>>(PairedData[1]);
                        //    DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        //}
                        //else if (data.Contains("pid_list*#"))
                        //{
                        //    PairedData = data.Split('#');
                        //    viewModel.pid_list = JsonConvert.DeserializeObject<ObservableCollection<PIDFrameId>>(PairedData[1]);
                        //    viewModel.static_pid_list = viewModel.pid_list;
                        //    DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        //}
                        else if (data.Contains("SelectedEcu*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.selected_ecu = JsonConvert.DeserializeObject<IvnEcusModel>(PairedData[1]);
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        //else if (data.Contains("IVN_PidList*#"))
                        //{
                        //    PairedData = data.Split('#');
                        //    viewModel.IVN_PidList = JsonConvert.DeserializeObject<List<IVN_LiveParameterSelectModel>>(PairedData[1]);
                        //    DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        //}
                        else if (data.Contains("SelectAnyParameter"))
                        {
                            var errorpage = new Popup.DisplayAlertPage("Alert", "Please select any parameter", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }

                    }
                });
            }
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
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("Tab_Clicked"))
            {
                viewModel.selected_ecu = JsonConvert.DeserializeObject<IvnEcusModel>(elementEventHandler.ElementName);
                await ChangeEcuTab();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("CheckBox_CheckedChanged"))
            {
                try
                {
                    PairedData = ReceiveValue.Split('*');
                    viewModel.check_changed_pid = viewModel.pid_list.FirstOrDefault(x => x.pid_description == elementEventHandler.ElementName);

                    if (PairedData[1].Contains("rue"))
                    {
                        viewModel.check_changed_pid.Selected = true;
                    }
                    else
                    {
                        viewModel.check_changed_pid.Selected = false;
                    }

                    var static_pid = viewModel.static_pid_list.FirstOrDefault(x => x.pid_description == viewModel.check_changed_pid.pid_description);
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
                            var IVNPidList = new List<IVN_LiveParameterSelectModel>();
                            var FrameId = new List<PIDFrameId>();

                            foreach (var ecu in StaticData.ecu_info)
                            {
                                if (ecu.read_dtc_index == "IVN")
                                {
                                    count1++;
                                    int pid_dataset = ecu.pid_dataset_id;

                                    viewModel.AllPIDDetail = get_All_pid_from_local().Result;

                                    viewModel.AllPid = viewModel.AllPIDDetail.FirstOrDefault().NormalPID;
                                    viewModel.IVNAllPIDDetail = viewModel.AllPIDDetail.FirstOrDefault().IVNPID;

                                    App.controlEventManager.SendRequestData("IVNAllPIDDetail*#" + JsonConvert.SerializeObject(viewModel.IVNAllPIDDetail));

                                    // Changes For Tork
                                    //int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).pid_dataset_id;
                                    int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).ivn_pid_dataset_id;

                                    if (viewModel.IVNAllPIDDetail.Count != 0)
                                    {
                                        foreach (var item in viewModel.IVNAllPIDDetail.FirstOrDefault(x => x.id == id).frame_datasets)
                                        {
                                            var frame_id = item.frame_id;
                                            var IVN_pid = item.frame_ids;
                                            var check_ecu_name = IVNPidList.FirstOrDefault(x => x.frame_name == ecu.ecu_name);
                                            if (check_ecu_name != null)
                                            {
                                                foreach (var frame_ids_item in item.frame_ids)
                                                {
                                                    FrameId.Add(new PIDFrameId
                                                    {
                                                        FramID = item.frame_id,
                                                        bit_coded = frame_ids_item.bit_coded,
                                                        @byte = frame_ids_item.@byte,
                                                        frame_of_pid_message = frame_ids_item.frame_of_pid_message,
                                                        message_type = frame_ids_item.message_type,
                                                        no_of_bits = frame_ids_item.no_of_bits,
                                                        offset = frame_ids_item.offset,
                                                        pid_description = frame_ids_item.pid_description,
                                                        resolution = frame_ids_item.resolution,
                                                        Selected = frame_ids_item.Selected,
                                                        start_bit = frame_ids_item.start_bit,
                                                        start_byte = frame_ids_item.start_byte,
                                                        unit = frame_ids_item.unit,
                                                        endian = frame_ids_item.endian,
                                                        num_type = frame_ids_item.num_type,
                                                    });
                                                    check_ecu_name.frame_ids.Add(FrameId.FirstOrDefault());
                                                    FrameId.Clear();
                                                }
                                                //check_ecu_name.frame_ids.Add(FrameId.FirstOrDefault());
                                            }
                                            else
                                            {
                                                var Fram = new List<PIDFrameId>();
                                                foreach (var frame_ids_item in item.frame_ids)
                                                {
                                                    FrameId.Add(new PIDFrameId
                                                    {
                                                        FramID = item.frame_id,
                                                        bit_coded = frame_ids_item.bit_coded,
                                                        @byte = frame_ids_item.@byte,
                                                        frame_of_pid_message = frame_ids_item.frame_of_pid_message,
                                                        message_type = frame_ids_item.message_type,
                                                        no_of_bits = frame_ids_item.no_of_bits,
                                                        offset = frame_ids_item.offset,
                                                        pid_description = frame_ids_item.pid_description,
                                                        resolution = frame_ids_item.resolution,
                                                        Selected = frame_ids_item.Selected,
                                                        start_bit = frame_ids_item.start_bit,
                                                        start_byte = frame_ids_item.start_byte,
                                                        unit = frame_ids_item.unit,
                                                        endian = frame_ids_item.endian,
                                                        num_type = frame_ids_item.num_type,
                                                    });
                                                    Fram.Add(FrameId.FirstOrDefault());
                                                    FrameId.Clear();
                                                }

                                                IVNPidList.Add(new IVN_LiveParameterSelectModel
                                                {
                                                    frame_name = ecu.ecu_name,
                                                    frame_id = item.frame_id,
                                                    //frame_ids = item.frame_ids
                                                    frame_ids = Fram
                                                });

                                                //Fram.Clear();
                                                //FrameId.Clear();
                                            }
                                        }

                                        viewModel.ecus_list.Add(
                                        new IvnEcusModel
                                        {
                                            ecu_name = ecu.ecu_name,
                                            opacity = count1 == 1 ? 1 : .5,
                                            pid_list = IVNPidList.FirstOrDefault(x => x.frame_name == ecu.ecu_name).frame_ids.OrderBy(x => x.pid_description).ToList()
                                            /*FirstOrDefault(x => x.id == pid_dataset).codes.OrderBy(x => x.short_name).ToList(),*/
                                        });
                                        
                                        viewModel.selected_ecu = viewModel.ecus_list.FirstOrDefault();
                                        viewModel.pid_list = new ObservableCollection<PIDFrameId>(viewModel.selected_ecu.pid_list);
                                        viewModel.IVN_PidList = IVNPidList;
                                        viewModel.static_pid_list = viewModel.pid_list;
                                        
                                        //App.controlEventManager.SendRequestData("EcuList*#" + JsonConvert.SerializeObject(viewModel.ecus_list));
                                        //App.controlEventManager.SendRequestData("pid_list*#" + JsonConvert.SerializeObject(viewModel.pid_list));
                                        //App.controlEventManager.SendRequestData("SelectedEcu*#" + JsonConvert.SerializeObject(viewModel.selected_ecu));
                                        //App.controlEventManager.SendRequestData("IVN_PidList*#" + JsonConvert.SerializeObject(viewModel.IVN_PidList));
                                    }
                                }
                            }
                            App.controlEventManager.SendRequestData("GetEcuPidList*#");
                        }
                        catch (Exception ex)
                        {

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

        //public  async Task GetPidListForExpert()
        //{
        //    Device.BeginInvokeOnMainThread(async () =>
        //    {
        //        using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
        //        {
        //            await Task.Delay(100);
        //            try
        //            {
        //                //int pid_count = 0;
        //                count1 = 0;
        //                var IVNPidList = new List<IVN_LiveParameterSelectModel>();
        //                var FrameId = new List<PIDFrameId>();

        //                foreach (var ecu in StaticData.ecu_info)
        //                {
        //                    if (ecu.read_dtc_index == "IVN")
        //                    {
        //                        count1++;
        //                        int pid_dataset = ecu.pid_dataset_id;

        //                        //viewModel.AllPIDDetail = get_All_pid_from_local().Result;

        //                        //viewModel.AllPid = viewModel.AllPIDDetail.FirstOrDefault().NormalPID;
        //                        //viewModel.IVNAllPIDDetail = viewModel.AllPIDDetail.FirstOrDefault().IVNPID;

        //                        int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).pid_dataset_id;

        //                        if (viewModel.IVNAllPIDDetail.Count != 0)
        //                        {
        //                            foreach (var item in viewModel.IVNAllPIDDetail.FirstOrDefault(x => x.id == id).frame_datasets)
        //                            {
        //                                var frame_id = item.frame_id;
        //                                var IVN_pid = item.frame_ids;
        //                                var check_ecu_name = IVNPidList.FirstOrDefault(x => x.frame_name == ecu.ecu_name);
        //                                if (check_ecu_name != null)
        //                                {
        //                                    foreach (var frame_ids_item in item.frame_ids)
        //                                    {
        //                                        FrameId.Add(new PIDFrameId
        //                                        {
        //                                            FramID = item.frame_id,
        //                                            bit_coded = frame_ids_item.bit_coded,
        //                                            @byte = frame_ids_item.@byte,
        //                                            frame_of_pid_message = frame_ids_item.frame_of_pid_message,
        //                                            message_type = frame_ids_item.message_type,
        //                                            no_of_bits = frame_ids_item.no_of_bits,
        //                                            offset = frame_ids_item.offset,
        //                                            pid_description = frame_ids_item.pid_description,
        //                                            resolution = frame_ids_item.resolution,
        //                                            Selected = frame_ids_item.Selected,
        //                                            start_bit = frame_ids_item.start_bit,
        //                                            start_byte = frame_ids_item.start_byte,
        //                                            unit = frame_ids_item.unit
        //                                        });
        //                                        check_ecu_name.frame_ids.Add(FrameId.FirstOrDefault());
        //                                        FrameId.Clear();
        //                                    }
        //                                    //check_ecu_name.frame_ids.Add(FrameId.FirstOrDefault());
        //                                }
        //                                else
        //                                {
        //                                    var Fram = new List<PIDFrameId>();
        //                                    foreach (var frame_ids_item in item.frame_ids)
        //                                    {
        //                                        FrameId.Add(new PIDFrameId
        //                                        {
        //                                            FramID = item.frame_id,
        //                                            bit_coded = frame_ids_item.bit_coded,
        //                                            @byte = frame_ids_item.@byte,
        //                                            frame_of_pid_message = frame_ids_item.frame_of_pid_message,
        //                                            message_type = frame_ids_item.message_type,
        //                                            no_of_bits = frame_ids_item.no_of_bits,
        //                                            offset = frame_ids_item.offset,
        //                                            pid_description = frame_ids_item.pid_description,
        //                                            resolution = frame_ids_item.resolution,
        //                                            Selected = frame_ids_item.Selected,
        //                                            start_bit = frame_ids_item.start_bit,
        //                                            start_byte = frame_ids_item.start_byte,
        //                                            unit = frame_ids_item.unit
        //                                        });
        //                                        Fram.Add(FrameId.FirstOrDefault());
        //                                        FrameId.Clear();
        //                                    }

        //                                    IVNPidList.Add(new IVN_LiveParameterSelectModel
        //                                    {
        //                                        frame_name = ecu.ecu_name,
        //                                        frame_id = item.frame_id,
        //                                        //frame_ids = item.frame_ids
        //                                        frame_ids = Fram
        //                                    });

        //                                    //Fram.Clear();
        //                                    //FrameId.Clear();
        //                                }
        //                            }

        //                            viewModel.ecus_list.Add(
        //                            new IvnEcusModel
        //                            {
        //                                ecu_name = ecu.ecu_name,
        //                                opacity = count1 == 1 ? 1 : .5,
        //                                pid_list = IVNPidList.FirstOrDefault(x => x.frame_name == ecu.ecu_name).frame_ids.OrderBy(x => x.pid_description).ToList()
        //                                    /*FirstOrDefault(x => x.id == pid_dataset).codes.OrderBy(x => x.short_name).ToList(),*/
        //                            });

        //                            viewModel.selected_ecu = viewModel.ecus_list.FirstOrDefault();
        //                            viewModel.pid_list = new ObservableCollection<PIDFrameId>(viewModel.selected_ecu.pid_list);
        //                            viewModel.IVN_PidList = IVNPidList;
        //                            viewModel.static_pid_list = viewModel.pid_list;
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //            }
        //        }
        //    });

        //}

        public async Task<List<PID>> get_All_pid_from_local()
        {
            // var res = await services.get_pid(App.JwtToken, 0);
            //return res;

            /* comment */
            //PID
            string Data = string.Empty;
            List<Results> PIDR = new List<Results>();
            Data = DependencyService.Get<ISaveLocalData>().GetData("pidjson");
            if (Data != null)
            {
                PIDR = JsonConvert.DeserializeObject<List<Results>>(Data);
            }

            //IVN PID
            string IVNData = string.Empty;
            List<PIDResult> IVNPID = new List<PIDResult>();
            IVNData = DependencyService.Get<ISaveLocalData>().GetData("IVN_PidJson");
            if (IVNData != null)
            {
                IVNPID = JsonConvert.DeserializeObject<List<PIDResult>>(IVNData);
            }

            var Result = new List<PID>();
            /* comment */
            //Result.Add(new Model.PID { NormalPID = PIDR, IVNPID = IVNPID });            
            Result.Add(new Model.PID { NormalPID = new List<Results>(), IVNPID = IVNPID });
            return Result;
        }

        private void Tab_Clicked(object sender, EventArgs e)
        {
            viewModel.selected_ecu = (IvnEcusModel)((Button)sender).BindingContext;
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
                        //else
                        //{
                        //    ecu.opacity = 1;
                        //}
                    }
                    viewModel.ecus_list.FirstOrDefault(x => x.ecu_name == viewModel.selected_ecu.ecu_name).opacity = 1;
                }
                viewModel.pid_list = new ObservableCollection<PIDFrameId>(viewModel.selected_ecu.pid_list);
                viewModel.static_pid_list = viewModel.pid_list;
                DependencyService.Get<ILodingPageService>().HideLoadingPage();
            }
            catch(Exception ex)
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
                    viewModel.pid_list = new ObservableCollection<PIDFrameId>(viewModel.static_pid_list.Where(x => x.pid_description.ToLower().Contains(viewModel.search_key.ToLower())).ToList());
                }
                else
                {
                    viewModel.pid_list = new ObservableCollection<PIDFrameId>(viewModel.static_pid_list.ToList());
                }
                var selected1 = viewModel.static_pid_list.Where(x => x.Selected);
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
                        if (count < 200)
                        {
                            pid.Selected = true;
                        }
                        else
                        {
                            pid.Selected = false;
                        }

                        //var static_pid = viewModel.static_pid_list.FirstOrDefault(x => x.id == pid.id);
                        //static_pid.Selected = pid.Selected;
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
                viewModel.check_changed_pid = (PIDFrameId)((ImageButton)sender).BindingContext;
                if (viewModel.check_changed_pid == null)
                    return;
                if (viewModel.pid_list.Where(x => x.Selected).ToList().Count < 200)
                {

                    viewModel.check_changed_pid.Selected = !viewModel.check_changed_pid.Selected;
                    var sele = viewModel.pid_list.Where(x => x.Selected).ToList();

                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = $"{viewModel.check_changed_pid.pid_description}",
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
                            ElementName = $"{viewModel.check_changed_pid.pid_description}",
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
                        var errorpage = new Popup.DisplayAlertPage("Alert", "You can select maximum 200 parameters", "OK");
                        await PopupNavigation.Instance.PushAsync(errorpage);
                    }
                }

                var static_pid = viewModel.static_pid_list.FirstOrDefault(x => x.pid_description == viewModel.check_changed_pid.pid_description);
                static_pid.Selected = viewModel.check_changed_pid.Selected;

            }
            catch (Exception ex)
            {
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
            if (viewModel.pid_list != null)
            {
                if (viewModel.pid_list.Count > 0)
                {
                    ObservableCollection<IVN_SelectedPID> IVNpidList = new ObservableCollection<IVN_SelectedPID>();

                    var selected_pid_list = viewModel.IVN_PidList.FirstOrDefault(x => x.frame_name == viewModel.selected_ecu.ecu_name);

                    if (selected_pid_list != null)
                    {
                        if (selected_pid_list.frame_ids != null)
                        {
                            var PIDFrameId = new List<PIDFrameId>();
                            var selected_pids = selected_pid_list.frame_ids.Where(x => x.Selected == true);
                            foreach (var item in selected_pids)
                            {
                                if(item.FramID.Length < 8)
                                    item.FramID = item.FramID.PadLeft(8,'0');
                                PIDFrameId.Add(new Model.PIDFrameId
                                {
                                    FramID = item.FramID,
                                    bit_coded = item.bit_coded,
                                    @byte = item.@byte,
                                    frame_of_pid_message = item.frame_of_pid_message,
                                    message_type = item.message_type,
                                    no_of_bits = item.no_of_bits,
                                    offset = item.offset,
                                    pid_description = item.pid_description,
                                    resolution = item.resolution,
                                    Selected = item.Selected,
                                    start_bit = item.start_bit,
                                    start_byte = item.start_byte,
                                    unit = item.unit,
                                    endian = item.endian,
                                    num_type = item.num_type,
                                });
                            }
                            IVNpidList.Add(new IVN_SelectedPID { frame_id = selected_pid_list.frame_id, frame_ids = PIDFrameId });

                            //var json = JsonConvert.SerializeObject(registerDongleModel);
                        }
                    }


                    var selected_pid = viewModel.static_pid_list.Where(x => x.Selected).ToList();

                    if (selected_pid.Count > 0)
                    {
                        await Navigation.PushAsync(new IvnPidSelectedPage(IVNpidList));
                        //await Navigation.PushAsync(new IvnLiveParameterSelectedPage(null, IVNpidList, viewModel.selected_ecu.ecu_name));
                        //await page.Navigation.PushAsync(new IvnLiveParameterSelectedPage(new ObservableCollection<PidCode>(selected_pid)));
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
}