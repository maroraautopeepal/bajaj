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
    public class FlashCurtisModel
    {
        public string Type { get; set; }
        public int NoOfSectors { get; set; }
        public List<SectorData1> SectorData { get; set; }
    }

    public class SectorData1
    {
        public string SectorLength { get; set; }
        public string JsonData { get; set; }
    }
}