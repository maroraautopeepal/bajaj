using SML.Interfaces;
using SML.UWP.Dependencies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: Dependency(typeof(SaveLocalData))]
namespace SML.UWP.Dependencies
{
    public class SaveLocalData : ISaveLocalData
    {
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        public string GetData(string file_name)
        {
            try
            {
                //StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

                //StorageFile createdFile = await storageFolder.GetFileAsync($"{file_name}.txt");

                //StorageFile createdFile = await Task.Factory.StartNew( () =>
                //{
                //    return  storageFolder.GetFileAsync($"{file_name}.txt");
                //}).Result;

                //string text = await Windows.Storage.FileIO.ReadTextAsync(createdFile);
                var text = Get_Data(file_name).Result;
                return text;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async Task<string> Get_Data(string file_name)
        {
            try
            {
                string text = string.Empty;
                //StorageFolder storageFolder = null;

                //StorageFile createdFile = await storageFolder.GetFileAsync($"{file_name}.txt");

                //StorageFile createdFile = await Task.Factory.StartNew(() =>
                //{
                //    return storageFolder.GetFileAsync($"{file_name}.txt");
                //}).Result;

                text = await Task.Factory.StartNew(async () =>
                {
                    StorageFile createdFile = await storageFolder.GetFileAsync($"{file_name}.txt");
                    // return await Windows.Storage.FileIO.ReadTextAsync(createdFile).AsTask();
                    var buffer = await FileIO.ReadBufferAsync(createdFile);
                    using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
                    {
                        return dataReader.ReadString(buffer.Length);
                    }
                }).Result.ConfigureAwait(false);

                //var buffer = await FileIO.ReadBufferAsync(createdFile);
                //using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
                //{
                //    text = dataReader.ReadString(buffer.Length);
                //}


                //////Uri uriFileToLoad = new Uri("ms-appx:///" + file_name,UriKind.Absolute);
                //////storageFolder = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uriFileToLoad);
                //////Stream readStream = await storageFolder.OpenStreamForReadAsync();
                //////using (StreamReader reader = new StreamReader(readStream))
                //////{
                //////    text = await reader.ReadToEndAsync();
                //////}
                //string text = await Windows.Storage.FileIO.ReadTextAsync(createdFile);

                return text;
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
                //StorageFolder storageFolder =  ApplicationData.Current.LocalFolder;
                // StorageFile sampleFile = await storageFolder.CreateFileAsync($"{file_name}.txt",CreationCollisionOption.ReplaceExisting);
                string write = await Task.Factory.StartNew(async () =>
                {
                    StorageFile createdFile = await storageFolder.CreateFileAsync($"{file_name}.txt", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(createdFile, data);
                    return "";

                }).Result.ConfigureAwait(false);

            }
            catch (Exception ex)
            {
            }
        }
    }
}