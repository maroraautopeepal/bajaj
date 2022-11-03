using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(SaveLocalData))]
namespace Bajaj.Droid.Dependencies
{
    public class SaveLocalData : ISaveLocalData
    {
        public string GetData(string file_name)
        {
            try
            {

                var backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), $"{file_name}.txt");

                if (backingFile == null || !File.Exists(backingFile))
                {
                    return null;
                }
                string FileData = string.Empty;
                using (var reader = new StreamReader(backingFile, true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        FileData = line;
                    }
                }

                return FileData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task SaveData(string file_name, string data)
        {
            try
            {
                var backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), $"{file_name}.txt");
                using (var writer = File.CreateText(backingFile))
                {
                    await writer.WriteLineAsync(data);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}