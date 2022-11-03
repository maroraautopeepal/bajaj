using Acr.UserDialogs;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.View;
using Bajaj.ViewModel;
using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bajaj.Services
{
    public class ApiServices
    {
        HttpClient client;
        //public static string BaseUrl = "http://52.66.67.214:8080/api/v1/";
        //public static string BaseUrl = "http://104.211.96.31:8080/api/v1/";
        //public static string BaseUrl = "http://104.211.96.31/api/v1/";
        //public static string BaseUrl = "http://digiscan.smleduconnect.in/api/v1/";
        //public static string BaseUrl = "http://digiscan.smleduconnect.com/api/v1/";
        //public static string BaseUrl = "http://165.232.189.202/api/v1/";//demo server
        //public static string BaseUrl = "http://159.65.152.179/api/v1/";//Bajaj server
        //public static string BaseUrl = "http://139.59.84.1:8000/api/v1/";//vecv server
        //public static string BaseUrl = "https://159.89.167.72/api/v1/"; //Bajaj Server
        //public static string BaseUrl = "http://digiscan.smltrucksbuses.com/api/v1/"; //Bajaj Server

        public static string BaseUrl = "https://fmsdev.bajajauto.co.in/";//Bajaj server

        public ApiServices()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            client = new HttpClient(clientHandler);
            //client = new HttpClient(new System.Net.Http.HttpClientHandler());
        }

        public async Task<LoginRespons> Login(UserModel model)
        {
            try
            {
                //bool retunValue = false;
                LoginRespons loginRespons = new LoginRespons();
                //var _isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                //if (_isReachable)
                //{
                    //client = new HttpClient();
                    //client.Timeout = TimeSpan.FromMilliseconds(10);
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    //demo Login
                    var response = await client.PostAsync(BaseUrl + "accounts/login/", content);//.ConfigureAwait(false);
                    //vecv Login
                    //var response = await client.PostAsync(BaseUrl + "accounts/login/new/", content);//.ConfigureAwait(false);
                    var Data = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var UserInfo = JsonConvert.DeserializeObject<UserResModel>(Data);
                        await DependencyService.Get<ISaveLocalData>().SaveData("UserLoginDetailes", Data);
                    AllOemModel selectedOem = new AllOemModel {
                        id = UserInfo.profile.oem.id,
                        name = UserInfo.profile.oem.name,
                        admin = UserInfo.profile.oem.admin,
                        oem_file = UserInfo.profile.oem.oem_file.attachment,
                        color = UserInfo.profile.oem.color,
                        app_name = UserInfo.profile.oem.app_name,
                        is_active = UserInfo.profile.oem.is_active
                    };
                    var data = JsonConvert.SerializeObject(selectedOem);
                    await DependencyService.Get<ISaveLocalData>().SaveData("selctedOemModel", data);

                    Application.Current.Properties["token"] = App.JwtToken = UserInfo.token.access;
                        await App.Current.SavePropertiesAsync();
                        loginRespons.userRes = UserInfo;
                    DependencyService.Get<Interfaces.IToastMessage>().Show("Token Subscribed");
                        //DependencyService.Get<INotification>().SubscribeUserNotificationId(Convert.ToString(loginRespons.userRes.first_name) + Convert.ToString(loginRespons.userRes.user_id));
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<LoginRes>(Data);
                        loginRespons.LoginRes = error;
                    //retunValue = false;
                    DependencyService.Get<Interfaces.IToastMessage>().Show("Token not Subscribed");
                }
                //}
                //else
                //{
                //    var _LoginValues = DependencyService.Get<ISaveLocalData>().GetData("UserLoginDetailes");
                //    var UserInfo = JsonConvert.DeserializeObject<UserResModel>(_LoginValues);
                //    Application.Current.Properties["token"] = UserInfo.token;
                //    await App.Current.SavePropertiesAsync();
                //    loginRespons.userRes = UserInfo;
                //}



                return loginRespons;
            }
            catch (Exception ex)
            {
                //Application.Current.MainPage.DisplayAlert("Alert", ex.StackTrace, "Ok");
                show_message(ex.Message);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<FirmwareUpdateResponseModel> UpdateFirmware(FirmwareUpdateModel model)
        {
            string url = BaseUrl + "devices/fotax/latest/firmware";
            FirmwareUpdateResponseModel Respons = new FirmwareUpdateResponseModel();
            try
            {
                //bool retunValue = false;

                //model.type = "fotax";
                var _isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (_isReachable)
                {
                    //client = new HttpClient();
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);//.ConfigureAwait(false);
                    var Data = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Respons = JsonConvert.DeserializeObject<FirmwareUpdateResponseModel>(Data);
                    }
                    else
                    {
                        Respons.error = response.StatusCode.ToString();
                    }
                }
                return Respons;
            }
            catch (Exception ex)
            {
                Respons.error = ex.Message;
                return Respons;
            }
        }

        public bool ExistPasswordCheck(UserModel model)
        {
            try
            {
                bool ReturnValue;
                //client = new HttpClient();
                //client.Timeout = TimeSpan.FromMilliseconds(10);
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync(BaseUrl + "accounts/login/", content).Result;//.ConfigureAwait(false);
                var Data = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ReturnValue = true;
                }
                else
                {
                    ReturnValue = false;
                }
                return ReturnValue;
            }
            catch (Exception ex)
            {
                //Application.Current.MainPage.DisplayAlert("Alert", ex.StackTrace, "Ok");
                show_message(ex.StackTrace);
                string x = ex.Message;
                return false;
            }
        }

        public bool ChangePassword(ChangePassword CP, string token)
        {
            try
            {
                bool ReturnValue;
                //client = new HttpClient();
                var json = JsonConvert.SerializeObject(CP);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync(BaseUrl + "accounts/password/change/", content).Result;//.ConfigureAwait(false);
                var Data = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ReturnValue = true;
                }
                else
                {
                    ReturnValue = false;
                }
                return ReturnValue;
            }
            catch (Exception ex)
            {
                //Application.Current.MainPage.DisplayAlert("Alert", ex.StackTrace, "Ok");
                show_message(ex.StackTrace);
                string x = ex.Message;
                return false;
            }
        }

        public async Task<JobcardNumber> GetJobCardNumber()
        {
            JobcardNumber jobcard_number = new JobcardNumber();
            try
            {
                string Data = string.Empty;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                var response = await client.GetAsync(BaseUrl + "analyze/gen-name");
                Data = response.Content.ReadAsStringAsync().Result;
                jobcard_number = JsonConvert.DeserializeObject<JobcardNumber>(Data);
                return jobcard_number;
            }
            catch (Exception ex)
            {
                jobcard_number.error = "Jobcard number not created";
                return jobcard_number;
            }
        }

        public async Task<List<JobCardModel>> GetJobCard(string token, string filename)
        {
            try
            {
                string Data = string.Empty;
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (isReachable)
                {
                    //client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                    var response = await client.GetAsync(BaseUrl + "analyze/my-job-card/");
                    Data = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await App.Current.MainPage.DisplayAlert("Unauthorized", "", "Ok");
                        App.Current.MainPage = new NavigationPage(new View.LoginPage());
                        return null;
                    }

                    else if (response.IsSuccessStatusCode)
                    {
                        //var Data = ReadTextFile(filename);
                        var UserInfo = JsonConvert.DeserializeObject<List<JobCardModel>>(Data);
                        var list = UserInfo;//.Where(x => x.vehicle_model.parent.name.ToLower().Contains("MDE".ToLower())).ToList();

                        await DependencyService.Get<ISaveLocalData>().SaveData("JsonList", Data);

                        return list;
                    }
                    else
                    {
                        Dictionary<string, string> htmlAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Data);
                        Console.WriteLine(htmlAttributes["detail"]);
                        return null;
                    }
                }
                else
                {
                    var JsonListData = DependencyService.Get<ISaveLocalData>().GetData("JsonList");

                    var UserInfo = JsonConvert.DeserializeObject<List<JobCardModel>>(JsonListData);
                    var list = UserInfo;//.Where(x => x.vehicle_model.parent.name.ToLower().Contains("MDE".ToLower())).ToList();
                    return list;
                }
            }
            catch (Exception ex)
            {
                show_message("Session is Expired");
                string x = ex.Message;

                App.Current.MainPage = new NavigationPage(new LoginPage());

                return null;
            }
        }

        public async Task<CheckJobCardModel> CheckJobCard(string token, string JobCardNumber)
        {
            try
            {
                var authData = string.Format("{0}:{1}", "uptime_user", "data1234");
                var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authData));
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
                var response = client.GetAsync("https://udaanapprovals.vecv.net/sap/opu/odata/sap/ZODATA_FIR_SRV/ES_HEADER(JobCrd='" + JobCardNumber + "')?&$format=json").Result;
                var Data = response.Content.ReadAsStringAsync().Result;
                var ClosedSession = JsonConvert.DeserializeObject<CheckJobCardModel>(Data);
                return ClosedSession;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<List<JobcardModelSecond>> CheckJobCardSecondAPI(string token, string JobCardNumber)
        {
            try
            {
                //client = new HttpClient();
                var response = client.GetAsync("http://eos.eicher.in:8082/Api/Ticket/" + JobCardNumber + "?Username=pbhujbal@vecv.in&password=eicher@123");
                var Data = response.Result.Content.ReadAsStringAsync().Result;
                var ClosedSession = JsonConvert.DeserializeObject<List<JobcardModelSecond>>(Data);
                return ClosedSession;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<MainResultClass> SendJobCard(SendJobcardData model)
        {
            try
            {
                MainResultClass mainResultClass = new MainResultClass();
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (isReachable)
                {
                    //client = new HttpClient();
                    var json = JsonConvert.SerializeObject(model);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(BaseUrl + "analyze/job-card/", content).Result;
                    var Data = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var UserInfo = JsonConvert.DeserializeObject<SameJobcard>(Data);
                        mainResultClass.SameJobcard = UserInfo;
                        mainResultClass.CreateJobcard = null;
                    }
                    else
                    {
                        var UserInfo = JsonConvert.DeserializeObject<JobCardModel>(Data);
                        mainResultClass.SameJobcard = null;
                        mainResultClass.CreateJobcard = UserInfo;
                    }
                }
                return mainResultClass;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<List<ExistJobCardResult>> GetExistJobCard(string token, string JobCardNumber)
        {
            try
            {
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = client.GetAsync(BaseUrl + "analyze/job-card/?job_card_name=" + JobCardNumber).Result;
                var Data = response.Content.ReadAsStringAsync().Result;
                var ExistJobCard = JsonConvert.DeserializeObject<ExistJobCard>(Data);
                return ExistJobCard.results;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<Result> PostJobCardSession(PostJobCardSession postJobCardSession, string token, string JobCardId)
        {
            try
            {
                string ReturnValue = string.Empty;
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var json = JsonConvert.SerializeObject(postJobCardSession);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync(BaseUrl + "analyze/job-card/" + JobCardId + "/job-card-session/", content).Result;
                var Data = response.Content.ReadAsStringAsync().Result;
                var JobCard = JsonConvert.DeserializeObject<Result>(Data);

                return JobCard;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                return null;
            }
        }

        public async Task<List<ModelNameClass>> GetModel(string token, string SelectedModelType)
        {
            try
            {
                List<ModelNameClass> modelNameClasses = new List<ModelNameClass>();
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = client.GetAsync(BaseUrl + "oem/get-models-dtc/");
                var Data = response.Result.Content.ReadAsStringAsync().Result;
                //var UserInfo = JsonConvert.DeserializeObject<UserResModel>(Data);
                //var firebaseLookup = JsonConvert.DeserializeObject<Dictionary<string, ModelName>>(Data);
                var list = JsonConvert.DeserializeObject<dynamic>(Data);

                var AdminPackageList = list["models"];
                var users = AdminPackageList.ToObject<Dictionary<string, ModelListModel>>();
                foreach (var item in users)
                {
                    ModelNameClass model = new ModelNameClass
                    {
                        ModelName = item.Key,
                        id = item.Value.NA_NA[0].model_id,
                    };

                    modelNameClasses.Add(model);
                }

                var ModelList = modelNameClasses.Where(x => x.ModelName.ToLower().Contains(SelectedModelType.ToLower())).ToList();
                // var FinalList = ModelList.Where(x => x.ModelName.ToLower().Contains("MDE".ToLower()) || x.ModelName.ToLower().Contains("VO".ToLower()) || x.ModelName.ToLower().Contains("VDE".ToLower())).ToList();

                //var firebaseLookup = JsonConvert.DeserializeObject<Dictionary<string, ModelListModel>>(Data);
                return ModelList;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<EcuModelNew> GetEcuData(string token, int ecu_id)
        {
            try
            {
                List<ModelNameClass> modelNameClasses = new List<ModelNameClass>();
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = client.GetAsync(BaseUrl + "models/get-ecu/?id=" + ecu_id);
                var Data = response.Result.Content.ReadAsStringAsync().Result;
                var list = JsonConvert.DeserializeObject<EcuModelNew>(Data);

                return list;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<OnlineExpertModel> GetOnlineExpert(string token)
        {
            try
            {
                //List<ModelNameClass> modelNameClasses = new List<ModelNameClass>();
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = client.GetAsync(BaseUrl + "user/online-expert-users/?format=json");
                var Data = response.Result.Content.ReadAsStringAsync().Result;
                var list = JsonConvert.DeserializeObject<OnlineExpertModel>(Data);
                return list;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<MainResponseModel> CreateRemoteJobCard(RemoteJobCardModel model, string session_id)
        {
            try
            {
                MainResponseModel mainResultClass = new MainResponseModel();
                //client = new HttpClient();
                var json = JsonConvert.SerializeObject(model);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(BaseUrl + "analyze/job-card-session/" + session_id + "/remote-session/", content);//.ConfigureAwait(false);
                var Data = response.Content.ReadAsStringAsync().Result;


                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var BadResult = JsonConvert.DeserializeObject<BadRequestResponseModel>(Data);
                    mainResultClass.status = "Already Exist";
                    mainResultClass.badRequestResponseModel = BadResult;
                }
                else
                {
                    var NewResult = JsonConvert.DeserializeObject<ResponseJobCardModel>(Data);
                    mainResultClass.status = "New";
                    mainResultClass.NewRequestResponseModel = NewResult;
                    //var UserInfo = JsonConvert.DeserializeObject<JobcardModel>(Data);
                    //mainResultClass.SameJobcard = null;
                    //mainResultClass.CreateJobcard = UserInfo;
                }
                return mainResultClass;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<ResponseJobCardModel> UpdateRemoteJobCard(RemoteJobCardModel model, string session_id, string remote_session_id)
        {
            try
            {
                MainResultClass mainResultClass = new MainResultClass();
                //client = new HttpClient();
                var json = JsonConvert.SerializeObject(model);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string url = $"{BaseUrl}analyze/job-card-session/{session_id}/remote-session/{remote_session_id}/";
                var response = client.PutAsync(url, content).Result;
                var Data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ResponseJobCardModel>(Data);

                //if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                //{
                //    var UserInfo = JsonConvert.DeserializeObject<SameJobcard>(Data);
                //    mainResultClass.SameJobcard = UserInfo;
                //    mainResultClass.CreateJobcard = null;
                //}
                //else
                //{
                //    var UserInfo = JsonConvert.DeserializeObject<JobcardModel>(Data);
                //    mainResultClass.SameJobcard = null;
                //    mainResultClass.CreateJobcard = UserInfo;
                //}

                return result;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<ResponseRoot> GetRemoteSession(string GetRemoteSession_id) /// Test
        {
            try
            {
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                string url = $"{BaseUrl}analyze/job-card-session/{GetRemoteSession_id}/remote-session/";
                var response = await client.GetAsync(url);
                var Data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ResponseRoot>(Data);
                return result;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<ResponseRoot> GetExpertRequestList(string expert_user) /// Test
        {
            try
            {
                ResponseRoot result = new ResponseRoot();
                //using (client = new HttpClient())
                //{
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                string url = $"{BaseUrl}analyze/expert-user-status-list/?expert_user={expert_user}";
                var response = await client.GetAsync(url);
                var Data = response.Content.ReadAsStringAsync().Result;
                result = JsonConvert.DeserializeObject<ResponseRoot>(Data);
                //return result;
                //}
                return result;

                ////client = new HttpClient();
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                //string url = $"{BaseUrl}analyze/expert-user-status-list/?expert_user={expert_user}";
                //var response = client.GetAsync(url);
                //var Data = response.Result.Content.ReadAsStringAsync().Result;
                //var result = JsonConvert.DeserializeObject<ResponseRoot>(Data);
                //return result;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }
        public async Task<ResponseJobCardModel> AcceptRemoteRequest(ResponseJobCardModel Accept_OR_DeclineModel, string JobCardRequestId, string RemoteSessionId)
        {
            try
            {
                //client = new HttpClient();
                var json = JsonConvert.SerializeObject(Accept_OR_DeclineModel);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string url = $"{BaseUrl}analyze/job-card-session/{JobCardRequestId}/remote-session/{RemoteSessionId}/";
                var response = await client.PutAsync(url, content);
                var Data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ResponseJobCardModel>(Data);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GetWorkShopData()
        {
            try
            {
                //List<ModelNameClass> modelNameClasses = new List<ModelNameClass>();
                //client = new HttpClient();
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = await client.GetAsync(BaseUrl + "oem/get-workshop");
                string Data = response.Content.ReadAsStringAsync().Result;
                //var result = JsonConvert.DeserializeObject<Dictionary<string, WorkShopGroupModel>>(Data);
                //var result = JsonConvert.DeserializeObject<WorkShopGroupModel>(Data);
                return Data;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return "";
            }
        }

        public async Task<bool> RegisterUser(SigninModel model)
        {
            try
            {
                //MainResultClass mainResultClass = new MainResultClass();
                //client = new HttpClient();
                var json = JsonConvert.SerializeObject(model);
                
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string url = $"{BaseUrl}accounts/register/";
                var response = client.PostAsync(url, content).Result;
                var Data = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }

                //var result = JsonConvert.DeserializeObject<ResponseJobCardModel>(Data);

                //if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                //{
                //    var UserInfo = JsonConvert.DeserializeObject<SameJobcard>(Data);
                //    mainResultClass.SameJobcard = UserInfo;
                //    mainResultClass.CreateJobcard = null;
                //}
                //else
                //{
                //    var UserInfo = JsonConvert.DeserializeObject<JobcardModel>(Data);
                //    mainResultClass.SameJobcard = null;
                //    mainResultClass.CreateJobcard = UserInfo;
                //}

            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return false;
            }
        }

        public async Task<string> GetDongleList(string token)
        {
            try
            {

                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = client.GetAsync(BaseUrl + "devices/list/obd-dongles/active/").Result;
                var Data = response.Content.ReadAsStringAsync().Result;
                //var UserInfo = JsonConvert.DeserializeObject<List<DongleModel>>(Data);
                //var list = UserInfo;//.Where(x => x.vehicle_model.parent.name.ToLower().Contains("MDE".ToLower())).ToList();
                return Data;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        //public async Task<RegisterDongleRespons> RegisterDongle(RegisterDongleModel registerDongleModel, string token)
        //{
        //    try
        //    {
        //        //string ReturnValue = string.Empty;
        //        RegisterDongleRespons registerDongleRespons = new RegisterDongleRespons();
        //        //client = new HttpClient();
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
        //        var json = JsonConvert.SerializeObject(registerDongleModel);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");
        //        var response = client.PostAsync(BaseUrl + "devices/register/odb-device/", content).Result;
        //        var Data = response.Content.ReadAsStringAsync().Result;
        //        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        //        {
        //            //var UserInfo = JsonConvert.DeserializeObject<UserResModel>(Data);
        //            //App.JwtToken = UserInfo.token;
        //            //registerDongleRespons.userRes = UserInfo;
        //        }
        //        else
        //        {
        //            var error = JsonConvert.DeserializeObject<ErrorRes>(Data);
        //            registerDongleRespons.errorRes = error;
        //            //retunValue = false;
        //        }
        //        return registerDongleRespons;
        //    }
        //    catch (Exception ex)
        //    {
        //        Application.Current.MainPage.DisplayAlert("Alert", ex.StackTrace, "Ok");
        //        return null;
        //    }
        //}

        public async Task<RegisterDongleRespons> RegisterDongle(RegisterDongleModel registerDongleModel, string token)
        {
            try
            {
                //string ReturnValue = string.Empty;
                RegisterDongleRespons registerDongleRespons = new RegisterDongleRespons();
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var json = JsonConvert.SerializeObject(registerDongleModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync(BaseUrl + "devices/register/odb-device/", content).Result;
                var Data = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    //var UserInfo = JsonConvert.DeserializeObject<UserResModel>(Data);
                    //App.JwtToken = UserInfo.token;
                    //registerDongleRespons.userRes = UserInfo;
                }
                else
                {
                    // var error = JsonConvert.DeserializeObject<ErrorRes>(Data);
                    var DongleRespons = JsonConvert.DeserializeObject<RegDongleRespons>(Data);
                    registerDongleRespons.errorRes = DongleRespons;
                    await DependencyService.Get<ISaveLocalData>().SaveData("LastUsedDongle", Data);
                    //retunValue = false;
                }
                return registerDongleRespons;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Alert", ex.StackTrace, "Ok");
                return null;
            }
        }

        public async Task<dynamic> Get_data(string token)
        {
            try
            {

                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = client.GetAsync(BaseUrl + "oem/get-data").Result;
                var Data = response.Content.ReadAsStringAsync().Result;
                dynamic list = JsonConvert.DeserializeObject(Data);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    show_message("Token has expired");
                }
                //var info = list["dtcs"];

                //var Data = ReadTextFile(filename);
                //var UserInfo = JsonConvert.DeserializeObject<List<JobCardModel>>(Data);
                //var list = UserInfo;//.Where(x => x.vehicle_model.parent.name.ToLower().Contains("MDE".ToLower())).ToList();
                return list;
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.StackTrace}", "Alert Get dat api", "Ok");
                //show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<OemModel> getAllOem(string token)
        {
            string Data = string.Empty;
            try
            {
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                client = new HttpClient();
                var response = await client.GetAsync($"{BaseUrl}oem/oem/");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                }
                else
                {
                    Data = response.Content.ReadAsStringAsync().Result;
                }
                var UserInfo = JsonConvert.DeserializeObject<OemModel>(Data);
                return UserInfo;
            }
            catch(Exception ex)
            {
                return null;
            }
        } 

        public async Task<AllModelsModel> get_all_models(string token, int id)
        {
            string Data = string.Empty;
            var data = DependencyService.Get<ISaveLocalData>().GetData("selctedOemModel");
            var selected_Oem = JsonConvert.DeserializeObject<AllOemModel>(data);
            id = selected_Oem.id;
            try
            {
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("http://www.google.com/");
                if (isReachable)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                    var response = await client.GetAsync($"{BaseUrl}models/get-models/?oem="+id);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                    }
                    else
                    {
                        Data = response.Content.ReadAsStringAsync().Result;
                    }
                    await DependencyService.Get<ISaveLocalData>().SaveData("modeljson", Data);
                    var UserInfo = JsonConvert.DeserializeObject<AllModelsModel>(Data);
                    return UserInfo;
                }
                else
                {
                    Data = DependencyService.Get<ISaveLocalData>().GetData("modeljson");

                    var UserInfo = JsonConvert.DeserializeObject<AllModelsModel>(Data);
                    return UserInfo;
                }
            }
            catch (Exception ex)
            {
                // UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.StackTrace}", "Alert Get dat api", "Ok");
                //show_message(ex.StackTrace);

                await Application.Current.MainPage.DisplayAlert("ERROR", "Please Re-Login", "OK");

                Application.Current.Properties["token"] = "";
                App.Current.Properties["MasterLoginUserBY"] = "";
                App.Current.Properties["MasterLoginUserRoleBY"] = "";

                App.Current.SavePropertiesAsync();
                App.Current.MainPage = new NavigationPage(new LoginPage());

                string x = ex.Message;
                return null;
            }
        }

        public async Task<Ecu2> getFlashRecords(string token, int ecu_id)
        {
            string Data = string.Empty;

            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("http://www.google.com/");
            if (isReachable)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = await client.GetAsync($"{BaseUrl}models/get-flash/?ecu={ecu_id}");
                Data = response.Content.ReadAsStringAsync().Result;
                var results = JsonConvert.DeserializeObject<FlashInfo>(Data).results.FirstOrDefault();
                return results;
            }
            else
            {
                UserDialogs.Instance.Alert("Alert","Please connect to internet.", "Ok");
                return null;
            }
        }

        public async Task<List<Results>> get_pid(string token, int id)
        {
            try
            {
                List<Results> results = new List<Results>();
                string Data = string.Empty;

                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("http://www.google.com/");
                if (isReachable)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                    var response = await client.GetAsync($"{BaseUrl}datasets/get-pid-datasets/?id={id}");
                    Data = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<Root>(Data).results;
                }
                else
                {
                    Data = DependencyService.Get<ISaveLocalData>().GetData("pidjson");
                    results = JsonConvert.DeserializeObject<List<Results>>(Data);
                    return results.Where(x => x.id == id).ToList();
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.StackTrace}", "Alert Get dat api", "Ok");
                string x = ex.Message;
                return null;
            }
        }

        public async Task<List<IVN_Result>> get_ivn_dtc(string token, int id)
        {
            try
            {
                List<IVN_Result> results = new List<IVN_Result>();
                string Data = string.Empty;
                //if (App.is_login)
                //{
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                    //var response = client.GetAsync($"{BaseUrl}datasets/get-dtc-datasets/?id={id}").Result;
                    var response = client.GetAsync($"{BaseUrl}ivn/get-ivn-dtc-datasets/?id={id}").Result;
                    Data = response.Content.ReadAsStringAsync().Result;
                    results = JsonConvert.DeserializeObject<IvnDtc>(Data).results;
                    //DependencyService.Get<ISaveLocalData>().SaveData("dtcjson", Data);
                    //DependencyService.Get<ISaveLocalData>().SaveData("dtcjson", "this is DTC data");
                //}
                //else
                //{
                //    Data = DependencyService.Get<ISaveLocalData>().GetData("dtcjson");
                //    results = JsonConvert.DeserializeObject<List<IVN_Result>>(Data);
                //}
                return results;
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.Message}", "Alert Get dat api", "Ok");
                //show_message(ex.Message);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<List<PIDResult>> get_ivn_pid(string token, int id)
        {
            try
            {
                List<PIDResult> results = new List<PIDResult>();
                string Data = string.Empty;

                //client = new HttpClient();
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                ////var response = client.GetAsync($"{BaseUrl}datasets/get-pid-datasets/?id={id}").Result;
                //var response = client.GetAsync($"{BaseUrl}datasets/get-pid-datasets/?id={id}").Result;
                //Data = response.Content.ReadAsStringAsync().Result;
                ////DependencyService.Get<ISaveLocalData>().SaveData("pidjson", Data);
                ////DependencyService.Get<ISaveLocalData>().SaveData("pidjson", "This is PID data");
                //results = JsonConvert.DeserializeObject<Root>(Data).results;

                //if (App.is_login)
                //{
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                    //var response = client.GetAsync($"{BaseUrl}datasets/get-pid-datasets/?id={id}").Result;
                    var response = client.GetAsync($"{BaseUrl}ivn/get-ivn-pid-datasets/?id={id}").Result;
                    Data = response.Content.ReadAsStringAsync().Result;
                    //DependencyService.Get<ISaveLocalData>().SaveData("pidjson", Data);
                    //DependencyService.Get<ISaveLocalData>().SaveData("pidjson", "This is PID data");
                    return JsonConvert.DeserializeObject<PIDModel>(Data).results;
                //}
                //else
                //{
                //    Data = DependencyService.Get<ISaveLocalData>().GetData("pidjson");
                //    results = JsonConvert.DeserializeObject<List<PIDResult>>(Data);
                //    return results.Where(x => x.id == id).ToList();
                //}
                //return results;
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.Message}", "Alert Get dat api", "Ok");
                //show_message(ex.Message);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<DTCMaskRoot> GetDTC_mask(string token)
        {
            try
            {
                //List<ModelNameClass> modelNameClasses = new List<ModelNameClass>();
                client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var response = client.GetAsync(BaseUrl + "dtc_mask/dtc-mask/");
                var Data = response.Result.Content.ReadAsStringAsync().Result;
                var list = JsonConvert.DeserializeObject<DTCMaskRoot>(Data);
                return list;
            }
            catch (Exception ex)
            {
                //show_message(ex.Message);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<List<DtcResults>> get_dtc(string token, int id)
        {
            string Data = string.Empty;
            try
            {
                List<DtcResults> results = new List<DtcResults>();

                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("http://www.google.com/");
                if (isReachable)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                    var response = await client.GetAsync($"{BaseUrl}datasets/get-dtc-datasets/?id={id}");
                    Data = response.Content.ReadAsStringAsync().Result;
                    results = JsonConvert.DeserializeObject<DtcMainModel>(Data).results;
                }
                else
                {
                    Data = DependencyService.Get<ISaveLocalData>().GetData("dtcjson");
                    results = JsonConvert.DeserializeObject<List<DtcResults>>(Data);
                }
                return results;
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.StackTrace}", "Alert Get dat api", "Ok");
                //show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }
        }

        public async Task<List<ResultUnlock>> getUnlockData()
        {
            string Data = string.Empty;
            try
            {
                List<ResultUnlock> results = new List<ResultUnlock>();
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("http://www.google.com/");
                if (isReachable)
                {
                    client = new HttpClient();
                    var response = await client.GetAsync($"{BaseUrl}models/unlock-list/");
                    Data = response.Content.ReadAsStringAsync().Result;
                    results = JsonConvert.DeserializeObject<UnlockEcuModel>(Data).results;
                    return results;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public async Task<GdModelGD> Get_gd(string token, string dtcPCode, int sub_model_id)
        {

            try
            {
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (isReachable)
                {
                    //client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);

                    string url = $"{BaseUrl}gdauthor/gd/gd-by-year_id-dtc_id/?dtc_id={dtcPCode}&name={sub_model_id}";
                    var response = client.GetAsync(url).Result;
                    var Data = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        GdModelGD myDeserializedClass = JsonConvert.DeserializeObject<GdModelGD>(Data);
                        return myDeserializedClass;
                    }
                    else
                    {
                        return null;
                    }

                    //dynamic list = JsonConvert.DeserializeObject<List<GDData>>(Data);
                    //dynamic list = JsonConvert.DeserializeObject(Data);
                    //var json = JsonConvert.SerializeObject(list);

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                string x = ex.Message;
                return null;
            }


            //try
            //{

            //    //client = new HttpClient();
            //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
            //    var response = client.GetAsync($"{BaseUrl}gdauthor/gd/gd-list/?dtc_id={pid_id}&name={model_id}").Result;
            //    var Data = response.Content.ReadAsStringAsync().Result;
            //    dynamic list = JsonConvert.DeserializeObject(Data);
            //    //var info = list["dtcs"];

            //    //var Data = ReadTextFile(filename);
            //    //var UserInfo = JsonConvert.DeserializeObject<List<JobCardModel>>(Data);
            //    //var list = UserInfo;//.Where(x => x.vehicle_model.parent.name.ToLower().Contains("MDE".ToLower())).ToList();
            //    return list;
            //}
            //catch (Exception ex)
            //{
            //    show_message(ex.StackTrace);
            //    string x = ex.Message;
            //    return null;
            //}
        }

        public string ReadTextFile(string FileName)
        {
            try
            {
                Stream stream = this.GetType().Assembly.GetManifestResourceStream("Bajaj.JsonFiles." + FileName);
                string text = "";
                using (var reader = new System.IO.StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
                return text;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public void show_message(string message)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert(message, "Error", "Ok");
        }

        public async Task<string> read_json_file(string json_file)
        {
            try
            {
                //HttpClient client = new HttpClient();

                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, json_file))
                {
                    var respo = await client.SendAsync(requestMessage);
                    var contentString = await respo.Content.ReadAsStringAsync();
                    if (respo.IsSuccessStatusCode)
                        return contentString;
                    else
                        throw new Exception(contentString);
                }

                //HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(json_file);
                //httpWebRequest.Method = WebRequestMethods.Http.Get;
                //httpWebRequest.Accept = "application/json; charset=utf-8";
                //string file = string.Empty;
                //var response = (HttpWebResponse)httpWebRequest.GetResponse();
                //using (var sr = new StreamReader(response.GetResponseStream()))
                //{
                //    file = sr.ReadToEnd();
                //}

                //return file;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async Task DtcRecord(List<PostDtcRecord> PDR, string token, string JobCardId)
        {
            try
            {
                string ReturnValue = string.Empty;
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);

                //var ob = new List<DtcR>();
                //ob.Add(new DtcR { dtc = PDR });

                var obg = new DtcR
                {
                    dtc = PDR
                };
                var json = JsonConvert.SerializeObject(obg);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(BaseUrl + "analyze/job-card-session/" + JobCardId + "/dtc-record/", content);
                var Data = response.Content.ReadAsStringAsync().Result;
                //var JobCard = JsonConvert.DeserializeObject<Result>(Data);
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
            }
        }

        public async void ClearDtcRecord(List<ClearDtcRecord> CDR, string token, string JobCardId)
        {
            try
            {
                string ReturnValue = string.Empty;
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var json = JsonConvert.SerializeObject(CDR).Replace("]", "");
                var JValue = json.Replace("[", "");
                var content = new StringContent(JValue, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(BaseUrl + "analyze/job-card-session/" + JobCardId + "/clear-record/", content);
                var Data = response.Content.ReadAsStringAsync().Result;
                //var JobCard = JsonConvert.DeserializeObject<Result>(Data);
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
            }
        }

        public async Task<bool> Pid_Write_Record(List<pid_write_record> PWR, string token, string JobCardId)
        {
            bool ReturnValue;
            try
            {
                //string ReturnValue = string.Empty;
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);

                var obg = new PidWriteRecord
                {
                    pid_write_records = PWR
                };

                var json = JsonConvert.SerializeObject(obg);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(BaseUrl + "analyze/job-card-session/" + JobCardId + "/pid-write-record/", content);
                var Data = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK|| response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    ReturnValue = true;
                }
                else
                {
                    ReturnValue = false;
                }
                return ReturnValue;
                //var JobCard = JsonConvert.DeserializeObject<Result>(Data);
            }
            catch (Exception ex)
            {
                return false;
                show_message(ex.StackTrace);
            }
        }

        public bool pid_live_record(List<PIDLiveRecord> PLR, string token, string JobCardId)
        {
            try
            {
                bool ReturnValue;

                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var json = JsonConvert.SerializeObject(PLR).Substring(1);
                var JValue = json.Substring(0, json.Length - 1);



                var content = new StringContent(JValue, Encoding.UTF8, "application/json");
                var response = client.PostAsync(BaseUrl + "analyze/job-card-session/" + JobCardId + "/pid-live-record/", content).Result;
                var Data = response.Content.ReadAsStringAsync().Result;

                Console.WriteLine($"RECORD RESPONSE {Data}");

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    ReturnValue = false;
                }
                else
                {
                    ReturnValue = true;
                }
                return ReturnValue;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                return false;
            }
        }

        public bool pid_snapshot_record(List<snapshot_record> SR, string token, string JobCardId)
        {
            try
            {
                bool ReturnValue;
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);

                var obj = new pid_snapshot_record
                {
                    pid_snapshot = SR
                };
                var json = JsonConvert.SerializeObject(obj);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync(BaseUrl + "analyze/job-card-session/" + JobCardId + "/pid-snapshot-record/", content).Result;
                var Data = response.Content.ReadAsStringAsync().Result;

                Console.WriteLine($"SNAPSHOT RESPONSE {Data}");

                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    ReturnValue = false;
                }
                else
                {
                    ReturnValue = true;
                }
                return ReturnValue;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> flash_record(List<flash_record> FR, string token, string JobCardId)
        {
            bool ReturnValue = false;
            try
            {
                //string ReturnValue = string.Empty;
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);
                var json = JsonConvert.SerializeObject(FR).Replace("]", "");
                var JValue = json.Replace("[", "");
                var content = new StringContent(JValue, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(BaseUrl + "analyze/job-card-session/" + JobCardId + "/flash-record/", content);
                var Data = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode != System.Net.HttpStatusCode.OK|| response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    ReturnValue = true;
                }
                else
                {
                    ReturnValue = false;
                }
                return ReturnValue;
            }
            catch (Exception ex)
            {
               return false;
                //show_message(ex.StackTrace);
            }
        }

        public bool CloseJobCard(List<ResCloseSession> resCloses, string token, string JobCardId)
        {
            try
            {
                bool ReturnValue;
                //client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                var json = JsonConvert.SerializeObject(resCloses).Replace("]", "");
                var JValue = json.Replace("[", "");
                var content = new StringContent(JValue, Encoding.UTF8, "application/json");
                var Putresponse = client.PutAsync(BaseUrl + "analyze/job-card-session/" + JobCardId + "/close-session/", content);
                var response = client.GetAsync(BaseUrl + "analyze/job-card-session/" + JobCardId + "/close-session/").Result;
                if (response.IsSuccessStatusCode == true)
                {
                    var Data = response.Content.ReadAsStringAsync().Result;
                    ReturnValue = true;
                }
                else
                {
                    ReturnValue = false;
                }
                return ReturnValue;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> CloseRemoteSession()
        {
            try
            {
                bool ReturnValue;
                client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", App.JwtToken);
                //var json = JsonConvert.SerializeObject(resCloses).Replace("]", "");
                //var JValue = json.Replace("[", "");
                var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                //var Putresponse = client.PutAsync(BaseUrl + "analyze/job-card-session/" + JobCardId + "/close-session/", content);
                var response = await client.PutAsync(BaseUrl + "analyze/remote-session/"+ App.RemoteSessionId + "/close-session/",content);
                var Data = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == HttpStatusCode.OK|| response.StatusCode == HttpStatusCode.Created)
                {
                    
                    ReturnValue = true;
                }
                else
                {
                    ReturnValue = false;
                }
                return ReturnValue;
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                return false;
            }
        }
        //public async Task<string> NewJobCardNumber()
        //{
        //    try
        //    {
        //        //client = new HttpClient();
        //        var response = client.GetAsync(BaseUrl + "analyze/gen-name").Result;
        //        var Data = response.Content.ReadAsStringAsync().Result;
        //        var list = JsonConvert.DeserializeObject<AutoNewJobCard>(Data);
        //        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //        {
        //            show_message("Token has expired");
        //            return null;
        //        }
        //        return list.name;
        //    }
        //    catch (Exception ex)
        //    {
        //        UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.StackTrace}", "Alert Get dat api", "Ok");
        //        //show_message(ex.StackTrace);
        //        string x = ex.Message;
        //        return null;
        //    }
        //}

        #region IOR Test
        public async Task<IorTestModel> GetIorTest(string token, int id)
        {
            try
            {
                IorTestModel results = new IorTestModel();
                string Data = string.Empty;
                var isReachable = await CrossConnectivity.Current.IsRemoteReachable("https://www.google.com/");
                if (isReachable)
                {
                    //client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", token);

                    string url = $"{BaseUrl}ior-test/ior-test-list/";
                    var response = await client.GetAsync(url);
                    Data = response.Content.ReadAsStringAsync().Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        results = JsonConvert.DeserializeObject<IorTestModel>(Data);
                    }
                    else
                    {
                        results.error = response.StatusCode.ToString();
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                //UserDialogs.Instance.Alert($"{ex.Message}\n\n\n\n{ex.StackTrace}", "Alert Get dat api", "Ok");
                //show_message(ex.StackTrace);
                //string x = ex.Message;
                return null;
            }
        }
        #endregion

        public async Task<string> ExperNotfy(NotificationModel notificationModel, NotificationM notificationModel1,string expert_id)
        {
            try
            {
                string ReturnValue = string.Empty;
                //NotificationRoot notificationRoot = new NotificationRoot
                //{
                //    to = $"/topics/{expert_id}",
                //    data = notificationModel,
                //    notification = notificationModel
                //};
                NotificationRoot notificationRoot = new NotificationRoot
                {
                    to = $"/topics/{expert_id}",
                    data = notificationModel,
                    //notification = notificationModel1
                };


                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", " = AAAA7_-LssA:APA91bG15iL62SoWaNGA2ZgQW-qUEAl0MvD9faziRdEyUPQXshylefQHGJw0H2RDDUrjG7Xcezi6fpbba5iLIrOK54peesX-UZ8PE84rVwsLBl2OwspO0urkQGFFseiJkhZc6W8QCuXG");
                var json = JsonConvert.SerializeObject(notificationRoot);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync("https://fcm.googleapis.com/fcm/send", content).Result;
                var Data = response.Content.ReadAsStringAsync().Result;
                return "";
            }
            catch (Exception ex)
            {
                show_message(ex.StackTrace);
                return "";
            }
        }


        #region BajajApis
        public async Task<WhoAmIModel> WhoAmI()
        {
            try
            {
                client = new HttpClient();
                string url = $"{BaseUrl}api/whoami?subscription_id=AUTOPEEPAL";

                var response = client.GetAsync(url).Result;
                var data = response.Content.ReadAsStringAsync().Result;

                var resp = JsonConvert.DeserializeObject<WhoAmIModel>(data);

                return resp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<DealerLoginModel> DealerLogin(string token, string dealerId, string serialNo)
        {
            try
            {
                client = new HttpClient();
                string url = $"{BaseUrl}api/v2/login-get-otp?serial_number={serialNo}&dealer_code={dealerId}";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync(url).Result;
                var data = response.Content.ReadAsStringAsync().Result;

                var resp = JsonConvert.DeserializeObject<DealerLoginModel>(data);
                return resp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ValidateLoginOtpResp> ValidateLoginOtp(string token, ValidateLoginOtpModel reqModel)
        {
            try
            {
                client = new HttpClient();
                string url = $"{BaseUrl}api/v2/validate-login-otp";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var json = JsonConvert.SerializeObject(reqModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //demo Login
                var response = await client.PostAsync(url, content);
                var data = response.Content.ReadAsStringAsync().Result;

                var resp = JsonConvert.DeserializeObject<ValidateLoginOtpResp>(data);
                return resp;
            }
            catch (Exception ex)
            {
                return null;
            }
        } 

        public async Task<VinOtpModel> GetVinOtp(string token, string serialNo, string vin)
        {
            try
            {
                client = new HttpClient();
                string url = $"{BaseUrl}api/v2/vin-get-otp?serial_number={serialNo}&vin={vin}";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync(url).Result;
                var data = response.Content.ReadAsStringAsync().Result;

                var resp = JsonConvert.DeserializeObject<VinOtpModel>(data);
                return resp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VinResponseModel> ValidateVinOtp(string token, ValidateVinOtpModel reqModel)
        {
            try
            {
                client = new HttpClient();
                string url = $"{BaseUrl}api/v2/vin-validate-otp";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var json = JsonConvert.SerializeObject(reqModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var data = response.Content.ReadAsStringAsync().Result;

                var resp = JsonConvert.DeserializeObject<VinResponseModel>(data);
                return resp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Stream> read_xml_file(string xmlUrl)
        {
            try
            {
                client = new HttpClient();
                var response = client.GetAsync(xmlUrl).Result;
                var data = response.Content.ReadAsStreamAsync().Result;
                var file = response.Content.ReadAsStringAsync().Result;
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion
    }

    public class Notification1
    {
        public string body { get; set; }
        public string title { get; set; }
    }

    public class Data
    {
        public string body { get; set; }
        public string title { get; set; }
        public string key_1 { get; set; }
        public string key_2 { get; set; }
    }

    public class NotificationRoot
    {
        public string to { get; set; }
        public NotificationM notification { get; set; }
        public NotificationModel data { get; set; }
    }
}
