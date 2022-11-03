using MultiEventController.Models;
using Bajaj.Model;
using Bajaj.Services;
using Bajaj.ViewModel;
using System;
using System.Collections.ObjectModel;

namespace Bajaj.View.ViewModel
{
    public class ActuatorTestViewModel : BaseViewModel
    {
        //ApiServices services;
        public ActuatorTestViewModel()
        {
            try
            {
                //services = new ApiServices();
                PidList = new ObservableCollection<PidCode>();
                EcuList = new ObservableCollection<EcuTestRoutine>();
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

        private ObservableCollection<PidCode> pidList;
        public ObservableCollection<PidCode> PidList
        {
            get => pidList;
            set
            {
                pidList = value;
                OnPropertyChanged("PidList");
            }
        }

        private PidCode selectedPid;
        public PidCode SelectedPid
        {
            get => selectedPid;
            set
            {
                selectedPid = value;
                OnPropertyChanged("SelectedPid");
            }
        }

        private ObservableCollection<Message> enumrateList;
        public ObservableCollection<Message> EnumrateList
        {
            get => enumrateList;
            set
            {
                enumrateList = value;
                OnPropertyChanged("EnumrateList");
            }
        }

        private Message selectedEnumrate;
        public Message SelectedEnumrate
        {
            get => selectedEnumrate;
            set
            {
                selectedEnumrate = value;
                OnPropertyChanged("SelectedEnumrate");
            }
        }

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

        private string errorText = "Actuator test not found.";
        public string ErrorText
        {
            get => errorText;
            set
            {
                errorText = value;
                OnPropertyChanged("ErrorText");
            }
        }

        private bool viewVisible = true;
        public bool ViewVisible
        {
            get => viewVisible;
            set
            {
                viewVisible = value;
                OnPropertyChanged("ViewVisible");
            }
        }

        private bool showError = false;
        public bool ShowError
        {
            get => showError;
            set
            {
                showError = value;
                OnPropertyChanged("ShowError");
            }
        }

        private bool pidListVisible = false;
        public bool PidListVisible
        {
            get => pidListVisible;
            set
            {
                pidListVisible = value;
                OnPropertyChanged("PidListVisible");
            }
        }

        private bool enumrateListVisible = false;
        public bool EnumrateListVisible
        {
            get => enumrateListVisible;
            set
            {
                enumrateListVisible = value;
                OnPropertyChanged("EnumrateListVisible");
            }
        }

        private bool manualEntryVisible = false;
        public bool ManualEntryVisible
        {
            get => manualEntryVisible;
            set
            {
                manualEntryVisible = value;
                OnPropertyChanged("ManualEntryVisible");
            }
        }

        private bool enumrateDropDownVisible = false;
        public bool EnumrateDropDownVisible
        {
            get => enumrateDropDownVisible;
            set
            {
                enumrateDropDownVisible = value;
                OnPropertyChanged("EnumrateDropDownVisible");
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

        private string btnText = "Play";
        public string BtnText
        {
            get => btnText;
            set
            {
                btnText = value;
                OnPropertyChanged("BtnText");
            }
        }

        private string newValue = string.Empty;
        public string NewValue
        {
            get => newValue;
            set
            {
                newValue = value;
                OnPropertyChanged("NewValue");
            }
        }

        private bool btnVisible = false;
        public bool BtnVisible
        {
            get => btnVisible;
            set
            {
                btnVisible = value;
                OnPropertyChanged("BtnVisible");
            }
        }

        private bool shortNameVisible = false;
        public bool ShortNameVisible
        {
            get => shortNameVisible;
            set
            {
                shortNameVisible = value;
                OnPropertyChanged("ShortNameVisible");
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

        private string txtPlaceholder = string.Empty;
        public string TxtPlaceholder
        {
            get => txtPlaceholder;
            set
            {
                txtPlaceholder = value;
                OnPropertyChanged("TxtPlaceholder");
            }
        }

    }
}
