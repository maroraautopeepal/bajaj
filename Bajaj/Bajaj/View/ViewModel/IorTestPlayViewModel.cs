using Bajaj.Model;
using Bajaj.ViewModel;
using System;
using System.Collections.ObjectModel;

namespace Bajaj.View.ViewModel
{
    public class IorTestPlayViewModel : BaseViewModel
    {
        public IorTestPlayViewModel(IorResult iorResult)
        {
            ServerPid = new ObservableCollection<PidCode>();
            this.IorTestList = new ObservableCollection<IorTestRoutine>(iorResult.ior_test_routine);
            this.MonitorList = new ObservableCollection<TestMonitor>(iorResult.test_monitors);
            RoutineListHeight = IorTestList.Count * 65;
        }

        private IorTestRoutine selectedIorTest;
        public IorTestRoutine SelectedIorTest
        {
            get => selectedIorTest;
            set
            {
                selectedIorTest = value;
                OnPropertyChanged("SelectedIorTest");
            }
        }

        private ObservableCollection<IorTestRoutine> iorTestList;
        public ObservableCollection<IorTestRoutine> IorTestList
        {
            get => iorTestList;
            set
            {
                iorTestList = value;
                OnPropertyChanged("IorTestList");
            }
        }

        private ObservableCollection<TestMonitor> monitorList;
        public ObservableCollection<TestMonitor> MonitorList
        {
            get => monitorList;
            set
            {
                monitorList = value;
                OnPropertyChanged("MonitorList");
            }
        }

        private ObservableCollection<PidCode> serverPid;
        public ObservableCollection<PidCode> ServerPid
        {
            get => serverPid;
            set
            {
                serverPid = value;
                OnPropertyChanged("ServerPid");
            }
        }

        private double routineListHeight;
        public double RoutineListHeight
        {
            get => routineListHeight;
            set
            {
                routineListHeight = value;
                OnPropertyChanged("RoutineListHeight");
            }
        }

        private string timer;
        public string Timer
        {
            get => timer;
            set
            {
                timer = value;
                OnPropertyChanged("Timer");
            }
        }

        private bool timerVisible = false;
        public bool TimerVisible
        {
            get => timerVisible;
            set
            {
                timerVisible = value;
                OnPropertyChanged("TimerVisible");
            }
        }

        private bool layoutVisible = false;
        public bool LayoutVisible
        {
            get => layoutVisible;
            set
            {
                layoutVisible = value;
                OnPropertyChanged("LayoutVisible");
            }
        }

        private bool timer_Visible = false;
        public bool Timer_Visible
        {
            get => timer_Visible;
            set
            {
                timer_Visible = value;
                OnPropertyChanged("Timer_Visible");
            }
        }

        private string txtAction = "Click play button to start the test";
        public string TxtAction
        {
            get => txtAction;
            set
            {
                txtAction = value;
                OnPropertyChanged("TxtAction");
            }
        }
    }
}
