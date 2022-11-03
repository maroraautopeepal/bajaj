using Bajaj.Model;
using Bajaj.Services;
using System.Collections.Generic;

namespace Bajaj.ViewModel
{
    public class VehiclePopupViewModel : BaseViewModel
    {
        ApiServices services;
        public VehiclePopupViewModel()
        {
            services = new ApiServices();
            get_model();
            //ModelList = new List<VehiclePopModel>
            //{
            //    new VehiclePopModel
            //    {
            //        ModelName = "BSIII",
            //    },
            //    new VehiclePopModel
            //    {
            //        ModelName = "BSIV",
            //    },
            //    new VehiclePopModel
            //    {
            //        ModelName = "BSVI",
            //    },
            //    //new VehiclePopModel
            //    //{
            //    //    ModelName = "GEN",
            //    //},
            //};
        }

        private List<VehiclePopModel> modelList;
        public List<VehiclePopModel> ModelList
        {
            get => modelList;
            set
            {
                modelList = value;
                OnPropertyChanged();
            }
        }

        public void get_model()
        {
            services.GetModel(App.JwtToken, "");
        }
    }
}
