using Android.App;
using Android.Graphics.Drawables;
using Android.Views;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using Bajaj.Popup;
using Plugin.CurrentActivity;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XFPlatform = Xamarin.Forms.Platform.Android.Platform;

[assembly: Xamarin.Forms.Dependency(typeof(LodingPageServiceDroid))]
namespace Bajaj.Droid.Dependencies
{
    public class LodingPageServiceDroid : ILodingPageService
    {
        private Android.Views.View _nativeView;

        private Android.Views.View _ExpertnativeView;

        private Dialog _dialog;
        private Dialog _IsExpertdialog;

        private bool _isInitialized;
        private bool _IsExpertTrueOrNot;

        public void InitLoadingPage(ContentPage loadingIndicatorPage)
        {
            try
            {
                // check if the page parameter is available
                if (loadingIndicatorPage != null)
                {
                    // build the loading page with native base
                    loadingIndicatorPage.Parent = Xamarin.Forms.Application.Current.MainPage;

                    loadingIndicatorPage.Layout(new Rectangle(0, 0,
                        Xamarin.Forms.Application.Current.MainPage.Width,
                        Xamarin.Forms.Application.Current.MainPage.Height));

                    var renderer = loadingIndicatorPage.GetOrCreateRenderer();

                    _nativeView = renderer.View;

                    _dialog = new Dialog(CrossCurrentActivity.Current.Activity);
                    _dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                    _dialog.SetCancelable(false);
                    _dialog.SetContentView(_nativeView);
                    Window window = _dialog.Window;
                    window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                    window.ClearFlags(WindowManagerFlags.DimBehind);
                    window.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));

                    _isInitialized = true;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void ShowLoadingPage()
        {
            // check if the user has set the page or not
            if (!_isInitialized)
                InitLoadingPage(new LoadingIndicatorPage()); // set the default page

            // showing the native loading page
            _dialog.Show();
        }

        private void XamFormsPage_Appearing(object sender, EventArgs e)
        {
            try
            {
                var animation = new Animation(callback: d => ((ContentPage)sender).Content.Rotation = d,
                                          start: ((ContentPage)sender).Content.Rotation,
                                          end: ((ContentPage)sender).Content.Rotation + 360,
                                          easing: Easing.Linear);
                animation.Commit(((ContentPage)sender).Content, "RotationLoopAnimation", 16, 800, null, null, () => true);
            }
            catch (Exception ex)
            {

            }
        }

        public void HideLoadingPage()
        {
            if (_isInitialized)
            {
                // Hide the page
                _dialog.Hide();
            }
        }

        public void IsExpert(ContentPage ExpertPage)
        {
            try
            {
                // check if the page parameter is available
                if (ExpertPage != null)
                {
                    // build the loading page with native base
                    ExpertPage.Parent = Xamarin.Forms.Application.Current.MainPage;

                    ExpertPage.Layout(new Rectangle(0, 0,
                        Xamarin.Forms.Application.Current.MainPage.Width,
                        Xamarin.Forms.Application.Current.MainPage.Height));

                    var renderer = ExpertPage.GetOrCreateRenderer();

                    _ExpertnativeView = renderer.View;

                    _IsExpertdialog = new Dialog(CrossCurrentActivity.Current.Activity);
                    _IsExpertdialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                    _IsExpertdialog.SetCancelable(false);
                    _IsExpertdialog.SetContentView(_ExpertnativeView);
                    Window window = _IsExpertdialog.Window;
                    window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                    window.ClearFlags(WindowManagerFlags.DimBehind);
                    window.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));
                    _IsExpertTrueOrNot = true;
                }
            }
            catch (Exception ex)
            {

            }            
        }

        public void IsExpertShow_Active()
        {
            try
            {
                // check if the user has set the page or not
                if (!_IsExpertTrueOrNot)
                    IsExpert(new ExpertSession()); // set the default page

                // showing the native loading page
                _IsExpertdialog.Show();
            }
            catch (Exception ex)
            {

            }   
        }
        public void IsExpertShow_NotActive()
        {
            if (_IsExpertTrueOrNot)
            {
                // Hide the page
                _IsExpertdialog.Hide();
            }
        }
    }

    internal static class PlatformExtension
    {
        public static IVisualElementRenderer GetOrCreateRenderer(this VisualElement bindable)
        {
            var renderer = XFPlatform.GetRenderer(bindable);
            if (renderer == null)
            {
                renderer = XFPlatform.CreateRendererWithContext(bindable, CrossCurrentActivity.Current.Activity);
                XFPlatform.SetRenderer(bindable, renderer);
            }
            return renderer;
        }
    }
}