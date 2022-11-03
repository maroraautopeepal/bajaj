using MultiEventController;
using MultiEventController.Models;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace Bajaj.Popup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DisplayAlertPage : PopupPage
    {
        private TaskCompletionSource<bool> taskCompletionSource;
        public Task<bool> PopupClosedTask { get { return taskCompletionSource.Task; } }

        public DisplayAlertPage(string title, string messege, string ok, string cancel)
        {
            try
            {
                InitializeComponent();
                taskCompletionSource = new TaskCompletionSource<bool>();

                this.lblTitle.Text = title;
                this.lblMessege.Text = messege;
                buttonOk.Text = ok;
                buttonCancel.Text = cancel;
            }
            catch (Exception ex)
            {
                
            }
        }
        public DisplayAlertPage(string title, string messege, string ok)
        {
            try
            {
                InitializeComponent();
                taskCompletionSource = new TaskCompletionSource<bool>();

                this.lblTitle.Text = title;
                this.lblMessege.Text = messege;
                buttonOk.Text = ok;
                buttonCancel.IsVisible = false;
            }
            catch (Exception ex)
            {
                
            }
        }
        public DisplayAlertPage(bool ActiveOrNot)
        {
            try
            {
                InitializeComponent();
                taskCompletionSource = new TaskCompletionSource<bool>();

                buttonCancel.IsVisible = ActiveOrNot;
            }
            catch (Exception ex)
            {

            }
        }       
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            taskCompletionSource = new TaskCompletionSource<bool>();

            App.controlEventManager.OnRecieved += ControlEventManager_OnRecieved;

            //if (controlEventManager == null)
            //{
            //    controlEventManager = new ControlEventManager();
            //    await controlEventManager.Init();
            //    controlEventManager.OnRecieved += ControlEventManager_OnRecieved;
            //}
        }

        string ReceiveValue = string.Empty;
        private void ControlEventManager_OnRecieved(object sender, EventArgs e)
        {
            var elementEventHandler = (sender as ElementEventHandler);
            this.ReceiveValue = elementEventHandler.ElementValue;
            App.controlEventManager.RecieveCallControlEvents(this, elementEventHandler, CurrentUserEvent.Instance.OwnerUserId);
        }
        private void buttonOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "buttonOk",
                        ElementValue = "BtnOkClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }

                taskCompletionSource.SetResult(true);
                PopupNavigation.Instance.PopAllAsync();
                App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;
            }
            catch (Exception ex)
            {

            }
        }

        private void buttonCancel_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (CurrentUserEvent.Instance.IsExpert)
                {
                    App.controlEventManager.SendRequestControlEvents(new ElementEventHandler()
                    {
                        ElementName = "buttonCancel",
                        ElementValue = "BtnCancelClicked",
                        ToUserId = CurrentUserEvent.Instance.ToUserId,
                        IsExpert = CurrentUserEvent.Instance.IsExpert
                    });
                }

                taskCompletionSource.SetResult(false);
                PopupNavigation.Instance.PopAllAsync();
                App.controlEventManager.OnRecieved -= ControlEventManager_OnRecieved;

            }
            catch (Exception ex)
            {

            }            
        }

        protected override bool OnBackgroundClicked()
        {            
            return false;
        }
    }
}