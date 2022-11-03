using System;
using System.Security.Cryptography;
using System.Text;

namespace ECUSeedkeyPCL
{
    public class ECUCalculateSeedkey
    {

        public uint CalculateSeedkey(SEEDKEYINDEXTYPE seedkeyindex, byte numseedbytes, ref byte numkeybytes, byte[] seed, ref byte[] key)
        {
            try
            {
                //seed = new byte[] { 0x18, 0x20, 0x8E, 0x6D, 0x68, 0xF7, 0xFF, 0x3F };
                //seed = new byte[] { 0xE3, 0x84, 0xB6, 0x59, 0x64, 0x92, 0xB0, 0x6D };
                CalculateSeedkeyBajaj calculateSeedkeyBajaj = new CalculateSeedkeyBajaj();
                switch (seedkeyindex)
                {
                    //case SEEDKEYINDEXTYPE.KOEL_BOSCH_BS6:
                    //    DotNetFramworkLibrary.Class1 class1 = new DotNetFramworkLibrary.Class1();
                    //    var keyByte = class1.getHash(seed, "KOEL_BOSCH_BS6");
                    //    Array.Resize(ref key, 8);
                    //    Array.Copy(keyByte, 0, key, 0, 8);
                    //    numkeybytes = 8;
                    //    break;
                    //case SEEDKEYINDEXTYPE.SML_BOSCH_BS6_PROD:
                    //    DotNetFramworkLibrary.Class1 class2 = new DotNetFramworkLibrary.Class1();
                    //    var keyByte2 = class2.getHash(seed, "SML_BOSCH_BS6_PROD");
                    //    Array.Resize(ref key, 8);
                    //    Array.Copy(keyByte2, 0, key, 0, 8);
                    //    numkeybytes = 8;
                    //    break;

                    //case SEEDKEYINDEXTYPE.SML_ADVANTEK_BS6_PROD:

                    //    //string clearText = "AABBCCDDEEFF00112233445566778899";
                    //    //string secretKey = "*supersecretkey*";

                    //    string clearText = ByteArrayToString(seed);
                    //    //string secretKey = "GCL&COSa&S#TR460";

                    //    string secretKey = string.Empty;
                    //    ;
                    //    //if (seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_BS6_A46)
                    //    //{
                    //    //    secretKey = "GCL&COSa&S#TR460";
                    //    //}
                    //    //else if (seedkeyindex == SEEDKEYINDEXTYPE.ATUL_ADVANTEK_BS6_A46)
                    //    //{
                    //    //secretKey = "ATUL9D&XP$A3A3900";
                    //    secretKey = "ISMLDA290BASU330";

                    //    //}

                    //    string initVector = "00000000000000000000000000000000";

                    //    byte[] dataBytes = encrypt(clearText, secretKey, initVector);
                    //    Array.Resize(ref key, 16);
                    //    Array.Copy(dataBytes, 0, key, 0, 16);
                    //    numkeybytes = 16;
                    //    break;
                    //case SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_BS6_A46:

                    //    //string clearText = "AABBCCDDEEFF00112233445566778899";
                    //    //string secretKey = "*supersecretkey*";

                    //    clearText = ByteArrayToString(seed);
                    //    //string secretKey = "GCL&COSa&S#TR460";

                    //    secretKey = string.Empty;
                    //    ;
                    //    if (seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_BS6_A46)
                    //    {
                    //        secretKey = "GCL&COSa&S#TR460";
                    //    }
                    //    else if (seedkeyindex == SEEDKEYINDEXTYPE.ATUL_ADVANTEK_BS6_A46)
                    //    {
                    //        secretKey = "ATUL9D&XP$A3A3900";
                    //    }
                    //    string keyformat = "STRING";
                    //    initVector = "00000000000000000000000000000000";
                    //    dataBytes = encrypt(clearText, secretKey, keyformat, initVector);
                    //    Array.Resize(ref key, 16);
                    //    Array.Copy(dataBytes, 0, key, 0, 16);
                    //    numkeybytes = 16;
                    //    break;

                    case SEEDKEYINDEXTYPE.BAJAJ_L3_SECURITY:
                        key = calculateSeedkeyBajaj.CalcKeyFromSeedL3(seed);
                        numkeybytes = 4;
                        break;

                    case SEEDKEYINDEXTYPE.BAJAJ_L5_SECURITY:
                        key = calculateSeedkeyBajaj.CalcKeyFromSeedL5(seed);
                        numkeybytes = 4;
                        break;
                    case SEEDKEYINDEXTYPE.BAJAJ_KEY1TO4_SECURITY:
                        UInt16 seed1 = BitConverter.ToUInt16(seed,0), _key;

                        UInt16 key1 = 0xAEFB;
                        UInt16 key2 = 0x1111;
                        UInt16 key3 = 0x0004;
                        UInt16 key4 = 0x00A0;

                        _key = (UInt16)((UInt16)((UInt16)((UInt16)(seed1 + key1) - key2) << key3) + key4);
                        key = BitConverter.GetBytes(_key);
                        numkeybytes = 2;
                        break;

                    case SEEDKEYINDEXTYPE.BAJAJ_RIPEMD160_SECURITY:
                        DotNetFramworkLibrary.Class1 class3 = new DotNetFramworkLibrary.Class1();
                        var keyByte3 = class3.getHash(seed, "BAJAJ_RIPEMD160_SECURITY");
                        Array.Resize(ref key, 8);
                        Array.Copy(keyByte3, 0, key, 0, 8);
                        numkeybytes = 8;
                        break;
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        //public uint CalculateSeedkey(SEEDKEYINDEXTYPE seedkeyindex, byte numseedbytes, ref byte numkeybytes, byte[] seed, ref byte[] key)
        //{
        //    try
        //    {
        //        UInt32 dKey_u32 = 0x0000000;
        //        UInt32 longseed = 0x0000000;
        //        UInt32 mask;

        //        byte numshift_u8;
        //        byte dir;
        //        switch (seedkeyindex)
        //        {
        //            case SEEDKEYINDEXTYPE.GREAVES_BOSCHBS6_PROD:

        //                if (numseedbytes != 4)
        //                {
        //                    // return error message
        //                }

        //                longseed = (UInt32)seed[3];
        //                longseed += (UInt32)seed[2] << 8;
        //                longseed += (UInt32)seed[1] << 16;
        //                longseed += (UInt32)seed[0] << 24;

        //                dKey_u32 = 0x00000000;
        //                mask = 0xEDC17789;

        //                numshift_u8 = (byte)((longseed & 0x000F0000) >> 16);
        //                dir = (byte)((longseed & 0x00000008) >> 3);

        //                for (int ct = 0; ct < numshift_u8; ct++)
        //                {
        //                    if (!((char)(longseed & 0x00000007) == 5))
        //                    {
        //                        if (dir != 0x00)
        //                        {
        //                            longseed = ((longseed >> 1) | (longseed << 31));
        //                            /* (32 -1) times shift in the other direction */
        //                        }
        //                        else
        //                        {
        //                            longseed = ((longseed << 1) | (longseed >> 31));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        break; /* to break the for loop */
        //                    }
        //                }

        //                dKey_u32 = (uint)(longseed ^ mask); //xor operation
        //                for (int ctKey = 4; ctKey > 0; ctKey--)
        //                {
        //                    key[ctKey - 1] = (byte)dKey_u32;
        //                    dKey_u32 = dKey_u32 >> 8;
        //                }
        //                numkeybytes = 4;
        //                break;

        //            case SEEDKEYINDEXTYPE.GREAVES_BOSCHBS6_DEV:

        //                key[0] = seed[0];
        //                key[1] = seed[1];
        //                key[2] = seed[2];
        //                key[3] = seed[3];
        //                numkeybytes = 4;
        //                break;

        //            case SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_A46_BS6:

        //                //string clearText = "AABBCCDDEEFF00112233445566778899";
        //                //string secretKey = "*supersecretkey*";

        //                string clearText = ByteArrayToString(seed);
        //                string secretKey = "GRE&COSa&S#TR460";
        //                string initVector = "00000000000000000000000000000000";

        //                byte[] dataBytes = encrypt(clearText, secretKey, initVector);
        //                Array.Resize(ref dataBytes, 16);
        //                Array.Copy(dataBytes, 0, key, 0, 16);
        //                numkeybytes = 16;
        //                break;
        //        }
        //        return 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        return 0;
        //    }
        //}

        private byte[] StringToByteArray(String hex)
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

        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        private byte[] encrypt(string clearText, string secretKey, string initVector)
        {
            try
            {
                AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
                aesProvider.KeySize = 128;
                aesProvider.BlockSize = 128;
                aesProvider.Mode = CipherMode.CBC;
                aesProvider.Padding = PaddingMode.PKCS7;


                byte[] byteKey = Encoding.UTF8.GetBytes(secretKey.PadRight(16, '\0'));
                if (byteKey.Length > 16)
                {
                    byte[] bytePass = new byte[16];
                    Buffer.BlockCopy(byteKey, 0, bytePass, 0, 16);
                    byteKey = bytePass;
                }

                byte[] byteIV = StringToByteArray(initVector);
                byte[] byteText = StringToByteArray(clearText);

                aesProvider.Key = byteKey;
                aesProvider.IV = byteIV;



                byte[] byteData = aesProvider.CreateEncryptor().TransformFinalBlock(byteText, 0, byteText.Length);
                if (byteData.Length >= 16)
                {
                    byte[] bytePass = new byte[16];
                    Buffer.BlockCopy(byteData, 0, bytePass, 0, 16);
                    byteData = bytePass;
                }
                return byteData;
            }
            catch (Exception ex)
            {
                byte[] ivector = { 0, 0, 0 };
                return ivector;
            }
        }

        private byte[] encrypt(string clearText, string secretKey, string keyformat, string initVector)
        {
            try
            {
                AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
                aesProvider.KeySize = 128;
                aesProvider.BlockSize = 128;
                aesProvider.Mode = CipherMode.CBC;
                aesProvider.Padding = PaddingMode.PKCS7;

                byte[] byteKey = null;

                if (keyformat == "STRING")
                {
                    byteKey = Encoding.UTF8.GetBytes(secretKey.PadRight(16, '\0'));
                    if (byteKey.Length > 16)
                    {
                        byte[] bytePass = new byte[16];
                        Buffer.BlockCopy(byteKey, 0, bytePass, 0, 16);
                        byteKey = bytePass;
                    }
                }
                else
                {
                    byteKey = HexStringToByteArray(secretKey);
                }

                byte[] byteIV = StringToByteArray(initVector);
                byte[] byteText = StringToByteArray(clearText);

                aesProvider.Key = byteKey;
                aesProvider.IV = byteIV;



                byte[] byteData = aesProvider.CreateEncryptor().TransformFinalBlock(byteText, 0, byteText.Length);
                if (byteData.Length >= 16)
                {
                    byte[] bytePass = new byte[16];
                    Buffer.BlockCopy(byteData, 0, bytePass, 0, 16);
                    byteData = bytePass;
                }
                return byteData;
            }
            catch (Exception ex)
            {
                byte[] ivector = { 0, 0, 0 };
                return ivector;
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

    }

    //public enum SEEDKEYINDEX_TYPE
    //{
    //    GREAVES_BOSCHBS6_DEV,
    //    GREAVES_BOSCHBS6_PROD,
    //    GREAVES_ADVANTEK_A46_BS6,
    //    ATULAUTO_ADVANTEK_A46_BS6,
    //    PIAGGIO_ADVANTEK_A46_BS6,
    //    TVS_ADVANTEK_A46_BS6,
    //}

    public enum SEEDKEYINDEXTYPE
    {
        //SML_BOSCH_BS6_PROD = 1,
        //SML_ADVANTEK_BS6_PROD = 2,
        //SML_ADVANTEK_BS4_PROD = 3,
        //KOEL_BOSCH_BS6 = 4,
        //GREAVES_BOSCH_BS6_DEV = 1,
        //GREAVES_BOSCH_BS6_PROD = 2,
        //GREAVES_ADVANTEK_BS6_A46 = 3,
        //ATUL_ADVANTEK_BS6_A46 = 4,
        //TVS_ADVANTEK_BS6_A46 = 5,
        //SML_ADVANTEK_BS6_A46 = 6,
        //GREAVES_SEDEMAC_BL_BS6 = 7,
        //GREAVES_SEDEMAC_APP_BS6 = 8,
        BAJAJ_RIPEMD160_SECURITY = 1,
        BAJAJ_KEY1TO4_SECURITY = 2,
        BAJAJ_L3_SECURITY = 3,
        BAJAJ_L5_SECURITY = 4,
        BAJAJ_BALNET_SECURITY = 5,
    }
}
