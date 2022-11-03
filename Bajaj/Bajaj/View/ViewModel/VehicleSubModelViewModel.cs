using Acr.UserDialogs;
using Bajaj.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bajaj.ViewModel
{
    public class VehicleSubModelViewModel : BaseViewModel
    {
        List<SubModel> Models;
        public VehicleSubModelViewModel(List<SubModel> Models)
        {
            vehicle_sub_model_list = new List<SubModel>();
            this.Models = Models;
            //{
            //    new VehicleSubModelModel
            //    {
            //       vehicle_sub_model= "Vehicle Sub Model_1"
            //    },
            //    new VehicleSubModelModel
            //    {
            //       vehicle_sub_model= "Vehicle Sub Model_2"
            //    },
            //    new VehicleSubModelModel
            //    {
            //       vehicle_sub_model= "Vehicle Sub Model_3"
            //    },
            //    new VehicleSubModelModel
            //    {
            //       vehicle_sub_model= "Vehicle Sub Model_4"
            //    },
            //    new VehicleSubModelModel
            //    {
            //       vehicle_sub_model= "Vehicle Sub Model_5"
            //    },
            //};

            getModels();
        }

        private List<SubModel> _vehicle_sub_model_list;
        public List<SubModel> vehicle_sub_model_list
        {
            get => _vehicle_sub_model_list;
            set
            {
                _vehicle_sub_model_list = value;
                OnPropertyChanged("vehicle_sub_model_list");
            }
        }

        private List<ModelNames> _model_name_list;
        public List<ModelNames> model_name_list
        {
            get => _model_name_list;
            set
            {
                _model_name_list = value;
                OnPropertyChanged("model_name_list");
            }
        }

        private async Task getModels()
        {
            using (UserDialogs.Instance.Loading("Loading...", null, null, true, MaskType.Black))
            {
                await Task.Delay(100);

                var group = Models.GroupBy(x => x.name).ToList();
                List<ModelNames> list = new List<ModelNames>();
                foreach (var item in group)
                {
                    list.Add(new ModelNames
                    {
                        name = item.Key
                    }); 
                }
                model_name_list = new List<ModelNames>(list);
            }
        }
    }

    public class ModelNames
    {
        public string name { get; set; }
    }
}

