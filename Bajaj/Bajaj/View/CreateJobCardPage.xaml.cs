using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.View.PopupPages;
using Bajaj.ViewModel;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateJobCardPage : DisplayAlertPage
    {
        ApiServices services;
        public string SelectedModelType;
        public string ModelId = string.Empty;
        public string JobCardSessionId = string.Empty;
        public string Source = string.Empty;
        //public string MacId = string.Empty;
        ModelResult modelResult;// = new ModelResult();
        string model_year = string.Empty;
        string SelectedModel = string.Empty;
        string mac_id = string.Empty;
        string model_id = string.Empty;
        JobcardViewModel viewModel;
        public List<JobCardListModel> Off_JBList;
        public CreateJobCardPage(string ModelType, string mac_id)
        {
            InitializeComponent();

            services = new ApiServices();

            //Random generator = new Random();
            //int randomumber = generator.Next(1, 999999999);
            //randomumber


            string str = string.Empty;

            Task.Run(async () =>
            {
                //var isReachable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
                //if (isReachable) { }
                //else
                //{
                //    show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                //}

                modelResult = new ModelResult();

                SelectedModelType = ModelType;
                this.mac_id = mac_id;
                Off_JBList = new List<JobCardListModel>();
                OfflineJobCard = new List<JobCardModel>();
                SubModelClass = new List<SubModelClass>();
                CreatedByCalss = new List<CreatedBy>();
                JobCardSession = new List<JobCardSession>();
            });

            MessagingCenter.Subscribe<PopupPages.ModelPopupPage, ModelResult>(this, "select_vehicle_model_year", async (sender, arg) =>
            {
                txt_Model.Text = arg.name;
                App.model_id = arg.id;
                txt_Model.TextColor = (Color)Application.Current.Resources["text_color"];
                modelResult = arg;
            });

            MessagingCenter.Subscribe<PopupPages.SubModelPopupPage, String>(this, "selected_vehicle_model", async (sender, arg) =>
            {
                //txt_sub_model.Text = arg.name;
                txt_sub_model.Text = arg;
                //App.sub_model_id = arg.id;
                txt_sub_model.TextColor = (Color)Application.Current.Resources["text_color"];
                //model_year = arg.model_year;
                SelectedModel = arg;
            });

            MessagingCenter.Subscribe<PopupPages.ModelYearPopupPage, SubModel>(this, "selected_vehicle_submodel", async (sender, arg) =>
            {
                txt_model_year.Text = arg.model_year;
                App.sub_model_id = arg.id;
                txt_model_year.TextColor = (Color)Application.Current.Resources["text_color"];
            });

        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            txtEngineNo.Text = "NA";
            
            GetJobcardId();
            //var Value = await services.NewJobCardNumber();
            //if (Value != null)
            //{
            //    txt_ecu.Text = Value;
            //}
        }


        public async void GetJobcardId()
        {
            txt_ecu.Text = string.Empty;
            var result = await services.GetJobCardNumber();
            if (result == null)
            {
                await DisplayAlert("", "Jobcard number not created", "Ok");
                await Navigation.PopAsync();
                return;
            }

            if (!string.IsNullOrEmpty(result.error) && result.error.Contains("Jobcard number not created"))
            {
                await DisplayAlert("", "Jobcard number not created", "Ok");
                await Navigation.PopAsync();
                return;
            }

            if (!result.success)
            {
                await DisplayAlert("", "Jobcard number not created", "Ok");
                await Navigation.PopAsync();
                return;
            }

            txt_ecu.Text = result.name;
        }

        private void txtRegNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
                && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
                !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
            {
                BtnSubmit.IsEnabled = true;
            }
        }

        private async void ClickOnModel(object sender, EventArgs e)
        {
            try
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    //box.IsVisible = true;
                    PopupNavigation.PushAsync(new ModelListPopupPage(SelectedModelType));
                    //Working = false;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //Working = false;
                ///box.IsVisible = false;
            }

        }

        //private void CheckJobCardTapped(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Task.Run(async () =>
        //        {
        //            // Run code here
        //            //Working = true;
        //            //WorkingStatusText = "Loading...";
        //            await Task.Delay(1000);
        //            Device.BeginInvokeOnMainThread(() =>
        //            {
        //                if (string.IsNullOrEmpty(txt_Model.Text))
        //                {
        //                    DisplayAlert("Alert", "Please Enter UDAA /EOS Jobcard No.", "Ok");
        //                    return;
        //                }
        //                var result = services.CheckJobCard(App.JwtToken, txt_Model.Text);

        //                if (result.Result.d != null && !string.IsNullOrEmpty(result.Result.d.Vhvin))
        //                {
        //                    txtRegNo.Text = result.Result.d.RegNo;
        //                    txtChassisNo.Text = result.Result.d.Vhvin;
        //                    txtEngineNo.Text = result.Result.d.EngNo;
        //                    txt_Model.Text = result.Result.d.VehMod;
        //                    txtKMSCoveredNo.Text = result.Result.d.KmsCover;
        //                    Source = "udaan";
        //                    //JobCardSessionId = result.Result.d.id
        //                }
        //                else
        //                {
        //                    var res = services.CheckJobCardSecondAPI(App.JwtToken, txt_Model.Text);
        //                    if (res.Result != null && res.Result.Count > 0)
        //                    {
        //                        txtRegNo.Text = res.Result[0].VehicleRegisterNumber;
        //                        txtChassisNo.Text = res.Result[0].ChassisNo;
        //                        //txtEngineNo.Text = res.Result[0].;
        //                        txt_Model.Text = res.Result[0].ModelNumber;
        //                        txtKMSCoveredNo.Text = Convert.ToString(res.Result[0].KmCovered);
        //                        txtRegNo.Text = res.Result[0].VehicleRegisterNumber;
        //                        Source = "eos";
        //                    }
        //                    else
        //                    {
        //                        Source = "gen";
        //                        DisplayAlert("Alert", "Invalid Job Card Number.", "Ok");
        //                        txtRegNo.Text = string.Empty;
        //                        txtChassisNo.Text = string.Empty;
        //                        txtEngineNo.Text = string.Empty;
        //                        txt_Model.Text = string.Empty;
        //                        txtKMSCoveredNo.Text = string.Empty;
        //                        txtRegNo.Text = string.Empty;
        //                        txtComplaints.Text = string.Empty;
        //                    }
        //                }
        //                if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
        //                && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
        //                !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
        //                {
        //                    BtnSubmit.IsEnabled = true;
        //                }
        //                //Working = false;
        //            });
        //        });         
        //    }
        //    catch (Exception ex)
        //    {
        //        //Working = false;
        //    }
        //    finally
        //    {
        //        //Working = false;
        //    }
        //}

        private void txtChassisNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
                && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
                !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
            {
                BtnSubmit.IsEnabled = true;
            }
        }

        private void txtEngineNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
                && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
                !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
            {
                BtnSubmit.IsEnabled = true;
            }
        }

        private void txt_Model_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
                && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
                !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
            {
                BtnSubmit.IsEnabled = true;
            }
        }

        private void txtKMSCoveredNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
                && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
                !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
            {
                BtnSubmit.IsEnabled = true;
            }
        }

        private void txtComplaints_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
                && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
                !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
            {
                BtnSubmit.IsEnabled = true;
            }
        }
        //string id = string.Empty;
        //public string Id
        //{
        //    get
        //    {
        //        if (!string.IsNullOrWhiteSpace(id))
        //            return id;

        //        id = Android.OS.Build.Serial;
        //        if (string.IsNullOrWhiteSpace(id) || id == Build.Unknown || id == "0")
        //        {
        //            try
        //            {
        //                var context = Android.App.Application.Context;
        //                id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
        //            }
        //            catch (Exception ex)
        //            {
        //                Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
        //            }
        //        }

        //        return id;
        //    }
        //}

        public List<JobCardModel> OfflineJobCard;
        public List<SubModelClass> SubModelClass;
        public List<CreatedBy> CreatedByCalss;
        public List<JobCardSession> JobCardSession;
        private async void BtnSubmit_Clicked(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                await Task.Delay(100);
                try
                {
                    string model_with_submodel = string.Empty;
                    if (txt_Model.Text != "Select Model" && App.model_id != 0 && txt_sub_model.Text != "Select Sub Model"
                        && App.sub_model_id != 0 && txt_model_year.Text != "Select Model Year"
                            && txtRegNo.Text != null && txtRegNo.Text != "" && txtChassisNo.Text != null && txtChassisNo.Text != "" && txtEngineNo.Text != null && txtEngineNo.Text != "" && txtKMSCoveredNo.Text != null && txtKMSCoveredNo.Text != "")
                    {
                        if (txtChassisNo.Text.Length == 6)
                        {
                            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                            if (isReachable)
                            {
                                var AndroidID = Id;
                                string device_mac_id = string.Empty;
                                switch (Device.RuntimePlatform)
                                {
                                    case Device.Android:
                                        device_mac_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
                                        break;
                                    case Device.UWP:
                                        device_mac_id = "1234567890";
                                        break;
                                    default:
                                        break;
                                }


                                SendJobcardData sendJobcardData = new SendJobcardData
                                {
                                    chasis_id = txtChassisNo.Text,
                                    complaints = txtComplaints.Text,
                                    date = DateTime.Now.ToString(),
                                    engine_no = txtEngineNo.Text,
                                    km_covered = txtKMSCoveredNo.Text,
                                    vehicle_model_id = Convert.ToString(App.sub_model_id),
                                    model = modelResult.name,
                                    VehModDes = "test",
                                    status = "new",
                                    job_card_name = txt_ecu.Text,
                                    submodel = "Na",
                                    vehicle_segment = "Na",
                                    job_card_status = "new",
                                    device_mac_id = device_mac_id,
                                    fert_code = "1233",
                                    registration_no = txtRegNo.Text,
                                    session_type = "regular",
                                    source = "gen",
                                };
                                var CreateJobCard = await services.SendJobCard(sendJobcardData);

                                if (CreateJobCard != null)
                                {
                                    if (CreateJobCard.CreateJobcard != null)
                                    {
                                        var item = CreateJobCard.CreateJobcard;
                                        JobCardListModel jobCardListModel = new JobCardListModel();
                                        var date = item.job_card_session[0].start_date?.ToString("dd-MM-yyyy");
                                        jobCardListModel = null;
                                        if (!string.IsNullOrEmpty(item.job_card_session[0].vehicle_model.name) && item.job_card_session[0].vehicle_model.name != "NA")
                                        {
                                            model_with_submodel = item.job_card_session[0].vehicle_model.parent.name + "-" + item.job_card_session[0].vehicle_model.name
                                                                    + " " + item.job_card_session[0].vehicle_model.model_year;
                                        }
                                        else
                                        {
                                            model_with_submodel = item.job_card_session[0].vehicle_model.parent.name;
                                        }
                                        if (item.job_card_session[0].status != "closed")
                                        {
                                            jobCardListModel = new JobCardListModel
                                            {
                                                jobcard_name = item.job_card_name,
                                                workshop = item.created_by.us_user.workshop.name,
                                                chasis_id = item.chasis_id,
                                                registration_no = item.registration_no,
                                                complaints = item.complaints,
                                                fert_code = item.fert_code,
                                                jobcard_status = item.status,
                                                vehicle_segment = item.vehicle_segment,
                                                modified = item.modified,
                                                km_covered = item.km_covered,
                                                session_id = item.job_card_session[0].session_id,
                                                status = item.job_card_session[0].status,
                                                session_type = item.job_card_session[0].session_type,
                                                source = item.job_card_session[0].source,
                                                //source = "gen",
                                                end_date = item.job_card_session[0].end_date,
                                                start_date = item.job_card_session[0].start_date,
                                                job_card = item.job_card_session[0].job_card,
                                                id = item.job_card_session[0].id,
                                                vehicle_model = item.job_card_session[0].vehicle_model.parent.name,
                                                model_year = item.job_card_session[0].vehicle_model.model_year,
                                                sub_model = item.job_card_session[0].vehicle_model.name,
                                                show_start_date = date,
                                                model_id = 0,
                                                sub_model_id = item.job_card_session[0].vehicle_model.id,
                                                model_with_submodel = model_with_submodel,
                                                city = item.created_by.us_user.workshop.city,
                                                state = item.created_by.us_user.workshop.state,
                                            };
                                        }

                                        App.notify_jobcard_id = jobCardListModel.jobcard_name;
                                        App.notify_workshop_name = jobCardListModel.workshop;
                                        App.notify_vehicle_model = $"{jobCardListModel.vehicle_model}-{jobCardListModel.sub_model}-{jobCardListModel.model_year}";
                                        App.notify_workshop_add = $"{jobCardListModel.city}, {jobCardListModel.state}";
                                        App.SessionId = "";
                                        App.SessionId = item.job_card_session[0].id;

                                        var model_detail = await Task.Run(() =>
                                        {
                                            return services.get_all_models(App.JwtToken, 0);
                                        });

                                        //var model_detail = await services.get_all_models(App.JwtToken, 0);

                                        //var model = model_detail.results.FirstOrDefault(x => x.id == App.model_id).sub_models.FirstOrDefault(x => x.id == App.sub_model_id);
                                        SubModel subModel = null;
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

                                        if (subModel.ecu_submodel.Count != 0)
                                        {
                                            StaticData.ecu_info.Clear();
                                            foreach (var ecu_item in subModel.ecu_submodel)
                                            {
                                                if (ecu_item.datasets.Count == 0)
                                                {
                                                    //await this.DisplayAlert("Alert! DTC Count 0", "ECU DTC Count are 0", "OK");

                                                    var errorpage = new Popup.DisplayAlertPage("Alert! DTC Count 0", "ECU DTC Count are 0", "OK");
                                                    await PopupNavigation.Instance.PushAsync(errorpage);
                                                }
                                                else if (ecu_item.pid_datasets.Count == 0)
                                                {
                                                    //await this.DisplayAlert("Alert! PID Count 0", "ECU PID Count are 0", "OK");

                                                    var errorpage = new Popup.DisplayAlertPage("Alert! PID Count 0", "ECU PID Count are 0", "OK");
                                                    await PopupNavigation.Instance.PushAsync(errorpage);
                                                }

                                                var ecuDataNew = await services.GetEcuData(App.JwtToken, ecu_item.ecu);

                                                StaticData.ecu_info.Add(
                                                    new EcuDataSet
                                                    {
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
                                                        ecu_ID = ecu_item.id,
                                                        ior_test_fn_index = ecuDataNew.results.FirstOrDefault().ior_test_fn_index.value,
                                                    });
                                            }
                                            if (StaticData.ecu_info.FirstOrDefault().pid_dataset_id != 0 && StaticData.ecu_info.FirstOrDefault().dtc_dataset_id != 0)
                                            {
                                                //await this.DisplayAlert("Alert", "Jobcard successfully created", "OK");
                                                var errorpage = new Popup.DisplayAlertPage("Alert", "Jobcard successfully created", "OK");
                                                await PopupNavigation.Instance.PushAsync(errorpage);
                                                await Navigation.PushAsync(new ConnectionPage());
                                            }
                                            else
                                            {
                                                if (StaticData.ecu_info.FirstOrDefault().pid_dataset_id == 0)
                                                {
                                                    //await this.DisplayAlert("Alert", "Jobcard created successfully, But PID Count are 0", "OK");
                                                    var errorpage = new Popup.DisplayAlertPage("Alert", "Jobcard created successfully, But PID Count are 0", "OK");
                                                    await PopupNavigation.Instance.PushAsync(errorpage);

                                                    var mac_id = "1234567890";
                                                    Application.Current.MainPage = new NavigationPage(new MasterDetailView(mac_id) { Detail = new NavigationPage(new JobCardPage(mac_id)) });
                                                }
                                                else if (StaticData.ecu_info.FirstOrDefault().dtc_dataset_id == 0)
                                                {
                                                    //await this.DisplayAlert("Alert", "Jobcard created successfully, But DTC Count are 0", "OK");

                                                    var errorpage = new Popup.DisplayAlertPage("Alert", "Jobcard created successfully, But DTC Count are 0", "OK");
                                                    await PopupNavigation.Instance.PushAsync(errorpage);

                                                    var mac_id = "1234567890";
                                                    Application.Current.MainPage = new NavigationPage(new MasterDetailView(mac_id) { Detail = new NavigationPage(new JobCardPage(mac_id)) });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //await this.DisplayAlert("Alert! ECU Count 0", "Jobcard created successfully, But ECU Count are 0", "OK");

                                            var errorpage = new Popup.DisplayAlertPage("Alert! ECU Count 0", "Jobcard created successfully, But ECU Count are 0", "OK");
                                            await PopupNavigation.Instance.PushAsync(errorpage);

                                            var mac_id = "1234567890";
                                            Application.Current.MainPage = new NavigationPage(new MasterDetailView(mac_id) { Detail = new NavigationPage(new JobCardPage(mac_id)) });
                                        }
                                    }
                                    else if (CreateJobCard.SameJobcard != null)
                                    {
                                        show_alert("Alert", "This jobcard number is already exist.", false, true);
                                    }
                                }
                                else
                                {
                                    show_alert("Alert", "Please Check Server API!!, Job Card Not Created", false, true);
                                }
                            }
                            else
                            {
                                show_alert("Alert", "Some Fields are Incomplete, Please Enter All Fields", false, true);
                            }
                        }
                        else
                        {
                            show_alert("Alert", "Enter Chassis number(only last 6 digits)", false, true);
                        }
                    }
                    else
                    {
                        show_alert("Alert", "Some Fields are Incomplete, Please Enter All Fields", false, true);
                    }
                }
                catch (Exception ex)
                {
                    show_alert("ERROR", "Please Contact to Administrator", false, true);
                }

            }
        }

        private async void Search_JobCard_clicked(object sender, EventArgs e)
        {
            //var result = await DisplayAlert("Alert", "There is no job card with this ID. Do you want to create a new Job card ?", "Ok","Cancel");
            show_alert("Alert", "There is no job card with this ID. Do you want to create a new Job card ?", true, false);
            //if (result)
            OkCommand = new Command(() =>
            {
                Working = false;
                frm_model.IsVisible = true;
                frm_sub_model.IsVisible = true;
                frm_model_year.IsVisible = true;
                frm_reg_number.IsVisible = true;
                frm_chassis_no.IsVisible = true;
                frm_engine_no.IsVisible = true;
                frm_kms.IsVisible = true;
                frm_complain.IsVisible = true;
                BtnSubmit.IsEnabled = true;
            });
        }

        

        [Obsolete]
        private void select_model_clicked(object sender, EventArgs e)
        {
            PopupNavigation.PushAsync(new Bajaj.View.PopupPages.ModelPopupPage());
        }

        [Obsolete]
        private void select_sub_model_clicked(object sender, EventArgs e)
        {
            PopupNavigation.PushAsync(new SubModelPopupPage(modelResult.sub_models));
        }

        [Obsolete]
        private void select_model_year_clicked(object sender, EventArgs e)
        {
            PopupNavigation.PushAsync(new ModelYearPopupPage(SelectedModel, modelResult.sub_models));
        }

        private void Create_JobCard_clicked(object sender, EventArgs e)
        {
            
            frm_model.IsVisible = true;
            frm_sub_model.IsVisible = true;
            frm_model_year.IsVisible = true;
            frm_reg_number.IsVisible = true;
            frm_chassis_no.IsVisible = true;
            frm_engine_no.IsVisible = true;
            frm_kms.IsVisible = true;
            frm_complain.IsVisible = true;
            BtnSubmit.IsEnabled = true;
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
    }


    //public partial class CreateJobCardPage : DisplayAlertPage
    //{
    //    ApiServices services;
    //    public string SelectedModelType;
    //    public string ModelId = string.Empty;
    //    public string JobCardSessionId = string.Empty;
    //    public string Source = string.Empty;
    //    //public string MacId = string.Empty;
    //    ModelResult modelResult;// = new ModelResult();
    //    string model_year = string.Empty;
    //    string mac_id = string.Empty;
    //    string model_id = string.Empty;
    //    JobcardViewModel viewModel;
    //    public List<JobCardListModel> Off_JBList;
    //    public CreateJobCardPage(string ModelType, string mac_id)
    //    {
    //        InitializeComponent();

    //        Task.Run(async () =>
    //        {
    //            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
    //            if (isReachable) { }
    //            else
    //            {
    //                services = new ApiServices();
    //                show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
    //            }

    //            modelResult = new ModelResult();
    //            services = new ApiServices();
    //            SelectedModelType = ModelType;
    //            this.mac_id = mac_id;
    //            Off_JBList = new List<JobCardListModel>();
    //            OfflineJobCard = new List<JobCardModel>();
    //            SubModelClass = new List<SubModelClass>();
    //            CreatedByCalss = new List<CreatedBy>();
    //            JobCardSession = new List<JobCardSession>();
    //        });

    //        MessagingCenter.Subscribe<PopupPages.ModelPopupPage, ModelResult>(this, "select_vehicle_model", async (sender, arg) =>
    //        {
    //            txt_Model.Text = arg.name;
    //            App.model_id = arg.id;
    //            txt_Model.TextColor = (Color)Application.Current.Resources["text_color"];
    //            modelResult = arg;
    //        });

    //        MessagingCenter.Subscribe<PopupPages.SubModelPopupPage, SubModel>(this, "selected_vehicle_sub_model", async (sender, arg) =>
    //        {
    //            txt_sub_model.Text = arg.name;
    //            App.sub_model_id = arg.id;
    //            txt_sub_model.TextColor = (Color)Application.Current.Resources["text_color"];
    //            model_year = arg.model_year;
    //        });

    //        MessagingCenter.Subscribe<PopupPages.ModelYearPopupPage, string>(this, "selected_vehicle_model_year", async (sender, arg) =>
    //        {
    //            txt_model_year.Text = arg;
    //            txt_model_year.TextColor = (Color)Application.Current.Resources["text_color"];
    //        });

    //    }
    //    protected async override void OnAppearing()
    //    {
    //        base.OnAppearing();
    //        var Value = await services.NewJobCardNumber();
    //        if (Value != null)
    //        {
    //            txt_ecu.Text = Value;
    //        }
    //    }
    //    private void txtRegNo_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
    //            && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
    //            !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
    //        {
    //            BtnSubmit.IsEnabled = true;
    //        }
    //    }

    //    private async void ClickOnModel(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
    //            {
    //                await Task.Delay(1000);
    //                //box.IsVisible = true;
    //                PopupNavigation.PushAsync(new ModelListPopupPage(SelectedModelType));
    //                //Working = false;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //        finally
    //        {
    //            //Working = false;
    //            ///box.IsVisible = false;
    //        }

    //    }

    //    //private void CheckJobCardTapped(object sender, EventArgs e)
    //    //{
    //    //    try
    //    //    {
    //    //        Task.Run(async () =>
    //    //        {
    //    //            // Run code here
    //    //            //Working = true;
    //    //            //WorkingStatusText = "Loading...";
    //    //            await Task.Delay(1000);
    //    //            Device.BeginInvokeOnMainThread(() =>
    //    //            {
    //    //                if (string.IsNullOrEmpty(txt_Model.Text))
    //    //                {
    //    //                    DisplayAlert("Alert", "Please Enter UDAA /EOS Jobcard No.", "Ok");
    //    //                    return;
    //    //                }
    //    //                var result = services.CheckJobCard(App.JwtToken, txt_Model.Text);

    //    //                if (result.Result.d != null && !string.IsNullOrEmpty(result.Result.d.Vhvin))
    //    //                {
    //    //                    txtRegNo.Text = result.Result.d.RegNo;
    //    //                    txtChassisNo.Text = result.Result.d.Vhvin;
    //    //                    txtEngineNo.Text = result.Result.d.EngNo;
    //    //                    txt_Model.Text = result.Result.d.VehMod;
    //    //                    txtKMSCoveredNo.Text = result.Result.d.KmsCover;
    //    //                    Source = "udaan";
    //    //                    //JobCardSessionId = result.Result.d.id
    //    //                }
    //    //                else
    //    //                {
    //    //                    var res = services.CheckJobCardSecondAPI(App.JwtToken, txt_Model.Text);
    //    //                    if (res.Result != null && res.Result.Count > 0)
    //    //                    {
    //    //                        txtRegNo.Text = res.Result[0].VehicleRegisterNumber;
    //    //                        txtChassisNo.Text = res.Result[0].ChassisNo;
    //    //                        //txtEngineNo.Text = res.Result[0].;
    //    //                        txt_Model.Text = res.Result[0].ModelNumber;
    //    //                        txtKMSCoveredNo.Text = Convert.ToString(res.Result[0].KmCovered);
    //    //                        txtRegNo.Text = res.Result[0].VehicleRegisterNumber;
    //    //                        Source = "eos";
    //    //                    }
    //    //                    else
    //    //                    {
    //    //                        Source = "gen";
    //    //                        DisplayAlert("Alert", "Invalid Job Card Number.", "Ok");
    //    //                        txtRegNo.Text = string.Empty;
    //    //                        txtChassisNo.Text = string.Empty;
    //    //                        txtEngineNo.Text = string.Empty;
    //    //                        txt_Model.Text = string.Empty;
    //    //                        txtKMSCoveredNo.Text = string.Empty;
    //    //                        txtRegNo.Text = string.Empty;
    //    //                        txtComplaints.Text = string.Empty;
    //    //                    }
    //    //                }
    //    //                if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
    //    //                && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
    //    //                !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
    //    //                {
    //    //                    BtnSubmit.IsEnabled = true;
    //    //                }
    //    //                //Working = false;
    //    //            });
    //    //        });         
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        //Working = false;
    //    //    }
    //    //    finally
    //    //    {
    //    //        //Working = false;
    //    //    }
    //    //}

    //    private void txtChassisNo_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
    //            && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
    //            !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
    //        {
    //            BtnSubmit.IsEnabled = true;
    //        }
    //    }

    //    private void txtEngineNo_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
    //            && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
    //            !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
    //        {
    //            BtnSubmit.IsEnabled = true;
    //        }
    //    }

    //    private void txt_Model_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
    //            && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
    //            !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
    //        {
    //            BtnSubmit.IsEnabled = true;
    //        }
    //    }

    //    private void txtKMSCoveredNo_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
    //            && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
    //            !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
    //        {
    //            BtnSubmit.IsEnabled = true;
    //        }
    //    }

    //    private void txtComplaints_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        if (!string.IsNullOrEmpty(txtRegNo.Text) && !string.IsNullOrEmpty(txtChassisNo.Text) && !string.IsNullOrEmpty(txtEngineNo.Text)
    //            && !string.IsNullOrEmpty(txt_Model.Text) && !string.IsNullOrEmpty(txtKMSCoveredNo.Text) &&
    //            !string.IsNullOrEmpty(txtComplaints.Text) && !txt_Model.Text.Contains("Select Model.."))
    //        {
    //            BtnSubmit.IsEnabled = true;
    //        }
    //    }
    //    //string id = string.Empty;
    //    //public string Id
    //    //{
    //    //    get
    //    //    {
    //    //        if (!string.IsNullOrWhiteSpace(id))
    //    //            return id;

    //    //        id = Android.OS.Build.Serial;
    //    //        if (string.IsNullOrWhiteSpace(id) || id == Build.Unknown || id == "0")
    //    //        {
    //    //            try
    //    //            {
    //    //                var context = Android.App.Application.Context;
    //    //                id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
    //    //            }
    //    //            catch (Exception ex)
    //    //            {
    //    //                Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
    //    //            }
    //    //        }

    //    //        return id;
    //    //    }
    //    //}

    //    public List<JobCardModel> OfflineJobCard;
    //    public List<SubModelClass> SubModelClass;
    //    public List<CreatedBy> CreatedByCalss;
    //    public List<JobCardSession> JobCardSession;
    //    private async void BtnSubmit_Clicked(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
    //            if (isReachable)
    //            {
    //                if (txt_Model.Text != "Select Model" && App.model_id != 0 && txt_sub_model.Text != "Select Sub Model" && App.sub_model_id != 0 && txt_model_year.Text != "Select Model Year"
    //                    && txtRegNo.Text != null && txtRegNo.Text != "" && txtChassisNo.Text != null && txtChassisNo.Text != "" && txtEngineNo.Text != null && txtEngineNo.Text != "" && txtKMSCoveredNo.Text != null && txtKMSCoveredNo.Text != "")
    //                {
    //                    var AndroidID = Id;
    //                    SendJobcardData sendJobcardData = new SendJobcardData
    //                    {
    //                        chasis_id = txtChassisNo.Text,
    //                        complaints = txtComplaints.Text,
    //                        date = DateTime.Now.ToString(),
    //                        engine_no = txtEngineNo.Text,
    //                        km_covered = txtKMSCoveredNo.Text,
    //                        vehicle_model_id = Convert.ToString(modelResult.id),
    //                        model = txt_Model.Text,
    //                        VehModDes = "test",
    //                        status = "new",
    //                        job_card_name = txt_ecu.Text,
    //                        submodel = "Na",
    //                        vehicle_segment = "Na",
    //                        job_card_status = "new",
    //                        device_mac_id = DependencyService.Get<IGetDeviceUniqueId>().GetId(),
    //                        fert_code = "1233",
    //                        registration_no = txtRegNo.Text,
    //                        session_type = "regular",
    //                        source = "gen",
    //                    };
    //                    var CreateJobCard = await services.SendJobCard(sendJobcardData);

    //                    if (CreateJobCard.CreateJobcard != null)
    //                    {
    //                        var item = CreateJobCard.CreateJobcard;
    //                        JobCardListModel jobCardListModel = new JobCardListModel();
    //                        var date = item.job_card_session[0].start_date?.ToString("dd-MM-yyyy");
    //                        jobCardListModel = null;
    //                        if (item.job_card_session[0].status != "closed")
    //                        {
    //                            jobCardListModel = new JobCardListModel
    //                            {
    //                                chasis_id = item.chasis_id,
    //                                registration_no = item.registration_no,
    //                                complaints = item.complaints,
    //                                fert_code = item.fert_code,
    //                                jobcard_status = item.status,
    //                                vehicle_segment = item.vehicle_segment,
    //                                modified = item.modified,
    //                                km_covered = item.km_covered,
    //                                session_id = item.job_card_session[0].session_id,
    //                                status = item.job_card_session[0].status,
    //                                session_type = item.job_card_session[0].session_type,
    //                                source = item.job_card_session[0].source,
    //                                //source = "gen",
    //                                end_date = item.job_card_session[0].end_date,
    //                                start_date = item.job_card_session[0].start_date,
    //                                job_card = item.job_card_session[0].job_card,
    //                                id = item.job_card_session[0].id,
    //                                vehicle_model = item.job_card_session[0].vehicle_model.name,
    //                                model_year = item.job_card_session[0].vehicle_model.sub_models[0].model_year,
    //                                sub_model = item.job_card_session[0].vehicle_model.sub_models[0].name,
    //                                show_start_date = date,
    //                                model_id = item.job_card_session[0].vehicle_model.id,
    //                                sub_model_id = item.job_card_session[0].vehicle_model.sub_models[0].id,
    //                            };
    //                        }

    //                        App.SessionId = "";
    //                        App.SessionId = item.job_card_session[0].id;

    //                        var model_detail = await Task.Run(() =>
    //                        {
    //                            return services.get_all_models(App.JwtToken, 0);
    //                        });

    //                        //var model_detail = await services.get_all_models(App.JwtToken, 0);

    //                        var model = model_detail.results.FirstOrDefault(x => x.id == App.model_id).sub_models.FirstOrDefault(x => x.id == App.sub_model_id);
    //                        if (model.ecus.Count != 0)
    //                        {
    //                            StaticData.ecu_info.Clear();
    //                            foreach (var ecu_item in model.ecus)
    //                            {
    //                                if (ecu_item.datasets.Count == 0)
    //                                {
    //                                    await this.DisplayAlert("Alert! DTC Count 0", "ECU DTC Count are 0", "OK");
    //                                }
    //                                else if (ecu_item.pid_datasets.Count == 0)
    //                                {
    //                                    await this.DisplayAlert("Alert! PID Count 0", "ECU PID Count are 0", "OK");
    //                                }

    //                                StaticData.ecu_info.Add(
    //                                    new EcuDataSet
    //                                    {
    //                                        read_dtc_index = ecu_item.read_dtc_fn_index.value,
    //                                        pid_dataset_id = ecu_item.pid_datasets[0].id,
    //                                        clear_dtc_index = ecu_item.clear_dtc_fn_index.value,
    //                                        dtc_dataset_id = ecu_item.datasets[0].id,
    //                                        ecu_name = ecu_item.name,
    //                                        seed_key_index = ecu_item.seedkeyalgo_fn_index.value,
    //                                        write_pid_index = ecu_item.write_data_fn_index.value,
    //                                    });
    //                            }
    //                            if (StaticData.ecu_info.FirstOrDefault().pid_dataset_id != 0 && StaticData.ecu_info.FirstOrDefault().dtc_dataset_id != 0)
    //                            {
    //                                await this.DisplayAlert("Alert", "Jobcard successfully created", "OK");
    //                                await Navigation.PushAsync(new ConnectionPage(jobCardListModel));
    //                            }
    //                            else
    //                            {
    //                                if (StaticData.ecu_info.FirstOrDefault().pid_dataset_id == 0)
    //                                {
    //                                    await this.DisplayAlert("Alert", "Jobcard created successfully, But PID Count are 0", "OK");

    //                                    var mac_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
    //                                    Application.Current.MainPage = new MasterDetailView(mac_id);
    //                                }
    //                                else if (StaticData.ecu_info.FirstOrDefault().dtc_dataset_id == 0)
    //                                {
    //                                    await this.DisplayAlert("Alert", "Jobcard created successfully, But DTC Count are 0", "OK");

    //                                    var mac_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
    //                                    Application.Current.MainPage = new MasterDetailView(mac_id);
    //                                }
    //                            }
    //                        }
    //                        else
    //                        {
    //                            await this.DisplayAlert("Alert! ECU Count 0", "Jobcard created successfully, But ECU Count are 0", "OK");

    //                            var mac_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
    //                            Application.Current.MainPage = new MasterDetailView(mac_id);
    //                        }
    //                    }
    //                    else if (CreateJobCard.SameJobcard != null)
    //                    {
    //                        show_alert("Alert", "This jobcard number is already exist.", false, true);
    //                    }
    //                }
    //                else
    //                {
    //                    show_alert("Alert", "Some Fields are Incomplete, Please Enter All Fields", false, true);
    //                }
    //            }
    //            else
    //            {
    //                //show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
    //                if (txt_Model.Text != "Select Model" && App.model_id != 0 && txt_sub_model.Text != "Select Sub Model" && App.sub_model_id != 0 && txt_model_year.Text != "Select Model Year"
    //                    && txtRegNo.Text != null && txtRegNo.Text != "" && txtChassisNo.Text != null && txtChassisNo.Text != "" && txtEngineNo.Text != null && txtEngineNo.Text != "" && txtKMSCoveredNo.Text != null && txtKMSCoveredNo.Text != "")
    //                {
    //                    var result = await services.GetJobCard(App.JwtToken, "JbCardJson.txt");

    //                    OfflineJobCard.Clear();

    //                    foreach (var item in result)
    //                    {
    //                        OfflineJobCard.Add(new JobCardModel
    //                        {
    //                            id = item.id,
    //                            job_card_name = item.job_card_name,
    //                            status = item.status,
    //                            fert_code = item.fert_code,
    //                            vehicle_segment = item.vehicle_segment,
    //                            vehicle_model = item.vehicle_model,
    //                            chasis_id = item.chasis_id,
    //                            registration_no = item.registration_no,
    //                            km_covered = item.km_covered,
    //                            complaints = item.complaints,
    //                            created_by = item.created_by,
    //                            created = item.created,
    //                            modified = item.modified,
    //                            job_card_age = item.job_card_age,
    //                            job_card_session = item.job_card_session,
    //                        });
    //                    }

    //                    //SubModelClass.Add(new Model.SubModelClass { id = modelResult.sub_models.FirstOrDefault().id, model_year = modelResult.sub_models.FirstOrDefault().model_year, name = modelResult.sub_models.FirstOrDefault().name});
    //                    VehicleModel VM = new VehicleModel();
    //                    VM = new VehicleModel
    //                    {
    //                        id = modelResult.id + 02,
    //                        name = txt_Model.Text,
    //                        parent = null,
    //                        model_year = "N/A",
    //                        sub_models = null
    //                    };
    //                    CreatedBy CB = new CreatedBy();
    //                    CB = new CreatedBy
    //                    {
    //                        email = App.UserName,
    //                        role = App.MasterLoginUserRoleBY,
    //                        workshop = null
    //                    };
    //                    JobCardSession.Add(new JobCardSession
    //                    {
    //                        automated_session = null,
    //                        job_card_remote_session = null,
    //                        status = "Open",
    //                        session_type = "regular",
    //                        source = "gen",
    //                        user = null,
    //                        device = null,
    //                        vehicle_model = VM,
    //                        device_dongle = null,
    //                        end_date = null,
    //                        start_date = System.DateTime.Now,
    //                        job_card = "OfflineJobCard",
    //                        session_id = txt_Model.Text + "_" + System.DateTime.Now.ToString("mm/dd/yyyy m:s"),
    //                        id = "Offline_ID"
    //                    });

    //                    OfflineJobCard.Add(new JobCardModel
    //                    {
    //                        id = "OfflineID_JobCard",
    //                        job_card_name = txt_Model.Text,
    //                        status = "NewOffline",
    //                        fert_code = "1233",
    //                        vehicle_segment = "Na",
    //                        vehicle_model = VM,
    //                        chasis_id = txtChassisNo.Text,
    //                        registration_no = txtRegNo.Text,
    //                        km_covered = Convert.ToInt32(txtKMSCoveredNo.Text),
    //                        complaints = txtComplaints.Text,
    //                        created_by = CB,
    //                        created = System.DateTime.Now,
    //                        modified = System.DateTime.Now,
    //                        job_card_age = 1,
    //                        job_card_session = JobCardSession,
    //                    });

    //                    var json = JsonConvert.SerializeObject(OfflineJobCard);
    //                    await DependencyService.Get<ISaveLocalData>().SaveData("JsonList", json);

    //                    //foreach (var ExstingJobCardList in result)
    //                    //{
    //                    //    var date = ExstingJobCardList.job_card_session[0].start_date?.ToString("dd-MM-yyyy");
    //                    //    Off_JBList.Add(new JobCardListModel
    //                    //    {
    //                    //        chasis_id = ExstingJobCardList.chasis_id,
    //                    //        registration_no = ExstingJobCardList.registration_no,
    //                    //        complaints = ExstingJobCardList.complaints,
    //                    //        fert_code = ExstingJobCardList.fert_code,
    //                    //        jobcard_status = ExstingJobCardList.status,
    //                    //        vehicle_segment = ExstingJobCardList.vehicle_segment,
    //                    //        modified = ExstingJobCardList.modified,
    //                    //        km_covered = ExstingJobCardList.km_covered,
    //                    //        session_id = ExstingJobCardList.job_card_session[0].session_id,
    //                    //        status = ExstingJobCardList.job_card_session[0].status,
    //                    //        session_type = ExstingJobCardList.job_card_session[0].session_type,
    //                    //        source = ExstingJobCardList.job_card_session[0].source,
    //                    //        end_date = ExstingJobCardList.job_card_session[0].end_date,
    //                    //        start_date = ExstingJobCardList.job_card_session[0].start_date,
    //                    //        job_card = ExstingJobCardList.job_card_session[0].job_card,
    //                    //        id = ExstingJobCardList.job_card_session[0].id,
    //                    //        vehicle_model = ExstingJobCardList.job_card_session[0].vehicle_model.name,
    //                    //        model_year = ExstingJobCardList.job_card_session[0].vehicle_model.sub_models[0].model_year,
    //                    //        sub_model = ExstingJobCardList.job_card_session[0].vehicle_model.sub_models[0].name,
    //                    //        show_start_date = date,
    //                    //        model_id = ExstingJobCardList.job_card_session[0].vehicle_model.id,
    //                    //        sub_model_id = ExstingJobCardList.job_card_session[0].vehicle_model.sub_models[0].id,
    //                    //    });
    //                    //}
    //                    string ID = OfflineJobCard.FirstOrDefault().id;

    //                    JobCardListModel jobCardListModel = new JobCardListModel();
    //                    jobCardListModel = null;

    //                    jobCardListModel = new JobCardListModel
    //                    {
    //                        chasis_id = txtChassisNo.Text,
    //                        registration_no = txtRegNo.Text,
    //                        complaints = txtComplaints.Text,
    //                        fert_code = "1233",
    //                        jobcard_status = "new",
    //                        vehicle_segment = "Na",
    //                        modified = System.DateTime.Now,
    //                        km_covered = Convert.ToInt32(txtKMSCoveredNo.Text),
    //                        session_id = txt_Model.Text + "_" + System.DateTime.Now.ToString("mm/dd/yyyy m:s"),
    //                        status = "NewOffline",
    //                        session_type = "regular",
    //                        source = "gen",
    //                        end_date = System.DateTime.Now,
    //                        start_date = System.DateTime.Now,
    //                        job_card = "",
    //                        id = ID,
    //                        vehicle_model = txt_Model.Text,
    //                        model_year = txt_model_year.Text,
    //                        sub_model = txt_sub_model.Text,
    //                        show_start_date = System.DateTime.Now.ToString("mm/dd/yyyy"),
    //                        model_id = App.model_id,
    //                        sub_model_id = App.sub_model_id,
    //                    };

    //                    //Off_JBList.Add(new JobCardListModel
    //                    //{
    //                    //    chasis_id = txtChassisNo.Text,
    //                    //    registration_no = txtRegNo.Text,
    //                    //    complaints = txtComplaints.Text,
    //                    //    fert_code = "1233",
    //                    //    jobcard_status = "new",
    //                    //    vehicle_segment = "Na",
    //                    //    modified = System.DateTime.Now,
    //                    //    km_covered = Convert.ToInt32(txtKMSCoveredNo.Text),
    //                    //    session_id = txt_Model.Text + "_" + System.DateTime.Now.ToString("mm/dd/yyyy m:s"),
    //                    //    status = "NewOffline",
    //                    //    session_type = "regular",
    //                    //    source = "gen",
    //                    //    end_date = System.DateTime.Now,
    //                    //    start_date = System.DateTime.Now,
    //                    //    job_card = "",
    //                    //    id = ID + "_" + "Offile_New",
    //                    //    vehicle_model = txt_Model.Text,
    //                    //    model_year = txt_model_year.Text,
    //                    //    sub_model = txt_sub_model.Text,
    //                    //    show_start_date = System.DateTime.Now.ToString("mm/dd/yyyy"),
    //                    //    model_id = App.model_id,
    //                    //    sub_model_id = App.sub_model_id,
    //                    //});

    //                    var model_detail = App.ForOfflineJobCardCreate;

    //                    if (model_detail != null)
    //                    {
    //                        var model = model_detail.results.FirstOrDefault(x => x.id == App.model_id).sub_models.FirstOrDefault(x => x.id == App.sub_model_id);

    //                        if (model.ecus.Count != 0)
    //                        {
    //                            StaticData.ecu_info.Clear();
    //                            foreach (var ecu_item in model.ecus)
    //                            {
    //                                if (ecu_item.datasets.Count == 0)
    //                                {
    //                                    await this.DisplayAlert("Alert! DTC Count 0", "ECU DTC Count are 0", "OK");
    //                                }
    //                                else if (ecu_item.pid_datasets.Count == 0)
    //                                {
    //                                    await this.DisplayAlert("Alert! PID Count 0", "ECU PID Count are 0", "OK");
    //                                }

    //                                StaticData.ecu_info.Add(
    //                                    new EcuDataSet
    //                                    {
    //                                        read_dtc_index = ecu_item.read_dtc_fn_index.value,
    //                                        pid_dataset_id = ecu_item.pid_datasets[0].id,
    //                                        clear_dtc_index = ecu_item.clear_dtc_fn_index.value,
    //                                        dtc_dataset_id = ecu_item.datasets[0].id,
    //                                        ecu_name = ecu_item.name,
    //                                        seed_key_index = ecu_item.seedkeyalgo_fn_index.value,
    //                                        write_pid_index = ecu_item.write_data_fn_index.value,
    //                                    });
    //                            }
    //                            if (StaticData.ecu_info.FirstOrDefault().pid_dataset_id != 0 && StaticData.ecu_info.FirstOrDefault().dtc_dataset_id != 0)
    //                            {
    //                                await this.DisplayAlert("Alert", "Jobcard successfully created", "OK");
    //                                await Navigation.PushAsync(new ConnectionPage(jobCardListModel));
    //                            }
    //                            else
    //                            {
    //                                if (StaticData.ecu_info.FirstOrDefault().pid_dataset_id == 0)
    //                                {
    //                                    await this.DisplayAlert("Alert", "Jobcard created successfully, But PID Count are 0", "OK");

    //                                    var mac_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
    //                                    Application.Current.MainPage = new MasterDetailView(mac_id);
    //                                }
    //                                else if (StaticData.ecu_info.FirstOrDefault().dtc_dataset_id == 0)
    //                                {
    //                                    await this.DisplayAlert("Alert", "Jobcard created successfully, But DTC Count are 0", "OK");

    //                                    var mac_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
    //                                    Application.Current.MainPage = new MasterDetailView(mac_id);
    //                                }
    //                            }
    //                        }
    //                        else
    //                        {
    //                            await this.DisplayAlert("Alert! ECU Count 0", "Jobcard created successfully, But ECU Count are 0", "OK");

    //                            var mac_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
    //                            Application.Current.MainPage = new MasterDetailView(mac_id);
    //                        }


    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            show_alert("ERROR", "Please Contact to Administrator", false, true);
    //        }
    //    }

    //    private async void Search_JobCard_clicked(object sender, EventArgs e)
    //    {
    //        //var result = await DisplayAlert("Alert", "There is no job card with this ID. Do you want to create a new Job card ?", "Ok","Cancel");
    //        show_alert("Alert", "There is no job card with this ID. Do you want to create a new Job card ?", true, false);
    //        //if (result)
    //        OkCommand = new Command(() =>
    //        {
    //            Working = false;
    //            frm_model.IsVisible = true;
    //            frm_sub_model.IsVisible = true;
    //            frm_model_year.IsVisible = true;
    //            frm_reg_number.IsVisible = true;
    //            frm_chassis_no.IsVisible = true;
    //            frm_engine_no.IsVisible = true;
    //            frm_kms.IsVisible = true;
    //            frm_complain.IsVisible = true;
    //            BtnSubmit.IsEnabled = true;
    //        });
    //    }

    //    [Obsolete]
    //    private void select_model_clicked(object sender, EventArgs e)
    //    {
    //        PopupNavigation.PushAsync(new Bajaj.View.PopupPages.ModelPopupPage());
    //    }

    //    [Obsolete]
    //    private void select_sub_model_clicked(object sender, EventArgs e)
    //    {
    //        PopupNavigation.PushAsync(new SubModelPopupPage(modelResult.sub_models));
    //    }

    //    [Obsolete]
    //    private void select_model_year_clicked(object sender, EventArgs e)
    //    {
    //        PopupNavigation.PushAsync(new ModelYearPopupPage(model_year));
    //    }

    //    private void Create_JobCard_clicked(object sender, EventArgs e)
    //    {
    //        frm_model.IsVisible = true;
    //        frm_sub_model.IsVisible = true;
    //        frm_model_year.IsVisible = true;
    //        frm_reg_number.IsVisible = true;
    //        frm_chassis_no.IsVisible = true;
    //        frm_engine_no.IsVisible = true;
    //        frm_kms.IsVisible = true;
    //        frm_complain.IsVisible = true;
    //        BtnSubmit.IsEnabled = true;
    //    }

    //    public void show_alert(string title, string message, bool btnCancel, bool btnOk)
    //    {
    //        Working = true;
    //        TitleText = title;
    //        MessageText = message;
    //        OkVisible = btnOk;
    //        CancelVisible = btnCancel;
    //        CancelCommand = new Command(() =>
    //        {
    //            Working = false;
    //        });
    //    }
    //}
}