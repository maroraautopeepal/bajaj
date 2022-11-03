using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChangePasswordPage : DisplayAlertPage
    {
        ApiServices services;
        public ChangePasswordPage()
        {
            InitializeComponent();

            services = new ApiServices();

            txtLabel.Text = "Password must have a minimum of 8 characters, at least one upper case and at least one special character ( _-*&^%$#@!)";
        }

        private void password_show_hide_clicke(object sender, EventArgs e)
        {
            var sen = sender as Image;
            switch (sen.ClassId)
            {
                case "exist":
                    if (txtExistPass.IsPassword)
                    {
                        img_exist.Source = "ic_hide_pass.png";
                    }
                    else
                    {
                        img_exist.Source = "ic_password.png";
                    }
                    txtExistPass.IsPassword = !txtExistPass.IsPassword;
                    break;

                case "new":
                    if (txtNewPass.IsPassword)
                    {
                        img_new.Source = "ic_hide_pass.png";
                    }
                    else
                    {
                        img_new.Source = "ic_password.png";
                    }
                    txtNewPass.IsPassword = !txtNewPass.IsPassword;
                    break;

                case "confirms":
                    if (txtconfPass.IsPassword)
                    {
                        img_conf.Source = "ic_hide_pass.png";
                    }
                    else
                    {
                        img_conf.Source = "ic_password.png";
                    }
                    txtconfPass.IsPassword = !txtconfPass.IsPassword;
                    break;

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

        private void BtnChangePassword_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtExistPass.Text) && string.IsNullOrEmpty(txtExistPass.Text))
                {
                    show_alert("Alert", "Please Enter Existing Password.", false, true);
                }
                else if (string.IsNullOrEmpty(txtNewPass.Text) && string.IsNullOrEmpty(txtNewPass.Text))
                {
                    show_alert("Alert", "Please Enter new Password", false, true);
                }
                else if (string.IsNullOrEmpty(txtconfPass.Text) && string.IsNullOrEmpty(txtconfPass.Text))
                {
                    show_alert("Alert", "Please Enter Confirm Password", false, true);
                }
                else
                {
                    string plateform = string.Empty;
                    var mac_id = DependencyService.Get<IDeviceMacAddress>().GetMacAddress();
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            //top = 20;
                            break;
                        case Device.Android:
                            plateform = "android";
                            break;
                        case Device.UWP:
                            plateform = "windows";
                            break;
                        default:
                            //top = 0;
                            break;
                    }
                    UserModel model = new UserModel
                    {
                        device_type = plateform,
                        mac_id = mac_id,
                        password = txtExistPass.Text,
                        username = Application.Current.Properties["user_id"].ToString(),
                    };

                    bool ReturnValue = services.ExistPasswordCheck(model);
                    if (ReturnValue == true)
                    {
                        ChangePassword CP = new ChangePassword
                        {
                            new_password1 = txtNewPass.Text,
                            new_password2 = txtconfPass.Text
                        };
                        bool ReturnResponse = services.ChangePassword(CP, App.JwtToken);
                        if (ReturnResponse == true)
                        {
                            show_alert("Successfully", "Password is changed successfully", true, false);
                        }
                        else
                        {
                            show_alert("ERROR", "Please Check Password again.", false, true);
                        }
                    }
                    else
                    {
                        show_alert("Alert", "Please Check Existing Password again.", false, true);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}