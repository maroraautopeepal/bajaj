using SML.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Forms;

namespace SML.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new SML.App());

            MessagingCenter.Subscribe<JobCardPage, string>(this, "Expert", async (sender, arg) =>
            {
                //AllScreen.IsOpen = true;
                //IsContentDialog signInDialog = new IsContentDialog();
                //await signInDialog.ShowAsync();


                Windows.UI.Popups.MessageDialog showDialog = new Windows.UI.Popups.MessageDialog("UWP message dialog without title - Windows 10");
                showDialog.Commands.Add(new UICommand("Yes")
                {
                    Id = 0
                });
                showDialog.Commands.Add(new UICommand("No")
                {
                    Id = 1
                });
                showDialog.DefaultCommandIndex = 0;
                showDialog.CancelCommandIndex = 1;
                var result = await showDialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    //do your task
                }
                else
                {
                    //skip your task
                }
            });
        }
    }
}
