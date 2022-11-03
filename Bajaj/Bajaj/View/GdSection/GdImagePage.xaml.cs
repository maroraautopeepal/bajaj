using Bajaj.Model;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.GdSection
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GdImagePage : DisplayAlertPage
    {
        public List<GdImageGD> gd_image = new List<GdImageGD>();

        public GdImagePage(List<GdImageGD> gdimage, string Code)
        {
            try
            {
                InitializeComponent();
                this.Title = Code;
                images.ItemsSource = gd_image = gdimage;
            }
            catch (Exception ex)
            {
            }
        }

        private void images_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection as GdImageGD;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                var selectedItem = (GdImageGD)((Image)sender).BindingContext;
                this.Navigation.PushAsync(new ImageZoomingPage(selectedItem, this.Title));
            }
            catch (Exception ex)
            {
            }
        }

        public void show_alert(string title, string message, bool btnCancel, bool btnOk)
        {
            Working = true;
            TitleText = title;
            MessageText = message;
            OkVisible = btnOk;
            CancelVisible = btnCancel;
            CancelCommand = new Command(() =>
            {
                Working = false;
            });
        }
    }
}