﻿using System;

namespace APDiagnosticAndroid.Models
{
    public class ReadDtcResponseModel
    {
        public string status { get; set; }
        public string[,] dtcs { get; set; }
        public UInt16 noofdtc { get; set; }
    }
}
