using Bajaj.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.GdSection
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageZoomingPage : DisplayAlertPage
    {
        public ImageZoomingPage(GdImageGD gdImage, string code)
        {
            InitializeComponent();
            this.Title = code;
            img.Source = gdImage.gd_image;
            this.Title = gdImage.image_name;
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