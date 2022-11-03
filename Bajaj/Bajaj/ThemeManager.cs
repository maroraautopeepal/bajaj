using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace Bajaj
{
    public class ThemeManager
    {
        public static System.Drawing.Color GetColorFromHexValue(string hex)
        {
            if (hex == null)
            {
                return Color.Red;
            }
            string cleanHex = hex.Replace("0x", "").TrimStart('#');

            if (cleanHex.Length == 6)
            {
                //Affix fully opaque alpha hex value of FF (225)
                cleanHex = "FF" + cleanHex;
            }

            int argb;

            if (Int32.TryParse(cleanHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out argb))
            {
                return System.Drawing.Color.FromArgb(argb);
            }

            //If method hasn't returned a color yet, then there's a problem
            throw new ArgumentException("Invalid Hex value. Hex must be either an ARGB (8 digits) or RGB (6 digits)");

        }
    }
}
