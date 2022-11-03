using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.Locations;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Bajaj.Droid.Dependencies;
using Bajaj.View;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Bajaj.View.TerminalPart;
using Rg.Plugins.Popup.Services;
using System.Net;
using Android.Gms.Common;
using Bajaj.View.ViewModel;

namespace Bajaj.Droid
{
    [Activity(Label = "Bajaj", Icon = "@drawable/ic_logo", Theme = "@style/MainTheme", MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        static readonly string TAG = typeof(MainActivity).Name;
        const string ACTION_USB_PERMISSION = "com.hoho.android.usbserial.examples.USB_PERMISSION";

        public static MainActivity Instance { get; private set; }

        UsbManager usbManager;
        BroadcastReceiver detachedReceiver;
        UsbSerialPort port;
        List<UsbSerialPort> portList = new List<UsbSerialPort>();

        //public const string EXTRA_TAG = "PortInfo";
        //const int READ_WAIT_MILLIS = 200;
        const int WRITE_WAIT_MILLIS = 200;
        //byte[] seq_command = new byte[] { 0x50, 0x0C, 0x47, 0x56, 0x8A, 0xFE, 0x56, 0x21, 0x4E, 0x23, 0x80, 0x00, 0xFF, 0xC3 };
        //string command = "200402" + protocolVersion.ToString("D2");
        //byte[] protocol_command = new byte[] { 0x20, 0x04, 0x4, 0x02 };

        SerialInputOutputManager serialIoManager;

        //private BluetoothDeviceReceiver _receiver;
        private static MainActivity _instance;

        public static MainActivity GetInstance()
        {
            return _instance;
        }

        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {

                TabLayoutResource = SML.Droid.Resource.Layout.Tabbar;
                ToolbarResource = SML.Droid.Resource.Layout.Toolbar;

                ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

                Instance = this;

                var scale = Resources.DisplayMetrics.Density;//density i.e., pixels per inch or cms  
                                                             //var widthPixels = Resources.DisplayMetrics.WidthPixels;
                                                             //App. = (double)((widthPixels - 0.5f) / scale);
                var heightPixels = Resources.DisplayMetrics.HeightPixels;////getting the height in pixels  
                App.ScreenHeight = (double)((heightPixels - 0.5f) / scale);

                base.OnCreate(savedInstanceState);

                _instance = this;

#if DEBUG
                //Task.Run(() => { FirebaseInstanceId.Instance.DeleteInstanceId(); });
#endif
                //Fotax.Model.NotificationModel model = null;
                if (Intent.Extras != null)
                {
                    try
                    {
                        //model = new Model.NotificationModel();
                        //model.title = Intent.Extras.GetString("title");
                        //model.vehicle = Intent.Extras.GetString("vehicle");
                        //model.lat = Intent.Extras.GetString("lat");
                        //model.lon = Intent.Extras.GetString("lon");
                        //model.number = Intent.Extras.GetString("number");
                        //model.altitude = Intent.Extras.GetString("altitude");
                        //model.speed = Intent.Extras.GetString("speed");
                        //model.distance = Intent.Extras.GetString("distance");
                        //model.latDir = Intent.Extras.GetString("latDir");
                        //model.lonDir = Intent.Extras.GetString("lonDir");
                        //if (string.IsNullOrEmpty(model.title))
                        //{
                        //    model = null;
                        //}
                    }
                    catch (Exception ex)
                    {
                    }
                }

                IsPlayServicesAvailable();

                CreateNotificationChannel();

                ConnectivityManager connection_manager = (ConnectivityManager)this.GetSystemService(Context.ConnectivityService);

                usbManager = GetSystemService(Context.UsbService) as UsbManager;

                //    this.RequestPermissions(new[]
                //    {
                //    Manifest . Permission . AccessCoarseLocation ,
                //    Manifest . Permission . AccessFineLocation ,
                //    Manifest . Permission . BluetoothPrivileged,
                //    Manifest.Permission.ReadExternalStorage,
                //    Manifest.Permission.WriteExternalStorage,
                //    Manifest.Permission.ReadPhoneState,
                //}, 0);

                const int locationPermissionsRequestCode = 1000;


                var Locaionp = new[]
                {
                Manifest . Permission . AccessCoarseLocation ,
                Manifest . Permission . AccessFineLocation ,
                Manifest . Permission . BluetoothPrivileged,
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage,
                Manifest.Permission.ReadPhoneState
                };

                // IsGpsEnable();

                //Intent intentt = new Intent(Android.Provider.Settings.ActionLocat‌​ionSourceSettings);
                //intentt.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

                var coarseLocationPermissionGranted =
               ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation);
                // check if the app has permission to access fine location
                var fineLocationPermissionGranted =
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation);
                // if either is denied permission, request permission from the user
                if (coarseLocationPermissionGranted == Permission.Denied ||
                    fineLocationPermissionGranted == Permission.Denied)
                {
                    ActivityCompat.RequestPermissions(this, Locaionp, locationPermissionsRequestCode);
                }

                //_receiver = new BluetoothDeviceReceiver();

                Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
                //Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
                Rg.Plugins.Popup.Popup.Init(this);

                UserDialogs.Init(this);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);
                global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

