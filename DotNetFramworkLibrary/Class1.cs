using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFramworkLibrary
{
    public class Class1
    {
        public byte[] getHash(byte[] seed, string seedkey)
        {
            // create a ripemd160 object
            byte[] s_seed = new byte[16];
            RIPEMD160 r160 = RIPEMD160Managed.Create("ripemd160");
            // convert the string to byte
            //15 FB ED 0C 4A FC F8 77 03 85 D4 47 5B FB 9F 35

            try
            {
                switch (seedkey)
                {
                    case "KOEL_BOSCH_BS6":
                        s_seed = HexStringToByteArray("15FBED0C4AFCF8770385D4475BFB9F35");
                        break;
                    case "SML_BOSCH_BS6_PROD":
                        s_seed = HexStringToByteArray("F633E29B600729855EEB398C199BBE88");
                        break;
                    case "BAJAJ_RIPEMD160_SECURITY":
                        s_seed = HexStringToByteArray("01AC6BD69ECDF74CBC172AB7F9D3CEA7"); //writing
                        //s_seed = HexStringToByteArray("A8C38E6EA6BD41C92F3FA879CDFC505B");//flashing
                        break;
                }
            }
            catch (Exception ex)
            {
                
            }


            #region USE FOR KIRLOSKAR
                //s_seed = HexStringToByteArray("15FBED0C4AFCF8770385D4475BFB9F35");
            #endregion

            #region USE FOR SML
            //s_seed = HexStringToByteArray("F633E29B600729855EEB398C199BBE88");
            #endregion
            //var seed = HexStringToByteArray(seed1);
            //var key = "FFC9632164C64C66";

            byte[] myByte = new byte[s_seed.Length + seed.Length];
            Buffer.BlockCopy(s_seed, 0, myByte, 0, s_seed.Length);
            Buffer.BlockCopy(seed, 0, myByte, s_seed.Length, seed.Length);
            //return bytes;

            //byte[] myByte = System.Text.Encoding.ASCII.GetBytes(s_seed);
            // compute the byte to RIPEMD160 hash
            byte[] encrypted = r160.ComputeHash(myByte);
            // create a new StringBuilder process the hash byte
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < encrypted.Length; i++)
            //{
            //    sb.Append(encrypted[i].ToString("X2"));
            //}
            //// convert the StringBuilder to String and convert it to lower case and return it.
            //return sb.ToString().ToLower();
            return encrypted;
        }

        private byte[] HexStringToByteArray(String hex)
        {
            try
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
            catch (Exception ex)
            {
                return null;
            }
        }

        
    }
}
