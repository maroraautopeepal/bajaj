using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChangePasswordPage : PopupPage
    {
        public ChangePasswordPage(string firmware)
        {
            InitializeComponent();
        }
    }
}