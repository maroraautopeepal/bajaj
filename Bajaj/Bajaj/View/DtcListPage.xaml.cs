using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Popup;
using Bajaj.Services;
using Bajaj.View.GdSection;
using Bajaj.ViewModel;
using MultiEventController;
using MultiEventController.Models;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DtcListPage : DisplayAlertPage
    {
        DtcViewModel viewModel;
        ApiServices services;
        string DebugTag = "Wifi Communication";

        public DtcListPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
            }
        }


        protected async override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = viewModel = new DtcViewModel();
            services = new ApiServices();
            await GetDTCList();
        }

        DtcEcusModel dtcEcusModel = new DtcEcusModel();
        string dtc_status = string.Empty;
        public async Task GetDTCList()
        {
            viewModel.DTCFoundOrNotMessage = "Looking for DTC Record";
            viewModel.empty_view_text = "Loading...";
            int count = 0;
            viewModel.ecus_list = new ObservableCollection<DtcEcusModel>();
            viewModel.dtc_list = new ObservableCollection<DtcCode>();
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                await Task.Delay(100);
                try
                {
                    //foreach (var ecu in StaticData.ecu_info)
                    //{
                        viewModel.is_running = true;
                        count++;
                    //int dtc_dataset = ecu.dtc_dataset_id;
                    //var dtc_li = await services.get_dtc(App.JwtToken, ecu.dtc_dataset_id);
                    var dtc_li = StaticControllerData.controllers.FirstOrDefault().diagnostic_Trouble_Codes.DTC_Identifier;
                        if (dtc_li == null)
                        {
                            await UserDialogs.Instance.AlertAsync("DTC not found from server", "Failed", "Ok");
                            viewModel.DTCFoundOrNotMessage = "DTC not found from server.";
                            return;
                        }

                        viewModel.dtc_server_list = new List<DtcCode>();
                    dtc_li.ForEach(x =>
                    {
                        viewModel.dtc_server_list.Add(new DtcCode
                        {
                            id = int.Parse(x.HEXCODE),
                            code = x.DTC_Code,
                            description = x.FaultName
                        });
                    });    


                        dtcEcusModel = new DtcEcusModel();
                        dtcEcusModel.ecu_name = StaticControllerData.controllers.FirstOrDefault().name;
                        dtcEcusModel.opacity = count == 1 ? 1 : .5;
                        

                        if (App.ConnectedVia == "USB")
                        {
                            viewModel.read_dtc = await DependencyService.Get<Interfaces.IConnectionUSB>().ReadDtc("UDS");
                        }
                        else if (App.ConnectedVia == "BT")
                        {
                            viewModel.read_dtc = await DependencyService.Get<Interfaces.IBth>().ReadDtc("UDS");
                        }

                        if (viewModel.read_dtc != null)
                        {
                            if (viewModel.read_dtc.status == "NO_ERROR")
                            {
                                var code = viewModel.read_dtc.dtcs.GetLength(0);
                                var status = viewModel.read_dtc.dtcs.GetLength(1);
                                for (int i = 0; i <= code - 1; i++)
                                {
                                    DtcCode dtcListModel = new DtcCode();
                                    dtcListModel.code = viewModel.read_dtc.dtcs[i, 0].ToString();
                                    for (int j = 0; j <= 0; j++)
                                    {
                                        dtc_status = viewModel.read_dtc.dtcs[i, 1].ToString();
                                        string[] split_string = dtc_status.Split(':');


                                        if (split_string[0] == "Inactive")
                                        {
                                            dtcListModel.status_activation = split_string[0];
                                            dtcListModel.status_activation_color = Color.Green;
                                        }
                                        else if (split_string[0] == "Active")
                                        {

                                            dtcListModel.status_activation = split_string[0];
                                            dtcListModel.status_activation_color = Color.Red;
                                        }

                                        if (split_string[1] == "LampOff")
                                        {
                                            dtcListModel.lamp_activation = split_string[1];
                                            dtcListModel.lamp_activation_color = Color.Green;
                                        }
                                        else if (split_string[1] == "LampOn")
                                        {

                                            dtcListModel.lamp_activation = split_string[1];
                                            dtcListModel.lamp_activation_color = Color.Red;
                                        }
                                        //Debug.WriteLine($"Dtc View Model : CODE = {dtcListModel.code}, "); //STATUS= {split_string[0]}:{split_string[1]}");
                                    }
                                    if (viewModel.dtc_server_list != null)
                                    {
                                        var desc = viewModel.dtc_server_list.FirstOrDefault(x => x.code == dtcListModel.code);
                                        if (desc != null)
                                        {
                                            dtcListModel.description = desc.description;
                                        }
                                        else
                                        {
                                            dtcListModel.description = "Description not found";
                                        }
                                        viewModel.dtc_list.Add(dtcListModel);
                                    }
                                    else
                                    {
                                        viewModel.DTCFoundOrNotMessage = "Please check DataSet Record !!! \nOR\n DataSet is Active or Not !!!";
                                    }
                                }
                                dtcEcusModel.dtc_list = new List<DtcCode>(viewModel.dtc_list);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(viewModel.read_dtc.status))
                                {

                                    //viewModel.is_running = false;
                                    viewModel.empty_view_text = viewModel.read_dtc.status;
                                    dtcEcusModel.emptyViewText = viewModel.read_dtc.status;
                                    dtcEcusModel.dtc_list = new List<DtcCode>();
                                }
                                else
                                {
                                    //await page.DisplayAlert("Alert", "ECU_COMMUNICATION_ERROR", "Ok");
                                    //viewModel.is_running = false;
                                    viewModel.empty_view_text = "ECU_COMMUNICATION_ERROR";
                                    dtcEcusModel.emptyViewText = "ECU_COMMUNICATION_ERROR";
                                    dtcEcusModel.dtc_list = new List<DtcCode>();
                                }
                            }
                        }
                        else
                        {
                            dtcEcusModel.dtc_list = new List<DtcCode>();
                        }

                        viewModel.ecus_list.Add(dtcEcusModel);


                        var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                    //}

                    viewModel.is_running = false;
                    viewModel.dtc_list = new ObservableCollection<DtcCode>(viewModel.ecus_list.FirstOrDefault().dtc_list);
                    viewModel.selected_ecu = viewModel.ecus_list[0];
                }
                catch (Exception ex)
                {
                    viewModel.is_running = false;
                    viewModel.empty_view_text = ex.Message + "\n\n\n" + ex.StackTrace;
                }

            }

        }

        private void btnRefresh_Clicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    await GetDTCList();
                }
            });
        }


        string clear_dtc_index = string.Empty;
        string Clear_dtc_android = string.Empty;
        private void btnClear_Clicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    try
                    {
                        string alertString = string.Empty;
                        foreach (var item in StaticData.ecu_info)
                        {
                            if (App.ConnectedVia == "USB")
                            {
                                DependencyService.Get<Interfaces.IConnectionUSB>().SetDongleProperties(item.protocol.autopeepal, item.tx_header, item.rx_header);
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                await DependencyService.Get<Interfaces.IBth>().SetDongleProperties(item.protocol.autopeepal, item.tx_header, item.rx_header);
                            }

                            Debug.WriteLine($"Clearing DTC for {item.ecu_name}");

                            clear_dtc_index = item.clear_dtc_index;
                            if (App.ConnectedVia == "USB")
                            {
                                Clear_dtc_android = await DependencyService.Get<Interfaces.IConnectionUSB>().ClearDtc(clear_dtc_index);
                            }
                            else if (App.ConnectedVia == "BT")
                            {
                                Clear_dtc_android = await DependencyService.Get<Interfaces.IBth>().ClearDtc(clear_dtc_index);
                            }
                            else if (App.ConnectedVia == "WIFI")
                            {
                                Clear_dtc_android = await DependencyService.Get<Interfaces.IConnectionWifi>().ClearDtc(clear_dtc_index);
                            }
                            else
                            {
                                Clear_dtc_android = await DependencyService.Get<Interfaces.IConnectionRP>().ClearDtc(clear_dtc_index);
                            }

                            if (Clear_dtc_android != null)
                            {
                                if (Clear_dtc_android.Contains("NOERROR"))
                                {
                                    alertString = alertString + $"\nDtc cleared for {item.ecu_name}";
                                }
                                else
                                {
                                    alertString = alertString + $"\nDtc not cleared for {item.ecu_name} : {Clear_dtc_android}";
                                }
                            }
                            else
                                alertString = alertString + $"\nCannot clear Dtc for {item.ecu_name}";
                        }
                        var errorpage = new Popup.DisplayAlertPage("Alert", alertString, "OK");
                        await PopupNavigation.Instance.PushAsync(errorpage);
                        await GetDTCList();
                    }
                    catch (Exception ex)
                    {

                    }

                }
            });
        }


    }
}