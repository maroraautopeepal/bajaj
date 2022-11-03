using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DisplayAlertPage : ContentPage
    {
        public DisplayAlertPage()
        {
            BindingContext = this;
            InitializeComponent();
        }
        private ICommand cancelCommand;
        private ICommand okCommand;
        private bool working;
        private bool cancelVisible = false;
        private bool okVisible = false;
        private string messageText;
        private string titleText;

        public ICommand OkCommand
        {
            get => okCommand;
            set
            {
                okCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand CancelCommand
        {
            get => cancelCommand;
            set
            {
                cancelCommand = value;
                OnPropertyChanged();
            }
        }

        public bool OkVisible
        {
            get => okVisible;
            set
            {
                okVisible = value;
                OnPropertyChanged();
            }
        }

        public bool CancelVisible
        {
            get => cancelVisible;
            set
            {
                cancelVisible = value;
                OnPropertyChanged();
            }
        }

        public bool Working
        {
            get => working;
            set
            {
                working = value;
                OnPropertyChanged();
            }
        }

        public string TitleText
        {
            get => titleText;
            set
            {
                titleText = value;
                OnPropertyChanged();
            }
        }

        public string MessageText
        {
            get => messageText;
            set
            {
                messageText = value;
                OnPropertyChanged();
            }
        }
    }
}