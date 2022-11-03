using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bajaj.View.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IsExpertByRemoteOnUWP : PopupPage
    {
        private readonly Action<bool> setResultAction;

        public void CancelAttendanceClicked(object sender, EventArgs e)
        {
            setResultAction?.Invoke(false);
            this.Navigation.PopPopupAsync().ConfigureAwait(false);
        }

        public void ConfirmAttendanceClicked(object sender, EventArgs e)
        {
            setResultAction?.Invoke(true);
            this.Navigation.PopPopupAsync().ConfigureAwait(false);
        }

        public IsExpertByRemoteOnUWP(Action<bool> setResultAction)
        {
            InitializeComponent();
            this.setResultAction = setResultAction;
        }
        //public IsExpertByRemoteOnUWP()
        //{
        //    InitializeComponent();
        //}
        //public static async Task<bool> ConfirmConferenceAttendance(INavigation navigation)
        //{
        //    TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

        //    void callback(bool didConfirm)
        //    {
        //        completionSource.TrySetResult(didConfirm);
        //    }

        //    var popup = new IsExpertByRemoteOnUWP(callback);

        //    await navigation.PushPopupAsync(popup);

        //    return await completionSource.Task;
        //}
        protected override bool OnBackgroundClicked()
        {
            return false;
        }
    }
}