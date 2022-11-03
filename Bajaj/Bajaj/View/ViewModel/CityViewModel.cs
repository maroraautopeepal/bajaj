using Bajaj.Model;
using Bajaj.Services;
using System.Collections.ObjectModel;

namespace Bajaj.ViewModel
{
    public class CityViewModel : BaseViewModel
    {
        ApiServices services;
        public CityViewModel()
        {
            services = new ApiServices();
            CityList = new ObservableCollection<CityModel>();
        }

        private ObservableCollection<CityModel> cityList;
        public ObservableCollection<CityModel> CityList
        {
            get => cityList;
            set
            {
                cityList = value;
                OnPropertyChanged("CityList");
            }
        }
    }
}
