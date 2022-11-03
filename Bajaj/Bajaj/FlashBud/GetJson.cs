using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bajaj.FlashBud
{
    public class GetJson
    {
        public async Task ConvertToJson(Stream stream)
        {
            Hex2JSON Hex2JSON = new Hex2JSON();
            using (StreamReader FileStream = new StreamReader(stream))
            {
                int counter = 0;
                string ln;
                string data = "";
                while ((ln = FileStream.ReadLine()) != null)
                {
                    Console.WriteLine(ln);
                    counter++;
                    Hex2JSON.hex2json(ln, out string input);
                    data = input;
                }
                FileStream.Close();
                Console.WriteLine("File has {counter} lines.");
                Console.WriteLine("APDATA =" + data);
            }
            Hex2JSON.endfile();
            //Hex2JSON.WriteDatta();
            var Value = await Hex2JSON.CreateJSON();
        }
    }
}
