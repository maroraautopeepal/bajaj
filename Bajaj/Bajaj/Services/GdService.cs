using Bajaj.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Xamarin.Forms;

namespace Bajaj.Services
{
    public static class GdService
    {
        static HttpClient client;
        //public static string BaseUrl = "http://104.211.96.31:8080/api/v1/";
        public static string BaseUrl = "http://104.211.96.31/api/v1/";
        public async static void GetGdauthorDataForLocalFile(string token)
        {
            try
            {
                client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(45);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = client.GetStringAsync(BaseUrl + "oem/get-gdauthor/").GetAwaiter().GetResult();
                //dynamic list = JsonConvert.DeserializeObject(json);
                await DependencyService.Get<IGdLocalFile>().SaveGdData(response);
                //var json = response.Content.ReadAsStringAsync().Result;
                //using (var httpClient = new HttpClient())
                //{
                //   var json= httpClient.GetStringAsync(BaseUrl + "oem/get-gdauthor/");
                //    DependencyService.Get<IGdLocalFile>().SaveGdData(json.Result);
                //}

            }
            catch (Exception ex)
            {
                Acr.UserDialogs.UserDialogs.Instance.Alert("GD Data Getted", "Alert", "Ok");
            }
        }
    }
}
