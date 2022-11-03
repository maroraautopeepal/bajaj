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
    public partial class ConnectedPage : ContentPage
    {
        ConnectedPageViewModel viewModel;
        public ConnectedPage(string firmwareVirsion)
        {
            InitializeComponent();
            BindingContext = viewModel = new ConnectedPageViewModel(firmwareVirsion, this);
        }
    }
}