using Bajaj.Model;
using Bajaj.ViewModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModelPopupPage : PopupPage
    {
        VehicleModelViewModel viewModel;
        public ModelPopupPage()
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new VehicleModelViewModel();
            }
            catch (Exception ex)
            {

            }
        }

        private void CustomEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                vehicle_model_List.ItemsSource = viewModel.vehicle_model_list.Where(x => x.name.ToLower().Contains(e.NewTextValue.ToLower()));
            }
            catch (Exception ex)
            {
            }
        }

        private async void vehicle_model_List_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var selected_vehicle_model_year = (e.Item as ModelResult);
                MessagingCenter.Send<ModelPopupPage, ModelResult>(this, "select_vehicle_model_year", selected_vehicle_model_year);
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