                ToggleScreenLock();

                LoadApplication(new App());

                MessagingCenter.Subscribe<ConnectionViewModel, string>(this, "connect_usb", async (sende, arg) =>
                        {
                            try
                            {

                                var portInfo = new UsbSerialPortInfo
                                {
                                    VendorId = 4292,
                                    PortNumber = portList.FirstOrDefault().PortNumber,
                                };

                                int vendorId = portInfo.VendorId;

                                int deviceId = portInfo.DeviceId;

                                int portNumber = portInfo.PortNumber;

                                Log.Info(TAG, string.Format("VendorId: {0} DeviceId: {1} PortNumber: {2}", vendorId, deviceId, portNumber));

                                // The following arrays contain data that is used for a custom firmware for
                                // the Elatec TWN4 RFID reader. This code is included here to show how to
                                // send data back to a USB serial device
                                // byte[] sleepdata = new byte[] { 0xf0, 0x04, 0x10, 0xf1 };
                                // byte[] wakedata = new byte[] { 0xf0, 0x04, 0x11, 0xf1 };

                                byte[] wakedata = new byte[] { };

                                //await PopulateListAsync();

                                //register the broadcast receivers


                                var drivers = await FindAllDriversAsync(usbManager);

                                var driver = drivers.Where((d) => d.Device.VendorId == vendorId).FirstOrDefault();
                                if (driver == null)
                                    throw new Exception("Driver specified in extra tag not found.");
                                string DeviceProductName = driver.Device.ProductName;

                                port = driver.Ports[portNumber];
                                if (port == null)
                                {
                                    DependencyService.Get<Interfaces.IToastMessage>().Show("No serial device.");
                                    Console.WriteLine("No serial device.");
                                    return;
                                }
                                Log.Info(TAG, "port=" + port);

                                Console.WriteLine("Serial device: " + port.GetType().Name);
                                permission();
                                //serialIoManager = new SerialInputOutputManager(port)
                                //{
                                //    BaudRate = 115200,
                                //    DataBits = 8,
                                //    StopBits = StopBits.One,
                                //    Parity = Parity.None,
                                //};
                                if (DeviceProductName.Contains("Bajaj") || DeviceProductName.Contains("Autopeepal") || DeviceProductName.Contains("Eicher"))
                                {
                                    serialIoManager = new SerialInputOutputManager(port)
                                    {
                                        BaudRate = 460800,
                                        DataBits = 8,
                                        StopBits = StopBits.One,
                                        Parity = Parity.None,
                                    };

                                    DependencyService.Get<Interfaces.IToastMessage>().Show("BaudRate Is 460800");
                                }
                                else
                                {
                                    serialIoManager = new SerialInputOutputManager(port)
                                    {
                                        BaudRate = 115200,
                                        DataBits = 8,
                                        StopBits = StopBits.One,
                                        Parity = Parity.None,
                                    };
                                    serialIoManager.ErrorReceived += SerialIoManager_ErrorReceived;
                                    DependencyService.Get<Interfaces.IToastMessage>().Show("BaudRate Is 115200");
                                }

                                serialIoManager.DataReceived += (sender, e) =>
                                {
                                    RunOnUiThread(() =>
                                    {
                                        UpdateReceivedData(e.Data);
                                    });
                                };
                                serialIoManager.ErrorReceived += (sender, e) =>
                                {
                                    RunOnUiThread(() =>
                                    {
                                        var intent = new Intent(this, typeof(MainActivity));
                                        StartActivity(intent);
                                    });
                                };



                                //serialIoManager.DataReceived += (sender, e) =>
                                //{
                                //    RunOnUiThread(() =>
                                //    {
                                //        UpdateReceivedData(e.Data);
                                //    });
                                //};
                                //serialIoManager.ErrorReceived += (sender, e) =>
                                //{
                                //    RunOnUiThread(() =>
                                //    {
                                //        var intent = new Intent(this, typeof(MainActivity));
                                //        StartActivity(intent);
                                //    });
                                //};


                                Log.Info(TAG, "Starting IO manager ..");
                                try
                                {
                                    serialIoManager.Open(usbManager);
                                }
                                catch (Java.IO.IOException e)
                                {
                                    Console.WriteLine("Error opening device: " + e.Message);
                                    return;
                                }
                                WriteData(wakedata);

                            }
                            catch (Exception ex)
                            {
                            }
                        });

