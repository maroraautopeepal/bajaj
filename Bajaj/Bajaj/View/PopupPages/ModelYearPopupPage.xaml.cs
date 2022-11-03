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
    public partial class ModelYearPopupPage : PopupPage
    {
        VehicleModelYearViewModel viewModel;
        public ModelYearPopupPage(string selectedModel, List<SubModel> subModels)
        {
            InitializeComponent();
            BindingContext = viewModel = new VehicleModelYearViewModel(selectedModel, subModels);
            //viewModel.vehicle_model_year_list.Add(new VehicleModelYearModel { vehicle_model_year = model_year });
        }

        private void CustomEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                vehicle_model_year_List.ItemsSource = viewModel.vehicle_model_year_list.Where(x => x.vehicle_model_year.ToLower().Contains(e.NewTextValue.ToLower()));
            }
            catch (Exception ex)
            {
            }
        }

        private async void vehicle_model_year_List_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var selected_model_year = (e.Item as SubModel);
                MessagingCenter.Send<ModelYearPopupPage, SubModel>(this, "selected_vehicle_submodel", selected_model_year);
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