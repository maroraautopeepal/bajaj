using Bajaj.Model;
using MultiEventController.Models;
using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;

namespace Bajaj.View.GdSection
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreeListPage : DisplayAlertPage
    {
        List<GdImageGD> gd_image = new List<GdImageGD>();
        //List<TreeClass> gd_list = new List<TreeClass>();

        //FirstListClass firstListClass = new FirstListClass();

        //GdImage gd_image = new GdImage();
        ResultGD gd_data = new ResultGD();

        public TreeListPage(List<ResultGD> gd_data, string Description, string Code)
        {
            InitializeComponent();
            BindingContext = this;
            this.Title = Code;
            this.gd_data = gd_data.FirstOrDefault();
            this.gd_image = gd_data.FirstOrDefault().gd_images;
            //gd_list = tree_list;
            list.ItemsSource = gd_data;
            //var TreeJson = JsonConvert.SerializeObject(tree_list);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
                App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
            }
            catch (Exception ex)
            {

            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            App.INFOPage = true;

            if (App.TreeL)
            {
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = "GoBack",
                    ElementValue = "GoBack",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }

            App.controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
            App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
        }
        private void ControlEventManager_OnRecievedData(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                #region Check Internet Connection
                if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
                {
                    await Task.Delay(100);
                    bool InsternetActive = true;

                    Device.StartTimer(new TimeSpan(0, 0, 01), () =>
                    {
                        // do something every 5 seconds
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            var _isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                            if (_isReachable)
                            {
                                string json = sender as string;
                                if (!string.IsNullOrEmpty(json))
                                {

                                }
                                InsternetActive = false;
                            }
                        });
                        return InsternetActive; // runs again, or false to stop
                    });
                }
                #endregion
            });
        }
        public string ReceiveValue = string.Empty;
        private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
        {
            var elementEventHandler = (sender as ElementEventHandler);
            this.ReceiveValue = elementEventHandler.ElementValue;
            if (ReceiveValue.Contains("Gd_Clicked_"))
            {
                string[] GD = { "Gd_Clicked_" };
                string[] Result = ReceiveValue.Split(GD, StringSplitOptions.RemoveEmptyEntries);
                var selectedItem = JsonConvert.DeserializeObject<ResultGD>(Result[0]);

                await Task.Delay(200);
                if (selectedItem != null)
                {
                    App.TreeL = false;
                    await this.Navigation.PushAsync(new TreeSurveyPage(gd_data, "", this.Title));
                }
            }
            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }
        private async void MenuItem1_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "MenuItem1",
                        ElementValue = "MenuItem1_Clicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }
               
                if (gd_data.tree_set != null && gd_data.tree_set.Any())
                {
                    await this.Navigation.PushAsync(new GdImagePage(gd_image, this.Title));
                }
                else
                {
                    var errorpage = new Popup.DisplayAlertPage("", "Tree not found.", "OK");
                    await PopupNavigation.Instance.PushAsync(errorpage);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async void Gd_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {

                    var selectedItem = (ResultGD)((Button)sender).BindingContext;
                    var JsonData = JsonConvert.SerializeObject(selectedItem);

                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "GDClick",
                        ElementValue = "Gd_Clicked_" + JsonData,
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });

                    if (selectedItem != null)
                    {
                        App.TreeL = false;
                        await this.Navigation.PushAsync(new TreeSurveyPage(gd_data, "", this.Title));
                    }
                }
                else
                {
                    var selectedItem = (ResultGD)((Button)sender).BindingContext;

                    if (selectedItem != null)
                    {
                        if (gd_data.tree_set != null && gd_data.tree_set.Any())
                        {
                            await this.Navigation.PushAsync(new TreeSurveyPage(gd_data, "", this.Title));
                        }
                        else
                        {
                            var errorpage = new Popup.DisplayAlertPage("", "Tree not found.", "OK");
                            await PopupNavigation.Instance.PushAsync(errorpage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
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



    #region old
    //public partial class TreeListPage : DisplayAlertPage
    //{
    //    List<GdImageGD> gd_image = new List<GdImageGD>();
    //    //List<TreeClass> gd_list = new List<TreeClass>();

    //    //FirstListClass firstListClass = new FirstListClass();

    //    //GdImage gd_image = new GdImage();
    //    ResultGD gd_data = new ResultGD();

    //    public TreeListPage(List<ResultGD> gd_data, string Description, string Code)
    //    {
    //        InitializeComponent();
    //        BindingContext = this;
    //        this.Title = Code;
    //        this.gd_data = gd_data.FirstOrDefault();
    //        this.gd_image = gd_data.FirstOrDefault().gd_images;
    //        //gd_list = tree_list;
    //        list.ItemsSource = gd_data;
    //        //var TreeJson = JsonConvert.SerializeObject(tree_list);
    //    }
    //    protected override void OnAppearing()
    //    {
    //        base.OnAppearing();
    //        try
    //        {
    //            App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
    //            App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //    }
    //    protected override void OnDisappearing()
    //    {
    //        base.OnDisappearing();

    //        App.INFOPage = true;

    //        App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //        {
    //            ElementName = "GoBack",
    //            ElementValue = "GoBack",
    //            ToUserId = CurrentUserEvent.Instance.ToUserId,
    //            IsExpert = CurrentUserEvent.Instance.IsExpert
    //        });

    //        App.controlEventManager.OnRecievedData -= ControlEventManager_OnRecievedData;
    //        App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
    //    }
    //    private void ControlEventManager_OnRecievedData(object sender, EventArgs e)
    //    {
    //        Device.BeginInvokeOnMainThread(async () =>
    //        {
    //            #region Check Internet Connection
    //            if (CurrentUserEvent.Instance.IsRemote && CurrentUserEvent.Instance.IsExpert)
    //            {
    //                await Task.Delay(100);
    //                bool InsternetActive = true;

    //                Device.StartTimer(new TimeSpan(0, 0, 01), () =>
    //                {
    //                    // do something every 5 seconds
    //                    Device.BeginInvokeOnMainThread(async () =>
    //                    {
    //                        var _isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
    //                        if (_isReachable)
    //                        {
    //                            string json = sender as string;
    //                            if (!string.IsNullOrEmpty(json))
    //                            {

    //                            }
    //                            InsternetActive = false;
    //                        }
    //                    });
    //                    return InsternetActive; // runs again, or false to stop
    //                });
    //            }
    //            #endregion
    //        });
    //    }
    //    public string ReceiveValue = string.Empty;
    //    private async void ControlEventManager_OnRecieved(object sender, EventArgs e)
    //    {
    //        var elementEventHandler = (sender as ElementEventHandler);
    //        this.ReceiveValue = elementEventHandler.ElementValue;
    //        if (ReceiveValue.Contains("Gd_Clicked_"))
    //        {
    //            string[] GD = { "Gd_Clicked_" };
    //            string[] Result = ReceiveValue.Split(GD, StringSplitOptions.RemoveEmptyEntries);
    //            var selectedItem = JsonConvert.DeserializeObject<ResultGD>(Result[0]);

    //            if (selectedItem != null)
    //            {
    //                this.Navigation.PushAsync(new TreeSurveyPage(gd_data, "", this.Title));
    //            }
    //        }
    //        App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
    //    }
    //    private void MenuItem1_Clicked(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            if (CurrentUserEvent.Instance.IsExpert)
    //            {
    //                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //                {
    //                    ElementName = "MenuItem1",
    //                    ElementValue = "MenuItem1_Clicked",
    //                    ToUserId = CurrentUserEvent.Instance.ToUserId,
    //                    IsExpert = CurrentUserEvent.Instance.IsExpert
    //                });
    //            }

    //            this.Navigation.PushAsync(new GdImagePage(gd_image, this.Title));
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }

    //    private void Gd_Clicked(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            if (CurrentUserEvent.Instance.IsExpert)
    //            {

    //                var selectedItem = (ResultGD)((Button)sender).BindingContext;
    //                var JsonData = JsonConvert.SerializeObject(selectedItem);                    

    //                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
    //                {
    //                    ElementName = "GDClick",
    //                    ElementValue = "Gd_Clicked_" + JsonData,
    //                    ToUserId = CurrentUserEvent.Instance.ToUserId,
    //                    IsExpert = CurrentUserEvent.Instance.IsExpert
    //                });

    //                if (selectedItem != null)
    //                {
    //                    this.Navigation.PushAsync(new TreeSurveyPage(gd_data, "", this.Title));
    //                }
    //            }
    //            else
    //            {
    //                var selectedItem = (ResultGD)((Button)sender).BindingContext;

    //                if (selectedItem != null)
    //                {
    //                    this.Navigation.PushAsync(new TreeSurveyPage(gd_data, "", this.Title));
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //        }
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
    #endregion
}