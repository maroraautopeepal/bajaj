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

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CityPopupPage : PopupPage
    {
        CityViewModel viewModel;
        WorkShopGroupModel workShopGroupModel;
        public CityPopupPage(WorkShopGroupModel workShopGroupModel)
        {
            try
            {
                InitializeComponent();
                viewModel = new CityViewModel();
                this.workShopGroupModel = new WorkShopGroupModel();
                this.workShopGroupModel = workShopGroupModel;
                BindingContext = viewModel = new CityViewModel();
                foreach (var item in this.workShopGroupModel.CityList)
                {
                    viewModel.CityList.Add(
                        new CityModel
                        {
                            City = item.city
                        });
                }
                //viewModel.CityList = workShopGroupModel;
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
                if (string.IsNullOrEmpty(txtSearch.Text))
                {
                    imgClose.IsVisible = false;
                    CityList.ItemsSource = viewModel.CityList;
                }
                else
                {
                    imgClose.IsVisible = true;
                    CityList.ItemsSource = viewModel.CityList.Where(x => x.City.ToLower().Contains(e.NewTextValue.ToLower()));
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
                    var item = (e.Item as CityModel).City;
                    var res = workShopGroupModel.CityList.FirstOrDefault(x => x.city == item);
                    MessagingCenter.Send<CityPopupPage, WorkCity>(this, "City", res);
                    await PopupNavigation.Instance.PopAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}