                #region Terminal USB Connection
                MessagingCenter.Subscribe<LinkDonglePage, string>(this, "connect_usb", async (sende, arg) =>
                {
                    try
                    {

                        var portInfo = new UsbSerialPortInfo
                        {
                            VendorId = 4292,
                            PortNumber = portList.FirstOrDefault().PortNumber,
                        };

                        int vendorId = portInfo.VendorId;

                        int deviceId = portInfo.DeviceId;

                        int portNumber = portInfo.PortNumber;

                        Log.Info(TAG, string.Format("VendorId: {0} DeviceId: {1} PortNumber: {2}", vendorId, deviceId, portNumber));

                        // The following arrays contain data that is used for a custom firmware for
                        // the Elatec TWN4 RFID reader. This code is included here to show how to
                        // send data back to a USB serial device
                        // byte[] sleepdata = new byte[] { 0xf0, 0x04, 0x10, 0xf1 };
                        // byte[] wakedata = new byte[] { 0xf0, 0x04, 0x11, 0xf1 };

                        byte[] wakedata = new byte[] { };

                        //await PopulateListAsync();

                        //register the broadcast receivers


                        var drivers = await FindAllDriversAsync(usbManager);

                        var driver = drivers.Where((d) => d.Device.VendorId == vendorId).FirstOrDefault();
                        if (driver == null)
                            throw new Exception("Driver specified in extra tag not found.");
                        string DeviceProductName = driver.Device.ProductName;

                        port = driver.Ports[portNumber];
                        if (port == null)
                        {
                            DependencyService.Get<Interfaces.IToastMessage>().Show("No serial device.");
                            Console.WriteLine("No serial device.");
                            return;
                        }
                        Log.Info(TAG, "port=" + port);

                        Console.WriteLine("Serial device: " + port.GetType().Name);
                        permission();
                        //serialIoManager = new SerialInputOutputManager(port)
                        //{
                        //    BaudRate = 115200,
                        //    DataBits = 8,
                        //    StopBits = StopBits.One,
                        //    Parity = Parity.None,
                        //};
                        if (DeviceProductName.Contains("Bajaj") || DeviceProductName.Contains("Autopeepal") || DeviceProductName.Contains("Eicher"))
                        {
                            serialIoManager = new SerialInputOutputManager(port)
                            {
                                BaudRate = 460800,
                                DataBits = 8,
                                StopBits = StopBits.One,
                                Parity = Parity.None,
                            };

                            DependencyService.Get<Interfaces.IToastMessage>().Show("BaudRate Is 460800");
                        }
                        else
                        {
                            serialIoManager = new SerialInputOutputManager(port)
                            {
                                BaudRate = 115200,
                                DataBits = 8,
                                StopBits = StopBits.One,
                                Parity = Parity.None,
                            };
                            serialIoManager.ErrorReceived += SerialIoManager_ErrorReceived;
                            DependencyService.Get<Interfaces.IToastMessage>().Show("BaudRate Is 115200");
                        }

                        serialIoManager.DataReceived += (sender, e) =>
                        {
                            RunOnUiThread(() =>
                            {
                                UpdateReceivedData(e.Data);
                            });
                        };
                        serialIoManager.ErrorReceived += (sender, e) =>
                        {
                            RunOnUiThread(() =>
                            {
                                var intent = new Intent(this, typeof(MainActivity));
                                StartActivity(intent);
                            });
                        };



                        //serialIoManager.DataReceived += (sender, e) =>
                        //{
                        //    RunOnUiThread(() =>
                        //    {
                        //        UpdateReceivedData(e.Data);
                        //    });
                        //};
                        //serialIoManager.ErrorReceived += (sender, e) =>
                        //{
                        //    RunOnUiThread(() =>
                        //    {
                        //        var intent = new Intent(this, typeof(MainActivity));
                        //        StartActivity(intent);
                        //    });
                        //};


                        Log.Info(TAG, "Starting IO manager ..");
                        try
                        {
                            serialIoManager.Open(usbManager);
                        }
                        catch (Java.IO.IOException e)
                        {
                            Console.WriteLine("Error opening device: " + e.Message);
                            return;
                        }
                        WriteData(wakedata);

                    }
                    catch (Exception ex)
                    {
                    }
                });
                #endregion

