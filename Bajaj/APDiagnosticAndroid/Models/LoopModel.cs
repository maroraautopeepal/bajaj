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

namespace APDiagnosticAndroid.Models
{
    public class LoopModel
    {
        public int loopId { get; set; }
        public int i { get; set; }
        public int maxIndex { get; set; }
        public int loopLocation { get; set; }
    }
}