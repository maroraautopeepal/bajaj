using Newtonsoft.Json;
using Bajaj.Interfaces;
using Bajaj.Model;
using Bajaj.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Acr.UserDialogs;

namespace Bajaj.ViewModel
{
    class VehicleModelViewModel : BaseViewModel
    {
        ApiServices services;
        public VehicleModelViewModel()
        {
            try
            {
                services = new ApiServices();
                vehicle_model_list = new List<ModelResult>();
                get_model();
            }
            catch (Exception ex)
            {

            }
            //{
            //    new VehicleModelModel
            //    {
            //       vehicle_model = "Vehicle Model_1"
            //    },
            //    new VehicleModelModel
            //    {
            //       vehicle_model = "Vehicle Model_2"
            //    },
            //    new VehicleModelModel
            //    {
            //       vehicle_model = "Vehicle Model_3"
            //    },
            //    new VehicleModelModel
            //    {
            //       vehicle_model = "Vehicle Model_4"
            //    },
            //    new VehicleModelModel
            //    {
            //       vehicle_model = "Vehicle Model_5"
            //    },
            //};
        }

        private List<ModelResult> _vehicle_model_list;
        public List<ModelResult> vehicle_model_list
        {
            get => _vehicle_model_list;
            set
            {
                _vehicle_model_list = value;
                OnPropertyChanged("vehicle_model_list");
            }
        }

        public async Task get_model()
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                try
                {
                    await Task.Delay(100);
                    var data = DependencyService.Get<ISaveLocalData>().GetData("selctedOemModel");
                    var selected_Oem = JsonConvert.DeserializeObject<AllOemModel>(data);
                    var id = selected_Oem.id;


                    //var allmodel = Task.Run(async () =>
                    //{
                    //    //var getOem = await services.Get_data(App.JwtToken);
                    //    var res = await services.get_all_models(App.JwtToken, id);
                    //    App.ForOfflineJobCardCreate = res;
                    //    return res;
                    //});

                    var allmodel = await services.get_all_models(App.JwtToken, id);

                    vehicle_model_list = allmodel.results;

                    Task.Run(() =>
                    {
                        foreach (var item in allmodel.results.ToList())
                        {


                            if (item.sub_models != null && item.sub_models.Count > 0)
                            {
                                var submodel_name = item.sub_models.FirstOrDefault();
                                if (!string.IsNullOrEmpty(submodel_name.name) && submodel_name.name != "NA")
                                {
                                    item.model_name = item.name + "-" + submodel_name.name;
                                }
                                else
                                {
                                    item.model_name = item.name;
                                }
                            }
                            else
                            {
                                item.model_name = item.name;
                            }
                        }
                    });
                    

                    //if (vehicle_model_list.Count == 0)
                    //{
                    //    emptyView = "Not Found";
                    //}
                    //return allmodel.Result;
                }
                catch (Exception ex)
                {
                    //return null;
                }

            }
        }
    }
}
