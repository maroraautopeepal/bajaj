using Acr.UserDialogs;
using Android.Bluetooth;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Services;
using Bajaj.View.ViewModel;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectionPage : DisplayAlertPage
    {
        
        ConnectionViewModel viewModel;
        public ConnectionPage()
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new ConnectionViewModel(this);
            }
            catch (Exception ex)
            {
            }
        }
    }

    //public class User
    //{
    //    public string email { get; set; }
    //    public object workshop { get; set; }
    //    public object role { get; set; }
    //}

    //public class CloseJobCard
    //{
    //    public string id { get; set; }
    //    public string source { get; set; }
    //    public string session_id { get; set; }
    //    public string job_card { get; set; }
    //    public DateTime start_date { get; set; }
    //    public DateTime end_date { get; set; }
    //    public User user { get; set; }
    //    public string session_type { get; set; }
    //    public string status { get; set; }
    //}

}