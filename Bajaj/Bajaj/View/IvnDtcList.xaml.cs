using Acr.UserDialogs;
using MultiEventController.Models;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Popup;
using Bajaj.Services;
using Bajaj.View.GdSection;
using Bajaj.View.ViewModel;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class IvnDtcList : DisplayAlertPage
    {
        IvnDtcViewModel viewModel;
        ApiServices services;
        JobCardListModel jobCard;
        bool IsPageRemove = false;
        string selected_ecu = string.Empty;

        public IvnDtcList(JobCardListModel jobCardSession)
        {
            try
            {
                InitializeComponent();
                jobCard = new JobCardListModel();
                jobCard = jobCardSession;
            }
            catch (Exception ex)
            {

            }

        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
            BindingContext = viewModel = new IvnDtcViewModel();
            services = new ApiServices();
            IsPageRemove = true;
            await GetDTCList();

        }

        protected override void OnDisappearing()
        {
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
        private void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {

            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
            {
                //await Task.Delay(100);
                Device.BeginInvokeOnMainThread(async () =>
                {
                    string data = (string)sender; //sender as string;
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (data.Contains("NoDtcFromServer*#"))
                        {
                            await UserDialogs.Instance.AlertAsync("DTC not found from server", "Failed", "Ok");
                            viewModel.DTCFoundOrNotMessage = "DTC not found from server.";
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("JDNotFound_"))
                        {
                            var errorpage = new Popup.DisplayAlertPage("", "Gd not found for this DTC", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                        else if (data.Contains("DtcClearOk*#"))
                        {
                            var errorpage = new Popup.DisplayAlertPage("Alert", "DTC Cleared", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                        else if (data.Contains("EcuList*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.ecus_list = JsonConvert.DeserializeObject<ObservableCollection<DtcEcusModel>>(PairedData[1]); ;
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("DtcList*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.dtc_list = JsonConvert.DeserializeObject<ObservableCollection<DtcCode>>(PairedData[1]); ;
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("SelectedEcu*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.selected_ecu = JsonConvert.DeserializeObject<DtcEcusModel>(PairedData[1]);
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                        else if (data.Contains("empty_view_text"))
                        {
                            PairedData = data.Split('#');
                            viewModel.empty_view_text = JsonConvert.DeserializeObject<string>(PairedData[1]);
                            viewModel.is_running = false;
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                    }
                    

                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    string data = (string)sender; //sender as string;
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (data.Contains("SelectedEcu*#"))
                        {
                            PairedData = data.Split('#');
                            viewModel.selected_ecu = JsonConvert.DeserializeObject<DtcEcusModel>(PairedData[1]);
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
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
            if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("DtcListScrolled"))
            {
                collectionView.ScrollTo(Convert.ToInt32(elementEventHandler.ElementName), 0, ScrollToPosition.Start);
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("BtnRefreshClicked"))
            {
                await GetDTCList();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("BtnClearClicked"))
            {
                await DtcClear();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("Tab_Clicked"))
            {
                viewModel.selected_ecu = JsonConvert.DeserializeObject<DtcEcusModel>(elementEventHandler.ElementName);
                await ChangeEcuTab();
            }
            else if (!CurrentUserEvent.Instance.IsExpert && ReceiveValue.Contains("GD_"))
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    string[] Remove = { "GD_" };
                    string[] Data = ReceiveValue.Split(Remove, StringSplitOptions.RemoveEmptyEntries);
                    var ClickedRowValue = JsonConvert.DeserializeObject<DtcListModel>(Data[0]);
                    if (ClickedRowValue != null)
                    {
                        var res = await services.Get_gd(App.JwtToken, ClickedRowValue.code, App.sub_model_id);
                        if (res != null)
                        {
                            if (!CurrentUserEvent.Instance.IsExpert)
                            {
                                var jsonData = JsonConvert.SerializeObject(res);
                                await Task.Delay(200);
                                App.controlEventManager.SendRequestData("Troubleshoot_ResultGetGD_" + jsonData);
                            }

                            if (res.count != 0)
                            {
                                await this.Navigation.PushAsync(new InfoPage(res.results));
                            }
                            else
                            {
                                //await this.DisplayAlert("", "JD Not Found for this DTC", "OK");

                                if (!CurrentUserEvent.Instance.IsExpert)
                                {
                                    await Task.Delay(200);
                                    App.controlEventManager.SendRequestData("JDNotFound_");
                                }

                                var errorpage = new Popup.DisplayAlertPage("", "Gd not found for this DTC", "OK");
                                await PopupNavigation.Instance.PushAsync(errorpage);
                            }
                        }
                        else
                        {
                            if (!CurrentUserEvent.Instance.IsExpert)
                            {
                                await Task.Delay(200);
                                App.controlEventManager.SendRequestData("JDNotFound_");
                            }

                            var errorpage = new Popup.DisplayAlertPage("", "Gd not found for this DTC", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }


                        //else
                        //{
                        //    await this.DisplayAlert("", "JD Not Found for this DTC", "OK");
                        //}
                    }
                    else
                    {
                        if (!CurrentUserEvent.Instance.IsExpert)
                        {
                            await Task.Delay(200);
                            App.controlEventManager.SendRequestData("JDNotFound_");
                        }

                        var errorpage = new Popup.DisplayAlertPage("", "Gd not found for this DTC", "OK");
                        await PopupNavigation.Instance.PushAsync(errorpage);

                        //await this.DisplayAlert("", "JD Not Found for this DTC", "OK");
                    }
                    //else
                    //{
                    //    await this.DisplayAlert("", "JD Not Found for this DTC", "OK");
                    //}
                }
            }
            DependencyService.Get<ILodingPageService>().HideLoadingPage();
            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }

        List<PostDtcRecord> PDR;
        string read_dtc_index = (dynamic)null;
        List<IVN_Result> IVN_Result = new List<IVN_Result>();
        DtcEcusModel dtcEcusModel = new DtcEcusModel();
        public async Task GetDTCList()
        {
            viewModel.DTCFoundOrNotMessage = "Looking for DTC Record";
            //viewModel.is_running = true;
            //viewModel.empty_view_text = "Loading...";
            int count = 0;
            viewModel.ecus_list = new ObservableCollection<DtcEcusModel>();
            viewModel.dtc_list = new ObservableCollection<DtcCode>();

            if (!CurrentUserEvent.Instance.IsExpert)
            {
                try
                {
                    foreach (var ecu in StaticData.ecu_info)
                    {
                        count++;
                        PDR = new List<PostDtcRecord>();

                        int id = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).dtc_dataset_id;

                        var Vales = get_dtc_All_Recoed(ecu.dtc_dataset_id);
                        if (Vales == null)
                        {
                            App.controlEventManager.SendRequestData("NoDtcFromServer*#");
                            await UserDialogs.Instance.AlertAsync("DTC not found from server", "Failed", "Ok");
                            viewModel.DTCFoundOrNotMessage = "DTC not found from server.";
                            return;
                        }

                        dtcEcusModel = new DtcEcusModel();
                        dtcEcusModel.ecu_name = ecu.ecu_name;
                        dtcEcusModel.opacity = count == 1 ? 1 : .5;
                        //Debug.WriteLine($"Dtc View Model : ECU NAME = {ecu.ecu_name},  DTC DATASET ID = {dtc_dataset}");

                        if (Vales != null)
                        {
                            foreach (var item in Vales.Result)
                            {
                                if (item.DTC_R.Count != 0)
                                {
                                    foreach (var item2 in item.DTC_R)
                                    {
                                        if (item2.id == id)
                                        {
                                            //var server_dtcc = viewModel.AllDtc.FirstOrDefault(x => x.id == id).dtc_code;
                                            var server_dtcc = Vales.Result.FirstOrDefault().DTC_R.FirstOrDefault(X => X.id == id).dtc_code1;
                                            await Task.Delay(500);
                                            viewModel.ServerDtcList.Add(new DtcModel { ecu_name = ecu.ecu_name, dtc_list = server_dtcc });
                                            read_dtc_index = StaticData.ecu_info.FirstOrDefault(x => x.ecu_name == ecu.ecu_name).read_dtc_index;
                                        }
                                    }
                                    //App.controlEventManager.SendRequestData("ServerDtcList*#" + JsonConvert.SerializeObject(viewModel.ServerDtcList));
                                    //App.controlEventManager.SendRequestData("read_dtc_index*#" + JsonConvert.SerializeObject(read_dtc_index));
                                }

                                if (item.IDCT_R.Count != 0)
                                {
                                    foreach (var item2 in item.IDCT_R)
                                    {
                                        if (item2.id == id)
                                        {

                                            //await Task.Delay(500);
                                            IVN_Result.Add(new IVN_Result
                                            {
                                                id = item2.id,
                                                code = item2.code,
                                                description = item2.description,
                                                frame_datasets = item2.frame_datasets
                                            });

                                        }
                                    }
                                    //App.controlEventManager.SendRequestData("IVN_Result*#" + JsonConvert.SerializeObject(IVN_Result));
                                }
                            }
                        }

                        List<DtcListModel> ecu_dtc = await Task.Run(async () =>
                        {

                            var result = await readDtc(read_dtc_index, ecu.ecu_name, IVN_Result);
                            return result;
                        });

                        if(viewModel.DTC_Error == "NO_ERROR")
                        {
                            if (ecu_dtc.Count > 0)
                            {
                                viewModel.is_running = false;
                                var Dtc_list = new ObservableCollection<DtcListModel>(ecu_dtc);

                                dtcEcusModel.dtc_list = new List<DtcCode>();

                                foreach (var item1 in Dtc_list)
                                {
                                    DtcCode dtcCode = new DtcCode
                                    {
                                        id = item1.id,
                                        code = item1.code,
                                        description = item1.description,
                                        status_activation = item1.status,
                                        lamp_activation = item1.status
                                    };
                                    dtcEcusModel.dtc_list.Add(dtcCode);
                                    viewModel.dtc_list.Add(dtcCode);
                                }
                            }
                            else
                            {
                                dtcEcusModel.dtc_list = new List<DtcCode>();
                            }
                            viewModel.ecus_list.Add(dtcEcusModel);
                            //viewModel.selected_ecu = viewModel.ecus_list[0];
                        }
                        else
                        {
                            dtcEcusModel.dtc_list = new List<DtcCode>();
                            viewModel.ecus_list.Add(dtcEcusModel);
                            viewModel.empty_view_text = viewModel.DTC_Error;
                            App.controlEventManager.SendRequestData("empty_view_text*#" + JsonConvert.SerializeObject(viewModel.empty_view_text));
                        }


                    }

                    viewModel.dtc_list = new ObservableCollection<DtcCode>(viewModel.ecus_list.FirstOrDefault().dtc_list);
                    viewModel.selected_ecu = viewModel.ecus_list[0];
                    App.controlEventManager.SendRequestData("DtcList*#" + JsonConvert.SerializeObject(viewModel.dtc_list));
                    App.controlEventManager.SendRequestData("EcuList*#" + JsonConvert.SerializeObject(viewModel.ecus_list));
                    App.controlEventManager.SendRequestData("SelectedEcu*#" + JsonConvert.SerializeObject(viewModel.selected_ecu));
                }
                catch (Exception ex)
                {

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

        List<IVN_Result> IVNDTC = new List<IVN_Result>();
        List<IVNdtc_AND_dtc> Result = new List<IVNdtc_AND_dtc>();
        IVNdtc_AND_dtc iVNdtc_AND_Dtc = new IVNdtc_AND_dtc();
        List<string> FrameIdList;
        public async Task<List<IVNdtc_AND_dtc>> get_dtc_All_Recoed(int ecu_id)
        {
            try
            {
                
                    //DTC
                    string Data = string.Empty;
                    List<DtcResults> DTC = new List<DtcResults>();
                    Data = DependencyService.Get<ISaveLocalData>().GetData("dtcjson");
                    if (Data != null)
                    {
                        DTC = JsonConvert.DeserializeObject<List<DtcResults>>(Data);
                    }

                    //IVN DTC
                    string IVNData = string.Empty;
                    //List<IVN_Result> IVNDTC = new List<IVN_Result>();
                    FrameIdList = new List<string>();
                    IVNData = DependencyService.Get<ISaveLocalData>().GetData("IVN_DtcJson");

                    if (IVNData != null)
                    {
                        IVNDTC = JsonConvert.DeserializeObject<List<IVN_Result>>(IVNData);
                        foreach (var item in IVNDTC.ToList())
                        {
                            if (item.id == ecu_id)
                            {
                                FrameIdList.Add(item.frame_datasets.FirstOrDefault().frame_id);
                                iVNdtc_AND_Dtc.IDCT_R.Add(item);
                            }
                        }
                    }


                    Result.Add(iVNdtc_AND_Dtc);

                //App.controlEventManager.SendRequestData("FrameIdList*#" + JsonConvert.SerializeObject(FrameIdList));
                //App.controlEventManager.SendRequestData("Result*#" + JsonConvert.SerializeObject(Result));
                //App.controlEventManager.SendRequestData("IVNDTC*#" + JsonConvert.SerializeObject(IVNDTC));
                //App.controlEventManager.SendRequestData("iVNdtc_AND_Dtc*#" + JsonConvert.SerializeObject(iVNdtc_AND_Dtc));

                return Result;
                

            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public ObservableCollection<IvnReadDtcResponseModel> IvnResponse;
        ReadDtcResponseModel read_dtc = new ReadDtcResponseModel();
        public async Task<List<DtcListModel>> readDtc(string read_dtc_index, string ecu, List<IVN_Result> iVN_Result)
        {
            try
            {
                IvnResponse = new ObservableCollection<IvnReadDtcResponseModel>();
                viewModel.DtcList = new List<DtcListModel>();

                Device.BeginInvokeOnMainThread(async () =>
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {

                        switch (Device.RuntimePlatform)
                        {
                            case Device.iOS:
                                //top = 20;
                                break;
                            case Device.Android:
                                if (App.ConnectedVia == "USB")
                                {
                                    //read_dtc = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadDtc(read_dtc_index);

                                    if (iVN_Result != null)
                                    {
                                        //var list = FrameIdList.Distinct().ToList();
                                        IvnResponse = await DependencyService.Get<Interfaces.IConnectionUSB>().IVN_ReadDtc(FrameIdList);
                                    }
                                    else
                                    {
                                        read_dtc = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadDtc(read_dtc_index);
                                    }
                                }
                                else if (App.ConnectedVia == "BT")
                                {
                                    if (FrameIdList.Count > 0)
                                    {
                                        //var list = FrameIdList.Distinct().ToList();
                                        IvnResponse = await DependencyService.Get<Interfaces.IBth>().IVN_ReadDtc(FrameIdList);
                                    }
                                    else
                                    {
                                        read_dtc = await DependencyService.Get<Interfaces.IBth>().ReadDtc(read_dtc_index);
                                    }
                                }
                                else
                                {
                                    if (FrameIdList.Count > 0)
                                    {
                                        //var list = FrameIdList.Distinct().ToList();
                                        IvnResponse = await DependencyService.Get<Interfaces.IConnectionWifi>().IVN_ReadDtc(FrameIdList);
                                    }
                                    else
                                    {
                                        read_dtc = await DependencyService.Get<Interfaces.IConnectionWifi>().ReadDtc(read_dtc_index);
                                    }
                                }
                                break;
                            case Device.UWP:
                                read_dtc = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadDtc(read_dtc_index);
                                break;
                            default:
                                //top = 0;
                                break;
                        }
                    }
                });

                Stopwatch sw = new Stopwatch();
                sw.Start();
                while(sw.Elapsed.Milliseconds < 10000 && IvnResponse.Count == 0)
                {

                }

                //var DTCMask = await services.GetDTC_mask(App.JwtToken);
                var Data = DependencyService.Get<ISaveLocalData>().GetData("DTCMaskJsonData");
                List<DTCMaskResult> MaskResult = new List<DTCMaskResult>();
                List<DTCMaskMaskId> MaskiSActive_ids = new List<DTCMaskMaskId>();
                if (Data != null)
                {
                    var DTCMask = JsonConvert.DeserializeObject<DTCMaskRoot>(Data);
                    MaskResult = DTCMask.results;

                    MaskiSActive_ids = MaskResult.FirstOrDefault(x => x?.is_active == "ACTIVE")?.mask_ids;
                }

                byte actual_acknjmnt = 0x00;
                if (FrameIdList.Count > 0)
                {
                    if (IvnResponse.Count > 0)
                    {
                        viewModel.DTC_Error = "NO_ERROR";
                        foreach (var item in IvnResponse.ToList())
                        {

                            if (item.ECUResponseStatus == "NOERROR")
                            {
                                foreach (var ivn_item in IVNDTC.ToList())
                                {
                                    var ivn_frame = ivn_item.frame_datasets.FirstOrDefault(x => x.frame_id == item.Frame);

                                    if (ivn_frame != null)
                                    {
                                        foreach (var dtc in ivn_frame.frame_ids.ToList())
                                        {
                                            int byte_no = Convert.ToInt32(dtc.@byte) - 1;
                                            int bit_no = Convert.ToInt32(dtc.bit) - 1;

                                            if (item.ActualDataBytes == null)
                                            {
                                                actual_acknjmnt = 0x00;
                                            }
                                            else
                                            {
                                                actual_acknjmnt = item.ActualDataBytes[byte_no];
                                            }

                                            string bit = Convert.ToString(actual_acknjmnt, 2).PadLeft(8, '0');
                                            char res = bit[bit_no];

                                            if (res == '1')
                                            {
                                                DtcListModel DtcModel = new DtcListModel
                                                {
                                                    code = dtc.dtc_code,
                                                    //status = dtc.status,
                                                    description = dtc.dtc_description
                                                };
                                                viewModel.DtcList.Add(DtcModel);
                                            }
                                            else
                                            { }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                viewModel.DTC_Error = item.ECUResponseStatus;
                            }
                        }

                    }
                    else
                    {
                        viewModel.DTC_Error = "No DTC Found";
                    }
                }
                else
                {
                    if (read_dtc.status == "NO_ERROR")
                    {
                        DtcListModel dtcListModel = new DtcListModel();

                        var PDR = new List<PostDtcRecord>();

                        var DTCLength = read_dtc.dtcs.Length;
                        var noofdtc = DTCLength / 2;

                        for (int i = 0; i < noofdtc; i++)
                        {
                            dtcListModel.code = read_dtc.dtcs[i, 0].ToString();

                            if (MaskResult != null && MaskiSActive_ids != null)
                            {
                                var Vehical_model = MaskiSActive_ids.FirstOrDefault(x => x.vehicle_model == App.model_id);
                                if (Vehical_model != null)
                                {
                                    foreach (var DtcCodeitem in MaskiSActive_ids.FirstOrDefault().dtc_code)
                                    {
                                        if (DtcCodeitem != dtcListModel.code)
                                        {
                                            dtcListModel.status = read_dtc.dtcs[i, 1].ToString();

                                            string desc = viewModel.ServerDtcList.FirstOrDefault(x => x.ecu_name == ecu).dtc_list.FirstOrDefault(x => x.code == dtcListModel.code)?.description;
                                            if (desc != null)
                                            {
                                                dtcListModel.description = desc;
                                            }
                                            else
                                            {
                                                dtcListModel.description = "Description not found";
                                            }

                                            DtcListModel DtcModel = new DtcListModel
                                            {
                                                code = dtcListModel.code,
                                                status = dtcListModel.status,
                                                description = dtcListModel.description
                                            };

                                            PDR.Add(new PostDtcRecord { value = dtcListModel.code, status = dtcListModel.status });

                                            viewModel.DtcList.Add(DtcModel);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    dtcListModel.status = read_dtc.dtcs[i, 1].ToString();

                                    string desc = viewModel.ServerDtcList.FirstOrDefault(x => x.ecu_name == ecu).dtc_list.FirstOrDefault(x => x.code == dtcListModel.code)?.description;
                                    if (desc != null)
                                    {
                                        dtcListModel.description = desc;
                                    }
                                    else
                                    {
                                        dtcListModel.description = "Description not found";
                                    }

                                    DtcListModel DtcModel = new DtcListModel
                                    {
                                        code = dtcListModel.code,
                                        status = dtcListModel.status,
                                        description = dtcListModel.description
                                    };

                                    PDR.Add(new PostDtcRecord { value = dtcListModel.code, status = dtcListModel.status });

                                    viewModel.DtcList.Add(DtcModel);
                                }
                            }
                            else
                            {
                                dtcListModel.status = read_dtc.dtcs[i, 1].ToString();

                                string desc = viewModel.ServerDtcList.FirstOrDefault(x => x.ecu_name == ecu).dtc_list.FirstOrDefault(x => x.code == dtcListModel.code)?.description;
                                if (desc != null)
                                {
                                    dtcListModel.description = desc;
                                }
                                else
                                {
                                    dtcListModel.description = "Description not found";
                                }

                                DtcListModel DtcModel = new DtcListModel
                                {
                                    code = dtcListModel.code,
                                    status = dtcListModel.status,
                                    description = dtcListModel.description
                                };

                                PDR.Add(new PostDtcRecord { value = dtcListModel.code, status = dtcListModel.status });

                                viewModel.DtcList.Add(DtcModel);
                            }


                        }
                        viewModel.DTC_Error = "NO_ERROR";
                        var isReachable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
                        if (isReachable)
                            services.DtcRecord(PDR, App.JwtToken, App.SessionId);
                    }
                    else
                    {
                        viewModel.DTC_Error = read_dtc.status;
                    }
                }

                return viewModel.DtcList;
            }
            catch (Exception ex)
            {
                viewModel.DtcList = new List<DtcListModel>();
                return viewModel.DtcList;
            }
        }

        
        private void Tab_Clicked(object sender, EventArgs e)
        {
            viewModel.selected_ecu = (DtcEcusModel)((Button)sender).BindingContext;
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
            if (viewModel.selected_ecu != null)
            {
                viewModel.selected_ecu.opacity = 1;

                foreach (var ecu in viewModel.ecus_list)
                {
                    if(viewModel.selected_ecu.ecu_name != ecu.ecu_name)
                    {
                        ecu.opacity = .5;
                    }
                }
                viewModel.ecus_list.FirstOrDefault(x => x.ecu_name == viewModel.selected_ecu.ecu_name).opacity = 1;
            }
            viewModel.dtc_list = new ObservableCollection<DtcCode>(viewModel.selected_ecu.dtc_list);
            DependencyService.Get<ILodingPageService>().HideLoadingPage();
        }

        private void btnRefresh_Clicked(object sender, EventArgs e)
        {
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
                                ElementName = "btnRefreshFrame",
                                ElementValue = "BtnRefreshClicked",
                                ToUserId = CurrentUserEvent.Instance.ToUserId,
                                IsExpert = CurrentUserEvent.Instance.IsExpert
                            });

                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                            DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                            await Task.Delay(200);
                        }
                        await GetDTCList();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            });
        }

        private void btnClear_Clicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    if (CurrentUserEvent.Instance.IsExpert)
                    {
                        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                        {
                            ElementName = "BtnClearFrame",
                            ElementValue = "BtnClearClicked",
                            ToUserId = CurrentUserEvent.Instance.ToUserId,
                            IsExpert = CurrentUserEvent.Instance.IsExpert
                        });
                        DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                        await Task.Delay(200);
                    }
                    await DtcClear();
                }
            });
        }

        string clear_dtc_index = string.Empty;
        string Clear_dtc_android = string.Empty;
        public async Task DtcClear()
        {
            try
            {
                foreach (var item in StaticData.ecu_info)
                {
                    if (item.ecu_name == viewModel.selected_ecu.ecu_name)
                    {
                        clear_dtc_index = item.clear_dtc_index;
                        if (App.ConnectedVia == "USB")
                        {
                            Clear_dtc_android = await DependencyService.Get<Interfaces.IConnectionUSB>().ClearDtc(clear_dtc_index);
                        }
                        else if (App.ConnectedVia == "BT")
                        {
                            Clear_dtc_android = await DependencyService.Get<Interfaces.IBth>().ClearDtc(clear_dtc_index);
                        }
                        else
                        {
                            Clear_dtc_android = await DependencyService.Get<Interfaces.IConnectionWifi>().ClearDtc(clear_dtc_index);
                        }
                        if (Clear_dtc_android.Contains("NOERROR"))
                        {
                            var ClearDTC = new List<ClearDtcRecord>();
                            ClearDTC.Add(new ClearDtcRecord { session = jobCard.id, status = "NOERROR" });

                            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                            if (isReachable)
                            {
                                services.ClearDtcRecord(ClearDTC, App.JwtToken, App.SessionId);
                            }

                            await GetDTCList();
                            App.controlEventManager.SendRequestData("DtcClearOk*#");
                            var errorpage = new Popup.DisplayAlertPage("Alert", "DTC Cleared", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                        else
                        {
                            //App.controlEventManager.SendRequestData($"DtcClearNotOk*#{Clear_dtc_android}");
                            //var NegativeAcknowledgement = new Popup.DisplayAlertPage("Alert", $"Negative Acknowledgement \n\n {Clear_dtc_android}", "OK");
                            //await PopupNavigation.Instance.PushAsync(NegativeAcknowledgement);
                        }
                    }
                }
            }
            catch (Exception ex)
            {


            }
        }


        private void DtcListScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (CurrentUserEvent.Instance.IsExpert)
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = Convert.ToString(e.FirstVisibleItemIndex),
                    ElementValue = "DtcListScrolled",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }
        }

        private async void GD_Clicked(object sender, EventArgs e)
        {
            try
            {
                IsPageRemove = false;
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.TreeSurveyPage = true;
                    string model = string.Empty;
                    var ee = e;
                    var SenderData = (Button)sender;
                    var ClickedRowValue = (DtcCode)((Button)sender).BindingContext;//SenderData.Parent.BindingContext as DtcListModel;
                    var JsonData = JsonConvert.SerializeObject(ClickedRowValue);
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "GD_Clicked",
                        ElementValue = "GD_" + JsonData,
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
                else
                {
                    #region Working Code Without Remote
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        string model = string.Empty;
                        var ee = e;
                        var SenderData = (Button)sender;
                        //var ClickedRowValue = SenderData.Parent.BindingContext as DtcListModel;
                        var ClickedRowValue = (DtcCode)((Button)sender).BindingContext;
                        //var DtcID = viewModel.AllDtc.FirstOrDefault().dtc_code.FirstOrDefault(x => x.code == ClickedRowValue.code);

                        var res = await services.Get_gd(App.JwtToken, ClickedRowValue.code, App.sub_model_id);
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            if (res != null)
                            {
                                if (res.results.Count() != 0)
                                {
                                    await this.Navigation.PushAsync(new InfoPage(res.results));
                                }
                                else
                                {
                                    //await this.DisplayAlert("", "JD Not Found for this DTC", "OK");

                                    if (!CurrentUserEvent.Instance.IsExpert)
                                    {
                                        App.controlEventManager.SendRequestData("JDNotFound_");
                                        await Task.Delay(200);
                                    }

                                    var errorpage = new Popup.DisplayAlertPage("", "Gd not found for this DTC", "OK");
                                    await PopupNavigation.Instance.PushAsync(errorpage);
                                }
                            }
                            else
                            {
                                if (!CurrentUserEvent.Instance.IsExpert)
                                {
                                    App.controlEventManager.SendRequestData("JDNotFound_");
                                    await Task.Delay(200);
                                }

                                var errorpage = new Popup.DisplayAlertPage("", "Gd not found for this DTC", "OK");
                                await PopupNavigation.Instance.PushAsync(errorpage);
                            }
                        });
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                await this.DisplayAlert("", ex.ToString(), "OK");
            }

        }

    }

}