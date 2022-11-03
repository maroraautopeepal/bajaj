using Bajaj.Model;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Bajaj.View.ViewModel
{
    public class IorTestViewModel : BaseViewModel
    {

        public IorTestViewModel()
        {
            try
            {
                IorTestList = new ObservableCollection<IorResult>();
                EcuList = new ObservableCollection<EcuTestRoutine>();
                //PreConditionList = new ObservableCollection<PreCondition>();
                //IorTestRoutineList = new ObservableCollection<IorTestRoutine>();
                //IorTestRoutineType = new IorTestRoutineType();
                //TestMonitorList = new ObservableCollection<TestMonitor>();
            }
            catch (Exception ex)
            {
            }            
        }

        private ObservableCollection<EcuTestRoutine> ecuList;
        public ObservableCollection<EcuTestRoutine> EcuList
        {
            get => ecuList;
            set
            {
                ecuList = value;
                OnPropertyChanged("EcuList");
            }
        }

        private EcuTestRoutine selectedEcu;
        public EcuTestRoutine SelectedEcu
        {
            get => selectedEcu;
            set
            {
                selectedEcu = value;
                OnPropertyChanged("SelectedEcu");
            }
        }

        //private ObservableCollection<PreCondition> preConditionList;
        //public ObservableCollection<PreCondition> PreConditionList
        //{
        //    get => preConditionList;
        //    set
        //    {
        //        preConditionList = value;
        //        OnPropertyChanged("PreConditionList");
        //    }
        //}

        //private ObservableCollection<IorTestRoutine> iorTestRoutineList;
        //public ObservableCollection<IorTestRoutine> IorTestRoutineList
        //{
        //    get => iorTestRoutineList;
        //    set
        //    {
        //        iorTestRoutineList = value;
        //        OnPropertyChanged("IorTestRoutineList");
        //    }
        //}

        private ObservableCollection<IorResult> iorTestList;
        public ObservableCollection<IorResult> IorTestList
        {
            get => iorTestList;
            set
            {
                iorTestList = value;
                OnPropertyChanged("IorTestList");
            }
        }

        private IorResult selectedTest;
        public IorResult SelectedTest
        {
            get => selectedTest;
            set
            {
                selectedTest = value;
                OnPropertyChanged("SelectedTest");
            }
        }

        //private ObservableCollection<TestMonitor> testMonitorList;
        //public ObservableCollection<TestMonitor> TestMonitorList
        //{
        //    get => testMonitorList;
        //    set
        //    {
        //        testMonitorList = value;
        //        OnPropertyChanged("TestMonitorList");
        //    }
        //}


        //private IorTestModel fullData;
        //public IorTestModel FullData
        //{
        //    get => fullData;
        //    set
        //    {
        //        fullData = value;
        //        OnPropertyChanged("FullData");
        //    }
        //}

        //private IorTestRoutineType iorTestRoutineType;
        //public IorTestRoutineType IorTestRoutineType
        //{
        //    get => iorTestRoutineType;
        //    set
        //    {
        //        iorTestRoutineType = value;
        //        OnPropertyChanged("IorTestRoutineType");
        //    }
        //}

        private string routineNotice;
        public string RoutineNotice
        {
            get => routineNotice;
            set
            {
                routineNotice = value;
                OnPropertyChanged("RoutineNotice");
            }
        }

        private string routineListStatus = "Loading...";
        public string RoutineListStatus
        {
            get => routineListStatus;
            set
            {
                routineListStatus = value;
                OnPropertyChanged("RoutineListStatus");
            }
        }
    }
}
