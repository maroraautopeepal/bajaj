using Bajaj.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OtpPage : ContentPage
    {
        OtpViewModel viewModel;
        public OtpPage(string otpType)
        {
            InitializeComponent();
            BindingContext = viewModel = new OtpViewModel(otpType, this);
        }
    }
}