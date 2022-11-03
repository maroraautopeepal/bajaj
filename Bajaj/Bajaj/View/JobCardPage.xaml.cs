using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Popup;
using Bajaj.Services;
using Bajaj.View.PopupPages;
using Bajaj.ViewModel;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using System.Globalization;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JobCardPage : DisplayAlertPage
    {
        ApiServices services;
        JobcardViewModel viewModel;
        string mac_id = string.Empty;
        public int Value = 0;
        List<int> IVNDTC = new List<int>();
        List<int> IVNPID = new List<int>();
        //DongleViewModel dongleView;
        public JobCardPage(string mac_id)
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new JobcardViewModel();
                //ThemeManager.ChangeTheme("GreenTheme");
                App.is_login = false;
                //dongleView = new DongleViewModel();
                //dongleView.GetDongleList();
                services = new ApiServices();
                this.mac_id = mac_id;
            }
            catch (Exception ex)
            {
            }
        }
        string UserID = string.Empty;
        public bool NotificationTimer = true;
        INavigation navigation;
        protected async override void OnAppearing()
        {
            try
            {
                
                var data = DependencyService.Get<ISaveLocalData>().GetData("selctedOemModel");
                if (data != null)
                {
                    App.selectedOem = JsonConvert.DeserializeObject<AllOemModel>(data);
                    var color = ThemeManager.GetColorFromHexValue(App.selectedOem.color?.ToString());
                    App.Current.Resources["theme_color"] = color;
                }
                base.OnAppearing();

                string Role = App.Current.Properties["MasterLoginUserRoleBY"].ToString();
                if (Role == "expert")
                {
                    if (App.Notification)
                    {
                        //MessagingCenter.Send<JobCardPage, string>(this, "Notification", "Expert");
                        App.Notification = false;
                    }

                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            App.IsExpert = true;
                            BellIconIsVisible.IsVisible = true;
                            break;
                        case Device.UWP:
                            break;
                        default:
                            break;
                    }
                    //NotificationCount.Text = "+0";

                    #region Old Code
                    //var UID = App.Current.Properties["LoginUserId"].ToString();
                    //if (UID != null)
                    //{
                    //    UserID = Convert.ToString(UID);
                    //    Task.Run(() =>
                    //     {
                    //         Device.StartTimer(new TimeSpan(0, 0, 05), () =>
                    //         {
                    //             do something every 5 seconds
                    //             Device.BeginInvokeOnMainThread(() =>
                    //             {
                    //                 var ExpertNotificationCount = services.GetExpertRequestList(UserID);
                    //                 if (ExpertNotificationCount != null)
                    //                 {
                    //                     if (ExpertNotificationCount.Result.results.Count() != 0)
                    //                     {
                    //                         NotificationCount.Text = "+" + Convert.ToString(ExpertNotificationCount.Result.results.Count);
                    //                     }
                    //                     else
                    //                     {
                    //                         NotificationCount.Text = "+0";
                    //                     }
                    //                 }
                    //                 else
                    //                 {
                    //                     NotificationCount.Text = "+0";
                    //                 }
                    //             });
                    //             return NotificationTimer; // runs again, or false to stop
                    //         });
                    //     }).ConfigureAwait(false);
                    //}
                    #endregion
                }
                else
                {
                    App.IsExpert = false;
                    BellIconIsVisible.IsVisible = false;
                }

            }
            catch (Exception ex)
            {

            }
        }
        protected override void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
                //var Role = App.Current.Properties["MasterLoginUserRoleBY"].ToString();
                //if (Role == "expert")
                //{
                //    NotificationTimer = false;
                //}
            }
            catch (Exception ex)
            {

            }
        }


        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        private void JobCartItem_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            CalcKeyFromSeedL5();
            Device.BeginInvokeOnMainThread(async () =>
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    try
                    {
                        App.SessionId = "";
                        App.SelectedModel = "";
                        App.model_id = 0;
                        App.sub_model_id = 0;


                        await Task.Delay(100);
                        var item = e.Item as JobCardListModel;
                        App.SessionId = item.id;
                        App.SelectedModel = item.vehicle_model;
                        App.notify_vehicle_model = $"{item.vehicle_model}-{item.sub_model}-{item.model_year}";
                        App.notify_workshop_add = $"{item.city}, {item.state}";
                        App.notify_jobcard_id = item.jobcard_name;
                        App.notify_workshop_name = item.workshop;
                        App.model_id = item.model_id;
                        App.sub_model_id = item.sub_model_id;
                        //
                        //
                        var model_detail = await services.get_all_models(App.JwtToken, 0);
                        //

                        //var IVN_Dtc = new List<IVN_Result>();
                        //var Dtc = new List<DtcResults>();

                        //var IVN_PID = new List<PIDResult>();
                        //var pid = new List<Results>();

                        //foreach (var item1 in model_detail.results)
                        //{
                        //    if (item1.sub_models.Count() != 0 && item1.sub_models.FirstOrDefault().ecus != null)
                        //    {
                        //        foreach (var item2 in item1.sub_models.FirstOrDefault().ecus)
                        //        {
                        //            //if(item2.ivn_dtc_datasets.FirstOrDefault().code)
                        //            if (item2.read_dtc_fn_index.value == "IVN")
                        //            {
                        //                if (item2.datasets.Count != 0 && item2.pid_datasets.Count != 0)
                        //                {
                        //                    // IVN DTC


                        //                    var dtc_dataset = IVNDTC.FirstOrDefault(x => x == item2.datasets.FirstOrDefault().id);

                        //                    if (dtc_dataset == null || dtc_dataset == 0)
                        //                    {
                        //                        IVNDTC.Add(item2.datasets.FirstOrDefault().id);
                        //                    }

                        //                    //IVN PID
                        //                    //Change done for tork
                        //                    //var pid_dataset = IVNPID.FirstOrDefault(x => x == item2.pid_datasets.FirstOrDefault().id);
                        //                    var pid_dataset = IVNPID.FirstOrDefault(x => x == item2.ivn_pid_datasets.FirstOrDefault().id);

                        //                    if (pid_dataset == null || pid_dataset == 0)
                        //                    {
                        //                        IVNPID.Add(item2.ivn_pid_datasets.FirstOrDefault().id);
                        //                    }

                        //                    ///// IVN DTC
                        //                    //var IVN_DTCvalue = item2.datasets.FirstOrDefault();
                        //                    //var IVN_DTCResult = await services.get_ivn_dtc(App.JwtToken, IVN_DTCvalue.id);
                        //                    //if (IVN_DTCResult != null)
                        //                    //{
                        //                    //    IVN_Dtc.Add(new IVN_Result { id = IVN_DTCResult.FirstOrDefault().id, code = IVN_DTCResult.FirstOrDefault().code, description = IVN_DTCResult.FirstOrDefault().description, frame_datasets = IVN_DTCResult.FirstOrDefault().frame_datasets });
                        //                    //}

                        //                    ////IVN PID
                        //                    //var IVN_PIDvalue = item2.pid_datasets.FirstOrDefault();
                        //                    //var IVN_PIDResult = await services.get_ivn_pid(App.JwtToken, IVN_PIDvalue.id);
                        //                    //if (IVN_PIDResult != null)
                        //                    //{
                        //                    //    IVN_PID.Add(new PIDResult { id = IVN_PIDResult.FirstOrDefault().id, code = IVN_PIDResult.FirstOrDefault().code, description = IVN_PIDResult.FirstOrDefault().description, frame_datasets = IVN_PIDResult.FirstOrDefault().frame_datasets });
                        //                    //}
                        //                }
                        //            }
                        //            else
                        //            {
                        //                if (item2.dtc_datasets != null && item2.dtc_datasets != null)
                        //                {
                        //                    if (item2.dtc_datasets.Count != 0 && item2.pid_pid_datasets.Count != 0)
                        //                    {
                        //                        /// DTC
                        //                        var DTCvalue = item2.dtc_datasets.FirstOrDefault();
                        //                        var DTCResult = await services.get_dtc(App.JwtToken, DTCvalue.id);
                        //                        if (DTCResult != null)
                        //                        {
                        //                            Dtc.Add(new DtcResults { id = DTCResult.FirstOrDefault().id, code = DTCResult.FirstOrDefault().code, description = DTCResult.FirstOrDefault().description, dtc_code = DTCResult.FirstOrDefault().dtc_code });
                        //                        }

                        //                        //PID Records
                        //                        var PIDvalue = item2.pid_pid_datasets.FirstOrDefault();
                        //                        var PIDResult = await services.get_pid(App.JwtToken, PIDvalue.id);
                        //                        if (PIDResult.Count != 0)
                        //                        {
                        //                            pid.Add(new Results { id = PIDResult.FirstOrDefault().id, code = PIDResult.FirstOrDefault().code, description = PIDResult.FirstOrDefault().description, codes = PIDResult.FirstOrDefault().codes });
                        //                        }
                        //                    }
                        //                }

                        //            }
                        //        }
                        //    }
                        //}

                        //if (IVNDTC != null)
                        //{
                        //    try
                        //    {
                        //        foreach (var item1 in IVNDTC)
                        //        {
                        //            var IVN_DTCResult = await services.get_ivn_dtc(App.JwtToken, item1);
                        //            if (IVN_DTCResult != null)
                        //            {
                        //                IVN_Dtc.Add(new IVN_Result { id = IVN_DTCResult.FirstOrDefault().id, code = IVN_DTCResult.FirstOrDefault().code, description = IVN_DTCResult.FirstOrDefault().description, frame_datasets = IVN_DTCResult.FirstOrDefault().frame_datasets });
                        //            }
                        //        }

                        //        var IVN_DtcJson = JsonConvert.SerializeObject(IVN_Dtc);
                        //        await DependencyService.Get<ISaveLocalData>().SaveData("IVN_DtcJson", IVN_DtcJson);
                        //    }
                        //    catch (Exception ex)
                        //    {

                        //    }
                        //}

                        //if (IVNPID != null)
                        //{
                        //    foreach (var item1 in IVNPID)
                        //    {
                        //        var IVN_PIDResult = await services.get_ivn_pid(App.JwtToken, item1);
                        //        if (IVN_PIDResult != null)
                        //        {
                        //            IVN_PID.Add(new PIDResult { id = IVN_PIDResult.FirstOrDefault().id, code = IVN_PIDResult.FirstOrDefault().code, description = IVN_PIDResult.FirstOrDefault().description, frame_datasets = IVN_PIDResult.FirstOrDefault().frame_datasets });
                        //        }
                        //    }

                        //    var IVN_PidJson = JsonConvert.SerializeObject(IVN_PID);
                        //    await DependencyService.Get<ISaveLocalData>().SaveData("IVN_PidJson", IVN_PidJson);
                        //}


                        //if (Dtc != null)
                        //{
                        //    var DtcJson = JsonConvert.SerializeObject(Dtc);
                        //    await DependencyService.Get<ISaveLocalData>().SaveData("dtcjson", DtcJson);
                        //}
                        ///////////// DTC

                        //if (pid.Count > 0)
                        //{
                        //    var PidJson = JsonConvert.SerializeObject(pid);
                        //    await DependencyService.Get<ISaveLocalData>().SaveData("pidjson", PidJson);
                        //}


                        //var DTCMask = await services.GetDTC_mask(App.JwtToken);
                        //var DTCMaskJson = JsonConvert.SerializeObject(DTCMask);
                        //await DependencyService.Get<ISaveLocalData>().SaveData("DTCMaskJsonData", DTCMaskJson);
                        /////////////////////////////////////////

                        /////////// PID



                        /////////////////////////////////////////

                        //if(!string.IsNullOrEmpty(item.sub_model)&& item.sub_model=="NA")
                        //{
                        //    item.model_with_submodel = item.vehicle_model + "-" + item.sub_model;
                        //}
                        //else
                        //{
                        //    item.model_with_submodel = item.vehicle_model;
                        //}
                        //Navigation.PushAsync(new AppFeaturePage(item));
                        //await Navigation.PushAsync(new ConnectionPage(item));
                        SubModel subModel = null;


                        //var model = model_detail.results.FirstOrDefault(x => x.id == App.model_id).sub_models.FirstOrDefault(x => x.id == App.sub_model_id);
                        foreach (var model1 in model_detail.results.ToList())
                        {
                            if (model1.sub_models != null && model1.sub_models.Count > 0)
                            {
                                foreach (var submodel in model1.sub_models.ToList())
                                {
                                    if (submodel.id == App.sub_model_id)
                                    {
                                        subModel = submodel;
                                    }
                                }
                            }
                        }

                        if (subModel.ecu_submodel != null)
                        {
                            if (subModel.ecu_submodel.FirstOrDefault().datasets.Count == 0)
                            {
                                await this.DisplayAlert("Alert! DTC Count 0", "Jobcard \n ( " + item.session_id + " ) \n DTC Count are 0", "OK");
                            }
                            else if (subModel.ecu_submodel.FirstOrDefault().pid_datasets.Count == 0)
                            {
                                await this.DisplayAlert("Alert! PID Count 0", "Jobcard \n ( " + item.session_id + " ) \n PID Count are 0", "OK");
                            }
                            else
                            {
                                

                                StaticData.ecu_info = new List<EcuDataSet>();
                                StaticData.ecu_info.Clear();
                                foreach (var ecu_item in subModel.ecu_submodel)
                                {

                                    var ecuDataNew = await services.GetEcuData(App.JwtToken, ecu_item.ecu);
                                    StaticData.ecu_info.Add(
                                        new EcuDataSet
                                        {
                                            //read_dtc_index = ecu_item.read_dtc_fn_index.value,
                                            read_dtc_index = ecuDataNew.results.FirstOrDefault().read_dtc_fn_index.value,
                                            pid_dataset_id = ecu_item.pid_datasets[0].id,
                                            //ivn_pid_dataset_id = ecu_item.ivn_pid_datasets[0].id,
                                            //ivn_dtc_dataset_id = ecu_item.ivn_dtc_datasets[0].id,
                                            clear_dtc_index = ecuDataNew.results.FirstOrDefault().clear_dtc_fn_index.value,
                                            dtc_dataset_id = ecu_item.datasets[0].id,
                                            ecu_name = ecuDataNew.results.FirstOrDefault().name,
                                            seed_key_index = ecuDataNew.results.FirstOrDefault().seedkeyalgo_fn_index,
                                            write_pid_index = ecuDataNew.results.FirstOrDefault().write_data_fn_index.value,
                                            tx_header = ecuDataNew.results.FirstOrDefault().tx_header,
                                            rx_header = ecuDataNew.results.FirstOrDefault().rx_header,
                                            protocol = ecuDataNew.results.FirstOrDefault().protocol,
                                            ecu_ID = ecu_item.ecu,
                                            ior_test_fn_index = ecuDataNew.results.FirstOrDefault().ior_test_fn_index.value,
                                        });
                                }
                                
                                await Navigation.PushAsync(new ConnectionPage());
                                //DependencyService.Get<INotification>().CreateNotification("SPTutorials", StaticData.ecu_info.FirstOrDefault().ecu_name);
                            }
                        }
                        else
                        {
                            await this.DisplayAlert("Alert! ECU Count 0", "Jobcard \n ( " + item.session_id + " ) \n ECU Count are 0", "OK");
                        }

                        App.ecu_list = new List<string>();
                        App.ecu_list.Clear();
                        //Remember to comment out
                        //foreach (var ecu in subModel.ecus)
                        //{
                        //    App.ecu_list.Add(ecu.name);
                        //}
                        

                    }
                    catch (Exception ex)
                    {
                    }
                }
            });
        }

        private void AddNewJobCardClick(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    //await services.ExperNotfy();
                    await Navigation.PushAsync(new CreateJobCardPage("", mac_id));
                }
            });
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

        private async void ExpertNotiFicationCountTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new RemoteRequestPage());

                //if (NotificationCount.Text != "+0" && NotificationCount.Text != "" && NotificationCount.Text != null)
                //{
                //    await Navigation.PushAsync(new RemoteRequestPage());
                //}
                //else
                //{
                //    await this.DisplayAlert("Alert! Remote Request.", "Remote Request Count are 0 !!!", "OK");
                //}
            }
            catch (Exception)
            {

            }
        }

        public byte[] CalcKeyFromSeedL5()
        {
            try
            {
                UInt32 level5_Const = 0x841B8CDB;

                var seedString = "4EB36010";
                UInt32 seed = Convert.ToUInt32(seedString, 16);
                UInt32 key, reminder;
                byte divisor_byte_index, reminder_byte_index, xor_dividend_index, xor_seed_index = 0;
                UInt32 divisor = 0;

                if (seed == 0)
                {
                    key = 0;
                    return null;
                }
                else
                {
                    //divisor_byte_index = (byte)((seed & 0x00000300) >> 8); // extract bit 9 + 8
                    divisor_byte_index = (byte)((seed & 0x00000300) >> 8);

                    //reminder_byte_index = (byte)((seed & 0x00000300) >> 8); // extract bit 9 + 8
                    reminder_byte_index = (byte)((seed & 0x00000300) >> 8);

                    //xor_dividend_index = (byte)((seed & 0x00000018) >> 3); // extract bit 4 + 3
                    xor_dividend_index = (byte)((seed & 0x00000018) >> 3);

                    //xor_seed_index = (byte)((seed & 0x0C000000) >> 26); // extract bit 27 + 26
                    xor_seed_index = (byte)((seed & 0x0C000000) >> 26);

                    if (divisor_byte_index == 0x00)
                    {
                        divisor = (seed >> 8);
                    }
                    else if (divisor_byte_index == 0x01)
                    {
                        var tem1 = ((seed >> 8) & 0x00FFFF00);
                        var tem2 = seed & 0x00000FF;
                        divisor = (tem1 | tem2);
                    }
                    else if (divisor_byte_index == 0x02)
                    {
                        var tem1 = ((seed >> 8) & 0xFF0000);
                        var tem2 = seed & 0x0000FFFF;
                        divisor = (tem1 | tem2);
                    }
                    else
                    {
                        divisor = seed & 0x00FFFFFF;
                    }

                    reminder = (level5_Const % divisor);

                    var temp1 = (byte)((level5_Const >> (8 * xor_dividend_index)) & 0x000000FF);

                    var temp2 = (byte)((seed >> (8 * xor_seed_index)) & 0x000000FF);

                    var xorbyte = (byte)(temp1 ^ temp2);

                    if (reminder_byte_index == 0x00)
                    {
                        reminder = reminder << 8;
                        key = reminder | xorbyte;
                    }
                    else if (reminder_byte_index == 0x01)
                    {
                        var t1 = (reminder & 0x00FFFF00) << 8;
                        var t2 = (reminder & 0x000000FF);
                        reminder = t1 | t2;
                        key = reminder | (uint)(xorbyte << 8);
                    }
                    else if (reminder_byte_index == 0x02)
                    {
                        var t1 = reminder & 0x0000FFFF;
                        var t2 = (reminder & 0xFF0000) << 8;
                        reminder = t1 | t2;
                        key = reminder | (uint)(xorbyte << 16);
                    }
                    else
                    {
                        //Do nothing for reminder
                        key = reminder | (uint)(xorbyte << 24);
                    }

                    return BitConverter.GetBytes(key);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public byte[] CalcKeyFromSeedL3()
        {
            UInt32 level3_Const = 0x2EDC6F45;

            var seedString = "E8B60120";
            UInt32 seed = Convert.ToUInt32(seedString, 16);
            UInt32 key, reminder;
            byte divisor_byte_index, reminder_byte_index, xor_dividend_index, xor_seed_index = 0;
            UInt32 divisor = 0;

            if (seed == 0)
            {
                key = 0;
                return null;
            }
            else
            {
                //divisor_byte_index = ; // extract bit 23 + 22
                divisor_byte_index = (byte)((seed & 0x00C00000) >> 22);

                reminder_byte_index = (byte)(seed & 0x00000003); // extract bit 1 + 0

                xor_dividend_index = (byte)((seed & 0x0000C000) >> 14);

                xor_seed_index = (byte)((seed & 0xC0000000) >> 30);

                if (divisor_byte_index == 0x00)
                {
                    divisor = (seed >> 8);
                }
                else if (divisor_byte_index == 0x01)
                {
                    var tem1 = ((seed >> 8) & 0x00FFFF00);
                    var tem2 = seed & 0x00000FF;
                    divisor = (tem1 | tem2);
                }
                else if (divisor_byte_index == 0x02)
                {
                    var tem1 = ((seed >> 8) & 0xFF0000);
                    var tem2 = seed & 0x0000FFFF;
                    divisor = (tem1 | tem2);
                }
                else
                {
                    divisor = seed & 0x00FFFFFF;
                }

                //reminder = mod(level3_Const, divisor);
                reminder = ((level3_Const % divisor) & 0x00ffffff);

                var temp1 = (byte)((level3_Const >> (8 * xor_dividend_index)) & 0x000000FF);

                var temp2 = (byte)((seed >> (8 * xor_seed_index)) & 0x000000FF);

                var xorbyte = (byte)(temp1 ^ temp2);

                if (reminder_byte_index == 0x00)
                {
                    reminder = reminder << 8;
                    key = reminder | xorbyte;
                }
                else if (reminder_byte_index == 0x01)
                {
                    var t1 = (reminder & 0x00FFFF00) << 8;
                    var t2 = (reminder & 0x000000FF);
                    reminder = t1 | t2;
                    key = reminder | (uint)(xorbyte << 8);
                }
                else if (reminder_byte_index == 0x02)
                {
                    var t1 = reminder & 0x0000FFFF;
                    var t2 = (reminder & 0xFF0000) << 8;
                    reminder = t1 | t2;
                    key = reminder | (uint)(xorbyte << 16);
                }
                else
                {
                    //Do nothing for reminder
                    key = reminder | (uint)(xorbyte << 24);
                }

                var keyByte =  BitConverter.GetBytes(key);

                //int n = keyByte.Length;
                //for (int i = 0; i < n / 2; i++)
                //{
                //    swap(keyByte, i, n - i - 1);
                //}


                //keyByte = Enumerable.Reverse(keyByte).ToArray();
                //Array.Reverse(keyByte, 0, keyByte.Length);
                return keyByte;
            }
        }


    }
}