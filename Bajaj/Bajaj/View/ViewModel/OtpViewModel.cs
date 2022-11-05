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
                                StaticControllerData.controllers = new List<Controller>();
                                if (resp.error != 0)
                                {
                                    await page.DisplayAlert("Alert", resp.message, "Ok");
                                    return;
                                }
                                else
                                {
                                    foreach (var ecu in resp.data.controllers)
                                    {
                                        //var udsUrl = ecu.uds_fault_list;
                                        var udsUrl = "http://159.65.152.179/media/media/KTM_BS6_DTC.xml";
                                        var dtcList = services.read_xml_file(udsUrl).Result;

                                        Diagnostics_Trouble_Codes dtcXmlData = new Diagnostics_Trouble_Codes();
                                        UDS_Diag_Measurements pidXmlData = new UDS_Diag_Measurements();
                                        if (dtcList != null)
                                        {
                                            XmlSerializer deserializer = new XmlSerializer(typeof(Diagnostics_Trouble_Codes));
                                            object obj = deserializer.Deserialize(dtcList);
                                            dtcXmlData = (Diagnostics_Trouble_Codes)obj;

                                        }

                                        //var didUrl = ecu.uds_did_list;
                                        var didUrl = "http://159.65.152.179/media/media/DidWithoutRoutine.xml";
                                        var pidList = services.read_xml_file(didUrl).Result;

                                        if (pidList != null)
                                        {
                                            XmlSerializer deserializer1 = new XmlSerializer(typeof(UDS_Diag_Measurements));
                                            object obj = deserializer1.Deserialize(pidList);
                                            pidXmlData = (UDS_Diag_Measurements)obj;
                                        }

                                        var hexUrl = ecu.hexfiles.FirstOrDefault().app_hex;
                                        var hexFile = services.read_xml_file(hexUrl).Result;

                                        //GetJson getJson = new GetJson();
                                        //await getJson.ConvertToJson(hexFile);

                                        pidXmlData.Basic_ECU_Information.protocolInfo = GetProtocol(pidXmlData.Basic_ECU_Information);

                                        ecu.diagnostic_Trouble_Codes = dtcXmlData;
                                        ecu.uds_Diag_Measurements = pidXmlData;

                                        StaticControllerData.controllers.Add(ecu); 
                                    }

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

        public ProtocolInfo GetProtocol(Basic_ECU_Information info)
        {
            try
            {
                string protocol = "ISO15765_";

                switch (info.BaudRate)
                {
                    case "500":
                        protocol += "500KB_";
                        break;
                    case "250":
                        protocol += "250KB_";
                        break;
                }

                info.TX_ID = info.TX_ID.Substring(2);
                info.RX_ID = info.RX_ID.Substring(2);

                if (info.TX_ID.Length <= 4)
                {
                    info.TX_ID = info.TX_ID.PadLeft(4, '0');
                    info.RX_ID = info.RX_ID.PadLeft(4, '0');
                    protocol += "11BIT_CAN";
                }
                else
                {
                    info.TX_ID = info.TX_ID.PadLeft(8, '0');
                    info.RX_ID = info.RX_ID.PadLeft(8, '0');
                    protocol += "29BIT_CAN";
                }
                    

                var protocolVal = (int)((ProtocolDetail)Enum.Parse(typeof(ProtocolDetail), protocol));

                ProtocolInfo protocolInfo = new ProtocolInfo();
                protocolInfo.protocol = protocol;
                protocolInfo.value = protocolVal.ToString("X2");

                return protocolInfo;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private enum ProtocolDetail
        {
            ISO15765_250KB_11BIT_CAN = 00,
            ISO15765_250Kb_29BIT_CAN = 01,
            ISO15765_500KB_11BIT_CAN = 02,
            ISO15765_500KB_29BIT_CAN = 03,
            ISO15765_1MB_11BIT_CAN = 04,
            ISO15765_1MB_29BIT_CAN = 05,
            I250KB_11BIT_CAN = 06,
            I250Kb_29BIT_CAN = 07,
            I500KB_11BIT_CAN = 08,
            I500KB_29BIT_CAN = 09,
            I1MB_11BIT_CAN = 0x0A,
            I1MB_29BIT_CAN = 0x0B,
            OE_IVN_250KBPS_11BIT_CAN = 0x0C,
            OE_IVN_250KBPS_29BIT_CAN = 0x0D,
            OE_IVN_500KBPS_11BIT_CAN = 0x0E,
            OE_IVN_500KBPS_29BIT_CAN = 0x0F,

            OE_IVN_1MBPS_11BIT_CAN = 0x10,
            OE_IVN_1MBPS_29BIT_CAN = 0x11,
            CANOPEN_125KBPS_11BIT_CAN = 0x12,
            CANOPEN_500KBPS_11BIT_CAN = 0x13,
            XMODEM_125KBPS_11BIT_CAN = 0x18,
            XMODEM_500KBPS_11BIT_CAN = 0x1a,
            XMODEM_500KBPS_29BIT_CAN = 0x1b,
            XMODEM_125KBPS_29BIT_CAN = 0x19
        }
    }
}
