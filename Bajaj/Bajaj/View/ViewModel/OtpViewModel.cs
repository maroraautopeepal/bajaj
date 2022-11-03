using Acr.UserDialogs;
using Bajaj.FlashBud;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Serialization;
using Xamarin.Forms;

namespace Bajaj.View.ViewModel
{
    public class OtpViewModel : BaseViewModel
    {
        readonly Page page;
        ApiServices services;
        string OtpType = string.Empty;

        public OtpViewModel(string otpType, Page page)
        {
            this.page = page;
            this.OtpType = otpType;
            services = new ApiServices();
        }

        private string mOtp;
        public string otp
        {
            get => mOtp;
            set
            {
                mOtp = value;
                OnPropertyChanged();
            }
        }

        public ICommand SubmitCommand => new Command(async () =>
        {
            try
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    if (!string.IsNullOrEmpty(otp))
                    {
                        if (OtpType == "DealerOtp")
                        {
                            ValidateLoginOtpModel reqModel = new ValidateLoginOtpModel();
                            reqModel.serial_number = DependencyService.Get<IGetDeviceUniqueId>().GetId();
                            reqModel.otp = otp;
                            var resp = await services.ValidateLoginOtp(App.whoamiToken, reqModel);

                            if (resp != null)
                            {
                                if (resp.error != 0)
                                {
                                    await page.DisplayAlert("Alert", resp.message, "Ok");
                                    return;
                                }
                                else
                                {
                                    App.loginToken = resp.token;
                                    await page.Navigation.PushAsync(new ConnectionPage());
                                }
                            }
                        }
                        else
                        {
                            ValidateVinOtpModel reqModel = new ValidateVinOtpModel();
                            reqModel.serial_number = DependencyService.Get<IGetDeviceUniqueId>().GetId();
                            reqModel.otp = otp;
                            reqModel.vin = App.vin;

                            var resp = await services.ValidateVinOtp(App.loginToken, reqModel);

                            if (resp != null)
                            {
                                if (resp.error != 0)
                                {
                                    await page.DisplayAlert("Alert", resp.message, "Ok");
                                    return;
                                }
                                else
                                {
                                    //var udsUrl = resp.data.controllers.FirstOrDefault().uds_fault_list;
                                    var udsUrl = "http://159.65.152.179/media/media/KTM_BS6_DTC.xml";
                                    var dtcList = services.read_xml_file(udsUrl).Result;

                                    if (dtcList != null)
                                    {
                                        XmlSerializer deserializer = new XmlSerializer(typeof(Diagnostics_Trouble_Codes));
                                        object obj = deserializer.Deserialize(dtcList);
                                        Diagnostics_Trouble_Codes XmlData = (Diagnostics_Trouble_Codes)obj;
                                    }

                                    //var didUrl = resp.data.controllers.FirstOrDefault().uds_did_list;
                                    var didUrl = "http://159.65.152.179/media/media/DidWithoutRoutine.xml";
                                    var pidList = services.read_xml_file(didUrl).Result;

                                    if (pidList != null)
                                    {
                                        XmlSerializer deserializer1 = new XmlSerializer(typeof(UDS_Diag_Measurements));
                                        object obj = deserializer1.Deserialize(pidList);
                                        UDS_Diag_Measurements XmlData = (UDS_Diag_Measurements)obj;
                                    }


                                    var hexUrl = resp.data.controllers.FirstOrDefault().hexfiles.FirstOrDefault().app_hex;
                                    var hexFile = services.read_xml_file(hexUrl).Result;

                                    //GetJson getJson = new GetJson();
                                    //await getJson.ConvertToJson(hexFile);

                                    await page.Navigation.PushAsync(new AppFeaturePage());
                                }
                            }
                        }
                    }
                    else
                    {
                        await page.DisplayAlert("Alert", "Please enter OTP", "Ok");
                    }
                }

            }
            catch (Exception ex)
            {

            }                
        });

        public ICommand ResendCommand => new Command(async () =>
        {

        });
    }
}
