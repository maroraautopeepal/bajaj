using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECUSeedkeyPCL
{
    public class CalculateSeedkeyBajaj
    {
        public byte[] CalcKeyFromSeedL3(byte[] _seed)
        {
            UInt32 level3_Const = 0x2EDC6F45;

            Array.Reverse(_seed, 0, _seed.Length);
            var seedString = ByteArrayToString(_seed);
            UInt32 seed = Convert.ToUInt32(seedString, 16);
            UInt32 key, reminder;
            byte divisor_byte_index, reminder_byte_index, xor_dividend_index, xor_seed_index = 0;
            UInt32 divisor = 0;

            if (seed == 0)
            {
                key = 0;
                return null;
            }
            else
            {
                //divisor_byte_index = ; // extract bit 23 + 22
                divisor_byte_index = (byte)((seed & 0x00C00000) >> 22);

                reminder_byte_index = (byte)(seed & 0x00000003); // extract bit 1 + 0

                xor_dividend_index = (byte)((seed & 0x0000C000) >> 14);

                xor_seed_index = (byte)((seed & 0xC0000000) >> 30);

                if (divisor_byte_index == 0x00)
                {
                    divisor = (seed >> 8);
                }
                else if (divisor_byte_index == 0x01)
                {
                    var tem1 = ((seed >> 8) & 0x00FFFF00);
                    var tem2 = seed & 0x00000FF;
                    divisor = (tem1 | tem2);
                }
                else if (divisor_byte_index == 0x02)
                {
                    var tem1 = ((seed >> 8) & 0xFF0000);
                    var tem2 = seed & 0x0000FFFF;
                    divisor = (tem1 | tem2);
                }
                else
                {
                    divisor = seed & 0x00FFFFFF;
                }

                //reminder = mod(level3_Const, divisor);
                reminder = ((level3_Const % divisor) & 0x00ffffff);

                var temp1 = (byte)((level3_Const >> (8 * xor_dividend_index)) & 0x000000FF);

                var temp2 = (byte)((seed >> (8 * xor_seed_index)) & 0x000000FF);

                var xorbyte = (byte)(temp1 ^ temp2);

                if (reminder_byte_index == 0x00)
                {
                    reminder = reminder << 8;
                    key = reminder | xorbyte;
                }
                else if (reminder_byte_index == 0x01)
                {
                    var t1 = (reminder & 0x00FFFF00) << 8;
                    var t2 = (reminder & 0x000000FF);
                    reminder = t1 | t2;
                    key = reminder | (uint)(xorbyte << 8);
                }
                else if (reminder_byte_index == 0x02)
                {
                    var t1 = reminder & 0x0000FFFF;
                    var t2 = (reminder & 0xFF0000) << 8;
                    reminder = t1 | t2;
                    key = reminder | (uint)(xorbyte << 16);
                }
                else
                {
                    //Do nothing for reminder
                    key = reminder | (uint)(xorbyte << 24);
                }

                var keyByte = BitConverter.GetBytes(key);
                keyByte.Reverse();
                //Array.Reverse(keyByte, 0, keyByte.Length);
                return keyByte;
            }
        }


        public byte[] CalcKeyFromSeedL5(byte[] _seed)
        {
            try
            {
                UInt32 level5_Const = 0x841B8CDB;

                Array.Reverse(_seed, 0, _seed.Length);
                var seedString = ByteArrayToString(_seed);

                UInt32 seed = Convert.ToUInt32(seedString, 16);
                UInt32 key, reminder;
                byte divisor_byte_index, reminder_byte_index, xor_dividend_index, xor_seed_index = 0;
                UInt32 divisor = 0;

                if (seed == 0)
                {
                    key = 0;
                    return null;
                }
                else
                {
                    //divisor_byte_index = (byte)((seed & 0x00000300) >> 8); // extract bit 9 + 8
                    divisor_byte_index = (byte)((seed & 0x00000300) >> 8);

                    //reminder_byte_index = (byte)((seed & 0x00000300) >> 8); // extract bit 9 + 8
                    reminder_byte_index = (byte)((seed & 0x00000300) >> 8);

                    //xor_dividend_index = (byte)((seed & 0x00000018) >> 3); // extract bit 4 + 3
                    xor_dividend_index = (byte)((seed & 0x00000018) >> 3);

                    //xor_seed_index = (byte)((seed & 0x0C000000) >> 26); // extract bit 27 + 26
                    xor_seed_index = (byte)((seed & 0x0C000000) >> 26);

                    if (divisor_byte_index == 0x00)
                    {
                        divisor = (seed >> 8);
                    }
                    else if (divisor_byte_index == 0x01)
                    {
                        var tem1 = ((seed >> 8) & 0x00FFFF00);
                        var tem2 = seed & 0x00000FF;
                        divisor = (tem1 | tem2);
                    }
                    else if (divisor_byte_index == 0x02)
                    {
                        var tem1 = ((seed >> 8) & 0xFF0000);
                        var tem2 = seed & 0x0000FFFF;
                        divisor = (tem1 | tem2);
                    }
                    else
                    {
                        divisor = seed & 0x00FFFFFF;
                    }

                    reminder = (level5_Const % divisor);

                    var temp1 = (byte)((level5_Const >> (8 * xor_dividend_index)) & 0x000000FF);

                    var temp2 = (byte)((seed >> (8 * xor_seed_index)) & 0x000000FF);

                    var xorbyte = (byte)(temp1 ^ temp2);

                    if (reminder_byte_index == 0x00)
                    {
                        reminder = reminder << 8;
                        key = reminder | xorbyte;
                    }
                    else if (reminder_byte_index == 0x01)
                    {
                        var t1 = (reminder & 0x00FFFF00) << 8;
                        var t2 = (reminder & 0x000000FF);
                        reminder = t1 | t2;
                        key = reminder | (uint)(xorbyte << 8);
                    }
                    else if (reminder_byte_index == 0x02)
                    {
                        var t1 = reminder & 0x0000FFFF;
                        var t2 = (reminder & 0xFF0000) << 8;
                        reminder = t1 | t2;
                        key = reminder | (uint)(xorbyte << 16);
                    }
                    else
                    {
                        //Do nothing for reminder
                        key = reminder | (uint)(xorbyte << 24);
                    }

                    var keyByte = BitConverter.GetBytes(key);
                    keyByte.Reverse();
                    //Array.Reverse(keyByte, 0, keyByte.Length);
                    return keyByte;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void CalculateSeedkeyBalnetSecurity(ulong BoschSeed)
        {
            DWordAlias_U PassedArg = new DWordAlias_U();
            DWordAlias_U SecretVal = new DWordAlias_U();
            
            var SECRET_B = 0xB400ACE1UL;

            byte LpIdxCtr = 0;
            //unsigned short XtrVals[2] = { 0, 0 };
            ushort XtrVals = 0;

            PassedArg.DblWord = BoschSeed;
            SecretVal.DblWord = SECRET_B;
            //while (LpIdxCtr < 2)
            //{
            //    XtrVals = (ushort)(PassedArg.WordElems & 0x0001 & 0xFF00);
            //    if (PassedArg.WordElems[LpIdxCtr])
            //    {
            //        PassedArg.WordElems[LpIdxCtr] >>= 1;
            //        PassedArg.WordElems[LpIdxCtr] ^= ((-1) * XtrVals[LpIdxCtr]) & SecretVal.WordElems[1];
            //    }
            //    else
            //    {
            //        PassedArg.WordElems[LpIdxCtr] = SecretVal.WordElems[0];
            //    }
            //    LpIdxCtr++;
            //}
            //return PassedArg.DblWord;
        }

        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

    }

    public class DWordAlias_U
    {
        public ushort WordElems;
        public ulong DblWord;
    }
    
}
