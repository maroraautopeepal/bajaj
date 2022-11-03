using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdvanceDiagnosticPage : DisplayAlertPage
    {
        public AdvanceDiagnosticPage()
        {
            InitializeComponent();
        }

        private async void BtnTestRoutines_Clicked(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new EicherDalLitePlus.View.TestRoutins.TestRoutinsPage(activityScheduler, serviceFactory,vdaProduct, base.physicalProduct, user));
        }

        private async void BtnCaliberation_Clicked(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new EicherDalLitePlus.View.Caliberation.CaliberationPage(activityScheduler,
            //    serviceFactory, vdaProduct, base.physicalProduct , user, null));
        }

        //private async void BtnFault_Clicked(object sender, EventArgs e)
        //{
        //    await Navigation.PushAsync(new EicherDalLitePlus.View.FaultList.FaultTabbedPage(activityScheduler, serviceFactory, base.physicalProduct, user));  
        //}

        private void BtnParameterWriting_Clicked(object sender, EventArgs e)
        {
            //var parametersToRead = new List<string>
            //{
            //    "P1AOC", "P1IP5", "P1ALV"
            //};
            //Navigation.PushAsync(new View.Parameter.ParameterListPage(activityScheduler, vdaProduct, base.physicalProduct,
            //    serviceFactory, user, null));
        }

        private void BtnProgrammingClicked(object sender, EventArgs e)
        {
            //Navigation.PushAsync(new View.Programming.ProgramingPage(activityScheduler, serviceFactory,
            //    vdaProduct, base.physicalProduct, user));
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

        //private void BackClicked(object sender, EventArgs e)
        //{
        //    this.Navigation.PopAsync();
        //}

        //private async void BtnHomeClicked(object sender, EventArgs e)
        //{
        //    for (var i = 1; i < 2; i++)
        //    {
        //        Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
        //    }
        //    Navigation.PopAsync();
        //}
        //private async void BtnExitClicked(object sender, EventArgs e)
        //{
        //    this.Navigation.PopAsync();
        //}
    }
}