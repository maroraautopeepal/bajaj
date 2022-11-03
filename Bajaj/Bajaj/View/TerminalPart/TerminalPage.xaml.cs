using Bajaj.Interfaces;
using Bajaj.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.TerminalPart
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TerminalPage : ContentPage
    {
        TerminalViewModel viewModel;
        List<DataClass> DataList;
        public TerminalPage(string[] Result)
        {
            InitializeComponent();
            BindingContext = viewModel = new TerminalViewModel(Result);
            DataList = new List<DataClass>();
        }


        private void LinkClicked(object sender, EventArgs e)
        {
            if (viewModel.ConnectImage == "ic_link.png")
            {
                this.Navigation.PushAsync(new LinkDonglePage());
            }
            else
            {

            }
        }

        string response = string.Empty;
        private async void CommandClicked(object sender, EventArgs e)
        {
            try
            {
                response = string.Empty;
                //await DisplayAlert("Success 1", "", "Ok");
                if (string.IsNullOrEmpty(viewModel.SenderCommand))
                {
                    await DisplayAlert("Alert", "Please enter a command", "Ok");
                    return;
                }

                if (viewModel.SenderCommand.Length % 2 != 0 ||
                    !System.Text.RegularExpressions.Regex.IsMatch(viewModel.SenderCommand, @"\A\b[0-9a-fA-F]+\b\Z"))
                {
                    await DisplayAlert("Alert", "Please enter valid command", "Ok");
                    return;
                }

                //await DisplayAlert("Success 2", "", "Ok");

                string sender_time = DateTime.Now.ToString("hh:mm:ss.fff");
                var con = App.ConnectedVia;
                if (App.ConnectedVia.Contains("USB"))
                {
                    response = await DependencyService.Get<IConnectionUSB>().SetData(viewModel.SenderCommand);
                }
                else if (App.ConnectedVia.Contains("BT"))
                {
                    response = await DependencyService.Get<IBth>().SetData(viewModel.SenderCommand);
                }
                else if (App.ConnectedVia.Contains("WIFI"))
                {
                    response = await DependencyService.Get<IConnectionWifi>().SetData(viewModel.SenderCommand);
                }

                viewModel.DataList = new ObservableCollection<DataClass>();

                DataList.Add(
                    new DataClass
                    {
                        ReceiveCommand = $"{DateTime.Now.ToString("hh:mm:ss.fff")} Dongle : {response}",
                        SendCommand = $"{sender_time} Tester : {viewModel.SenderCommand}",
                    });
                DataList.Reverse();
                foreach (var item in DataList.ToList())
                {
                    viewModel.DataList.Add(item);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", ex.Message+"\n\n"+ex.StackTrace, "Ok");
            }

        }
    }
}