                //MessagingCenter.Subscribe<JobCardPage, string>(this, "Notification", async (sender, arg) =>
                //{
                //    StartService(new Intent(this, typeof(BackgroundService)));
                //});
            }
            catch (Exception ex)
            {
            }
        }
        public void ToggleScreenLock()
        {
            DeviceDisplay.KeepScreenOn = true;
        }
        private void SerialIoManager_ErrorReceived(object sender, UnhandledExceptionEventArgs e)
        {

        }

        public UsbSerialPort ReturnPort()
        {
            return port;
        }

        public SerialInputOutputManager inputOutputManager()
        {
            return serialIoManager;
        }

        public async void permission()
        {
            var permissionGranted = await usbManager.RequestPermissionAsync(port.Driver.Device, this);
            if (permissionGranted)
            {
                //WriteData(seq_command);
                //WriteData(protocol_command);
            }
        }

        public bool IsGpsEnable()
        {
            LocationManager locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
            return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
        }
        void WriteData(byte[] data)
        {
            var byteData = new byte[] { };
            if (serialIoManager.IsOpen)
            {
                port.Write(data, WRITE_WAIT_MILLIS);

            }
        }

        void UpdateReceivedData(byte[] data)
        {
            //var message = "Read " + data.Length + " bytes: \n"
            //    + HexDump.DumpHexString(data) + "\n\n";

            //dumpTextView.Append(message);
            //scrollView.SmoothScrollTo(0, dumpTextView.Bottom);
        }

        protected override async void OnResume()
        {
            base.OnResume();
            await PopulateListAsync();

            //register the broadcast receivers
            detachedReceiver = new UsbDeviceDetachedReceiver(this);
            RegisterReceiver(detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
        }

        protected override void OnPause()
        {
            base.OnPause();

            // unregister the broadcast receivers
            var temp = detachedReceiver; // copy reference for thread safety
            if (temp != null)
                UnregisterReceiver(temp);
        }

        internal static async Task<IList<IUsbSerialDriver>> FindAllDriversAsync(UsbManager usbManager)
        {
            // using the default probe table
            // return UsbSerialProber.DefaultProber.FindAllDriversAsync (usbManager);

            // adding a custom driver to the default probe table
            var table = UsbSerialProber.DefaultProbeTable;
            table.AddProduct(0x1b4f, 0x0008, typeof(CdcAcmSerialDriver)); // IOIO OTG

            table.AddProduct(0x09D8, 0x0420, typeof(CdcAcmSerialDriver)); // Elatec TWN4

            var prober = new UsbSerialProber(table);
            return prober.FindAllDrivers(usbManager);
        }

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
                PopupNavigation.Instance.PopAsync();
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
        }

        async Task PopulateListAsync()
        {
            //ShowProgressBar();

            Log.Info(TAG, "Refreshing device list ...");

            var drivers = await FindAllDriversAsync(usbManager);

            //adapter.Clear();
            foreach (var driver in drivers)
            {
                var ports = driver.Ports;
                Log.Info(TAG, string.Format("+ {0}: {1} port{2}", driver, ports.Count, ports.Count == 1 ? string.Empty : "s"));
                foreach (var po in ports)
                {
                    portList.Add(po);
                }
            }

            //adapter.NotifyDataSetChanged();
            //progressBarTitle.Text = string.Format("{0} device{1} found", adapter.Count, adapter.Count == 1 ? string.Empty : "s");
            //HideProgressBar();
            //Log.Info(TAG, "Done refreshing, " + adapter.Count + " entries found.");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                { //msgText.Text = GoogleApiAvailability.Instance.GetErrorString(resultCode); 
                }
                else
                {
                    //msgText.Text = "This device is not supported";
                    Finish();
                }
                return false;
            }
            else
            {
                //msgText.Text = "Google Play Services is available.";
                return true;
            }
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID,
                                                  "FCM Notifications",
                                                  NotificationImportance.Default)
            {

                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        void AutoStartService()
        {
            try
            {
                string manufacturer = Android.OS.Build.Manufacturer;//android.os.Build.MANUFACTURER;

                Intent intent = new Intent();
                if ("Xiaomi".Equals(manufacturer))//equalsIgnoreCase(manufacturer))
                {
                    //intent.setComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity"));
                    intent.SetComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity"));
                }
                else if ("vivo".Equals(manufacturer))//.equalsIgnoreCase(manufacturer))
                {
                    intent.SetComponent(new ComponentName("com.vivo.permissionmanager", "com.vivo.permissionmanager.activity.BgStartUpManagerActivity"));
                }
                //else if ("oppo".equalsIgnoreCase(manufacturer))
                //{
                //    intent.setComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.permission.startup.StartupAppListActivity"));
                //}
                //else if ("vivo".equalsIgnoreCase(manufacturer))
                //{
                //    intent.setComponent(new ComponentName("com.vivo.permissionmanager", "com.vivo.permissionmanager.activity.BgStartUpManagerActivity"));
                //}
                //else if ("Letv".equalsIgnoreCase(manufacturer))
                //{
                //    intent.setComponent(new ComponentName("com.letv.android.letvsafe", "com.letv.android.letvsafe.AutobootManageActivity"));
                //}
                //else if ("Honor".equalsIgnoreCase(manufacturer))
                //{
                //    intent.setComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.optimize.process.ProtectActivity"));
                //}

                //List<ResolveInfo> list = getPackageManager().queryIntentActivities(intent, PackageManager.MATCH_DEFAULT_ONLY);
                //if (list.size() > 0)
                //{
                //    startActivity(intent);
                //}

                var res = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
                if (res.Count > 0)
                {
                    StartActivity(intent);
                }

            }
            catch (Exception e)
            {
                //e.printStackTrace();
            }
        }

        #region UsbDeviceDetachedReceiver implementation

        class UsbDeviceDetachedReceiver
            : BroadcastReceiver
        {
            readonly string TAG = typeof(UsbDeviceDetachedReceiver).Name;
            readonly MainActivity activity;

            public UsbDeviceDetachedReceiver(MainActivity activity)
            {
                this.activity = activity;
            }

            public async override void OnReceive(Context context, Intent intent)
            {
                var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;

                Log.Info(TAG, "USB device detached: " + device.DeviceName);

                await activity.PopulateListAsync();
            }
        }
        #endregion
    }

}