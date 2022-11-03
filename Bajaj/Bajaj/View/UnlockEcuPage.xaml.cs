using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
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
    public partial class UnlockEcuPage : DisplayAlertPage
    {
        JobCardListModel jobCard;
        UnlockEcuViewModel viewModel;
        ApiServices services;
        public int model_id;
        public int submodel_id;


        public UnlockEcuPage(JobCardListModel jobCardSession)
        {
            try
            {
                InitializeComponent();
                jobCard = jobCardSession;
                model_id = jobCard.model_id;
                submodel_id = jobCard.sub_model_id;
            }
            catch (Exception ex)
            {

            }
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = viewModel = new UnlockEcuViewModel();
            services = new ApiServices();
            GetEcuList();
        }

        ModelResult model;
        int count1 = 0;
        SubModel sub_model;
        Ecu ecu1;
        public async Task GetEcuList()
        {
            try
            {
                viewModel.ecus_list = new ObservableCollection<UnlockModel>();
                count1 = 0;
                viewModel.all_models_list = await services.get_all_models(App.JwtToken, 0);
                foreach (var ecu in StaticData.ecu_info)
                {
                    count1++;
                    int pid_dataset = ecu.pid_dataset_id;
                    //var pid_li = await services.get_pid(App.JwtToken, pid_dataset);
                    //viewModel.flash_file_list= viewModel.all_models_list.results.FirstOrDefault(x=>x.id== jobCard.model_id),
                    model = viewModel.all_models_list.results.FirstOrDefault(x => x.name == jobCard.vehicle_model);
                    if (model != null && model.sub_models.Any())
                    {
                        sub_model = model.sub_models.FirstOrDefault(x => x.id == jobCard.sub_model_id);
                        if (sub_model != null && sub_model.ecus.Any())
                        {
                            ecu1 = sub_model.ecus.FirstOrDefault(x => x.name == ecu.ecu_name);
                        }
                    }
                    viewModel.ecus_list.Add(
                       new UnlockModel
                       {
                           ecu_name = ecu.ecu_name,
                           opacity = count1 == 1 ? 1 : .5,
                           ecu2 = ecu1.ecu.FirstOrDefault(),
                           tx_header = ecu.tx_header,
                           rx_header = ecu.rx_header,
                           protocol = ecu.protocol
                       }); 

                }
                viewModel.selected_ecu = viewModel.ecus_list.FirstOrDefault();
            }
            catch(Exception ex)
            {

            }

        }

        private async void BtnUnlock_Clicked(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                try
                {
                    await Task.Delay(100);
                    var unlockdata = await services.getUnlockData();
                    ResultUnlock selectedUnlockData = new ResultUnlock();
                    foreach (var item in unlockdata)
                    {
                        if (item.submodel == submodel_id)
                        {
                            if (viewModel.selected_ecu.ecu_name == item.ecu.name)
                            {
                                selectedUnlockData = item;
                            }
                        }
                    }

                    string resonse = string.Empty;
                    if (selectedUnlockData.is_active == null)
                    {
                        await App.Current.MainPage.DisplayAlert("Alert","No unlock data available for this Ecu","Ok");
                    }
                    else if (selectedUnlockData.is_active == "ACTIVE")
                    {
                        DependencyService.Get<IToastMessage>().Show("Usb Unlocking started");
                        if (App.ConnectedVia == "USB")
                        {
                            await DependencyService.Get<Interfaces.IConnectionUSB>().unlockEcu(selectedUnlockData);
                        }
                        else if (App.ConnectedVia == "BT")
                        {
                            await DependencyService.Get<Interfaces.IBth>().unlockEcu(selectedUnlockData);
                        }
                        else
                        {
                            await DependencyService.Get<Interfaces.IConnectionWifi>().unlockEcu(selectedUnlockData);
                        }
                        await App.Current.MainPage.DisplayAlert("Success", "Ecu Unlocking completed", "Ok");
                        //DependencyService.Get<IToastMessage>().Show("Usb Unlocking completed");
                    }
                    else if (selectedUnlockData.is_active == "INACTIVE")
                    {
                        await App.Current.MainPage.DisplayAlert("Alert", "Ecu unlocking inactive", "Ok");
                    }
                }
                catch (Exception ex)
                {

                }
            }
            

        }
    }
}