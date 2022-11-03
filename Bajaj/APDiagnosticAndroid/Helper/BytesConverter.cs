using System;

namespace APDiagnosticAndroid.Helper
{
    public static class BytesConverter
    {
        public static String HexToASCII(String hex)
        {
            // initialize the ASCII code string as empty. 
            String ascii = "";

            for (int i = 0; i < hex.Length; i += 2)
            {

                // extract two characters from hex string 
                String part = hex.Substring(i, 2);

                // change it into base 16 and  
                // typecast as the character 
                char ch = (char)Convert.ToInt32(part, 16); ;

                // add this char to final ASCII string 
                ascii = ascii + ch;
            }
            return ascii;
        }
    }
}
