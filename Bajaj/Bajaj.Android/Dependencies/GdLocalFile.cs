using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(GdLocalFile))]
namespace Bajaj.Droid.Dependencies
{
    public class GdLocalFile : IGdLocalFile
    {
        public async Task<string> GetGdData()
        {
            try
            {

                var backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "GdLocalFile.txt");

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

        public async Task SaveGdData(string Data)
        {
            try
            {
                var backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "GdLocalFile.txt");
                using (var writer = File.CreateText(backingFile))
                {
                    await writer.WriteLineAsync(Data);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<string> GetLogsData(string backingFile)
        {
            try
            {

                //var backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "GdLocalFile.txt");

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

        public void getFileFolder()
        {
            var file = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
        }
    }
}