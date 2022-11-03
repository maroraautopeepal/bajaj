using SML.Model;
using SML.Services;
using MultiEventController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Popup = Rg.Plugins.Popup.Popup;

namespace SML.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 

        ApiServices services = new ApiServices();
        public static ControlEventManager controlEventManager = null;

        public static string MasterLoginUserBY = string.Empty;
        public static string MasterLoginUserRoleBY = string.Empty;
        public static string UserName = string.Empty;
        public static int LoginUserID = 0;

        public static string JwtToken = string.Empty;
        public static string SelectedModel = string.Empty;
        public static double ScreenHeight = 0;
        public static string SessionId = string.Empty;
        public static string ConnectedVia = string.Empty;
        public static bool NetConnected = true;
        public static int model_id = 0;
        public static int sub_model_id = 0;
        public static bool is_login = false;
        public static ObservableCollection<BluetoothDevicesModel> bluetoothDeviceds = new ObservableCollection<BluetoothDevicesModel>();
        public static List<string> ecu_list = new List<string>();
        public static string firmwareVersion = string.Empty;
        public static bool PageFreezIsEnable = false;


        public static AllModelsModel ForOfflineJobCardCreate;

        public static bool PageISTrueOrNot = false;

        public static bool ContentPageIsEnabled { get; set; }
        public static bool NotGoBack = true;
        public static bool PageOneToOtherGoBack = false;
        public static bool ExpertIsNotConnected = true;

        public static bool IsExpert = false;
        public static List<ResponseJobCardModel> IsSameResponce = null;
        public static bool isRunningTimer = false;
        public static bool DCTPage = true;
        public static bool INFOPage = true;
        public static bool TreeSurveyPage = true;
        public static bool ControlBYExpert = true;
        public static bool Notification = true;
        public static bool TreeL = true;
        public static JobCardListModel JCM;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                Windows.UI.Xaml.Application.Current.UnhandledException += Current_UnhandledException;
                Rg.Plugins.Popup.Popup.Init();
                //Popup.Init();
                
                Xamarin.Forms.Forms.Init(e);

                SML.App.ScreenHeight = Window.Current.Bounds.Height;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void Current_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
            //throw new Exception("Failed to load Page " + e.Message);
        }

            /// <summary>
            /// Invoked when Navigation to a certain page fails
            /// </summary>
            /// <param name="sender">The Frame which failed navigation</param>
            /// <param name="e">Details about the navigation failure</param>
            void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
