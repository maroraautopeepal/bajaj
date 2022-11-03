using System;
using System.Security.Cryptography;
using System.Text;

namespace ECUSeedkey
{
    #region New
    public class ECUCalculateSeedkey
    {
        public uint CalculateSeedkey(SEEDKEYINDEXTYPE seedkeyindex, byte numseedbytes, ref byte numkeybytes, byte[] seed, ref byte[] key)
        {
            try
            {
                UInt32 dKey_u32 = 0x0000000;
                UInt32 longseed = 0x0000000;
                UInt32 mask;

                byte numshift_u8;
                byte dir;
                switch (seedkeyindex)
                {
                    case SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_PROD:

                        if (numseedbytes != 4)
                        {
                            // return error message
                        }

                        longseed = (UInt32)seed[3];
                        longseed += (UInt32)seed[2] << 8;
                        longseed += (UInt32)seed[1] << 16;
                        longseed += (UInt32)seed[0] << 24;

                        dKey_u32 = 0x00000000;
                        mask = 0xEDC17789;

                        numshift_u8 = (byte)((longseed & 0x000F0000) >> 16);
                        dir = (byte)((longseed & 0x00000008) >> 3);

                        for (int ct = 0; ct < numshift_u8; ct++)
                        {
                            if (!((char)(longseed & 0x00000007) == 5))
                            {
                                if (dir != 0x00)
                                {
                                    longseed = ((longseed >> 1) | (longseed << 31));
                                    /* (32 -1) times shift in the other direction */
                                }
                                else
                                {
                                    longseed = ((longseed << 1) | (longseed >> 31));
                                }
                            }
                            else
                            {
                                break; /* to break the for loop */
                            }
                        }

                        dKey_u32 = (uint)(longseed ^ mask); //xor operation
                        for (int ctKey = 4; ctKey > 0; ctKey--)
                        {
                            key[ctKey - 1] = (byte)dKey_u32;
                            dKey_u32 = dKey_u32 >> 8;
                        }
                        numkeybytes = 4;
                        break;

                    case SEEDKEYINDEXTYPE.GREAVES_BOSCH_BS6_DEV:

                        key[0] = seed[0];
                        key[1] = seed[1];
                        key[2] = seed[2];
                        key[3] = seed[3];
                        numkeybytes = 4;
                        break;

                    case SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_BS6_A46:
                    case SEEDKEYINDEXTYPE.ATUL_ADVANTEK_BS6_A46:

                        //string clearText = "AABBCCDDEEFF00112233445566778899";
                        //string secretKey = "*supersecretkey*";

                        string clearText = ByteArrayToString(seed);
                        //string secretKey = "GCL&COSa&S#TR460";

                        string secretKey = string.Empty;
                        ;
                        if (seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_BS6_A46)
                        {
                            secretKey = "GCL&COSa&S#TR460";
                        }
                        else if (seedkeyindex == SEEDKEYINDEXTYPE.ATUL_ADVANTEK_BS6_A46)
                        {
                            secretKey = "ATUL9D&XP$A3A3900";
                        }

                        string initVector = "00000000000000000000000000000000";

                        byte[] dataBytes = encrypt(clearText, secretKey, initVector);
                        Array.Resize(ref key, 16);
                        Array.Copy(dataBytes, 0, key, 0, 16);
                        numkeybytes = 16;
                        break;
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        //    public uint CalculateSeedkey(SEEDKEYINDEXTYPE seedkeyindex, byte numseedbytes, ref byte numkeybytes, byte[] seed, ref byte[] key)
        //    {
        //        try
        //        {
        //            UInt32 dKey_u32 = 0x0000000;
        //            UInt32 longseed;
        //            UInt32 mask;

        //            byte numshift_u8;
        //            byte dir;
        //            switch (seedkeyindex)
        //            {
        //                case SEEDKEYINDEXTYPE.GREAVES_BOSCHBS6_PROD:



        //                    if (numseedbytes != 4)
        //                    {
        //                        // return error message
        //                    }

        //                    dKey_u32 = 0x00000000;
        //                    longseed = (UInt32)seed[3];
        //                    longseed += (UInt32)(seed[2] << 8);
        //                    longseed += (UInt32)(seed[1] << 16);
        //                    longseed += (UInt32)(seed[0] << 24);

        //                    mask = 0xEDC17456;

        //                    numshift_u8 = (byte)((longseed & 0x000F0000) >> 16);
        //                    dir = (byte)((longseed & 0x00000008) >> 3);

        //                    for (int ct = 0; ct < numshift_u8; ct++)
        //                    {
        //                        if (!((byte)(longseed & 0x00000007) == 5))
        //                        {
        //                            if (dir != 0x00)
        //                            {
        //                                longseed = ((longseed >> 1) | (longseed << 31));
        //                                /* (32 -1) times shift in the other direction */
        //                            }
        //                            else
        //                            {
        //                                longseed = ((longseed << 1) | (longseed >> 31));
        //                            }
        //                        }
        //                        else
        //                        {

        //                        }
        //                        {
        //                            break; /* to break the for loop */
        //                        }
        //                    }

        //                    dKey_u32 = longseed ^ mask; //xor operation
        //                    for (int ctKey = 4; ctKey > 0; ctKey--)
        //                    {
        //                        key[ctKey - 1] = (byte)dKey_u32;
        //                        dKey_u32 = dKey_u32 >> 8;
        //                    }
        //                    numkeybytes = 4;
        //                    break;

        //                case SEEDKEYINDEXTYPE.GREAVES_BOSCHBS6_DEV:

        //                    key[0] = seed[0];
        //                    key[1] = seed[1];
        //                    key[2] = seed[2];
        //                    key[3] = seed[3];
        //                    numkeybytes = 4;
        //                    break;
        //            }
        //            return 0;
        //        }
        //        catch (Exception ex)
        //        {
        //            return 0;
        //        }
        //    }
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
        //            case SEEDKEYINDEXTYPE.ATULAUTO_ADVANTEK_A46_BS6:

        //                //string clearText = "AABBCCDDEEFF00112233445566778899";
        //                //string secretKey = "*supersecretkey*";

        //                string clearText = ByteArrayToString(seed);
        //                //string secretKey = "GCL&COSa&S#TR460";

        //                string secretKey = string.Empty;
        //                    ;
        //                if (seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_A46_BS6)
        //                {
        //                    secretKey = "GCL&COSa&S#TR460"; 
        //                }
        //                else if(seedkeyindex == SEEDKEYINDEXTYPE.ATULAUTO_ADVANTEK_A46_BS6)
        //                {
        //                    secretKey = "ATUL9D&XP$A3A3900";
        //                }

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

    }

    public enum SEEDKEYINDEXTYPE
    {
        GREAVES_BOSCH_BS6_DEV,
        GREAVES_BOSCH_BS6_PROD,
        GREAVES_ADVANTEK_BS6_A46,
        ATUL_ADVANTEK_BS6_A46,
        TVS_ADVANTEK_BS6_A46,
        SML_ADVANTEK_BS6_A46,
    }
    #endregion

    #region Old Code
    //public class ECUCalculateSeedkey
    //{
    //    //    public uint CalculateSeedkey(SEEDKEYINDEXTYPE seedkeyindex, byte numseedbytes, ref byte numkeybytes, byte[] seed, ref byte[] key)
    //    //    {
    //    //        try
    //    //        {
    //    //            UInt32 dKey_u32 = 0x0000000;
    //    //            UInt32 longseed;
    //    //            UInt32 mask;

    //    //            byte numshift_u8;
    //    //            byte dir;
    //    //            switch (seedkeyindex)
    //    //            {
    //    //                case SEEDKEYINDEXTYPE.GREAVES_BOSCHBS6_PROD:



    //    //                    if (numseedbytes != 4)
    //    //                    {
    //    //                        // return error message
    //    //                    }

    //    //                    dKey_u32 = 0x00000000;
    //    //                    longseed = (UInt32)seed[3];
    //    //                    longseed += (UInt32)(seed[2] << 8);
    //    //                    longseed += (UInt32)(seed[1] << 16);
    //    //                    longseed += (UInt32)(seed[0] << 24);

    //    //                    mask = 0xEDC17456;

    //    //                    numshift_u8 = (byte)((longseed & 0x000F0000) >> 16);
    //    //                    dir = (byte)((longseed & 0x00000008) >> 3);

    //    //                    for (int ct = 0; ct < numshift_u8; ct++)
    //    //                    {
    //    //                        if (!((byte)(longseed & 0x00000007) == 5))
    //    //                        {
    //    //                            if (dir != 0x00)
    //    //                            {
    //    //                                longseed = ((longseed >> 1) | (longseed << 31));
    //    //                                /* (32 -1) times shift in the other direction */
    //    //                            }
    //    //                            else
    //    //                            {
    //    //                                longseed = ((longseed << 1) | (longseed >> 31));
    //    //                            }
    //    //                        }
    //    //                        else
    //    //                        {

    //    //                        }
    //    //                        {
    //    //                            break; /* to break the for loop */
    //    //                        }
    //    //                    }

    //    //                    dKey_u32 = longseed ^ mask; //xor operation
    //    //                    for (int ctKey = 4; ctKey > 0; ctKey--)
    //    //                    {
    //    //                        key[ctKey - 1] = (byte)dKey_u32;
    //    //                        dKey_u32 = dKey_u32 >> 8;
    //    //                    }
    //    //                    numkeybytes = 4;
    //    //                    break;

    //    //                case SEEDKEYINDEXTYPE.GREAVES_BOSCHBS6_DEV:

    //    //                    key[0] = seed[0];
    //    //                    key[1] = seed[1];
    //    //                    key[2] = seed[2];
    //    //                    key[3] = seed[3];
    //    //                    numkeybytes = 4;
    //    //                    break;
    //    //            }
    //    //            return 0;
    //    //        }
    //    //        catch (Exception ex)
    //    //        {
    //    //            return 0;
    //    //        }
    //    //    }
    //    public uint CalculateSeedkey(SEEDKEYINDEXTYPE seedkeyindex, byte numseedbytes, ref byte numkeybytes, byte[] seed, ref byte[] key)
    //    {
    //        try
    //        {
    //            UInt32 dKey_u32 = 0x0000000;
    //            UInt32 longseed = 0x0000000;
    //            UInt32 mask;

    //            byte numshift_u8;
    //            byte dir;
    //            switch (seedkeyindex)
    //            {
    //                case SEEDKEYINDEXTYPE.GREAVES_BOSCHBS6_PROD:

    //                    if (numseedbytes != 4)
    //                    {
    //                        // return error message
    //                    }

    //                    longseed = (UInt32)seed[3];
    //                    longseed += (UInt32)seed[2] << 8;
    //                    longseed += (UInt32)seed[1] << 16;
    //                    longseed += (UInt32)seed[0] << 24;

    //                    dKey_u32 = 0x00000000;
    //                    mask = 0xEDC17789;

    //                    numshift_u8 = (byte)((longseed & 0x000F0000) >> 16);
    //                    dir = (byte)((longseed & 0x00000008) >> 3);

    //                    for (int ct = 0; ct < numshift_u8; ct++)
    //                    {
    //                        if (!((char)(longseed & 0x00000007) == 5))
    //                        {
    //                            if (dir != 0x00)
    //                            {
    //                                longseed = ((longseed >> 1) | (longseed << 31));
    //                                /* (32 -1) times shift in the other direction */
    //                            }
    //                            else
    //                            {
    //                                longseed = ((longseed << 1) | (longseed >> 31));
    //                            }
    //                        }
    //                        else
    //                        {
    //                            break; /* to break the for loop */
    //                        }
    //                    }

    //                    dKey_u32 = (uint)(longseed ^ mask); //xor operation
    //                    for (int ctKey = 4; ctKey > 0; ctKey--)
    //                    {
    //                        key[ctKey - 1] = (byte)dKey_u32;
    //                        dKey_u32 = dKey_u32 >> 8;
    //                    }
    //                    numkeybytes = 4;
    //                    break;

    //                case SEEDKEYINDEXTYPE.GREAVES_BOSCHBS6_DEV:

    //                    key[0] = seed[0];
    //                    key[1] = seed[1];
    //                    key[2] = seed[2];
    //                    key[3] = seed[3];
    //                    numkeybytes = 4;
    //                    break;

    //                case SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_A46_BS6:
    //                case SEEDKEYINDEXTYPE.ATULAUTO_ADVANTEK_A46_BS6:

    //                    //string clearText = "AABBCCDDEEFF00112233445566778899";
    //                    //string secretKey = "*supersecretkey*";

    //                    string clearText = ByteArrayToString(seed);
    //                    //string secretKey = "GCL&COSa&S#TR460";

    //                    string secretKey = string.Empty;
    //                    ;
    //                    if (seedkeyindex == SEEDKEYINDEXTYPE.GREAVES_ADVANTEK_A46_BS6)
    //                    {
    //                        secretKey = "GCL&COSa&S#TR460";
    //                    }
    //                    else if (seedkeyindex == SEEDKEYINDEXTYPE.ATULAUTO_ADVANTEK_A46_BS6)
    //                    {
    //                        secretKey = "ATUL9D&XP$A3A3900";
    //                    }

    //                    string initVector = "00000000000000000000000000000000";

    //                    byte[] dataBytes = encrypt(clearText, secretKey, initVector);
    //                    Array.Resize(ref dataBytes, 16);
    //                    Array.Copy(dataBytes, 0, key, 0, 16);
    //                    numkeybytes = 16;
    //                    break;
    //            }
    //            return 0;
    //        }
    //        catch (Exception ex)
    //        {
    //            return 0;
    //        }
    //    }

    //    private byte[] StringToByteArray(String hex)
    //    {
    //        hex = hex.Replace(" ", "");
    //        int numberChars = hex.Length;
    //        if (numberChars % 2 != 0)
    //        {
    //            hex = "0" + hex;
    //            numberChars++;
    //        }
    //        byte[] bytes = new byte[numberChars / 2];
    //        for (int i = 0; i < numberChars; i += 2)
    //            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
    //        return bytes;
    //    }

    //    private string ByteArrayToString(byte[] ba)
    //    {
    //        string hex = BitConverter.ToString(ba);
    //        return hex.Replace("-", "");
    //    }
    //    private byte[] encrypt(string clearText, string secretKey, string initVector)
    //    {
    //        try
    //        {
    //            AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
    //            aesProvider.KeySize = 128;
    //            aesProvider.BlockSize = 128;
    //            aesProvider.Mode = CipherMode.CBC;
    //            aesProvider.Padding = PaddingMode.PKCS7;


    //            byte[] byteKey = Encoding.UTF8.GetBytes(secretKey.PadRight(16, '\0'));
    //            if (byteKey.Length > 16)
    //            {
    //                byte[] bytePass = new byte[16];
    //                Buffer.BlockCopy(byteKey, 0, bytePass, 0, 16);
    //                byteKey = bytePass;
    //            }

    //            byte[] byteIV = StringToByteArray(initVector);
    //            byte[] byteText = StringToByteArray(clearText);

    //            aesProvider.Key = byteKey;
    //            aesProvider.IV = byteIV;



    //            byte[] byteData = aesProvider.CreateEncryptor().TransformFinalBlock(byteText, 0, byteText.Length);
    //            if (byteData.Length >= 16)
    //            {
    //                byte[] bytePass = new byte[16];
    //                Buffer.BlockCopy(byteData, 0, bytePass, 0, 16);
    //                byteData = bytePass;
    //            }
    //            return byteData;
    //        }
    //        catch (Exception ex)
    //        {
    //            byte[] ivector = { 0, 0, 0 };
    //            return ivector;
    //        }
    //    }

    //}

    //public enum SEEDKEYINDEXTYPE
    //{
    //    GREAVES_BOSCHBS6_DEV,
    //    GREAVES_BOSCHBS6_PROD,
    //    GREAVES_ADVANTEK_A46_BS6,
    //    ATULAUTO_ADVANTEK_A46_BS6,
    //    PIAGGIO_ADVANTEK_A46_BS6,
    //    TVS_ADVANTEK_A46_BS6,
    //    GREAVES_BOSCH_BS6_DEV,
    //    GREAVES_BOSCH_BS6_PROD
    //}
    #endregion
}
