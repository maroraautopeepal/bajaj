using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.Popup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IsExpertUWPSession : PopupPage
    {
        private TaskCompletionSource<bool> taskCompletionSource;
        public Task<bool> PopupClosedTask { get { return taskCompletionSource.Task; } }       
        public IsExpertUWPSession(bool ActiveOrNot)
        {
            try
            {
                InitializeComponent();
                taskCompletionSource = new TaskCompletionSource<bool>();
                if (ActiveOrNot == true)
                {
                    taskCompletionSource.SetResult(false);
                    PopupNavigation.Instance.PopAllAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }
        protected override bool OnBackgroundClicked()
        {
            return false;
        }
    }
}