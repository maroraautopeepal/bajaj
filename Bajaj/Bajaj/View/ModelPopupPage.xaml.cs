using Acr.UserDialogs;
using Bajaj.Model;
using Bajaj.ViewModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModelPopupPage : PopupPage
    {
        VehiclePopupViewModel viewModel;
        public ModelPopupPage()
        {
            InitializeComponent();
            viewModel = new VehiclePopupViewModel();
            BindingContext = viewModel = new VehiclePopupViewModel();
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
                if (string.IsNullOrEmpty(txtSearch.Text))
                {
                    imgClose.IsVisible = false;
                    modelList.ItemsSource = viewModel.ModelList;
                }
                else
                {
                    imgClose.IsVisible = true;
                    modelList.ItemsSource = viewModel.ModelList.Where(x => x.ModelName.ToLower().Contains(e.NewTextValue.ToLower()));
                }
            }
            catch (Exception ex)
            {
            }

        }

        private async void modelList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                {
                    await Task.Delay(100);
                    var item = (e.Item as VehiclePopModel).ModelName;
                    //await this.Navigation.PushAsync(new CreateJobCardPage(item));
                    await PopupNavigation.Instance.PopAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}