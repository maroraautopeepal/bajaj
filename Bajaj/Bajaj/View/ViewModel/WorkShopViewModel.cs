using Bajaj.Model;
using Bajaj.Services;
using System.Collections.ObjectModel;

namespace Bajaj.ViewModel
{
    public class WorkShopViewModel : BaseViewModel
    {
        ApiServices services;
        public WorkShopViewModel()
        {
            services = new ApiServices();
            //WorkShopList = new List<Workshop>();
            WorkShopList = new ObservableCollection<WorkShopModel>();
        }

        private ObservableCollection<WorkShopModel> workShopList;
        public ObservableCollection<WorkShopModel> WorkShopList
        {
            get => workShopList;
            set
            {
                workShopList = value;
                OnPropertyChanged("WorkShopList ");
            }
        }

        //private List<Workshop> workShopList;
        //public List<Workshop> WorkShopList
        //{
        //    get => workShopList;
        //    set
        //    {
        //        workShopList = value;
        //        OnPropertyChanged("WorkShopList ");
        //    }
        //}
    }
    //public class WorkShopViewModel : BaseViewModel
    //{
    //    ApiServices services;
    //    public WorkShopViewModel()
    //    {
    //        services = new ApiServices();
    //        WorkShopList = new ObservableCollection<WorkShopModel>();
    //    }

    //    private ObservableCollection<WorkShopModel> workShopList;
    //    public ObservableCollection<WorkShopModel> WorkShopList
    //    {
    //        get => workShopList;
    //        set
    //        {
    //            workShopList = value;
    //            OnPropertyChanged("WorkShopList ");
    //        }
    //    }
    //}
}

