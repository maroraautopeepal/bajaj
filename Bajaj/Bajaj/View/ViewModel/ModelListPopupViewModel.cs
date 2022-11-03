using Bajaj.Services;
using System.Collections.ObjectModel;

namespace Bajaj.ViewModel
{
    public class ModelListPopupViewModel : BaseViewModel
    {
        ApiServices services;
        public string SelectedModelType;
        public ModelListPopupViewModel(string ModelType)
        {
            services = new ApiServices();
            ModelList = new ObservableCollection<ModelNameClass>();
            SelectedModelType = ModelType;
            GetModelList(SelectedModelType);


        }

        private ObservableCollection<ModelNameClass> modelList;
        public ObservableCollection<ModelNameClass> ModelList
        {
            get => modelList;
            set
            {
                modelList = value;
                OnPropertyChanged();
            }
        }

        public void GetModelList(string selectedModelType)
        {
            var result = services.GetModel(App.JwtToken, selectedModelType);
            //ModelList = result.Result;

            foreach (var item in result.Result)
            {
                ModelNameClass model = new ModelNameClass
                {
                    ModelName = item.ModelName,
                    id = item.id,
                };

                ModelList.Add(model);
            }

        }
    }
}
