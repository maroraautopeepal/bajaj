using Bajaj.Model;
using Bajaj.ViewModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SubModelPopupPage : PopupPage
    {
        VehicleSubModelViewModel viewModel;
        public SubModelPopupPage(List<SubModel> Models)
        {
            try
            {
                InitializeComponent();
                BindingContext = viewModel = new VehicleSubModelViewModel(Models);
                //viewModel.vehicle_sub_model_list = subModels;
            }
            catch (Exception ex)
            {

            }
        }

        private void CustomEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                vehicle_sub_model_List.ItemsSource = viewModel.vehicle_sub_model_list.Where(x => x.name.ToLower().Contains(e.NewTextValue.ToLower()));
            }
            catch (Exception ex)
            {
            }
        }

        private async void vehicle_sub_model_List_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                //var selected_sub_model = (e.Item as SubModel);
                var selected_model = (e.Item as ModelNames);
                MessagingCenter.Send<SubModelPopupPage, String>(this, "selected_vehicle_model", selected_model.name);
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