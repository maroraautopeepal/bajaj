using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
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
    public class DealerIdViewModel : BaseViewModel
    {
        readonly Page page;
        ApiServices services;
        string serialNo = string.Empty;

        public DealerIdViewModel(Page page)
        {
            this.page = page;
            services = new ApiServices();

            serialNo = DependencyService.Get<IGetDeviceUniqueId>().GetId();

            Task.Run(async () =>
            {
                var resp = await services.WhoAmI();

                if (resp != null)
                {
                    if (resp.error != 0)
                    {
                        await page.DisplayAlert("Alert", resp.message,"Ok");
                    }
                    else
                        App.whoamiToken = resp.token;
                }
            });

        }

        private string mdealerId;
        public string dealerId
        {
            get => mdealerId;
            set
            {
                mdealerId = value;
                OnPropertyChanged();
            }
        }

        public ICommand DealerCodeCommand => new Command(async () =>
        {
            try
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    if (!string.IsNullOrEmpty(dealerId))
                    {
                        var resp = await services.DealerLogin(App.whoamiToken, dealerId, serialNo);
                        if (resp != null)
                        {
                            if (resp.error != 0)
                            {
                                await page.DisplayAlert("Alert", resp.message, "Ok");
                                return;
                            }

                            await page.Navigation.PushAsync(new OtpPage("DealerOtp"));
                        }
                    }
                    else
                    {
                        await page.DisplayAlert("Alert", "Please enter dealer Id", "Ok");
                    }
                }
                    
            }
            catch (Exception ex)
            {

            }
        });
    }
}
