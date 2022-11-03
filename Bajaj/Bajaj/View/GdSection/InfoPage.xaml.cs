using Acr.UserDialogs;
using Bajaj.Model;
using MultiEventController.Models;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.GdSection
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    #region Old Code
    public partial class InfoPage : DisplayAlertPage
    {
        List<ResultGD> gd_data = new List<ResultGD>();
        List<GdImage> gd_image = new List<GdImage>();
        bool IsPageRemove = false;
        public InfoPage(List<ResultGD> gd_data)
        {
            try
            {
                InitializeComponent();
                this.gd_data = gd_data;
                //this.firstListClass = firstListClass;
                //UserId = "M387190";
                //ChessisId = "123456";
                Title = gd_data.FirstOrDefault().gd_id;
                txtDescription.Text = gd_data.FirstOrDefault().gd_description;
                txtCauses.Text = gd_data.FirstOrDefault().causes;
                txtRemAction.Text = gd_data.FirstOrDefault().remedial_actions;
                txtEffOnVehicle.Text = gd_data.FirstOrDefault().effects_on_vehicle;
                //trees = firstListClass.tree;
            }
            catch (Exception ex)
            {
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                App.controlEventManager.OnRecievedData += ControlEventManager_OnRecievedData;
                App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
                IsPageRemove = true;
            }
            catch (Exception ex)
            {

            }
        }

        private void ControlEventManager_OnRecievedData  (object sender, EventArgs e)
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
            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }

        private async void BtnStart_Clicked(object sender, EventArgs e)
        {
            //IsPageRemove = false;
            if (CurrentUserEvent.Instance.IsExpert)
            {
                App.INFOPage = false;
                App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                {
                    ElementName = "BtnStart",
                    ElementValue = "BtnStart_Clicked",
                    ToUserId = CurrentUserEvent.Instance.ToUserId,
                    IsExpert = CurrentUserEvent.Instance.IsExpert
                });
            }

            await Navigation.PushAsync(new TreeListPage(gd_data, txtDescription.Text, Title));
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsPageRemove)
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    //App.DCTPage = true;

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
        private void gd_image_clicked(object sender, EventArgs e)
        {
            try
            {
                //this.Navigation.PushAsync(new GdImagePage(firstListClass.main_list[0].gd_image, this.Title));
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
    #endregion
}