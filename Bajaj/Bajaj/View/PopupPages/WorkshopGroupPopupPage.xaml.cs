﻿using Acr.UserDialogs;
using Bajaj.Model;
using Bajaj.ViewModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkshopGroupPopupPage : PopupPage
    {
        WorkShopGroupViewModel viewModel;
        public WorkshopGroupPopupPage()
        {
            //using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            //{
            InitializeComponent();
            //viewModel = new WorkShopGroupViewModel();
            BindingContext = viewModel = new WorkShopGroupViewModel();
            //};
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
                    WorkShopGroupList.ItemsSource = viewModel.WorkShopGroupList;
                }
                else
                {
                    imgClose.IsVisible = true;
                    //WorkShopGroupList.ItemsSource = viewModel.WorkShopGroupList.Where(x => x.WorkShopGroup.ToLower().Contains(e.NewTextValue.ToLower()));
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
                    var item = (e.Item as WorkShopGroupModel);//.workshopsGroupName;
                    MessagingCenter.Send<WorkshopGroupPopupPage, WorkShopGroupModel>(this, "WorkShopGroup", item);
                    await PopupNavigation.Instance.PopAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}