using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bajaj.View.ViewModel
{
    public class OemViewModel : BaseViewModel
    {
        ApiServices services;

        public OemViewModel()
        {
            services = new ApiServices();
            oem_list = new List<AllOemModel>();
            get_Oem();
        }

        private List<AllOemModel> _oem_list;
        public List<AllOemModel> oem_list
        {
            get => _oem_list;
            set
            {
                _oem_list = value;
                OnPropertyChanged("oem_list");
            }
        }

        public async void get_Oem()
        {
            try
            {
                var res = await services.getAllOem(App.JwtToken);
                oem_list = res.results;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
