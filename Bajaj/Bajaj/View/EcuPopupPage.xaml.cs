using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.ViewModel;
using MultiEventController;
using MultiEventController.Models;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Pages;
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
    public partial class EcuPopupPage : PopupPage
    {
        EcuViewModel viewModel;
        public EcuPopupPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new EcuViewModel();

            MessagingCenter.Subscribe<File>(this, "ItemTapped", async (sender) =>
            {
                var send = sender as File;
                var context = this.BindingContext as File;
                //var items = viewModel.WriteParameterList[int.Parse(send)];
                await ListItemSelected(send, 0);
            });
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
                App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;

                //await viewModel.get_models();

                if (CurrentUserEvent.Instance.IsExpert && CurrentUserEvent.Instance.IsRemote)
                {
                    DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                    await Task.Delay(200);
                }
                else if (!CurrentUserEvent.Instance.IsExpert)
                {
                    await viewModel.get_models();
                    string JsonData = JsonConvert.SerializeObject(viewModel.file);
                    App.controlEventManager.SendRequestData("FlashingFileListData_" + JsonData);
                }

                //if (controlEventManager == null)
                //{
                //    controlEventManager = new ControlEventManager();
                //    var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                //    if (isReachable)
                //    {
                //        await controlEventManager.Init();
                //    }
                //    controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
                //    controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;

                //    //await viewModel.get_models();

                //    if (CurrentUserEvent.Instance.IsExpert && CurrentUserEvent.Instance.IsRemote)
                //    {
                //        DependencyService.Get<ILodingPageService>().ShowLoadingPage();
                //        await Task.Delay(200);
                //    }
                //    else if (!CurrentUserEvent.Instance.IsExpert)
                //    {
                //        await viewModel.get_models();
                //        string JsonData = JsonConvert.SerializeObject(viewModel.file);
                //        controlEventManager.SendRequestData("FlashingFileListData_" + JsonData);
                //    }

                //    //Device.BeginInvokeOnMainThread(() =>
                //    //{
                //    //    if (!CurrentUserEvent.Instance.IsExpert)
                //    //    {
                //    //        string JsonData = JsonConvert.SerializeObject(viewModel.file);
                //    //        controlEventManager.SendRequestData("WriteParameterList_" + JsonData);
                //    //    }
                //    //});
                //}
            }
            catch (Exception ex)
            {

            }
           
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
            App.controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
        }
        public async Task<string> GetModel()
        {
            await viewModel.get_models();
            var Value = viewModel.file;
            string JsonData = JsonConvert.SerializeObject(Value);
            return JsonData;
        }

        private void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
                {
                    //viewModel.WriteParameterList = new List<PidCode>();

                    string json = sender as string;
                    if (!string.IsNullOrEmpty(json))
                    {
                        if (json.Contains("FlashingFileListData_"))
                        {
                            json = json.Replace("FlashingFileListData_", "");
                            var FileList = JsonConvert.DeserializeObject<List<File>>(json);
                            viewModel.file = new List<File>(FileList);
                            DependencyService.Get<ILodingPageService>().HideLoadingPage();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public string ReceiveValue = string.Empty;
        private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
        {
            try
            {
                var elementEventHandler = (sender as ElementEventHandler);
                this.ReceiveValue = elementEventHandler.ElementValue;

                if (elementEventHandler.ElementName == "ClosePopupTapped" && elementEventHandler.ElementValue == "Close_Popup_Tapped")
                {
                    if (PopupNavigation.Instance.PopupStack.Any())
                    {
                        await PopupNavigation.Instance.PopAsync();
                    }
                }

                var List = viewModel.file.Cast<object>().ToList();
                App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId, List);
            }
            catch (Exception ex)
            {

            }
        }

        private void CloseClick(object sender, EventArgs e)
        {
            try
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = "imgClose",
                    ElementValue = "Close_Popup_Tapped",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });

                txtSearch.Text = string.Empty;
                imgClose.IsVisible = false;
            }
            catch (Exception ex)
            {

            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = "txtSearch",
                    ElementValue = e.NewTextValue,
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });

                if (string.IsNullOrEmpty(txtSearch.Text))
                {
                    imgClose.IsVisible = false;
                    Ecu_List.ItemsSource = viewModel.file;
                }
                else
                {
                    imgClose.IsVisible = true;
                    Ecu_List.ItemsSource = viewModel.file.Where(x => x.data_file_name.ToLower().Contains(e.NewTextValue.ToLower()));
                }
            }
            catch (Exception ex)
            {
            }

        }

        private async void modelList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "Ecu_List",
                        ElementValue = Convert.ToString(e.ItemIndex),// "modelList_ItemTapped",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });

                    await ListItemSelected((e.Item as File), e.ItemIndex);
                }
            }
            catch (Exception ex)
            {
            }
        }
        private async Task ListItemSelected(File e, int index)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async() => 
                {
                    //var result = await DisplayAlert("Alert", $"Are you sure you want to Flash the dataset – {item.data_file_name}", "Ok", "Cancel");
                    //if (result)
                    var Alert = new Popup.DisplayAlertPage("Alert", $"Are you sure you want to Flash the dataset – {e.data_file_name}", "Ok", "Cancel");
                    await PopupNavigation.Instance.PushAsync(Alert);
                    bool alertResult = await Alert.PopupClosedTask;
                    await Task.Delay(200);
                    if (alertResult)
                    {
                        using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                        {
                            await Task.Delay(200);
                            FlashData flashData = new FlashData { ecu2 = viewModel.flash_data, file = e, SeedkeyalgoFnIndex_Values = viewModel.SeedkeyalgoFnIndex_Value, ecu_map_file = viewModel.ecu_map_file };
                            MessagingCenter.Unsubscribe<EcuPopupPage, FlashData>(this, "selected_parameter");
                            MessagingCenter.Send<EcuPopupPage, FlashData>(this, "selected_parameter", flashData);
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                if (PopupNavigation.Instance.PopupStack.Any())
                                {
                                    await PopupNavigation.Instance.PopAllAsync();
                                }
                            });
                        }
                    }
                    else
                    {
                        if (PopupNavigation.Instance.PopupStack.Any())
                        {
                            await PopupNavigation.Instance.PopAllAsync();
                        }
                    }
                });
                
                if (PopupNavigation.Instance.PopupStack.Any())
                {
                    await PopupNavigation.Instance.PopAllAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void Close_Popup_Tapped(object sender, EventArgs e)
        {
            try
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = "ClosePopupTapped",
                    ElementValue = "Close_Popup_Tapped",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });

                if (PopupNavigation.Instance.PopupStack.Any())
                {
                    await PopupNavigation.Instance.PopAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
    public class FlashData
    {
        public Ecu2 ecu2 { get; set; }
        public Model.File file { get; set; }
        public string seqFileUrl { get; set; }
        public Model.SeedkeyalgoFnIndex SeedkeyalgoFnIndex_Values { get; set; }
        public List<EcuMapFile> ecu_map_file { get; set; }
    }
}