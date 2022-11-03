
using Bajaj.View.ViewModel;
using System;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FirmwareUpdatePage : DisplayAlertPage
    {
        FirmwareUpdateViewModel viewModel;

        //public FirmwareUpdatePage()
        //{
        //    InitializeComponent();
        //    BindingContext = viewModel = new FirmwareUpdateViewModel(this);
        //}

        public FirmwareUpdatePage(string FirmWareVersion)
        {
            InitializeComponent();
            BindingContext = viewModel = new FirmwareUpdateViewModel(this, FirmWareVersion);

            if (!string.IsNullOrEmpty(FirmWareVersion))
            {
                viewModel.UpdateFirmware(FirmWareVersion);
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

        private async void UpdateVersionClicked(object sender, System.EventArgs e)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(viewModel.Url);
                //var bytes1 = Encoding.ASCII.GetBytes(viewModel.Url);
                //int result = BitConverter.ToInt32(bytes, 0);
                //string decoded = Encoding.ASCII.GetString(bytes);

                var hexString = BitConverter.ToString(bytes);
                hexString = hexString.Replace("-", "");

                var Result1 = await DependencyService.Get<Interfaces.IConnectionWifi>().GetSsidPassword();

                var Result = await DependencyService.Get<Interfaces.IConnectionWifi>().SendFotaCommand(viewModel.Url);

                //string Data1 = "";
                //string sData = "";
                //while (viewModel.Url.Length > 0)
                ////first take two hex value using substring.  
                ////then convert Hex value into ascii.  
                ////then convert ascii value into character.  
                //{
                //    Data1 = System.Convert.ToChar(System.Convert.ToUInt32(viewModel.Url.Substring(0, 2), 16)).ToString();
                //    sData = sData + Data1;
                //    viewModel.Url = viewModel.Url.Substring(2, viewModel.Url.Length - 2);
                //}
                //return sData;
            }
            catch (Exception ex)
            {
            }
        }
    }
}