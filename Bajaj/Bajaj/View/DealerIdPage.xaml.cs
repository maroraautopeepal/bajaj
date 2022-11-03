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
    public partial class DealerIdPage : ContentPage
    {
        DealerIdViewModel viewModel;
        public DealerIdPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new DealerIdViewModel(this);
        }
    }
}