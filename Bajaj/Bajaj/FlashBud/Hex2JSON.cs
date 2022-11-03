using Bajaj.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bajaj.FlashBud
{
    public class Hex2JSON
    {
        string filetype = "";
        public string TAG = "HEX2JSON";
        UInt32 expected_strt_addr = 0;
        UInt32[] sectorstrtaddr = new UInt32[40];
        UInt32 sectorindex = 0;
        UInt32 linestrtaddr, linestrtaddrHS = 0x00000000;
        bool firstline = true;

        UInt32 defaultecuaddressing = 4; // 4 byte addressing. This change change based on the addressing as it comes by
        UInt32 sector_data_index = 0;
        UInt32 actualdatalength = 0;
        UInt32[] sector_len = new UInt32[20];
        string return_status = "NOERROR";
        // IList<uint, byte[]> sectorData = new List<uint, byte[]>();
        byte[,] inputfiledataarray = new byte[20, 1024 * 10000];


        UInt32 file_start_addr = 0x00000000;
        public string ProjectName = string.Empty;
        private string returnstatus = string.Empty;

        public List<ReturnJsonDataANDPath> ReturnJson { get; set; }

        UInt32[] ecustartaddress, ecuendaddress;
        UInt32[] ecumemmapnumsectors = null;
        UInt32[] jsonstartaddress, jsonendaddress = null;

        public string hex2json(string lineBytes, out string inputArray)
        {
            inputArray = string.Empty;
            /************************************************************************/
            /* as read from hex / srec file											*/
            /* inputfilenumsectors													*/
            /* inputfilestrtaddr[i], inputfiledataarray[i]		*/
            /************************************************************************/
            //STEP1: open hex file
            //STEP2: seggregate contiguous memory areas into sectors and put them into filestrtaddr[i], fileendaddr[i], filedata[i]

            //1. open hex file
            //2. Read line by line. 
            //	If the line is of an extended address, then update extended address, increment sector, reset sector data index
            //	if the line is of data, then update address, copy data bytes to sector
            //	do this until file end...

            var firstchar = lineBytes[0];

            // determine if it is a hex or a srec file

            // Read first line. if the line has "S" as its first character, it is SREC, if it is ":", then it is hex file
            if (firstchar == 'S')
            {
                filetype = "SREC";
            }
            else if (firstchar == ':')
            {
                filetype = "HEX";
            }
            else
            {
                /* Invalid file type */
                return "INVALIDFILETYPE";
            }



            switch (filetype)
            {
                case "HEX":
                    {
                        var line = lineBytes.ToArray();
                        string atualata = lineBytes.Trim(':');
                        var hxArray = HexStringToByteArray(atualata);

                        byte[] linehexarray = null;
                        if (line[0] == ':')
                        {
                            // put the entire line from line[1] to end linehexarray[]
                            linehexarray = hxArray;

                            var linelen = linehexarray[0];
                            var linetype = linehexarray[3];

                            // check for crc authenticity of the line ----------------TBD
                            Console.WriteLine("linelen =" + linelen.ToString("X2"), TAG);
                            Console.WriteLine("linetype =" + linetype.ToString("X2"), TAG);

                            Console.WriteLine("expected_strt_addr =" + expected_strt_addr.ToString("X2"), TAG);

                            Console.WriteLine("sectorstrtaddr[sectorindex] =" + sectorstrtaddr[sectorindex].ToString("X2"), TAG);
                            Console.WriteLine("sectorindex =" + sectorindex.ToString(), TAG);


                            if (linetype == 0x00) // data line
                            {

                                Console.WriteLine("linehexarray =" + ByteArrayToString(linehexarray), TAG);
                                // Ex - :0B 0010 00 6164647265737320676170 A7
                                linestrtaddr = (UInt32)(linestrtaddrHS + ((linehexarray[1] << 8) + linehexarray[2])); // update the start address of the line
                                                                                                                      //copybytearray(linehexarray[4], inputfiledataarray[sector_data_index], linelen);

                                if (firstline == true)
                                {
                                    expected_strt_addr = linestrtaddr;
                                    firstline = false;
                                    sectorstrtaddr[sectorindex] = linestrtaddr;
                                }
                                Console.WriteLine("actualdatalength =" + actualdatalength, TAG);

                                if (expected_strt_addr == linestrtaddr) // check if there is a break in address
                                {
                                    /* there is continuity in teh address - so append to the same sector */
                                }
                                else
                                {
                                    sector_len[sectorindex] = sector_data_index;
                                    /* start new sector - update start address of new sector and get the sector_data_index to 0. also increment sector index */
                                    sector_data_index = 0;
                                    sectorstrtaddr[++sectorindex] = linestrtaddr;
                                }

                                for (int i = 0; i < linelen; i++)
                                {
                                    inputfiledataarray[sectorindex, sector_data_index + i] = linehexarray[4 + i];
                                }
                                sector_data_index += linelen; // get prepared to accept new dataline

                                expected_strt_addr = (UInt32)(linestrtaddr + linelen);
                            }
                            else if (linetype == 0x04) // 4 byte addressing
                            {
                                // Ex - :02 0000 04 FFFF FC
                                linestrtaddrHS = (UInt32)((linehexarray[4] << 24) + (linehexarray[5] << 16)); // update the HS 2 bytes of the start address
                                defaultecuaddressing = 4;
                            }
                            else if (linetype == 0x03) // 3 byte addressing
                            {
                                // Example - :04 0000 03 00003800 C1
                                /* havent got these kinds of hex fiels yet. will update later */
                                linestrtaddrHS = (UInt32)((linehexarray[5] << 16) + (linehexarray[6] << 8) + (linehexarray[7])); // update the 24 byte start address of sector
                                defaultecuaddressing = 3;

                                if (expected_strt_addr == linestrtaddr) // check if there is a break in address
                                {
                                    /* there is continuity in teh address - so append to the same sector */
                                }
                                else
                                {
                                    /* start new sector - update start address of new sector and get the sector_data_index to 0. also increment sector index */
                                    sector_data_index = 0;
                                    sectorstrtaddr[sectorindex] = linestrtaddr;
                                    sectorindex++;
                                }
                                expected_strt_addr = linestrtaddrHS;
                            }
                            else if (linetype == 0x02) // 3 byte addressing
                            {
                                /* havent got these kinds of hex fiels yet. will update later */
                                linestrtaddrHS = (UInt32)((linehexarray[4] << 8) + (linehexarray[4])); // update the 16 byte start address of sector
                                defaultecuaddressing = 2;
                                if (expected_strt_addr == linestrtaddr) // check if there is a break in address
                                {
                                    /* there is continuity in teh address - so append to the same sector */
                                }
                                else
                                {
                                    /* start new sector - update start address of new sector and get the sector_data_index to 0. also increment sector index */
                                    sector_data_index = 0;
                                    sectorstrtaddr[sectorindex] = linestrtaddr;
                                    sectorindex++;
                                }
                                expected_strt_addr = linestrtaddrHS;
                            }
                            else if (linetype == 0x01) // designates end of line - break from loop
                            {
                                break;
                            }

                        }
                        else
                        {
                            returnstatus = "CORRUPTEDHEXFILE";
                            return returnstatus;
                        }
                    }
                    break;
                #region MyRegion
                case "SREC":
                    {
                        /* S0 - Header linetype 													*/
                        /* S1 - 16 bit addressing - data line type, S9 - Start address Termination	*/
                        /* S2 - 24 Bit addressing - data line type, S8 - Start address Termination  */
                        /* S3 - 32 Bit addressing - data line type, S7 - Start address Termination	*/
                        /* S5 - 16 bit count of S1 / S2 / S3 data records in the file				*/
                        /* S6 - 24 bit count of S1 / S2 / S3 data records in the file				*/
                        /* S4 - Reserved															*/
                        /* S7 - start address 32 bit												*/
                        /* S8 - start address 24 bit												*/
                        /* S9 - start address 16 bit    											*/

                        var line = lineBytes.ToArray();

                        if (line[0] == 'S')
                        {
                            /* Example line: S3 25 A0020020 000004001D000040FFFFFFFF0000000007008004020100030C00801300093200 50 */

                            var linetype = line[1];


                            // put the entire line from line[2] to end into a hex array linehexarray[]
                            string atualata = lineBytes.Substring(2);
                            byte[] linehexarray = HexStringToByteArray(atualata);

                            uint linelen = linehexarray[0];

                            // check for crc authenticity of the line ----------------TBD

                            UInt32 linestrtaddr = 0;

                            if (linetype == '1') // 16 byte addressing data line
                            {
                                // Eg: S1 23 0020 000004001D000040FFFFFFFF0000000007008004020100030C00801300093200 50
                                linestrtaddr = (UInt32)((linehexarray[1] << 8) + linehexarray[2]); // update the start address of the line

                                if (firstline == true)
                                {
                                    expected_strt_addr = linestrtaddr;
                                    firstline = false;
                                    sectorstrtaddr[sectorindex] = linestrtaddr;
                                }

                                if (expected_strt_addr == linestrtaddr) // check if there is a break in address
                                {
                                    /* there is continuity in teh address - so append to the same sector */
                                }
                                else
                                {
                                    sector_len[sectorindex] = sector_data_index;
                                    /* start new sector - update start address of new sector and get the sector_data_index to 0. also increment sector index */
                                    sector_data_index = 0;
                                    sectorstrtaddr[++sectorindex] = linestrtaddr;
                                }

                                for (int i = 0; i < linelen - 3; i++)
                                {
                                    inputfiledataarray[sectorindex, sector_data_index + i] = linehexarray[3 + i];
                                }

                                sector_data_index += (linelen - 3); // get prepared to accept new dataline
                                defaultecuaddressing = 2;

                                expected_strt_addr = linestrtaddr + linelen - 3;

                            }
                            else if (linetype == '2') // 24 byte addressing data line
                            {
                                // Eg: S2 23 000020 000004001D000040FFFFFFFF0000000007008004020100030C00801300093200 50
                                linestrtaddr = (UInt32)((linehexarray[1] << 16) + (linehexarray[2] << 8) + linehexarray[3]); // update the start address of the line

                                if (firstline == true)
                                {
                                    expected_strt_addr = linestrtaddr;
                                    firstline = false;
                                    sectorstrtaddr[sectorindex] = linestrtaddr;
                                }

                                if (expected_strt_addr == linestrtaddr) // check if there is a break in address
                                {
                                    /* there is continuity in teh address - so append to the same sector */
                                }
                                else
                                {
                                    sector_len[sectorindex] = sector_data_index;
                                    /* start new sector - update start address of new sector and get the sector_data_index to 0. also increment sector index */
                                    sector_data_index = 0;
                                    sectorstrtaddr[++sectorindex] = linestrtaddr;
                                }

                                for (int i = 0; i < linelen - 4; i++)
                                {
                                    inputfiledataarray[sectorindex, sector_data_index + i] = linehexarray[4 + i];
                                    //Console.WriteLine("sectorindex " + Convert.ToString(sectorindex) + " sector_data_index " + Convert.ToString(sector_data_index) + " linelen " + Convert.ToString(linelen),TAG);
                                    //Debug.Write("sectorindex " + Convert.ToString(sectorindex) + " sector_data_index " + Convert.ToString(sector_data_index) + " linelen " + Convert.ToString(linelen) + "\n");
                                }

                                sector_data_index += (linelen - 4); // get prepared to accept new dataline
                                defaultecuaddressing = 3;

                                expected_strt_addr = linestrtaddr + linelen - 4;
                            }
                            else if (linetype == '3') // 32 byte addressing data line
                            {
                                /* Example line: S3 25 A0020000 000004001D000040FFFFFFFF0000000007008004020100030C00801300093200 50 */
                                linestrtaddr = (UInt32)((linehexarray[1] << 24) + (linehexarray[2] << 16) + (linehexarray[3] << 8) + linehexarray[4]); // update the start address of the line

                                if (firstline == true)
                                {
                                    expected_strt_addr = linestrtaddr;
                                    firstline = false;
                                    sectorstrtaddr[sectorindex] = linestrtaddr;
                                }

                                if (expected_strt_addr == linestrtaddr) // check if there is a break in address
                                {
                                    /* there is continuity in teh address - so append to the same sector */
                                }
                                else
                                {
                                    sector_len[sectorindex] = sector_data_index;
                                    /* start new sector - update start address of new sector and get the sector_data_index to 0. also increment sector index */
                                    sector_data_index = 0;
                                    sectorstrtaddr[++sectorindex] = linestrtaddr;
                                }

                                for (int i = 0; i < linelen - 5; i++)
                                {
                                    inputfiledataarray[sectorindex, sector_data_index + i] = linehexarray[5 + i];
                                }

                                sector_data_index += (linelen - 5); // get prepared to accept new dataline
                                defaultecuaddressing = 4;

                                expected_strt_addr = linestrtaddr + linelen - 5;
                            }
                            else if (linetype == '7')
                            {
                                file_start_addr = (UInt32)((linehexarray[1] << 24) + (linehexarray[2] << 16) + (linehexarray[3] << 8) + linehexarray[4]); // update the start address of the line
                            }
                            else if (linetype == '8')
                            {
                                file_start_addr = (UInt32)((linehexarray[1] << 16) + (linehexarray[2] << 8) + linehexarray[3]); // update the start address of the line
                            }
                            else if (linetype == '9')
                            {
                                file_start_addr = (UInt32)((linehexarray[1] << 8) + linehexarray[2]); // update the start address of the line
                            }
                            // neglect other line types
                        }
                        else
                        {
                            returnstatus = "CORRUPTEDSRECFILE";
                            return (returnstatus);
                        }

                        break;
                        #endregion
                    }
            }

            var inputfilenumsectors = sectorindex;

            return (return_status);
        }

        public void endfile()
        {
            sector_len[sectorindex++] = sector_data_index;
        }

        public async Task<List<ReturnJsonDataANDPath>> CreateJSON()
        {
            ReturnJson = new List<ReturnJsonDataANDPath>();

            int Count = 0;
            int i = 0;
            jsonstartaddress = new UInt32[20];
            jsonendaddress = new UInt32[20];

            
            for (i = 0; i < sector_len.Length; i++)
            {
                if (sector_len[i + 1] == 0)
                {
                    Count = i + 1;
                    break;
                }
            }
            ecustartaddress = new UInt32[Count];
            ecuendaddress = new UInt32[Count];

            Array.Copy(sectorstrtaddr, 0, ecustartaddress, 0, Count);
            for (i = 0; i < Count; i++)
            {
                ecuendaddress[i] = sectorstrtaddr[i] + sector_len[i];
            }

            if (filetype == "HEX")
            {
                ReturnJson = await createjsonbyecumemmap(Count, ecustartaddress, ecuendaddress);
            }
            else if (filetype == "SREC")
            {
                ReturnJson = await SRECcreatejsonbyecumemmap(Count, ecustartaddress, ecuendaddress);
            }
            //else if (FileName.Contains(".ulp"))
            //{
            //    ReturnJson = await For_VCV(FileName.Replace(".ulp", ""), Count, ecustartaddress, ecuendaddress, JsonEncryptORNot, ChecksumType, FlashStatus);
            //}
            //else if (FileName.Contains(".enc"))
            //{
            //    ReturnJson = await createjsonbyecumemmap(FileName.Replace(".enc", ""), Count, ecustartaddress, ecuendaddress, JsonEncryptORNot, FlashStatus);
            //}


            return ReturnJson;
        }


        #region ( .Hex ) Create Json By ECU Memmap
        public async Task<List<ReturnJsonDataANDPath>> createjsonbyecumemmap(int ecumemmapnumsectors, UInt32[] ecustartaddress, UInt32[] ecuendaddress)
        {
            List<FlashingMatrix> flashingMatrixCollection = new List<FlashingMatrix>();
            FlashingMatrixData FlashingMatrixData = new FlashingMatrixData();

            UInt16 noofjsonsectors = 0;

            for (int i = 0; i < ecumemmapnumsectors; i++)
            {
                // check if the ecumemblock lies completely within any of the inputfilesectors
                for (int j = 0; j < sectorindex; j++)
                {
                    var stringData = string.Empty;
                    if ((ecustartaddress[i] >= sectorstrtaddr[j]) && ((sectorstrtaddr[j] + sector_len[j]) > ecustartaddress[i]))
                    {
                        jsonstartaddress[i] = Math.Max(ecustartaddress[i], sectorstrtaddr[j]);

                        jsonendaddress[i] = Math.Min(ecuendaddress[i], (sectorstrtaddr[j] + sector_len[j] - 1));

                        //---------------

                        var number = Convert.ToInt32(jsonendaddress[i] - jsonstartaddress[i] + 1);
                        byte[,] newArray = new byte[1, number];
                        var num = (jsonstartaddress[i] - sectorstrtaddr[j]);
                        var len = inputfiledataarray.GetLength(1);
                        Array.Copy(inputfiledataarray, (j * 1024 * 10000) + (jsonstartaddress[i] - sectorstrtaddr[j]), newArray, 0, number);


                        //var actualArray = inputfiledataarray[j, ((jsonstartaddress[i] - sectorstrtaddr[j]))];

                        var bytesCollection = Enumerable.Range(0, newArray.GetLength(1))
                                    .Select(x => newArray[0, x])
                                    .ToArray();

                        stringData = ByteArrayToString(bytesCollection);

                        UInt16 checksum = 0x0000;
                        for (int temp = 0; temp < number; temp++)
                            checksum += bytesCollection[temp];

                        var flashingMatrix = new FlashingMatrix
                        {
                            JsonEndAddress = jsonendaddress[i].ToString("X4"),
                            JsonStartAddress = jsonstartaddress[i].ToString("X4"),
                            JsonCheckSum = checksum.ToString("X2"),

                            ECUMemMapStartAddress = ecustartaddress[i].ToString("X4"),
                            ECUMemMapEndAddress = ecuendaddress[i].ToString("X4"),

                            JsonData = stringData,

                        };
                        flashingMatrixCollection.Add(flashingMatrix);
                        noofjsonsectors++;
                        break;
                    }
                }
                FlashingMatrixData.NoOfSectors = ecumemmapnumsectors;
                FlashingMatrixData.SectorData = flashingMatrixCollection;
            }


            for (int j = 0; j < sectorindex; j++)
            {
                // check if the ecumemblock lies completely within any of the inputfilesectors
                for (int i = 0; i < ecumemmapnumsectors; i++)
                {
                    var stringData = string.Empty;
                    if ((sectorstrtaddr[j] > ecustartaddress[i]) && ((sectorstrtaddr[j] + sector_len[j]) < ecuendaddress[i]))
                    {
                        jsonstartaddress[i] = Math.Max(ecustartaddress[i], sectorstrtaddr[j]);

                        jsonendaddress[i] = Math.Min(ecuendaddress[i], (sectorstrtaddr[j] + sector_len[j] - 1));

                        //---------------

                        var number = Convert.ToInt32(jsonendaddress[i] - jsonstartaddress[i] + 1);
                        byte[,] newArray = new byte[1, number];
                        var num = (jsonstartaddress[i] - sectorstrtaddr[j]);
                        var len = inputfiledataarray.GetLength(1);
                        Array.Copy(inputfiledataarray, (j * 1024 * 10000) + (jsonstartaddress[i] - sectorstrtaddr[j]), newArray, 0, number);


                        var bytesCollection = Enumerable.Range(0, newArray.GetLength(1))
                                    .Select(x => newArray[0, x])
                                    .ToArray();

                        stringData = ByteArrayToString(bytesCollection);

                        UInt16 checksum = 0x0000;
                        for (int temp = 0; temp < number; temp++)
                            checksum += bytesCollection[temp];

                        var flashingMatrix = new FlashingMatrix
                        {
                            JsonEndAddress = jsonendaddress[i].ToString("X4"),
                            JsonStartAddress = jsonstartaddress[i].ToString("X4"),
                            JsonCheckSum = checksum.ToString("X2"),

                            ECUMemMapStartAddress = ecustartaddress[i].ToString("X4"),
                            ECUMemMapEndAddress = ecuendaddress[i].ToString("X4"),

                            JsonData = stringData,

                        };
                        flashingMatrixCollection.Add(flashingMatrix);
                        noofjsonsectors++;
                        break;
                    }
                }
                FlashingMatrixData.NoOfSectors = ecumemmapnumsectors;
                FlashingMatrixData.SectorData = flashingMatrixCollection;
            }
            FlashingMatrixData.NoOfSectors = noofjsonsectors;

            var JsonMatrix = JsonConvert.SerializeObject(FlashingMatrixData);

            if (JsonMatrix != null)
            {
                //var dialog = new MessageDialog("Json File are Created Successfully", "Created");
                //dialog.Commands.Add(new UICommand("OK", async delegate (IUICommand command)
                //{
                //    //await Task.Delay(1000);

                //}));
                //// Show dialog and save result
                //var result = await dialog.ShowAsync();

                //ReturnJson = await GenerateJSONFile(FileName, JsonMatrix, JsonEncryptORNot);
            }
            return ReturnJson;
        }
        #endregion


        #region ( .SREC ) Create Json By ECU Memmap
        public async Task<List<ReturnJsonDataANDPath>> SRECcreatejsonbyecumemmap(int ecumemmapnumsectors, UInt32[] ecustartaddress, UInt32[] ecuendaddress)
        {
            ReturnJson = new List<ReturnJsonDataANDPath>();

            List<FlashingMatrix> flashingMatrixCollection = new List<FlashingMatrix>();
            SREC_FlashingMatrixData SREC_FlashingMatrixData = new SREC_FlashingMatrixData();

            UInt16 noofjsonsectors = 0;

            for (int i = 0; i < ecumemmapnumsectors; i++)
            {
                // check if the ecumemblock lies completely within any of the inputfilesectors
                for (int j = 0; j < sectorindex; j++)
                {
                    var stringData = string.Empty;
                    if ((ecustartaddress[i] >= sectorstrtaddr[j]) && ((sectorstrtaddr[j] + sector_len[j]) > ecustartaddress[i]))
                    {
                        jsonstartaddress[i] = Math.Max(ecustartaddress[i], sectorstrtaddr[j]);

                        jsonendaddress[i] = Math.Min(ecuendaddress[i], (sectorstrtaddr[j] + sector_len[j] - 1));

                        //---------------

                        var number = Convert.ToInt32(jsonendaddress[i] - jsonstartaddress[i] + 1);
                        byte[,] newArray = new byte[1, number];
                        var num = (jsonstartaddress[i] - sectorstrtaddr[j]);
                        var len = inputfiledataarray.GetLength(1);
                        Array.Copy(inputfiledataarray, (j * 1024 * 10000) + (jsonstartaddress[i] - sectorstrtaddr[j]), newArray, 0, number);


                        //var actualArray = inputfiledataarray[j, ((jsonstartaddress[i] - sectorstrtaddr[j]))];

                        var bytesCollection = Enumerable.Range(0, newArray.GetLength(1))
                                    .Select(x => newArray[0, x])
                                    .ToArray();



                        stringData = ByteArrayToString(bytesCollection);

                        UInt16 checksum = 0x0000;

                        //if (ChecksumType == "COMPUTEBYSECTOR_WOADDR_CRCCCITT16")
                        //if (ChecksumType == "COMPAREBYSECTOR_WOADDR_CRCCCITT16")
                        //{
                        //    checksum = cr.ComputeChecksumBytes(bytesCollection);
                        //}
                        //else
                        //{
                            for (int temp = 0; temp < number; temp++)
                                checksum += bytesCollection[temp];
                        //}

                        var flashingMatrix = new FlashingMatrix
                        {
                            JsonEndAddress = jsonendaddress[i].ToString("X4"),
                            JsonStartAddress = jsonstartaddress[i].ToString("X4"),
                            JsonCheckSum = checksum.ToString("X2"),

                            ECUMemMapStartAddress = ecustartaddress[i].ToString("X4"),
                            ECUMemMapEndAddress = ecuendaddress[i].ToString("X4"),

                            JsonData = stringData,

                        };
                        flashingMatrixCollection.Add(flashingMatrix);
                        noofjsonsectors++;
                        break;
                    }
                }
                SREC_FlashingMatrixData.NoOfSectors = ecumemmapnumsectors;
                SREC_FlashingMatrixData.file_start_addr = file_start_addr.ToString("X4");
                SREC_FlashingMatrixData.SectorData = flashingMatrixCollection;
            }


            for (int j = 0; j < sectorindex; j++)
            {
                // check if the ecumemblock lies completely within any of the inputfilesectors
                for (int i = 0; i < ecumemmapnumsectors; i++)
                {
                    var stringData = string.Empty;
                    if ((sectorstrtaddr[j] > ecustartaddress[i]) && ((sectorstrtaddr[j] + sector_len[j]) < ecuendaddress[i]))
                    {
                        jsonstartaddress[i] = Math.Max(ecustartaddress[i], sectorstrtaddr[j]);
                        // print this as the first field

                        jsonendaddress[i] = Math.Min(ecuendaddress[i], (sectorstrtaddr[j] + sector_len[j] - 1));

                        //---------------

                        var number = Convert.ToInt32(jsonendaddress[i] - jsonstartaddress[i] + 1);
                        byte[,] newArray = new byte[1, number];
                        var num = (jsonstartaddress[i] - sectorstrtaddr[j]);
                        var len = inputfiledataarray.GetLength(1);
                        Array.Copy(inputfiledataarray, (j * 1024 * 10000) + (jsonstartaddress[i] - sectorstrtaddr[j]), newArray, 0, number);


                        var bytesCollection = Enumerable.Range(0, newArray.GetLength(1))
                                    .Select(x => newArray[0, x])
                                    .ToArray();

                        stringData = ByteArrayToString(bytesCollection);

                        UInt16 checksum = 0x0000;
                        for (int temp = 0; temp < number; temp++)
                            checksum += bytesCollection[temp];

                        var flashingMatrix = new FlashingMatrix
                        {
                            JsonEndAddress = jsonendaddress[i].ToString("X4"),
                            JsonStartAddress = jsonstartaddress[i].ToString("X4"),
                            JsonCheckSum = checksum.ToString("X2"),

                            ECUMemMapStartAddress = ecustartaddress[i].ToString("X4"),
                            ECUMemMapEndAddress = ecuendaddress[i].ToString("X4"),

                            JsonData = stringData,

                        };
                        flashingMatrixCollection.Add(flashingMatrix);
                        noofjsonsectors++;
                        break;
                    }
                }
                SREC_FlashingMatrixData.NoOfSectors = ecumemmapnumsectors;
                SREC_FlashingMatrixData.file_start_addr = file_start_addr.ToString("X4");
                SREC_FlashingMatrixData.SectorData = flashingMatrixCollection;
            }
            SREC_FlashingMatrixData.NoOfSectors = noofjsonsectors;
            SREC_FlashingMatrixData.file_start_addr = file_start_addr.ToString("X4");

            var JsonMatrix = JsonConvert.SerializeObject(SREC_FlashingMatrixData);

            //if (JsonMatrix != null)
            //{
            //    var dialog = new MessageDialog("Json File are Created Successfully", "Created");
            //    dialog.Commands.Add(new UICommand("OK", async delegate (IUICommand command)
            //    {
            //        //await Task.Delay(1000);

            //    }));
            //    // Show dialog and save result
            //    var result = await dialog.ShowAsync();

            //    ReturnJson = await GenerateJSONFile(FileName, JsonMatrix, JsonEncryptORNot);
            //}
            return ReturnJson;
        }

        #endregion


        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
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

    public class ReturnJsonDataANDPath
    {
        public string JsonPath { get; set; }
        public string JsonData { get; set; }
    }
}
