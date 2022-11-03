using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bajaj.Droid.Dependencies
{
    public class TimeTrackingServiceBindder : Binder
    {
        public TimeTrackingServiceConnection Service { get; set; }
        public TimeTrackingServiceBindder(TimeTrackingServiceConnection pService)
        {
            this.Service = pService;
        }
    }
}