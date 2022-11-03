using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.View.PopupPages;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignInPage : DisplayAlertPage
    {
        WorkShopGroupModel workShopGroupModel;
        WorkCity workCity;
        WorkShopModel workShopModel;
        AllOemModel allOemModel;
        ApiServices services;
        long WorkShopId;
        public SignInPage()
        {
            try
            {
                InitializeComponent();
                workShopGroupModel = new WorkShopGroupModel();
                workCity = new WorkCity();
                workShopModel = new WorkShopModel();
                allOemModel = new AllOemModel();
                services = new ApiServices();

                MessagingCenter.Subscribe<OemPopupPage, AllOemModel>(this, "select_oem", (sender, arg) =>
                {
                    txtOem.Text = arg.name;
                    txtOem.TextColor = (Color)Application.Current.Resources["text_color"];
                    allOemModel = arg;
                });

                MessagingCenter.Subscribe<WorkshopGroupPopupPage, WorkShopGroupModel>(this, "WorkShopGroup", (sender, arg) =>
                {
                    txtWorkShopGroup.Text = arg.workshopsGroupName;
                    txtWorkShopGroup.TextColor = (Color)Application.Current.Resources["text_color"];
                    workShopGroupModel = arg;
                });

                MessagingCenter.Subscribe<CityPopupPage, WorkCity>(this, "City", (sender, arg) =>
                {
                    txtCity.Text = arg.city;
                    txtCity.TextColor = (Color)Application.Current.Resources["text_color"];
                    workCity = arg;
                });

                MessagingCenter.Subscribe<WorkShopPopupPage, WorkShopModel>(this, "WorkShop", (sender, arg) =>
                {
                    txtWorkShop.Text = arg.Workshop;
                    txtWorkShop.TextColor = (Color)Application.Current.Resources["text_color"];
                    WorkShopId = arg.WorkshopId;
                });
            }
            catch (Exception ex)
            {
            }
        }

        private async void SubmitClicked(object sender, EventArgs e)
        {
            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
            if (isReachable)
            {
                if (string.IsNullOrEmpty(txtFirstName.Text)
               || string.IsNullOrEmpty(txtLastName.Text)
               || string.IsNullOrEmpty(txtEmail.Text)
               || string.IsNullOrEmpty(txtMobileNumber.Text)
               || string.IsNullOrEmpty(txtPassword.Text)
               || string.IsNullOrEmpty(txtConfirmPassword.Text)
               || string.IsNullOrEmpty(txtWorkShopGroup.Text)
               || string.IsNullOrEmpty(txtCity.Text)
               || string.IsNullOrEmpty(txtWorkShop.Text))
                {
                    show_alert("Alert", "Please fill all required fields.", false, true);
                }
                else
                {
                    if (txtOem.Text == "Select Oem")
                    {
                        show_alert("Alert", "Please select oem.", false, true);
                    }
                    if (txtWorkShopGroup.Text == "Select Workshop Group")
                    {
                        show_alert("Alert", "Please select workshop group.", false, true);
                    }
                    if (txtCity.Text == "Select City")
                    {
                        show_alert("Alert", "Please select workshop city.", false, true);
                    }
                    if (txtWorkShop.Text == "Select Workshop")
                    {
                        show_alert("Alert", "Please select workshop name.", false, true);
                    }
                    else
                    {
                        if (txtPassword.Text != txtConfirmPassword.Text)
                        {
                            show_alert("Alert", "Password and Confirm Password are not matched.", false, true);
                        }
                        else
                        {
                            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                            {
                                await Task.Delay(100);
                                string plateform = string.Empty;
                                var data = JsonConvert.SerializeObject(App.selectedOem);
                                await DependencyService.Get<ISaveLocalData>().SaveData("selctedOemModel", data);
                                //var mac_id = DependencyService.Get<IDeviceMacAddress>().GetMacAddress();
                                string device_id = string.Empty;
                                switch (Device.RuntimePlatform)
                                {
                                    case Device.iOS:
                                        //top = 20;
                                        break;
                                    case Device.Android:
                                        device_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
                                        plateform = "android";
                                        break;
                                    case Device.UWP:
                                        plateform = "windows";
                                        device_id = "1234567890";
                                        break;
                                    default:
                                        //top = 0;
                                        break;
                                }
                                SigninModel signinModel = new SigninModel
                                {
                                    first_name = txtFirstName.Text,
                                    last_name = txtLastName.Text,
                                    email = txtEmail.Text,
                                    mobile = txtMobileNumber.Text,
                                    password = txtPassword.Text,
                                    password2 = txtConfirmPassword.Text,
                                    device_type = plateform,
                                    
                                    workshop = WorkShopId,
                                    mac_id = device_id,
                                    serial_number = "1234",
                                };
                                bool result = await services.RegisterUser(signinModel);
                                if (result)
                                {
                                    var errorpage = new Popup.DisplayAlertPage("Register Successfully", "Please Contact to Administrator for Approval !!", "OK");
                                    await PopupNavigation.Instance.PushAsync(errorpage);
                                    //show_alert("Register Successfully", "Please Contact to Administrator for Approval ", false, true);
                                    //await DisplayAlert("Register Successfully", "Please Contact to Administrator for Approval !!", "OK");
                                    Navigation.PopAsync();
                                }
                                else
                                {
                                    //await DisplayAlert("ERROR", "Email already Registered, Please Contact to Administrator for Approval !!", "OK");
                                    var errorpage = new Popup.DisplayAlertPage("ERROR", "Email already Registered, Please Contact to Administrator for Approval !!", "OK");
                                    await PopupNavigation.Instance.PushAsync(errorpage);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
            }
        }

        [Obsolete]
        private async void select_oem_clicked(object sender, EventArgs e)
        {
            try
            {
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (isReachable)
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        PopupNavigation.PushAsync(new Bajaj.View.PopupPages.OemPopupPage());
                    }
                }
                else
                {
                    show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                }

            }
            catch (Exception ex)
            {
            }
            
        }

        private async void SelectWorkshopGroup_Tapped(object sender, EventArgs e)
        {
            try
            {
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (isReachable)
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        if (txtOem.Text == "Select Oem")
                        {
                            show_alert("Alert", "Select firstly Workshop Group.", false, true);
                        }
                        else
                        {
                            PopupNavigation.PushAsync(new WorkshopGroupPopupPage());
                        }
                        
                    }
                }
                else
                {
                    show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                }

            }
            catch (Exception ex)
            {
            }
        }

        private async void SelectCity_Tapped(object sender, EventArgs e)
        {
            try
            {
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (isReachable)
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        if (txtWorkShopGroup.Text == "Select Workshop Group")
                        {
                            show_alert("Alert", "Select firstly Workshop Group.", false, true);
                        }
                        else
                        {
                            PopupNavigation.PushAsync(new CityPopupPage(workShopGroupModel));
                        }
                    }
                }
                else
                {
                    show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                }

            }
            catch (Exception ex)
            {
            }
        }

        private async void SelectWorkshop_Tapped(object sender, EventArgs e)
        {
            try
            {
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (isReachable)
                {
                    using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                    {
                        await Task.Delay(100);
                        if (txtCity.Text == "Select City" && txtWorkShopGroup.Text == "Select Workshop Group")
                        {
                            show_alert("Alert", "Select firstly Workshop Group and city.", false, true);
                        }
                        else if (txtCity.Text == "Select City")
                        {
                            show_alert("Alert", "Select firstly Workshop City.", false, true);
                        }
                        else
                        {
                            PopupNavigation.PushAsync(new WorkShopPopupPage(workCity));
                        }
                    }
                }
                else
                {
                    show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                }
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
}