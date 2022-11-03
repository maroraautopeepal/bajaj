using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Bajaj
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            try
            {

                InitializeComponent();
            }
            catch (System.Exception ex)
            {
            }
        }

        private void Entry_Completed(object sender, System.EventArgs e)
        {
            try
            {

                byte[] data_bytes = new byte[2];
                string outputHex = int.Parse(txtValue.Text.ToUpper()).ToString("X");
                byte[] writeinput = HexStringToByteArray(outputHex);

                if (writeinput.Length < 2)
                {
                    data_bytes[0] = 0x00;
                    data_bytes[1] = writeinput[0];
                }
                else
                {
                    Array.Copy(writeinput, 0, data_bytes, 1 - 1, writeinput.Length);
                }
                //    string input = txtValue.Text;
                //    string outputHex = int.Parse(input).ToString("X");
                //    Console.WriteLine($"String Value : {txtValue.Text}, Hex String Value : {outputHex}");

                //    var writeParaPID = HexStringToByteArray(outputHex);
                //    var writeParaPID1 = HexToBytes(outputHex);

            }
            catch (System.Exception ex)
            {
            }
        }

        private byte[] HexStringToByteArray(String hex)
        {
            hex = hex.Replace(" ", "");
            int numberChars = hex.Length;
            if (numberChars % 2 != 0)
            {
                hex = "0" + hex;
                numberChars++;
            }
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        static byte[] HexToBytes(string input)
        {
            byte[] result = new byte[input.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(input.Substring(2 * i, 2), 16);
            }
            return result;
        }

    }
}
