using Bajaj.Model;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Bajaj.View.ViewModel
{
    public class PreConditionViewModel :BaseViewModel
    {
        public PreConditionViewModel(List<PreCondition> PreConditionList)
        {
            this.PreConditionList = new ObservableCollection<PreCondition>(PreConditionList);
            //PidPrecondition = new ObservableCollection<PidCode>();
        }

        private ObservableCollection<PreCondition> preConditionList;
        public ObservableCollection<PreCondition> PreConditionList
        {
            get => preConditionList;
            set
            {
                preConditionList = value;
                OnPropertyChanged("PreConditionList");
            }
        }

        private PreCondition selectPreCondition;
        public PreCondition SelectPreCondition
        {
            get => selectPreCondition;
            set
            {
                selectPreCondition = value;
                OnPropertyChanged("SelectPreCondition");
            }
        }


        private bool btnContinueEnable = false;
        public bool BtnContinueEnable
        {
            get => btnContinueEnable;
            set
            {
                btnContinueEnable = value;
                OnPropertyChanged("BtnContinueEnable");
            }
        }

        private ObservableCollection<PidCode> pidPrecondition;
        public ObservableCollection<PidCode> PidPrecondition
        {
            get => pidPrecondition;
            set
            {
                pidPrecondition = value;
                OnPropertyChanged("PidPrecondition");
            }
        }
    }
}
