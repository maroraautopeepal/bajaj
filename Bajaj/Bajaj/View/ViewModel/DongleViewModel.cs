using Bajaj.Model;
using Bajaj.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bajaj.ViewModel
{
    public class DongleViewModel : BaseViewModel
    {
        ApiServices services;
        public DongleViewModel()
        {
            try
            {
                services = new ApiServices();
                DongleModel = new DongleModel();
                //GetDongleList();
            }
            catch (Exception ex)
            {
            }
        }

        private DongleModel dongleModel;
        public DongleModel DongleModel
        {
            get => dongleModel;
            set
            {
                dongleModel = value;
                OnPropertyChanged("DongleModel");
            }
        }

        public async Task<List<DongleResult>> GetDongleList()
        {
            try
            {
                var result = await services.GetDongleList(App.JwtToken);

                dynamic list = JsonConvert.DeserializeObject(result);
                foreach (var item in list)
                {
                    var InfoKey = item.Name;
                    var InfoValue = item.Value;
                    if (item.Name == "count")
                    {
                        DongleModel.Count = item.Value;
                    }
                    if (item.Name == "results")
                    {
                        foreach (var model in item.Value)
                        {
                            var InfoKey1 = model.Name;
                            var InfoValue1 = model.Value;
                            DongleResult dongleResult = new DongleResult();
                            foreach (var results in model)
                            {

                                //foreach (var results in resultsItem)
                                //{
                                var InfoKey2 = results.Name;
                                var InfoValue2 = results.Value;
                                if (results.Name == "mac_id")
                                {
                                    dongleResult.MacId = results.Value;
                                }
                                if (results.Name == "is_active")
                                {
                                    dongleResult.IsActive = results.Value;
                                }
                                if (results.Name == "serial_number")
                                {
                                    dongleResult.SerialNumber = results.Value;
                                }
                                if (results.Name == "user")
                                {
                                    foreach (var user in results)
                                    {
                                        foreach (var userFesult in user)
                                        {
                                            DongleUser dongleUser = new DongleUser();
                                            foreach (var userRes in userFesult)
                                            {
                                                var InfoKey5 = userRes.Name;
                                                var InfoValue5 = userRes.Value;
                                                if (userRes.Name == "email")
                                                {
                                                    dongleUser.Email = userRes.Value;
                                                }
                                                if (userRes.Name == "full_name")
                                                {
                                                    dongleUser.FullName = userRes.Value;
                                                }
                                                if (userRes.Name == "mobile")
                                                {
                                                    dongleUser.Mobile = userRes.Value;
                                                }
                                                if (userRes.Name == "workshop")
                                                {
                                                    dongleUser.Workshop = userRes.Value;
                                                }
                                                if (userRes.Name == "role")
                                                {
                                                    dongleUser.Role = userRes.Value;
                                                }
                                            }
                                            dongleResult.User.Add(dongleUser);
                                        }
                                    }
                                }
                                if (results.Name == "oem")
                                {
                                    foreach (var oem in results)
                                    {
                                        foreach (var oemResult in oem)
                                        {
                                            var InfoKey4 = oemResult.Name;
                                            var InfoValue4 = oemResult.Value;
                                            if (oemResult.Name == "name")
                                            {
                                                dongleResult.Oem.Name = oemResult.Value;
                                            }
                                            if (oemResult.Name == "slug")
                                            {
                                                dongleResult.Oem.Slug = oemResult.Value;
                                            }
                                            if (oemResult.Name == "admin")
                                            {
                                                dongleResult.Oem.Admin = oemResult.Value;
                                            }
                                            if (oemResult.Name == "is_active")
                                            {
                                                dongleResult.Oem.IsActive = oemResult.Value;
                                            }
                                        }
                                    }
                                }
                                //}

                            }
                            DongleModel.Results.Add(dongleResult);
                        }
                    }
                }
                return DongleModel.Results;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
