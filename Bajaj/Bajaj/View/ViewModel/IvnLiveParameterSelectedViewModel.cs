using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Bajaj.View.ViewModel
{
    public class IvnLiveParameterSelectedViewModel : BaseViewModel
    {
        ApiServices services;
        public IvnLiveParameterSelectedViewModel()
        {
            try
            {
                services = new ApiServices();
                IECUParameterList = new List<PidCode>();
                EMSParameterList = new ObservableCollection<ReadParameterPID>();
                OldValueList = new ObservableCollection<ReadParameterPID>();
                SelectedParameterList = new ObservableCollection<IVN_SelectedPID>();
            }
            catch (Exception ex)
            {
            }
        }

        private ObservableCollection<IVN_SelectedPID> selectedParameterList;
        public ObservableCollection<IVN_SelectedPID> SelectedParameterList
        {
            get => selectedParameterList;
            set
            {
                selectedParameterList = value;
                OnPropertyChanged("SelectedParameterList");
            }
        }

        private ObservableCollection<ReadParameterPID> eOldValueList;
        public ObservableCollection<ReadParameterPID> OldValueList
        {
            get => eOldValueList;
            set
            {
                eOldValueList = value;
            }
        }
        private ObservableCollection<ReadParameterPID> eMSParameterList;
        public ObservableCollection<ReadParameterPID> EMSParameterList
        {
            get => eMSParameterList;
            set
            {
                eMSParameterList = value;
                OnPropertyChanged("EMSParameterList");
            }
        }

        private ObservableCollection<IVN_SelectedPID> _IVNeMSParameterList;
        public ObservableCollection<IVN_SelectedPID> IVNEMSParameterList
        {
            get => _IVNeMSParameterList;
            set
            {
                _IVNeMSParameterList = value;
                OnPropertyChanged("IVNEMSParameterList");
            }
        }

        private List<PidCode> iECUParameterList;
        public List<PidCode> IECUParameterList
        {
            get => iECUParameterList;
            set
            {
                iECUParameterList = value;
                OnPropertyChanged("IECUParameterList");
            }
        }

    }
}
