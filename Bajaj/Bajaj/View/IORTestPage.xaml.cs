
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IORTestPage : DisplayAlertPage
    {
        public IORTestPage()
        {
            InitializeComponent();
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