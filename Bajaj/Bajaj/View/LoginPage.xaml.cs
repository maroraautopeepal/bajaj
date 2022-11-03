using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : DisplayAlertPage
    {
        LoginViewModel viewModel;
        ApiServices services;
        List<DatasetId> dtc_datset_id = new List<DatasetId>();
        List<DatasetId> pid_datset_id = new List<DatasetId>();
        List<Results> pid_list = new List<Results>();
        List<DtcResults> dtc_list = new List<DtcResults>();
        public LoginPage()
        {
            try
            {
                InitializeComponent();

                services = new ApiServices();
                viewModel = new LoginViewModel();
                //txtUserId.Text = Application.Current.Properties["user_id"].ToString();
                //txtPassword.Text = Application.Current.Properties["password"].ToString();
                //if (string.IsNullOrEmpty(txtUserId.Text))
                //{
                //    checkRemember.IsChecked = false;
                //}
                //else
                //{
                //    checkRemember.IsChecked = true;
                //}

                //checkRemember.IsChecked = true;
            }
            catch (Exception ex)
            {
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                //var cc1 = System.Drawing.Color.FromArgb(0x66ff33);
                //App.Current.Resources["theme_color"] = cc1;
                //System.Drawing.Color color = System.Drawing.Color.FromArgb(0x66ff33);

                App.Current.Properties["MasterLoginUserRoleBY"] = null;
                App.Current.Properties["LoginUserId"] = null;
                App.Current.Properties["token"] = null;

                var data = DependencyService.Get<ISaveLocalData>().GetData("selctedOemModel");
                if (data != null)
                {
                    App.selectedOem = JsonConvert.DeserializeObject<AllOemModel>(data);
                    if (App.selectedOem.oem_file.Contains("http"))
                    {
                        logoImage.Source = App.selectedOem.oem_file;
                    }
                    else
                    {
                        string logoUrl = "http://159.65.152.179" + App.selectedOem.oem_file;
                        logoImage.Source = logoUrl;
                    }
                    
                    var color = ThemeManager.GetColorFromHexValue(App.selectedOem.color.ToString());
                    App.Current.Resources["theme_color"] = color;
                }
                

                //App.IsSameResponce.Clear();
                App.is_login = true;
                if (!App.NetConnected)
                {
                    show_alert("Alert", "Please check device internet connectivity.", false, true);
                }
            }
            catch (Exception ex)
            {

            }
        }


        
        //protected override void OnAppearing()
        //{
        //    CrossConnectivity.Current.ConnectivityChanged += UpdateNetworkInfo;
        //    base.OnAppearing();
        //}

        //protected override void OnDisappearing()
        //{
        //    CrossConnectivity.Current.ConnectivityChanged -= UpdateNetworkInfo;
        //}

        private void UpdateNetworkInfo(object sender, ConnectivityChangedEventArgs e)
        {
            var connectionType = CrossConnectivity.Current.ConnectionTypes.FirstOrDefault();
            //ConNet.Text = connectionType.ToString();
        }
        //string id = string.Empty;
        //public string Id
        //{
        //    get
        //    {
        //        if (!string.IsNullOrWhiteSpace(id))
        //            return id;

        //        id = Android.OS.Build.Serial;
        //        if (string.IsNullOrWhiteSpace(id) || id == Build.Unknown || id == "0")
        //        {
        //            try
        //            {
        //                var context = Android.App.Application.Context;
        //                id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
        //            }
        //            catch (Exception ex)
        //            {
        //                Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
        //            }
        //        }

        //        return id;
        //    }
        //}
        public bool RememberMeIsChecked;
        private async void SignInClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtUserId.Text) && string.IsNullOrEmpty(txtPassword.Text))
                {
                    show_alert("Alert", "Enter user name and password.", false, true);
                }
                else if (string.IsNullOrEmpty(txtUserId.Text))
                {
                    show_alert("Alert", "Enter user name", false, true);
                }
                else if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    show_alert("Alert", "Enter user password", false, true);
                }
                else
                {
                    var _LoginValues = DependencyService.Get<ISaveLocalData>().GetData("UserLoginDetailes");
                    var _RememberME = DependencyService.Get<ISaveLocalData>().GetData("RememberIsChecked");
                    if (_LoginValues != null && _RememberME != null)
                    {
                        var _RememberMEModel = JsonConvert.DeserializeObject<UserModelRememberIsChecked>(_RememberME);
                        if (_RememberMEModel.username == txtUserId.Text && _RememberMEModel.password == txtPassword.Text && _RememberMEModel.RememberIsChecked == true)
                        {
                            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                            {
                                //checkRemember.IsChecked = true;

                                await Task.Delay(100);
                                await LoadDataOffline();
                            }
                        }
                        else
                        {
                            //var _isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                            //if (_isReachable)
                            //{
                            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                            {
                                await Task.Delay(100);
                                await LoadDataOffline();
                            }
                            //}
                            //else
                            //{
                            //    await this.DisplayAlert("Internet Connection Issue", "Please Connect with Internet", "OK");
                            //}
                        }
                    }
                    else
                    {
                        //var _isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                        //if (_isReachable)
                        //{
                        //if (checkRemember.IsChecked)
                        //{
                        //    Application.Current.Properties["RememberMeIsChecked"] = true;
                        //}
                        //else
                        //{
                        //    Application.Current.Properties["RememberMeIsChecked"] = true;
                        //}
                        //await App.Current.SavePropertiesAsync();

                        using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                        {
                            await Task.Delay(100);
                            await LoadDataOffline();
                        }
                        //}
                        //else
                        //{
                        //await this.DisplayAlert("Internet Connection Issue", "Please Connect with Internet", "OK");
                        //}
                    }
                }
                var user = Application.Current.Properties["user_id"].ToString();
                var pass = Application.Current.Properties["password"].ToString();


                //var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                //if (isReachable)
                //{

                //}
                //else
                //{
                //    show_alert("Please check Internet Connection", "Internet Connection Problem", false, true);
                //}
            }
            catch (Exception ex)
            {
            }
        }
        public async Task LoadDataOffline()
        {
            try
            {
                //var _isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                //if (_isReachable)
                //{
                LoginRespons loginRespons = new LoginRespons();
                //using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                //{
                var AndroidID = Id;
                //await Task.Delay(1000);
                string plateform = string.Empty;
                var device_id = (dynamic)null;
                switch (Device.RuntimePlatform)
                {
                    case Device.Android:
                        device_id = DependencyService.Get<IGetDeviceUniqueId>().GetId();
                        break;
                    case Device.UWP:
                        device_id = "1234567890";
                        break;
                    default:
                        break;
                }

                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        break;
                    case Device.Android:
                        plateform = "android";
                        break;
                    case Device.UWP:
                        plateform = "windows";
                        break;
                    default:
                        break;
                }
                UserModel model = new UserModel
                {
                    device_type = plateform,
                    mac_id = device_id,
                    password = txtPassword.Text,
                    username = txtUserId.Text,
                };
                loginRespons = await services.Login(model);
                if (loginRespons.userRes != null)
                {
                    App.LoginUserID = loginRespons.userRes.user_id;

                    App.Current.Properties["LoginUserId"] = loginRespons.userRes.user_id;
                    App.Current.Properties["MasterLoginUserBY"] = loginRespons.userRes.first_name + " " + loginRespons.userRes.last_name;
                    App.Current.Properties["MasterLoginUserRoleBY"] = loginRespons.userRes.role;
                    Application.Current.Properties["UserName"] = txtUserId.Text;

                    if (loginRespons.userRes.licences.terminal)
                    {
                        App.Current.Properties["IsTerminal"] = "True";
                    }
                    else
                    {
                        App.Current.Properties["IsTerminal"] = "False";
                    }

                    await App.Current.SavePropertiesAsync();
                    //App.Current.MainPage = new NavigationPage(new View.IorTest.IorTestPage());
                    var models = await viewModel.get_all_models(0);
                    var model_list = models.results;
                    foreach (var item in model_list)
                    {
                        if (item.sub_models.Count > 0)
                        {
                            foreach (var sub_model in item.sub_models.ToList())
                            {
                                if (sub_model.ecus != null)
                                {
                                    foreach (var ecu in sub_model.ecus.ToList())
                                    {
                                        if (ecu.pid_datasets.Count > 0)
                                        {
                                            var pid = pid_datset_id.Find(x => x.id == ecu.pid_datasets[0].id);
                                            if (pid == null)
                                            {
                                                pid_datset_id.Add(new DatasetId { id = ecu.pid_datasets[0].id });
                                            }
                                        }

                                        if (ecu.datasets.Count > 0)
                                        {
                                            var dtc = dtc_datset_id.Find(x => x.id == ecu.datasets[0].id);
                                            if (dtc == null)
                                            {
                                                dtc_datset_id.Add(new DatasetId { id = ecu.datasets[0].id });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (var pid_id in pid_datset_id)
                    {
                        var pid = await viewModel.get_pid(pid_id.id);
                        pid_list.Add(pid);
                    }
                    var pid_list_json = JsonConvert.SerializeObject(pid_list);

                    await DependencyService.Get<ISaveLocalData>().SaveData("pidjson", pid_list_json);

                    foreach (var dtc_id in dtc_datset_id)
                    {
                        var dtc = await viewModel.get_dtc(dtc_id.id);
                        dtc_list.Add(dtc);
                    }
                    var dtc_list_json = JsonConvert.SerializeObject(dtc_list);

                    await DependencyService.Get<ISaveLocalData>().SaveData("dtcjson", dtc_list_json);


                    App.notify_technitian_name = $"{loginRespons.userRes.first_name} {loginRespons.userRes.last_name}";

                    //var val = await services.GetJobCard(App.JwtToken, "JbCardJson.txt");


                    //var DtcData = DependencyService.Get<ISaveLocalData>().GetData("dtcjson");
                    //var DtcInfo = JsonConvert.DeserializeObject<List<DtcResults>>(DtcData);

                    //if (checkRemember.IsChecked)
                    //{
                    //    Application.Current.Properties["user_id"] = model.username;
                    //    Application.Current.Properties["password"] = model.password;

                    //    UserModelRememberIsChecked Remember = new UserModelRememberIsChecked
                    //    {
                    //        device_type = plateform,
                    //        mac_id = device_id,
                    //        password = txtPassword.Text,
                    //        username = txtUserId.Text,
                    //        RememberIsChecked = true
                    //    };
                    //    var json = JsonConvert.SerializeObject(Remember);
                    //    await DependencyService.Get<ISaveLocalData>().SaveData("RememberIsChecked", json);
                    //}
                    //else
                    //{
                    //    Application.Current.Properties["user_id"] = "";
                    //    Application.Current.Properties["password"] = "";

                    //    UserModelRememberIsChecked Remember = new UserModelRememberIsChecked
                    //    {
                    //        device_type = plateform,
                    //        mac_id = device_id,
                    //        password = txtPassword.Text,
                    //        username = txtUserId.Text,
                    //        RememberIsChecked = false
                    //    };
                    //    var json = JsonConvert.SerializeObject(Remember);

                    //    await DependencyService.Get<ISaveLocalData>().SaveData("RememberIsChecked", json);
                    //}
                    //await App.Current.SavePropertiesAsync();
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            break;
                        case Device.Android:
                            App.Current.MainPage = new NavigationPage(new MasterDetailView(device_id) { Detail = new NavigationPage(new JobCardPage(device_id)) });
                            break;
                        case Device.UWP:
                            App.Current.MainPage = new NavigationPage(new MasterDetailView(device_id) { Detail = new NavigationPage(new JobCardPage(device_id)) });
                            App.is_login = true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    show_alert("Alert", $"{loginRespons.LoginRes.Error}   Device Id : {device_id}", false, true);
                }
                //}
                //}
                //else
                //{
                //    show_alert("Alert", "Please Check Internet Connection!!", false, true);
                //}
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

        private void ShowPassword_Tapped(object sender, EventArgs e)
        {
            if (txtPassword.IsPassword)
            {
                txtPassword.IsPassword = false;
                img_pass.Source = "ic_hide_pass.png";
            }
            else
            {
                txtPassword.IsPassword = true;
                img_pass.Source = "ic_password.png";
            }
        }

        private void CreateNewAccountTapped(object sender, EventArgs e)
        {
            try
            {
                Navigation.PushAsync(new SignInPage());
            }
            catch (Exception ex)
            {
            }
        }

        private void Forgot_Password_Tapped(object sender, EventArgs e)
        {
            show_alert("Forgot Password", "Please contact admin", false, true);
        }

        private void NeedHelpClicked(object sender, EventArgs e)
        {
            help_popup.IsVisible = true;
            //show_alert("Help", "Please contact admin", false, true);
        }

        private void OkClicked(object sender, EventArgs e)
        {
            help_popup.IsVisible = false;
            //show_alert("Help", "Please contact admin", false, true);
        }
    }

    public class DatasetId
    {
        public int id { get; set; }
    }
    public class UserModelRememberIsChecked
    {
        public string username { get; set; }
        public string password { get; set; }
        public string mac_id { get; set; }
        public string device_type { get; set; }
        public bool RememberIsChecked { get; set; }
    }
}