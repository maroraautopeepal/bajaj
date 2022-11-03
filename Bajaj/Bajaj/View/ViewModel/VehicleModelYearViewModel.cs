using Bajaj.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Bajaj.ViewModel
{
    public class VehicleModelYearViewModel : BaseViewModel
    {
        string selectedModel = string.Empty;
        public VehicleModelYearViewModel(string selectedModel, List<SubModel> subModels)
        {
            vehicle_model_year_list = new ObservableCollection<VehicleModelYearModel>();
            this.selectedModel = selectedModel;
            getSubModels(subModels);
        }

        private ObservableCollection<VehicleModelYearModel> _vehicle_model_year_list;
        public ObservableCollection<VehicleModelYearModel> vehicle_model_year_list
        {
            get => _vehicle_model_year_list;
            set
            {
                _vehicle_model_year_list = value;
                OnPropertyChanged("vehicle_model_year_list");
            }
        }

        private ObservableCollection<SubModel> _subModelList;
        public ObservableCollection<SubModel> subModelList
        {
            get => _subModelList;
            set
            {
                _subModelList = value;
                OnPropertyChanged("subModelList");
            }
        }


        private async Task getSubModels(List<SubModel> subModels)
        {
            subModelList = new ObservableCollection<SubModel>(subModels.Where(x => x.name == selectedModel));
        }
    }
}

