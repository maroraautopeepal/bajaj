using Bajaj.Model;
using Bajaj.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Bajaj.View.ViewModel
{
    public class ProtocolViewModel : BaseViewModel
    {
        public ProtocolViewModel()
        {
            ProtocolList = new ObservableCollection<ProtocolModel>
            {
                new ProtocolModel{ Protocol="ISO15765_250KB_11BIT_CAN",ProtocolIndex = 00 },
                new ProtocolModel{ Protocol="ISO15765_250Kb_29BIT_CAN",ProtocolIndex = 01 },
                new ProtocolModel{ Protocol="ISO15765_500KB_11BIT_CAN",ProtocolIndex = 02 },
                new ProtocolModel{ Protocol="ISO15765_500KB_29BIT_CAN",ProtocolIndex = 03 },
                new ProtocolModel{ Protocol="ISO15765_1MB_11BIT_CAN",ProtocolIndex = 04 },
                new ProtocolModel{ Protocol="ISO15765_1MB_29BIT_CAN",ProtocolIndex = 05 },
                new ProtocolModel{ Protocol="I250KB_11BIT_CAN",ProtocolIndex = 06 },
                new ProtocolModel{ Protocol="I250Kb_29BIT_CAN",ProtocolIndex = 07 },
                new ProtocolModel{ Protocol="I500KB_11BIT_CAN",ProtocolIndex = 08 },
                new ProtocolModel{ Protocol="I500KB_29BIT_CAN",ProtocolIndex = 09 },
                new ProtocolModel{ Protocol="I1MB_11BIT_CAN",ProtocolIndex = (int)0x0A },
                new ProtocolModel{ Protocol="I1MB_29BIT_CAN",ProtocolIndex = (int)0x0B},
                new ProtocolModel{ Protocol="OE_IVN_250KBPS_11BIT_CAN",ProtocolIndex = (int)0x0C},
                new ProtocolModel{ Protocol="OE_IVN_250KBPS_29BIT_CAN",ProtocolIndex = (int)0x0D},
                new ProtocolModel{ Protocol="OE_IVN_500KBPS_11BIT_CAN",ProtocolIndex = (int)0x0E},
                new ProtocolModel{ Protocol="OE_IVN_500KBPS_29BIT_CAN",ProtocolIndex = (int)0x0F},
                new ProtocolModel{ Protocol="OE_IVN_1MBPS_11BIT_CAN",ProtocolIndex = (int)0x10},
                new ProtocolModel{ Protocol="OE_IVN_1MBPS_29BIT_CAN",ProtocolIndex = (int)0x11 },
            };
        }



        private ObservableCollection<ProtocolModel> protocolList;
        public ObservableCollection<ProtocolModel> ProtocolList
        {
            get => protocolList;
            set
            {
                protocolList = value;
                OnPropertyChanged("ProtocolList");
            }
        }

        private ProtocolModel selectedProtocol ;
        public ProtocolModel SelectedProtocol
        {
            get => selectedProtocol;
            set
            {
                selectedProtocol = value;
                OnPropertyChanged("SelectedProtocol");
            }
        }

        private string txHeader = "07E0";
        public string TxHeader 
        {
            get => txHeader;
            set
            {
                txHeader = value;
                OnPropertyChanged("TxHeader");
            }
        }

        private string rxHeader = "07E8";
        public string RxHeader
        {
            get => rxHeader;
            set
            {
                rxHeader = value;
                OnPropertyChanged("RxHeader");
            }
        }

        private bool isPadding;
        public bool IsPadding
        {
            get => isPadding;
            set
            {
                isPadding = value;
                OnPropertyChanged("IsPadding");
            }
        }

        private bool isTesterPresent;
        public bool IsTesterPresent
        {
            get => isTesterPresent;
            set
            {
                isTesterPresent = value;
                OnPropertyChanged("IsTesterPresent");
            }
        }

        private string[] commands = new string[6];
        public string[] Commands
        {
            get => commands;
            set
            {
                commands = value;
                OnPropertyChanged("Commands");
            }
        }
    }
}
