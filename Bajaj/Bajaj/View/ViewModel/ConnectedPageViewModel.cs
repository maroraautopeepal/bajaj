using Acr.UserDialogs;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class ConnectedPageViewModel : BaseViewModel
    {
        readonly Page page;
        public ConnectedPageViewModel(string firmwareVirsion, Page page)
        {
            this.page = page;
            FWVersion = firmwareVirsion;
            ConnectedImage = App.ConnectedVia == "BT" ? "ic_bluetooth.png" : "ic_usb.png";
        }

        private string mConnectedImage;
        public string ConnectedImage
        {
            get => mConnectedImage;
            set
            {
                mConnectedImage = value;
            }
        }

        private string mFWVersion;
        public string FWVersion
        {
            get => mFWVersion;
            set
            {
                mFWVersion = value;
                OnPropertyChanged();
            }
        }

        public ICommand VinCommand => new Command(async () =>
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                await Task.Delay(100);
                await page.Navigation.PushAsync(new ReadVinPage());
            }
        });
    }
}
