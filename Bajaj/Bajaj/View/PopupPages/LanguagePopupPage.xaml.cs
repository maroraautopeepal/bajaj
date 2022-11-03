using Bajaj.Model;
using Bajaj.ViewModel;
using System;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LanguagePopupPage : ContentPage
    {
        LanguageViewModel viewModel;
        public LanguagePopupPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new LanguageViewModel();
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
                    Language_List.ItemsSource = viewModel.LaguageList;
                }
                else
                {
                    imgClose.IsVisible = true;
                    Language_List.ItemsSource = viewModel.LaguageList.Where(x => x.Language.ToLower().Contains(e.NewTextValue.ToLower()));
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
                //var item = (e.Item as LanguageModel).Language;
                //var result = await DisplayAlert("Alert",$"Are you sure you want to change your APP language to {item}","Ok","Cancel");
                //using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
                //{
                //    await Task.Delay(100);

                //    //var res = workShopGroupModel.CityList.FirstOrDefault(x => x.city == item);
                //    //MessagingCenter.Send<CityPopupPage, WorkCity>(this, "City", res);
                //    App.Current.MainPage = new NavigationPage(new LoginPage());
                //    await PopupNavigation.Instance.PopAsync();
                //}
            }
            catch (Exception ex)
            {
            }
        }

        private void check_Tapped(object sender, EventArgs e)
        {
            var selectedItem = (LanguageModel)((Grid)sender).BindingContext;

            foreach (var item in viewModel.LaguageList)
            {
                if (selectedItem.id == item.id)
                {
                    item.is_checked = true;
                }
                else
                {
                    item.is_checked = false;
                }
            }

        }

        private void btnSubmit_Clicked(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}