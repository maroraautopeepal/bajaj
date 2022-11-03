using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class ReadVinViewModel : BaseViewModel
    {
        readonly Page page;
        ApiServices services;
        public ReadVinViewModel(Page page)
        {
            this.page = page;
            services = new ApiServices();
        }

        #region Properties
        private string mEnteredPackage;
        public string EnteredPackage
        {
            get => mEnteredPackage;
            set
            {
                mEnteredPackage = value;
                OnPropertyChanged();
            }
        }

        private bool mIsScanning = true;
        public bool IsScanning
        {
            get => mIsScanning;
            set
            {
                mIsScanning = value;
                OnPropertyChanged();
            }
        }

        private bool mIsAnalyzing = true;
        public bool IsAnalyzing
        {
            get => mIsAnalyzing;
            set
            {
                mIsAnalyzing = value;
                OnPropertyChanged();
            }
        }
        #endregion


        #region Methods
        public async void GetOtpForVin(string vin)
        {
            Device.BeginInvokeOnMainThread( async () =>
            {
                try
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        var serialNumber = DependencyService.Get<IGetDeviceUniqueId>().GetId();
                        var resp = await services.GetVinOtp(App.loginToken, serialNumber, vin);

                        if (resp.error != 0)
                        {
                            await page.DisplayAlert("Alert", resp.message, "Ok");
                            IsAnalyzing = true;
                            IsScanning = true;
                        }
                        else
                        {
                            App.vin = vin;
                            await page.Navigation.PushAsync(new OtpPage("VinOtp"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    IsAnalyzing = true;
                    IsScanning = true;
                }

            });
        }
        #endregion


        #region Commands
        public ICommand ManualSubscriptionCommand => new Command(async (obj) =>
        {
            IsAnalyzing = false;
            IsScanning = false;

            string vin = (string)obj;

            if (string.IsNullOrEmpty(vin))
            {
                await page.DisplayAlert("Alert","Please enter vin number","Ok");
                IsAnalyzing = true;
                IsScanning = true;
                return;
            }
            GetOtpForVin(vin);
        });
        #endregion
    }
}
