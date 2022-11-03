using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectorPopupPage : PopupPage
    {
        public ConnectorPopupPage()
        {
            InitializeComponent();
            //connectorImage.Source = App.connector_type_URL;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}