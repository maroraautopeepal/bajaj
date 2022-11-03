using Bajaj.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReadVinPage : ContentPage
    {
        ReadVinViewModel viewModel;
        public ReadVinPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new ReadVinViewModel(this);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.IsAnalyzing = true;
            viewModel.IsScanning = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            viewModel.IsAnalyzing = false;
            viewModel.IsScanning = false;
        }

        private async void zxing_OnScanResult(ZXing.Result result)
        {
            try
            {
                viewModel.IsAnalyzing = false;
                viewModel.IsScanning = false;

                if (result != null)
                {
                    if (string.IsNullOrEmpty(result.Text))
                    {
                        viewModel.IsAnalyzing = true;
                        viewModel.IsScanning = true;
                        return;
                    }
                    viewModel.GetOtpForVin(result.Text);
                }
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Alert", "This is not valid vin number", "Ok");
                });
                viewModel.IsAnalyzing = true;
                viewModel.IsScanning = true;
            }
        }
    }
}