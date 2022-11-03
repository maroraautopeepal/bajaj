using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Popup;
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
using System.Collections.ObjectModel;

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WriteParameterPopupPage : PopupPage
    {
        WriteParameterViewModel viewModel;
        EcuDataSet selected_ecuData;
        public WriteParameterPopupPage(EcuDataSet selected_ecu)
        {
            try
            {
                InitializeComponent();

                BindingContext = viewModel = new WriteParameterViewModel();

                selected_ecuData = selected_ecu;

                //MessagingCenter.Subscribe<ControlEventManager>(this, "PopUpWriteParameter_ItemClicked", async (sender) =>
                //{
                //    var send = sender.ObjectReceiveValue as ElementEventHandler;
                //    var context = this.BindingContext as PidCode;
                //    var items = viewModel.WriteParameterList[int.Parse(send.ItemIndex)];
                //    await ListItemSelected(items, 0);
                //});

                //viewModel.get_pid(selected_ecu);


                MessagingCenter.Subscribe<PidCode>(this, "ItemTapped", async (sender) =>
                {
                    var send = sender as PidCode;
                    var context = this.BindingContext as PidCode;
                    //var items = viewModel.WriteParameterList[int.Parse(send)];
                    //await ListItemSelected(send, 0);
                });
            }
            catch (Exception)
            {

            }
        }
        //protected async override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    try
        //    {
        //        App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
        //        App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;

        //        if (CurrentUserEvent.Instance.IsExpert)
        //        {
        //            DependencyService.Get<ILodingPageService>().InitLoadingPage(new LoadingIndicatorPage());
        //            DependencyService.Get<ILodingPageService>().ShowLoadingPage();
        //            await Task.Delay(2000);

        //            //DependencyService.Get<ILodingPageService>().HideLoadingPage();

        //            //loaderPopUp = new LoaderPopUpPage(true);
        //            //await PopupNavigation.Instance.PushAsync(loaderPopUp, true);
        //        }
        //        else if (!CurrentUserEvent.Instance.IsExpert)
        //        {
        //            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
        //            {
        //                await Task.Delay(100);
        //                var Value = viewModel.PidList(selected_ecuData);
        //                string JsonData = JsonConvert.SerializeObject(Value);
        //                App.controlEventManager.SendRequestData("WriteParameterList_" + JsonData);

        //                viewModel.WriteParameterList = new ObservableCollection<PidCode>(Value);
        //            }
        //        }

        //        //if (controlEventManager == null)
        //        //{
        //        //    controlEventManager = new ControlEventManager();
        //        //    var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
        //        //    if (isReachable)
        //        //    {
        //        //        await controlEventManager.Init();
        //        //    }

        //        //    controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
        //        //    controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;

        //        //    if (CurrentUserEvent.Instance.IsExpert)
        //        //    {
        //        //        DependencyService.Get<ILodingPageService>().InitLoadingPage(new LoadingIndicatorPage());
        //        //        DependencyService.Get<ILodingPageService>().ShowLoadingPage();
        //        //        await Task.Delay(2000);

        //        //        //DependencyService.Get<ILodingPageService>().HideLoadingPage();

        //        //        //loaderPopUp = new LoaderPopUpPage(true);
        //        //        //await PopupNavigation.Instance.PushAsync(loaderPopUp, true);
        //        //    }
        //        //    else if (!CurrentUserEvent.Instance.IsExpert)
        //        //    {
        //        //        var Value = viewModel.PidList(selected_ecuData);
        //        //        string JsonData = JsonConvert.SerializeObject(Value);
        //        //        controlEventManager.SendRequestData("WriteParameterList_" + JsonData);

        //        //        viewModel.WriteParameterList = Value;
        //        //    }

        //        //    //Device.BeginInvokeOnMainThread(async () =>
        //        //    //{
        //        //    //    if (!CurrentUserEvent.Instance.IsExpert)
        //        //    //    {
        //        //    //        var Value = viewModel.PidList(selected_ecuData);
        //        //    //        viewModel.WriteParameterList = Value;

        //        //    //        string JsonData = JsonConvert.SerializeObject(viewModel.WriteParameterList);
        //        //    //        controlEventManager.SendRequestData("WriteParameterList_" + JsonData);
        //        //    //    }
        //        //    //});

        //        //    //if (!CurrentUserEvent.Instance.IsExpert)
        //        //    //{
        //        //    //    var ParameterList = viewModel.WriteParameterList;
        //        //    //    string JsonData = JsonConvert.SerializeObject(ParameterList);
        //        //    //    controlEventManager.SendRequestData(JsonData);
        //        //    //}
        //        //}
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}
        //private void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
        //        {
        //            //viewModel.WriteParameterList = new List<PidCode>();

        //            string json = sender as string;
        //            if (!string.IsNullOrEmpty(json))
        //            {
        //                if (json.Contains("WriteParameterList_"))
        //                {
        //                    DependencyService.Get<ILodingPageService>().HideLoadingPage();
        //                    json = json.Replace("WriteParameterList_", "");
        //                    var ParameterList = JsonConvert.DeserializeObject<List<PidCode>>(json);
        //                    viewModel.WriteParameterList = new ObservableCollection<PidCode>(ParameterList);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}
        //public string ReceiveValue = string.Empty;
        //private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var elementEventHandler = (sender as ElementEventHandler);
        //        this.ReceiveValue = elementEventHandler.ElementValue;

        //        if (elementEventHandler.ElementName == "ClosePopupTapped" && elementEventHandler.ElementValue == "Close_Popup_Tapped")
        //        {
        //            if (PopupNavigation.Instance.PopupStack.Any())
        //            {
        //                await PopupNavigation.Instance.PopAllAsync();
        //            }
        //        }

        //        //if (elementEventHandler.ListItem != null)
        //        //{
        //        //    var eListItem = elementEventHandler.ListItem;
        //        //    var eItemIndex = elementEventHandler.ItemIndex;
        //        //    var eValuesData = elementEventHandler.eValues;
        //        //    modelList_ItemTapped(null, eValuesData);
        //        //}

        //        var List = viewModel.WriteParameterList.Cast<object>().ToList();
        //        var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
        //        if (isReachable)
        //        {
        //            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId, List);
        //        }

        //        //MessagingCenter.Send<PidCode>((PidCode)elementEventHandler.ListItem, "PopUpWriteParameter_ItemClicked");
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        //private void CloseClick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
        //        {
        //            ElementName = "imgClose",
        //            ElementValue = "Close_Popup_Tapped",
        //            ToUserId = CurrentUserEvent.Instance.ToUserId,
        //            IsExpert = CurrentUserEvent.Instance.IsExpert
        //        });

        //        txtSearch.Text = string.Empty;
        //        imgClose.IsVisible = false;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        //private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    try
        //    {
        //        WriteParameter_List.ItemsSource = null;
        //        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
        //        {
        //            ElementName = "txtSearch",
        //            ElementValue = e.NewTextValue,
        //            ToUserId = CurrentUserEvent.Instance.ToUserId,
        //            IsExpert = CurrentUserEvent.Instance.IsExpert
        //        });

        //        if (string.IsNullOrEmpty(txtSearch.Text))
        //        {
        //            imgClose.IsVisible = false;
        //            WriteParameter_List.ItemsSource = viewModel.WriteParameterList;
        //        }
        //        else
        //        {
        //            imgClose.IsVisible = true;
        //            WriteParameter_List.ItemsSource = viewModel.WriteParameterList.Where(x => x.short_name.ToLower().Contains(e.NewTextValue.ToLower()));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //}

        //private async void modelList_ItemTapped(object sender, ItemTappedEventArgs e)
        //{
        //    try
        //    {
        //        if (CurrentUserEvent.Instance.IsExpert)
        //        {
        //            App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
        //            {
        //                ElementName = "WriteParameter_List",
        //                ElementValue = Convert.ToString(e.ItemIndex),// "modelList_ItemTapped",
        //                ToUserId = CurrentUserEvent.Instance.ToUserId,
        //                IsExpert = CurrentUserEvent.Instance.IsExpert
        //            });

        //            await ListItemSelected((e.Item as PidCode), e.ItemIndex);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
        //bool Clicked = true;
        //private async Task ListItemSelected(PidCode e, int index)
        //{
        //    try
        //    {
        //        Device.BeginInvokeOnMainThread(async () =>
        //        {
        //            //var item = (e.Item as PidCode);
        //            var Alert = new Popup.DisplayAlertPage("Alert", $"Are you sure you want to write the parameter – {e.short_name}", "Ok", "Cancel");
        //            await PopupNavigation.Instance.PushAsync(Alert);
        //            bool alertResult = await Alert.PopupClosedTask;
        //            if (alertResult)
        //            {
        //                MessagingCenter.Unsubscribe<WriteParameterPopupPage, PidCode>(this, "write_parameter");

        //                MessagingCenter.Send<WriteParameterPopupPage, PidCode>(this, "write_parameter", e);

        //                //controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
        //                //controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
        //                if (PopupNavigation.Instance.PopupStack.Any())
        //                {
        //                    await PopupNavigation.Instance.PopAllAsync();
        //                }
        //                //using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
        //                //{
        //                //    await Task.Delay(100);

        //                //    MessagingCenter.Unsubscribe<WriteParameterPopupPage, PidCode>(this, "write_parameter");

        //                //    MessagingCenter.Send<WriteParameterPopupPage, PidCode>(this, "write_parameter", e);

        //                //    //controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
        //                //    //controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
        //                //    if (PopupNavigation.Instance.PopupStack.Any())
        //                //    {
        //                //        await PopupNavigation.Instance.PopAllAsync();
        //                //    }
        //                //}
        //            }
        //            else
        //            {
        //                if (PopupNavigation.Instance.PopupStack.Any())
        //                {
        //                    await PopupNavigation.Instance.PopAllAsync();
        //                }
        //            }
        //        });

        //        if (PopupNavigation.Instance.PopupStack.Any())
        //        {
        //            await PopupNavigation.Instance.PopAllAsync();
        //        }
        //        #region Old Code
        //        ////var item = (e.Item as PidCode);
        //        //var Alert = new Popup.DisplayAlertPage("Alert", $"Are you sure you want to write the parameter – {e.short_name}", "Ok", "Cancel");
        //        ////await PopupNavigation.Instance.PushAsync(Alert);
        //        //bool alertResult = await Alert.PopupClosedTask;
        //        //if (alertResult)
        //        //{
        //        //    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
        //        //    {
        //        //        await Task.Delay(100);

        //        //        MessagingCenter.Unsubscribe<WriteParameterPopupPage, PidCode>(this, "write_parameter");

        //        //        MessagingCenter.Send<WriteParameterPopupPage, PidCode>(this, "write_parameter", e);

        //        //        //controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
        //        //        //controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
        //        //        if (PopupNavigation.Instance.PopupStack.Any())
        //        //        {
        //        //            await PopupNavigation.Instance.PopAllAsync();
        //        //        }
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    if (PopupNavigation.Instance.PopupStack.Any())
        //        //    {
        //        //        await PopupNavigation.Instance.PopAllAsync();
        //        //    }
        //        //}
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {

        //    }



        //    //var result = await DisplayAlert("Alert", $"Are you sure you want to write the parameter – {e.short_name}", "Ok", "Cancel");
        //    //if (result)
        //    //{
        //    //    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
        //    //    {
        //    //        await Task.Delay(100);

        //    //        if (!CurrentUserEvent.Instance.IsExpert)
        //    //        {
        //    //            MessagingCenter.Unsubscribe<WriteParameterPopupPage, PidCode>(this, "write_parameter");

        //    //            MessagingCenter.Send<WriteParameterPopupPage, PidCode>(this, "write_parameter", e);
        //    //            await PopupNavigation.Instance.PopAllAsync();
        //    //        }
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    await PopupNavigation.Instance.PopAllAsync();
        //    //}
        //}
        //private async void Close_Popup_Tapped(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
        //        {
        //            ElementName = "ClosePopupTapped",
        //            ElementValue = "Close_Popup_Tapped",
        //            ToUserId = CurrentUserEvent.Instance.ToUserId,
        //            IsExpert = CurrentUserEvent.Instance.IsExpert
        //        });

        //        if (PopupNavigation.Instance.PopupStack.Any())
        //        {
        //            await PopupNavigation.Instance.PopAllAsync();
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}
    }
}