
using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class IvnDtcListViewModel : BaseViewModel
    {
        ApiServices services;
        List<string> FrameIdList;

        public IvnDtcListViewModel()
        {
            try
            {
                services = new ApiServices();
                FrameIdList = new List<string>();
                ServerDtcList = new List<DtcModel>();
                EcuDtcList = new List<DtcModel>();
                DtcList = new List<DtcListModel>();
                //AllDtc = get_dtc_from_local().Result;


                AllDtc = new List<DtcResults>();// get_dtc_All_Recoed().Result.FirstOrDefault().DTC_R;
                //IVNAllDTCDetail = get_dtc_All_Recoed().Result.FirstOrDefault().IDCT_R;

                DTC_Error = "NoValue";
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.StackTrace}", "Alert", "Ok");
            }

        }

        private List<IVN_DtcModel> serverIVNDtcList;
        public List<IVN_DtcModel> ServerIVNDtcList
        {
            get => serverIVNDtcList;
            set
            {
                serverIVNDtcList = value;
                OnPropertyChanged("ServerIVNDtcList");
            }
        }


        private List<DtcModel> serverDtcList;
        public List<DtcModel> ServerDtcList
        {
            get => serverDtcList;
            set
            {
                serverDtcList = value;
                OnPropertyChanged("ServerDtcList");
            }
        }

        private List<DtcModel> ecuDtcList;
        public List<DtcModel> EcuDtcList
        {
            get => ecuDtcList;
            set
            {
                ecuDtcList = value;
                OnPropertyChanged("EcuDtcList");
            }
        }

        private List<DtcListModel> dtcList;
        public List<DtcListModel> DtcList
        {
            get => dtcList;
            set
            {
                dtcList = value;
                OnPropertyChanged("DtcList");
            }
        }

        private List<DtcResults> allDtc;
        public List<DtcResults> AllDtc
        {
            get => allDtc;
            set
            {
                allDtc = value;
                OnPropertyChanged("AllDtc");
            }
        }

        private List<IVN_Result> _IVNAllDTCDetail;
        public List<IVN_Result> IVNAllDTCDetail
        {
            get => _IVNAllDTCDetail;
            set
            {
                _IVNAllDTCDetail = value;
                OnPropertyChanged("IVNAllDTCDetail");
            }
        }

        public async Task<List<IVNdtc_AND_dtc>> get_dtc_from_local()
        {
            try
            {
                //var DTC = await services.get_dtc(App.JwtToken, 0);
                //var IVNDTC = await services.get_ivn_dtc(App.JwtToken, 0);

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
                List<IVN_Result> IVNDTC = new List<IVN_Result>();
                IVNData = DependencyService.Get<ISaveLocalData>().GetData("IVN_DtcJson");
                if (IVNData != null)
                {
                    IVNDTC = JsonConvert.DeserializeObject<List<IVN_Result>>(IVNData);
                }

                var Result = new List<IVNdtc_AND_dtc>();
                Result.Add(new IVNdtc_AND_dtc { DTC_R = DTC, IDCT_R = IVNDTC });
                return Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        List<IVN_Result> IVNDTC = new List<IVN_Result>();
        List<IVNdtc_AND_dtc> Result = new List<IVNdtc_AND_dtc>();
        IVNdtc_AND_dtc iVNdtc_AND_Dtc = new IVNdtc_AND_dtc();
        public async Task<List<IVNdtc_AND_dtc>> get_dtc_All_Recoed(int ecu_id)
        {
            try
            {
                //var DTC = await services.get_dtc(App.JwtToken, 0);
                //var IVNDTC = await services.get_ivn_dtc(App.JwtToken, 0);

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

                //var Result = new List<IVNdtc_AND_dtc>();
                //IVNdtc_AND_dtc
                //Result.Add(new IVNdtc_AND_dtc { DTC_R = DTC, IDCT_R = IVNDTC });


                //foreach (var item in IVNDTC.ToList())
                //{
                //    foreach (var item1 in item.frame_datasets)
                //    {
                //        FrameIdList.Add(item1.frame_id);
                //    }
                //}
                return Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string _DTC_Error;
        public string DTC_Error
        {
            get => _DTC_Error;
            set
            {
                _DTC_Error = value;
                OnPropertyChanged("DTC_Error");
            }
        }

        public ObservableCollection<IvnReadDtcResponseModel> IvnResponse;
        public async Task<List<DtcListModel>> readDtc(string read_dtc_index, string ecu, List<IVN_Result> iVN_Result)
        {
            try
            {
                IvnResponse = new ObservableCollection<IvnReadDtcResponseModel>();
                DtcList = new List<DtcListModel>();
                ReadDtcResponseModel read_dtc = new ReadDtcResponseModel();
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
                    if (IvnResponse != null)
                    {
                        DTC_Error = "NO_ERROR";
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
                                                DtcList.Add(DtcModel);
                                            }
                                            else
                                            { }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                DTC_Error = item.ECUResponseStatus;
                            }
                        }
                        //if (iVN_Result.FirstOrDefault().code.Contains("IVN") == true)
                        //{
                        //    if (DtcResponseModel_IVN.FirstOrDefault().status == "Active")
                        //    {
                        //        DtcListModel dtcListModel = new DtcListModel();
                        //        var PDR = new List<PostDtcRecord>();

                        //        foreach (var item in DtcResponseModel_IVN)
                        //        {

                        //            var Value = DtcList.FirstOrDefault(x => x.code == item.dtc_code)?.code;
                        //            if (item.dtc_code != Value)
                        //            {
                        //                if (MaskResult != null && MaskiSActive_ids != null)
                        //                {
                        //                    var Vehical_model = MaskiSActive_ids.FirstOrDefault(x => x.vehicle_model == App.model_id);
                        //                    if (Vehical_model != null)
                        //                    {
                        //                        foreach (var DtcCodeitem in MaskiSActive_ids.FirstOrDefault().dtc_code)
                        //                        {
                        //                            if (DtcCodeitem != item.dtc_code)
                        //                            {
                        //                                DtcListModel DtcModel = new DtcListModel
                        //                                {
                        //                                    code = item.dtc_code,
                        //                                    status = item.status,
                        //                                    description = item.dtc_description
                        //                                };
                        //                                DtcList.Add(DtcModel);
                        //                                break;
                        //                            }
                        //                        }
                        //                    }
                        //                    else
                        //                    {
                        //                        DtcListModel DtcModel = new DtcListModel
                        //                        {
                        //                            code = item.dtc_code,
                        //                            status = item.status,
                        //                            description = item.dtc_description
                        //                        };
                        //                        DtcList.Add(DtcModel);
                        //                    }
                        //                }
                        //                else
                        //                {
                        //                    DtcListModel DtcModel = new DtcListModel
                        //                    {
                        //                        code = item.dtc_code,
                        //                        status = item.status,
                        //                        description = item.dtc_description
                        //                    };
                        //                    DtcList.Add(DtcModel);
                        //                }
                        //            }

                        //        }

                        //        PDR.Add(new PostDtcRecord { value = dtcListModel.code, status = dtcListModel.status });


                        //        DTC_Error = "NO_ERROR";

                        //        var isReachable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
                        //        if (isReachable)
                        //            services.DtcRecord(PDR, App.JwtToken, App.SessionId);
                        //    }
                        //    else
                        //    {
                        //        DTC_Error = read_dtc.status;
                        //    }
                        //}
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

                                            string desc = ServerDtcList.FirstOrDefault(x => x.ecu_name == ecu).dtc_list.FirstOrDefault(x => x.code == dtcListModel.code)?.description;
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

                                            DtcList.Add(DtcModel);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    dtcListModel.status = read_dtc.dtcs[i, 1].ToString();

                                    string desc = ServerDtcList.FirstOrDefault(x => x.ecu_name == ecu).dtc_list.FirstOrDefault(x => x.code == dtcListModel.code)?.description;
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

                                    DtcList.Add(DtcModel);
                                }
                            }
                            else
                            {
                                dtcListModel.status = read_dtc.dtcs[i, 1].ToString();

                                string desc = ServerDtcList.FirstOrDefault(x => x.ecu_name == ecu).dtc_list.FirstOrDefault(x => x.code == dtcListModel.code)?.description;
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

                                DtcList.Add(DtcModel);
                            }

                            #region Old Code
                            //var Codes = DtcList.FirstOrDefault(x => x.code == dtcListModel.code)?.code;
                            //if (Codes == null)
                            //{
                            //    if (MaskResult != null && MaskiSActive_ids != null)
                            //    {
                            //        var Vehical_model = MaskiSActive_ids.FirstOrDefault(x => x.vehicle_model == App.model_id);
                            //        if (Vehical_model != null)
                            //        {
                            //            foreach (var DtcCodeitem in MaskiSActive_ids.FirstOrDefault().dtc_code)
                            //            {
                            //                if (DtcCodeitem != dtcListModel.code)
                            //                {
                            //                    dtcListModel.status = read_dtc.dtcs[i, 1].ToString();

                            //                    string desc = ServerDtcList.FirstOrDefault(x => x.ecu_name == ecu).dtc_list.FirstOrDefault(x => x.code == dtcListModel.code)?.description;
                            //                    if (desc != null)
                            //                    {
                            //                        dtcListModel.description = desc;
                            //                    }
                            //                    else
                            //                    {
                            //                        dtcListModel.description = "Description not found";
                            //                    }

                            //                    DtcListModel DtcModel = new DtcListModel
                            //                    {
                            //                        code = dtcListModel.code,
                            //                        status = dtcListModel.status,
                            //                        description = dtcListModel.description
                            //                    };

                            //                    PDR.Add(new PostDtcRecord { value = dtcListModel.code, status = dtcListModel.status });

                            //                    DtcList.Add(DtcModel);
                            //                    break;
                            //                }
                            //            }
                            //        }
                            //        else
                            //        {
                            //            dtcListModel.status = read_dtc.dtcs[i, 1].ToString();

                            //            string desc = ServerDtcList.FirstOrDefault(x => x.ecu_name == ecu).dtc_list.FirstOrDefault(x => x.code == dtcListModel.code)?.description;
                            //            if (desc != null)
                            //            {
                            //                dtcListModel.description = desc;
                            //            }
                            //            else
                            //            {
                            //                dtcListModel.description = "Description not found";
                            //            }

                            //            DtcListModel DtcModel = new DtcListModel
                            //            {
                            //                code = dtcListModel.code,
                            //                status = dtcListModel.status,
                            //                description = dtcListModel.description
                            //            };

                            //            PDR.Add(new PostDtcRecord { value = dtcListModel.code, status = dtcListModel.status });

                            //            DtcList.Add(DtcModel);
                            //        }
                            //    }
                            //    else
                            //    {
                            //        dtcListModel.status = read_dtc.dtcs[i, 1].ToString();

                            //        string desc = ServerDtcList.FirstOrDefault(x => x.ecu_name == ecu).dtc_list.FirstOrDefault(x => x.code == dtcListModel.code)?.description;
                            //        if (desc != null)
                            //        {
                            //            dtcListModel.description = desc;
                            //        }
                            //        else
                            //        {
                            //            dtcListModel.description = "Description not found";
                            //        }

                            //        DtcListModel DtcModel = new DtcListModel
                            //        {
                            //            code = dtcListModel.code,
                            //            status = dtcListModel.status,
                            //            description = dtcListModel.description
                            //        };

                            //        PDR.Add(new PostDtcRecord { value = dtcListModel.code, status = dtcListModel.status });

                            //        DtcList.Add(DtcModel);
                            //    }
                            //    //DtcList.Add(dtcListModel);
                            //}
                            #endregion
                        }
                        DTC_Error = "NO_ERROR";
                        var isReachable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
                        if (isReachable)
                            services.DtcRecord(PDR, App.JwtToken, App.SessionId);
                    }
                    else
                    {
                        DTC_Error = read_dtc.status;
                    }
                }

                return DtcList;
            }
            catch (Exception ex)
            {
                DtcList = new List<DtcListModel>();
                return DtcList;
            }
        }

    }
}
