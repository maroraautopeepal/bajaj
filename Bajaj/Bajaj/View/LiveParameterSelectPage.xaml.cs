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

        int count1 = 0;
        List<PidCode> pidCodes = new List<PidCode>();
        public async void GetPidList()
        {
            UserDialogs.Instance.ShowLoading("Loading...");
            await Task.Delay(100);
            try
            {
                //int pid_count = 0;
                count1 = 0;
                foreach (var ecu in StaticControllerData.controllers)
                {
                    count1++;
                    //int pid_dataset = ecu.pid_dataset_id;
                    //var pid_li = await apiServices.get_pid(App.JwtToken, pid_dataset);

                    List<PidCode> pidCode = new List<PidCode>();
                    foreach (var item in ecu.uds_Diag_Measurements.DataIdentifier)
                    {
                        item.Did__hex = item.Did__hex.Substring(2);
                        item.Did__hex = item.Did__hex.PadLeft(4, '0');
                        pidCode.Add(new PidCode
                        {
                            //id = ,
                            code = item.Did__hex,
                            short_name = item.Desc,
                            long_name = item.Desc,
                            total_len = int.Parse(item.Size_Bits) / 8,
                            byte_position = 1,
                            length = int.Parse(item.Size_Bits) / 8,
                            bitcoded = false,
                            //start_bit_position = null,
                            //end_bit_position = null,
                            resolution = item.Scaling == "NONE" ? 1 : double.Parse(item.Scaling),
                            offset = item.Offset == "NONE" ? 0 : double.Parse(item.Offset),
                            //min = item1.min,
                            //max = item1.max,
                            read = item.Access_Pvg.Contains("Read") ? true : false,
                            message_type = item.Conv_Rule == "VALUE" ? "CONTINUOUS" : "",
                            unit = item.Unit,
                            //messages = item1.messages
                        });
                    }

                    //pid_li.FirstOrDefault(x => x.id == pid_dataset).codes.Sort(x=>x.short_name);

                    viewModel.ecus_list.Add(
                    new EcusModel
                    {
                        ecu_name = ecu.name,
                        opacity = count1 == 1 ? 1 : .5,
                            //pid_list = pid_li.FirstOrDefault(x => x.id == pid_dataset).codes.OrderBy(x => x.short_name).ToList(),
                        pid_list = pidCode.OrderBy(x => x.short_name).ToList(),
                        protocol = ecu.uds_Diag_Measurements.Basic_ECU_Information.protocolInfo.value,
                        txHeader = ecu.uds_Diag_Measurements.Basic_ECU_Information.TX_ID,
                        rxHeader = ecu.uds_Diag_Measurements.Basic_ECU_Information.RX_ID
                    });
                }
                viewModel.static_pid_list = viewModel.pid_list = new ObservableCollection<PidCode>(viewModel.ecus_list.FirstOrDefault().pid_list);
                viewModel.selected_ecu = viewModel.ecus_list.FirstOrDefault();

            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
            }
            UserDialogs.Instance.HideLoading();
        }

        private async void ContinueClicked(object sender, EventArgs e)
        {
            //using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            //{
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
                            var errorpage = new Popup.DisplayAlertPage("Alert", "Please select any parameter", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                    }
                }
            //}
        }

        int count = 0;
        private async void SelectPidClicked(object sender, EventArgs e)
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
                }
                else
                {
                    if (viewModel.check_changed_pid.Selected)
                    {
                        viewModel.check_changed_pid.Selected = !viewModel.check_changed_pid.Selected;
                    }
                    else
                    {
                        var errorpage = new Popup.DisplayAlertPage("Alert", "You can select maximum 20 parameters", "OK");
                        await PopupNavigation.Instance.PushAsync(errorpage);
                    }
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
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