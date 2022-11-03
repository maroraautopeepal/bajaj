using Bajaj.Model;
using Bajaj.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bajaj.ViewModel
{
    public class LiveParameterSelectedViewModel : BaseViewModel
    {
        ApiServices services;
        public LiveParameterSelectedViewModel()
        {
            try
            {
                services = new ApiServices();
                //IECUParameterList = new List<PidCode>();
                SelectedParameterList = new ObservableCollection<PidCode>();

            }
            catch (Exception ex)
            {
            }
        }

        private ObservableCollection<PidCode> selectedParameterList;
        public ObservableCollection<PidCode> SelectedParameterList
        {
            get => selectedParameterList;
            set
            {
                selectedParameterList = value;
                OnPropertyChanged("SelectedParameterList");
            }
        }

       
    }
}
