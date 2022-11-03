using Newtonsoft.Json;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.View.ViewModel;
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
    public partial class OemPopupPage : PopupPage
    {
        OemViewModel viewModel;

        public OemPopupPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new OemViewModel();
        }

        private void CustomEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Oem_list.ItemsSource = viewModel.oem_list.Where(x => x.name.ToLower().Contains(e.NewTextValue.ToLower()));
            }
            catch (Exception ex)
            {
            }
        }

        private async void oem_List_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                App.selectedOem = (e.Item as AllOemModel);
                MessagingCenter.Send<OemPopupPage, AllOemModel>(this, "select_oem", App.selectedOem);
                //var data = JsonConvert.SerializeObject(App.selectedOem);
                //await DependencyService.Get<ISaveLocalData>().SaveData("selctedOemModel", data);
                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
            }
        }

        private async void close_popup(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }

    
}