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
    public partial class WorkShopPopupPage : PopupPage
    {
        WorkShopViewModel viewModel;
        public WorkShopPopupPage(WorkCity workCity)
        {
            InitializeComponent();
            viewModel = new WorkShopViewModel();
            BindingContext = viewModel = new WorkShopViewModel();
            foreach (var item in workCity.workshops)
            {
                viewModel.WorkShopList.Add(
                    new WorkShopModel
                    {
                        Workshop = item.name,
                        WorkshopId = item.id,
                    });
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
                    WorkShopList.ItemsSource = viewModel.WorkShopList;
                }
                else
                {
                    imgClose.IsVisible = true;
                    WorkShopList.ItemsSource = viewModel.WorkShopList.Where(x => x.Workshop.ToLower().Contains(e.NewTextValue.ToLower()));
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
                    var item = (e.Item as WorkShopModel);
                    MessagingCenter.Send<WorkShopPopupPage, WorkShopModel>(this, "WorkShop", item);
                    await PopupNavigation.Instance.PopAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}