using Bajaj.ViewModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModelListPopupPage : PopupPage
    {
        ModelListPopupViewModel viewModel;
        public string SelectedModelType;
        public ModelListPopupPage(string ModelType)
        {
            try
            {
                InitializeComponent();
                SelectedModelType = ModelType;
                BindingContext = viewModel = new ModelListPopupViewModel(SelectedModelType);
            }
            catch (Exception ex)
            {
            }
        }

        private void CloseClick(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            imgClose.IsVisible = false;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtSearch.Text))
                //{
                //    imgClose.IsVisible = false;
                //    modelList.ItemsSource = viewModel.ModelList;
                //}
                //else
                //{
                //    imgClose.IsVisible = true;
                //    modelList.ItemsSource = viewModel.ModelList.Where(x => x.ModelName.ToLower().Contains(e.NewTextValue.ToLower()));

                //}
            }
            catch (Exception ex)
            {
            }

        }

        private async void modelList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var item = (e.Item as ModelNameClass);
                App.SelectedModel = item.ModelName;
                string[] variable_name = new string[2];
                variable_name[0] = item.ModelName;
                variable_name[1] = Convert.ToString(item.id);
                MessagingCenter.Send<ModelListPopupPage, string[]>(this, "ModelName", variable_name);
                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
            }
        }
    }
}